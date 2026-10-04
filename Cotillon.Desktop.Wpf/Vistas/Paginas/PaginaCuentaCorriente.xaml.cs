using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Cotillon.Desktop.Wpf.Servicios;
using Cotillon.Shared.DTOs.Permisos;
using Cotillon.Shared.DTOs.Terceros;

namespace Cotillon.Desktop.Wpf.Vistas.Paginas;

/// <summary>
/// La cuenta de los clientes que compran en el mes y pagan a fin de mes.
///
/// A la izquierda, quién debe y hace cuánto que no se mueve: eso es lo que dice
/// a quién hay que llamar. A la derecha, el resumen con el saldo corriendo
/// renglón por renglón, que es lo que se le muestra al cliente cuando discute
/// el total.
/// </summary>
public partial class PaginaCuentaCorriente : UserControl
{
    private readonly bool _puedeCobrar = Sesion.PuedeEditar(Modulos.CuentaCorriente);
    private readonly bool _puedeAjustar = Sesion.PuedeTodo(Modulos.CuentaCorriente);

    private int _clienteElegido;

    public PaginaCuentaCorriente()
    {
        InitializeComponent();

        // Ajustar el saldo a mano mueve plata sin que haya venta ni pago: va con
        // el permiso más alto.
        PanelAjuste.Visibility = _puedeAjustar ? Visibility.Visible : Visibility.Collapsed;

        Loaded += async (_, _) =>
        {
            await CargarMediosAsync();
            await BuscarAsync();
        };

        Refresco.Automatico(this, async () =>
        {
            if (_clienteElegido == 0)
                await BuscarAsync();
        });
    }

    private async Task CargarMediosAsync()
    {
        var medios = await App.Api.MediosPagoAsync();

        if (!medios.Success || medios.Data is null)
            return;

        // La cuenta corriente no se paga con cuenta corriente.
        var cobrables = medios.Data
            .Where(m => !string.Equals(m.NombreMedioPago, "CUENTA CORRIENTE", StringComparison.OrdinalIgnoreCase))
            .ToList();

        CboMedioPago.ItemsSource = cobrables;
        CboMedioPago.SelectedIndex = 0;
    }

    private async void AlBuscar(object sender, RoutedEventArgs e) => await BuscarAsync();

    private async Task BuscarAsync()
    {
        var respuesta = await App.Api.SaldosClientesAsync(soloConDeuda: ChkTodos.IsChecked != true);
        var saldos = respuesta.Data ?? [];

        GrillaSaldos.ItemsSource = saldos;

        LblTotalDeuda.Text = saldos
            .Where(s => s.Saldo > 0)
            .Sum(s => s.Saldo)
            .ToString("C2", CultureInfo.CurrentCulture);
    }

    private async void AlSeleccionar(object sender, SelectionChangedEventArgs e)
    {
        if (GrillaSaldos.SelectedItem is not SaldoClienteDto fila)
            return;

        await CargarCuentaAsync(fila.ClienteId);
    }

    private async Task CargarCuentaAsync(int clienteId)
    {
        var respuesta = await App.Api.CuentaClienteAsync(clienteId);

        if (!respuesta.Success || respuesta.Data is null)
            return;

        var cuenta = respuesta.Data;

        _clienteElegido = cuenta.ClienteId;

        LblCliente.Text = cuenta.Cliente;
        LblSaldo.Text = cuenta.Saldo.ToString("C2", CultureInfo.CurrentCulture);
        GrillaMovimientos.ItemsSource = cuenta.Movimientos;

        // Sin deuda no hay nada que cobrar, pero sí se puede ajustar.
        BtnPagar.IsEnabled = _puedeCobrar && cuenta.Saldo > 0;
        TxtPago.Text = cuenta.Saldo > 0
            ? cuenta.Saldo.ToString("0.##", CultureInfo.CurrentCulture)
            : "0";

        PanelCobro.Visibility = _puedeCobrar || _puedeAjustar ? Visibility.Visible : Visibility.Collapsed;

        OcultarAviso();
    }

    private async void AlPagar(object sender, RoutedEventArgs e)
    {
        if (!_puedeCobrar || _clienteElegido == 0 || CboMedioPago.SelectedValue is not int medioPagoId)
            return;

        var importe = Numero(TxtPago.Text);

        if (importe <= 0)
        {
            MostrarAviso("Poné cuánto está pagando.");
            return;
        }

        BtnPagar.IsEnabled = false;

        try
        {
            var respuesta = await App.Api.PagarCuentaAsync(new RegistrarPagoCuentaDto
            {
                UsuarioId = Sesion.UsuarioId,
                ClienteId = _clienteElegido,
                Importe = importe,
                MedioPagoId = medioPagoId
            });

            if (!respuesta.Success)
            {
                MostrarAviso(respuesta.Message);
                return;
            }

            await CargarCuentaAsync(_clienteElegido);
            await BuscarAsync();
        }
        finally
        {
            BtnPagar.IsEnabled = true;
        }
    }

    private async void AlAjustar(object sender, RoutedEventArgs e)
    {
        if (!_puedeAjustar || _clienteElegido == 0)
            return;

        var importe = Numero(TxtImporteAjuste.Text);
        var motivo = TxtMotivoAjuste.Text.Trim();

        if (importe == 0 || motivo.Length == 0)
        {
            MostrarAviso("Para ajustar hace falta un importe distinto de cero y un motivo.");
            return;
        }

        var confirmar = MessageBox.Show(
            $"Ajustar la cuenta en {importe:C2}.\n\n" +
            $"{(importe > 0 ? "Suma deuda" : "Baja la deuda")}. Queda registrado en Auditoría.\n\n¿Seguimos?",
            "Cotillón", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirmar != MessageBoxResult.Yes)
            return;

        var respuesta = await App.Api.AjustarCuentaAsync(new AjustarCuentaDto
        {
            UsuarioId = Sesion.UsuarioId,
            ClienteId = _clienteElegido,
            Importe = importe,
            Motivo = motivo
        });

        if (!respuesta.Success)
        {
            MostrarAviso(respuesta.Message);
            return;
        }

        TxtImporteAjuste.Text = "0";
        TxtMotivoAjuste.Clear();

        await CargarCuentaAsync(_clienteElegido);
        await BuscarAsync();
    }

    private void MostrarAviso(string mensaje)
    {
        LblAviso.Text = mensaje;
        LblAviso.Visibility = Visibility.Visible;
    }

    private void OcultarAviso() => LblAviso.Visibility = Visibility.Collapsed;

    private static decimal Numero(string? texto) =>
        decimal.TryParse(texto?.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out var valor)
            ? valor
            : 0m;
}

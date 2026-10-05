using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Cotillon.Desktop.Wpf.Servicios;
using Cotillon.Shared.DTOs;
using Cotillon.Shared.DTOs.Caja;
using Cotillon.Shared.DTOs.Permisos;

namespace Cotillon.Desktop.Wpf.Vistas.Paginas;

public partial class PaginaCaja : UserControl
{
    private readonly VentanaPrincipal _principal;
    private ArqueoCajaDto? _arqueo;
    private readonly bool _puedeOperar = Sesion.PuedeEditar(Modulos.Caja);
    private readonly bool _puedeCerrar = Sesion.PuedeTodo(Modulos.Caja);

    /// <summary>
    /// Arqueo ciego: oculta el esperado al que cuenta. El dueno queda afuera,
    /// porque es el que despues tiene que revisar la diferencia.
    /// </summary>
    private bool _ciego;

    public PaginaCaja(VentanaPrincipal principal)
    {
        InitializeComponent();

        _principal = principal;

        // Con permiso de sólo ver, la caja queda como tablero de consulta: se ve el
        // arqueo y los movimientos del día, pero no se abre, no se mueve plata ni
        // se cierra. Cerrar es aparte porque es lo que fija la diferencia del turno.
        BtnAbrirCaja.Visibility = _puedeOperar ? Visibility.Visible : Visibility.Collapsed;
        PanelMovimientoManual.Visibility = _puedeOperar ? Visibility.Visible : Visibility.Collapsed;
        PanelCierre.Visibility = _puedeCerrar ? Visibility.Visible : Visibility.Collapsed;

        Loaded += async (_, _) => await CargarAsync();
    }

    private async Task CargarAsync()
    {
        var sesion = await App.Api.CajaActualAsync();

        if (!sesion.Success || sesion.Data is null)
        {
            PanelCerrada.Visibility = Visibility.Visible;
            PanelAbierta.Visibility = Visibility.Collapsed;
            return;
        }

        PanelCerrada.Visibility = Visibility.Collapsed;
        PanelAbierta.Visibility = Visibility.Visible;

        LblApertura.Text =
            $"Abierta por {sesion.Data.UsuarioApertura} el {sesion.Data.FechaApertura:dd/MM/yyyy HH:mm}";

        var configuracion = await App.Api.ConfiguracionAsync();

        _ciego = ArqueoCiego.Activo(configuracion.Data) && !Sesion.PuedeTodo(Modulos.Configuracion);

        var arqueo = await App.Api.ArqueoAsync();

        if (arqueo.Success && arqueo.Data is not null)
        {
            _arqueo = arqueo.Data;

            LblMontoInicial.Text = _arqueo.Sesion.MontoInicial.ToString("C2", CultureInfo.CurrentCulture);
            LblTotalVentas.Text = _arqueo.TotalVentas.ToString("C2", CultureInfo.CurrentCulture);
            LblIngresos.Text = _arqueo.TotalIngresos.ToString("C2", CultureInfo.CurrentCulture);
            LblEgresos.Text = _arqueo.TotalEgresos.ToString("C2", CultureInfo.CurrentCulture);
            // Arqueo ciego: el que cuenta la plata no ve cuanto tendria que haber.
            // Si lo ve antes de contar, un faltante se tapa escribiendo el
            // esperado en vez de lo que hay en el cajon, y el control no sirve.
            LblEfectivoEsperado.Text = _ciego
                ? "oculto hasta cerrar"
                : _arqueo.EfectivoEsperado.ToString("C2", CultureInfo.CurrentCulture);

            LblEfectivoEsperado.FontSize = _ciego ? 15 : 24;
            ListaMedios.ItemsSource = _ciego ? null : _arqueo.TotalesPorMedioPago;
        }

        var movimientos = await App.Api.MovimientosCajaAsync(sesion.Data.SesionCajaId);
        GrillaMovimientos.ItemsSource = movimientos.Data;

        AlCambiarContado(this, null!);
    }

    private async void AlRefrescar(object sender, RoutedEventArgs e) => await CargarAsync();

    private async void AlAbrirCaja(object sender, RoutedEventArgs e)
    {
        if (!_puedeOperar)
            return;

        var respuesta = await App.Api.AbrirCajaAsync(new AbrirCajaDto
        {
            UsuarioId = Sesion.UsuarioId,
            MontoInicial = Numero(TxtMontoInicial.Text),
            Observaciones = TxtObsApertura.Text.Trim()
        });

        if (!respuesta.Success)
        {
            MessageBox.Show(respuesta.Message, "Cotillón", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        await CargarAsync();
        await _principal.RefrescarEstadoCajaAsync();
    }

    private async void AlRegistrarMovimiento(object sender, RoutedEventArgs e)
    {
        if (!_puedeOperar)
            return;

        var tipo = (CboTipoMovimiento.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "INGRESO";

        var respuesta = await App.Api.MovimientoCajaAsync(new RegistrarMovimientoCajaDto
        {
            UsuarioId = Sesion.UsuarioId,
            TipoMovimiento = tipo,
            Importe = Numero(TxtImporteMovimiento.Text),
            Motivo = TxtMotivoMovimiento.Text.Trim()
        });

        if (!respuesta.Success)
        {
            MessageBox.Show(respuesta.Message, "Cotillón", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        TxtImporteMovimiento.Clear();
        TxtMotivoMovimiento.Clear();

        await CargarAsync();
    }

    private void AlCambiarContado(object sender, TextChangedEventArgs e)
    {
        if (_arqueo is null || LblDiferencia is null)
            return;

        var contado = Numero(TxtMontoContado.Text);

        if (contado <= 0)
        {
            PanelDiferencia.Visibility = Visibility.Collapsed;
            return;
        }

        if (_ciego)
        {
            // Tampoco la diferencia en vivo: seria el esperado dicho de otra forma.
            PanelDiferencia.Visibility = Visibility.Collapsed;
            return;
        }

        var diferencia = contado - _arqueo.EfectivoEsperado;

        LblDiferencia.Text = diferencia.ToString("C2", CultureInfo.CurrentCulture);
        LblDiferencia.Foreground = diferencia switch
        {
            0 => (Brush)FindResource("Exito"),
            < 0 => (Brush)FindResource("Peligro"),
            _ => (Brush)FindResource("Advertencia")
        };

        PanelDiferencia.Visibility = Visibility.Visible;
    }

    private async void AlCerrarCaja(object sender, RoutedEventArgs e)
    {
        if (!_puedeCerrar)
            return;

        var contado = Numero(TxtMontoContado.Text);
        var diferencia = contado - (_arqueo?.EfectivoEsperado ?? 0);

        // Cerrar con diferencia es una decisión que deja rastro: se confirma aparte.
        var mensaje = diferencia == 0
            ? "¿Confirmás el cierre de caja?"
            : $"El arqueo da una diferencia de {diferencia:C2}.\n\n¿Cerrar la caja igual?";

        if (MessageBox.Show(mensaje, "Cerrar caja", MessageBoxButton.YesNo, MessageBoxImage.Question)
            != MessageBoxResult.Yes)
            return;

        BtnCerrarCaja.IsEnabled = false;

        try
        {
            var respuesta = await App.Api.CerrarCajaAsync(new CerrarCajaDto
            {
                UsuarioId = Sesion.UsuarioId,
                MontoContado = contado,
                Observaciones = TxtObsCierre.Text.Trim()
            });

            if (!respuesta.Success)
            {
                MessageBox.Show(respuesta.Message, "Cotillón", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show(respuesta.Message, "Caja cerrada", MessageBoxButton.OK, MessageBoxImage.Information);

            await ImprimirCierreAsync(contado, diferencia);

            TxtMontoContado.Clear();
            TxtObsCierre.Clear();
            TxtMontoInicial.Text = "0";

            await CargarAsync();
            await _principal.RefrescarEstadoCajaAsync();
        }
        finally
        {
            BtnCerrarCaja.IsEnabled = true;
        }
    }

    /// <summary>
    /// Ofrece el comprobante del cierre. Es el papel que se guarda junto con la
    /// plata del día, así que se arma con el arqueo que se acaba de cerrar y no
    /// con lo que quede en pantalla después de recargar.
    /// </summary>
    private async Task ImprimirCierreAsync(decimal contado, decimal diferencia)
    {
        if (_arqueo is null)
            return;

        if (MessageBox.Show("¿Imprimir el comprobante de cierre?", "Cotillón",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        _arqueo.Sesion.MontoContado = contado;
        _arqueo.Sesion.Diferencia = diferencia;
        _arqueo.Sesion.FechaCierre = DateTime.Now;
        _arqueo.Sesion.UsuarioCierre = Sesion.NombreCompleto;
        _arqueo.Sesion.Observaciones = TxtObsCierre.Text.Trim();

        var configuracion = await App.Api.ConfiguracionAsync();
        var datos = configuracion.Data;

        try
        {
            Impresion.Imprimir(
                Ticket.CierreDeCaja(_arqueo, DatosTicket.Desde(datos), Impresion.AnchoConfigurado(datos)),
                Impresion.AnchoConfigurado(datos),
                $"Cierre de caja {_arqueo.Sesion.SesionCajaId}",
                preguntar: true);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"La caja quedó cerrada, pero no se pudo imprimir: {ex.Message}",
                "Cotillón", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private static decimal Numero(string? texto) =>
        decimal.TryParse(texto?.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out var valor)
            ? valor
            : 0m;
}

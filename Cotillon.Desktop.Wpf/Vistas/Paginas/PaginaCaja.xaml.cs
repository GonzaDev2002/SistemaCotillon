using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Cotillon.Desktop.Wpf.Servicios;
using Cotillon.Shared.DTOs.Caja;

namespace Cotillon.Desktop.Wpf.Vistas.Paginas;

public partial class PaginaCaja : UserControl
{
    private readonly VentanaPrincipal _principal;
    private ArqueoCajaDto? _arqueo;

    public PaginaCaja(VentanaPrincipal principal)
    {
        InitializeComponent();

        _principal = principal;

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

        var arqueo = await App.Api.ArqueoAsync();

        if (arqueo.Success && arqueo.Data is not null)
        {
            _arqueo = arqueo.Data;

            LblMontoInicial.Text = _arqueo.Sesion.MontoInicial.ToString("C2", CultureInfo.CurrentCulture);
            LblTotalVentas.Text = _arqueo.TotalVentas.ToString("C2", CultureInfo.CurrentCulture);
            LblIngresos.Text = _arqueo.TotalIngresos.ToString("C2", CultureInfo.CurrentCulture);
            LblEgresos.Text = _arqueo.TotalEgresos.ToString("C2", CultureInfo.CurrentCulture);
            LblEfectivoEsperado.Text = _arqueo.EfectivoEsperado.ToString("C2", CultureInfo.CurrentCulture);
            ListaMedios.ItemsSource = _arqueo.TotalesPorMedioPago;
        }

        var movimientos = await App.Api.MovimientosCajaAsync(sesion.Data.SesionCajaId);
        GrillaMovimientos.ItemsSource = movimientos.Data;

        AlCambiarContado(this, null!);
    }

    private async void AlRefrescar(object sender, RoutedEventArgs e) => await CargarAsync();

    private async void AlAbrirCaja(object sender, RoutedEventArgs e)
    {
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

    private static decimal Numero(string? texto) =>
        decimal.TryParse(texto?.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out var valor)
            ? valor
            : 0m;
}

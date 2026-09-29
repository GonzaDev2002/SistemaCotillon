using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Cotillon.Desktop.Wpf.Servicios;
using Cotillon.Shared.DTOs.Ventas;

namespace Cotillon.Desktop.Wpf.Vistas.Paginas;

public partial class PaginaHistorialVentas : UserControl
{
    private int _ventaSeleccionada;

    public PaginaHistorialVentas()
    {
        InitializeComponent();

        FechaDesde.SelectedDate = DateTime.Today;
        FechaHasta.SelectedDate = DateTime.Today;

        Loaded += async (_, _) => await BuscarAsync();
    }

    private async void AlBuscar(object sender, RoutedEventArgs e) => await BuscarAsync();

    private async Task BuscarAsync()
    {
        var respuesta = await App.Api.VentasAsync(
            FechaDesde.SelectedDate ?? DateTime.Today,
            FechaHasta.SelectedDate ?? DateTime.Today,
            ChkAnuladas.IsChecked == true);

        var ventas = respuesta.Data ?? [];

        Grilla.ItemsSource = ventas;

        var completadas = ventas.Where(v => v.Estado == "COMPLETADA").ToList();

        LblCantidad.Text = $"{completadas.Count} venta(s) completada(s)";
        LblTotal.Text = completadas.Sum(v => v.TotalVenta).ToString("C2", CultureInfo.CurrentCulture);
    }

    private async void AlSeleccionar(object sender, SelectionChangedEventArgs e)
    {
        if (Grilla.SelectedItem is not VentaResumenDto resumen)
            return;

        var respuesta = await App.Api.VentaAsync(resumen.VentaId);

        if (!respuesta.Success || respuesta.Data is null)
            return;

        var venta = respuesta.Data;

        _ventaSeleccionada = venta.VentaId;

        LblDetalle.Text = $"Venta #{venta.VentaId} · {venta.FechaVenta:dd/MM/yyyy HH:mm}";
        GrillaDetalle.ItemsSource = venta.Lineas;
        ListaPagos.ItemsSource = venta.Pagos;

        LblSubtotal.Text = venta.Subtotal.ToString("C2", CultureInfo.CurrentCulture);
        LblDescuento.Text = venta.DescuentoTotal.ToString("C2", CultureInfo.CurrentCulture);
        LblTotalVenta.Text = venta.TotalVenta.ToString("C2", CultureInfo.CurrentCulture);

        // Anular es una operación de supervisión y sólo tiene sentido sobre una
        // venta que siga vigente.
        PanelAnular.Visibility = venta.Estado == "COMPLETADA" && Sesion.PuedeSupervisar
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private async void AlAnular(object sender, RoutedEventArgs e)
    {
        if (_ventaSeleccionada <= 0)
            return;

        if (string.IsNullOrWhiteSpace(TxtMotivo.Text))
        {
            MessageBox.Show("Indicá el motivo de la anulación.", "Cotillón",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var confirmar = MessageBox.Show(
            $"¿Anular la venta #{_ventaSeleccionada}?\n\nSe devuelve el stock y se descuenta de la caja.",
            "Cotillón", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirmar != MessageBoxResult.Yes)
            return;

        BtnAnular.IsEnabled = false;

        try
        {
            var respuesta = await App.Api.AnularVentaAsync(_ventaSeleccionada, new AnularVentaDto
            {
                UsuarioId = Sesion.UsuarioId,
                Motivo = TxtMotivo.Text.Trim()
            });

            MessageBox.Show(respuesta.Message, "Cotillón", MessageBoxButton.OK,
                respuesta.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (respuesta.Success)
            {
                TxtMotivo.Clear();
                PanelAnular.Visibility = Visibility.Collapsed;
                await BuscarAsync();
            }
        }
        finally
        {
            BtnAnular.IsEnabled = true;
        }
    }
}

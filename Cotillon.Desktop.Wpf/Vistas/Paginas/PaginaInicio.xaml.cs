using System.Windows;
using System.Windows.Controls;

namespace Cotillon.Desktop.Wpf.Vistas.Paginas;

public partial class PaginaInicio : UserControl
{
    private readonly VentanaPrincipal _principal;

    public PaginaInicio(VentanaPrincipal principal)
    {
        InitializeComponent();

        _principal = principal;

        Loaded += async (_, _) => await CargarAsync();
    }

    private async Task CargarAsync()
    {
        var resumen = await App.Api.ResumenDashboardAsync();

        if (resumen.Success && resumen.Data is not null)
        {
            LblTotal.Text = resumen.Data.TotalProductos.ToString();
            LblActivos.Text = resumen.Data.ProductosActivos.ToString();
            LblInactivos.Text = resumen.Data.ProductosInactivos.ToString();
            LblBajoStock.Text = resumen.Data.ProductosBajoStock.ToString();
        }

        var bajoStock = await App.Api.BajoStockAsync();
        GrillaBajoStock.ItemsSource = bajoStock.Data;

        var ultimos = await App.Api.UltimosProductosAsync();
        GrillaUltimos.ItemsSource = ultimos.Data;
    }

    private async void AlActualizar(object sender, RoutedEventArgs e)
    {
        await CargarAsync();
        await _principal.RefrescarEstadoCajaAsync();
    }

    private void AlIrAVentas(object sender, RoutedEventArgs e) => _principal.IrAVentas();

    private void AlIrACaja(object sender, RoutedEventArgs e) => _principal.IrACaja();
}

using System.Windows;
using System.Windows.Controls;
using Cotillon.Desktop.Wpf.Servicios;
using Cotillon.Shared.DTOs.Permisos;

namespace Cotillon.Desktop.Wpf.Vistas.Paginas;

public partial class PaginaInicio : UserControl
{
    private readonly VentanaPrincipal _principal;

    public PaginaInicio(VentanaPrincipal principal)
    {
        InitializeComponent();

        _principal = principal;

        Loaded += async (_, _) => await CargarAsync();

        // El tablero es la pantalla que el dueño deja abierta: tiene que mostrar
        // lo que la cajera vende y lo que baja de stock sin que él haga nada.
        Refresco.Automatico(this, async () =>
        {
            await CargarAsync();
            await _principal.RefrescarEstadoCajaAsync();
        });
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

        await RevisarRespaldoAsync();
    }

    /// <summary>
    /// El aviso de respaldo. Se muestra sólo al que puede hacer algo al respecto:
    /// un cartel rojo que la vendedora no puede resolver es ruido que se aprende
    /// a ignorar, y el día que importe de verdad ya nadie lo mira.
    /// </summary>
    private async Task RevisarRespaldoAsync()
    {
        if (!Sesion.PuedeTodo(Modulos.Configuracion))
        {
            PanelRespaldo.Visibility = Visibility.Collapsed;
            return;
        }

        var respuesta = await App.Api.EstadoRespaldosAsync();

        if (!respuesta.Success || respuesta.Data is null)
            return;

        LblRespaldo.Text = respuesta.Data.Aviso;
        PanelRespaldo.Visibility = respuesta.Data.HayQueAvisar ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void AlRespaldar(object sender, RoutedEventArgs e)
    {
        BtnRespaldar.IsEnabled = false;
        BtnRespaldar.Content = "Respaldando...";

        try
        {
            var respuesta = await App.Api.RespaldarAhoraAsync();

            MessageBox.Show(
                respuesta.Data?.Mensaje ?? respuesta.Message, "Cotillón", MessageBoxButton.OK,
                respuesta.Data?.Exito == true ? MessageBoxImage.Information : MessageBoxImage.Warning);

            await RevisarRespaldoAsync();
        }
        finally
        {
            BtnRespaldar.IsEnabled = true;
            BtnRespaldar.Content = "Respaldar ahora";
        }
    }

    private async void AlActualizar(object sender, RoutedEventArgs e)
    {
        await CargarAsync();
        await _principal.RefrescarEstadoCajaAsync();
    }

    private void AlIrAVentas(object sender, RoutedEventArgs e) => _principal.IrAVentas();

    private void AlIrACaja(object sender, RoutedEventArgs e) => _principal.IrACaja();
}

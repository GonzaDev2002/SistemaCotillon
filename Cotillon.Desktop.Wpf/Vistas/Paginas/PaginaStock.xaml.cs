using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Cotillon.Desktop.Wpf.Servicios;
using Cotillon.Shared.DTOs.Permisos;
using Cotillon.Shared.DTOs.Stock;

namespace Cotillon.Desktop.Wpf.Vistas.Paginas;

public partial class PaginaStock : UserControl
{
    private int _productoSeleccionado;
    private readonly bool _puedeAjustar = Sesion.PuedeEditar(Modulos.Stock);

    public PaginaStock()
    {
        InitializeComponent();

        // Con permiso de sólo ver queda la trazabilidad: se consulta qué pasó con
        // el stock, pero no se puede tocar.
        PanelAjuste.Visibility = _puedeAjustar ? Visibility.Visible : Visibility.Collapsed;

        FechaDesde.SelectedDate = DateTime.Today.AddDays(-7);
        FechaHasta.SelectedDate = DateTime.Today;

        Loaded += async (_, _) => await BuscarAsync();
    }

    private async void AlBuscar(object sender, RoutedEventArgs e) => await BuscarAsync();

    private async Task BuscarAsync()
    {
        var respuesta = await App.Api.MovimientosStockAsync(
            FechaDesde.SelectedDate ?? DateTime.Today,
            FechaHasta.SelectedDate ?? DateTime.Today,
            null);

        Grilla.ItemsSource = respuesta.Data;
    }

    private async void AlBuscarProducto(object sender, RoutedEventArgs e) => await BuscarProductoAsync();

    private async void AlTeclaProducto(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await BuscarProductoAsync();
    }

    private async Task BuscarProductoAsync()
    {
        var texto = TxtProducto.Text.Trim();

        if (string.IsNullOrWhiteSpace(texto))
            return;

        var respuesta = await App.Api.BuscarProductosAsync(texto);

        if (!respuesta.Success || respuesta.Data is null || respuesta.Data.Count == 0)
        {
            _productoSeleccionado = 0;
            LblProducto.Text = "No se encontró el producto.";
            BtnAjustar.IsEnabled = false;
            return;
        }

        var producto = respuesta.Data[0];

        _productoSeleccionado = producto.ProductoId;
        LblProducto.Text = $"{producto.NombreProducto} · stock actual: {producto.StockFormateado}";
        BtnAjustar.IsEnabled = true;
    }

    private async void AlAjustar(object sender, RoutedEventArgs e)
    {
        if (!_puedeAjustar || _productoSeleccionado <= 0)
            return;

        var tipo = (CboTipo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "AJUSTE";

        var respuesta = await App.Api.AjustarStockAsync(new AjustarStockDto
        {
            UsuarioId = Sesion.UsuarioId,
            ProductoId = _productoSeleccionado,
            TipoMovimiento = tipo,
            Cantidad = Numero(TxtCantidad.Text),
            Motivo = TxtMotivo.Text.Trim()
        });

        MessageBox.Show(respuesta.Message, "Cotillón", MessageBoxButton.OK,
            respuesta.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);

        if (!respuesta.Success)
            return;

        TxtCantidad.Text = "0";
        TxtMotivo.Clear();
        TxtProducto.Clear();
        LblProducto.Text = string.Empty;
        _productoSeleccionado = 0;
        BtnAjustar.IsEnabled = false;

        await BuscarAsync();
    }

    private static decimal Numero(string? texto) =>
        decimal.TryParse(texto?.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out var valor)
            ? valor
            : 0m;
}

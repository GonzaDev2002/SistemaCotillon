using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Cotillon.Shared.DTOs;

namespace Cotillon.Desktop.Wpf.Vistas.Paginas;

public partial class PaginaProductos : UserControl
{
    private List<ProductoListadoDto> _productos = [];
    private int _productoSeleccionado;

    public PaginaProductos()
    {
        InitializeComponent();

        Loaded += async (_, _) =>
        {
            await CargarSeccionesAsync();
            await CargarProductosAsync();
        };
    }

    private async Task CargarSeccionesAsync()
    {
        var secciones = await App.Api.SeccionesAsync();

        if (secciones.Success && secciones.Data is not null)
            CboSeccion.ItemsSource = secciones.Data;
    }

    private async Task CargarProductosAsync()
    {
        var respuesta = await App.Api.ProductosAsync(ChkInactivos.IsChecked == true);

        _productos = respuesta.Data ?? [];
        Grilla.ItemsSource = _productos;

        AplicarFiltro();
    }

    private async void AlCambiarSeccion(object sender, SelectionChangedEventArgs e)
    {
        if (CboSeccion.SelectedValue is not int seccionId)
            return;

        var categorias = await App.Api.CategoriasPorSeccionAsync(seccionId);

        CboCategoria.ItemsSource = categorias.Data;
    }

    private async void AlRefrescar(object sender, RoutedEventArgs e) => await CargarProductosAsync();

    private void AlFiltrar(object sender, TextChangedEventArgs e) => AplicarFiltro();

    private void AplicarFiltro()
    {
        if (Grilla.ItemsSource is null)
            return;

        var texto = TxtFiltro.Text.Trim();
        var vista = CollectionViewSource.GetDefaultView(Grilla.ItemsSource);

        vista.Filter = string.IsNullOrWhiteSpace(texto)
            ? null
            : objeto =>
            {
                var producto = (ProductoListadoDto)objeto;

                return producto.NombreProducto.Contains(texto, StringComparison.OrdinalIgnoreCase)
                    || producto.NombreSeccion.Contains(texto, StringComparison.OrdinalIgnoreCase)
                    || producto.NombreCategoria.Contains(texto, StringComparison.OrdinalIgnoreCase);
            };
    }

    private async void AlSeleccionar(object sender, SelectionChangedEventArgs e)
    {
        if (Grilla.SelectedItem is not ProductoListadoDto fila)
            return;

        var respuesta = await App.Api.ProductoAsync(fila.ProductoId);

        if (!respuesta.Success || respuesta.Data is null)
            return;

        var producto = respuesta.Data;

        _productoSeleccionado = producto.ProductoId;
        LblFormulario.Text = $"Editando: {producto.NombreProducto}";

        TxtNombre.Text = producto.NombreProducto;
        TxtCodigoBarras.Text = producto.CodigoBarras;
        TxtPrecio.Text = producto.Precio.ToString(CultureInfo.CurrentCulture);
        TxtCosto.Text = producto.Costo.ToString(CultureInfo.CurrentCulture);
        TxtStock.Text = producto.Stock.ToString();
        TxtStockMinimo.Text = producto.StockMinimo.ToString();
        TxtMarca.Text = producto.Marca;
        ChkSinStock.IsChecked = producto.PermiteVentaSinStock;

        // El stock no se edita desde acá: cambiarlo a mano dejaría el inventario sin
        // explicación. Para eso está la pantalla de movimientos de stock.
        TxtStock.IsEnabled = false;
        LblStockAviso.Text = "El stock se modifica desde Movimientos de stock, así queda registrado el motivo.";
        LblStockAviso.Visibility = Visibility.Visible;

        BtnEliminar.IsEnabled = true;

        await SeleccionarCategoriaAsync(producto.CategoriaId);
    }

    private async Task SeleccionarCategoriaAsync(int categoriaId)
    {
        var categorias = await App.Api.CategoriasAsync();

        var categoria = categorias.Data?.FirstOrDefault(c => c.CategoriaId == categoriaId);

        if (categoria is null)
            return;

        CboSeccion.SelectedValue = categoria.SeccionId;

        var deLaSeccion = await App.Api.CategoriasPorSeccionAsync(categoria.SeccionId);
        CboCategoria.ItemsSource = deLaSeccion.Data;
        CboCategoria.SelectedValue = categoriaId;
    }

    private async void AlGuardar(object sender, RoutedEventArgs e)
    {
        if (CboCategoria.SelectedValue is not int categoriaId)
        {
            MessageBox.Show("Elegí una sección y una categoría.", "Cotillón",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(TxtNombre.Text))
        {
            MessageBox.Show("El nombre del producto es obligatorio.", "Cotillón",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var producto = new ProductoDto
        {
            ProductoId = _productoSeleccionado,
            CategoriaId = categoriaId,
            NombreProducto = TxtNombre.Text.Trim(),
            CodigoBarras = TxtCodigoBarras.Text.Trim(),
            Precio = Numero(TxtPrecio.Text),
            Costo = Numero(TxtCosto.Text),
            Stock = (int)Numero(TxtStock.Text),
            StockMinimo = (int)Numero(TxtStockMinimo.Text),
            Marca = TxtMarca.Text.Trim(),
            UnidadMedida = "UNIDAD",
            PermiteVentaSinStock = ChkSinStock.IsChecked == true
        };

        BtnGuardar.IsEnabled = false;

        try
        {
            var respuesta = _productoSeleccionado > 0
                ? await App.Api.ActualizarProductoAsync(producto)
                : await App.Api.CrearProductoAsync(producto);

            if (!respuesta.Success)
            {
                MessageBox.Show(respuesta.Message, "Cotillón", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show(respuesta.Message, "Cotillón", MessageBoxButton.OK, MessageBoxImage.Information);

            Limpiar();
            await CargarProductosAsync();
        }
        finally
        {
            BtnGuardar.IsEnabled = true;
        }
    }

    private async void AlEliminar(object sender, RoutedEventArgs e)
    {
        if (_productoSeleccionado <= 0)
            return;

        var confirmar = MessageBox.Show(
            "¿Dar de baja este producto? No se borra: deja de aparecer en ventas pero su historial se conserva.",
            "Cotillón", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirmar != MessageBoxResult.Yes)
            return;

        var respuesta = await App.Api.EliminarProductoAsync(_productoSeleccionado);

        MessageBox.Show(respuesta.Message, "Cotillón", MessageBoxButton.OK,
            respuesta.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);

        if (respuesta.Success)
        {
            Limpiar();
            await CargarProductosAsync();
        }
    }

    private void AlNuevo(object sender, RoutedEventArgs e) => Limpiar();

    private void Limpiar()
    {
        _productoSeleccionado = 0;
        LblFormulario.Text = "Nuevo producto";

        TxtNombre.Clear();
        TxtCodigoBarras.Clear();
        TxtPrecio.Text = "0";
        TxtCosto.Text = "0";
        TxtStock.Text = "0";
        TxtStockMinimo.Text = "0";
        TxtMarca.Clear();
        ChkSinStock.IsChecked = false;

        TxtStock.IsEnabled = true;
        LblStockAviso.Visibility = Visibility.Collapsed;
        BtnEliminar.IsEnabled = false;

        CboCategoria.SelectedIndex = -1;
        Grilla.SelectedItem = null;
    }

    private static decimal Numero(string? texto) =>
        decimal.TryParse(texto?.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out var valor)
            ? valor
            : 0m;
}

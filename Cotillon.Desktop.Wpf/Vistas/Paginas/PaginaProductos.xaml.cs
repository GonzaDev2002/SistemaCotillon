using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Cotillon.Shared.DTOs;

namespace Cotillon.Desktop.Wpf.Vistas.Paginas;

public partial class PaginaProductos : UserControl
{
    private List<ProductoListadoDto> _productos = [];
    private int _productoSeleccionado;

    public PaginaProductos()
    {
        InitializeComponent();

        Adjuntos.Titulo = "Fotos del producto";
        Adjuntos.AvisoSinSeleccion =
            "Elegí o escaneá un producto para agregarle fotos o su ficha del proveedor.";

        Loaded += async (_, _) =>
        {
            await CargarSeccionesAsync();
            await CargarProductosAsync();

            // El lector de código de barras es un teclado: si el campo arranca
            // enfocado, disparar sobre una etiqueta ya carga el producto.
            TxtCodigoBarras.Focus();
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

    // ------------------------------------------------- Carga con el lector --

    /// <summary>
    /// El lector escribe el código y manda Enter. Si el código ya está cargado se
    /// trae el producto completo y se llenan todos los campos para editarlo; si no
    /// existe, el código queda puesto y el foco pasa al nombre para darlo de alta.
    /// </summary>
    private async void AlTeclaCodigo(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        e.Handled = true;

        var codigo = TxtCodigoBarras.Text.Trim();

        if (codigo.Length == 0)
            return;

        var respuesta = await App.Api.ProductoPorCodigoAsync(codigo);

        if (!respuesta.Success)
        {
            MostrarEscaneo(respuesta.Message, Peligro);
            return;
        }

        if (respuesta.Data is null)
        {
            // Código nuevo: se arranca un alta en limpio pero conservando el código.
            Limpiar(conservarCodigo: codigo);
            MostrarEscaneo($"El código {codigo} es nuevo. Completá el producto y guardalo.", Exito);
            TxtNombre.Focus();
            return;
        }

        await CargarEnFormularioAsync(respuesta.Data);

        var aviso = respuesta.Data.Activo
            ? $"Producto cargado: {respuesta.Data.NombreProducto}. Modificá lo que necesites y guardá."
            : $"«{respuesta.Data.NombreProducto}» está dado de baja, por eso no aparece en ventas. " +
              "Usá «Volver a poner en venta» si lo vas a vender de nuevo.";

        MostrarEscaneo(aviso, respuesta.Data.Activo ? Exito : Advertencia);

        // Dejar el foco en el precio: al reponer mercadería es lo que más cambia.
        TxtPrecio.Focus();
        TxtPrecio.SelectAll();
    }

    private void MostrarEscaneo(string mensaje, Brush color)
    {
        LblEscaneo.Text = mensaje;
        LblEscaneo.Foreground = color;
    }

    // ------------------------------------------------------------ Selección --

    private async void AlSeleccionar(object sender, SelectionChangedEventArgs e)
    {
        if (Grilla.SelectedItem is not ProductoListadoDto fila)
            return;

        var respuesta = await App.Api.ProductoAsync(fila.ProductoId);

        if (!respuesta.Success || respuesta.Data is null)
            return;

        await CargarEnFormularioAsync(respuesta.Data);
    }

    /// <summary>
    /// Vuelca el producto en todos los campos del formulario. Lo usan tanto el
    /// lector como la selección en la grilla, para que las dos vías carguen
    /// exactamente lo mismo.
    /// </summary>
    private async Task CargarEnFormularioAsync(ProductoDto producto)
    {
        _productoSeleccionado = producto.ProductoId;
        LblFormulario.Text = $"Editando: {producto.NombreProducto}";

        TxtNombre.Text = producto.NombreProducto;
        TxtCodigoBarras.Text = producto.CodigoBarras;
        TxtCodigoProducto.Text = producto.CodigoProducto;
        TxtPrecio.Text = producto.Precio.ToString(CultureInfo.CurrentCulture);
        TxtCosto.Text = producto.Costo.ToString(CultureInfo.CurrentCulture);
        TxtStock.Text = producto.Stock.ToString();
        TxtStockMinimo.Text = producto.StockMinimo.ToString();
        TxtMarca.Text = producto.Marca;
        TxtDescripcion.Text = producto.Descripcion;
        ChkSinStock.IsChecked = producto.PermiteVentaSinStock;

        // El stock no se edita desde acá: cambiarlo a mano dejaría el inventario sin
        // explicación. Para eso está la pantalla de movimientos de stock.
        TxtStock.IsEnabled = false;
        LblStockAviso.Text = "El stock se modifica desde Movimientos de stock, así queda registrado el motivo.";
        LblStockAviso.Visibility = Visibility.Visible;

        BtnEliminar.IsEnabled = producto.Activo;
        BtnReactivar.Visibility = producto.Activo ? Visibility.Collapsed : Visibility.Visible;

        ActualizarMargen();

        Adjuntos.Titulo = $"Fotos de {producto.NombreProducto}";

        await SeleccionarCategoriaAsync(producto.CategoriaId);
        await Adjuntos.CargarAsync(producto.ProductoId);
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

    // --------------------------------------------------------------- Margen --

    private void AlCambiarPrecioOCosto(object sender, TextChangedEventArgs e) => ActualizarMargen();

    /// <summary>
    /// Muestra el margen sobre el costo mientras se tipea. Es el control que evita
    /// el error más caro del alta: dejar un precio por debajo de lo que costó.
    /// </summary>
    private void ActualizarMargen()
    {
        // TextChanged salta durante InitializeComponent, cuando los controles de
        // más abajo todavía no existen.
        if (PanelMargen is null || LblMargen is null)
            return;

        var precio = Numero(TxtPrecio.Text);
        var costo = Numero(TxtCosto.Text);

        if (costo <= 0 || precio <= 0)
        {
            PanelMargen.Visibility = Visibility.Collapsed;
            return;
        }

        var ganancia = precio - costo;
        var porcentaje = ganancia / costo * 100m;

        LblMargen.Text = $"{ganancia:C2}   ({porcentaje:0.#} %)";
        LblMargen.Foreground = ganancia <= 0 ? Peligro : Exito;

        PanelMargen.Visibility = Visibility.Visible;
    }

    // --------------------------------------------------------------- Guardar --

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
            CodigoProducto = TxtCodigoProducto.Text.Trim(),
            Descripcion = TxtDescripcion.Text.Trim(),
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

            MostrarEscaneo($"{producto.NombreProducto} guardado. Dispará el lector para el siguiente.", Exito);

            Limpiar();
            await CargarProductosAsync();

            // Volver al lector cierra el ciclo: cargar una caja entera de mercadería
            // es escanear, completar, guardar, escanear otra vez, sin usar el mouse.
            TxtCodigoBarras.Focus();
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

    private async void AlReactivar(object sender, RoutedEventArgs e)
    {
        if (_productoSeleccionado <= 0)
            return;

        var respuesta = await App.Api.ReactivarProductoAsync(_productoSeleccionado);

        MessageBox.Show(respuesta.Message, "Cotillón", MessageBoxButton.OK,
            respuesta.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);

        if (!respuesta.Success)
            return;

        BtnReactivar.Visibility = Visibility.Collapsed;
        BtnEliminar.IsEnabled = true;

        await CargarProductosAsync();
    }

    private void AlNuevo(object sender, RoutedEventArgs e)
    {
        Limpiar();
        MostrarEscaneo(
            "Dispará el lector sobre la etiqueta. Si el código ya existe se cargan todos los campos " +
            "para editarlo; si es nuevo, completás el resto.",
            (Brush)Application.Current.Resources["TextoSecundario"]);

        TxtCodigoBarras.Focus();
    }

    /// <summary>
    /// Deja el formulario en blanco. Con <paramref name="conservarCodigo"/> mantiene
    /// el código recién escaneado, que es justo el dato que no hay que volver a pedir.
    /// </summary>
    private void Limpiar(string? conservarCodigo = null)
    {
        _productoSeleccionado = 0;
        LblFormulario.Text = "Nuevo producto";

        TxtNombre.Clear();
        TxtCodigoBarras.Text = conservarCodigo ?? string.Empty;
        TxtCodigoProducto.Clear();
        TxtPrecio.Text = "0";
        TxtCosto.Text = "0";
        TxtStock.Text = "0";
        TxtStockMinimo.Text = "0";
        TxtMarca.Clear();
        TxtDescripcion.Clear();
        ChkSinStock.IsChecked = false;

        TxtStock.IsEnabled = true;
        LblStockAviso.Visibility = Visibility.Collapsed;
        PanelMargen.Visibility = Visibility.Collapsed;
        BtnEliminar.IsEnabled = false;
        BtnReactivar.Visibility = Visibility.Collapsed;

        CboCategoria.SelectedIndex = -1;
        Grilla.SelectedItem = null;

        Adjuntos.Titulo = "Fotos del producto";
        _ = Adjuntos.LimpiarAsync();
    }

    private static Brush Exito => (Brush)Application.Current.Resources["Exito"];
    private static Brush Peligro => (Brush)Application.Current.Resources["Peligro"];
    private static Brush Advertencia => (Brush)Application.Current.Resources["Advertencia"];

    private static decimal Numero(string? texto) =>
        decimal.TryParse(texto?.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out var valor)
            ? valor
            : 0m;
}

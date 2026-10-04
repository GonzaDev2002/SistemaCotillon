using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Cotillon.Desktop.Wpf.Servicios;
using Cotillon.Shared.DTOs;
using Cotillon.Shared.DTOs.Permisos;

namespace Cotillon.Desktop.Wpf.Vistas.Paginas;

public partial class PaginaProductos : UserControl
{
    private List<ProductoListadoDto> _productos = [];
    private int _productoSeleccionado;
    private bool _puedeEditar;

    private readonly ObservableCollection<ComponenteComboDto> _componentes = [];

    public PaginaProductos()
    {
        InitializeComponent();

        Adjuntos.Titulo = "Fotos del producto";
        Adjuntos.AvisoSinSeleccion =
            "Elegí o escaneá un producto para agregarle fotos o su ficha del proveedor.";
        Adjuntos.AplicarPermisos(Modulos.Productos);

        // Con permiso de sólo ver, el lector sigue sirviendo para consultar precio
        // y stock de una etiqueta, pero el formulario no guarda.
        _puedeEditar = Sesion.PuedeEditar(Modulos.Productos);

        BtnGuardar.IsEnabled = _puedeEditar;
        BtnGuardar.Visibility = _puedeEditar ? Visibility.Visible : Visibility.Collapsed;

        var acciones = Sesion.PuedeTodo(Modulos.Productos) ? Visibility.Visible : Visibility.Collapsed;
        BtnEliminar.Visibility = acciones;

        // El alta masiva pide permiso total: es la pantalla desde la que se crean
        // doscientos productos de un saque y conviene que sea cosa del dueño.
        BtnImportar.Visibility = acciones;

        // Cambiar el precio de media lista de un saque es cosa del dueno.
        BtnPrecios.Visibility = acciones;

        CboUnidad.ItemsSource = UnidadesMedida.Todas;
        CboUnidad.SelectedValue = UnidadesMedida.Unidad;

        CboTemporada.ItemsSource = Temporadas.Todas.Select(t => new { t.Codigo, t.Nombre }).ToList();
        CboTemporada.SelectedValue = Temporadas.TodoElAno;

        GrillaCombo.ItemsSource = _componentes;

        Loaded += async (_, _) =>
        {
            await CargarSeccionesAsync();
            await CargarProductosAsync();

            // El lector de código de barras es un teclado: si el campo arranca
            // enfocado, disparar sobre una etiqueta ya carga el producto.
            TxtCodigoBarras.Focus();
        };

        PreviewKeyDown += AlTeclaFormulario;
    }

    /// <summary>
    /// Carga en cadena: Enter baja al campo siguiente y F12 guarda. Cargar una caja
    /// de mercadería es disparar el lector, tipear nombre, precio, costo y stock
    /// bajando con Enter, apretar F12 y disparar el lector otra vez. La mano no se
    /// va del teclado y no se toca el mouse en ningún momento.
    /// </summary>
    private void AlTeclaFormulario(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F12)
        {
            AlGuardar(this, new RoutedEventArgs());
            e.Handled = true;
            return;
        }

        if (e.Key != Key.Enter)
            return;

        // El campo del lector tiene su propio Enter: busca el código. Y en la
        // descripción Enter es un salto de línea, porque admite varias.
        if (Keyboard.FocusedElement is not TextBox campo ||
            ReferenceEquals(campo, TxtCodigoBarras) ||
            ReferenceEquals(campo, TxtDescripcion))
            return;

        campo.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

        // El siguiente campo queda seleccionado: se sobreescribe tipeando, sin
        // tener que borrar el 0 que trae.
        if (Keyboard.FocusedElement is TextBox siguiente)
            siguiente.SelectAll();

        e.Handled = true;
    }

    private async Task CargarSeccionesAsync()
    {
        var secciones = await App.Api.SeccionesAsync();

        if (!secciones.Success || secciones.Data is null)
            return;

        CboSeccion.ItemsSource = secciones.Data;

        // Arranca con una sección elegida: cargar el primer producto del día no
        // tiene por qué empezar pasando por los dos combos.
        if (secciones.Data.Count > 0 && CboSeccion.SelectedIndex < 0)
            CboSeccion.SelectedIndex = 0;
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

        // En un alta se deja elegida la primera categoría de la sección. Al editar
        // no, porque la categoría que corresponde la pone el producto cargado.
        if (_productoSeleccionado == 0 && categorias.Data?.Count > 0)
            CboCategoria.SelectedIndex = 0;
    }

    /// <summary>
    /// Las etiquetas siguen a la unidad elegida: "PRECIO POR KILO" deja claro que
    /// el número que se carga es el del kilo y no el del paquete.
    /// </summary>
    private void AlCambiarUnidad(object sender, SelectionChangedEventArgs e)
    {
        if (CboUnidad.SelectedItem is not UnidadMedida unidad || LblPrecio is null)
            return;

        LblPrecio.Text = unidad.PrecioPor;
        LblCosto.Text = unidad.Fraccionada ? $"COSTO POR {unidad.Simbolo.ToUpperInvariant()}" : "COSTO";
        LblStock.Text = $"STOCK ({unidad.Simbolo})";
        LblStockMinimo.Text = $"STOCK MÍNIMO ({unidad.Simbolo})";
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
        TxtStock.Text = producto.Stock.ToString("0.###", CultureInfo.CurrentCulture);
        TxtStockMinimo.Text = producto.StockMinimo.ToString("0.###", CultureInfo.CurrentCulture);
        CboUnidad.SelectedValue = UnidadesMedida.De(producto.UnidadMedida).Codigo;
        TxtMarca.Text = producto.Marca;
        TxtDescripcion.Text = producto.Descripcion;
        ChkSinStock.IsChecked = producto.PermiteVentaSinStock;
        TxtUnidadesPorBulto.Text = producto.UnidadesPorBulto.ToString("0.###", CultureInfo.CurrentCulture);
        TxtPrecioBulto.Text = producto.PrecioBulto.ToString(CultureInfo.CurrentCulture);
        CboTemporada.SelectedValue = producto.Temporada ?? Temporadas.TodoElAno;
        ChkEsCombo.IsChecked = producto.EsCombo;

        _componentes.Clear();

        if (producto.EsCombo)
        {
            var receta = await App.Api.ComponentesComboAsync(producto.ProductoId);

            foreach (var componente in receta.Data ?? [])
                _componentes.Add(componente);

            ActualizarResumenCombo();
        }

        // El stock no se edita desde acá: cambiarlo a mano dejaría el inventario sin
        // explicación. Para eso está la pantalla de movimientos de stock.
        TxtStock.IsEnabled = false;
        LblStockAviso.Text = "El stock se modifica desde Movimientos de stock, así queda registrado el motivo.";
        LblStockAviso.Visibility = Visibility.Visible;

        var acciones = Sesion.PuedeTodo(Modulos.Productos);

        BtnEliminar.IsEnabled = producto.Activo && acciones;
        BtnReactivar.Visibility = !producto.Activo && acciones ? Visibility.Visible : Visibility.Collapsed;

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
        if (!_puedeEditar)
            return;

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
            Stock = Numero(TxtStock.Text),
            StockMinimo = Numero(TxtStockMinimo.Text),
            Marca = TxtMarca.Text.Trim(),
            UnidadMedida = CboUnidad.SelectedValue as string ?? UnidadesMedida.Unidad,
            PermiteVentaSinStock = ChkSinStock.IsChecked == true,
            UnidadesPorBulto = Numero(TxtUnidadesPorBulto.Text),
            PrecioBulto = Numero(TxtPrecioBulto.Text),
            Temporada = CboTemporada.SelectedValue as string ?? string.Empty,
            EsCombo = ChkEsCombo.IsChecked == true
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

            // La receta se guarda aparte porque necesita el id del combo. En un alta
            // el id lo asigna la base reción ahora, asi que el producto se guarda
            // primero y la receta se carga al volver a abrirlo.
            if (producto.EsCombo && _componentes.Count > 0 && _productoSeleccionado > 0)
            {
                var receta = await App.Api.GuardarComboAsync(_productoSeleccionado, [.. _componentes]);

                if (!receta.Success)
                {
                    MessageBox.Show(receta.Message, "Cotillón", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else if (producto.EsCombo && _componentes.Count > 0)
            {
                MessageBox.Show(
                    "El combo quedó creado. Abrilo de la lista para guardarle qué lleva adentro.",
                    "Cotillón", MessageBoxButton.OK, MessageBoxImage.Information);
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

    // ------------------------------------------------------------- Combos --

    private void AlCambiarCombo(object sender, RoutedEventArgs e)
    {
        if (PanelCombo is null)
            return;

        PanelCombo.Visibility = ChkEsCombo.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

        // Un combo no tiene stock propio: se arma con lo que hay de sus partes.
        TxtStock.IsEnabled = ChkEsCombo.IsChecked != true && _productoSeleccionado == 0;
    }

    private async void AlTeclaComponente(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        e.Handled = true;
        await AgregarComponenteAsync();
    }

    private async void AlAgregarComponente(object sender, RoutedEventArgs e) =>
        await AgregarComponenteAsync();

    /// <summary>
    /// Suma un producto a la receta. Acepta "5*caramelo" igual que el buscador de
    /// ventas, para no tener que cargar cinco veces el mismo renglón.
    /// </summary>
    private async Task AgregarComponenteAsync()
    {
        var texto = TxtComponente.Text.Trim();

        if (texto.Length == 0)
            return;

        var cantidad = 1m;
        var separador = texto.IndexOfAny(['*', 'x', 'X']);

        if (separador > 0 && Numero(texto[..separador]) > 0)
        {
            cantidad = Numero(texto[..separador]);
            texto = texto[(separador + 1)..].Trim();
        }

        var respuesta = await App.Api.BuscarProductosAsync(texto);

        if (!respuesta.Success || respuesta.Data is null || respuesta.Data.Count == 0)
        {
            MostrarEscaneo($"No se encontró ningún producto con «{texto}».", Peligro);
            return;
        }

        var producto = respuesta.Data[0];

        if (producto.ProductoId == _productoSeleccionado)
        {
            MostrarEscaneo("Un combo no puede llevarse a sí mismo adentro.", Peligro);
            return;
        }

        if (producto.EsCombo)
        {
            MostrarEscaneo($"«{producto.NombreProducto}» también es un combo y no puede ir adentro de otro.", Peligro);
            return;
        }

        // Si ya estaba, se suma en vez de duplicar el renglón.
        var existente = _componentes.FirstOrDefault(c => c.ComponenteProductoId == producto.ProductoId);

        if (existente is not null)
        {
            existente.Cantidad += cantidad;
            GrillaCombo.Items.Refresh();
        }
        else
        {
            _componentes.Add(new ComponenteComboDto
            {
                ComponenteProductoId = producto.ProductoId,
                NombreProducto = producto.NombreProducto,
                Cantidad = cantidad,
                UnidadMedida = producto.UnidadMedida,
                StockComponente = producto.Stock
            });
        }

        TxtComponente.Clear();
        TxtComponente.Focus();
        ActualizarResumenCombo();
    }

    private void AlQuitarComponente(object sender, RoutedEventArgs e)
    {
        if (GrillaCombo.SelectedItem is ComponenteComboDto componente)
            _componentes.Remove(componente);

        ActualizarResumenCombo();
    }

    private void ActualizarResumenCombo()
    {
        if (_componentes.Count == 0)
        {
            LblCostoCombo.Text = string.Empty;
            return;
        }

        var armables = _componentes.Min(c => c.CombosQueAlcanza);

        LblCostoCombo.Text =
            $"Con el stock de hoy se pueden armar {armables:0} combos. " +
            $"El costo de armar uno es {_componentes.Sum(c => c.CostoLinea):C2}.";
    }

    private async void AlAjustarPrecios(object sender, RoutedEventArgs e)
    {
        var ventana = new VentanaAumentoPrecios { Owner = Window.GetWindow(this) };

        if (ventana.ShowDialog() == true)
            await CargarProductosAsync();
    }

    private async void AlImportar(object sender, RoutedEventArgs e)
    {
        var ventana = new VentanaImportarProductos { Owner = Window.GetWindow(this) };

        if (ventana.ShowDialog() == true)
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
        CboUnidad.SelectedValue = UnidadesMedida.Unidad;
        TxtMarca.Clear();
        TxtDescripcion.Clear();
        ChkSinStock.IsChecked = false;
        TxtUnidadesPorBulto.Text = "1";
        TxtPrecioBulto.Text = "0";
        CboTemporada.SelectedValue = Temporadas.TodoElAno;
        ChkEsCombo.IsChecked = false;
        PanelCombo.Visibility = Visibility.Collapsed;
        _componentes.Clear();
        LblCostoCombo.Text = string.Empty;

        TxtStock.IsEnabled = true;
        LblStockAviso.Visibility = Visibility.Collapsed;
        PanelMargen.Visibility = Visibility.Collapsed;
        BtnGuardar.IsEnabled = _puedeEditar;
        BtnEliminar.IsEnabled = false;
        BtnReactivar.Visibility = Visibility.Collapsed;

        // La sección y la categoría NO se limpian: la mercadería entra por cajas
        // del mismo rubro, así que el que acaba de cargar diez globos va a cargar
        // el once en el mismo lugar. Se elige una vez por caja, no una por producto.
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

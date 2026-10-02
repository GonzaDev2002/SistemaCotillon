using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Cotillon.Shared.DTOs;

namespace Cotillon.Desktop.Wpf.Vistas.Paginas;

public partial class PaginaCatalogo : UserControl
{
    private int _seccionSeleccionada;
    private int _categoriaSeleccionada;

    public PaginaCatalogo()
    {
        InitializeComponent();

        Loaded += async (_, _) => await CargarSeccionesAsync();
    }

    private async Task CargarSeccionesAsync()
    {
        var respuesta = await App.Api.SeccionesAsync();
        var secciones = respuesta.Data ?? [];

        GrillaSecciones.ItemsSource = secciones;

        // El combo de la derecha es el que permite mover una categoría de sección,
        // así que se llena con la misma lista que la grilla.
        var elegida = CboSeccionCategoria.SelectedValue;
        CboSeccionCategoria.ItemsSource = secciones;
        CboSeccionCategoria.SelectedValue = elegida ?? _seccionSeleccionada;
    }

    private async void AlSeleccionarSeccion(object sender, SelectionChangedEventArgs e)
    {
        if (GrillaSecciones.SelectedItem is not SeccionDto seccion)
            return;

        _seccionSeleccionada = seccion.SeccionId;
        TxtSeccion.Text = seccion.NombreSeccion;
        ChkSeccionActiva.IsChecked = seccion.Activo;

        BtnActualizarSeccion.IsEnabled = true;
        BtnBorrarSeccion.IsEnabled = true;

        LblCategorias.Text = $"Categorías de {seccion.NombreSeccion}";
        LblAyudaCategorias.Text = seccion.CantidadCategorias == 0
            ? "Esta sección todavía no tiene categorías. Agregá la primera abajo."
            : $"{seccion.CantidadCategorias} categoría(s) y {seccion.CantidadProductos} producto(s) activos.";

        CboSeccionCategoria.SelectedValue = seccion.SeccionId;

        await RecargarCategoriasAsync();
    }

    private void AlSeleccionarCategoria(object sender, SelectionChangedEventArgs e)
    {
        if (GrillaCategorias.SelectedItem is not CategoriaDto categoria)
            return;

        _categoriaSeleccionada = categoria.CategoriaId;
        TxtCategoria.Text = categoria.NombreCategoria;
        ChkCategoriaActiva.IsChecked = categoria.Activo;
        CboSeccionCategoria.SelectedValue = categoria.SeccionId;

        BtnActualizarCategoria.IsEnabled = true;
        BtnBorrarCategoria.IsEnabled = true;
    }

    // ------------------------------------------------------------ Secciones --

    /// <summary>
    /// Enter guarda sobre la sección ya elegida, o da de alta si no hay ninguna.
    /// Cargar un catálogo entero es escribir y apretar Enter, sin ir al botón.
    /// </summary>
    private void AlTeclaSeccion(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        e.Handled = true;

        if (_seccionSeleccionada > 0)
            AlActualizarSeccion(sender, e);
        else
            AlAgregarSeccion(sender, e);
    }

    private async void AlAgregarSeccion(object sender, RoutedEventArgs e)
    {
        if (!Validar(TxtSeccion.Text, "el nombre de la sección"))
            return;

        var respuesta = await App.Api.CrearSeccionAsync(new SeccionDto
        {
            NombreSeccion = TxtSeccion.Text.Trim(),
            Activo = ChkSeccionActiva.IsChecked == true
        });

        Informar(respuesta.Success, respuesta.Message);

        if (respuesta.Success)
        {
            LimpiarSeccion();
            await CargarSeccionesAsync();
        }
    }

    private async void AlActualizarSeccion(object sender, RoutedEventArgs e)
    {
        if (_seccionSeleccionada <= 0)
        {
            MessageBox.Show("Elegí una sección de la lista.", "Cotillón",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!Validar(TxtSeccion.Text, "el nombre de la sección"))
            return;

        var respuesta = await App.Api.ActualizarSeccionAsync(new SeccionDto
        {
            SeccionId = _seccionSeleccionada,
            NombreSeccion = TxtSeccion.Text.Trim(),
            Activo = ChkSeccionActiva.IsChecked == true
        });

        Informar(respuesta.Success, respuesta.Message);

        if (respuesta.Success)
        {
            LimpiarSeccion();
            await CargarSeccionesAsync();
        }
    }

    private async void AlBorrarSeccion(object sender, RoutedEventArgs e)
    {
        if (_seccionSeleccionada <= 0)
            return;

        var confirmar = MessageBox.Show(
            $"¿Borrar la sección «{TxtSeccion.Text.Trim()}»?",
            "Cotillón", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirmar != MessageBoxResult.Yes)
            return;

        // La API rechaza el borrado si quedó algo colgando y explica por qué.
        var respuesta = await App.Api.EliminarSeccionAsync(_seccionSeleccionada);

        Informar(respuesta.Success, respuesta.Message);

        if (respuesta.Success)
        {
            LimpiarSeccion();
            LimpiarCategoria();
            GrillaCategorias.ItemsSource = null;
            LblCategorias.Text = "Categorías";
            LblAyudaCategorias.Text = "Elegí primero una sección de la izquierda.";

            await CargarSeccionesAsync();
        }
    }

    private void AlLimpiarSeccion(object sender, RoutedEventArgs e) => LimpiarSeccion();

    private void LimpiarSeccion()
    {
        _seccionSeleccionada = 0;
        TxtSeccion.Clear();
        ChkSeccionActiva.IsChecked = true;
        GrillaSecciones.SelectedItem = null;

        BtnActualizarSeccion.IsEnabled = false;
        BtnBorrarSeccion.IsEnabled = false;
    }

    // ----------------------------------------------------------- Categorías --

    private void AlTeclaCategoria(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        e.Handled = true;

        if (_categoriaSeleccionada > 0)
            AlActualizarCategoria(sender, e);
        else
            AlAgregarCategoria(sender, e);
    }

    private async void AlAgregarCategoria(object sender, RoutedEventArgs e)
    {
        if (CboSeccionCategoria.SelectedValue is not int seccionId)
        {
            MessageBox.Show("Elegí la sección a la que pertenece.", "Cotillón",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!Validar(TxtCategoria.Text, "el nombre de la categoría"))
            return;

        var respuesta = await App.Api.CrearCategoriaAsync(new CategoriaDto
        {
            SeccionId = seccionId,
            NombreCategoria = TxtCategoria.Text.Trim(),
            Activo = ChkCategoriaActiva.IsChecked == true
        });

        Informar(respuesta.Success, respuesta.Message);

        if (respuesta.Success)
            await RefrescarTodoAsync();
    }

    private async void AlActualizarCategoria(object sender, RoutedEventArgs e)
    {
        if (_categoriaSeleccionada <= 0)
        {
            MessageBox.Show("Elegí una categoría de la lista.", "Cotillón",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (CboSeccionCategoria.SelectedValue is not int seccionId)
        {
            MessageBox.Show("Elegí la sección a la que pertenece.", "Cotillón",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!Validar(TxtCategoria.Text, "el nombre de la categoría"))
            return;

        var respuesta = await App.Api.ActualizarCategoriaAsync(new CategoriaDto
        {
            CategoriaId = _categoriaSeleccionada,
            SeccionId = seccionId,
            NombreCategoria = TxtCategoria.Text.Trim(),
            Activo = ChkCategoriaActiva.IsChecked == true
        });

        Informar(respuesta.Success, respuesta.Message);

        if (respuesta.Success)
            await RefrescarTodoAsync();
    }

    private async void AlBorrarCategoria(object sender, RoutedEventArgs e)
    {
        if (_categoriaSeleccionada <= 0)
            return;

        var confirmar = MessageBox.Show(
            $"¿Borrar la categoría «{TxtCategoria.Text.Trim()}»?",
            "Cotillón", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirmar != MessageBoxResult.Yes)
            return;

        var respuesta = await App.Api.EliminarCategoriaAsync(_categoriaSeleccionada);

        Informar(respuesta.Success, respuesta.Message);

        if (respuesta.Success)
            await RefrescarTodoAsync();
    }

    private void AlLimpiarCategoria(object sender, RoutedEventArgs e) => LimpiarCategoria();

    /// <summary>
    /// Recarga las dos listas: mover una categoría o borrarla cambia los contadores
    /// de las secciones, no sólo la lista de la derecha.
    /// </summary>
    private async Task RefrescarTodoAsync()
    {
        LimpiarCategoria();

        await CargarSeccionesAsync();
        await RecargarCategoriasAsync();
    }

    private async Task RecargarCategoriasAsync()
    {
        if (_seccionSeleccionada <= 0)
            return;

        var categorias = await App.Api.CategoriasPorSeccionAsync(_seccionSeleccionada);
        GrillaCategorias.ItemsSource = categorias.Data;
    }

    private void LimpiarCategoria()
    {
        _categoriaSeleccionada = 0;
        TxtCategoria.Clear();
        ChkCategoriaActiva.IsChecked = true;
        GrillaCategorias.SelectedItem = null;

        CboSeccionCategoria.SelectedValue = _seccionSeleccionada;

        BtnActualizarCategoria.IsEnabled = false;
        BtnBorrarCategoria.IsEnabled = false;
    }

    private static bool Validar(string texto, string campo)
    {
        if (!string.IsNullOrWhiteSpace(texto))
            return true;

        MessageBox.Show($"Ingresá {campo}.", "Cotillón", MessageBoxButton.OK, MessageBoxImage.Warning);

        return false;
    }

    private static void Informar(bool exito, string mensaje) =>
        MessageBox.Show(mensaje, "Cotillón", MessageBoxButton.OK,
            exito ? MessageBoxImage.Information : MessageBoxImage.Warning);
}

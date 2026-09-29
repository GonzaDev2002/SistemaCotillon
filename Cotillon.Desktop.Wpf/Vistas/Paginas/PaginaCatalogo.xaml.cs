using System.Windows;
using System.Windows.Controls;
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

        GrillaSecciones.ItemsSource = respuesta.Data;
    }

    private async void AlSeleccionarSeccion(object sender, SelectionChangedEventArgs e)
    {
        if (GrillaSecciones.SelectedItem is not SeccionDto seccion)
            return;

        _seccionSeleccionada = seccion.SeccionId;
        TxtSeccion.Text = seccion.NombreSeccion;
        ChkSeccionActiva.IsChecked = seccion.Activo;

        LblCategorias.Text = $"Categorías de {seccion.NombreSeccion}";

        var categorias = await App.Api.CategoriasPorSeccionAsync(seccion.SeccionId);
        GrillaCategorias.ItemsSource = categorias.Data;

        LimpiarCategoria();
    }

    private void AlSeleccionarCategoria(object sender, SelectionChangedEventArgs e)
    {
        if (GrillaCategorias.SelectedItem is not CategoriaDto categoria)
            return;

        _categoriaSeleccionada = categoria.CategoriaId;
        TxtCategoria.Text = categoria.NombreCategoria;
        ChkCategoriaActiva.IsChecked = categoria.Activo;
    }

    // ------------------------------------------------------------ Secciones --

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

    private void AlLimpiarSeccion(object sender, RoutedEventArgs e) => LimpiarSeccion();

    private void LimpiarSeccion()
    {
        _seccionSeleccionada = 0;
        TxtSeccion.Clear();
        ChkSeccionActiva.IsChecked = true;
        GrillaSecciones.SelectedItem = null;
    }

    // ----------------------------------------------------------- Categorías --

    private async void AlAgregarCategoria(object sender, RoutedEventArgs e)
    {
        if (_seccionSeleccionada <= 0)
        {
            MessageBox.Show("Primero elegí la sección a la que pertenece.", "Cotillón",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!Validar(TxtCategoria.Text, "el nombre de la categoría"))
            return;

        var respuesta = await App.Api.CrearCategoriaAsync(new CategoriaDto
        {
            SeccionId = _seccionSeleccionada,
            NombreCategoria = TxtCategoria.Text.Trim(),
            Activo = ChkCategoriaActiva.IsChecked == true
        });

        Informar(respuesta.Success, respuesta.Message);

        if (respuesta.Success)
            await RecargarCategoriasAsync();
    }

    private async void AlActualizarCategoria(object sender, RoutedEventArgs e)
    {
        if (_categoriaSeleccionada <= 0)
        {
            MessageBox.Show("Elegí una categoría de la lista.", "Cotillón",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!Validar(TxtCategoria.Text, "el nombre de la categoría"))
            return;

        var respuesta = await App.Api.ActualizarCategoriaAsync(new CategoriaDto
        {
            CategoriaId = _categoriaSeleccionada,
            SeccionId = _seccionSeleccionada,
            NombreCategoria = TxtCategoria.Text.Trim(),
            Activo = ChkCategoriaActiva.IsChecked == true
        });

        Informar(respuesta.Success, respuesta.Message);

        if (respuesta.Success)
            await RecargarCategoriasAsync();
    }

    private void AlLimpiarCategoria(object sender, RoutedEventArgs e) => LimpiarCategoria();

    private async Task RecargarCategoriasAsync()
    {
        LimpiarCategoria();

        var categorias = await App.Api.CategoriasPorSeccionAsync(_seccionSeleccionada);
        GrillaCategorias.ItemsSource = categorias.Data;
    }

    private void LimpiarCategoria()
    {
        _categoriaSeleccionada = 0;
        TxtCategoria.Clear();
        ChkCategoriaActiva.IsChecked = true;
        GrillaCategorias.SelectedItem = null;
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

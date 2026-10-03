using System.Windows;
using System.Windows.Controls;
using Cotillon.Shared.DTOs;

namespace Cotillon.Desktop.Wpf.Vistas;

/// <summary>
/// Alta masiva de productos pegando la lista desde Excel. Cargar doscientos
/// productos de a uno por el formulario es media tarde; acá se pegan todos, se
/// ve antes qué va a entrar y qué no, y se importa de una.
///
/// La sección, la categoría y la unidad se eligen una sola vez para toda la
/// lista: una planilla del proveedor siempre es de un mismo rubro.
/// </summary>
public partial class VentanaImportarProductos : Window
{
    private List<FilaImportada> _filas = [];

    public VentanaImportarProductos()
    {
        InitializeComponent();

        CboUnidad.ItemsSource = UnidadesMedida.Todas;
        CboUnidad.SelectedValue = UnidadesMedida.Unidad;

        Loaded += async (_, _) =>
        {
            await CargarSeccionesAsync();
            TxtLista.Focus();
        };
    }

    /// <summary>Cuántos productos se crearon, para avisarlo en la pantalla que abrió esto.</summary>
    public int Importados { get; private set; }

    private async Task CargarSeccionesAsync()
    {
        var secciones = await App.Api.SeccionesAsync();

        if (!secciones.Success || secciones.Data is null || secciones.Data.Count == 0)
        {
            LblEstado.Text = "Primero creá una sección y una categoría en Catálogo.";
            return;
        }

        CboSeccion.ItemsSource = secciones.Data;
        CboSeccion.SelectedIndex = 0;
    }

    private async void AlCambiarSeccion(object sender, SelectionChangedEventArgs e)
    {
        if (CboSeccion.SelectedValue is not int seccionId)
            return;

        var categorias = await App.Api.CategoriasPorSeccionAsync(seccionId);

        CboCategoria.ItemsSource = categorias.Data;

        if (categorias.Data?.Count > 0)
            CboCategoria.SelectedIndex = 0;

        Revisar();
    }

    private void AlCambiarLista(object sender, TextChangedEventArgs e) => Revisar();

    /// <summary>
    /// Relee la lista en cada tecla y muestra qué va a entrar. Ver los importes ya
    /// formateados antes de importar es lo que evita descubrir después que una
    /// columna se leyó mal en doscientos productos.
    /// </summary>
    private void Revisar()
    {
        _filas = ListaDeProductos.Leer(TxtLista.Text);

        GrillaPrevia.ItemsSource = _filas;

        var validas = _filas.Count(f => f.EsValida);
        var conError = _filas.Count - validas;

        LblResumen.Text = _filas.Count == 0
            ? "VISTA PREVIA"
            : $"VISTA PREVIA — {validas} LISTOS" + (conError > 0 ? $", {conError} CON PROBLEMAS" : string.Empty);

        if (conError > 0)
        {
            LblErrores.Text = string.Join("\n", _filas
                .Where(f => !f.EsValida)
                .Take(5)
                .Select(f => $"Renglón {f.Linea}: {f.Error}"))
                + (conError > 5 ? $"\n…y {conError - 5} más." : string.Empty);

            LblErrores.Visibility = Visibility.Visible;
        }
        else
        {
            LblErrores.Visibility = Visibility.Collapsed;
        }

        BtnImportar.Content = validas > 0 ? $"Importar {validas} productos" : "Importar";
        BtnImportar.IsEnabled = validas > 0 && CboCategoria.SelectedValue is int;
    }

    private async void AlImportar(object sender, RoutedEventArgs e)
    {
        if (CboCategoria.SelectedValue is not int categoriaId)
            return;

        var aImportar = _filas.Where(f => f.EsValida).ToList();

        var confirmar = MessageBox.Show(
            $"Se van a dar de alta {aImportar.Count} productos en «{CboSeccion.Text} / {CboCategoria.Text}».\n\n" +
            "¿Seguimos?",
            "Cotillón", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirmar != MessageBoxResult.Yes)
            return;

        BtnImportar.IsEnabled = false;
        TxtLista.IsEnabled = false;

        var unidad = CboUnidad.SelectedValue as string ?? UnidadesMedida.Unidad;
        var rechazados = new List<string>();

        foreach (var fila in aImportar)
        {
            LblEstado.Text = $"Importando {Importados + rechazados.Count + 1} de {aImportar.Count}…";

            var respuesta = await App.Api.CrearProductoAsync(new ProductoDto
            {
                CategoriaId = categoriaId,
                NombreProducto = fila.Nombre,
                CodigoBarras = fila.Codigo,
                Precio = fila.Precio,
                Costo = fila.Costo,
                Stock = fila.Stock,
                UnidadMedida = unidad
            });

            if (respuesta.Success)
                Importados++;
            else
                rechazados.Add($"Renglón {fila.Linea} ({fila.Nombre}): {respuesta.Message}");
        }

        TxtLista.IsEnabled = true;

        if (rechazados.Count == 0)
        {
            MessageBox.Show($"Se cargaron {Importados} productos.", "Cotillón",
                MessageBoxButton.OK, MessageBoxImage.Information);

            DialogResult = true;
            return;
        }

        // Los que entraron, entraron. Se deja la ventana abierta con el detalle de
        // los que no, para corregirlos sin volver a pegar toda la lista.
        MessageBox.Show(
            $"Se cargaron {Importados} productos.\n\nQuedaron {rechazados.Count} sin cargar:\n\n" +
            string.Join("\n", rechazados.Take(10)) +
            (rechazados.Count > 10 ? $"\n…y {rechazados.Count - 10} más." : string.Empty),
            "Cotillón", MessageBoxButton.OK, MessageBoxImage.Warning);

        LblEstado.Text = $"{Importados} cargados. Corregí los que quedaron y volvé a importar.";
        BtnImportar.IsEnabled = true;
    }

    private void AlCancelar(object sender, RoutedEventArgs e)
    {
        // Si ya se importó algo antes de cancelar, la pantalla de atrás tiene que
        // refrescarse igual: esos productos existen.
        DialogResult = Importados > 0;
    }
}

using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Cotillon.Desktop.Wpf.Servicios;
using Cotillon.Shared.DTOs;

namespace Cotillon.Desktop.Wpf.Vistas;

/// <summary>
/// Aumento de precios en masa.
///
/// Lo importante de esta pantalla no es el porcentaje: es la lista. Se recalcula
/// con cada tecla contra el servidor, así lo que se ve es exactamente lo que se
/// va a guardar, y lo que quedaría por debajo del costo sale en rojo antes de
/// que alguien lo venda perdiendo plata.
/// </summary>
public partial class VentanaAumentoPrecios : Window
{
    private const string Todas = "(Todas)";

    private List<CambioPrecioDto> _cambios = [];
    private bool _cargando = true;

    public VentanaAumentoPrecios()
    {
        InitializeComponent();

        Loaded += async (_, _) =>
        {
            await CargarFiltrosAsync();
            _cargando = false;
            TxtPorcentaje.Focus();
            TxtPorcentaje.SelectAll();
        };
    }

    /// <summary>Cuántos productos se cambiaron, para avisarlo en la pantalla de atrás.</summary>
    public int Afectados { get; private set; }

    private async Task CargarFiltrosAsync()
    {
        var secciones = await App.Api.SeccionesAsync();

        var lista = new List<SeccionDto> { new() { SeccionId = 0, NombreSeccion = Todas } };

        if (secciones.Success && secciones.Data is not null)
            lista.AddRange(secciones.Data);

        CboSeccion.ItemsSource = lista;
        CboSeccion.SelectedIndex = 0;

        CboTemporada.ItemsSource = Temporadas.Todas
            .Select(t => new { t.Codigo, Nombre = t.Codigo.Length == 0 ? Todas : t.Nombre })
            .ToList();

        CboTemporada.SelectedIndex = 0;

        CboCategoria.ItemsSource = new[] { new CategoriaDto { CategoriaId = 0, NombreCategoria = Todas } };
        CboCategoria.SelectedIndex = 0;
    }

    private async void AlCambiarFiltro(object sender, RoutedEventArgs e)
    {
        if (_cargando)
            return;

        // Cambiar de sección recarga sus categorías: filtrar por una categoría de
        // otra sección no devolvería nada y parecería un error.
        if (ReferenceEquals(sender, CboSeccion))
        {
            var categorias = new List<CategoriaDto> { new() { CategoriaId = 0, NombreCategoria = Todas } };

            if (CboSeccion.SelectedValue is int seccionId && seccionId > 0)
            {
                var respuesta = await App.Api.CategoriasPorSeccionAsync(seccionId);

                if (respuesta.Success && respuesta.Data is not null)
                    categorias.AddRange(respuesta.Data);
            }

            CboCategoria.ItemsSource = categorias;
            CboCategoria.SelectedIndex = 0;
        }

        await SimularAsync();
    }

    private async void AlCambiarFiltroSimple(object sender, SelectionChangedEventArgs e)
    {
        if (!_cargando)
            await SimularAsync();
    }

    private async void AlCambiarTexto(object sender, TextChangedEventArgs e)
    {
        if (!_cargando)
            await SimularAsync();
    }

    private AumentoPreciosDto Armar() => new()
    {
        UsuarioId = Sesion.UsuarioId,
        SeccionId = CboSeccion.SelectedValue as int? is > 0 ? (int)CboSeccion.SelectedValue : null,
        CategoriaId = CboCategoria.SelectedValue as int? is > 0 ? (int)CboCategoria.SelectedValue : null,
        Temporada = CboTemporada.SelectedValue as string,
        Marca = TxtMarca.Text.Trim(),
        Porcentaje = Numero(TxtPorcentaje.Text),
        AjustarCosto = ChkAjustarCosto.IsChecked == true,
        RedondearA = int.TryParse((CboRedondeo.SelectedItem as ComboBoxItem)?.Tag?.ToString(), out var r) ? r : 0
    };

    private async Task SimularAsync()
    {
        var aumento = Armar();

        if (aumento.Porcentaje == 0)
        {
            _cambios = [];
            Grilla.ItemsSource = null;
            LblEstado.Text = "Poné el porcentaje para ver cómo quedan los precios.";
            BtnAplicar.IsEnabled = false;
            return;
        }

        var respuesta = await App.Api.SimularAumentoAsync(aumento);

        if (!respuesta.Success)
        {
            _cambios = [];
            Grilla.ItemsSource = null;
            LblEstado.Text = respuesta.Message;
            BtnAplicar.IsEnabled = false;
            return;
        }

        _cambios = respuesta.Data ?? [];
        Grilla.ItemsSource = _cambios;

        var enPerdida = _cambios.Count(c => c.QuedaEnPerdida);

        LblEstado.Text = _cambios.Count == 0
            ? "Ningún producto coincide con el filtro."
            : $"{_cambios.Count} producto(s)" +
              (enPerdida > 0 ? $" · {enPerdida} quedaría(n) por debajo del costo" : string.Empty);

        BtnAplicar.Content = _cambios.Count > 0 ? $"Aplicar a {_cambios.Count}" : "Aplicar";
        BtnAplicar.IsEnabled = _cambios.Count > 0;
    }

    private async void AlAplicar(object sender, RoutedEventArgs e)
    {
        var aumento = Armar();
        var enPerdida = _cambios.Count(c => c.QuedaEnPerdida);

        var confirmar = MessageBox.Show(
            $"Se van a cambiar los precios de {_cambios.Count} producto(s) un {aumento.Porcentaje:0.##} %.\n\n" +
            (enPerdida > 0
                ? $"OJO: {enPerdida} quedaría(n) con el precio por debajo del costo.\n\n"
                : string.Empty) +
            "Esto no se deshace con un botón. ¿Seguimos?",
            "Cotillón", MessageBoxButton.YesNo,
            enPerdida > 0 ? MessageBoxImage.Warning : MessageBoxImage.Question);

        if (confirmar != MessageBoxResult.Yes)
            return;

        BtnAplicar.IsEnabled = false;

        try
        {
            var respuesta = await App.Api.AplicarAumentoAsync(aumento);

            if (!respuesta.Success)
            {
                MessageBox.Show(respuesta.Message, "Cotillón", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Afectados = respuesta.Data?.Afectados ?? 0;

            MessageBox.Show($"Se actualizaron {Afectados} producto(s).", "Cotillón",
                MessageBoxButton.OK, MessageBoxImage.Information);

            DialogResult = true;
        }
        finally
        {
            BtnAplicar.IsEnabled = true;
        }
    }

    private void AlCancelar(object sender, RoutedEventArgs e) => DialogResult = Afectados > 0;

    private static decimal Numero(string? texto) =>
        decimal.TryParse(texto?.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out var valor)
            ? valor
            : 0m;
}

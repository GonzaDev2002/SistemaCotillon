using System.Globalization;
using Cotillon.Shared.DTOs;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace Cotillon.Desktop.Wpf.Vistas.Paginas;

public partial class PaginaReportes : UserControl
{
    private const string TipoTemporada = "Temporada contra el año pasado";
    private const string TipoControl = "Control por usuario";

    private string _titulo = "Reporte";

    public PaginaReportes()
    {
        InitializeComponent();

        FechaDesde.SelectedDate = DateTime.Today.AddDays(-30);
        FechaHasta.SelectedDate = DateTime.Today;

        CboTemporada.ItemsSource = Temporadas.Todas
            .Select(t => new { t.Codigo, Nombre = t.Codigo.Length == 0 ? "(Todas)" : t.Nombre })
            .ToList();

        CboTemporada.SelectedIndex = 0;
    }

    private void AlCambiarTipo(object sender, SelectionChangedEventArgs e)
    {
        if (PanelFechas is null)
            return;

        var tipo = TipoSeleccionado();

        // La temporada tambien necesita el rango: la comparacion es contra la
        // misma franja de dias del ano anterior.
        PanelFechas.Visibility = tipo is "Ventas por rango" or TipoTemporada or TipoControl
            ? Visibility.Visible
            : Visibility.Collapsed;

        CboTemporada.Visibility = tipo == TipoTemporada ? Visibility.Visible : Visibility.Collapsed;
    }

    private string TipoSeleccionado() =>
        (CboTipo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Productos";

    private async void AlGenerar(object sender, RoutedEventArgs e)
    {
        var tipo = TipoSeleccionado();

        switch (tipo)
        {
            case "Productos bajo stock":
            {
                var respuesta = await App.Api.ReporteBajoStockAsync();
                Grilla.ItemsSource = respuesta.Data;
                _titulo = "Productos bajo stock";
                LblResultado.Text = $"{respuesta.Data?.Count ?? 0} producto(s) por debajo del stock mínimo";
                break;
            }

            case TipoControl:
            {
                var respuesta = await App.Api.ControlPorUsuarioAsync(
                    FechaDesde.SelectedDate ?? DateTime.Today, FechaHasta.SelectedDate ?? DateTime.Today);

                Grilla.ItemsSource = respuesta.Data;
                _titulo = "Control por usuario";

                var faltante = respuesta.Data?.Sum(c => c.FaltanteAcumulado) ?? 0;
                var cancelado = respuesta.Data?.Sum(c => c.ImporteCancelado) ?? 0;

                LblResultado.Text =
                    $"Faltantes de caja: {faltante.ToString("C2", CultureInfo.CurrentCulture)} · " +
                    $"ventas canceladas antes de cobrar: {cancelado.ToString("C2", CultureInfo.CurrentCulture)}. " +
                    "Un faltante aislado es un error de vuelto; el mismo faltante todos los días, no.";
                break;
            }

            case TipoTemporada:
            {
                var desde = FechaDesde.SelectedDate ?? DateTime.Today;
                var hasta = FechaHasta.SelectedDate ?? DateTime.Today;

                var respuesta = await App.Api.ReporteTemporadaAsync(
                    CboTemporada.SelectedValue as string, desde, hasta);

                Grilla.ItemsSource = respuesta.Data;
                _titulo = "Temporada";

                var esteAno = respuesta.Data?.Sum(t => t.ImporteEsteAno) ?? 0;
                var anoPasado = respuesta.Data?.Sum(t => t.ImporteAnoPasado) ?? 0;

                LblResultado.Text = anoPasado > 0
                    ? $"{esteAno.ToString("C2", CultureInfo.CurrentCulture)} contra " +
                      $"{anoPasado.ToString("C2", CultureInfo.CurrentCulture)} del año pasado " +
                      $"({(esteAno - anoPasado) / anoPasado * 100:+0.#;-0.#;0} %)"
                    : $"{esteAno.ToString("C2", CultureInfo.CurrentCulture)}. " +
                      "El año pasado no hay ventas en esta franja para comparar.";
                break;
            }

            case "Ventas por rango":
            {
                var desde = FechaDesde.SelectedDate ?? DateTime.Today;
                var hasta = FechaHasta.SelectedDate ?? DateTime.Today;

                if (desde > hasta)
                {
                    MessageBox.Show("La fecha desde no puede ser mayor a la fecha hasta.", "Cotillón",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var respuesta = await App.Api.ReporteVentasAsync(desde, hasta);
                Grilla.ItemsSource = respuesta.Data;
                _titulo = "Ventas por rango";

                var total = respuesta.Data?.Where(v => v.Estado == "COMPLETADA").Sum(v => v.TotalVenta) ?? 0;

                LblResultado.Text =
                    $"{respuesta.Data?.Count ?? 0} venta(s) · Total: {total.ToString("C2", CultureInfo.CurrentCulture)}";
                break;
            }

            default:
            {
                var respuesta = await App.Api.ReporteProductosAsync();
                Grilla.ItemsSource = respuesta.Data;
                _titulo = "Productos";
                LblResultado.Text = $"{respuesta.Data?.Count ?? 0} producto(s)";
                break;
            }
        }
    }

    private void AlExportar(object sender, RoutedEventArgs e)
    {
        if (Grilla.Items.Count == 0)
        {
            MessageBox.Show("Generá un reporte antes de exportar.", "Cotillón",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialogo = new SaveFileDialog
        {
            Title = "Exportar reporte",
            Filter = "Archivo CSV (*.csv)|*.csv",
            FileName = $"{_titulo.Replace(' ', '_')}_{DateTime.Now:yyyyMMdd}.csv"
        };

        if (dialogo.ShowDialog() != true)
            return;

        var texto = new StringBuilder();
        var columnas = Grilla.Columns.ToList();

        texto.AppendLine(string.Join(";", columnas.Select(c => c.Header)));

        foreach (var fila in Grilla.Items)
        {
            var valores = columnas.Select(columna =>
            {
                var contenido = columna.GetCellContent(fila);

                return contenido is TextBlock bloque ? Escapar(bloque.Text) : string.Empty;
            });

            texto.AppendLine(string.Join(";", valores));
        }

        // UTF-8 con BOM para que Excel respete los acentos al abrir el archivo.
        File.WriteAllText(dialogo.FileName, texto.ToString(), new UTF8Encoding(true));

        MessageBox.Show("Reporte exportado con éxito.", "Cotillón",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private static string Escapar(string valor) =>
        valor.Contains(';') ? $"\"{valor}\"" : valor;
}

using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace Cotillon.Desktop.Wpf.Vistas.Paginas;

public partial class PaginaReportes : UserControl
{
    private string _titulo = "Reporte";

    public PaginaReportes()
    {
        InitializeComponent();

        FechaDesde.SelectedDate = DateTime.Today.AddDays(-30);
        FechaHasta.SelectedDate = DateTime.Today;
    }

    private void AlCambiarTipo(object sender, SelectionChangedEventArgs e)
    {
        if (PanelFechas is null)
            return;

        PanelFechas.Visibility = TipoSeleccionado() == "Ventas por rango"
            ? Visibility.Visible
            : Visibility.Collapsed;
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

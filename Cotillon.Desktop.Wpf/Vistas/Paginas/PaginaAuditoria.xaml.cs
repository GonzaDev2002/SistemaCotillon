using System.Windows;
using System.Windows.Controls;

namespace Cotillon.Desktop.Wpf.Vistas.Paginas;

public partial class PaginaAuditoria : UserControl
{
    public PaginaAuditoria()
    {
        InitializeComponent();

        FechaDesde.SelectedDate = DateTime.Today;
        FechaHasta.SelectedDate = DateTime.Today;

        Loaded += async (_, _) => await BuscarAsync();
    }

    private async void AlBuscar(object sender, RoutedEventArgs e) => await BuscarAsync();

    private async Task BuscarAsync()
    {
        var respuesta = await App.Api.AuditoriaAsync(
            FechaDesde.SelectedDate ?? DateTime.Today,
            FechaHasta.SelectedDate ?? DateTime.Today);

        Grilla.ItemsSource = respuesta.Data;
        LblCantidad.Text = $"{respuesta.Data?.Count ?? 0} evento(s)";
    }
}

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Cotillon.Desktop.Wpf.Servicios;
using Cotillon.Shared.DTOs.Compras;

namespace Cotillon.Desktop.Wpf.Vistas.Paginas;

public partial class PaginaCompras : UserControl
{
    private readonly ObservableCollection<ItemCompra> _items = [];
    private CompraResumenDto? _compraSeleccionada;

    public PaginaCompras()
    {
        InitializeComponent();

        GrillaItems.ItemsSource = _items;
        _items.CollectionChanged += (_, _) => RecalcularTotal();

        FechaDesde.SelectedDate = DateTime.Today.AddDays(-30);
        FechaHasta.SelectedDate = DateTime.Today;

        Loaded += async (_, _) =>
        {
            await CargarProveedoresAsync();
            await BuscarComprasAsync();
        };
    }

    private async Task CargarProveedoresAsync()
    {
        var respuesta = await App.Api.ProveedoresAsync(null, false);

        CboProveedor.ItemsSource = respuesta.Data;

        if (respuesta.Data is { Count: > 0 })
            CboProveedor.SelectedIndex = 0;
    }

    // ----------------------------------------------------------- Nueva compra --

    private async void AlBuscarProducto(object sender, RoutedEventArgs e) => await BuscarProductoAsync();

    private async void AlTeclaProducto(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await BuscarProductoAsync();
    }

    private async Task BuscarProductoAsync()
    {
        var texto = TxtBuscarProducto.Text.Trim();

        if (string.IsNullOrWhiteSpace(texto))
            return;

        var respuesta = await App.Api.BuscarProductosAsync(texto);

        if (!respuesta.Success || respuesta.Data is null || respuesta.Data.Count == 0)
        {
            MessageBox.Show("No se encontró ningún producto.", "Cotillón",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var producto = respuesta.Data[0];
        var existente = _items.FirstOrDefault(i => i.ProductoId == producto.ProductoId);

        if (existente is not null)
        {
            existente.Cantidad += 1;
            GrillaItems.Items.Refresh();
            RecalcularTotal();
        }
        else
        {
            var item = new ItemCompra
            {
                ProductoId = producto.ProductoId,
                Nombre = producto.NombreProducto,
                Cantidad = 1,
                Costo = producto.Precio
            };

            item.PropertyChanged += (_, _) => RecalcularTotal();
            _items.Add(item);
        }

        TxtBuscarProducto.Clear();
        TxtBuscarProducto.Focus();
    }

    private void AlQuitarItem(object sender, RoutedEventArgs e)
    {
        if (GrillaItems.SelectedItem is ItemCompra item)
            _items.Remove(item);
    }

    private void RecalcularTotal()
    {
        if (LblTotalCompra is null)
            return;

        GrillaItems.Items.Refresh();

        LblTotalCompra.Text = _items.Sum(i => i.Subtotal).ToString("C2", CultureInfo.CurrentCulture);
    }

    private async void AlRegistrar(object sender, RoutedEventArgs e)
    {
        if (CboProveedor.SelectedValue is not int proveedorId)
        {
            MessageBox.Show("Elegí un proveedor. Si no hay ninguno, cargalo primero en Proveedores.",
                "Cotillón", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_items.Count == 0)
        {
            MessageBox.Show("Agregá al menos un producto.", "Cotillón",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var compra = new CrearCompraDto
        {
            UsuarioId = Sesion.UsuarioId,
            ProveedorId = proveedorId,
            NumeroComprobante = TxtComprobante.Text.Trim(),
            RecibirAhora = ChkRecibir.IsChecked == true,
            ActualizarCostos = ChkActualizarCostos.IsChecked == true,
            Lineas = [.. _items.Select(i => new CrearLineaCompraDto
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad,
                CostoUnitario = i.Costo
            })]
        };

        BtnRegistrar.IsEnabled = false;

        try
        {
            var respuesta = await App.Api.CrearCompraAsync(compra);

            MessageBox.Show(respuesta.Message, "Cotillón", MessageBoxButton.OK,
                respuesta.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (respuesta.Success)
            {
                _items.Clear();
                TxtComprobante.Clear();
                await BuscarComprasAsync();
            }
        }
        finally
        {
            BtnRegistrar.IsEnabled = true;
        }
    }

    // -------------------------------------------------------------- Historial --

    private async void AlBuscarCompras(object sender, RoutedEventArgs e) => await BuscarComprasAsync();

    private async Task BuscarComprasAsync()
    {
        var respuesta = await App.Api.ComprasAsync(
            FechaDesde.SelectedDate ?? DateTime.Today.AddDays(-30),
            FechaHasta.SelectedDate ?? DateTime.Today);

        GrillaCompras.ItemsSource = respuesta.Data;
    }

    private async void AlSeleccionarCompra(object sender, SelectionChangedEventArgs e)
    {
        if (GrillaCompras.SelectedItem is not CompraResumenDto resumen)
            return;

        _compraSeleccionada = resumen;

        var respuesta = await App.Api.CompraAsync(resumen.CompraId);

        if (!respuesta.Success || respuesta.Data is null)
            return;

        LblDetalleCompra.Text = $"Compra #{respuesta.Data.CompraId} · {respuesta.Data.Estado}";
        GrillaDetalleCompra.ItemsSource = respuesta.Data.Lineas;

        BtnRecibir.IsEnabled = respuesta.Data.Estado == "PENDIENTE";
        BtnAnularCompra.IsEnabled = respuesta.Data.Estado != "ANULADA";
    }

    private async void AlRecibir(object sender, RoutedEventArgs e)
    {
        if (_compraSeleccionada is null)
            return;

        var respuesta = await App.Api.RecibirCompraAsync(_compraSeleccionada.CompraId, new RecibirCompraDto
        {
            UsuarioId = Sesion.UsuarioId,
            ActualizarCostos = true
        });

        MessageBox.Show(respuesta.Message, "Cotillón", MessageBoxButton.OK,
            respuesta.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);

        if (respuesta.Success)
            await BuscarComprasAsync();
    }

    private async void AlAnularCompra(object sender, RoutedEventArgs e)
    {
        if (_compraSeleccionada is null)
            return;

        var confirmar = MessageBox.Show(
            $"¿Anular la compra #{_compraSeleccionada.CompraId}?\n\n" +
            "Si ya había ingresado stock, se descuenta.",
            "Cotillón", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirmar != MessageBoxResult.Yes)
            return;

        var respuesta = await App.Api.AnularCompraAsync(_compraSeleccionada.CompraId, "Anulada desde el sistema");

        MessageBox.Show(respuesta.Message, "Cotillón", MessageBoxButton.OK,
            respuesta.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);

        if (respuesta.Success)
            await BuscarComprasAsync();
    }

    private sealed class ItemCompra : INotifyPropertyChanged
    {
        private decimal _cantidad = 1;
        private decimal _costo;

        public int ProductoId { get; init; }
        public string Nombre { get; init; } = string.Empty;

        public decimal Cantidad
        {
            get => _cantidad;
            set { _cantidad = value; Notificar(); }
        }

        public decimal Costo
        {
            get => _costo;
            set { _costo = value; Notificar(); }
        }

        public decimal Subtotal => Cantidad * Costo;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void Notificar()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }
    }
}

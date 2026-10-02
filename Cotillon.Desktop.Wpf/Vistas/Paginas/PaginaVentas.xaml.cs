using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Cotillon.Desktop.Wpf.Servicios;
using Cotillon.Shared.DTOs;
using Cotillon.Shared.DTOs.Permisos;
using Cotillon.Shared.DTOs.Terceros;
using Cotillon.Shared.DTOs.Usuarios;
using Cotillon.Shared.DTOs.Ventas;

namespace Cotillon.Desktop.Wpf.Vistas.Paginas;

public partial class PaginaVentas : UserControl
{
    private readonly VentanaPrincipal _principal;
    private readonly ObservableCollection<ItemCarrito> _carrito = [];
    private readonly bool _puedeCobrar;

    public PaginaVentas(VentanaPrincipal principal)
    {
        InitializeComponent();

        _principal = principal;
        GrillaCarrito.ItemsSource = _carrito;
        _carrito.CollectionChanged += (_, _) => Recalcular();

        _puedeCobrar = Sesion.PuedeEditar(Modulos.Ventas);

        BtnCobrar.IsEnabled = _puedeCobrar;
        PanelDescuento.Visibility = Sesion.PuedeTodo(Modulos.Ventas)
            ? Visibility.Visible
            : Visibility.Collapsed;

        Loaded += async (_, _) =>
        {
            await CargarCombosAsync();
            TxtBuscar.Focus();
        };

        // F12 cobra desde cualquier parte de la pantalla, como en un POS real.
        PreviewKeyDown += (_, e) =>
        {
            if (e.Key == Key.F12)
            {
                AlCobrar(this, new RoutedEventArgs());
                e.Handled = true;
            }
        };
    }

    private async Task CargarCombosAsync()
    {
        var medios = await App.Api.MediosPagoAsync();

        if (medios.Success && medios.Data is not null)
        {
            CboMedioPago.ItemsSource = medios.Data;
            CboMedioPago.SelectedIndex = 0;
        }

        var clientes = await App.Api.ClientesAsync(null, false);
        var lista = new List<ClienteDto>
        {
            new() { ClienteId = 0, NombreCompleto = "Consumidor final" }
        };

        if (clientes.Success && clientes.Data is not null)
            lista.AddRange(clientes.Data);

        CboCliente.ItemsSource = lista;
        CboCliente.SelectedIndex = 0;
    }

    // ------------------------------------------------------------- Búsqueda --

    private async void AlBuscar(object sender, RoutedEventArgs e) => await BuscarAsync();

    private async void AlTeclaBuscar(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await BuscarAsync();
    }

    private async Task BuscarAsync()
    {
        var texto = TxtBuscar.Text.Trim();

        if (string.IsNullOrWhiteSpace(texto))
            return;

        var respuesta = await App.Api.BuscarProductosAsync(texto);

        if (!respuesta.Success || respuesta.Data is null || respuesta.Data.Count == 0)
        {
            PanelResultados.Visibility = Visibility.Collapsed;
            MostrarAviso("No se encontró ningún producto con ese código o nombre.");
            return;
        }

        OcultarAviso();

        // Un único resultado exacto se agrega directo: es el caso del lector de
        // código de barras, donde parar a elegir rompería el ritmo de la caja.
        if (respuesta.Data.Count == 1)
        {
            Agregar(respuesta.Data[0]);
            TxtBuscar.Clear();
            PanelResultados.Visibility = Visibility.Collapsed;
            return;
        }

        GrillaResultados.ItemsSource = respuesta.Data;
        PanelResultados.Visibility = Visibility.Visible;
    }

    private void AlElegirResultado(object sender, MouseButtonEventArgs e)
    {
        if (GrillaResultados.SelectedItem is not ProductoListadoDto producto)
            return;

        Agregar(producto);
        TxtBuscar.Clear();
        TxtBuscar.Focus();
        PanelResultados.Visibility = Visibility.Collapsed;
    }

    private void Agregar(ProductoListadoDto producto)
    {
        var existente = _carrito.FirstOrDefault(i => i.ProductoId == producto.ProductoId);

        if (existente is not null)
        {
            existente.Cantidad += 1;
            GrillaCarrito.Items.Refresh();
            Recalcular();
            return;
        }

        var item = new ItemCarrito
        {
            ProductoId = producto.ProductoId,
            Nombre = producto.NombreProducto,
            Precio = producto.Precio,
            Cantidad = 1
        };

        item.PropertyChanged += (_, _) => Recalcular();

        _carrito.Add(item);
    }

    private void AlQuitarItem(object sender, RoutedEventArgs e)
    {
        if (GrillaCarrito.SelectedItem is ItemCarrito item)
            _carrito.Remove(item);
    }

    private void AlEditarCantidad(object sender, DataGridCellEditEndingEventArgs e)
    {
        // La grilla escribe el valor después de este evento, por eso se recalcula
        // en el siguiente ciclo del dispatcher.
        Dispatcher.BeginInvoke(new Action(() =>
        {
            foreach (var item in _carrito.Where(i => i.Cantidad <= 0).ToList())
                item.Cantidad = 1;

            GrillaCarrito.Items.Refresh();
            Recalcular();
        }));
    }

    // -------------------------------------------------------------- Totales --

    private void AlCambiarDescuento(object sender, TextChangedEventArgs e) => Recalcular();

    private void AlCambiarPago(object sender, TextChangedEventArgs e) => Recalcular();

    private void AlTeclaPago(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            AlCobrar(this, new RoutedEventArgs());
    }

    private decimal Subtotal => _carrito.Sum(i => i.Precio * i.Cantidad);

    private decimal DescuentoLineas => _carrito.Sum(i => i.Descuento);

    private decimal DescuentoGeneral => Numero(TxtDescuentoGeneral?.Text);

    private decimal Total => Math.Max(0, Subtotal - DescuentoLineas - DescuentoGeneral);

    private void Recalcular()
    {
        // Al asignar Text="0" en el XAML, TextChanged se dispara en medio de
        // InitializeComponent, cuando los controles de más abajo todavía no existen.
        if (LblSubtotal is null || PanelVuelto is null)
            return;

        LblSubtotal.Text = Subtotal.ToString("C2", CultureInfo.CurrentCulture);
        LblDescuentos.Text = (DescuentoLineas + DescuentoGeneral).ToString("C2", CultureInfo.CurrentCulture);
        LblTotal.Text = Total.ToString("C2", CultureInfo.CurrentCulture);

        var paga = Numero(TxtPagaCon?.Text);
        var vuelto = paga - Total;

        if (paga > 0 && vuelto > 0)
        {
            LblVuelto.Text = vuelto.ToString("C2", CultureInfo.CurrentCulture);
            PanelVuelto.Visibility = Visibility.Visible;
        }
        else
        {
            PanelVuelto.Visibility = Visibility.Collapsed;
        }
    }

    private static decimal Numero(string? texto) =>
        decimal.TryParse(texto?.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out var valor)
            ? valor
            : 0m;

    // ---------------------------------------------------------------- Cobro --

    private async void AlCobrar(object sender, RoutedEventArgs e)
    {
        if (!_puedeCobrar)
            return;

        if (_carrito.Count == 0)
        {
            MostrarAviso("Agregá al menos un producto antes de cobrar.");
            return;
        }

        if (CboMedioPago.SelectedValue is not int medioPagoId)
        {
            MostrarAviso("Elegí un medio de pago.");
            return;
        }

        var paga = Numero(TxtPagaCon.Text);

        // Con tarjeta o transferencia el importe entregado es exactamente el total.
        if (paga <= 0)
            paga = Total;

        if (paga < Total)
        {
            MostrarAviso($"El pago es insuficiente. Faltan {(Total - paga):C2}.");
            return;
        }

        var venta = new CrearVentaDto
        {
            UsuarioId = Sesion.UsuarioId,
            ClienteId = CboCliente.SelectedValue is int clienteId && clienteId > 0 ? clienteId : null,
            TipoComprobante = "TICKET",
            DescuentoTotal = DescuentoGeneral,
            Lineas = [.. _carrito.Select(i => new CrearLineaVentaDto
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad,
                DescuentoLinea = i.Descuento
            })],
            Pagos =
            [
                new CrearPagoVentaDto { MedioPagoId = medioPagoId, Importe = paga }
            ]
        };

        BtnCobrar.IsEnabled = false;
        BtnCobrar.Content = "Cobrando...";

        try
        {
            var respuesta = await App.Api.CrearVentaAsync(venta);

            if (!respuesta.Success || respuesta.Data is null)
            {
                MostrarAviso(respuesta.Message);
                return;
            }

            var registrada = respuesta.Data;

            MessageBox.Show(
                $"Venta #{registrada.VentaId} registrada.\n\n" +
                $"Total: {registrada.TotalVenta:C2}\n" +
                $"Vuelto: {registrada.Vuelto:C2}",
                "Venta completada", MessageBoxButton.OK, MessageBoxImage.Information);

            Limpiar();
            await _principal.RefrescarEstadoCajaAsync();
        }
        finally
        {
            BtnCobrar.IsEnabled = true;
            BtnCobrar.Content = "Cobrar   (F12)";
        }
    }

    private void AlCancelar(object sender, RoutedEventArgs e)
    {
        if (_carrito.Count == 0)
            return;

        var confirmar = MessageBox.Show(
            "¿Querés cancelar la venta en curso?", "Cotillón",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirmar == MessageBoxResult.Yes)
            Limpiar();
    }

    private void Limpiar()
    {
        _carrito.Clear();
        TxtBuscar.Clear();
        TxtPagaCon.Clear();
        TxtDescuentoGeneral.Text = "0";
        CboCliente.SelectedIndex = 0;
        PanelResultados.Visibility = Visibility.Collapsed;
        OcultarAviso();
        Recalcular();
        TxtBuscar.Focus();
    }

    private void MostrarAviso(string mensaje)
    {
        LblAviso.Text = mensaje;
        LblAviso.Visibility = Visibility.Visible;
    }

    private void OcultarAviso() => LblAviso.Visibility = Visibility.Collapsed;

    private sealed class ItemCarrito : INotifyPropertyChanged
    {
        private decimal _cantidad = 1;
        private decimal _descuento;

        public int ProductoId { get; init; }
        public string Nombre { get; init; } = string.Empty;
        public decimal Precio { get; init; }

        public decimal Cantidad
        {
            get => _cantidad;
            set
            {
                _cantidad = value;
                Notificar(nameof(Cantidad));
                Notificar(nameof(Subtotal));
            }
        }

        public decimal Descuento
        {
            get => _descuento;
            set
            {
                _descuento = value;
                Notificar(nameof(Descuento));
                Notificar(nameof(Subtotal));
            }
        }

        public decimal Subtotal => (Precio * Cantidad) - Descuento;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void Notificar(string propiedad) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propiedad));
    }
}

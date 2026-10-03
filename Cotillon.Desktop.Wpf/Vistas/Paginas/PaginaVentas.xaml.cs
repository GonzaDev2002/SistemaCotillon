using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
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

    /// <summary>Multiplicador tipeado antes de buscar, mientras se elige de la lista.</summary>
    private decimal? _cantidadPendiente;

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

        // Los atajos se escuchan en toda la pantalla, como en un POS real: el
        // vendedor no tiene que estar parado en el control correcto para usarlos.
        PreviewKeyDown += AlTeclaGlobal;
    }

    /// <summary>
    /// Atajos de la venta. Están pensados para cobrar sin soltar el teclado: la
    /// mano no se va del lector ni del teclado numérico entre cliente y cliente.
    /// </summary>
    private void AlTeclaGlobal(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.F12:
                AlCobrar(this, new RoutedEventArgs());
                break;

            case Key.F2:
                EditarCantidadDelSeleccionado();
                break;

            case Key.F3:
                CboCliente.Focus();
                CboCliente.IsDropDownOpen = true;
                break;

            case Key.F4:
                CboMedioPago.Focus();
                CboMedioPago.IsDropDownOpen = true;
                break;

            case Key.F5:
                TxtPagaCon.Focus();
                TxtPagaCon.SelectAll();
                break;

            // Supr sólo borra la línea cuando el foco no está en un campo de texto,
            // para no comerse la tecla mientras se corrige un importe.
            case Key.Delete when Keyboard.FocusedElement is not TextBox:
                AlQuitarItem(this, new RoutedEventArgs());
                break;

            case Key.Escape:
                if (PanelResultados.Visibility == Visibility.Visible)
                {
                    PanelResultados.Visibility = Visibility.Collapsed;
                    TxtBuscar.Focus();
                }
                else
                {
                    AlCancelar(this, new RoutedEventArgs());
                }

                break;

            default:
                return;
        }

        e.Handled = true;
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

    /// <summary>
    /// Separa el multiplicador del código: "3*7791234" o "3*" y después el disparo
    /// del lector. Es el atajo clásico de caja para llevar varios iguales sin
    /// tener que escanear el mismo producto tres veces.
    /// </summary>
    private static readonly Regex Multiplicador =
        new(@"^\s*(\d+(?:[.,]\d+)?)\s*[*xX]\s*(.*)$", RegexOptions.Compiled);

    private async Task BuscarAsync()
    {
        var texto = TxtBuscar.Text.Trim();

        if (string.IsNullOrWhiteSpace(texto))
            return;

        decimal? cantidad = null;

        if (Multiplicador.Match(texto) is { Success: true } m)
        {
            cantidad = Numero(m.Groups[1].Value);
            texto = m.Groups[2].Value.Trim();

            // "3*" solo: queda esperando el disparo del lector, sin ir a buscar.
            if (texto.Length == 0)
            {
                MostrarAviso($"Listo para {cantidad:0.##}: escaneá o escribí el producto.");
                return;
            }
        }

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
            // Se limpia ANTES de agregar. Si el producto abre el cuadro de cantidad,
            // el Enter que lo confirma se filtra al buscador cuando el cuadro se
            // cierra; con el campo ya vacío esa búsqueda de más no encuentra nada
            // en vez de volver a pedir el peso del mismo producto.
            TxtBuscar.Clear();

            Agregar(respuesta.Data[0], cantidad);

            TxtBuscar.Focus();
            PanelResultados.Visibility = Visibility.Collapsed;
            return;
        }

        _cantidadPendiente = cantidad;

        GrillaResultados.ItemsSource = respuesta.Data;
        PanelResultados.Visibility = Visibility.Visible;

        // El foco pasa a la lista para elegir con las flechas y Enter.
        GrillaResultados.SelectedIndex = 0;
        GrillaResultados.Focus();
    }

    private void AlElegirResultado(object sender, MouseButtonEventArgs e) => TomarResultado();

    private void AlTeclaResultados(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        TomarResultado();
        e.Handled = true;
    }

    private void TomarResultado()
    {
        if (GrillaResultados.SelectedItem is not ProductoListadoDto producto)
            return;

        // Igual que al escanear: el campo se limpia antes de abrir el cuadro de
        // cantidad, para que el Enter que lo confirma no dispare otra búsqueda.
        TxtBuscar.Clear();
        PanelResultados.Visibility = Visibility.Collapsed;

        Agregar(producto, _cantidadPendiente);

        _cantidadPendiente = null;

        TxtBuscar.Focus();
    }

    /// <summary>
    /// Agrega el producto al carrito. Si se vende por peso o por metro y no vino
    /// una cantidad explícita, se pregunta: sumar "1" sería un kilo, que casi
    /// nunca es lo que el cliente pidió.
    /// </summary>
    private void Agregar(ProductoListadoDto producto, decimal? cantidad = null)
    {
        var unidad = UnidadesMedida.De(producto.UnidadMedida);

        if (cantidad is null && unidad.Fraccionada)
        {
            cantidad = PreguntarCantidad(
                producto.NombreProducto, producto.UnidadMedida, producto.Precio,
                stock: producto.Stock);

            if (cantidad is null)
            {
                TxtBuscar.Focus();
                return;
            }
        }

        var existente = _carrito.FirstOrDefault(i => i.ProductoId == producto.ProductoId);

        if (existente is not null)
        {
            existente.Cantidad += cantidad ?? 1;
            GrillaCarrito.Items.Refresh();
            Recalcular();
            return;
        }

        var item = new ItemCarrito
        {
            ProductoId = producto.ProductoId,
            Nombre = producto.NombreProducto,
            Precio = producto.Precio,
            UnidadMedida = producto.UnidadMedida,
            Cantidad = cantidad ?? 1
        };

        item.PropertyChanged += (_, _) => Recalcular();

        _carrito.Add(item);
    }

    private decimal? PreguntarCantidad(
        string producto, string unidad, decimal precio, decimal? actual = null, decimal? stock = null)
    {
        var ventana = new VentanaCantidad(producto, unidad, precio, actual, stock)
        {
            Owner = Window.GetWindow(this)
        };

        return ventana.ShowDialog() == true ? ventana.Cantidad : null;
    }

    /// <summary>
    /// F2 sobre la línea elegida: corrige el peso mal tipeado o cambia la cantidad
    /// sin tener que borrar la línea y volver a escanear.
    /// </summary>
    private void EditarCantidadDelSeleccionado()
    {
        if (GrillaCarrito.SelectedItem is not ItemCarrito item)
        {
            MostrarAviso("Elegí primero una línea del detalle.");
            return;
        }

        var cantidad = PreguntarCantidad(item.Nombre, item.UnidadMedida, item.Precio, item.Cantidad);

        if (cantidad is null)
            return;

        item.Cantidad = cantidad.Value;

        GrillaCarrito.Items.Refresh();
        Recalcular();
        OcultarAviso();
    }

    private void AlQuitarItem(object sender, RoutedEventArgs e)
    {
        if (GrillaCarrito.SelectedItem is ItemCarrito item)
            _carrito.Remove(item);
    }

    /// <summary>Enter sobre una línea abre el mismo cuadro que F2.</summary>
    private void AlTeclaCarrito(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || GrillaCarrito.CurrentColumn?.IsReadOnly == false)
            return;

        EditarCantidadDelSeleccionado();
        e.Handled = true;
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
        _cantidadPendiente = null;
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
        public string UnidadMedida { get; init; } = UnidadesMedida.Unidad;

        /// <summary>"$ 4.500,00 / kg": sin esto el precio del kilo parece el del paquete.</summary>
        public string PrecioConUnidad =>
            $"{Precio.ToString("C2", CultureInfo.CurrentCulture)} / {UnidadesMedida.De(UnidadMedida).Simbolo}";

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

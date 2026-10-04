using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Cotillon.Desktop.Wpf.Servicios;
using Cotillon.Shared.DTOs;
using Cotillon.Shared.DTOs.Encargues;
using Cotillon.Shared.DTOs.Permisos;
using Cotillon.Shared.DTOs.Terceros;

namespace Cotillon.Desktop.Wpf.Vistas.Paginas;

/// <summary>
/// La agenda de encargues: qué hay que tener listo y para cuándo.
///
/// Se ordena por fecha de entrega y no por fecha de pedido, porque lo que el
/// negocio necesita saber a la mañana es qué se entrega hoy, no qué se pidió
/// primero. Lo atrasado queda en rojo.
/// </summary>
public partial class PaginaEncargues : UserControl
{
    private readonly ObservableCollection<LineaEncargueDto> _lineas = [];
    private readonly bool _puedeEditar = Sesion.PuedeEditar(Modulos.Encargues);
    private readonly bool _puedeCancelar = Sesion.PuedeTodo(Modulos.Encargues);

    private EncargueDto? _elegido;

    public PaginaEncargues()
    {
        InitializeComponent();

        GrillaLineas.ItemsSource = _lineas;
        _lineas.CollectionChanged += (_, _) => Recalcular();

        FechaEntrega.SelectedDate = DateTime.Today.AddDays(7);

        PanelAlta.Visibility = _puedeEditar ? Visibility.Visible : Visibility.Collapsed;
        BtnCancelarEncargue.Visibility = _puedeCancelar ? Visibility.Visible : Visibility.Collapsed;

        Loaded += async (_, _) =>
        {
            await CargarCombosAsync();
            await BuscarAsync();
        };

        // La agenda es pantalla de mirar: si otro puesto toma un pedido, acá
        // aparece solo.
        Refresco.Automatico(this, async () =>
        {
            if (_elegido is null)
                await BuscarAsync();
        });
    }

    private async Task CargarCombosAsync()
    {
        var medios = await App.Api.MediosPagoAsync();

        if (medios.Success && medios.Data is not null)
        {
            CboMedioEntrega.ItemsSource = medios.Data;
            CboMedioEntrega.SelectedIndex = 0;
        }

        var clientes = await App.Api.ClientesAsync(null, false);
        var lista = new List<ClienteDto> { new() { ClienteId = 0, NombreCompleto = "(Sin cliente)" } };

        if (clientes.Success && clientes.Data is not null)
            lista.AddRange(clientes.Data);

        CboCliente.ItemsSource = lista;
        CboCliente.SelectedIndex = 0;
    }

    // -------------------------------------------------------------- Agenda --

    private async void AlBuscar(object sender, RoutedEventArgs e) => await BuscarAsync();

    private async void AlCambiarPlazo(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded)
            await BuscarAsync();
    }

    private async Task BuscarAsync()
    {
        var dias = int.TryParse((CboPlazo.SelectedItem as ComboBoxItem)?.Tag?.ToString(), out var d) ? d : 7;
        DateTime? hasta = dias > 0 ? DateTime.Today.AddDays(dias) : null;

        var respuesta = await App.Api.EncarguesAsync(hasta, ChkCerrados.IsChecked == true);
        var encargues = respuesta.Data ?? [];

        Grilla.ItemsSource = encargues;

        var pendientes = encargues.Where(x => x.Estado == "PENDIENTE").ToList();
        var atrasados = pendientes.Count(x => x.Atrasado);

        LblResumen.Text = pendientes.Count == 0
            ? "No hay encargues pendientes en este plazo."
            : $"{pendientes.Count} pendiente(s) por {pendientes.Sum(x => x.Total):C2}, " +
              $"con {pendientes.Sum(x => x.Saldo):C2} sin cobrar" +
              (atrasados > 0 ? $" · {atrasados} pasado(s) de fecha" : string.Empty);
    }

    private async void AlSeleccionar(object sender, SelectionChangedEventArgs e)
    {
        if (Grilla.SelectedItem is not EncargueDto fila)
            return;

        var respuesta = await App.Api.EncargueAsync(fila.EncargueId);

        if (!respuesta.Success || respuesta.Data is null)
            return;

        _elegido = respuesta.Data;

        LblDetalle.Text =
            $"Encargue #{_elegido.EncargueId} · {_elegido.NombreContacto}\n" +
            $"Entrega el {_elegido.FechaEntrega:dd/MM/yyyy}" +
            (_elegido.Telefono is { Length: > 0 } tel ? $" · {tel}" : string.Empty) +
            (_elegido.Observaciones is { Length: > 0 } obs ? $"\n{obs}" : string.Empty);

        GrillaDetalle.ItemsSource = _elegido.Lineas;

        LblSaldo.Text = _elegido.Estado == "PENDIENTE"
            ? $"Falta cobrar {_elegido.Saldo:C2}   (de {_elegido.Total:C2}, señó {_elegido.Sena:C2})"
            : $"{_elegido.Estado} · {_elegido.Total:C2}";

        // Entregar y cancelar sólo tienen sentido sobre un pedido vigente.
        PanelEntrega.Visibility = _elegido.Estado == "PENDIENTE" && _puedeEditar
            ? Visibility.Visible
            : Visibility.Collapsed;

        PanelDetalle.Visibility = Visibility.Visible;
        PanelAlta.Visibility = Visibility.Collapsed;
        OcultarAviso();
    }

    private void AlNuevo(object sender, RoutedEventArgs e) => Limpiar();

    // ----------------------------------------------------------- Entregar --

    private async void AlEntregar(object sender, RoutedEventArgs e)
    {
        if (_elegido is null || CboMedioEntrega.SelectedValue is not int medioPagoId)
            return;

        var confirmar = MessageBox.Show(
            $"Entregar el encargue #{_elegido.EncargueId} y cobrar {_elegido.Saldo:C2}.\n\n" +
            "Recién ahora se descuenta el stock. ¿Seguimos?",
            "Cotillón", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirmar != MessageBoxResult.Yes)
            return;

        BtnEntregar.IsEnabled = false;

        try
        {
            var respuesta = await App.Api.EntregarEncargueAsync(_elegido.EncargueId, new EntregarEncargueDto
            {
                UsuarioId = Sesion.UsuarioId,
                MedioPagoId = medioPagoId,
                Importe = _elegido.Saldo
            });

            if (!respuesta.Success)
            {
                MostrarAviso(respuesta.Message);
                return;
            }

            MessageBox.Show($"Entregado. Venta #{respuesta.Data} registrada.", "Cotillón",
                MessageBoxButton.OK, MessageBoxImage.Information);

            Limpiar();
            await BuscarAsync();
        }
        finally
        {
            BtnEntregar.IsEnabled = true;
        }
    }

    private async void AlCancelarEncargue(object sender, RoutedEventArgs e)
    {
        if (_elegido is null || !_puedeCancelar)
            return;

        var confirmar = MessageBox.Show(
            $"¿Cancelar el encargue #{_elegido.EncargueId}?\n\n" +
            $"La seña de {_elegido.Sena:C2} NO se devuelve sola: si corresponde, " +
            "registrala como egreso en Caja.",
            "Cotillón", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirmar != MessageBoxResult.Yes)
            return;

        var respuesta = await App.Api.CancelarEncargueAsync(_elegido.EncargueId, "Cancelado desde la agenda");

        if (!respuesta.Success)
        {
            MostrarAviso(respuesta.Message);
            return;
        }

        Limpiar();
        await BuscarAsync();
    }

    // ---------------------------------------------------------------- Alta --

    private void AlElegirCliente(object sender, SelectionChangedEventArgs e)
    {
        // Elegir el cliente completa el nombre y el teléfono: el encargue casi
        // siempre va a nombre de quien ya está cargado.
        if (CboCliente.SelectedItem is not ClienteDto cliente || cliente.ClienteId == 0)
            return;

        if (string.IsNullOrWhiteSpace(TxtContacto.Text))
            TxtContacto.Text = cliente.NombreCompleto;

        if (string.IsNullOrWhiteSpace(TxtTelefono.Text))
            TxtTelefono.Text = cliente.Telefono ?? string.Empty;
    }

    private async void AlTeclaProducto(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        e.Handled = true;
        await AgregarLineaAsync();
    }

    private async void AlAgregarLinea(object sender, RoutedEventArgs e) => await AgregarLineaAsync();

    /// <summary>
    /// Agrega un producto al pedido. Acepta "60*bolsita", que es como se dicta un
    /// encargue: primero cuántas y después qué.
    /// </summary>
    private async Task AgregarLineaAsync()
    {
        var texto = TxtProducto.Text.Trim();

        if (texto.Length == 0)
            return;

        var cantidad = 1m;
        var separador = texto.IndexOfAny(['*', 'x', 'X']);

        if (separador > 0 && Numero(texto[..separador]) > 0)
        {
            cantidad = Numero(texto[..separador]);
            texto = texto[(separador + 1)..].Trim();
        }

        var respuesta = await App.Api.BuscarProductosAsync(texto);

        if (!respuesta.Success || respuesta.Data is null || respuesta.Data.Count == 0)
        {
            MostrarAviso($"No se encontró ningún producto con «{texto}».");
            return;
        }

        var producto = respuesta.Data[0];
        var existente = _lineas.FirstOrDefault(l => l.ProductoId == producto.ProductoId);

        if (existente is not null)
        {
            existente.Cantidad += cantidad;
            GrillaLineas.Items.Refresh();
            Recalcular();
        }
        else
        {
            _lineas.Add(new LineaEncargueDto
            {
                ProductoId = producto.ProductoId,
                NombreProducto = producto.NombreProducto,
                Cantidad = cantidad,
                PrecioUnitario = producto.Precio,
                UnidadMedida = producto.UnidadMedida
            });
        }

        OcultarAviso();
        TxtProducto.Clear();
        TxtProducto.Focus();
    }

    private void AlQuitarLinea(object sender, RoutedEventArgs e)
    {
        if (GrillaLineas.SelectedItem is LineaEncargueDto linea)
            _lineas.Remove(linea);
    }

    private void Recalcular() =>
        LblTotal.Text = _lineas.Sum(l => l.Subtotal).ToString("C2", CultureInfo.CurrentCulture);

    private async void AlGuardar(object sender, RoutedEventArgs e)
    {
        if (!_puedeEditar)
            return;

        if (_lineas.Count == 0)
        {
            MostrarAviso("Agregá al menos un producto al encargue.");
            return;
        }

        if (string.IsNullOrWhiteSpace(TxtContacto.Text))
        {
            MostrarAviso("Poné a nombre de quién va el encargue.");
            return;
        }

        if (FechaEntrega.SelectedDate is not { } entrega)
        {
            MostrarAviso("Elegí la fecha de entrega.");
            return;
        }

        BtnGuardar.IsEnabled = false;

        try
        {
            var respuesta = await App.Api.CrearEncargueAsync(new CrearEncargueDto
            {
                UsuarioId = Sesion.UsuarioId,
                ClienteId = CboCliente.SelectedValue is int id && id > 0 ? id : null,
                NombreContacto = TxtContacto.Text.Trim(),
                Telefono = TxtTelefono.Text.Trim(),
                FechaEntrega = entrega,
                Sena = Numero(TxtSena.Text),
                Observaciones = TxtObservaciones.Text.Trim(),
                Lineas = [.. _lineas.Select(l => new CrearLineaEncargueDto
                {
                    ProductoId = l.ProductoId,
                    Cantidad = l.Cantidad
                })]
            });

            if (!respuesta.Success)
            {
                MostrarAviso(respuesta.Message);
                return;
            }

            MessageBox.Show(
                $"Encargue #{respuesta.Data?.EncargueId} tomado para el {entrega:dd/MM/yyyy}.",
                "Cotillón", MessageBoxButton.OK, MessageBoxImage.Information);

            Limpiar();
            await BuscarAsync();
        }
        finally
        {
            BtnGuardar.IsEnabled = true;
        }
    }

    private void Limpiar()
    {
        _elegido = null;

        _lineas.Clear();
        TxtContacto.Clear();
        TxtTelefono.Clear();
        TxtProducto.Clear();
        TxtSena.Text = "0";
        TxtObservaciones.Clear();
        FechaEntrega.SelectedDate = DateTime.Today.AddDays(7);
        CboCliente.SelectedIndex = 0;
        Grilla.SelectedItem = null;

        LblFormulario.Text = "Nuevo encargue";
        PanelDetalle.Visibility = Visibility.Collapsed;
        PanelAlta.Visibility = _puedeEditar ? Visibility.Visible : Visibility.Collapsed;

        OcultarAviso();
        Recalcular();
    }

    private void MostrarAviso(string mensaje)
    {
        LblAviso.Text = mensaje;
        LblAviso.Visibility = Visibility.Visible;
    }

    private void OcultarAviso() => LblAviso.Visibility = Visibility.Collapsed;

    private static decimal Numero(string? texto) =>
        decimal.TryParse(texto?.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out var valor)
            ? valor
            : 0m;
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Cotillon.Desktop.Wpf.Servicios;
using Cotillon.Shared.DTOs.Permisos;
using Cotillon.Shared.DTOs.Terceros;

namespace Cotillon.Desktop.Wpf.Vistas.Paginas;

public partial class PaginaClientes : UserControl
{
    private int _clienteSeleccionado;
    private bool _puedeEditar;

    public PaginaClientes()
    {
        InitializeComponent();

        Adjuntos.Titulo = "Archivos del cliente";
        Adjuntos.AvisoSinSeleccion =
            "Elegí un cliente de la lista para ver sus facturas, comprobantes o fotos.";
        Adjuntos.AplicarPermisos(Modulos.Clientes);

        // Sin permiso de edición la pantalla queda de sólo consulta: se ve la lista
        // y los archivos, pero el formulario no deja guardar.
        _puedeEditar = Sesion.PuedeEditar(Modulos.Clientes);

        BtnGuardar.IsEnabled = _puedeEditar;
        BtnGuardar.Visibility = _puedeEditar ? Visibility.Visible : Visibility.Collapsed;
        BtnBaja.Visibility = Sesion.PuedeTodo(Modulos.Clientes) ? Visibility.Visible : Visibility.Collapsed;

        Loaded += async (_, _) => await BuscarAsync();
    }

    private async void AlBuscar(object sender, RoutedEventArgs e) => await BuscarAsync();

    private async void AlTecla(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await BuscarAsync();
    }

    private async Task BuscarAsync()
    {
        var respuesta = await App.Api.ClientesAsync(TxtBuscar.Text.Trim(), ChkInactivos.IsChecked == true);

        Grilla.ItemsSource = respuesta.Data;
    }

    private async void AlSeleccionar(object sender, SelectionChangedEventArgs e)
    {
        if (Grilla.SelectedItem is not ClienteDto cliente)
            return;

        _clienteSeleccionado = cliente.ClienteId;
        LblFormulario.Text = $"Editando: {cliente.NombreCompleto}";

        Adjuntos.Titulo = $"Archivos de {cliente.NombreCompleto}";
        await Adjuntos.CargarAsync(cliente.ClienteId);

        TxtNombre.Text = cliente.NombreCompleto;
        TxtDocumento.Text = cliente.Documento;
        TxtTelefono.Text = cliente.Telefono;
        TxtEmail.Text = cliente.Email;
        TxtDireccion.Text = cliente.Direccion;
        TxtObservaciones.Text = cliente.Observaciones;
        ChkActivo.IsChecked = cliente.Activo;

        BtnBaja.IsEnabled = cliente.Activo && Sesion.PuedeTodo(Modulos.Clientes);
    }

    private async void AlGuardar(object sender, RoutedEventArgs e)
    {
        if (!_puedeEditar)
            return;

        if (string.IsNullOrWhiteSpace(TxtNombre.Text))
        {
            MessageBox.Show("El nombre del cliente es obligatorio.", "Cotillón",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var cliente = new ClienteDto
        {
            ClienteId = _clienteSeleccionado,
            NombreCompleto = TxtNombre.Text.Trim(),
            Documento = TxtDocumento.Text.Trim(),
            Telefono = TxtTelefono.Text.Trim(),
            Email = TxtEmail.Text.Trim(),
            Direccion = TxtDireccion.Text.Trim(),
            Observaciones = TxtObservaciones.Text.Trim(),
            Activo = ChkActivo.IsChecked == true
        };

        BtnGuardar.IsEnabled = false;

        try
        {
            var respuesta = _clienteSeleccionado > 0
                ? await App.Api.ActualizarClienteAsync(cliente)
                : await App.Api.CrearClienteAsync(cliente);

            MessageBox.Show(respuesta.Message, "Cotillón", MessageBoxButton.OK,
                respuesta.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (respuesta.Success)
            {
                Limpiar();
                await BuscarAsync();
            }
        }
        finally
        {
            BtnGuardar.IsEnabled = true;
        }
    }

    private async void AlDarDeBaja(object sender, RoutedEventArgs e)
    {
        if (_clienteSeleccionado <= 0)
            return;

        var confirmar = MessageBox.Show("¿Dar de baja este cliente?", "Cotillón",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirmar != MessageBoxResult.Yes)
            return;

        var respuesta = await App.Api.DesactivarClienteAsync(_clienteSeleccionado);

        MessageBox.Show(respuesta.Message, "Cotillón", MessageBoxButton.OK,
            respuesta.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);

        if (respuesta.Success)
        {
            Limpiar();
            await BuscarAsync();
        }
    }

    private void AlNuevo(object sender, RoutedEventArgs e) => Limpiar();

    private void Limpiar()
    {
        _clienteSeleccionado = 0;
        LblFormulario.Text = "Nuevo cliente";

        TxtNombre.Clear();
        TxtDocumento.Clear();
        TxtTelefono.Clear();
        TxtEmail.Clear();
        TxtDireccion.Clear();
        TxtObservaciones.Clear();
        ChkActivo.IsChecked = true;

        BtnGuardar.IsEnabled = _puedeEditar;
        BtnBaja.IsEnabled = false;
        Grilla.SelectedItem = null;

        // Sin cliente elegido no hay archivos que mostrar. Con id 0 el panel no
        // consulta la API, así que no hace falta esperar la tarea.
        Adjuntos.Titulo = "Archivos del cliente";
        _ = Adjuntos.LimpiarAsync();
    }
}

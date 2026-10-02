using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Cotillon.Desktop.Wpf.Servicios;
using Cotillon.Shared.DTOs.Permisos;
using Cotillon.Shared.DTOs.Terceros;

namespace Cotillon.Desktop.Wpf.Vistas.Paginas;

public partial class PaginaProveedores : UserControl
{
    private int _proveedorSeleccionado;
    private bool _puedeEditar;

    public PaginaProveedores()
    {
        InitializeComponent();

        Adjuntos.Titulo = "Archivos del proveedor";
        Adjuntos.AvisoSinSeleccion =
            "Elegí un proveedor de la lista para ver sus facturas, remitos o listas de precios.";
        Adjuntos.AplicarPermisos(Modulos.Proveedores);

        // Sin permiso de edición queda de sólo consulta: se ve la lista y los
        // archivos, pero el formulario no deja guardar.
        _puedeEditar = Sesion.PuedeEditar(Modulos.Proveedores);

        BtnGuardar.IsEnabled = _puedeEditar;
        BtnGuardar.Visibility = _puedeEditar ? Visibility.Visible : Visibility.Collapsed;
        BtnBaja.Visibility = Sesion.PuedeTodo(Modulos.Proveedores) ? Visibility.Visible : Visibility.Collapsed;

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
        var respuesta = await App.Api.ProveedoresAsync(TxtBuscar.Text.Trim(), ChkInactivos.IsChecked == true);

        Grilla.ItemsSource = respuesta.Data;
    }

    private async void AlSeleccionar(object sender, SelectionChangedEventArgs e)
    {
        if (Grilla.SelectedItem is not ProveedorDto proveedor)
            return;

        _proveedorSeleccionado = proveedor.ProveedorId;
        LblFormulario.Text = $"Editando: {proveedor.NombreProveedor}";

        Adjuntos.Titulo = $"Archivos de {proveedor.NombreProveedor}";
        await Adjuntos.CargarAsync(proveedor.ProveedorId);

        TxtNombre.Text = proveedor.NombreProveedor;
        TxtCuit.Text = proveedor.Cuit;
        TxtTelefono.Text = proveedor.Telefono;
        TxtEmail.Text = proveedor.Email;
        TxtDireccion.Text = proveedor.Direccion;
        TxtObservaciones.Text = proveedor.Observaciones;
        ChkActivo.IsChecked = proveedor.Activo;

        BtnBaja.IsEnabled = proveedor.Activo && Sesion.PuedeTodo(Modulos.Proveedores);
    }

    private async void AlGuardar(object sender, RoutedEventArgs e)
    {
        if (!_puedeEditar)
            return;

        if (string.IsNullOrWhiteSpace(TxtNombre.Text))
        {
            MessageBox.Show("El nombre del proveedor es obligatorio.", "Cotillón",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var proveedor = new ProveedorDto
        {
            ProveedorId = _proveedorSeleccionado,
            NombreProveedor = TxtNombre.Text.Trim(),
            Cuit = TxtCuit.Text.Trim(),
            Telefono = TxtTelefono.Text.Trim(),
            Email = TxtEmail.Text.Trim(),
            Direccion = TxtDireccion.Text.Trim(),
            Observaciones = TxtObservaciones.Text.Trim(),
            Activo = ChkActivo.IsChecked == true
        };

        BtnGuardar.IsEnabled = false;

        try
        {
            var respuesta = _proveedorSeleccionado > 0
                ? await App.Api.ActualizarProveedorAsync(proveedor)
                : await App.Api.CrearProveedorAsync(proveedor);

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
        if (_proveedorSeleccionado <= 0)
            return;

        var confirmar = MessageBox.Show("¿Dar de baja este proveedor?", "Cotillón",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirmar != MessageBoxResult.Yes)
            return;

        var respuesta = await App.Api.DesactivarProveedorAsync(_proveedorSeleccionado);

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
        _proveedorSeleccionado = 0;
        LblFormulario.Text = "Nuevo proveedor";

        TxtNombre.Clear();
        TxtCuit.Clear();
        TxtTelefono.Clear();
        TxtEmail.Clear();
        TxtDireccion.Clear();
        TxtObservaciones.Clear();
        ChkActivo.IsChecked = true;

        BtnGuardar.IsEnabled = _puedeEditar;
        BtnBaja.IsEnabled = false;
        Grilla.SelectedItem = null;

        Adjuntos.Titulo = "Archivos del proveedor";
        _ = Adjuntos.LimpiarAsync();
    }
}

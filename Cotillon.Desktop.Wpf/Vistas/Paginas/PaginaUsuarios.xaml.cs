using System.Windows;
using System.Windows.Controls;
using Cotillon.Shared.DTOs.Usuarios;

namespace Cotillon.Desktop.Wpf.Vistas.Paginas;

public partial class PaginaUsuarios : UserControl
{
    private int _usuarioSeleccionado;

    public PaginaUsuarios()
    {
        InitializeComponent();

        Loaded += async (_, _) => await CargarAsync();
    }

    private async Task CargarAsync()
    {
        var respuesta = await App.Api.UsuariosAsync(ChkInactivos.IsChecked == true);

        Grilla.ItemsSource = respuesta.Data;
    }

    private async void AlRefrescar(object sender, RoutedEventArgs e) => await CargarAsync();

    private void AlSeleccionar(object sender, SelectionChangedEventArgs e)
    {
        if (Grilla.SelectedItem is not UsuarioDto usuario)
            return;

        _usuarioSeleccionado = usuario.UsuarioId;
        LblFormulario.Text = $"Editando: {usuario.NombreUsuario}";

        TxtUsuario.Text = usuario.NombreUsuario;
        TxtUsuario.IsEnabled = false;
        TxtNombre.Text = usuario.NombreCompleto;
        ChkActivo.IsChecked = usuario.Activo;

        foreach (ComboBoxItem item in CboRol.Items)
        {
            if ((string?)item.Content == usuario.Rol)
                item.IsSelected = true;
        }

        // Al editar, el PIN sólo se toca con el botón de restablecer.
        TxtPin.Clear();
        TxtPin.IsEnabled = false;
        LblPin.Text = "PIN (usá Restablecer PIN para cambiarlo)";
        BtnResetPin.IsEnabled = true;
    }

    private async void AlGuardar(object sender, RoutedEventArgs e)
    {
        var rol = (CboRol.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Cajero";

        if (string.IsNullOrWhiteSpace(TxtNombre.Text))
        {
            MessageBox.Show("El nombre completo es obligatorio.", "Cotillón",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        BtnGuardar.IsEnabled = false;

        try
        {
            if (_usuarioSeleccionado > 0)
            {
                var respuesta = await App.Api.ActualizarUsuarioAsync(new UsuarioDto
                {
                    UsuarioId = _usuarioSeleccionado,
                    NombreUsuario = TxtUsuario.Text.Trim(),
                    NombreCompleto = TxtNombre.Text.Trim(),
                    Rol = rol,
                    Activo = ChkActivo.IsChecked == true
                });

                Informar(respuesta.Success, respuesta.Message);

                if (respuesta.Success)
                {
                    Limpiar();
                    await CargarAsync();
                }

                return;
            }

            if (string.IsNullOrWhiteSpace(TxtUsuario.Text))
            {
                MessageBox.Show("El nombre de usuario es obligatorio.", "Cotillón",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TxtPin.Password.Trim().Length < 4)
            {
                MessageBox.Show("El PIN debe tener al menos 4 dígitos.", "Cotillón",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var creacion = await App.Api.CrearUsuarioAsync(new CrearUsuarioDto
            {
                NombreUsuario = TxtUsuario.Text.Trim(),
                NombreCompleto = TxtNombre.Text.Trim(),
                Pin = TxtPin.Password.Trim(),
                Rol = rol,
                Activo = ChkActivo.IsChecked == true
            });

            Informar(creacion.Success, creacion.Message);

            if (creacion.Success)
            {
                Limpiar();
                await CargarAsync();
            }
        }
        finally
        {
            BtnGuardar.IsEnabled = true;
        }
    }

    private async void AlResetearPin(object sender, RoutedEventArgs e)
    {
        if (_usuarioSeleccionado <= 0)
            return;

        var ventana = new VentanaPin();

        if (ventana.ShowDialog() != true)
            return;

        var respuesta = await App.Api.CambiarPinAsync(new CambiarPinDto
        {
            UsuarioId = _usuarioSeleccionado,
            PinNuevo = ventana.Pin
        }, comoAdmin: true);

        Informar(respuesta.Success, respuesta.Message);
    }

    private void AlNuevo(object sender, RoutedEventArgs e) => Limpiar();

    private void Limpiar()
    {
        _usuarioSeleccionado = 0;
        LblFormulario.Text = "Nuevo usuario";

        TxtUsuario.Clear();
        TxtUsuario.IsEnabled = true;
        TxtNombre.Clear();
        TxtPin.Clear();
        TxtPin.IsEnabled = true;
        LblPin.Text = "PIN (mínimo 4 dígitos)";
        ChkActivo.IsChecked = true;
        CboRol.SelectedIndex = 0;

        BtnResetPin.IsEnabled = false;
        Grilla.SelectedItem = null;
    }

    private static void Informar(bool exito, string mensaje) =>
        MessageBox.Show(mensaje, "Cotillón", MessageBoxButton.OK,
            exito ? MessageBoxImage.Information : MessageBoxImage.Warning);
}

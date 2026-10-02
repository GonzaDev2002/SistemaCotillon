using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Cotillon.Desktop.Wpf.Servicios;
using Cotillon.Shared.DTOs.Permisos;
using Cotillon.Shared.DTOs.Usuarios;

namespace Cotillon.Desktop.Wpf.Vistas.Paginas;

public partial class PaginaUsuarios : UserControl
{
    private readonly bool _puedeEditar = Sesion.PuedeEditar(Modulos.Usuarios);
    private readonly bool _puedePermisos = Sesion.PuedeTodo(Modulos.Usuarios);

    private int _usuarioSeleccionado;
    private List<FilaPermiso> _permisos = [];

    public PaginaUsuarios()
    {
        InitializeComponent();

        DataContext = this;

        // El tema fija 36px de alto por fila, que acá corta la explicación de cada
        // módulo. NaN deja que la fila crezca con su contenido.
        GrillaPermisos.RowHeight = double.NaN;

        BtnGuardar.Visibility = _puedeEditar ? Visibility.Visible : Visibility.Collapsed;
        BtnResetPin.Visibility = _puedePermisos ? Visibility.Visible : Visibility.Collapsed;
        BtnPlantilla.Visibility = _puedePermisos ? Visibility.Visible : Visibility.Collapsed;
        BtnGuardarPermisos.Visibility = _puedePermisos ? Visibility.Visible : Visibility.Collapsed;

        Loaded += async (_, _) => await CargarAsync();
    }

    /// <summary>Opciones del combo de cada módulo, en orden de menor a mayor.</summary>
    public IReadOnlyList<OpcionNivel> Niveles { get; } =
    [
        new(NivelAcceso.Ninguno, "Sin acceso"),
        new(NivelAcceso.Ver, "Sólo ver"),
        new(NivelAcceso.Editar, "Ver y cargar"),
        new(NivelAcceso.Total, "Todo")
    ];

    private async Task CargarAsync()
    {
        var respuesta = await App.Api.UsuariosAsync(ChkInactivos.IsChecked == true);

        Grilla.ItemsSource = respuesta.Data;
    }

    private async void AlRefrescar(object sender, RoutedEventArgs e) => await CargarAsync();

    private async void AlSeleccionar(object sender, SelectionChangedEventArgs e)
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
        BtnResetPin.IsEnabled = _puedePermisos;

        await CargarPermisosAsync(usuario);
    }

    // --------------------------------------------------------------- Permisos --

    private async Task CargarPermisosAsync(UsuarioDto usuario)
    {
        var respuesta = await App.Api.PermisosDeUsuarioAsync(usuario.UsuarioId);

        LblPermisos.Text = $"Permisos de {usuario.NombreCompleto}";

        if (!respuesta.Success || respuesta.Data is null)
        {
            LblAyudaPermisos.Text = respuesta.Message;
            return;
        }

        MostrarPermisos(respuesta.Data);

        LblAyudaPermisos.Text = usuario.UsuarioId == Sesion.UsuarioId
            ? "Son tus propios permisos. Si te sacás un módulo, lo perdés al volver a entrar."
            : $"Rol {usuario.Rol}. Cambiá lo que necesites y guardá: el cambio se aplica cuando el usuario vuelve a entrar.";

        // Editar permisos es la acción delicada del módulo de usuarios.
        GrillaPermisos.IsEnabled = _puedePermisos;
        BtnGuardarPermisos.IsEnabled = _puedePermisos;
        BtnPlantilla.IsEnabled = _puedePermisos;
    }

    private void MostrarPermisos(Dictionary<string, NivelAcceso> permisos)
    {
        _permisos = Modulos.Todos
            .Select(m => new FilaPermiso(m, permisos.GetValueOrDefault(m.Codigo, NivelAcceso.Ninguno)))
            .ToList();

        GrillaPermisos.ItemsSource = _permisos;
    }

    private async void AlAplicarPlantilla(object sender, RoutedEventArgs e)
    {
        if (!_puedePermisos || _usuarioSeleccionado <= 0)
            return;

        var rol = (CboRol.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Cajero";

        var respuesta = await App.Api.PlantillaPermisosAsync(rol);

        if (!respuesta.Success || respuesta.Data is null)
        {
            Informar(false, respuesta.Message);
            return;
        }

        // Sólo se cargan en pantalla: el administrador todavía puede ajustarlos
        // antes de guardar.
        MostrarPermisos(respuesta.Data);

        LblAyudaPermisos.Text =
            $"Cargada la plantilla del rol {rol}. Ajustá lo que haga falta y guardá.";
    }

    private async void AlGuardarPermisos(object sender, RoutedEventArgs e)
    {
        if (!_puedePermisos || _usuarioSeleccionado <= 0)
            return;

        if (_usuarioSeleccionado == Sesion.UsuarioId)
        {
            var seguir = MessageBox.Show(
                "Estás cambiando tus propios permisos. Si te quitás un módulo vas a perderlo " +
                "la próxima vez que entres.\n\n¿Guardar igual?",
                "Cotillón", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (seguir != MessageBoxResult.Yes)
                return;
        }

        BtnGuardarPermisos.IsEnabled = false;

        try
        {
            var respuesta = await App.Api.GuardarPermisosAsync(
                _usuarioSeleccionado, _permisos.ToDictionary(p => p.Codigo, p => p.Nivel));

            Informar(respuesta.Success, respuesta.Message);
        }
        finally
        {
            BtnGuardarPermisos.IsEnabled = _puedePermisos;
        }
    }

    // ------------------------------------------------------- Alta y edición --

    private async void AlGuardar(object sender, RoutedEventArgs e)
    {
        if (!_puedeEditar)
            return;

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

            if (creacion.Success)
            {
                MessageBox.Show(
                    $"{creacion.Message}\n\nQuedó con los permisos del rol {rol}. " +
                    "Elegilo en la lista para ajustarlos módulo por módulo.",
                    "Cotillón", MessageBoxButton.OK, MessageBoxImage.Information);

                Limpiar();
                await CargarAsync();
                return;
            }

            Informar(false, creacion.Message);
        }
        finally
        {
            BtnGuardar.IsEnabled = _puedeEditar;
        }
    }

    private async void AlResetearPin(object sender, RoutedEventArgs e)
    {
        if (!_puedePermisos || _usuarioSeleccionado <= 0)
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

        BtnGuardar.IsEnabled = _puedeEditar;
        BtnResetPin.IsEnabled = false;
        Grilla.SelectedItem = null;

        // Sin usuario elegido no hay permisos que mostrar.
        _permisos = [];
        GrillaPermisos.ItemsSource = null;
        GrillaPermisos.IsEnabled = false;
        BtnGuardarPermisos.IsEnabled = false;
        BtnPlantilla.IsEnabled = false;

        LblPermisos.Text = "Permisos";
        LblAyudaPermisos.Text =
            "Elegí un usuario de la lista de arriba para ver y cambiar qué puede hacer en cada módulo.";
    }

    private static void Informar(bool exito, string mensaje) =>
        MessageBox.Show(mensaje, "Cotillón", MessageBoxButton.OK,
            exito ? MessageBoxImage.Information : MessageBoxImage.Warning);

    public sealed record OpcionNivel(NivelAcceso Nivel, string Texto);

    /// <summary>
    /// Una fila del editor: el módulo, qué significan sus niveles y el elegido.
    /// </summary>
    private sealed class FilaPermiso : INotifyPropertyChanged
    {
        private NivelAcceso _nivel;

        public FilaPermiso(Modulo modulo, NivelAcceso nivel)
        {
            Codigo = modulo.Codigo;
            Modulo = modulo.Nombre;
            _nivel = nivel;

            var partes = new List<string>();

            if (modulo.QueEsEditar != "—")
                partes.Add($"Ver y cargar: {modulo.QueEsEditar}");

            if (modulo.QueEsTotal != "—")
                partes.Add($"Todo: {modulo.QueEsTotal}");

            Ayuda = partes.Count > 0
                ? string.Join("  ", partes)
                : "Sólo se consulta: no hay nada que modificar en este módulo.";
        }

        public string Codigo { get; }
        public string Modulo { get; }
        public string Ayuda { get; }

        public NivelAcceso Nivel
        {
            get => _nivel;
            set
            {
                _nivel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Nivel)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}

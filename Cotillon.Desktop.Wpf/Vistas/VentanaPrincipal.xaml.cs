using System.Windows;
using System.Windows.Controls;
using Cotillon.Desktop.Wpf.Servicios;
using Cotillon.Desktop.Wpf.Vistas.Paginas;

namespace Cotillon.Desktop.Wpf.Vistas;

public partial class VentanaPrincipal : Window
{
    public VentanaPrincipal()
    {
        InitializeComponent();

        LblUsuario.Text = Sesion.NombreCompleto;
        LblRol.Text = Sesion.Rol;

        AplicarPermisos();

        Loaded += async (_, _) =>
        {
            await CargarNombreNegocioAsync();
            await RefrescarEstadoCajaAsync();
            Navegar(NavInicio);
        };
    }

    /// <summary>
    /// Oculta lo que el rol no puede usar en lugar de dejarlo visible y fallar al
    /// entrar: el cajero no ve administración ni compras.
    /// </summary>
    private void AplicarPermisos()
    {
        var administracion = Sesion.PuedeAdministrar ? Visibility.Visible : Visibility.Collapsed;

        LblAdministracion.Visibility = administracion;
        NavUsuarios.Visibility = administracion;
        NavAuditoria.Visibility = administracion;
        NavConfiguracion.Visibility = administracion;

        var supervision = Sesion.PuedeSupervisar ? Visibility.Visible : Visibility.Collapsed;

        NavCompras.Visibility = supervision;
        NavProveedores.Visibility = supervision;
        NavStock.Visibility = supervision;
    }

    private async Task CargarNombreNegocioAsync()
    {
        var respuesta = await App.Api.ConfiguracionAsync();

        if (respuesta.Success && respuesta.Data is not null &&
            respuesta.Data.TryGetValue("NEGOCIO_NOMBRE", out var nombre) &&
            !string.IsNullOrWhiteSpace(nombre))
        {
            LblNegocio.Text = nombre;
            Title = $"{nombre} - Sistema de gestión";
        }
    }

    public async Task RefrescarEstadoCajaAsync()
    {
        var respuesta = await App.Api.CajaActualAsync();
        var abierta = respuesta.Success && respuesta.Data is not null;

        LblEstadoCaja.Text = abierta
            ? $"Caja abierta · {respuesta.Data!.UsuarioApertura}"
            : "Caja cerrada";

        ChipCaja.Background = abierta
            ? (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#D1FAE5")!
            : (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#FEF3C7")!;

        LblEstadoCaja.Foreground = abierta
            ? (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#065F46")!
            : (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#92400E")!;
    }

    private void AlNavegar(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded)
            return;

        Navegar((RadioButton)sender);
    }

    private void Navegar(RadioButton origen)
    {
        var (titulo, descripcion, pagina) = Resolver(origen);

        LblTitulo.Text = titulo;
        LblDescripcion.Text = descripcion;
        Contenido.Content = pagina;
    }

    private (string Titulo, string Descripcion, UserControl Pagina) Resolver(RadioButton origen)
    {
        if (origen == NavVentas)
            return ("Ventas", "Registrá una venta nueva", new PaginaVentas(this));

        if (origen == NavCaja)
            return ("Caja", "Apertura, movimientos y arqueo", new PaginaCaja(this));

        if (origen == NavHistorialVentas)
            return ("Historial de ventas", "Consultá y anulá ventas", new PaginaHistorialVentas());

        if (origen == NavProductos)
            return ("Productos", "Catálogo, precios y stock", new PaginaProductos());

        if (origen == NavCatalogo)
            return ("Secciones y categorías", "Organización del catálogo", new PaginaCatalogo());

        if (origen == NavStock)
            return ("Movimientos de stock", "Ajustes, mermas y trazabilidad", new PaginaStock());

        if (origen == NavCompras)
            return ("Compras", "Pedidos a proveedores y recepción", new PaginaCompras());

        if (origen == NavClientes)
            return ("Clientes", "Base de clientes del negocio", new PaginaClientes());

        if (origen == NavProveedores)
            return ("Proveedores", "Proveedores y datos de contacto", new PaginaProveedores());

        if (origen == NavReportes)
            return ("Reportes", "Productos, stock y ventas", new PaginaReportes());

        if (origen == NavUsuarios)
            return ("Usuarios", "Accesos y roles del sistema", new PaginaUsuarios());

        if (origen == NavAuditoria)
            return ("Auditoría", "Todo lo que pasó en el sistema", new PaginaAuditoria());

        if (origen == NavConfiguracion)
            return ("Configuración", "Datos del negocio y preferencias", new PaginaConfiguracion());

        return ("Inicio", "Resumen del negocio", new PaginaInicio(this));
    }

    public void IrAVentas()
    {
        NavVentas.IsChecked = true;
    }

    public void IrACaja()
    {
        NavCaja.IsChecked = true;
    }

    private void AlCerrarSesion(object sender, RoutedEventArgs e)
    {
        var confirmar = MessageBox.Show(
            "¿Querés cerrar la sesión?", "Cotillón",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirmar != MessageBoxResult.Yes)
            return;

        Sesion.Cerrar();

        // Mientras se reemplaza la ventana principal la aplicación queda sin
        // MainWindow, así que el cierre vuelve a ser explícito por un momento.
        Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var login = new VentanaLogin();

        if (login.ShowDialog() == true)
        {
            var principal = new VentanaPrincipal();
            Application.Current.MainWindow = principal;
            principal.Show();

            Close();

            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            return;
        }

        Close();
        Application.Current.Shutdown();
    }
}

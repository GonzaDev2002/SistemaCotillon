using System.Windows;
using System.Windows.Controls;
using Cotillon.Desktop.Wpf.Servicios;
using Cotillon.Desktop.Wpf.Vistas.Paginas;
using Cotillon.Shared.DTOs.Permisos;

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

            // Se entra por el primer módulo que el usuario tenga habilitado, que
            // para un cajero suele ser Ventas y no Inicio.
            Navegar(PrimerModuloVisible());
        };

        // El estado de la caja está en la barra de arriba de todas las pantallas:
        // si otro puesto la abre o la cierra, acá se ve sin salir de donde se está.
        Refresco.Automatico(this, RefrescarEstadoCajaAsync);
    }

    /// <summary>
    /// Cada botón del menú sabe a qué módulo corresponde. Tenerlo en un solo lugar
    /// evita que al agregar una pantalla se olvide el permiso en alguno de los dos
    /// lados y quede visible para todos.
    /// </summary>
    private IEnumerable<(RadioButton Boton, string Modulo)> BotonesPorModulo() =>
    [
        (NavInicio, Modulos.Inicio),
        (NavVentas, Modulos.Ventas),
        (NavCaja, Modulos.Caja),
        (NavHistorialVentas, Modulos.HistorialVentas),
        (NavProductos, Modulos.Productos),
        (NavCatalogo, Modulos.Catalogo),
        (NavStock, Modulos.Stock),
        (NavCompras, Modulos.Compras),
        (NavClientes, Modulos.Clientes),
        (NavCuentaCorriente, Modulos.CuentaCorriente),
        (NavEncargues, Modulos.Encargues),
        (NavProveedores, Modulos.Proveedores),
        (NavReportes, Modulos.Reportes),
        (NavUsuarios, Modulos.Usuarios),
        (NavAuditoria, Modulos.Auditoria),
        (NavConfiguracion, Modulos.Configuracion)
    ];

    /// <summary>
    /// Oculta lo que el usuario no tiene habilitado, en lugar de dejarlo visible y
    /// fallar al entrar. Los títulos de grupo desaparecen cuando no queda ningún
    /// módulo debajo, así el menú de un cajero no muestra secciones vacías.
    /// </summary>
    private void AplicarPermisos()
    {
        foreach (var (boton, modulo) in BotonesPorModulo())
            boton.Visibility = Sesion.PuedeVer(modulo) ? Visibility.Visible : Visibility.Collapsed;

        LblOperacion.Visibility = VisibilidadDeGrupo(
            Modulos.Inicio, Modulos.Ventas, Modulos.Caja, Modulos.HistorialVentas);

        LblInventario.Visibility = VisibilidadDeGrupo(
            Modulos.Productos, Modulos.Catalogo, Modulos.Stock, Modulos.Compras);

        LblGestion.Visibility = VisibilidadDeGrupo(
            Modulos.Clientes, Modulos.Proveedores, Modulos.Reportes);

        LblAdministracion.Visibility = VisibilidadDeGrupo(
            Modulos.Usuarios, Modulos.Auditoria, Modulos.Configuracion);
    }

    private static Visibility VisibilidadDeGrupo(params string[] modulos) =>
        modulos.Any(Sesion.PuedeVer) ? Visibility.Visible : Visibility.Collapsed;

    private RadioButton PrimerModuloVisible() =>
        BotonesPorModulo().FirstOrDefault(b => Sesion.PuedeVer(b.Modulo)).Boton ?? NavInicio;

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
        // Segunda red: ocultar el botón ya alcanza para el menú, pero a una pantalla
        // también se llega desde los accesos del panel de inicio.
        var modulo = BotonesPorModulo().FirstOrDefault(b => b.Boton == origen).Modulo;

        if (modulo is not null && !Sesion.PuedeVer(modulo))
            return;

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

        if (origen == NavCuentaCorriente)
            return ("Cuenta corriente", "Quién debe y cuánto", new PaginaCuentaCorriente());

        if (origen == NavEncargues)
            return ("Encargues", "Pedidos con seña y entregas", new PaginaEncargues());

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
        if (Sesion.PuedeVer(Modulos.Ventas))
            NavVentas.IsChecked = true;
    }

    public void IrACaja()
    {
        if (Sesion.PuedeVer(Modulos.Caja))
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

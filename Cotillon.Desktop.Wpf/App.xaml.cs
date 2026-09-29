using System.Windows;
using System.Windows.Threading;
using Cotillon.Desktop.Wpf.Servicios;
using Cotillon.Desktop.Wpf.Vistas;

namespace Cotillon.Desktop.Wpf;

public partial class App : Application
{
    public static ConfiguracionApp Configuracion { get; private set; } = new();

    public static ServiciosApi Api { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += AlFallar;

        Configuracion = ConfiguracionApp.Cargar();
        Api = new ServiciosApi(new ApiCliente(Configuracion.UrlApi));

        var pantallaCarga = new VentanaCarga();
        pantallaCarga.Show();

        var disponible = Configuracion.EsServidor
            ? await ServidorLocal.AsegurarEncendidoAsync(Api.Cliente)
            : await Api.Cliente.EstaDisponibleAsync();

        pantallaCarga.Close();

        if (!disponible)
        {
            MessageBox.Show(
                $"No se pudo conectar con el servicio en {Configuracion.UrlApi}\n\n" +
                "Si esta computadora es el servidor, verificá que el servicio esté instalado.\n" +
                "Si es una terminal, revisá la dirección del servidor y la red.",
                "Cotillón", MessageBoxButton.OK, MessageBoxImage.Error);

            Shutdown();
            return;
        }

        var login = new VentanaLogin();

        if (login.ShowDialog() != true)
        {
            Shutdown();
            return;
        }

        var principal = new VentanaPrincipal();
        MainWindow = principal;
        principal.Show();

        // Hasta acá el cierre es explícito, porque entre la pantalla de carga y el
        // login la aplicación se queda sin ventanas y WPF la daría por terminada.
        ShutdownMode = ShutdownMode.OnMainWindowClose;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        ServidorLocal.Detener();

        base.OnExit(e);
    }

    private void AlFallar(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            $"Ocurrió un error inesperado:\n\n{e.Exception.Message}",
            "Cotillón", MessageBoxButton.OK, MessageBoxImage.Error);

        e.Handled = true;
    }
}

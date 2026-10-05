using System.Diagnostics;
using System.IO;

namespace Cotillon.Desktop.Wpf.Servicios;

/// <summary>
/// Levanta el servicio de API en el equipo servidor cuando todavía no está corriendo,
/// para que el usuario abra un solo programa y el sistema quede operativo.
/// </summary>
public static class ServidorLocal
{
    private const string NombreProceso = "Cotillon.LocalApi.Service";

    private static Process? _proceso;

    /// <summary>
    /// Nombre del servicio de Windows que instala el instalador. Si está
    /// instalado, es él el que manda: esta aplicación no levanta una segunda
    /// copia ni lo apaga al cerrarse.
    /// </summary>
    private const string NombreServicio = "Cotillon";

    /// <summary>Si el servicio de Windows está instalado en este equipo.</summary>
    public static bool HayServicioInstalado { get; private set; }

    public static async Task<bool> AsegurarEncendidoAsync(ApiCliente cliente)
    {
        HayServicioInstalado = ServicioInstalado();

        if (await cliente.EstaDisponibleAsync())
            return true;

        // Con el servicio instalado la espera vale la pena: puede estar
        // arrancando todavía, porque Windows lo levanta junto con el resto del
        // equipo. Lo que NO hay que hacer es lanzar otro proceso aparte: dos
        // instancias escribiendo la misma base es el peor escenario posible.
        if (HayServicioInstalado)
            return await EsperarDisponibleAsync(cliente, TimeSpan.FromSeconds(40));

        // Puede haberlo levantado otra instancia de la aplicación o Visual Studio.
        if (Process.GetProcessesByName(NombreProceso).Length > 0)
            return await EsperarDisponibleAsync(cliente, TimeSpan.FromSeconds(20));

        var ejecutable = LocalizarEjecutable();

        if (ejecutable is null)
            return false;

        try
        {
            _proceso = Process.Start(new ProcessStartInfo
            {
                FileName = ejecutable,
                WorkingDirectory = Path.GetDirectoryName(ejecutable)!,
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }
        catch
        {
            return false;
        }

        return await EsperarDisponibleAsync(cliente, TimeSpan.FromSeconds(30));
    }

    /// <summary>
    /// Mira si existe el servicio de Windows, sin pedir permisos de
    /// administrador. Se consulta la clave del registro en vez de usar
    /// ServiceController para no arrastrar una dependencia por una sola
    /// pregunta.
    /// </summary>
    private static bool ServicioInstalado()
    {
        try
        {
            using var clave = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                $@"SYSTEM\CurrentControlSet\Services\{NombreServicio}");

            return clave is not null;
        }
        catch
        {
            // Sin acceso al registro se asume que no está: el peor caso es que
            // la aplicación intente levantar la API por su cuenta, que es lo que
            // hacía antes.
            return false;
        }
    }

    public static void Detener()
    {
        // El servicio de Windows no se toca nunca: lo usan las otras terminales,
        // corren los respaldos, y el administrador puede estar conectado desde
        // afuera. Que alguien cierre la aplicación en el local no puede apagar el
        // sistema entero.
        if (HayServicioInstalado)
            return;

        // Sólo se baja el servicio si lo encendió esta aplicación: si ya estaba
        // corriendo, puede haber otras terminales usándolo.
        if (_proceso is null || _proceso.HasExited)
            return;

        try
        {
            _proceso.Kill(entireProcessTree: true);
        }
        catch
        {
            // El proceso ya no está: no hay nada que hacer.
        }
    }

    private static async Task<bool> EsperarDisponibleAsync(ApiCliente cliente, TimeSpan limite)
    {
        var reloj = Stopwatch.StartNew();

        while (reloj.Elapsed < limite)
        {
            if (await cliente.EstaDisponibleAsync())
                return true;

            await Task.Delay(500);
        }

        return false;
    }

    private static string? LocalizarEjecutable()
    {
        var nombre = $"{NombreProceso}.exe";

        var candidatos = new List<string>
        {
            // Instalación: el servicio se publica junto al cliente.
            Path.Combine(AppContext.BaseDirectory, nombre),
            Path.Combine(AppContext.BaseDirectory, "Servicio", nombre)
        };

        // Desarrollo: se busca la salida de compilación del proyecto hermano.
        var directorio = new DirectoryInfo(AppContext.BaseDirectory);

        while (directorio is not null)
        {
            var proyecto = Path.Combine(directorio.FullName, NombreProceso);

            if (Directory.Exists(proyecto))
            {
                candidatos.AddRange(
                    Directory.GetFiles(proyecto, nombre, SearchOption.AllDirectories)
                        .OrderByDescending(File.GetLastWriteTimeUtc));

                break;
            }

            directorio = directorio.Parent;
        }

        return candidatos.FirstOrDefault(File.Exists);
    }
}

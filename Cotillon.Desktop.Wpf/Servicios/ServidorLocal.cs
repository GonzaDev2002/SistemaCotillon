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

    public static async Task<bool> AsegurarEncendidoAsync(ApiCliente cliente)
    {
        if (await cliente.EstaDisponibleAsync())
            return true;

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

    public static void Detener()
    {
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

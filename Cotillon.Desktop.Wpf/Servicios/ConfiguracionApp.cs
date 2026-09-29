using System.IO;
using System.Text.Json;

namespace Cotillon.Desktop.Wpf.Servicios;

/// <summary>
/// Configuración local de la terminal. Se guarda junto al ejecutable para que cada
/// puesto apunte al servidor que le corresponde sin recompilar: en el equipo
/// servidor queda en localhost, y en las terminales se apunta a su IP.
/// </summary>
public class ConfiguracionApp
{
    private const string NombreArchivo = "cotillon.config.json";

    public string UrlApi { get; set; } = "http://localhost:5189/";

    /// <summary>
    /// Si es true, esta terminal levanta el servicio de API cuando no responde.
    /// El equipo servidor lo tiene activado; las terminales de red, no.
    /// </summary>
    public bool EsServidor { get; set; } = true;

    public static string RutaArchivo =>
        Path.Combine(AppContext.BaseDirectory, NombreArchivo);

    public static ConfiguracionApp Cargar()
    {
        try
        {
            if (File.Exists(RutaArchivo))
            {
                var json = File.ReadAllText(RutaArchivo);
                var configuracion = JsonSerializer.Deserialize<ConfiguracionApp>(json);

                if (configuracion is not null && !string.IsNullOrWhiteSpace(configuracion.UrlApi))
                    return configuracion;
            }
        }
        catch
        {
            // Un archivo corrupto no debe impedir que la aplicación abra: se
            // continúa con los valores por defecto.
        }

        return new ConfiguracionApp();
    }

    public void Guardar()
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(RutaArchivo, json);
    }
}

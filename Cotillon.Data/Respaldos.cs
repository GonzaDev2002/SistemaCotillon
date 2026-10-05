using System.IO.Compression;
using Cotillon.Shared.DTOs.Respaldos;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Cotillon.Data;

public class RespaldoFallidoException(string mensaje, Exception? causa = null)
    : Exception(mensaje, causa);

/// <summary>
/// Saca copias de seguridad de la base y de los archivos adjuntos.
///
/// Lo único que de verdad importa acá es CÓMO se saca la copia. Copiar el
/// archivo .db con File.Copy mientras el sistema está andando produce una copia
/// inservible: con WAL activado parte de los datos todavía está en el archivo
/// -wal, y además se copia mientras alguien escribe. Esa copia abre, parece
/// normal, y falla el día que hace falta.
///
/// Se usa VACUUM INTO, que es la forma que da SQLite para sacar una copia
/// consistente de una base en uso: espera el momento correcto, escribe un
/// archivo nuevo completo y además lo compacta.
///
/// Y la copia se verifica antes de darla por buena. Un respaldo que nunca se
/// probó no es un respaldo, es un archivo.
/// </summary>
public class Respaldos
{
    public const string Prefijo = "cotillon-respaldo-";

    private readonly IDbConnectionFactory _factory;

    public Respaldos(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    /// <summary>Carpeta de respaldos, al lado de la base.</summary>
    public string Carpeta =>
        Path.Combine(Path.GetDirectoryName(_factory.RutaBase) ?? ".", "Respaldos");

    private string CarpetaAdjuntos =>
        Path.Combine(Path.GetDirectoryName(_factory.RutaBase) ?? ".", "Adjuntos");

    /// <summary>
    /// Saca una copia y la deja en un zip con la base y los adjuntos. Devuelve el
    /// resultado en vez de tirar excepción cuando el problema es del entorno
    /// (disco lleno, carpeta de nube inexistente): que falle un respaldo no puede
    /// voltear el servicio que está cobrando.
    /// </summary>
    public ResultadoRespaldoDto Crear(ConfiguracionRespaldoDto configuracion)
    {
        Directory.CreateDirectory(Carpeta);

        var marca = DateTime.Now;
        var temporal = Path.Combine(Carpeta, $"base-{Guid.NewGuid():N}.db");

        // El zip se arma con un nombre temporal y se mueve al definitivo recién
        // cuando está completo. Así un respaldo que falla a mitad de camino nunca
        // toca un archivo que no creó él: la primera versión de esto, al fallar,
        // borraba el respaldo BUENO que ya existía con ese nombre.
        var temporalZip = Path.Combine(Carpeta, $"parcial-{Guid.NewGuid():N}.zip");
        var destinoZip = string.Empty;

        try
        {
            CopiarBase(temporal);
            Verificar(temporal);
            Empaquetar(temporal, temporalZip);

            destinoZip = NombreLibre(marca);
            File.Move(temporalZip, destinoZip);

            var tamano = new FileInfo(destinoZip).Length;
            var enLaNube = CopiarALaNube(destinoZip, configuracion.CarpetaNube, out var errorNube);

            Limpiar(configuracion.Copias);

            return new ResultadoRespaldoDto
            {
                Exito = true,
                Archivo = destinoZip,
                TamanoBytes = tamano,
                CopiadoALaNube = enLaNube,
                Mensaje = enLaNube
                    ? $"Respaldo guardado y copiado a la nube ({Legible(tamano)})."
                    : errorNube is null
                        ? $"Respaldo guardado ({Legible(tamano)})."
                        // El respaldo local SÍ se hizo: el aviso es sobre la copia.
                        : $"Respaldo guardado ({Legible(tamano)}), pero no se pudo copiar a la nube: {errorNube}"
            };
        }
        catch (Exception ex)
        {
            return new ResultadoRespaldoDto
            {
                Exito = false,
                Mensaje = $"No se pudo hacer el respaldo: {ex.Message}"
            };
        }
        finally
        {
            Borrar(temporal);

            // Un zip a medias es peor que ninguno: confunde al que lo necesite.
            // Sólo se borra el temporal, que es el único que creó este intento.
            Borrar(temporalZip);
        }
    }

    /// <summary>
    /// Nombre con la fecha y la hora. Si ya existe —dos respaldos en el mismo
    /// segundo— agrega un sufijo en vez de pisar el que estaba.
    /// </summary>
    private string NombreLibre(DateTime marca)
    {
        var baseNombre = $"{Prefijo}{marca:yyyyMMdd-HHmmss}";
        var candidato = Path.Combine(Carpeta, $"{baseNombre}.zip");

        for (var sufijo = 2; File.Exists(candidato) && sufijo < 100; sufijo++)
            candidato = Path.Combine(Carpeta, $"{baseNombre}-{sufijo}.zip");

        return candidato;
    }

    /// <summary>
    /// VACUUM INTO: la única forma correcta de copiar una base SQLite en uso.
    /// El archivo destino no puede existir, por eso se usa un nombre único.
    /// </summary>
    private void CopiarBase(string destino)
    {
        using var conexion = _factory.Crear();

        // La ruta va entre comillas simples y escapando las que traiga, porque
        // VACUUM INTO no acepta parámetros.
        var ruta = destino.Replace("'", "''");

        conexion.Execute($"VACUUM INTO '{ruta}';");

        if (!File.Exists(destino))
            throw new RespaldoFallidoException("La copia de la base no se generó.");
    }

    /// <summary>
    /// Abre la copia y le pide a SQLite que la revise entera. Es lo que convierte
    /// el archivo en un respaldo: si está dañado, se sabe hoy y no el día que se
    /// necesita restaurar.
    /// </summary>
    private static void Verificar(string archivo)
    {
        var cadena = new SqliteConnectionStringBuilder
        {
            DataSource = archivo,
            Mode = SqliteOpenMode.ReadOnly,
            // Sin pool: con el pool activado, cerrar la conexión devuelve el
            // handle al pool en vez de soltar el archivo, y después no se puede
            // borrar el temporal. Quedaba una copia huérfana del tamaño de la
            // base por cada respaldo.
            Pooling = false
        }.ToString();

        using var conexion = new SqliteConnection(cadena);
        conexion.Open();

        var resultado = conexion.ExecuteScalar<string>("PRAGMA integrity_check;");

        if (!string.Equals(resultado, "ok", StringComparison.OrdinalIgnoreCase))
            throw new RespaldoFallidoException(
                $"La copia salió dañada y se descartó. SQLite informó: {resultado}");

        // Que tenga las tablas que tiene que tener. Una base vacía pasa el
        // integrity_check y no sirve para restaurar nada.
        var tablas = conexion.ExecuteScalar<long>(
            "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'Ventas';");

        if (tablas == 0)
            throw new RespaldoFallidoException("La copia no tiene las tablas del sistema.");
    }

    /// <summary>
    /// Mete la base y los adjuntos en un zip. Van juntos porque un respaldo de la
    /// base sin los comprobantes adjuntos deja la mitad del archivo del negocio
    /// afuera, y encima uno solo es más fácil de copiar y de encontrar.
    /// </summary>
    private void Empaquetar(string baseCopiada, string destinoZip)
    {
        using var zip = ZipFile.Open(destinoZip, ZipArchiveMode.Create);

        zip.CreateEntryFromFile(baseCopiada, "cotillon.db", CompressionLevel.Optimal);

        if (!Directory.Exists(CarpetaAdjuntos))
            return;

        foreach (var archivo in Directory.GetFiles(CarpetaAdjuntos, "*", SearchOption.AllDirectories))
        {
            var relativo = Path.GetRelativePath(CarpetaAdjuntos, archivo);

            try
            {
                zip.CreateEntryFromFile(archivo, Path.Combine("Adjuntos", relativo).Replace('\\', '/'),
                    CompressionLevel.Optimal);
            }
            catch (IOException)
            {
                // Un adjunto en uso no puede frenar el respaldo de la base, que es
                // lo irreemplazable.
            }
        }
    }

    /// <summary>
    /// Deja una copia en la carpeta que sincroniza el servicio de nube. Se copia
    /// un zip ya cerrado, nunca la base en uso.
    /// </summary>
    private static bool CopiarALaNube(string zip, string? carpeta, out string? error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(carpeta))
            return false;

        try
        {
            Directory.CreateDirectory(carpeta);
            File.Copy(zip, Path.Combine(carpeta, Path.GetFileName(zip)), overwrite: true);

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Borra las copias más viejas. Sólo toca la carpeta local: lo que ya subió a
    /// la nube se deja, porque es la copia que sobrevive si se rompe este disco.
    /// </summary>
    private void Limpiar(int conservar)
    {
        if (conservar <= 0)
            return;

        foreach (var viejo in Listar().Skip(conservar))
            Borrar(Path.Combine(Carpeta, viejo.Archivo));
    }

    /// <summary>Las copias que hay, de la más nueva a la más vieja.</summary>
    public List<RespaldoDto> Listar()
    {
        if (!Directory.Exists(Carpeta))
            return [];

        return Directory.GetFiles(Carpeta, $"{Prefijo}*.zip")
            .Select(archivo => new FileInfo(archivo))
            .OrderByDescending(info => info.LastWriteTime)
            .Select(info => new RespaldoDto
            {
                Archivo = info.Name,
                Fecha = info.LastWriteTime,
                TamanoBytes = info.Length
            })
            .ToList();
    }

    public EstadoRespaldoDto Estado(ConfiguracionRespaldoDto configuracion)
    {
        var copias = Listar();

        if (!string.IsNullOrWhiteSpace(configuracion.CarpetaNube) && Directory.Exists(configuracion.CarpetaNube))
        {
            var enLaNube = Directory.GetFiles(configuracion.CarpetaNube, $"{Prefijo}*.zip")
                .Select(Path.GetFileName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var copia in copias)
                copia.EnLaNube = enLaNube.Contains(copia.Archivo);
        }

        return new EstadoRespaldoDto
        {
            Configuracion = configuracion,
            Copias = copias,
            Carpeta = Carpeta
        };
    }

    /// <summary>
    /// Si toca respaldar. Mira la fecha de la última copia que hay en disco y no
    /// una marca guardada en la base: si alguien borró las copias, corresponde
    /// hacer una nueva, aunque la base diga que ya se hizo.
    /// </summary>
    public bool Corresponde(ConfiguracionRespaldoDto configuracion)
    {
        if (!configuracion.Activo)
            return false;

        var copias = Listar();

        if (copias.Count == 0)
            return true;

        return (DateTime.Now.Date - copias[0].Fecha.Date).TotalDays >= configuracion.CadaDias;
    }

    private static void Borrar(string archivo)
    {
        try
        {
            if (File.Exists(archivo))
                File.Delete(archivo);
        }
        catch (IOException)
        {
            // Si no se puede borrar ahora, se intenta en el respaldo siguiente.
        }
    }

    private static string Legible(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:0.#} KB",
        _ => $"{bytes / (1024.0 * 1024):0.#} MB"
    };
}

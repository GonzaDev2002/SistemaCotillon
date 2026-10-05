namespace Cotillon.Shared.DTOs.Respaldos;

/// <summary>
/// Cada cuánto se respalda, cuántas copias se guardan y a qué carpeta se deja
/// una copia para que el servicio de nube la suba.
/// </summary>
public class ConfiguracionRespaldoDto
{
    public const string ClaveActivo = "BACKUP_ACTIVO";
    public const string ClaveCadaDias = "BACKUP_CADA_DIAS";
    public const string ClaveCopias = "BACKUP_COPIAS";
    public const string ClaveCarpetaNube = "BACKUP_CARPETA_NUBE";
    public const string ClaveUltimo = "BACKUP_ULTIMO";

    public bool Activo { get; set; } = true;

    /// <summary>1 = todos los días.</summary>
    public int CadaDias { get; set; } = 1;

    /// <summary>Cuántas copias se conservan antes de borrar las más viejas.</summary>
    public int Copias { get; set; } = 30;

    /// <summary>
    /// Carpeta sincronizada con la nube (OneDrive, Drive, Dropbox). Vacía: sólo
    /// queda la copia local.
    ///
    /// Se deja ACÁ una copia terminada y cerrada, no la base en uso. La base
    /// viviendo dentro de una carpeta sincronizada es la forma más común de
    /// corromper SQLite: el servicio la copia mientras el sistema escribe y
    /// queda a medias.
    /// </summary>
    public string CarpetaNube { get; set; } = string.Empty;

    public static ConfiguracionRespaldoDto Desde(IReadOnlyDictionary<string, string>? configuracion)
    {
        var datos = configuracion ?? new Dictionary<string, string>();

        string Valor(string clave) => datos.TryGetValue(clave, out var valor) ? valor.Trim() : string.Empty;

        return new ConfiguracionRespaldoDto
        {
            Activo = !bool.TryParse(Valor(ClaveActivo), out var activo) || activo,
            CadaDias = int.TryParse(Valor(ClaveCadaDias), out var dias) && dias > 0 ? dias : 1,
            Copias = int.TryParse(Valor(ClaveCopias), out var copias) && copias > 0 ? copias : 30,
            CarpetaNube = Valor(ClaveCarpetaNube)
        };
    }
}

/// <summary>Una copia que existe en disco.</summary>
public class RespaldoDto
{
    public string Archivo { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public long TamanoBytes { get; set; }
    public bool EnLaNube { get; set; }

    public string TamanoLegible => TamanoBytes switch
    {
        < 1024 => $"{TamanoBytes} B",
        < 1024 * 1024 => $"{TamanoBytes / 1024.0:0.#} KB",
        _ => $"{TamanoBytes / (1024.0 * 1024):0.#} MB"
    };

    public int DiasDeAntiguedad => (int)(DateTime.Now - Fecha).TotalDays;
}

public class EstadoRespaldoDto
{
    public ConfiguracionRespaldoDto Configuracion { get; set; } = new();
    public List<RespaldoDto> Copias { get; set; } = [];
    public string Carpeta { get; set; } = string.Empty;

    public DateTime? Ultimo => Copias.Count > 0 ? Copias[0].Fecha : null;

    public int? DiasSinRespaldo => Ultimo is { } fecha
        ? (int)(DateTime.Now.Date - fecha.Date).TotalDays
        : null;

    /// <summary>
    /// Si hay que avisar. Se tolera un día de atraso sobre la frecuencia elegida
    /// para no gritar por un respaldo que todavía está por correr hoy.
    /// </summary>
    public bool HayQueAvisar => Configuracion.Activo
        && (DiasSinRespaldo is null || DiasSinRespaldo > Configuracion.CadaDias);

    public string Aviso => !Configuracion.Activo
        ? "Los respaldos automáticos están apagados."
        : DiasSinRespaldo switch
        {
            null => "Nunca se hizo un respaldo. Si se rompe el disco, se pierde todo.",
            0 => "Respaldado hoy.",
            1 => "El último respaldo es de ayer.",
            var dias => $"Hace {dias} días que no hay respaldo."
        };
}

public class ResultadoRespaldoDto
{
    public bool Exito { get; set; }
    public string Archivo { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public bool CopiadoALaNube { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}

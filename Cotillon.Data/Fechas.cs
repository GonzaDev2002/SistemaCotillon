using System.Globalization;

namespace Cotillon.Data;

/// <summary>
/// Las fechas se guardan como texto ISO-8601 local, así que los rangos de consulta
/// tienen que compararse con ese mismo formato.
/// </summary>
public static class Fechas
{
    public const string Formato = "yyyy-MM-dd HH:mm:ss";

    public static string Texto(DateTime fecha) => fecha.ToString(Formato, CultureInfo.InvariantCulture);

    public static string Ahora() => Texto(DateTime.Now);

    /// <summary>Primer instante del día.</summary>
    public static string Inicio(DateTime fecha) => Texto(fecha.Date);

    /// <summary>Último instante del día, para rangos con BETWEEN.</summary>
    public static string Fin(DateTime fecha) => Texto(fecha.Date.AddDays(1).AddSeconds(-1));
}

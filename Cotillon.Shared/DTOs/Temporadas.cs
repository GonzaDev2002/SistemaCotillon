namespace Cotillon.Shared.DTOs;

/// <summary>
/// Las fechas que mueven un cotillón. La temporada no cambia cómo se vende el
/// producto: sirve para comparar contra el mismo momento del año pasado, que es
/// la única comparación útil en un negocio estacional. Un diciembre no se mide
/// contra noviembre, se mide contra el diciembre anterior.
/// </summary>
public static class Temporadas
{
    public const string TodoElAno = "";

    public static readonly IReadOnlyList<(string Codigo, string Nombre)> Todas =
    [
        (TodoElAno, "Todo el año"),
        ("CUMPLEANOS", "Cumpleaños"),
        ("CARNAVAL", "Carnaval"),
        ("PASCUAS", "Pascuas"),
        ("DIA_DEL_NINO", "Día del Niño"),
        ("HALLOWEEN", "Halloween"),
        ("NAVIDAD", "Navidad y Año Nuevo"),
        ("PATRIAS", "Fechas patrias"),
        ("CASAMIENTO", "Casamientos"),
        ("BABY_SHOWER", "Baby shower")
    ];

    public static string Nombre(string? codigo) =>
        Todas.FirstOrDefault(t => string.Equals(t.Codigo, codigo ?? string.Empty,
            StringComparison.OrdinalIgnoreCase)).Nombre ?? "Todo el año";
}

namespace Cotillon.Shared.DTOs.Reportes;

/// <summary>
/// Lo que se vendió de una temporada contra lo mismo del año pasado. Es la única
/// comparación que sirve en un negocio estacional: un diciembre no se mide
/// contra noviembre, se mide contra el diciembre anterior.
/// </summary>
public class ReporteTemporadaDto
{
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string Temporada { get; set; } = string.Empty;

    public decimal CantidadEsteAno { get; set; }
    public decimal ImporteEsteAno { get; set; }
    public decimal CantidadAnoPasado { get; set; }
    public decimal ImporteAnoPasado { get; set; }

    /// <summary>Stock de hoy, para decidir cuánto falta comprar.</summary>
    public decimal Stock { get; set; }

    /// <summary>
    /// Cuánto más o menos se vendió que el año pasado. Null cuando el año pasado
    /// no se vendió nada: ahí el porcentaje no significa nada y mostrar un
    /// "+100 %" sería inventar una comparación que no existe.
    /// </summary>
    public decimal? VariacionPorcentual => CantidadAnoPasado > 0
        ? (CantidadEsteAno - CantidadAnoPasado) / CantidadAnoPasado * 100m
        : null;

    public string Variacion => VariacionPorcentual is { } porcentaje
        ? $"{(porcentaje >= 0 ? "+" : string.Empty)}{porcentaje:0.#} %"
        : CantidadEsteAno > 0 ? "nuevo" : "—";
}

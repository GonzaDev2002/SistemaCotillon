namespace Cotillon.Shared.DTOs;

/// <summary>
/// Un renglón de la receta del combo: qué lleva la bolsita y cuánto de cada cosa.
/// </summary>
public class ComponenteComboDto
{
    public int ComponenteProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public decimal Cantidad { get; set; } = 1;
    public string UnidadMedida { get; set; } = UnidadesMedida.Unidad;

    /// <summary>Costo unitario del componente, para saber cuánto sale armar el combo.</summary>
    public decimal Costo { get; set; }

    /// <summary>Stock del componente, para mostrar cuántos combos se pueden armar.</summary>
    public decimal StockComponente { get; set; }

    public decimal CostoLinea => Cantidad * Costo;

    public string CantidadFormateada => UnidadesMedida.De(UnidadMedida).Formatear(Cantidad);

    /// <summary>
    /// Cuántos combos alcanza a armar este componente. Es el número que limita al
    /// combo entero: alcanza con que falte uno para que no se pueda armar ninguno.
    /// </summary>
    public decimal CombosQueAlcanza => Cantidad > 0
        ? Math.Floor(StockComponente / Cantidad)
        : 0;
}

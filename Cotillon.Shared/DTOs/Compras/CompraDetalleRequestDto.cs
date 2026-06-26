namespace Cotillon.Shared.DTOs.Compras;

public class CompraDetalleRequestDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal SubtotalLinea { get; set; }
}

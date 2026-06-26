namespace Cotillon.Shared.DTOs.Ventas;

public class DetalleVentaRequestDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}
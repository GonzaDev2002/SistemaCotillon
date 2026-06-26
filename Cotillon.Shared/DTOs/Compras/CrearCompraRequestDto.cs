namespace Cotillon.Shared.DTOs.Compras;

public class CrearCompraRequestDto
{
    public int ProveedorId { get; set; }
    public int UsuarioId { get; set; }
    public string NumeroComprobante { get; set; } = "";
    public decimal Subtotal { get; set; }
    public decimal DescuentoTotal { get; set; }
    public decimal TotalCompra { get; set; }
    public string Estado { get; set; } = "Confirmada";
    public string Observaciones { get; set; } = "";
    public List<CompraDetalleRequestDto> Detalles { get; set; } = new();
}

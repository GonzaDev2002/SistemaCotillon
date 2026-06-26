namespace Cotillon.Shared.DTOs.Compras;

public class CompraListadoDto
{
    public int CompraId { get; set; }
    public DateTime FechaCompra { get; set; }
    public string Proveedor { get; set; } = "";
    public string Usuario { get; set; } = "";
    public string NumeroComprobante { get; set; } = "";
    public decimal TotalCompra { get; set; }
    public string Estado { get; set; } = "";
}

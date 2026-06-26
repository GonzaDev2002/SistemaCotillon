namespace Cotillon.Shared.DTOs.Ventas;

public class VentaListadoDto
{
    public int VentaId { get; set; }
    public DateTime FechaVenta { get; set; }
    public string Cliente { get; set; } = "";
    public string Usuario { get; set; } = "";
    public decimal TotalVenta { get; set; }
    public string Estado { get; set; } = "";
}
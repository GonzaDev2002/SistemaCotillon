namespace Cotillon.Shared.DTOs.Reportes;

public class ReporteVentaDto
{
    public int VentaId { get; set; }
    public DateTime FechaVenta { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public decimal TotalVenta { get; set; }
    public string Estado { get; set; } = string.Empty;
}
namespace Cotillon.Shared.DTOs.Reportes;

public class ReporteProductoDto
{
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string Seccion { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public decimal Stock { get; set; }
    public decimal StockMinimo { get; set; }
    public bool Activo { get; set; }
}
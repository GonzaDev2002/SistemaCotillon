namespace Cotillon.Shared.DTOs;

public class ProductoDashboardDto
{
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string NombreSeccion { get; set; } = string.Empty;
    public string NombreCategoria { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public decimal Stock { get; set; }
    public decimal StockMinimo { get; set; }
    public bool Activo { get; set; }
    public DateTime? FechaAlta { get; set; }
}
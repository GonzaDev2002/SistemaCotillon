namespace Cotillon.Shared.DTOs;

public class ProductoDto
{
    public int ProductoId { get; set; }
    public int CategoriaId { get; set; }
    public string CodigoProducto { get; set; } = string.Empty;
    public string NombreProducto { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
    public string CodigoBarras { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public decimal Costo { get; set; }
    public string ImagenUrl { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = string.Empty;
    public bool PermiteVentaSinStock { get; set; }
}
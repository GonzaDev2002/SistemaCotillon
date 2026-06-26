namespace Cotillon.Shared.DTOs.Estadisticas;

public class EstadisticaBajoStockDto
{
    public string Producto { get; set; } = string.Empty;
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
}
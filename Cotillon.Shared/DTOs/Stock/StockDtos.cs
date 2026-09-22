namespace Cotillon.Shared.DTOs.Stock;

public class MovimientoStockDto
{
    public int StockMovimientoId { get; set; }
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public DateTime FechaMovimiento { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal StockAnterior { get; set; }
    public decimal StockNuevo { get; set; }
    public string? OrigenTipo { get; set; }
    public int? OrigenId { get; set; }
    public string? Observaciones { get; set; }
}

public class AjustarStockDto
{
    public int UsuarioId { get; set; }
    public int ProductoId { get; set; }
    /// <summary>AJUSTE, MERMA o DEVOLUCION.</summary>
    public string TipoMovimiento { get; set; } = "AJUSTE";
    /// <summary>Positivo suma stock, negativo lo descuenta.</summary>
    public decimal Cantidad { get; set; }
    public string Motivo { get; set; } = string.Empty;
}

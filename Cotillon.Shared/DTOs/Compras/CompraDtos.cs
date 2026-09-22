namespace Cotillon.Shared.DTOs.Compras;

public class CrearCompraDto
{
    public int UsuarioId { get; set; }
    public int ProveedorId { get; set; }
    public string? NumeroComprobante { get; set; }
    public decimal DescuentoTotal { get; set; }
    public string? Observaciones { get; set; }
    /// <summary>Si es true, la compra ingresa el stock al confirmarse.</summary>
    public bool RecibirAhora { get; set; } = true;
    /// <summary>Actualiza el costo del producto con el de esta compra.</summary>
    public bool ActualizarCostos { get; set; } = true;
    public List<CrearLineaCompraDto> Lineas { get; set; } = [];
}

public class CrearLineaCompraDto
{
    public int ProductoId { get; set; }
    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
}

public class CompraDto
{
    public int CompraId { get; set; }
    public int ProveedorId { get; set; }
    public string Proveedor { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public DateTime FechaCompra { get; set; }
    public string? NumeroComprobante { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DescuentoTotal { get; set; }
    public decimal TotalCompra { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public List<LineaCompraDto> Lineas { get; set; } = [];
}

public class LineaCompraDto
{
    public int DetalleCompraId { get; set; }
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal SubtotalLinea { get; set; }
}

public class CompraResumenDto
{
    public int CompraId { get; set; }
    public DateTime FechaCompra { get; set; }
    public string Proveedor { get; set; } = string.Empty;
    public string? NumeroComprobante { get; set; }
    public int CantidadItems { get; set; }
    public decimal TotalCompra { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class RecibirCompraDto
{
    public int UsuarioId { get; set; }
    public bool ActualizarCostos { get; set; } = true;
}

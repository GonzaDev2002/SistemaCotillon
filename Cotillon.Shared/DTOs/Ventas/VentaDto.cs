namespace Cotillon.Shared.DTOs.Ventas;

using Cotillon.Shared.DTOs;

public class CrearVentaDto
{
    public int UsuarioId { get; set; }
    public int? ClienteId { get; set; }
    public string TipoComprobante { get; set; } = "TICKET";
    public decimal DescuentoTotal { get; set; }
    public string? Observaciones { get; set; }
    public List<CrearLineaVentaDto> Lineas { get; set; } = [];
    public List<CrearPagoVentaDto> Pagos { get; set; } = [];
}

public class CrearLineaVentaDto
{
    public int ProductoId { get; set; }
    public decimal Cantidad { get; set; }
    public decimal DescuentoLinea { get; set; }
}

public class CrearPagoVentaDto
{
    public int MedioPagoId { get; set; }
    public decimal Importe { get; set; }
    public string? Referencia { get; set; }
}

public class VentaDto
{
    public int VentaId { get; set; }
    public int? ClienteId { get; set; }
    public string Cliente { get; set; } = "Consumidor final";
    public int UsuarioId { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public int SesionCajaId { get; set; }
    public DateTime FechaVenta { get; set; }
    public string TipoComprobante { get; set; } = string.Empty;
    public string? NumeroComprobante { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DescuentoTotal { get; set; }
    public decimal TotalVenta { get; set; }
    public decimal Vuelto { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public List<LineaVentaDto> Lineas { get; set; } = [];
    public List<PagoVentaDto> Pagos { get; set; } = [];
}

public class LineaVentaDto
{
    public int DetalleVentaId { get; set; }
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal DescuentoLinea { get; set; }
    public decimal SubtotalLinea { get; set; }

    /// <summary>
    /// Cómo se vendió, para que el ticket diga "0,35 kg" y no "0,35" a secas.
    /// Sale del producto, no del detalle: si el producto ya no está, cae en unidad.
    /// </summary>
    public string UnidadMedida { get; set; } = UnidadesMedida.Unidad;
}

public class PagoVentaDto
{
    public int PagoVentaId { get; set; }
    public int MedioPagoId { get; set; }
    public string MedioPago { get; set; } = string.Empty;
    public decimal Importe { get; set; }
    public decimal Vuelto { get; set; }
    public string? Referencia { get; set; }
}

public class AnularVentaDto
{
    public int UsuarioId { get; set; }
    public string Motivo { get; set; } = string.Empty;
}

public class VentaResumenDto
{
    public int VentaId { get; set; }
    public DateTime FechaVenta { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public int CantidadItems { get; set; }
    public decimal TotalVenta { get; set; }
    public string Estado { get; set; } = string.Empty;
}

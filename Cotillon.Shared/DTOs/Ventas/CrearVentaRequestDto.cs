namespace Cotillon.Shared.DTOs.Ventas;

public class CrearVentaRequestDto
{
    public int SesionCajaId { get; set; }
    public int? ClienteId { get; set; }
    public int UsuarioId { get; set; }
    public int MedioPagoId { get; set; }
    public string TipoComprobante { get; set; } = "Ticket";
    public string NumeroComprobante { get; set; } = "";
    public decimal DescuentoTotal { get; set; }
    public List<DetalleVentaRequestDto> Detalles { get; set; } = new();
}
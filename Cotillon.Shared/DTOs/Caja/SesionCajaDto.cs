namespace Cotillon.Shared.DTOs.Caja;

public class SesionCajaDto
{
    public int SesionCajaId { get; set; }
    public int UsuarioId { get; set; }
    public DateTime FechaApertura { get; set; }
    public DateTime? FechaCierre { get; set; }
    public decimal MontoInicial { get; set; }
    public decimal MontoEsperado { get; set; }
    public decimal MontoContado { get; set; }
    public decimal Diferencia { get; set; }
    public string Estado { get; set; } = "";
    public string Observaciones { get; set; } = "";
}
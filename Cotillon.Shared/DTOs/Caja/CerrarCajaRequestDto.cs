namespace Cotillon.Shared.DTOs.Caja;

public class CerrarCajaRequestDto
{
    public int SesionCajaId { get; set; }

    public decimal MontoContado { get; set; }

    public string Observaciones { get; set; } = "";
}
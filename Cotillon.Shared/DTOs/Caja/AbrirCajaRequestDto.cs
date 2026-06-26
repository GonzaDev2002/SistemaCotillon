namespace Cotillon.Shared.DTOs.Caja;

public class AbrirCajaRequestDto
{
    public int UsuarioId { get; set; }
    public decimal MontoInicial { get; set; }
    public string Observaciones { get; set; } = "";
}
namespace Cotillon.Shared.DTOs.Auditoria;

public class EventoAuditoriaDto
{
    public int AuditoriaId { get; set; }
    public int? UsuarioId { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public DateTime FechaEvento { get; set; }
    public string TablaAfectada { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public int? RegistroId { get; set; }
    public string? Detalle { get; set; }
    public string? Terminal { get; set; }
}

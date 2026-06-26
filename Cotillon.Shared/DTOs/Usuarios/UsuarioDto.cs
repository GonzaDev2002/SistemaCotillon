namespace Cotillon.Shared.DTOs.Usuarios;

public class UsuarioDto
{
    public int UsuarioId { get; set; }
    public string NombreUsuario { get; set; } = "";
    public string NombreCompleto { get; set; } = "";
    public string Pin { get; set; } = "";
    public string Rol { get; set; } = "Trabajador";
    public bool Activo { get; set; } = true;
    public DateTime FechaAlta { get; set; }
    public DateTime FechaActualizacion { get; set; }
}
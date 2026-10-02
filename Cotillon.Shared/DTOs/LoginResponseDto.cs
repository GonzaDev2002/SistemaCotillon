using Cotillon.Shared.DTOs.Permisos;

namespace Cotillon.Shared.DTOs;

public class LoginResponseDto
{
    public int UsuarioId { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;

    /// <summary>
    /// Qué puede hacer en cada módulo. Viaja con el login para que la pantalla
    /// arme el menú de entrada, sin una consulta más por cada módulo.
    /// </summary>
    public Dictionary<string, NivelAcceso> Permisos { get; set; } = [];
}
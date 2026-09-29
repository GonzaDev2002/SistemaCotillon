namespace Cotillon.Shared.DTOs.Usuarios;

public class UsuarioDto
{
    public int UsuarioId { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Rol { get; set; } = "Cajero";
    public bool Activo { get; set; } = true;
    public DateTime FechaAlta { get; set; }
    public DateTime? UltimoAcceso { get; set; }
}

public class CrearUsuarioDto
{
    public string NombreUsuario { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Pin { get; set; } = string.Empty;
    public string Rol { get; set; } = "Cajero";
    public bool Activo { get; set; } = true;
}

public class CambiarPinDto
{
    public int UsuarioId { get; set; }
    public string PinActual { get; set; } = string.Empty;
    public string PinNuevo { get; set; } = string.Empty;
}

public class MedioPagoDto
{
    public int MedioPagoId { get; set; }
    public string NombreMedioPago { get; set; } = string.Empty;
    public bool AfectaEfectivo { get; set; }
    public int OrdenVisual { get; set; }
    public bool Activo { get; set; } = true;
}

public class ConfiguracionItemDto
{
    public string Clave { get; set; } = string.Empty;
    public string? Valor { get; set; }
    public string? Descripcion { get; set; }
}

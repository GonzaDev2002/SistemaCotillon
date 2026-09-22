namespace Cotillon.Shared.DTOs.Terceros;

public class ClienteDto
{
    public int ClienteId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Documento { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? Observaciones { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaAlta { get; set; }
}

public class ProveedorDto
{
    public int ProveedorId { get; set; }
    public string NombreProveedor { get; set; } = string.Empty;
    public string? Cuit { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? Observaciones { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaAlta { get; set; }
}

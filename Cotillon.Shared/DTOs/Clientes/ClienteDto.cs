namespace Cotillon.Shared.DTOs.Clientes;

public class ClienteDto
{
    public int ClienteId { get; set; }
    public string NombreCompleto { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Email { get; set; } = "";
    public string Direccion { get; set; } = "";
    public string Documento { get; set; } = "";
    public string Observaciones { get; set; } = "";
    public bool Activo { get; set; } = true;
    public DateTime FechaAlta { get; set; }
    public DateTime FechaActualizacion { get; set; }
}
namespace Cotillon.Shared.DTOs.Proveedores;

public class ProveedorDto
{
    public int ProveedorId { get; set; }
    public string NombreProveedor { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Email { get; set; } = "";
    public string Direccion { get; set; } = "";
    public string Cuit { get; set; } = "";
    public string Observaciones { get; set; } = "";
    public bool Activo { get; set; } = true;
}

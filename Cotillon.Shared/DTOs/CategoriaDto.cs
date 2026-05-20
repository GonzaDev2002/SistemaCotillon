namespace Cotillon.Shared.DTOs;

public class CategoriaDto
{
    public int CategoriaId { get; set; }
    public int SeccionId { get; set; }
    public string NombreCategoria { get; set; } = string.Empty;
    public bool Activo { get; set; }

}
namespace Cotillon.Shared.DTOs;

public class CategoriaDto
{
    public int CategoriaId { get; set; }
    public int SeccionId { get; set; }
    public string NombreCategoria { get; set; } = string.Empty;
    public bool Activo { get; set; }

    /// <summary>Sección a la que pertenece, para poder listar todas las categorías juntas.</summary>
    public string NombreSeccion { get; set; } = string.Empty;

    public int CantidadProductos { get; set; }
}

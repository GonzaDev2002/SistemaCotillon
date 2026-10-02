namespace Cotillon.Shared.DTOs;

public class SeccionDto
{
    public int SeccionId { get; set; }
    public string NombreSeccion { get; set; } = string.Empty;
    public bool Activo { get; set; }

    /// <summary>
    /// Cuántas categorías y productos cuelgan de la sección. Se calculan en la
    /// misma consulta para que la pantalla no tenga que pedir una lista por fila,
    /// y para que al renombrar o desactivar se vea de entrada qué se está tocando.
    /// </summary>
    public int CantidadCategorias { get; set; }

    public int CantidadProductos { get; set; }
}

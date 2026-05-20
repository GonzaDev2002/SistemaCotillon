namespace Cotillon.Shared.DTOs;

public class ProductoListadoDto
{
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string NombreSeccion { get; set; } = string.Empty;
    public string NombreCategoria { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public bool Activo { get; set; }
}
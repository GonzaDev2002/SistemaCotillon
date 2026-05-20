namespace Cotillon.Shared.DTOs;

public class DashboardResumenDto
{
    public int TotalProductos { get; set; }
    public int ProductosActivos { get; set; }
    public int ProductosInactivos { get; set; }
    public int ProductosBajoStock { get; set; }
}
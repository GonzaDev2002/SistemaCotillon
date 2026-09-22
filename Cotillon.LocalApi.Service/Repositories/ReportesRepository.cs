using Cotillon.Data;
using Cotillon.Shared.DTOs.Reportes;
using Dapper;

namespace Cotillon.LocalApi.Service.Repositories;

public class ReportesRepository
{
    private const string FormatoFecha = "yyyy-MM-dd HH:mm:ss";

    private readonly IDbConnectionFactory _factory;

    public ReportesRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public List<ReporteProductoDto> ListarProductos() => ConsultarProductos(soloBajoStock: false);

    public List<ReporteProductoDto> ListarProductosBajoStock() => ConsultarProductos(soloBajoStock: true);

    private List<ReporteProductoDto> ConsultarProductos(bool soloBajoStock)
    {
        using var conexion = _factory.Crear();

        var filas = conexion.Query<FilaProducto>(
            $"""
            SELECT
                p.ProductoId,
                p.NombreProducto,
                COALESCE(s.NombreSeccion, '(Sin sección)')     AS Seccion,
                COALESCE(c.NombreCategoria, '(Sin categoría)') AS Categoria,
                p.PrecioCentavos,
                p.Stock,
                p.StockMinimo,
                p.Activo
            FROM Productos p
            LEFT JOIN Categorias c ON p.CategoriaId = c.CategoriaId
            LEFT JOIN Secciones  s ON c.SeccionId   = s.SeccionId
            {(soloBajoStock ? "WHERE p.Activo = 1 AND p.Stock <= p.StockMinimo" : string.Empty)}
            ORDER BY {(soloBajoStock ? "p.Stock ASC" : "p.NombreProducto")};
            """);

        return filas.Select(f => new ReporteProductoDto
        {
            ProductoId = f.ProductoId,
            NombreProducto = f.NombreProducto,
            Seccion = f.Seccion,
            Categoria = f.Categoria,
            Precio = Money.FromCentavos(f.PrecioCentavos),
            Stock = (int)f.Stock,
            StockMinimo = (int)f.StockMinimo,
            Activo = f.Activo
        }).ToList();
    }

    public List<ReporteVentaDto> ListarVentasPorRango(DateTime desde, DateTime hasta)
    {
        using var conexion = _factory.Crear();

        var filas = conexion.Query<FilaVenta>(
            """
            SELECT
                v.VentaId,
                v.FechaVenta,
                COALESCE(cl.NombreCompleto, 'Consumidor final') AS Cliente,
                COALESCE(u.NombreCompleto, '')                  AS Usuario,
                v.TotalVentaCentavos,
                v.Estado
            FROM Ventas v
            LEFT JOIN Clientes cl ON v.ClienteId = cl.ClienteId
            LEFT JOIN Usuarios u  ON v.UsuarioId = u.UsuarioId
            WHERE v.FechaVenta BETWEEN @Desde AND @Hasta
            ORDER BY v.FechaVenta DESC;
            """,
            new
            {
                Desde = desde.Date.ToString(FormatoFecha),
                Hasta = hasta.Date.AddDays(1).AddSeconds(-1).ToString(FormatoFecha)
            });

        return filas.Select(f => new ReporteVentaDto
        {
            VentaId = f.VentaId,
            FechaVenta = f.FechaVenta,
            Cliente = f.Cliente,
            Usuario = f.Usuario,
            TotalVenta = Money.FromCentavos(f.TotalVentaCentavos),
            Estado = f.Estado
        }).ToList();
    }

    private sealed class FilaProducto
    {
        public int ProductoId { get; init; }
        public string NombreProducto { get; init; } = string.Empty;
        public string Seccion { get; init; } = string.Empty;
        public string Categoria { get; init; } = string.Empty;
        public long PrecioCentavos { get; init; }
        public double Stock { get; init; }
        public double StockMinimo { get; init; }
        public bool Activo { get; init; }
    }

    private sealed class FilaVenta
    {
        public int VentaId { get; init; }
        public DateTime FechaVenta { get; init; }
        public string Cliente { get; init; } = string.Empty;
        public string Usuario { get; init; } = string.Empty;
        public long TotalVentaCentavos { get; init; }
        public string Estado { get; init; } = string.Empty;
    }
}

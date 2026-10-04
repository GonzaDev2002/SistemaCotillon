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

    /// <summary>
    /// Qué se vendió de una temporada, comparado con la misma temporada del año
    /// anterior. La ventana es la misma franja de días corrida un año: comparar
    /// contra "todo el año pasado" daría siempre que este año se vendió menos.
    /// </summary>
    public List<ReporteTemporadaDto> Temporada(string? temporada, DateTime desde, DateTime hasta)
    {
        using var conexion = _factory.Crear();

        var filas = conexion.Query<FilaTemporada>(
            """
            SELECT
                p.ProductoId,
                p.NombreProducto,
                COALESCE(p.Temporada, '') AS Temporada,
                p.Stock,
                COALESCE(SUM(CASE WHEN v.FechaVenta BETWEEN @Desde    AND @Hasta    THEN d.Cantidad END), 0)             AS CantidadEsteAno,
                COALESCE(SUM(CASE WHEN v.FechaVenta BETWEEN @Desde    AND @Hasta    THEN d.SubtotalLineaCentavos END), 0) AS ImporteEsteAnoCentavos,
                COALESCE(SUM(CASE WHEN v.FechaVenta BETWEEN @DesdeAnt AND @HastaAnt THEN d.Cantidad END), 0)             AS CantidadAnoPasado,
                COALESCE(SUM(CASE WHEN v.FechaVenta BETWEEN @DesdeAnt AND @HastaAnt THEN d.SubtotalLineaCentavos END), 0) AS ImporteAnoPasadoCentavos
            FROM Productos p
            LEFT JOIN DetalleVentas d ON d.ProductoId = p.ProductoId
            LEFT JOIN Ventas v        ON d.VentaId    = v.VentaId AND v.Estado = 'COMPLETADA'
            WHERE (@Temporada IS NULL OR COALESCE(p.Temporada, '') = @Temporada)
            GROUP BY p.ProductoId, p.NombreProducto, p.Temporada, p.Stock
            HAVING CantidadEsteAno > 0 OR CantidadAnoPasado > 0
            ORDER BY ImporteEsteAnoCentavos DESC, p.NombreProducto;
            """,
            new
            {
                Temporada = string.IsNullOrWhiteSpace(temporada) ? null : temporada.Trim(),
                Desde = Fechas.Inicio(desde),
                Hasta = Fechas.Fin(hasta),
                DesdeAnt = Fechas.Inicio(desde.AddYears(-1)),
                HastaAnt = Fechas.Fin(hasta.AddYears(-1))
            });

        return filas.Select(f => new ReporteTemporadaDto
        {
            ProductoId = f.ProductoId,
            NombreProducto = f.NombreProducto,
            Temporada = f.Temporada,
            Stock = (decimal)f.Stock,
            CantidadEsteAno = (decimal)f.CantidadEsteAno,
            ImporteEsteAno = Money.FromCentavos(f.ImporteEsteAnoCentavos),
            CantidadAnoPasado = (decimal)f.CantidadAnoPasado,
            ImporteAnoPasado = Money.FromCentavos(f.ImporteAnoPasadoCentavos)
        }).ToList();
    }

    private sealed class FilaTemporada
    {
        public int ProductoId { get; init; }
        public string NombreProducto { get; init; } = string.Empty;
        public string Temporada { get; init; } = string.Empty;
        public double Stock { get; init; }
        public double CantidadEsteAno { get; init; }
        public long ImporteEsteAnoCentavos { get; init; }
        public double CantidadAnoPasado { get; init; }
        public long ImporteAnoPasadoCentavos { get; init; }
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

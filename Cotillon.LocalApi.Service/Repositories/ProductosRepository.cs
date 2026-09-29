using Cotillon.Data;
using Cotillon.Shared.DTOs;
using Dapper;

namespace Cotillon.LocalApi.Service.Repositories;

public class ProductosRepository
{
    private readonly IDbConnectionFactory _factory;

    public ProductosRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public bool Insertar(ProductoDto producto)
    {
        using var conexion = _factory.Crear();

        return conexion.Execute(
            """
            INSERT INTO Productos
                (CategoriaId, CodigoProducto, NombreProducto, Descripcion, PrecioCentavos, CostoCentavos,
                 Stock, StockMinimo, CodigoBarras, Marca, UnidadMedida, ImagenUrl, PermiteVentaSinStock, Activo)
            VALUES
                (@CategoriaId, @CodigoProducto, @NombreProducto, @Descripcion, @Precio, @Costo,
                 @Stock, @StockMinimo, @CodigoBarras, @Marca, @UnidadMedida, @ImagenUrl, @PermiteVentaSinStock, 1);
            """,
            AParametros(producto)) > 0;
    }

    public bool Actualizar(ProductoDto producto)
    {
        using var conexion = _factory.Crear();

        return conexion.Execute(
            """
            UPDATE Productos SET
                CategoriaId = @CategoriaId,
                CodigoProducto = @CodigoProducto,
                NombreProducto = @NombreProducto,
                Descripcion = @Descripcion,
                PrecioCentavos = @Precio,
                CostoCentavos = @Costo,
                StockMinimo = @StockMinimo,
                CodigoBarras = @CodigoBarras,
                Marca = @Marca,
                UnidadMedida = @UnidadMedida,
                ImagenUrl = @ImagenUrl,
                PermiteVentaSinStock = @PermiteVentaSinStock,
                FechaActualizacion = datetime('now', 'localtime')
            WHERE ProductoId = @ProductoId;
            """,
            AParametros(producto)) > 0;
    }

    public List<ProductoListadoDto> ObtenerTodos(bool incluirInactivos)
    {
        using var conexion = _factory.Crear();

        var filas = conexion.Query<FilaListado>(
            $"""
            SELECT
                p.ProductoId,
                p.NombreProducto,
                p.PrecioCentavos,
                p.Stock,
                p.Activo,
                COALESCE(c.NombreCategoria, '(Sin categoría)') AS NombreCategoria,
                COALESCE(s.NombreSeccion, '(Sin sección)')     AS NombreSeccion
            FROM Productos p
            LEFT JOIN Categorias c ON p.CategoriaId = c.CategoriaId
            LEFT JOIN Secciones  s ON c.SeccionId   = s.SeccionId
            {(incluirInactivos ? string.Empty : "WHERE p.Activo = 1")}
            ORDER BY p.NombreProducto;
            """);

        return filas.Select(f => new ProductoListadoDto
        {
            ProductoId = f.ProductoId,
            NombreProducto = f.NombreProducto,
            NombreSeccion = f.NombreSeccion,
            NombreCategoria = f.NombreCategoria,
            Precio = Money.FromCentavos(f.PrecioCentavos),
            Stock = (int)f.Stock,
            Activo = f.Activo
        }).ToList();
    }

    /// <summary>
    /// Búsqueda para la pantalla de ventas: código exacto primero (es lo que manda
    /// el lector de código de barras) y después coincidencia por nombre o marca.
    /// </summary>
    public List<ProductoListadoDto> Buscar(string texto, int limite)
    {
        using var conexion = _factory.Crear();

        var filas = conexion.Query<FilaListado>(
            """
            SELECT
                p.ProductoId,
                p.NombreProducto,
                p.PrecioCentavos,
                p.Stock,
                p.Activo,
                COALESCE(c.NombreCategoria, '(Sin categoría)') AS NombreCategoria,
                COALESCE(s.NombreSeccion, '(Sin sección)')     AS NombreSeccion
            FROM Productos p
            LEFT JOIN Categorias c ON p.CategoriaId = c.CategoriaId
            LEFT JOIN Secciones  s ON c.SeccionId   = s.SeccionId
            WHERE p.Activo = 1
              AND (p.CodigoBarras = @Exacto
                OR p.CodigoProducto = @Exacto
                OR p.NombreProducto LIKE @Parcial
                OR p.Marca LIKE @Parcial)
            ORDER BY
                CASE WHEN p.CodigoBarras = @Exacto OR p.CodigoProducto = @Exacto THEN 0 ELSE 1 END,
                p.NombreProducto
            LIMIT @Limite;
            """,
            new { Exacto = texto, Parcial = $"%{texto}%", Limite = limite });

        return filas.Select(f => new ProductoListadoDto
        {
            ProductoId = f.ProductoId,
            NombreProducto = f.NombreProducto,
            NombreSeccion = f.NombreSeccion,
            NombreCategoria = f.NombreCategoria,
            Precio = Money.FromCentavos(f.PrecioCentavos),
            Stock = (int)f.Stock,
            Activo = f.Activo
        }).ToList();
    }

    public ProductoDto? ObtenerPorId(int productoId)
    {
        using var conexion = _factory.Crear();

        var fila = conexion.QuerySingleOrDefault<FilaProducto>(
            """
            SELECT ProductoId, CategoriaId, CodigoProducto, NombreProducto, Descripcion,
                   PrecioCentavos, CostoCentavos, Stock, StockMinimo, CodigoBarras, Marca,
                   UnidadMedida, ImagenUrl, PermiteVentaSinStock
            FROM Productos
            WHERE ProductoId = @Id;
            """,
            new { Id = productoId });

        if (fila is null)
            return null;

        return new ProductoDto
        {
            ProductoId = fila.ProductoId,
            CategoriaId = fila.CategoriaId,
            CodigoProducto = fila.CodigoProducto ?? string.Empty,
            NombreProducto = fila.NombreProducto,
            Descripcion = fila.Descripcion ?? string.Empty,
            Precio = Money.FromCentavos(fila.PrecioCentavos),
            Costo = Money.FromCentavos(fila.CostoCentavos),
            Stock = (int)fila.Stock,
            StockMinimo = (int)fila.StockMinimo,
            CodigoBarras = fila.CodigoBarras ?? string.Empty,
            Marca = fila.Marca ?? string.Empty,
            UnidadMedida = fila.UnidadMedida,
            ImagenUrl = fila.ImagenUrl ?? string.Empty,
            PermiteVentaSinStock = fila.PermiteVentaSinStock
        };
    }

    public bool EliminarLogico(int productoId)
    {
        using var conexion = _factory.Crear();

        return conexion.Execute(
            """
            UPDATE Productos
            SET Activo = 0, FechaActualizacion = datetime('now', 'localtime')
            WHERE ProductoId = @ProductoId;
            """,
            new { ProductoId = productoId }) > 0;
    }

    public DashboardResumenDto ObtenerResumenDashboard()
    {
        using var conexion = _factory.Crear();

        return conexion.QuerySingle<DashboardResumenDto>(
            """
            SELECT
                COUNT(*)                                                                AS TotalProductos,
                COALESCE(SUM(CASE WHEN Activo = 1 THEN 1 ELSE 0 END), 0)                AS ProductosActivos,
                COALESCE(SUM(CASE WHEN Activo = 0 THEN 1 ELSE 0 END), 0)                AS ProductosInactivos,
                COALESCE(SUM(CASE WHEN Activo = 1 AND Stock <= StockMinimo THEN 1 ELSE 0 END), 0) AS ProductosBajoStock
            FROM Productos;
            """);
    }

    public List<ProductoDashboardDto> ObtenerProductosBajoStock()
    {
        using var conexion = _factory.Crear();

        return Mapear(conexion.Query<FilaDashboard>(
            """
            SELECT
                p.ProductoId, p.NombreProducto, p.PrecioCentavos, p.Stock, p.StockMinimo, p.Activo, p.FechaAlta,
                COALESCE(c.NombreCategoria, '(Sin categoría)') AS NombreCategoria,
                COALESCE(s.NombreSeccion, '(Sin sección)')     AS NombreSeccion
            FROM Productos p
            LEFT JOIN Categorias c ON p.CategoriaId = c.CategoriaId
            LEFT JOIN Secciones  s ON c.SeccionId   = s.SeccionId
            WHERE p.Activo = 1 AND p.Stock <= p.StockMinimo
            ORDER BY p.Stock ASC, p.NombreProducto ASC;
            """));
    }

    public List<ProductoDashboardDto> ObtenerUltimosProductos(int cantidad = 10)
    {
        using var conexion = _factory.Crear();

        return Mapear(conexion.Query<FilaDashboard>(
            """
            SELECT
                p.ProductoId, p.NombreProducto, p.PrecioCentavos, p.Stock, p.StockMinimo, p.Activo, p.FechaAlta,
                COALESCE(c.NombreCategoria, '(Sin categoría)') AS NombreCategoria,
                COALESCE(s.NombreSeccion, '(Sin sección)')     AS NombreSeccion
            FROM Productos p
            LEFT JOIN Categorias c ON p.CategoriaId = c.CategoriaId
            LEFT JOIN Secciones  s ON c.SeccionId   = s.SeccionId
            ORDER BY p.FechaAlta DESC, p.ProductoId DESC
            LIMIT @Cantidad;
            """,
            new { Cantidad = cantidad }));
    }

    private static List<ProductoDashboardDto> Mapear(IEnumerable<FilaDashboard> filas) =>
        filas.Select(f => new ProductoDashboardDto
        {
            ProductoId = f.ProductoId,
            NombreProducto = f.NombreProducto,
            NombreSeccion = f.NombreSeccion,
            NombreCategoria = f.NombreCategoria,
            Precio = Money.FromCentavos(f.PrecioCentavos),
            Stock = (int)f.Stock,
            StockMinimo = (int)f.StockMinimo,
            Activo = f.Activo,
            FechaAlta = f.FechaAlta
        }).ToList();

    private static object AParametros(ProductoDto producto) => new
    {
        producto.ProductoId,
        producto.CategoriaId,
        CodigoProducto = Opcional(producto.CodigoProducto),
        producto.NombreProducto,
        Descripcion = Opcional(producto.Descripcion),
        Precio = Money.ToCentavos(producto.Precio),
        Costo = Money.ToCentavos(producto.Costo),
        producto.Stock,
        producto.StockMinimo,
        CodigoBarras = Opcional(producto.CodigoBarras),
        Marca = Opcional(producto.Marca),
        UnidadMedida = string.IsNullOrWhiteSpace(producto.UnidadMedida) ? "UNIDAD" : producto.UnidadMedida.Trim(),
        ImagenUrl = Opcional(producto.ImagenUrl),
        PermiteVentaSinStock = producto.PermiteVentaSinStock ? 1 : 0
    };

    // Los códigos únicos se guardan como NULL cuando vienen vacíos: el índice único
    // parcial ignora los NULL pero rechazaría varias cadenas vacías.
    private static string? Opcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private class FilaListado
    {
        public int ProductoId { get; init; }
        public string NombreProducto { get; init; } = string.Empty;
        public string NombreSeccion { get; init; } = string.Empty;
        public string NombreCategoria { get; init; } = string.Empty;
        public long PrecioCentavos { get; init; }
        public double Stock { get; init; }
        public bool Activo { get; init; }
    }

    private sealed class FilaDashboard : FilaListado
    {
        public double StockMinimo { get; init; }
        public DateTime? FechaAlta { get; init; }
    }

    private sealed class FilaProducto
    {
        public int ProductoId { get; init; }
        public int CategoriaId { get; init; }
        public string? CodigoProducto { get; init; }
        public string NombreProducto { get; init; } = string.Empty;
        public string? Descripcion { get; init; }
        public long PrecioCentavos { get; init; }
        public long CostoCentavos { get; init; }
        public double Stock { get; init; }
        public double StockMinimo { get; init; }
        public string? CodigoBarras { get; init; }
        public string? Marca { get; init; }
        public string UnidadMedida { get; init; } = "UNIDAD";
        public string? ImagenUrl { get; init; }
        public bool PermiteVentaSinStock { get; init; }
    }
}

using Cotillon.LocalApi.Service.Data;
using Cotillon.Shared.DTOs;
using System.Data.OleDb;

namespace Cotillon.LocalApi.Service.Repositories;

public class ProductosRepository
{
    private readonly AccessDb _accessDb;

    public ProductosRepository(AccessDb accessDb)
    {
        _accessDb = accessDb;
    }

    public bool Insertar(ProductoDto producto)
    {
        using var connection = (OleDbConnection)_accessDb.CreateConnection();
        connection.Open();

        const string sql = @"
INSERT INTO [Productos]
(
    [CategoriaId],
    [CodigoProducto],
    [NombreProducto],
    [Descripcion],
    [Precio],
    [Stock],
    [StockMinimo],
    [CodigoBarras],
    [Marca],
    [Activo],
    [FechaAlta],
    [Costo],
    [ImagenUrl],
    [UnidadMedida],
    [PermiteVentaSinStock]
)
VALUES
(
    ?, ?, ?, ?, ?, ?, ?, ?, ?, True, Now(), ?, ?, ?, ?
)";

        using var command = new OleDbCommand(sql, connection);

        command.Parameters.AddWithValue("?", producto.CategoriaId);
        command.Parameters.AddWithValue("?", string.IsNullOrWhiteSpace(producto.CodigoProducto) ? DBNull.Value : producto.CodigoProducto);
        command.Parameters.AddWithValue("?", producto.NombreProducto);
        command.Parameters.AddWithValue("?", string.IsNullOrWhiteSpace(producto.Descripcion) ? DBNull.Value : producto.Descripcion);
        command.Parameters.AddWithValue("?", producto.Precio);
        command.Parameters.AddWithValue("?", producto.Stock);
        command.Parameters.AddWithValue("?", producto.StockMinimo);
        command.Parameters.AddWithValue("?", string.IsNullOrWhiteSpace(producto.CodigoBarras) ? DBNull.Value : producto.CodigoBarras);
        command.Parameters.AddWithValue("?", string.IsNullOrWhiteSpace(producto.Marca) ? DBNull.Value : producto.Marca);
        command.Parameters.AddWithValue("?", producto.Costo);
        command.Parameters.AddWithValue("?", string.IsNullOrWhiteSpace(producto.ImagenUrl) ? DBNull.Value : producto.ImagenUrl);
        command.Parameters.AddWithValue("?", string.IsNullOrWhiteSpace(producto.UnidadMedida) ? DBNull.Value : producto.UnidadMedida);
        command.Parameters.AddWithValue("?", producto.PermiteVentaSinStock);

        return command.ExecuteNonQuery() > 0;
    }

    public List<ProductoListadoDto> ObtenerTodos(bool incluirInactivos)
    {
        var lista = new List<ProductoListadoDto>();

        using var connection = (OleDbConnection)_accessDb.CreateConnection();
        connection.Open();

        var sql = @"
SELECT
    p.[ProductoId],
    p.[NombreProducto],
    p.[Precio],
    p.[Stock],
    p.[Activo],
    c.[NombreCategoria],
    s.[NombreSeccion]
FROM
    ([Productos] AS p
    LEFT JOIN [Categorias] AS c ON p.[CategoriaId] = c.[CategoriaId])
    LEFT JOIN [Secciones] AS s ON c.[SeccionId] = s.[SeccionId]";

        if (!incluirInactivos)
        {
            sql += " WHERE p.[Activo] = True";
        }

        sql += " ORDER BY p.[NombreProducto]";

        using var command = new OleDbCommand(sql, connection);
        using var reader = command.ExecuteReader();

        while (reader != null && reader.Read())
        {
            lista.Add(new ProductoListadoDto
            {
                ProductoId = Convert.ToInt32(reader["ProductoId"]),
                NombreProducto = reader["NombreProducto"]?.ToString() ?? string.Empty,
                NombreSeccion = reader["NombreSeccion"] == DBNull.Value ? "(Sin sección)" : reader["NombreSeccion"]?.ToString() ?? string.Empty,
                NombreCategoria = reader["NombreCategoria"] == DBNull.Value ? "(Sin categoría)" : reader["NombreCategoria"]?.ToString() ?? string.Empty,
                Precio = reader["Precio"] != DBNull.Value ? Convert.ToDecimal(reader["Precio"]) : 0,
                Stock = reader["Stock"] != DBNull.Value ? Convert.ToInt32(reader["Stock"]) : 0,
                Activo = reader["Activo"] != DBNull.Value && Convert.ToBoolean(reader["Activo"])
            });
        }

        return lista;
    }

    public bool EliminarLogico(int productoId)
    {
        using var connection = (OleDbConnection)_accessDb.CreateConnection();
        connection.Open();

        const string sql = @"
UPDATE [Productos]
SET [Activo] = False
WHERE [ProductoId] = ?";

        using var command = new OleDbCommand(sql, connection);
        command.Parameters.AddWithValue("?", productoId);

        return command.ExecuteNonQuery() > 0;
    }

    public DashboardResumenDto ObtenerResumenDashboard()
    {
        using var connection = (OleDbConnection)_accessDb.CreateConnection();
        connection.Open();

        var resumen = new DashboardResumenDto();

        // Total productos
        using (var cmd = new OleDbCommand("SELECT COUNT(*) FROM [Productos]", connection))
        {
            resumen.TotalProductos = Convert.ToInt32(cmd.ExecuteScalar());
        }

        // Activos
        using (var cmd = new OleDbCommand("SELECT COUNT(*) FROM [Productos] WHERE [Activo] = True", connection))
        {
            resumen.ProductosActivos = Convert.ToInt32(cmd.ExecuteScalar());
        }

        // Inactivos
        using (var cmd = new OleDbCommand("SELECT COUNT(*) FROM [Productos] WHERE [Activo] = False", connection))
        {
            resumen.ProductosInactivos = Convert.ToInt32(cmd.ExecuteScalar());
        }

        // Bajo stock
        using (var cmd = new OleDbCommand(
            "SELECT COUNT(*) FROM [Productos] WHERE [Activo] = True AND [Stock] <= Nz([StockMinimo], 0)", connection))
        {
            resumen.ProductosBajoStock = Convert.ToInt32(cmd.ExecuteScalar());
        }

        return resumen;
    }

    public List<ProductoDashboardDto> ObtenerProductosBajoStock()
    {
        var lista = new List<ProductoDashboardDto>();

        using var connection = (OleDbConnection)_accessDb.CreateConnection();
        connection.Open();

        const string sql = @"
SELECT
    p.[ProductoId],
    p.[NombreProducto],
    p.[Precio],
    p.[Stock],
    p.[StockMinimo],
    p.[Activo],
    p.[FechaAlta],
    c.[NombreCategoria],
    s.[NombreSeccion]
FROM
    ([Productos] AS p
    LEFT JOIN [Categorias] AS c ON p.[CategoriaId] = c.[CategoriaId])
    LEFT JOIN [Secciones] AS s ON c.[SeccionId] = s.[SeccionId]
WHERE
    p.[Activo] = True
    AND p.[Stock] <= Nz(p.[StockMinimo], 0)
ORDER BY
    p.[Stock] ASC, p.[NombreProducto] ASC";

        using var command = new OleDbCommand(sql, connection);
        using var reader = command.ExecuteReader();

        while (reader != null && reader.Read())
        {
            lista.Add(new ProductoDashboardDto
            {
                ProductoId = Convert.ToInt32(reader["ProductoId"]),
                NombreProducto = reader["NombreProducto"]?.ToString() ?? string.Empty,
                NombreSeccion = reader["NombreSeccion"] == DBNull.Value ? "(Sin sección)" : reader["NombreSeccion"]?.ToString() ?? string.Empty,
                NombreCategoria = reader["NombreCategoria"] == DBNull.Value ? "(Sin categoría)" : reader["NombreCategoria"]?.ToString() ?? string.Empty,
                Precio = reader["Precio"] != DBNull.Value ? Convert.ToDecimal(reader["Precio"]) : 0,
                Stock = reader["Stock"] != DBNull.Value ? Convert.ToInt32(reader["Stock"]) : 0,
                StockMinimo = reader["StockMinimo"] != DBNull.Value ? Convert.ToInt32(reader["StockMinimo"]) : 0,
                Activo = reader["Activo"] != DBNull.Value && Convert.ToBoolean(reader["Activo"]),
                FechaAlta = reader["FechaAlta"] != DBNull.Value ? Convert.ToDateTime(reader["FechaAlta"]) : null
            });
        }

        return lista;
    }

    public List<ProductoDashboardDto> ObtenerUltimosProductos(int cantidad = 10)
    {
        var lista = new List<ProductoDashboardDto>();

        using var connection = (OleDbConnection)_accessDb.CreateConnection();
        connection.Open();

        string sql = $@"
SELECT TOP {cantidad}
    p.[ProductoId],
    p.[NombreProducto],
    p.[Precio],
    p.[Stock],
    p.[StockMinimo],
    p.[Activo],
    p.[FechaAlta],
    c.[NombreCategoria],
    s.[NombreSeccion]
FROM
    ([Productos] AS p
    LEFT JOIN [Categorias] AS c ON p.[CategoriaId] = c.[CategoriaId])
    LEFT JOIN [Secciones] AS s ON c.[SeccionId] = s.[SeccionId]
ORDER BY
    p.[FechaAlta] DESC, p.[ProductoId] DESC";

        using var command = new OleDbCommand(sql, connection);
        using var reader = command.ExecuteReader();

        while (reader != null && reader.Read())
        {
            lista.Add(new ProductoDashboardDto
            {
                ProductoId = Convert.ToInt32(reader["ProductoId"]),
                NombreProducto = reader["NombreProducto"]?.ToString() ?? string.Empty,
                NombreSeccion = reader["NombreSeccion"] == DBNull.Value ? "(Sin sección)" : reader["NombreSeccion"]?.ToString() ?? string.Empty,
                NombreCategoria = reader["NombreCategoria"] == DBNull.Value ? "(Sin categoría)" : reader["NombreCategoria"]?.ToString() ?? string.Empty,
                Precio = reader["Precio"] != DBNull.Value ? Convert.ToDecimal(reader["Precio"]) : 0,
                Stock = reader["Stock"] != DBNull.Value ? Convert.ToInt32(reader["Stock"]) : 0,
                StockMinimo = reader["StockMinimo"] != DBNull.Value ? Convert.ToInt32(reader["StockMinimo"]) : 0,
                Activo = reader["Activo"] != DBNull.Value && Convert.ToBoolean(reader["Activo"]),
                FechaAlta = reader["FechaAlta"] != DBNull.Value ? Convert.ToDateTime(reader["FechaAlta"]) : null
            });
        }

        return lista;
    }
}
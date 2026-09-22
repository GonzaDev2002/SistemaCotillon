using Cotillon.LocalApi.Service.Data;
using Cotillon.Shared.DTOs.Reportes;
using System.Data.OleDb;

namespace Cotillon.LocalApi.Service.Repositories;

public class ReportesRepository
{
    private readonly AccessDb _db;

    public ReportesRepository(AccessDb db)
    {
        _db = db;
    }

    public async Task<List<ReporteProductoDto>> ListarProductosAsync()
    {
        List<ReporteProductoDto> lista = new();

        using var conexionBase = _db.CreateConnection();
        var conexion = (OleDbConnection)conexionBase;
        await conexion.OpenAsync();

        string sql = @"
            SELECT 
                p.ProductoId,
                p.NombreProducto,
                s.NombreSeccion,
                c.NombreCategoria,
                p.Precio,
                p.Stock,
                p.StockMinimo,
                p.Activo
            FROM 
                (Productos AS p
                LEFT JOIN Categorias AS c ON p.CategoriaId = c.CategoriaId)
                LEFT JOIN Secciones AS s ON c.SeccionId = s.SeccionId
            ORDER BY p.NombreProducto";

        using var comando = new OleDbCommand(sql, conexion);
        using var lector = await comando.ExecuteReaderAsync();

        while (await lector.ReadAsync())
        {
            lista.Add(new ReporteProductoDto
            {
                ProductoId = Convert.ToInt32(lector["ProductoId"]),
                NombreProducto = lector["NombreProducto"]?.ToString() ?? "",
                Seccion = lector["NombreSeccion"]?.ToString() ?? "",
                Categoria = lector["NombreCategoria"]?.ToString() ?? "",
                Precio = lector["Precio"] == DBNull.Value ? 0 : Convert.ToDecimal(lector["Precio"]),
                Stock = lector["Stock"] == DBNull.Value ? 0 : Convert.ToInt32(lector["Stock"]),
                StockMinimo = lector["StockMinimo"] == DBNull.Value ? 0 : Convert.ToInt32(lector["StockMinimo"]),
                Activo = lector["Activo"] != DBNull.Value && Convert.ToBoolean(lector["Activo"])
            });
        }

        return lista;
    }

    public async Task<List<ReporteProductoDto>> ListarProductosBajoStockAsync()
    {
        List<ReporteProductoDto> lista = new();

        using var conexionBase = _db.CreateConnection();
        var conexion = (OleDbConnection)conexionBase;
        await conexion.OpenAsync();

        string sql = @"
            SELECT 
                p.ProductoId,
                p.NombreProducto,
                s.NombreSeccion,
                c.NombreCategoria,
                p.Precio,
                p.Stock,
                p.StockMinimo,
                p.Activo
            FROM 
                (Productos AS p
                LEFT JOIN Categorias AS c ON p.CategoriaId = c.CategoriaId)
                LEFT JOIN Secciones AS s ON c.SeccionId = s.SeccionId
            WHERE p.Stock <= p.StockMinimo AND p.Activo = True
            ORDER BY p.Stock ASC";

        using var comando = new OleDbCommand(sql, conexion);
        using var lector = await comando.ExecuteReaderAsync();

        while (await lector.ReadAsync())
        {
            lista.Add(new ReporteProductoDto
            {
                ProductoId = Convert.ToInt32(lector["ProductoId"]),
                NombreProducto = lector["NombreProducto"]?.ToString() ?? "",
                Seccion = lector["NombreSeccion"]?.ToString() ?? "",
                Categoria = lector["NombreCategoria"]?.ToString() ?? "",
                Precio = lector["Precio"] == DBNull.Value ? 0 : Convert.ToDecimal(lector["Precio"]),
                Stock = lector["Stock"] == DBNull.Value ? 0 : Convert.ToInt32(lector["Stock"]),
                StockMinimo = lector["StockMinimo"] == DBNull.Value ? 0 : Convert.ToInt32(lector["StockMinimo"]),
                Activo = lector["Activo"] != DBNull.Value && Convert.ToBoolean(lector["Activo"])
            });
        }

        return lista;
    }

    public async Task<List<ReporteVentaDto>> ListarVentasPorRangoAsync(DateTime desde, DateTime hasta)
    {
        List<ReporteVentaDto> lista = new();

        using var conexionBase = _db.CreateConnection();
        var conexion = (OleDbConnection)conexionBase;
        await conexion.OpenAsync();

        string sql = @"
            SELECT 
                v.VentaId,
                v.FechaVenta,
                cl.NombreCompleto AS Cliente,
                u.NombreCompleto AS Usuario,
                v.TotalVenta,
                v.Estado
            FROM 
                (Ventas AS v
                LEFT JOIN Clientes AS cl ON v.ClienteId = cl.ClienteId)
                LEFT JOIN Usuarios AS u ON v.UsuarioId = u.UsuarioId
            WHERE v.FechaVenta BETWEEN ? AND ?
            ORDER BY v.FechaVenta DESC";

        using var comando = new OleDbCommand(sql, conexion);
        comando.Parameters.AddWithValue("@Desde", desde.Date);
        comando.Parameters.AddWithValue("@Hasta", hasta.Date.AddDays(1).AddSeconds(-1));

        using var lector = await comando.ExecuteReaderAsync();

        while (await lector.ReadAsync())
        {
            lista.Add(new ReporteVentaDto
            {
                VentaId = Convert.ToInt32(lector["VentaId"]),
                FechaVenta = lector["FechaVenta"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(lector["FechaVenta"]),
                Cliente = lector["Cliente"]?.ToString() ?? "Consumidor final",
                Usuario = lector["Usuario"]?.ToString() ?? "",
                TotalVenta = lector["TotalVenta"] == DBNull.Value ? 0 : Convert.ToDecimal(lector["TotalVenta"]),
                Estado = lector["Estado"]?.ToString() ?? ""
            });
        }

        return lista;
    }
}
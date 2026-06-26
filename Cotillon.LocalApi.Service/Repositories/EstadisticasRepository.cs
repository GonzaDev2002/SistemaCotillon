using Cotillon.LocalApi.Service.Data;
using Cotillon.Shared.DTOs.Estadisticas;
using System.Data.OleDb;

namespace Cotillon.LocalApi.Service.Repositories;

public class EstadisticasRepository
{
    private readonly AccessDb _db;

    public EstadisticasRepository(AccessDb db)
    {
        _db = db;
    }

    public async Task<List<EstadisticaVentaDiaDto>> VentasPorDiaAsync(DateTime desde, DateTime hasta)
    {
        List<EstadisticaVentaDiaDto> lista = new();

        using var conexionBase = _db.CreateConnection();
        var conexion = (OleDbConnection)conexionBase;
        await conexion.OpenAsync();

        string sql = @"
            SELECT 
                DateValue(FechaVenta) AS Fecha,
                SUM(TotalVenta) AS Total
            FROM Ventas
            WHERE FechaVenta BETWEEN ? AND ?
            GROUP BY DateValue(FechaVenta)
            ORDER BY DateValue(FechaVenta)";

        using var comando = new OleDbCommand(sql, conexion);
        comando.Parameters.AddWithValue("@Desde", desde.Date);
        comando.Parameters.AddWithValue("@Hasta", hasta.Date.AddDays(1).AddSeconds(-1));

        using var lector = await comando.ExecuteReaderAsync();

        while (await lector.ReadAsync())
        {
            lista.Add(new EstadisticaVentaDiaDto
            {
                Fecha = lector["Fecha"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(lector["Fecha"]),
                Total = lector["Total"] == DBNull.Value ? 0 : Convert.ToDecimal(lector["Total"])
            });
        }

        return lista;
    }

    public async Task<List<EstadisticaProductoVendidoDto>> ProductosMasVendidosAsync(DateTime desde, DateTime hasta)
    {
        List<EstadisticaProductoVendidoDto> lista = new();

        using var conexionBase = _db.CreateConnection();
        var conexion = (OleDbConnection)conexionBase;
        await conexion.OpenAsync();

        string sql = @"
            SELECT TOP 10
                p.NombreProducto AS Producto,
                SUM(dv.Cantidad) AS Cantidad
            FROM 
                (DetalleVentas AS dv
                INNER JOIN Productos AS p ON dv.ProductoId = p.ProductoId)
                INNER JOIN Ventas AS v ON dv.VentaId = v.VentaId
            WHERE v.FechaVenta BETWEEN ? AND ?
            GROUP BY p.NombreProducto
            ORDER BY SUM(dv.Cantidad) DESC";

        using var comando = new OleDbCommand(sql, conexion);
        comando.Parameters.AddWithValue("@Desde", desde.Date);
        comando.Parameters.AddWithValue("@Hasta", hasta.Date.AddDays(1).AddSeconds(-1));

        using var lector = await comando.ExecuteReaderAsync();

        while (await lector.ReadAsync())
        {
            lista.Add(new EstadisticaProductoVendidoDto
            {
                Producto = lector["Producto"]?.ToString() ?? "",
                Cantidad = lector["Cantidad"] == DBNull.Value ? 0 : Convert.ToInt32(lector["Cantidad"])
            });
        }

        return lista;
    }

    public async Task<List<EstadisticaMedioPagoDto>> VentasPorMedioPagoAsync(DateTime desde, DateTime hasta)
    {
        List<EstadisticaMedioPagoDto> lista = new();

        using var conexionBase = _db.CreateConnection();
        var conexion = (OleDbConnection)conexionBase;
        await conexion.OpenAsync();

        string sql = @"
            SELECT 
                mp.NombreMedioPago AS MedioPago,
                SUM(pv.Importe) AS Total
            FROM 
                (PagosVenta AS pv
                INNER JOIN MediosPago AS mp ON pv.MedioPagoId = mp.MedioPagoId)
                INNER JOIN Ventas AS v ON pv.VentaId = v.VentaId
            WHERE v.FechaVenta BETWEEN ? AND ?
            GROUP BY mp.NombreMedioPago
            ORDER BY SUM(pv.Importe) DESC";

        using var comando = new OleDbCommand(sql, conexion);
        comando.Parameters.AddWithValue("@Desde", desde.Date);
        comando.Parameters.AddWithValue("@Hasta", hasta.Date.AddDays(1).AddSeconds(-1));

        using var lector = await comando.ExecuteReaderAsync();

        while (await lector.ReadAsync())
        {
            lista.Add(new EstadisticaMedioPagoDto
            {
                MedioPago = lector["MedioPago"]?.ToString() ?? "",
                Total = lector["Total"] == DBNull.Value ? 0 : Convert.ToDecimal(lector["Total"])
            });
        }

        return lista;
    }

    public async Task<List<EstadisticaBajoStockDto>> ProductosBajoStockAsync()
    {
        List<EstadisticaBajoStockDto> lista = new();

        using var conexionBase = _db.CreateConnection();
        var conexion = (OleDbConnection)conexionBase;
        await conexion.OpenAsync();

        string sql = @"
            SELECT TOP 10
                NombreProducto,
                Stock,
                StockMinimo
            FROM Productos
            WHERE Stock <= StockMinimo AND Activo = True
            ORDER BY Stock ASC";

        using var comando = new OleDbCommand(sql, conexion);
        using var lector = await comando.ExecuteReaderAsync();

        while (await lector.ReadAsync())
        {
            lista.Add(new EstadisticaBajoStockDto
            {
                Producto = lector["NombreProducto"]?.ToString() ?? "",
                Stock = lector["Stock"] == DBNull.Value ? 0 : Convert.ToInt32(lector["Stock"]),
                StockMinimo = lector["StockMinimo"] == DBNull.Value ? 0 : Convert.ToInt32(lector["StockMinimo"])
            });
        }

        return lista;
    }
}
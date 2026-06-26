using Cotillon.LocalApi.Service.Data;
using Cotillon.Shared.DTOs.Compras;
using System.Data.OleDb;

namespace Cotillon.LocalApi.Service.Repositories;

public class ComprasRepository
{
    private readonly AccessDb _db;

    public ComprasRepository(AccessDb db)
    {
        _db = db;
    }

    public async Task<List<CompraListadoDto>> ListarAsync()
    {
        List<CompraListadoDto> lista = new();

        using var conexion = (OleDbConnection)_db.CreateConnection();
        await conexion.OpenAsync();

        string sql = @"
            SELECT TOP 50 
                c.CompraId,
                c.FechaCompra,
                p.NombreProveedor,
                u.NombreCompleto,
                c.NumeroComprobante,
                c.TotalCompra,
                c.Estado
            FROM (Compras AS c
            LEFT JOIN Proveedores AS p ON c.ProveedorId = p.ProveedorId)
            LEFT JOIN Usuarios AS u ON c.UsuarioId = u.UsuarioId
            ORDER BY c.FechaCompra DESC";

        using var cmd = new OleDbCommand(sql, conexion);
        using var lector = await cmd.ExecuteReaderAsync();

        while (await lector.ReadAsync())
        {
            lista.Add(new CompraListadoDto
            {
                CompraId = Convert.ToInt32(lector["CompraId"]),
                FechaCompra = Convert.ToDateTime(lector["FechaCompra"]),
                Proveedor = lector["NombreProveedor"]?.ToString() ?? "",
                Usuario = lector["NombreCompleto"]?.ToString() ?? "",
                NumeroComprobante = lector["NumeroComprobante"]?.ToString() ?? "",
                TotalCompra = lector["TotalCompra"] == DBNull.Value ? 0 : Convert.ToDecimal(lector["TotalCompra"]),
                Estado = lector["Estado"]?.ToString() ?? ""
            });
        }

        return lista;
    }

    public async Task<int> CrearCompraAsync(CrearCompraRequestDto request)
    {
        using var conexion = (OleDbConnection)_db.CreateConnection();
        await conexion.OpenAsync();

        using var transaction = conexion.BeginTransaction();

        try
        {
            decimal subtotal = request.Detalles.Sum(x => x.SubtotalLinea);
            decimal totalCompra = subtotal - request.DescuentoTotal;

            string sqlCompra = @"
                INSERT INTO Compras
                (ProveedorId, UsuarioId, FechaCompra, NumeroComprobante, Subtotal, DescuentoTotal, TotalCompra, Estado, Observaciones, FechaAlta, FechaActualizacion)
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

            using var cmdCompra = new OleDbCommand(sqlCompra, conexion, transaction);
            cmdCompra.Parameters.Add("@ProveedorId", OleDbType.Integer).Value = request.ProveedorId;
            cmdCompra.Parameters.Add("@UsuarioId", OleDbType.Integer).Value = request.UsuarioId;
            cmdCompra.Parameters.Add("@FechaCompra", OleDbType.Date).Value = DateTime.Now;
            cmdCompra.Parameters.Add("@NumeroComprobante", OleDbType.VarWChar).Value = request.NumeroComprobante;
            cmdCompra.Parameters.Add("@Subtotal", OleDbType.Currency).Value = subtotal;
            cmdCompra.Parameters.Add("@DescuentoTotal", OleDbType.Currency).Value = request.DescuentoTotal;
            cmdCompra.Parameters.Add("@TotalCompra", OleDbType.Currency).Value = totalCompra;
            cmdCompra.Parameters.Add("@Estado", OleDbType.VarWChar).Value = string.IsNullOrWhiteSpace(request.Estado) ? "Confirmada" : request.Estado;
            cmdCompra.Parameters.Add("@Observaciones", OleDbType.LongVarWChar).Value = request.Observaciones ?? "";
            cmdCompra.Parameters.Add("@FechaAlta", OleDbType.Date).Value = DateTime.Now;
            cmdCompra.Parameters.Add("@FechaActualizacion", OleDbType.Date).Value = DateTime.Now;

            await cmdCompra.ExecuteNonQueryAsync();

            using var cmdId = new OleDbCommand("SELECT @@IDENTITY", conexion, transaction);
            int compraId = Convert.ToInt32(await cmdId.ExecuteScalarAsync());

            foreach (var detalle in request.Detalles)
            {
                string sqlDetalle = @"
                    INSERT INTO DetalleCompras
                    (CompraId, ProductoId, Cantidad, CostoUnitario, SubtotalLinea)
                    VALUES (?, ?, ?, ?, ?)";

                using var cmdDetalle = new OleDbCommand(sqlDetalle, conexion, transaction);
                cmdDetalle.Parameters.Add("@CompraId", OleDbType.Integer).Value = compraId;
                cmdDetalle.Parameters.Add("@ProductoId", OleDbType.Integer).Value = detalle.ProductoId;
                cmdDetalle.Parameters.Add("@Cantidad", OleDbType.Integer).Value = detalle.Cantidad;
                cmdDetalle.Parameters.Add("@CostoUnitario", OleDbType.Currency).Value = detalle.CostoUnitario;
                cmdDetalle.Parameters.Add("@SubtotalLinea", OleDbType.Currency).Value = detalle.SubtotalLinea;

                await cmdDetalle.ExecuteNonQueryAsync();

                string sqlStockAnterior = "SELECT Stock FROM Productos WHERE ProductoId = ?";
                using var cmdStockAnterior = new OleDbCommand(sqlStockAnterior, conexion, transaction);
                cmdStockAnterior.Parameters.Add("@ProductoId", OleDbType.Integer).Value = detalle.ProductoId;

                object? stockResult = await cmdStockAnterior.ExecuteScalarAsync();
                int stockAnterior = stockResult == DBNull.Value || stockResult == null ? 0 : Convert.ToInt32(stockResult);
                int stockNuevo = stockAnterior + detalle.Cantidad;

                string sqlActualizarStock = @"
                    UPDATE Productos
                    SET Stock = ?, Costo = ?, FechaActualizacion = ?
                    WHERE ProductoId = ?";

                using var cmdActualizarStock = new OleDbCommand(sqlActualizarStock, conexion, transaction);
                cmdActualizarStock.Parameters.Add("@Stock", OleDbType.Integer).Value = stockNuevo;
                cmdActualizarStock.Parameters.Add("@Costo", OleDbType.Currency).Value = detalle.CostoUnitario;
                cmdActualizarStock.Parameters.Add("@FechaActualizacion", OleDbType.Date).Value = DateTime.Now;
                cmdActualizarStock.Parameters.Add("@ProductoId", OleDbType.Integer).Value = detalle.ProductoId;

                await cmdActualizarStock.ExecuteNonQueryAsync();

                string sqlMovimiento = @"
                    INSERT INTO StockMovimientos
                    (ProductoId, UsuarioId, FechaMovimiento, TipoMovimiento, OrigenId, OrigenTipo, Cantidad, StockAnterior, StockNuevo, Observaciones)
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                using var cmdMovimiento = new OleDbCommand(sqlMovimiento, conexion, transaction);
                cmdMovimiento.Parameters.Add("@ProductoId", OleDbType.Integer).Value = detalle.ProductoId;
                cmdMovimiento.Parameters.Add("@UsuarioId", OleDbType.Integer).Value = request.UsuarioId;
                cmdMovimiento.Parameters.Add("@FechaMovimiento", OleDbType.Date).Value = DateTime.Now;
                cmdMovimiento.Parameters.Add("@TipoMovimiento", OleDbType.VarWChar).Value = "Entrada";
                cmdMovimiento.Parameters.Add("@OrigenId", OleDbType.Integer).Value = compraId;
                cmdMovimiento.Parameters.Add("@OrigenTipo", OleDbType.VarWChar).Value = "Compra";
                cmdMovimiento.Parameters.Add("@Cantidad", OleDbType.Integer).Value = detalle.Cantidad;
                cmdMovimiento.Parameters.Add("@StockAnterior", OleDbType.Integer).Value = stockAnterior;
                cmdMovimiento.Parameters.Add("@StockNuevo", OleDbType.Integer).Value = stockNuevo;
                cmdMovimiento.Parameters.Add("@Observaciones", OleDbType.LongVarWChar).Value = "Ingreso de mercadería por compra";

                await cmdMovimiento.ExecuteNonQueryAsync();
            }

            transaction.Commit();
            return compraId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}

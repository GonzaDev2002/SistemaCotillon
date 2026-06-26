using Cotillon.LocalApi.Service.Data;
using Cotillon.Shared.DTOs.Ventas;
using System.Data.OleDb;

namespace Cotillon.LocalApi.Service.Repositories;

public class VentasRepository
{
    private readonly AccessDb _db;

    public VentasRepository(AccessDb db)
    {
        _db = db;
    }

    public async Task<int> CrearVentaAsync(CrearVentaRequestDto request)
    {
        using var conexionBase = _db.CreateConnection();
        var conexion = (OleDbConnection)conexionBase;
        await conexion.OpenAsync();

        using var transaccion = conexion.BeginTransaction();

        try
        {
            if (request.SesionCajaId <= 0)
                throw new Exception("No hay una sesión de caja abierta para registrar la venta.");

            decimal subtotal = request.Detalles.Sum(x => x.Cantidad * x.PrecioUnitario);
            decimal total = subtotal - request.DescuentoTotal;

            string numeroComprobante = string.IsNullOrWhiteSpace(request.NumeroComprobante)
                ? DateTime.Now.ToString("yyyyMMddHHmmss")
                : request.NumeroComprobante;

            string medioPagoNombre = request.MedioPagoId switch
            {
                1 => "Efectivo",
                2 => "Transferencia",
                3 => "Tarjeta",
                _ => "Efectivo"
            };

            int? clienteId = request.ClienteId;

            if (clienteId <= 0)
            {
                clienteId = await ObtenerOCrearConsumidorFinalAsync(conexion, transaccion);
            }

            string sqlVenta = @"
INSERT INTO Ventas
(
ClienteId,
UsuarioId,
SesionCajaId,
FechaVenta,
TipoComprobante,
NumeroComprobante,
Subtotal,
DescuentoTotal,
TotalVenta,
MedioPagoPrincipal,
Estado,
Observaciones,
FechaAlta,
FechaActualizacion
)
VALUES
(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

            using var cmdVenta = new OleDbCommand(sqlVenta, conexion, transaccion);

            cmdVenta.Parameters.Add("@ClienteId", OleDbType.Integer).Value =
    clienteId.HasValue ? clienteId.Value : DBNull.Value;

            cmdVenta.Parameters.Add("@UsuarioId", OleDbType.Integer).Value = request.UsuarioId;

            cmdVenta.Parameters.Add("@SesionCajaId", OleDbType.Integer).Value = request.SesionCajaId;

            cmdVenta.Parameters.Add("@FechaVenta", OleDbType.Date).Value = DateTime.Now;

            cmdVenta.Parameters.Add("@TipoComprobante", OleDbType.VarWChar).Value = request.TipoComprobante;

            cmdVenta.Parameters.Add("@NumeroComprobante", OleDbType.VarWChar).Value = numeroComprobante;

            cmdVenta.Parameters.Add("@Subtotal", OleDbType.Currency).Value = subtotal;

            cmdVenta.Parameters.Add("@DescuentoTotal", OleDbType.Currency).Value = request.DescuentoTotal;

            cmdVenta.Parameters.Add("@TotalVenta", OleDbType.Currency).Value = total;

            cmdVenta.Parameters.Add("@MedioPagoPrincipal", OleDbType.VarWChar).Value = medioPagoNombre;

            cmdVenta.Parameters.Add("@Estado", OleDbType.VarWChar).Value = "Confirmada";

            cmdVenta.Parameters.Add("@Observaciones", OleDbType.LongVarWChar).Value = "";

            cmdVenta.Parameters.Add("@FechaAlta", OleDbType.Date).Value = DateTime.Now;

            cmdVenta.Parameters.Add("@FechaActualizacion", OleDbType.Date).Value = DateTime.Now;

            await cmdVenta.ExecuteNonQueryAsync();

            using var cmdId = new OleDbCommand("SELECT @@IDENTITY", conexion, transaccion);
            int ventaId = Convert.ToInt32(await cmdId.ExecuteScalarAsync());

            foreach (var item in request.Detalles)
            {
                decimal subtotalLinea = item.Cantidad * item.PrecioUnitario;

                string sqlDetalle = @"
                    INSERT INTO DetalleVentas
                    (VentaId, ProductoId, Cantidad, PrecioUnitario, DescuentoLinea, SubtotalLinea)
                    VALUES (?, ?, ?, ?, ?, ?)";

                using var cmdDetalle = new OleDbCommand(sqlDetalle, conexion, transaccion);
                cmdDetalle.Parameters.Add("@VentaId", OleDbType.Integer).Value = ventaId;
                cmdDetalle.Parameters.Add("@ProductoId", OleDbType.Integer).Value = item.ProductoId;
                cmdDetalle.Parameters.Add("@Cantidad", OleDbType.Integer).Value = item.Cantidad;
                cmdDetalle.Parameters.Add("@PrecioUnitario", OleDbType.Currency).Value = item.PrecioUnitario;
                cmdDetalle.Parameters.Add("@DescuentoLinea", OleDbType.Currency).Value = 0;
                cmdDetalle.Parameters.Add("@SubtotalLinea", OleDbType.Currency).Value = subtotalLinea;
                await cmdDetalle.ExecuteNonQueryAsync();

                string sqlStock = @"
                    UPDATE Productos
                    SET Stock = Stock - ?, FechaActualizacion = ?
                    WHERE ProductoId = ?";

                using var cmdStock = new OleDbCommand(sqlStock, conexion, transaccion);
                cmdStock.Parameters.Add("@Cantidad", OleDbType.Integer).Value = item.Cantidad;
                cmdStock.Parameters.Add("@FechaActualizacion", OleDbType.Date).Value = DateTime.Now;
                cmdStock.Parameters.Add("@ProductoId", OleDbType.Integer).Value = item.ProductoId;
                await cmdStock.ExecuteNonQueryAsync();

                string sqlMovimiento = @"
                    INSERT INTO StockMovimientos
                    (ProductoId, UsuarioId, FechaMovimiento, TipoMovimiento, OrigenId, OrigenTipo, Cantidad, StockAnterior, StockNuevo, Observaciones)
                    SELECT ProductoId, ?, ?, ?, ?, ?, ?, Stock + ?, Stock, ?
                    FROM Productos
                    WHERE ProductoId = ?";

                using var cmdMov = new OleDbCommand(sqlMovimiento, conexion, transaccion);
                cmdMov.Parameters.Add("@UsuarioId", OleDbType.Integer).Value = request.UsuarioId;
                cmdMov.Parameters.Add("@FechaMovimiento", OleDbType.Date).Value = DateTime.Now;
                cmdMov.Parameters.Add("@TipoMovimiento", OleDbType.VarWChar).Value = "Salida";
                cmdMov.Parameters.Add("@OrigenId", OleDbType.Integer).Value = ventaId;
                cmdMov.Parameters.Add("@OrigenTipo", OleDbType.VarWChar).Value = "Venta";
                cmdMov.Parameters.Add("@Cantidad", OleDbType.Integer).Value = item.Cantidad;
                cmdMov.Parameters.Add("@StockAnterior", OleDbType.Integer).Value = item.Cantidad;
                cmdMov.Parameters.Add("@Observaciones", OleDbType.LongVarWChar).Value = "Venta registrada";
                cmdMov.Parameters.Add("@ProductoId", OleDbType.Integer).Value = item.ProductoId;
                await cmdMov.ExecuteNonQueryAsync();
            }

            string sqlPago = @"
                INSERT INTO PagosVenta
                (VentaId, MedioPagoId, Importe, Referencia, FechaPago, Observaciones)
                VALUES (?, ?, ?, ?, ?, ?)";

            using var cmdPago = new OleDbCommand(sqlPago, conexion, transaccion);
            cmdPago.Parameters.Add("@VentaId", OleDbType.Integer).Value = ventaId;
            cmdPago.Parameters.Add("@MedioPagoId", OleDbType.Integer).Value = request.MedioPagoId;
            cmdPago.Parameters.Add("@Importe", OleDbType.Currency).Value = total;
            cmdPago.Parameters.Add("@Referencia", OleDbType.VarWChar).Value = numeroComprobante;
            cmdPago.Parameters.Add("@FechaPago", OleDbType.Date).Value = DateTime.Now;
            cmdPago.Parameters.Add("@Observaciones", OleDbType.LongVarWChar).Value = "Pago de venta";
            await cmdPago.ExecuteNonQueryAsync();

            transaccion.Commit();
            return ventaId;
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    public async Task<List<VentaListadoDto>> ListarVentasAsync()
    {
        List<VentaListadoDto> lista = new();

        using var conexionBase = _db.CreateConnection();
        var conexion = (OleDbConnection)conexionBase;
        await conexion.OpenAsync();

        string sql = @"
            SELECT 
                v.VentaId,
                v.FechaVenta,
                c.NombreCompleto AS Cliente,
                u.NombreCompleto AS Usuario,
                v.TotalVenta,
                v.Estado
            FROM 
                (Ventas AS v
                LEFT JOIN Clientes AS c ON v.ClienteId = c.ClienteId)
                LEFT JOIN Usuarios AS u ON v.UsuarioId = u.UsuarioId
            ORDER BY v.FechaVenta DESC";

        using var cmd = new OleDbCommand(sql, conexion);
        using var lector = await cmd.ExecuteReaderAsync();

        while (await lector.ReadAsync())
        {
            lista.Add(new VentaListadoDto
            {
                VentaId = Convert.ToInt32(lector["VentaId"]),
                FechaVenta = Convert.ToDateTime(lector["FechaVenta"]),
                Cliente = lector["Cliente"]?.ToString() ?? "",
                Usuario = lector["Usuario"]?.ToString() ?? "",
                TotalVenta = lector["TotalVenta"] == DBNull.Value ? 0 : Convert.ToDecimal(lector["TotalVenta"]),
                Estado = lector["Estado"]?.ToString() ?? ""
            });
        }

        return lista;
    }

    private async Task<int> ObtenerOCrearConsumidorFinalAsync(OleDbConnection conexion, OleDbTransaction transaccion)
    {
        string sqlBuscar = @"
        SELECT TOP 1 ClienteId
        FROM Clientes
        WHERE NombreCompleto = ?";

        using var cmdBuscar = new OleDbCommand(sqlBuscar, conexion, transaccion);
        cmdBuscar.Parameters.Add("@NombreCompleto", OleDbType.VarWChar).Value = "Consumidor Final";

        object? result = await cmdBuscar.ExecuteScalarAsync();

        if (result != null && result != DBNull.Value)
            return Convert.ToInt32(result);

        string sqlInsert = @"
        INSERT INTO Clientes
        (NombreCompleto, Telefono, Email, Direccion, Documento, Observaciones, Activo, FechaAlta, FechaActualizacion)
        VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)";

        using var cmdInsert = new OleDbCommand(sqlInsert, conexion, transaccion);
        cmdInsert.Parameters.Add("@NombreCompleto", OleDbType.VarWChar).Value = "Consumidor Final";
        cmdInsert.Parameters.Add("@Telefono", OleDbType.VarWChar).Value = "-";
        cmdInsert.Parameters.Add("@Email", OleDbType.VarWChar).Value = "-";
        cmdInsert.Parameters.Add("@Direccion", OleDbType.VarWChar).Value = "-";
        cmdInsert.Parameters.Add("@Documento", OleDbType.VarWChar).Value = "-";
        cmdInsert.Parameters.Add("@Observaciones", OleDbType.LongVarWChar).Value = "Cliente genérico para ventas sin identificar";
        cmdInsert.Parameters.Add("@Activo", OleDbType.Boolean).Value = true;
        cmdInsert.Parameters.Add("@FechaAlta", OleDbType.Date).Value = DateTime.Now;
        cmdInsert.Parameters.Add("@FechaActualizacion", OleDbType.Date).Value = DateTime.Now;

        await cmdInsert.ExecuteNonQueryAsync();

        using var cmdId = new OleDbCommand("SELECT @@IDENTITY", conexion, transaccion);
        return Convert.ToInt32(await cmdId.ExecuteScalarAsync());
    }
}
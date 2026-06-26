using Cotillon.LocalApi.Service.Data;
using Cotillon.Shared.DTOs.Caja;
using System.Data.OleDb;

namespace Cotillon.LocalApi.Service.Repositories;

public class CajaRepository
{
    private readonly AccessDb _db;

    public CajaRepository(AccessDb db)
    {
        _db = db;
    }

    public async Task<SesionCajaDto?> ObtenerCajaAbiertaAsync(int usuarioId)
    {
        using var conexion = (OleDbConnection)_db.CreateConnection();
        await conexion.OpenAsync();

        string sql = @"
            SELECT TOP 1 SesionCajaId, UsuarioId, FechaApertura, FechaCierre,
                   MontoInicial, MontoEsperado, MontoContado, Diferencia, Estado, Observaciones
            FROM SesionesCaja
            WHERE UsuarioId = ? AND Estado = ?
            ORDER BY FechaApertura DESC";

        using var cmd = new OleDbCommand(sql, conexion);
        cmd.Parameters.Add("@UsuarioId", OleDbType.Integer).Value = usuarioId;
        cmd.Parameters.Add("@Estado", OleDbType.VarWChar).Value = "Abierta";

        using var lector = await cmd.ExecuteReaderAsync();

        if (!await lector.ReadAsync())
            return null;

        return new SesionCajaDto
        {
            SesionCajaId = Convert.ToInt32(lector["SesionCajaId"]),
            UsuarioId = Convert.ToInt32(lector["UsuarioId"]),
            FechaApertura = Convert.ToDateTime(lector["FechaApertura"]),
            FechaCierre = lector["FechaCierre"] == DBNull.Value ? null : Convert.ToDateTime(lector["FechaCierre"]),
            MontoInicial = lector["MontoInicial"] == DBNull.Value ? 0 : Convert.ToDecimal(lector["MontoInicial"]),
            MontoEsperado = lector["MontoEsperado"] == DBNull.Value ? 0 : Convert.ToDecimal(lector["MontoEsperado"]),
            MontoContado = lector["MontoContado"] == DBNull.Value ? 0 : Convert.ToDecimal(lector["MontoContado"]),
            Diferencia = lector["Diferencia"] == DBNull.Value ? 0 : Convert.ToDecimal(lector["Diferencia"]),
            Estado = lector["Estado"]?.ToString() ?? "",
            Observaciones = lector["Observaciones"]?.ToString() ?? ""
        };
    }

    public async Task<int> AbrirCajaAsync(AbrirCajaRequestDto request)
    {
        using var conexion = (OleDbConnection)_db.CreateConnection();
        await conexion.OpenAsync();

        string sql = @"
            INSERT INTO SesionesCaja
            (UsuarioId, FechaApertura, MontoInicial, MontoEsperado, MontoContado, Diferencia, Estado, Observaciones)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?)";

        using var cmd = new OleDbCommand(sql, conexion);
        cmd.Parameters.Add("@UsuarioId", OleDbType.Integer).Value = request.UsuarioId;
        cmd.Parameters.Add("@FechaApertura", OleDbType.Date).Value = DateTime.Now;
        cmd.Parameters.Add("@MontoInicial", OleDbType.Currency).Value = request.MontoInicial;
        cmd.Parameters.Add("@MontoEsperado", OleDbType.Currency).Value = request.MontoInicial;
        cmd.Parameters.Add("@MontoContado", OleDbType.Currency).Value = 0;
        cmd.Parameters.Add("@Diferencia", OleDbType.Currency).Value = 0;
        cmd.Parameters.Add("@Estado", OleDbType.VarWChar).Value = "Abierta";
        cmd.Parameters.Add("@Observaciones", OleDbType.LongVarWChar).Value = request.Observaciones ?? "";

        await cmd.ExecuteNonQueryAsync();

        using var cmdId = new OleDbCommand("SELECT @@IDENTITY", conexion);
        return Convert.ToInt32(await cmdId.ExecuteScalarAsync());
    }
    public async Task CerrarCajaAsync(CerrarCajaRequestDto request)
    {
        using var conexion = (OleDbConnection)_db.CreateConnection();
        await conexion.OpenAsync();

        string sqlTotalVentas = @"
        SELECT SUM(TotalVenta)
        FROM Ventas
        WHERE SesionCajaId = ? AND Estado = ?";

        using var cmdTotal = new OleDbCommand(sqlTotalVentas, conexion);
        cmdTotal.Parameters.Add("@SesionCajaId", OleDbType.Integer).Value = request.SesionCajaId;
        cmdTotal.Parameters.Add("@Estado", OleDbType.VarWChar).Value = "Confirmada";

        object? resultado = await cmdTotal.ExecuteScalarAsync();
        decimal totalVentas = resultado == DBNull.Value || resultado == null ? 0 : Convert.ToDecimal(resultado);

        string sqlMontoInicial = @"
        SELECT MontoInicial
        FROM SesionesCaja
        WHERE SesionCajaId = ?";

        using var cmdInicial = new OleDbCommand(sqlMontoInicial, conexion);
        cmdInicial.Parameters.Add("@SesionCajaId", OleDbType.Integer).Value = request.SesionCajaId;

        object? inicialResult = await cmdInicial.ExecuteScalarAsync();
        decimal montoInicial = inicialResult == DBNull.Value || inicialResult == null ? 0 : Convert.ToDecimal(inicialResult);

        decimal montoEsperado = montoInicial + totalVentas;
        decimal diferencia = request.MontoContado - montoEsperado;

        string sqlCerrar = @"
        UPDATE SesionesCaja
        SET FechaCierre = ?, 
            MontoEsperado = ?, 
            MontoContado = ?, 
            Diferencia = ?, 
            Estado = ?, 
            Observaciones = ?
        WHERE SesionCajaId = ?";

        using var cmdCerrar = new OleDbCommand(sqlCerrar, conexion);
        cmdCerrar.Parameters.Add("@FechaCierre", OleDbType.Date).Value = DateTime.Now;
        cmdCerrar.Parameters.Add("@MontoEsperado", OleDbType.Currency).Value = montoEsperado;
        cmdCerrar.Parameters.Add("@MontoContado", OleDbType.Currency).Value = request.MontoContado;
        cmdCerrar.Parameters.Add("@Diferencia", OleDbType.Currency).Value = diferencia;
        cmdCerrar.Parameters.Add("@Estado", OleDbType.VarWChar).Value = "Cerrada";
        cmdCerrar.Parameters.Add("@Observaciones", OleDbType.LongVarWChar).Value = request.Observaciones ?? "";
        cmdCerrar.Parameters.Add("@SesionCajaId", OleDbType.Integer).Value = request.SesionCajaId;

        await cmdCerrar.ExecuteNonQueryAsync();
    }

}
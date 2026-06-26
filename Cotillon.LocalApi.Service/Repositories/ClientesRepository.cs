using Cotillon.LocalApi.Service.Data;
using Cotillon.Shared.DTOs.Clientes;
using System.Data.OleDb;

namespace Cotillon.LocalApi.Service.Repositories;

public class ClientesRepository
{
    private readonly AccessDb _db;

    public ClientesRepository(AccessDb db)
    {
        _db = db;
    }

    public async Task<List<ClienteDto>> ListarAsync()
    {
        List<ClienteDto> lista = new();

        using var conexionBase = _db.CreateConnection();
        var conexion = (OleDbConnection)conexionBase;
        await conexion.OpenAsync();

        string sql = @"
            SELECT ClienteId, NombreCompleto, Telefono, Email, Direccion, Documento, Observaciones, Activo, FechaAlta, FechaActualizacion
            FROM Clientes
            WHERE Activo = True
            ORDER BY NombreCompleto";

        using var cmd = new OleDbCommand(sql, conexion);
        using var lector = await cmd.ExecuteReaderAsync();

        while (await lector.ReadAsync())
        {
            lista.Add(new ClienteDto
            {
                ClienteId = Convert.ToInt32(lector["ClienteId"]),
                NombreCompleto = lector["NombreCompleto"]?.ToString() ?? "",
                Telefono = lector["Telefono"]?.ToString() ?? "",
                Email = lector["Email"]?.ToString() ?? "",
                Direccion = lector["Direccion"]?.ToString() ?? "",
                Documento = lector["Documento"]?.ToString() ?? "",
                Observaciones = lector["Observaciones"]?.ToString() ?? "",
                Activo = lector["Activo"] != DBNull.Value && Convert.ToBoolean(lector["Activo"]),
                FechaAlta = lector["FechaAlta"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(lector["FechaAlta"]),
                FechaActualizacion = lector["FechaActualizacion"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(lector["FechaActualizacion"])
            });
        }

        return lista;
    }

    public async Task<bool> ExisteDocumentoAsync(string documento, int clienteIdExcluir = 0)
    {
        if (string.IsNullOrWhiteSpace(documento))
            return false;

        using var conexionBase = _db.CreateConnection();
        var conexion = (OleDbConnection)conexionBase;
        await conexion.OpenAsync();

        string sql = @"
            SELECT COUNT(*)
            FROM Clientes
            WHERE Documento = ? AND ClienteId <> ?";

        using var cmd = new OleDbCommand(sql, conexion);
        cmd.Parameters.Add("@Documento", OleDbType.VarWChar).Value = documento.Trim();
        cmd.Parameters.Add("@ClienteId", OleDbType.Integer).Value = clienteIdExcluir;

        int cantidad = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        return cantidad > 0;
    }

    public async Task<bool> ExisteEmailAsync(string email, int clienteIdExcluir = 0)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        using var conexionBase = _db.CreateConnection();
        var conexion = (OleDbConnection)conexionBase;
        await conexion.OpenAsync();

        string sql = @"
            SELECT COUNT(*)
            FROM Clientes
            WHERE Email = ? AND ClienteId <> ?";

        using var cmd = new OleDbCommand(sql, conexion);
        cmd.Parameters.Add("@Email", OleDbType.VarWChar).Value = email.Trim();
        cmd.Parameters.Add("@ClienteId", OleDbType.Integer).Value = clienteIdExcluir;

        int cantidad = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        return cantidad > 0;
    }

    public async Task CrearAsync(ClienteDto cliente)
    {
        using var conexionBase = _db.CreateConnection();
        var conexion = (OleDbConnection)conexionBase;
        await conexion.OpenAsync();

        string sql = @"
            INSERT INTO Clientes
            (NombreCompleto, Telefono, Email, Direccion, Documento, Observaciones, Activo, FechaAlta, FechaActualizacion)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)";

        using var cmd = new OleDbCommand(sql, conexion);
        cmd.Parameters.Add("@NombreCompleto", OleDbType.VarWChar).Value = cliente.NombreCompleto.Trim();
        cmd.Parameters.Add("@Telefono", OleDbType.VarWChar).Value = cliente.Telefono.Trim();
        cmd.Parameters.Add("@Email", OleDbType.VarWChar).Value = cliente.Email.Trim();
        cmd.Parameters.Add("@Direccion", OleDbType.VarWChar).Value = cliente.Direccion.Trim();
        cmd.Parameters.Add("@Documento", OleDbType.VarWChar).Value = cliente.Documento.Trim();
        cmd.Parameters.Add("@Observaciones", OleDbType.LongVarWChar).Value = cliente.Observaciones.Trim();
        cmd.Parameters.Add("@Activo", OleDbType.Boolean).Value = true;
        cmd.Parameters.Add("@FechaAlta", OleDbType.Date).Value = DateTime.Now;
        cmd.Parameters.Add("@FechaActualizacion", OleDbType.Date).Value = DateTime.Now;

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task ActualizarAsync(ClienteDto cliente)
    {
        using var conexionBase = _db.CreateConnection();
        var conexion = (OleDbConnection)conexionBase;
        await conexion.OpenAsync();

        string sql = @"
            UPDATE Clientes
            SET NombreCompleto = ?, Telefono = ?, Email = ?, Direccion = ?, Documento = ?, Observaciones = ?, FechaActualizacion = ?
            WHERE ClienteId = ?";

        using var cmd = new OleDbCommand(sql, conexion);
        cmd.Parameters.Add("@NombreCompleto", OleDbType.VarWChar).Value = cliente.NombreCompleto.Trim();
        cmd.Parameters.Add("@Telefono", OleDbType.VarWChar).Value = cliente.Telefono.Trim();
        cmd.Parameters.Add("@Email", OleDbType.VarWChar).Value = cliente.Email.Trim();
        cmd.Parameters.Add("@Direccion", OleDbType.VarWChar).Value = cliente.Direccion.Trim();
        cmd.Parameters.Add("@Documento", OleDbType.VarWChar).Value = cliente.Documento.Trim();
        cmd.Parameters.Add("@Observaciones", OleDbType.LongVarWChar).Value = cliente.Observaciones.Trim();
        cmd.Parameters.Add("@FechaActualizacion", OleDbType.Date).Value = DateTime.Now;
        cmd.Parameters.Add("@ClienteId", OleDbType.Integer).Value = cliente.ClienteId;

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task EliminarAsync(int clienteId)
    {
        using var conexionBase = _db.CreateConnection();
        var conexion = (OleDbConnection)conexionBase;
        await conexion.OpenAsync();

        string sql = @"
            UPDATE Clientes
            SET Activo = False, FechaActualizacion = ?
            WHERE ClienteId = ?";

        using var cmd = new OleDbCommand(sql, conexion);
        cmd.Parameters.Add("@FechaActualizacion", OleDbType.Date).Value = DateTime.Now;
        cmd.Parameters.Add("@ClienteId", OleDbType.Integer).Value = clienteId;

        await cmd.ExecuteNonQueryAsync();
    }
}
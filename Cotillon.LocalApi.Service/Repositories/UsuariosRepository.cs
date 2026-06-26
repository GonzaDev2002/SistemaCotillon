using Cotillon.LocalApi.Service.Data;
using Cotillon.Shared.DTOs.Usuarios;
using System.Data.OleDb;

namespace Cotillon.LocalApi.Service.Repositories;

public class UsuariosRepository
{
    private readonly AccessDb _db;

    public UsuariosRepository(AccessDb db)
    {
        _db = db;
    }

    public async Task<List<UsuarioDto>> ListarAsync(bool incluirInactivos)
    {
        List<UsuarioDto> lista = new();

        using var conexion = (OleDbConnection)_db.CreateConnection();
        await conexion.OpenAsync();

        string sql = @"
            SELECT UsuarioId, NombreUsuario, NombreCompleto, Pin, Rol, Activo, FechaAlta, FechaActualizacion
            FROM Usuarios";

        if (!incluirInactivos)
            sql += " WHERE Activo = True";

        sql += " ORDER BY NombreCompleto";

        using var cmd = new OleDbCommand(sql, conexion);
        using var lector = await cmd.ExecuteReaderAsync();

        while (await lector.ReadAsync())
        {
            lista.Add(new UsuarioDto
            {
                UsuarioId = Convert.ToInt32(lector["UsuarioId"]),
                NombreUsuario = lector["NombreUsuario"]?.ToString() ?? "",
                NombreCompleto = lector["NombreCompleto"]?.ToString() ?? "",
                Pin = lector["Pin"]?.ToString() ?? "",
                Rol = lector["Rol"]?.ToString() ?? "",
                Activo = lector["Activo"] != DBNull.Value && Convert.ToBoolean(lector["Activo"]),
                FechaAlta = lector["FechaAlta"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(lector["FechaAlta"]),
                FechaActualizacion = lector["FechaActualizacion"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(lector["FechaActualizacion"])
            });
        }

        return lista;
    }

    public async Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, int excluirUsuarioId = 0)
    {
        using var conexion = (OleDbConnection)_db.CreateConnection();
        await conexion.OpenAsync();

        string sql = @"
            SELECT COUNT(*)
            FROM Usuarios
            WHERE NombreUsuario = ? AND UsuarioId <> ?";

        using var cmd = new OleDbCommand(sql, conexion);
        cmd.Parameters.Add("@NombreUsuario", OleDbType.VarWChar).Value = nombreUsuario.Trim();
        cmd.Parameters.Add("@UsuarioId", OleDbType.Integer).Value = excluirUsuarioId;

        return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
    }

    public async Task CrearAsync(UsuarioDto usuario)
    {
        using var conexion = (OleDbConnection)_db.CreateConnection();
        await conexion.OpenAsync();

        string sql = @"
            INSERT INTO Usuarios
            (NombreUsuario, NombreCompleto, Pin, Rol, Activo, FechaAlta, FechaActualizacion)
            VALUES (?, ?, ?, ?, ?, ?, ?)";

        using var cmd = new OleDbCommand(sql, conexion);
        cmd.Parameters.Add("@NombreUsuario", OleDbType.VarWChar).Value = usuario.NombreUsuario.Trim();
        cmd.Parameters.Add("@NombreCompleto", OleDbType.VarWChar).Value = usuario.NombreCompleto.Trim();
        cmd.Parameters.Add("@Pin", OleDbType.VarWChar).Value = usuario.Pin.Trim();
        cmd.Parameters.Add("@Rol", OleDbType.VarWChar).Value = usuario.Rol.Trim();
        cmd.Parameters.Add("@Activo", OleDbType.Boolean).Value = true;
        cmd.Parameters.Add("@FechaAlta", OleDbType.Date).Value = DateTime.Now;
        cmd.Parameters.Add("@FechaActualizacion", OleDbType.Date).Value = DateTime.Now;

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task ActualizarAsync(UsuarioDto usuario)
    {
        using var conexion = (OleDbConnection)_db.CreateConnection();
        await conexion.OpenAsync();

        string sql = @"
            UPDATE Usuarios
            SET NombreUsuario = ?, NombreCompleto = ?, Pin = ?, Rol = ?, Activo = ?, FechaActualizacion = ?
            WHERE UsuarioId = ?";

        using var cmd = new OleDbCommand(sql, conexion);
        cmd.Parameters.Add("@NombreUsuario", OleDbType.VarWChar).Value = usuario.NombreUsuario.Trim();
        cmd.Parameters.Add("@NombreCompleto", OleDbType.VarWChar).Value = usuario.NombreCompleto.Trim();
        cmd.Parameters.Add("@Pin", OleDbType.VarWChar).Value = usuario.Pin.Trim();
        cmd.Parameters.Add("@Rol", OleDbType.VarWChar).Value = usuario.Rol.Trim();
        cmd.Parameters.Add("@Activo", OleDbType.Boolean).Value = usuario.Activo;
        cmd.Parameters.Add("@FechaActualizacion", OleDbType.Date).Value = DateTime.Now;
        cmd.Parameters.Add("@UsuarioId", OleDbType.Integer).Value = usuario.UsuarioId;

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task CambiarEstadoAsync(int usuarioId, bool activo)
    {
        using var conexion = (OleDbConnection)_db.CreateConnection();
        await conexion.OpenAsync();

        string sql = @"
            UPDATE Usuarios
            SET Activo = ?, FechaActualizacion = ?
            WHERE UsuarioId = ?";

        using var cmd = new OleDbCommand(sql, conexion);
        cmd.Parameters.Add("@Activo", OleDbType.Boolean).Value = activo;
        cmd.Parameters.Add("@FechaActualizacion", OleDbType.Date).Value = DateTime.Now;
        cmd.Parameters.Add("@UsuarioId", OleDbType.Integer).Value = usuarioId;

        await cmd.ExecuteNonQueryAsync();
    }
}
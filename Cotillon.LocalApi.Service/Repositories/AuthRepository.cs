using Cotillon.LocalApi.Service.Data;
using Cotillon.Shared.DTOs;
using System.Data.OleDb;

namespace Cotillon.LocalApi.Service.Repositories;

public class AuthRepository
{
    private readonly AccessDb _accessDb;

    public AuthRepository(AccessDb accessDb)
    {
        _accessDb = accessDb;
    }

    public LoginResponseDto? Login(string username, string pin)
    {
        using var connection = (OleDbConnection)_accessDb.CreateConnection();
        connection.Open();

        const string sql = @"
SELECT 
    [UsuarioId],
    [NombreUsuario],
    [NombreCompleto],
    [Rol]
FROM [Usuarios]
WHERE [NombreUsuario] = ?
  AND [Pin] = ?
  AND [Activo] = True";

        using var command = new OleDbCommand(sql, connection);
        command.Parameters.AddWithValue("?", username);
        command.Parameters.AddWithValue("?", pin);

        using var reader = command.ExecuteReader();

        if (reader == null || !reader.Read())
            return null;

        return new LoginResponseDto
        {
            UsuarioId = Convert.ToInt32(reader["UsuarioId"]),
            NombreUsuario = reader["NombreUsuario"]?.ToString() ?? string.Empty,
            NombreCompleto = reader["NombreCompleto"]?.ToString() ?? string.Empty,
            Rol = reader["Rol"]?.ToString() ?? string.Empty
        };
    }
}
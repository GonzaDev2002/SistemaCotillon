using Cotillon.Data;
using Cotillon.Shared.DTOs;
using Dapper;

namespace Cotillon.LocalApi.Service.Repositories;

public class AuthRepository
{
    private readonly IDbConnectionFactory _factory;

    public AuthRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public LoginResponseDto? Login(string username, string pin)
    {
        using var conexion = _factory.Crear();

        var usuario = conexion.QuerySingleOrDefault<UsuarioCredenciales>(
            """
            SELECT UsuarioId, NombreUsuario, NombreCompleto, Rol, PinHash, PinSalt
            FROM Usuarios
            WHERE NombreUsuario = @Usuario AND Activo = 1;
            """,
            new { Usuario = username });

        if (usuario is null || !PinHasher.Verificar(pin, usuario.PinHash, usuario.PinSalt))
            return null;

        conexion.Execute(
            "UPDATE Usuarios SET UltimoAcceso = datetime('now', 'localtime') WHERE UsuarioId = @Id;",
            new { Id = usuario.UsuarioId });

        return new LoginResponseDto
        {
            UsuarioId = usuario.UsuarioId,
            NombreUsuario = usuario.NombreUsuario,
            NombreCompleto = usuario.NombreCompleto,
            Rol = usuario.Rol
        };
    }

    private sealed class UsuarioCredenciales
    {
        public int UsuarioId { get; init; }
        public string NombreUsuario { get; init; } = string.Empty;
        public string NombreCompleto { get; init; } = string.Empty;
        public string Rol { get; init; } = string.Empty;
        public string PinHash { get; init; } = string.Empty;
        public string PinSalt { get; init; } = string.Empty;
    }
}

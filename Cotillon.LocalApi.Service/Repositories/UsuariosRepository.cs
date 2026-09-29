using Cotillon.Data;
using Cotillon.Shared.DTOs.Usuarios;
using Dapper;

namespace Cotillon.LocalApi.Service.Repositories;

public class UsuarioInvalidoException : Exception
{
    public UsuarioInvalidoException(string mensaje) : base(mensaje) { }
}

public class UsuariosRepository
{
    private readonly IDbConnectionFactory _factory;

    public UsuariosRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public List<UsuarioDto> ObtenerTodos(bool incluirInactivos)
    {
        using var conexion = _factory.Crear();

        return conexion.Query<UsuarioDto>(
            $"""
            SELECT UsuarioId, NombreUsuario, NombreCompleto, Rol, Activo, FechaAlta, UltimoAcceso
            FROM Usuarios
            {(incluirInactivos ? string.Empty : "WHERE Activo = 1")}
            ORDER BY NombreCompleto;
            """).ToList();
    }

    public UsuarioDto? ObtenerPorId(int usuarioId)
    {
        using var conexion = _factory.Crear();

        return conexion.QuerySingleOrDefault<UsuarioDto>(
            """
            SELECT UsuarioId, NombreUsuario, NombreCompleto, Rol, Activo, FechaAlta, UltimoAcceso
            FROM Usuarios
            WHERE UsuarioId = @Id;
            """,
            new { Id = usuarioId });
    }

    public int Crear(CrearUsuarioDto usuario, int usuarioEjecutor)
    {
        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        var nombre = usuario.NombreUsuario.Trim();

        var existe = conexion.ExecuteScalar<long>(
            "SELECT COUNT(*) FROM Usuarios WHERE NombreUsuario = @Nombre;", new { Nombre = nombre }, tx) > 0;

        if (existe)
            throw new UsuarioInvalidoException($"Ya existe un usuario llamado '{nombre}'.");

        var (hash, salt) = PinHasher.Hashear(usuario.Pin);

        var id = conexion.ExecuteScalar<int>(
            """
            INSERT INTO Usuarios (NombreUsuario, NombreCompleto, PinHash, PinSalt, Rol, Activo)
            VALUES (@Nombre, @NombreCompleto, @Hash, @Salt, @Rol, @Activo);
            SELECT last_insert_rowid();
            """,
            new
            {
                Nombre = nombre,
                NombreCompleto = usuario.NombreCompleto.Trim(),
                Hash = hash,
                Salt = salt,
                usuario.Rol,
                Activo = usuario.Activo ? 1 : 0
            }, tx);

        Auditoria.Registrar(conexion, tx, usuarioEjecutor, "Usuarios", "ALTA", id, $"{nombre} ({usuario.Rol})");

        tx.Commit();

        return id;
    }

    public bool Actualizar(UsuarioDto usuario, int usuarioEjecutor)
    {
        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        // No se puede dejar el sistema sin ningún administrador activo con el que entrar.
        if (!usuario.Activo || usuario.Rol != "Admin")
            GarantizarQueQuedaUnAdmin(conexion, tx, usuario.UsuarioId);

        var filas = conexion.Execute(
            """
            UPDATE Usuarios SET
                NombreCompleto = @NombreCompleto,
                Rol = @Rol,
                Activo = @Activo,
                FechaActualizacion = datetime('now', 'localtime')
            WHERE UsuarioId = @UsuarioId;
            """,
            new
            {
                usuario.UsuarioId,
                NombreCompleto = usuario.NombreCompleto.Trim(),
                usuario.Rol,
                Activo = usuario.Activo ? 1 : 0
            }, tx);

        if (filas > 0)
            Auditoria.Registrar(conexion, tx, usuarioEjecutor, "Usuarios", "MODIFICACION", usuario.UsuarioId,
                $"{usuario.NombreCompleto} ({usuario.Rol}, {(usuario.Activo ? "activo" : "inactivo")})");

        tx.Commit();

        return filas > 0;
    }

    public void CambiarPin(CambiarPinDto datos, bool exigirPinActual)
    {
        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        var credenciales = conexion.QuerySingleOrDefault<Credenciales>(
            "SELECT PinHash, PinSalt FROM Usuarios WHERE UsuarioId = @Id;", new { Id = datos.UsuarioId }, tx)
            ?? throw new UsuarioInvalidoException("No se encontró el usuario.");

        if (exigirPinActual && !PinHasher.Verificar(datos.PinActual, credenciales.PinHash, credenciales.PinSalt))
            throw new UsuarioInvalidoException("El PIN actual no es correcto.");

        var (hash, salt) = PinHasher.Hashear(datos.PinNuevo);

        conexion.Execute(
            """
            UPDATE Usuarios
            SET PinHash = @Hash, PinSalt = @Salt, FechaActualizacion = datetime('now', 'localtime')
            WHERE UsuarioId = @Id;
            """,
            new { Hash = hash, Salt = salt, Id = datos.UsuarioId }, tx);

        Auditoria.Registrar(conexion, tx, datos.UsuarioId, "Usuarios", "MODIFICACION", datos.UsuarioId,
            "Cambio de PIN");

        tx.Commit();
    }

    private static void GarantizarQueQuedaUnAdmin(
        System.Data.IDbConnection conexion, System.Data.IDbTransaction tx, int usuarioId)
    {
        var otrosAdmins = conexion.ExecuteScalar<long>(
            """
            SELECT COUNT(*) FROM Usuarios
            WHERE Rol = 'Admin' AND Activo = 1 AND UsuarioId <> @Id;
            """,
            new { Id = usuarioId }, tx);

        if (otrosAdmins == 0)
            throw new UsuarioInvalidoException(
                "Tiene que quedar al menos un administrador activo: si no, nadie podría volver a entrar al sistema.");
    }

    private sealed class Credenciales
    {
        public string PinHash { get; init; } = string.Empty;
        public string PinSalt { get; init; } = string.Empty;
    }
}

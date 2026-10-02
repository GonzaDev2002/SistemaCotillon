using Cotillon.Data;
using Cotillon.Shared.DTOs;
using Dapper;

namespace Cotillon.LocalApi.Service.Repositories;

public class SeccionesRepository
{
    private readonly IDbConnectionFactory _factory;

    public SeccionesRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public List<SeccionDto> ObtenerTodas()
    {
        using var conexion = _factory.Crear();

        return conexion.Query<SeccionDto>(
            """
            SELECT
                s.SeccionId,
                s.NombreSeccion,
                s.Activo,
                (SELECT COUNT(*) FROM Categorias c
                 WHERE c.SeccionId = s.SeccionId)                       AS CantidadCategorias,
                (SELECT COUNT(*) FROM Productos p
                 JOIN Categorias c ON p.CategoriaId = c.CategoriaId
                 WHERE c.SeccionId = s.SeccionId AND p.Activo = 1)      AS CantidadProductos
            FROM Secciones s
            ORDER BY s.NombreSeccion;
            """).ToList();
    }

    /// <summary>
    /// Borra la sección sólo si no quedó nada colgando. La FK con ON DELETE RESTRICT
    /// ya lo impediría, pero preguntando antes se puede explicar el motivo en vez
    /// de mostrarle al usuario un error de la base.
    /// </summary>
    public bool Eliminar(int seccionId)
    {
        using var conexion = _factory.Crear();

        return conexion.Execute(
            "DELETE FROM Secciones WHERE SeccionId = @SeccionId;",
            new { SeccionId = seccionId }) > 0;
    }

    public int ContarCategorias(int seccionId)
    {
        using var conexion = _factory.Crear();

        return (int)conexion.ExecuteScalar<long>(
            "SELECT COUNT(*) FROM Categorias WHERE SeccionId = @SeccionId;",
            new { SeccionId = seccionId });
    }

    public bool Insertar(SeccionDto seccion)
    {
        using var conexion = _factory.Crear();

        return conexion.Execute(
            """
            INSERT INTO Secciones (NombreSeccion, Activo)
            VALUES (@NombreSeccion, @Activo);
            """,
            new { seccion.NombreSeccion, Activo = seccion.Activo ? 1 : 0 }) > 0;
    }

    public bool Actualizar(SeccionDto seccion)
    {
        using var conexion = _factory.Crear();

        return conexion.Execute(
            """
            UPDATE Secciones
            SET NombreSeccion = @NombreSeccion, Activo = @Activo
            WHERE SeccionId = @SeccionId;
            """,
            new { seccion.NombreSeccion, Activo = seccion.Activo ? 1 : 0, seccion.SeccionId }) > 0;
    }

    public bool ExisteNombre(string nombre, int excluirId = 0)
    {
        using var conexion = _factory.Crear();

        return conexion.ExecuteScalar<long>(
            """
            SELECT COUNT(*) FROM Secciones
            WHERE NombreSeccion = @Nombre AND SeccionId <> @ExcluirId;
            """,
            new { Nombre = nombre, ExcluirId = excluirId }) > 0;
    }
}

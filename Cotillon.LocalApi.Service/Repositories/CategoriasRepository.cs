using Cotillon.Data;
using Cotillon.Shared.DTOs;
using Dapper;

namespace Cotillon.LocalApi.Service.Repositories;

public class CategoriasRepository
{
    private readonly IDbConnectionFactory _factory;

    public CategoriasRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public List<CategoriaDto> ObtenerTodas()
    {
        using var conexion = _factory.Crear();

        return conexion.Query<CategoriaDto>(
            $"""
            {Consulta}
            ORDER BY s.NombreSeccion, c.NombreCategoria;
            """).ToList();
    }

    public List<CategoriaDto> ObtenerPorSeccion(int seccionId)
    {
        using var conexion = _factory.Crear();

        return conexion.Query<CategoriaDto>(
            $"""
            {Consulta}
            WHERE c.SeccionId = @SeccionId
            ORDER BY c.NombreCategoria;
            """,
            new { SeccionId = seccionId }).ToList();
    }

    private const string Consulta =
        """
        SELECT
            c.CategoriaId,
            c.SeccionId,
            c.NombreCategoria,
            c.Activo,
            COALESCE(s.NombreSeccion, '(Sin sección)') AS NombreSeccion,
            (SELECT COUNT(*) FROM Productos p
             WHERE p.CategoriaId = c.CategoriaId AND p.Activo = 1) AS CantidadProductos
        FROM Categorias c
        LEFT JOIN Secciones s ON c.SeccionId = s.SeccionId
        """;

    /// <summary>
    /// Igual que en secciones: se borra sólo si no tiene productos, para poder dar
    /// un mensaje claro en lugar del error de clave foránea.
    /// </summary>
    public bool Eliminar(int categoriaId)
    {
        using var conexion = _factory.Crear();

        return conexion.Execute(
            "DELETE FROM Categorias WHERE CategoriaId = @CategoriaId;",
            new { CategoriaId = categoriaId }) > 0;
    }

    public int ContarProductos(int categoriaId)
    {
        using var conexion = _factory.Crear();

        return (int)conexion.ExecuteScalar<long>(
            "SELECT COUNT(*) FROM Productos WHERE CategoriaId = @CategoriaId;",
            new { CategoriaId = categoriaId });
    }

    public bool Insertar(CategoriaDto categoria)
    {
        using var conexion = _factory.Crear();

        return conexion.Execute(
            """
            INSERT INTO Categorias (SeccionId, NombreCategoria, Activo)
            VALUES (@SeccionId, @NombreCategoria, @Activo);
            """,
            new { categoria.SeccionId, categoria.NombreCategoria, Activo = categoria.Activo ? 1 : 0 }) > 0;
    }

    public bool Actualizar(CategoriaDto categoria)
    {
        using var conexion = _factory.Crear();

        return conexion.Execute(
            """
            UPDATE Categorias
            SET SeccionId = @SeccionId, NombreCategoria = @NombreCategoria, Activo = @Activo
            WHERE CategoriaId = @CategoriaId;
            """,
            new
            {
                categoria.SeccionId,
                categoria.NombreCategoria,
                Activo = categoria.Activo ? 1 : 0,
                categoria.CategoriaId
            }) > 0;
    }

    public bool ExisteNombreEnSeccion(int seccionId, string nombre, int excluirId = 0)
    {
        using var conexion = _factory.Crear();

        return conexion.ExecuteScalar<long>(
            """
            SELECT COUNT(*) FROM Categorias
            WHERE SeccionId = @SeccionId AND NombreCategoria = @Nombre AND CategoriaId <> @ExcluirId;
            """,
            new { SeccionId = seccionId, Nombre = nombre, ExcluirId = excluirId }) > 0;
    }
}

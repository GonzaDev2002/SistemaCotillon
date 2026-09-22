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
            """
            SELECT CategoriaId, SeccionId, NombreCategoria, Activo
            FROM Categorias
            ORDER BY NombreCategoria;
            """).ToList();
    }

    public List<CategoriaDto> ObtenerPorSeccion(int seccionId)
    {
        using var conexion = _factory.Crear();

        return conexion.Query<CategoriaDto>(
            """
            SELECT CategoriaId, SeccionId, NombreCategoria, Activo
            FROM Categorias
            WHERE SeccionId = @SeccionId
            ORDER BY NombreCategoria;
            """,
            new { SeccionId = seccionId }).ToList();
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

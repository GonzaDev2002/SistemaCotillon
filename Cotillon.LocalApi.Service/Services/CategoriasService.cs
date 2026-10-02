using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs;
using Cotillon.Shared.Responses;

namespace Cotillon.LocalApi.Service.Services;

public class CategoriasService
{
    private readonly CategoriasRepository _repository;

    public CategoriasService(CategoriasRepository repository)
    {
        _repository = repository;
    }

    public ApiResponse<List<CategoriaDto>> ObtenerTodas()
    {
        var lista = _repository.ObtenerTodas();
        return ApiResponse<List<CategoriaDto>>.Ok(lista, "Categorías obtenidas correctamente.");
    }

    public ApiResponse<List<CategoriaDto>> ObtenerPorSeccion(int seccionId)
    {
        if (seccionId <= 0)
            return ApiResponse<List<CategoriaDto>>.Fail("La sección es obligatoria.");

        var lista = _repository.ObtenerPorSeccion(seccionId);
        return ApiResponse<List<CategoriaDto>>.Ok(lista, "Categorías obtenidas correctamente.");
    }

    public ApiResponse<string> Crear(CategoriaDto categoria)
    {
        if (categoria.SeccionId <= 0)
            return ApiResponse<string>.Fail("La sección es obligatoria.");

        if (string.IsNullOrWhiteSpace(categoria.NombreCategoria))
            return ApiResponse<string>.Fail("El nombre de la categoría es obligatorio.");

        categoria.NombreCategoria = categoria.NombreCategoria.Trim();

        if (_repository.ExisteNombreEnSeccion(categoria.SeccionId, categoria.NombreCategoria))
            return ApiResponse<string>.Fail(
                $"Ya existe una categoría llamada '{categoria.NombreCategoria}' en esa sección.");

        var ok = _repository.Insertar(categoria);

        if (!ok)
            return ApiResponse<string>.Fail("No se pudo guardar la categoría.");

        return ApiResponse<string>.Ok("OK", "Categoría agregada con éxito.");
    }

    public ApiResponse<string> Actualizar(CategoriaDto categoria)
    {
        if (categoria.CategoriaId <= 0)
            return ApiResponse<string>.Fail("La categoría es inválida.");

        if (categoria.SeccionId <= 0)
            return ApiResponse<string>.Fail("La sección es obligatoria.");

        if (string.IsNullOrWhiteSpace(categoria.NombreCategoria))
            return ApiResponse<string>.Fail("El nombre de la categoría es obligatorio.");

        categoria.NombreCategoria = categoria.NombreCategoria.Trim();

        if (_repository.ExisteNombreEnSeccion(categoria.SeccionId, categoria.NombreCategoria, categoria.CategoriaId))
            return ApiResponse<string>.Fail(
                $"Ya existe otra categoría llamada '{categoria.NombreCategoria}' en esa sección.");

        var ok = _repository.Actualizar(categoria);

        if (!ok)
            return ApiResponse<string>.Fail("No se pudo actualizar la categoría.");

        return ApiResponse<string>.Ok("OK", "Categoría actualizada con éxito.");
    }

    /// <summary>
    /// Borra la categoría sólo si no tiene productos, incluidos los dados de baja:
    /// esos productos siguen apareciendo en el historial de ventas y necesitan su
    /// categoría para mostrarse.
    /// </summary>
    public ApiResponse<string> Eliminar(int categoriaId)
    {
        if (categoriaId <= 0)
            return ApiResponse<string>.Fail("La categoría es inválida.");

        var productos = _repository.ContarProductos(categoriaId);

        if (productos > 0)
            return ApiResponse<string>.Fail(
                $"La categoría tiene {productos} producto(s). Pasalos a otra categoría primero, " +
                "o desactivala para que deje de ofrecerse al cargar productos nuevos.");

        return _repository.Eliminar(categoriaId)
            ? ApiResponse<string>.Ok("OK", "Categoría borrada.")
            : ApiResponse<string>.Fail("No se encontró la categoría.");
    }
}
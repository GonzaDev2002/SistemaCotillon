using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs;
using Cotillon.Shared.Responses;

namespace Cotillon.LocalApi.Service.Services;

public class SeccionesService
{
    private readonly SeccionesRepository _repository;

    public SeccionesService(SeccionesRepository repository)
    {
        _repository = repository;
    }

    public ApiResponse<List<SeccionDto>> ObtenerTodas()
    {
        var lista = _repository.ObtenerTodas();
        return ApiResponse<List<SeccionDto>>.Ok(lista, "Secciones obtenidas correctamente.");
    }

    public ApiResponse<string> Crear(SeccionDto seccion)
    {
        if (string.IsNullOrWhiteSpace(seccion.NombreSeccion))
            return ApiResponse<string>.Fail("El nombre de la sección es obligatorio.");

        seccion.NombreSeccion = seccion.NombreSeccion.Trim();

        if (_repository.ExisteNombre(seccion.NombreSeccion))
            return ApiResponse<string>.Fail($"Ya existe una sección llamada '{seccion.NombreSeccion}'.");

        var ok = _repository.Insertar(seccion);

        if (!ok)
            return ApiResponse<string>.Fail("No se pudo guardar la sección.");

        return ApiResponse<string>.Ok("OK", "Sección agregada con éxito.");
    }

    public ApiResponse<string> Actualizar(SeccionDto seccion)
    {
        if (seccion.SeccionId <= 0)
            return ApiResponse<string>.Fail("La sección es inválida.");

        if (string.IsNullOrWhiteSpace(seccion.NombreSeccion))
            return ApiResponse<string>.Fail("El nombre de la sección es obligatorio.");

        seccion.NombreSeccion = seccion.NombreSeccion.Trim();

        if (_repository.ExisteNombre(seccion.NombreSeccion, seccion.SeccionId))
            return ApiResponse<string>.Fail($"Ya existe otra sección llamada '{seccion.NombreSeccion}'.");

        var ok = _repository.Actualizar(seccion);

        if (!ok)
            return ApiResponse<string>.Fail("No se pudo actualizar la sección.");

        return ApiResponse<string>.Ok("OK", "Sección actualizada con éxito.");
    }

    /// <summary>
    /// Borra la sección sólo si está vacía. Una sección con categorías se desactiva
    /// en lugar de borrarse: sus productos siguen existiendo en ventas históricas
    /// y perder el nombre dejaría reportes viejos sin explicación.
    /// </summary>
    public ApiResponse<string> Eliminar(int seccionId)
    {
        if (seccionId <= 0)
            return ApiResponse<string>.Fail("La sección es inválida.");

        var categorias = _repository.ContarCategorias(seccionId);

        if (categorias > 0)
            return ApiResponse<string>.Fail(
                $"La sección tiene {categorias} categoría(s). Movelas o borralas primero, " +
                "o desactivá la sección para que deje de aparecer al cargar productos.");

        return _repository.Eliminar(seccionId)
            ? ApiResponse<string>.Ok("OK", "Sección borrada.")
            : ApiResponse<string>.Fail("No se encontró la sección.");
    }
}
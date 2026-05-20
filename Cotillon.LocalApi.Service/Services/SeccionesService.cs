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

        var ok = _repository.Actualizar(seccion);

        if (!ok)
            return ApiResponse<string>.Fail("No se pudo actualizar la sección.");

        return ApiResponse<string>.Ok("OK", "Sección actualizada con éxito.");
    }
}
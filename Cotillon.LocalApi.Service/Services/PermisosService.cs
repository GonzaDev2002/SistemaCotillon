using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs.Permisos;
using Cotillon.Shared.Responses;

namespace Cotillon.LocalApi.Service.Services;

public class PermisosService
{
    private readonly PermisosRepository _repository;
    private readonly UsuariosRepository _usuarios;

    public PermisosService(PermisosRepository repository, UsuariosRepository usuarios)
    {
        _repository = repository;
        _usuarios = usuarios;
    }

    public ApiResponse<Dictionary<string, NivelAcceso>> ObtenerDe(int usuarioId)
    {
        if (usuarioId <= 0)
            return ApiResponse<Dictionary<string, NivelAcceso>>.Fail("El usuario es inválido.");

        return ApiResponse<Dictionary<string, NivelAcceso>>.Ok(
            _repository.ObtenerDe(usuarioId), "Permisos obtenidos correctamente.");
    }

    public ApiResponse<Dictionary<string, NivelAcceso>> Plantilla(string rol) =>
        ApiResponse<Dictionary<string, NivelAcceso>>.Ok(
            PlantillasDeRol.Para(rol), $"Permisos sugeridos para el rol {rol}.");

    public ApiResponse<string> Guardar(
        int usuarioId, Dictionary<string, NivelAcceso>? permisos, int usuarioEjecutor)
    {
        if (usuarioId <= 0)
            return ApiResponse<string>.Fail("El usuario es inválido.");

        if (permisos is null || permisos.Count == 0)
            return ApiResponse<string>.Fail("No llegó ningún permiso para guardar.");

        var usuario = _usuarios.ObtenerPorId(usuarioId);

        if (usuario is null)
            return ApiResponse<string>.Fail("No se encontró el usuario.");

        var desconocidos = permisos.Keys.Where(m => !Modulos.Existe(m)).ToList();

        if (desconocidos.Count > 0)
            return ApiResponse<string>.Fail($"Módulos desconocidos: {string.Join(", ", desconocidos)}.");

        var limpios = permisos.ToDictionary(
            p => p.Key.ToUpperInvariant(),
            p => Enum.IsDefined(p.Value) ? p.Value : NivelAcceso.Ninguno);

        // Un usuario sin un solo módulo visible entra y se encuentra con la nada.
        // Es casi siempre un error de carga, así que se avisa en vez de guardarlo.
        if (limpios.Values.All(n => n == NivelAcceso.Ninguno))
            return ApiResponse<string>.Fail(
                "El usuario quedaría sin ningún módulo visible. Dale acceso al menos a uno.");

        _repository.Guardar(usuarioId, limpios, usuarioEjecutor);

        return ApiResponse<string>.Ok("OK", $"Permisos de {usuario.NombreCompleto} actualizados.");
    }

    /// <summary>
    /// Deja a un usuario recién creado con los permisos de su rol. Se llama en el
    /// alta para que nunca exista un usuario sin ninguna fila de permisos, que en
    /// la práctica sería un usuario que entra y no ve nada.
    ///
    /// No se audita: el alta del usuario ya quedó registrada con su rol, y repetir
    /// los catorce módulos en el historial sólo tapa los cambios que sí importan.
    /// </summary>
    public void SembrarSegunRol(int usuarioId, string rol, int usuarioEjecutor) =>
        _repository.Guardar(usuarioId, PlantillasDeRol.Para(rol), usuarioEjecutor, auditar: false);
}

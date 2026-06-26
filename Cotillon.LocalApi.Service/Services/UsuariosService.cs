using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs.Usuarios;

namespace Cotillon.LocalApi.Service.Services;

public class UsuariosService
{
    private readonly UsuariosRepository _repository;

    public UsuariosService(UsuariosRepository repository)
    {
        _repository = repository;
    }

    public Task<List<UsuarioDto>> ListarAsync(bool incluirInactivos)
    {
        return _repository.ListarAsync(incluirInactivos);
    }

    public async Task CrearAsync(UsuarioDto usuario)
    {
        if (await _repository.ExisteNombreUsuarioAsync(usuario.NombreUsuario))
            throw new Exception("Ya existe un usuario con ese nombre de usuario.");

        await _repository.CrearAsync(usuario);
    }

    public async Task ActualizarAsync(UsuarioDto usuario)
    {
        if (await _repository.ExisteNombreUsuarioAsync(usuario.NombreUsuario, usuario.UsuarioId))
            throw new Exception("Ya existe otro usuario con ese nombre de usuario.");

        await _repository.ActualizarAsync(usuario);
    }

    public Task CambiarEstadoAsync(int usuarioId, bool activo)
    {
        return _repository.CambiarEstadoAsync(usuarioId, activo);
    }
}
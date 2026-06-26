using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs.Clientes;

namespace Cotillon.LocalApi.Service.Services;

public class ClientesService
{
    private readonly ClientesRepository _repository;

    public ClientesService(ClientesRepository repository)
    {
        _repository = repository;
    }

    public Task<List<ClienteDto>> ListarAsync()
    {
        return _repository.ListarAsync();
    }

    public async Task CrearAsync(ClienteDto cliente)
    {
        if (await _repository.ExisteDocumentoAsync(cliente.Documento))
            throw new Exception("Ya existe un cliente con ese documento.");

        if (await _repository.ExisteEmailAsync(cliente.Email))
            throw new Exception("Ya existe un cliente con ese email.");

        await _repository.CrearAsync(cliente);
    }

    public async Task ActualizarAsync(ClienteDto cliente)
    {
        if (await _repository.ExisteDocumentoAsync(cliente.Documento, cliente.ClienteId))
            throw new Exception("Ya existe otro cliente con ese documento.");

        if (await _repository.ExisteEmailAsync(cliente.Email, cliente.ClienteId))
            throw new Exception("Ya existe otro cliente con ese email.");

        await _repository.ActualizarAsync(cliente);
    }

    public Task EliminarAsync(int clienteId)
    {
        return _repository.EliminarAsync(clienteId);
    }
}
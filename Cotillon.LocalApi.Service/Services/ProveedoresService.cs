using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs.Proveedores;

namespace Cotillon.LocalApi.Service.Services;

public class ProveedoresService
{
    private readonly ProveedoresRepository _repository;

    public ProveedoresService(ProveedoresRepository repository)
    {
        _repository = repository;
    }

    public Task<List<ProveedorDto>> ListarAsync()
    {
        return _repository.ListarAsync();
    }

    public Task<int> CrearAsync(ProveedorDto proveedor)
    {
        if (string.IsNullOrWhiteSpace(proveedor.NombreProveedor))
            throw new Exception("Debe ingresar el nombre del proveedor.");

        return _repository.CrearAsync(proveedor);
    }
}

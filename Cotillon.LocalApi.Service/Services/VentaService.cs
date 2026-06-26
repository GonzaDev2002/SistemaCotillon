using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs.Ventas;

namespace Cotillon.LocalApi.Service.Services;

public class VentasService
{
    private readonly VentasRepository _repository;

    public VentasService(VentasRepository repository)
    {
        _repository = repository;
    }

    public Task<int> CrearVentaAsync(CrearVentaRequestDto request)
    {
        return _repository.CrearVentaAsync(request);
    }

    public Task<List<VentaListadoDto>> ListarVentasAsync()
    {
        return _repository.ListarVentasAsync();
    }
}
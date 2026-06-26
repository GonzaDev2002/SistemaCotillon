using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs.Compras;

namespace Cotillon.LocalApi.Service.Services;

public class ComprasService
{
    private readonly ComprasRepository _repository;

    public ComprasService(ComprasRepository repository)
    {
        _repository = repository;
    }

    public Task<List<CompraListadoDto>> ListarAsync()
    {
        return _repository.ListarAsync();
    }

    public Task<int> CrearCompraAsync(CrearCompraRequestDto request)
    {
        if (request.ProveedorId <= 0)
            throw new Exception("Debe seleccionar un proveedor.");

        if (request.UsuarioId <= 0)
            throw new Exception("El usuario no es válido.");

        if (request.Detalles == null || request.Detalles.Count == 0)
            throw new Exception("Debe agregar al menos un producto.");

        foreach (var detalle in request.Detalles)
        {
            if (detalle.ProductoId <= 0)
                throw new Exception("El producto no es válido.");

            if (detalle.Cantidad <= 0)
                throw new Exception("La cantidad debe ser mayor a cero.");

            if (detalle.CostoUnitario <= 0)
                throw new Exception("El costo unitario debe ser mayor a cero.");
        }

        return _repository.CrearCompraAsync(request);
    }
}

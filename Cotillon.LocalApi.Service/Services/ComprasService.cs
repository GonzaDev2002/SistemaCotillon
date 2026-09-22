using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs.Compras;
using Cotillon.Shared.Responses;

namespace Cotillon.LocalApi.Service.Services;

public class ComprasService
{
    private readonly ComprasRepository _repository;

    public ComprasService(ComprasRepository repository)
    {
        _repository = repository;
    }

    public ApiResponse<CompraDto> Crear(CrearCompraDto compra)
    {
        if (compra.UsuarioId <= 0)
            return ApiResponse<CompraDto>.Fail("El usuario es obligatorio.");

        if (compra.ProveedorId <= 0)
            return ApiResponse<CompraDto>.Fail("El proveedor es obligatorio.");

        if (compra.Lineas.Count == 0)
            return ApiResponse<CompraDto>.Fail("Agregá al menos un producto a la compra.");

        if (compra.DescuentoTotal < 0)
            return ApiResponse<CompraDto>.Fail("El descuento no puede ser negativo.");

        try
        {
            var compraId = _repository.Crear(compra);
            var creada = _repository.ObtenerPorId(compraId);

            return creada is null
                ? ApiResponse<CompraDto>.Fail("La compra se registró pero no se pudo recuperar.")
                : ApiResponse<CompraDto>.Ok(creada, $"Compra #{compraId} registrada con éxito.");
        }
        catch (CompraInvalidaException ex)
        {
            return ApiResponse<CompraDto>.Fail(ex.Message);
        }
    }

    public ApiResponse<CompraDto> ObtenerPorId(int compraId)
    {
        if (compraId <= 0)
            return ApiResponse<CompraDto>.Fail("La compra es inválida.");

        var compra = _repository.ObtenerPorId(compraId);

        return compra is null
            ? ApiResponse<CompraDto>.Fail("No se encontró la compra.")
            : ApiResponse<CompraDto>.Ok(compra, "Compra obtenida correctamente.");
    }

    public ApiResponse<List<CompraResumenDto>> Listar(DateTime desde, DateTime hasta, int? proveedorId)
    {
        if (desde.Date > hasta.Date)
            return ApiResponse<List<CompraResumenDto>>.Fail("La fecha desde no puede ser mayor a la fecha hasta.");

        return ApiResponse<List<CompraResumenDto>>.Ok(
            _repository.Listar(desde, hasta, proveedorId), "Compras obtenidas correctamente.");
    }

    public ApiResponse<CompraDto> Recibir(int compraId, RecibirCompraDto datos)
    {
        if (compraId <= 0)
            return ApiResponse<CompraDto>.Fail("La compra es inválida.");

        if (datos.UsuarioId <= 0)
            return ApiResponse<CompraDto>.Fail("El usuario es obligatorio.");

        try
        {
            _repository.Recibir(compraId, datos);

            return ApiResponse<CompraDto>.Ok(
                _repository.ObtenerPorId(compraId), "Mercadería recibida e ingresada al stock.");
        }
        catch (CompraInvalidaException ex)
        {
            return ApiResponse<CompraDto>.Fail(ex.Message);
        }
    }

    public ApiResponse<string> Anular(int compraId, int usuarioId, string motivo)
    {
        if (compraId <= 0)
            return ApiResponse<string>.Fail("La compra es inválida.");

        if (usuarioId <= 0)
            return ApiResponse<string>.Fail("El usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(motivo))
            return ApiResponse<string>.Fail("El motivo de la anulación es obligatorio.");

        try
        {
            _repository.Anular(compraId, usuarioId, motivo.Trim());

            return ApiResponse<string>.Ok("OK", $"Compra #{compraId} anulada con éxito.");
        }
        catch (CompraInvalidaException ex)
        {
            return ApiResponse<string>.Fail(ex.Message);
        }
    }
}

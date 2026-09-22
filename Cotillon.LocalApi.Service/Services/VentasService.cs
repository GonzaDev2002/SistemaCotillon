using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs.Ventas;
using Cotillon.Shared.Responses;

namespace Cotillon.LocalApi.Service.Services;

public class VentasService
{
    private static readonly string[] TiposComprobante = ["TICKET", "REMITO", "PRESUPUESTO"];

    private readonly VentasRepository _repository;
    private readonly ConfiguracionRepository _configuracion;

    public VentasService(VentasRepository repository, ConfiguracionRepository configuracion)
    {
        _repository = repository;
        _configuracion = configuracion;
    }

    public ApiResponse<VentaDto> Crear(CrearVentaDto venta)
    {
        if (venta.UsuarioId <= 0)
            return ApiResponse<VentaDto>.Fail("El usuario es obligatorio.");

        if (venta.Lineas.Count == 0)
            return ApiResponse<VentaDto>.Fail("Agregá al menos un producto a la venta.");

        if (venta.DescuentoTotal < 0)
            return ApiResponse<VentaDto>.Fail("El descuento no puede ser negativo.");

        venta.TipoComprobante = string.IsNullOrWhiteSpace(venta.TipoComprobante)
            ? "TICKET"
            : venta.TipoComprobante.Trim().ToUpperInvariant();

        if (!TiposComprobante.Contains(venta.TipoComprobante))
            return ApiResponse<VentaDto>.Fail("El tipo de comprobante no es válido.");

        try
        {
            var permitirSinStock = _configuracion.ObtenerBooleano("VENTA_PERMITE_SIN_STOCK");
            var ventaId = _repository.Crear(venta, permitirSinStock);
            var creada = _repository.ObtenerPorId(ventaId);

            return creada is null
                ? ApiResponse<VentaDto>.Fail("La venta se registró pero no se pudo recuperar.")
                : ApiResponse<VentaDto>.Ok(creada, $"Venta #{ventaId} registrada con éxito.");
        }
        catch (VentaInvalidaException ex)
        {
            return ApiResponse<VentaDto>.Fail(ex.Message);
        }
    }

    public ApiResponse<VentaDto> ObtenerPorId(int ventaId)
    {
        if (ventaId <= 0)
            return ApiResponse<VentaDto>.Fail("La venta es inválida.");

        var venta = _repository.ObtenerPorId(ventaId);

        return venta is null
            ? ApiResponse<VentaDto>.Fail("No se encontró la venta.")
            : ApiResponse<VentaDto>.Ok(venta, "Venta obtenida correctamente.");
    }

    public ApiResponse<List<VentaResumenDto>> Listar(DateTime desde, DateTime hasta, bool incluirAnuladas)
    {
        if (desde.Date > hasta.Date)
            return ApiResponse<List<VentaResumenDto>>.Fail("La fecha desde no puede ser mayor a la fecha hasta.");

        return ApiResponse<List<VentaResumenDto>>.Ok(
            _repository.Listar(desde, hasta, incluirAnuladas), "Ventas obtenidas correctamente.");
    }

    public ApiResponse<string> Anular(int ventaId, AnularVentaDto datos)
    {
        if (ventaId <= 0)
            return ApiResponse<string>.Fail("La venta es inválida.");

        if (datos.UsuarioId <= 0)
            return ApiResponse<string>.Fail("El usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(datos.Motivo))
            return ApiResponse<string>.Fail("El motivo de la anulación es obligatorio.");

        datos.Motivo = datos.Motivo.Trim();

        try
        {
            _repository.Anular(ventaId, datos);

            return ApiResponse<string>.Ok("OK", $"Venta #{ventaId} anulada con éxito.");
        }
        catch (VentaInvalidaException ex)
        {
            return ApiResponse<string>.Fail(ex.Message);
        }
    }
}

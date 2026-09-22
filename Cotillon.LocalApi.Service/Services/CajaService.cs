using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs.Caja;
using Cotillon.Shared.Responses;

namespace Cotillon.LocalApi.Service.Services;

public class CajaService
{
    private static readonly string[] TiposMovimientoManual = ["INGRESO", "EGRESO", "RETIRO"];

    private readonly CajaRepository _repository;

    public CajaService(CajaRepository repository)
    {
        _repository = repository;
    }

    public ApiResponse<SesionCajaDto?> ObtenerSesionAbierta()
    {
        var sesion = _repository.ObtenerSesionAbierta();

        return sesion is null
            ? ApiResponse<SesionCajaDto?>.Ok(null, "No hay una caja abierta.")
            : ApiResponse<SesionCajaDto?>.Ok(sesion, "Caja abierta.");
    }

    public ApiResponse<SesionCajaDto> Abrir(AbrirCajaDto datos)
    {
        if (datos.UsuarioId <= 0)
            return ApiResponse<SesionCajaDto>.Fail("El usuario es obligatorio.");

        if (datos.MontoInicial < 0)
            return ApiResponse<SesionCajaDto>.Fail("El monto inicial no puede ser negativo.");

        if (_repository.ObtenerSesionAbierta() is not null)
            return ApiResponse<SesionCajaDto>.Fail("Ya hay una caja abierta. Cerrala antes de abrir otra.");

        var sesionId = _repository.Abrir(datos.UsuarioId, datos.MontoInicial, datos.Observaciones);
        var sesion = _repository.ObtenerPorId(sesionId);

        return sesion is null
            ? ApiResponse<SesionCajaDto>.Fail("No se pudo abrir la caja.")
            : ApiResponse<SesionCajaDto>.Ok(sesion, "Caja abierta con éxito.");
    }

    public ApiResponse<ArqueoCajaDto> Cerrar(CerrarCajaDto datos)
    {
        if (datos.UsuarioId <= 0)
            return ApiResponse<ArqueoCajaDto>.Fail("El usuario es obligatorio.");

        if (datos.MontoContado < 0)
            return ApiResponse<ArqueoCajaDto>.Fail("El monto contado no puede ser negativo.");

        var sesion = _repository.ObtenerSesionAbierta();

        if (sesion is null)
            return ApiResponse<ArqueoCajaDto>.Fail("No hay una caja abierta para cerrar.");

        var arqueo = _repository.ObtenerArqueo(sesion);

        _repository.Cerrar(
            sesion.SesionCajaId, datos.UsuarioId, datos.MontoContado, arqueo.EfectivoEsperado, datos.Observaciones);

        var cerrada = _repository.ObtenerPorId(sesion.SesionCajaId);

        if (cerrada is not null)
            arqueo.Sesion = cerrada;

        var diferencia = datos.MontoContado - arqueo.EfectivoEsperado;

        var mensaje = diferencia == 0
            ? "Caja cerrada sin diferencias."
            : $"Caja cerrada con una diferencia de {diferencia:C2}.";

        return ApiResponse<ArqueoCajaDto>.Ok(arqueo, mensaje);
    }

    public ApiResponse<ArqueoCajaDto> ObtenerArqueoActual()
    {
        var sesion = _repository.ObtenerSesionAbierta();

        return sesion is null
            ? ApiResponse<ArqueoCajaDto>.Fail("No hay una caja abierta.")
            : ApiResponse<ArqueoCajaDto>.Ok(_repository.ObtenerArqueo(sesion), "Arqueo calculado correctamente.");
    }

    public ApiResponse<ArqueoCajaDto> ObtenerArqueo(int sesionCajaId)
    {
        var sesion = _repository.ObtenerPorId(sesionCajaId);

        return sesion is null
            ? ApiResponse<ArqueoCajaDto>.Fail("No se encontró la sesión de caja.")
            : ApiResponse<ArqueoCajaDto>.Ok(_repository.ObtenerArqueo(sesion), "Arqueo calculado correctamente.");
    }

    public ApiResponse<string> RegistrarMovimiento(RegistrarMovimientoCajaDto movimiento)
    {
        if (movimiento.UsuarioId <= 0)
            return ApiResponse<string>.Fail("El usuario es obligatorio.");

        var tipo = movimiento.TipoMovimiento?.Trim().ToUpperInvariant() ?? string.Empty;

        if (!TiposMovimientoManual.Contains(tipo))
            return ApiResponse<string>.Fail("El tipo de movimiento debe ser INGRESO, EGRESO o RETIRO.");

        if (movimiento.Importe <= 0)
            return ApiResponse<string>.Fail("El importe debe ser mayor a cero.");

        if (string.IsNullOrWhiteSpace(movimiento.Motivo))
            return ApiResponse<string>.Fail("El motivo es obligatorio.");

        var sesion = _repository.ObtenerSesionAbierta();

        if (sesion is null)
            return ApiResponse<string>.Fail("No hay una caja abierta para registrar el movimiento.");

        if (tipo is "EGRESO" or "RETIRO")
        {
            var arqueo = _repository.ObtenerArqueo(sesion);

            if (movimiento.Importe > arqueo.EfectivoEsperado)
                return ApiResponse<string>.Fail(
                    $"No hay efectivo suficiente en caja. Disponible: {arqueo.EfectivoEsperado:C2}.");
        }

        movimiento.TipoMovimiento = tipo;
        movimiento.Motivo = movimiento.Motivo.Trim();

        _repository.RegistrarMovimiento(sesion.SesionCajaId, movimiento);

        return ApiResponse<string>.Ok("OK", "Movimiento registrado con éxito.");
    }

    public ApiResponse<List<MovimientoCajaDto>> ObtenerMovimientos(int sesionCajaId)
    {
        if (sesionCajaId <= 0)
            return ApiResponse<List<MovimientoCajaDto>>.Fail("La sesión de caja es inválida.");

        return ApiResponse<List<MovimientoCajaDto>>.Ok(
            _repository.ObtenerMovimientos(sesionCajaId), "Movimientos obtenidos correctamente.");
    }

    public ApiResponse<List<SesionCajaDto>> ObtenerHistorial(DateTime desde, DateTime hasta)
    {
        if (desde.Date > hasta.Date)
            return ApiResponse<List<SesionCajaDto>>.Fail("La fecha desde no puede ser mayor a la fecha hasta.");

        return ApiResponse<List<SesionCajaDto>>.Ok(
            _repository.ObtenerHistorial(desde, hasta), "Historial obtenido correctamente.");
    }
}

using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs.Caja;

namespace Cotillon.LocalApi.Service.Services;

public class CajaService
{
    private readonly CajaRepository _repository;

    public CajaService(CajaRepository repository)
    {
        _repository = repository;
    }

    public Task<SesionCajaDto?> ObtenerCajaAbiertaAsync(int usuarioId)
    {
        return _repository.ObtenerCajaAbiertaAsync(usuarioId);
    }

    public async Task<int> AbrirCajaAsync(AbrirCajaRequestDto request)
    {
        var cajaAbierta = await _repository.ObtenerCajaAbiertaAsync(request.UsuarioId);

        if (cajaAbierta != null)
            return cajaAbierta.SesionCajaId;

        return await _repository.AbrirCajaAsync(request);
    }
    public Task CerrarCajaAsync(CerrarCajaRequestDto request)
    {
        return _repository.CerrarCajaAsync(request);
    }
}
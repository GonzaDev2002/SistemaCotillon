using Cotillon.Data;
using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs.Respaldos;

namespace Cotillon.LocalApi.Service.Services;

/// <summary>
/// Corre los respaldos solo, mientras el servicio está prendido.
///
/// Vive en el servicio y no en la aplicación de escritorio a propósito: el
/// servicio es lo que va a quedar corriendo como servicio de Windows, prendido
/// aunque nadie tenga sesión abierta. Si el respaldo dependiera de que alguien
/// abra el programa, el día que la vendedora se va temprano no hay copia.
///
/// Revisa cada hora en vez de programar una hora fija. Una PC de local se
/// apaga, se reinicia y se queda sin luz: si el respaldo estuviera agendado a
/// las 23:00 y ese día la apagaron a las 21:00, no se hace y nadie se entera.
/// Preguntando "¿hace más de N días que no hay copia?" se recupera solo en el
/// próximo arranque.
/// </summary>
public class RespaldoAutomatico : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromHours(1);

    /// <summary>
    /// Espera antes del primer intento: deja que el servicio termine de levantar
    /// y no le pone un VACUUM encima justo cuando se está abriendo la caja.
    /// </summary>
    private static readonly TimeSpan EsperaInicial = TimeSpan.FromMinutes(2);

    private readonly IServiceProvider _servicios;
    private readonly ILogger<RespaldoAutomatico> _log;

    public RespaldoAutomatico(IServiceProvider servicios, ILogger<RespaldoAutomatico> log)
    {
        _servicios = servicios;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken cancelacion)
    {
        try
        {
            await Task.Delay(EsperaInicial, cancelacion);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        while (!cancelacion.IsCancellationRequested)
        {
            Revisar();

            try
            {
                await Task.Delay(Intervalo, cancelacion);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private void Revisar()
    {
        try
        {
            // Alcance propio: este servicio es singleton y los repositorios son
            // scoped.
            using var alcance = _servicios.CreateScope();

            var factory = alcance.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
            var configuracionRepo = alcance.ServiceProvider.GetRequiredService<ConfiguracionRepository>();

            var configuracion = ConfiguracionRespaldoDto.Desde(configuracionRepo.ObtenerTodo());
            var respaldos = new Respaldos(factory);

            if (!respaldos.Corresponde(configuracion))
                return;

            var resultado = respaldos.Crear(configuracion);

            if (resultado.Exito)
                _log.LogInformation("Respaldo automático: {Mensaje}", resultado.Mensaje);
            else
                _log.LogError("Respaldo automático falló: {Mensaje}", resultado.Mensaje);
        }
        catch (Exception ex)
        {
            // Nada de lo que pase acá puede voltear el servicio: del otro lado hay
            // una caja cobrando.
            _log.LogError(ex, "Error al revisar los respaldos.");
        }
    }
}

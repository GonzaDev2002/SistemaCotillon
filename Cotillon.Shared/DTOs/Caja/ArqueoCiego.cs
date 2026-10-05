namespace Cotillon.Shared.DTOs.Caja;

/// <summary>
/// Arqueo ciego: el que cuenta la plata no ve cuánto tendría que haber.
///
/// Es el único control de caja que realmente funciona. Si el cajero ve el
/// esperado antes de contar, un faltante se tapa escribiendo ese número en vez
/// de lo que hay en el cajón, y la diferencia que el sistema registra es
/// siempre cero. Sin verlo, tiene que declarar lo que contó, y la diferencia
/// aparece sola.
///
/// Viene apagado: en un negocio donde el dueño es el único que toca la caja,
/// ocultarle el esperado es molestia sin beneficio.
/// </summary>
public static class ArqueoCiego
{
    public const string Clave = "CAJA_ARQUEO_CIEGO";

    public static bool Activo(IReadOnlyDictionary<string, string>? configuracion) =>
        configuracion is not null
        && configuracion.TryGetValue(Clave, out var valor)
        && bool.TryParse(valor, out var activo)
        && activo;
}

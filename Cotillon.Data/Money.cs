namespace Cotillon.Data;

/// <summary>
/// Conversión entre los importes decimales del dominio y los enteros en centavos
/// que se guardan en la base.
/// </summary>
public static class Money
{
    public static long ToCentavos(decimal importe) =>
        (long)decimal.Round(importe * 100m, 0, MidpointRounding.AwayFromZero);

    public static decimal FromCentavos(long centavos) => centavos / 100m;
}

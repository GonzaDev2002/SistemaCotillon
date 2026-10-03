using System.Globalization;

namespace Cotillon.Shared.DTOs;

/// <summary>
/// Cómo se vende un producto. El negocio tiene de los dos tipos: la bolsa de
/// globos se vende por unidad, las golosinas por peso y la cinta por metro.
/// </summary>
/// <param name="Codigo">Lo que se guarda en Productos.UnidadMedida.</param>
/// <param name="Nombre">Para el combo de la pantalla de productos.</param>
/// <param name="Simbolo">Abreviatura para las grillas y el ticket.</param>
/// <param name="PrecioPor">Cómo se titula el precio: "PRECIO POR KILO", etc.</param>
/// <param name="Fraccionada">
/// Si admite decimales. Define si al vender se pregunta la cantidad en vez de
/// agregar uno, y cuántos decimales se muestran.
/// </param>
/// <param name="SubUnidad">
/// Nombre de la fracción con la que se mide en la práctica (gramos, centímetros).
/// Es en lo que se tipea la cantidad, porque es lo que muestra la balanza.
/// </param>
/// <param name="PorUnidad">Cuántas sub-unidades entran en una: 1000 g por kilo.</param>
public sealed record UnidadMedida(
    string Codigo,
    string Nombre,
    string Simbolo,
    string PrecioPor,
    bool Fraccionada,
    string? SubUnidad = null,
    int PorUnidad = 1)
{
    /// <summary>Cantidades de uso frecuente, en sub-unidades, para los botones rápidos.</summary>
    public int[] Atajos { get; init; } = [];

    /// <summary>Formatea una cantidad con su símbolo: "0,350 kg", "3 u.".</summary>
    public string Formatear(decimal cantidad) =>
        Fraccionada
            ? $"{cantidad.ToString("0.###", CultureInfo.CurrentCulture)} {Simbolo}"
            : $"{cantidad.ToString("0.##", CultureInfo.CurrentCulture)} {Simbolo}";
}

public static class UnidadesMedida
{
    public const string Unidad = "UNIDAD";
    public const string Kilo = "KILO";
    public const string Metro = "METRO";

    public static readonly IReadOnlyList<UnidadMedida> Todas =
    [
        new(Unidad, "Por unidad", "u.", "PRECIO POR UNIDAD", Fraccionada: false),

        new(Kilo, "Por peso (kilo)", "kg", "PRECIO POR KILO", Fraccionada: true,
            SubUnidad: "gramos", PorUnidad: 1000)
        {
            // Lo que más se pide en el mostrador de golosinas.
            Atajos = [100, 250, 500, 1000]
        },

        new(Metro, "Por metro", "m", "PRECIO POR METRO", Fraccionada: true,
            SubUnidad: "centímetros", PorUnidad: 100)
        {
            Atajos = [50, 100, 200, 500]
        }
    ];

    /// <summary>
    /// Devuelve la unidad por su código. Lo desconocido cae en unidad: una base
    /// migrada puede traer textos viejos y una venta no se puede frenar por eso.
    /// </summary>
    public static UnidadMedida De(string? codigo) =>
        Todas.FirstOrDefault(u => string.Equals(u.Codigo, codigo, StringComparison.OrdinalIgnoreCase))
        ?? Todas[0];

    public static bool EsFraccionada(string? codigo) => De(codigo).Fraccionada;
}

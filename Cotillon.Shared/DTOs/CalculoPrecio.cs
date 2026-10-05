namespace Cotillon.Shared.DTOs;

/// <summary>
/// Las cuentas que hoy se hacen con calculadora y papel al lado de la caja.
///
/// Son cuatro, y cada una es la misma cuenta despejada por una variable
/// distinta. Están acá y no en la pantalla porque son plata: un margen mal
/// despejado se arrastra a todos los precios de la lista.
///
/// Lo importante: el margen se calcula SOBRE EL COSTO, que es como lo piensa
/// quien compra ("lo compré a 100 y lo vendo a 150, le gano el 50 %"). No es lo
/// mismo que el margen sobre la venta, que para ese caso daría 33 %. Mezclar
/// las dos definiciones es el error clásico de las planillas de precios.
/// </summary>
public static class CalculoPrecio
{
    /// <summary>Costo + margen % = precio.</summary>
    public static decimal PrecioDesdeMargen(decimal costo, decimal margenPorcentaje) =>
        costo * (1m + margenPorcentaje / 100m);

    /// <summary>Qué porcentaje sobre el costo deja este precio. Sin costo no hay margen que calcular.</summary>
    public static decimal MargenDesdePrecio(decimal costo, decimal precio) =>
        costo > 0 ? (precio - costo) / costo * 100m : 0m;

    /// <summary>
    /// Qué costo admite un precio dado para dejar ese margen. Sirve para el
    /// camino inverso: "lo quiero vender a $1.500 ganando 60 %, ¿a cuánto me lo
    /// tiene que dejar el proveedor?".
    /// </summary>
    public static decimal CostoDesdeMargen(decimal precio, decimal margenPorcentaje)
    {
        var factor = 1m + margenPorcentaje / 100m;

        return factor != 0 ? precio / factor : 0m;
    }

    /// <summary>
    /// Cuánto sale la unidad de un bulto cerrado. Es la cuenta más frecuente del
    /// mostrador: la bolsa de 50 globos salió $12.000.
    /// </summary>
    public static decimal CostoUnitario(decimal costoBulto, decimal unidadesPorBulto) =>
        unidadesPorBulto > 0 ? costoBulto / unidadesPorBulto : 0m;

    /// <summary>Cuánto subió algo entre dos precios, en porcentaje.</summary>
    public static decimal Variacion(decimal anterior, decimal nuevo) =>
        anterior > 0 ? (nuevo - anterior) / anterior * 100m : 0m;

    /// <summary>
    /// Redondea a múltiplos de <paramref name="paso"/> pesos. Un precio de
    /// mostrador termina en 0: cobrar $1.487 obliga a tener monedas que no hay.
    /// Nunca devuelve cero si había un importe: quedaría un producto regalado.
    /// </summary>
    public static decimal Redondear(decimal importe, int paso)
    {
        if (paso <= 0 || importe <= 0)
            return importe;

        var redondeado = Math.Round(importe / paso, MidpointRounding.AwayFromZero) * paso;

        return redondeado < paso ? paso : redondeado;
    }

    /// <summary>Agrega IVA a un importe neto.</summary>
    public static decimal ConIva(decimal neto, decimal ivaPorcentaje) =>
        neto * (1m + ivaPorcentaje / 100m);

    /// <summary>Saca el IVA de un importe que ya lo incluye.</summary>
    public static decimal SinIva(decimal conIva, decimal ivaPorcentaje)
    {
        var factor = 1m + ivaPorcentaje / 100m;

        return factor != 0 ? conIva / factor : conIva;
    }
}

using System.Globalization;

namespace Cotillon.Shared.DTOs;

/// <summary>
/// Una fila de la lista pegada. Si <see cref="Error"/> tiene texto, la fila no se
/// importa y ese texto es lo que se le muestra al usuario para que la corrija.
/// </summary>
public sealed record FilaImportada(
    int Linea,
    string Nombre,
    decimal Precio,
    decimal Costo,
    decimal Stock,
    string Codigo,
    string? Error = null)
{
    public bool EsValida => Error is null;
}

/// <summary>
/// Lee una lista de productos pegada desde Excel para darlos de alta de una vez.
/// Cargar doscientos productos de a uno por el formulario es media tarde; pegados
/// es un rato.
///
/// Las columnas van en este orden, y sólo el nombre y el precio son obligatorios:
///
///     Nombre | Precio | Costo | Stock | Código de barras
///
/// El separador es el TABULADOR (lo que pone Excel al copiar) o el punto y coma.
/// La coma queda afuera a propósito: acá es el separador decimal, y partir por
/// coma convertiría $1.500,50 en dos columnas.
/// </summary>
public static class ListaDeProductos
{
    public static List<FilaImportada> Leer(string? texto)
    {
        var filas = new List<FilaImportada>();

        if (string.IsNullOrWhiteSpace(texto))
            return filas;

        var lineas = texto.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

        for (var i = 0; i < lineas.Length; i++)
        {
            var linea = lineas[i];

            if (string.IsNullOrWhiteSpace(linea))
                continue;

            var numero = i + 1;
            var columnas = linea.Split(linea.Contains('\t') ? '\t' : ';');
            var nombre = Columna(columnas, 0);

            // Encabezado de la planilla: se saltea en vez de intentar importarlo.
            if (numero == 1 && nombre.Equals("nombre", StringComparison.OrdinalIgnoreCase))
                continue;

            if (nombre.Length == 0)
            {
                filas.Add(Invalida(numero, columnas, "Falta el nombre del producto."));
                continue;
            }

            if (!Importe(Columna(columnas, 1), out var precio))
            {
                filas.Add(Invalida(numero, columnas,
                    $"No se entiende el precio «{Columna(columnas, 1)}»."));
                continue;
            }

            if (!Importe(Columna(columnas, 2), out var costo))
            {
                filas.Add(Invalida(numero, columnas,
                    $"No se entiende el costo «{Columna(columnas, 2)}»."));
                continue;
            }

            if (!Importe(Columna(columnas, 3), out var stock))
            {
                filas.Add(Invalida(numero, columnas,
                    $"No se entiende el stock «{Columna(columnas, 3)}»."));
                continue;
            }

            if (precio < 0 || costo < 0 || stock < 0)
            {
                filas.Add(Invalida(numero, columnas, "Los números no pueden ser negativos."));
                continue;
            }

            filas.Add(new FilaImportada(
                numero, nombre, precio, costo, stock, Columna(columnas, 4)));
        }

        MarcarCodigosRepetidos(filas);

        return filas;
    }

    /// <summary>
    /// Avisa de los códigos repetidos dentro de la misma lista. Sin esto, la
    /// segunda fila con el mismo código la rechaza la base con un mensaje que no
    /// dice en qué renglón está el problema.
    /// </summary>
    private static void MarcarCodigosRepetidos(List<FilaImportada> filas)
    {
        var vistos = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < filas.Count; i++)
        {
            var fila = filas[i];

            if (!fila.EsValida || fila.Codigo.Length == 0)
                continue;

            if (vistos.TryGetValue(fila.Codigo, out var primera))
                filas[i] = fila with { Error = $"El código {fila.Codigo} ya está en el renglón {primera}." };
            else
                vistos[fila.Codigo] = fila.Linea;
        }
    }

    private static FilaImportada Invalida(int linea, string[] columnas, string error) =>
        new(linea, Columna(columnas, 0), 0, 0, 0, Columna(columnas, 4), error);

    private static string Columna(string[] columnas, int indice) =>
        indice < columnas.Length ? columnas[indice].Trim() : string.Empty;

    /// <summary>
    /// Lee un importe como lo escribe un argentino: "1.500,50", "1500,50", "$ 1.500".
    /// Sin coma, los puntos son separadores de miles, no decimales: en una lista de
    /// precios de acá "1.500" son mil quinientos pesos y no uno con cinco.
    /// Vacío es cero, que es lo que corresponde a una columna que no se cargó.
    /// </summary>
    public static bool Importe(string? texto, out decimal valor)
    {
        valor = 0m;

        // La columna en blanco vale cero. Una columna CON texto que no deja
        // ningún dígito, en cambio, es un error: "a convenir" no puede entrar
        // como precio cero y salir vendido gratis.
        if (string.IsNullOrWhiteSpace(texto))
            return true;

        var limpio = new string(texto
            .Where(c => char.IsAsciiDigit(c) || c is '.' or ',' or '-')
            .ToArray());

        if (limpio.Length == 0)
            return false;

        limpio = limpio.Contains(',')
            ? limpio.Replace(".", string.Empty).Replace(',', '.')
            : limpio.Replace(".", string.Empty);

        return decimal.TryParse(limpio, NumberStyles.Any, CultureInfo.InvariantCulture, out valor);
    }
}

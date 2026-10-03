using System.Text;

namespace Cotillon.Shared.DTOs;

/// <summary>Lo que venía escrito dentro del código de barras de la balanza.</summary>
/// <param name="Codigo">Código del artículo tal como se configuró en la balanza.</param>
/// <param name="Peso">Peso en kilos, si la etiqueta lo trae.</param>
/// <param name="Importe">Importe en pesos, si la etiqueta trae el precio en vez del peso.</param>
public sealed record LecturaBalanza(string Codigo, decimal? Peso, decimal? Importe);

/// <summary>
/// Lee las etiquetas que imprime una balanza con impresora. Esas etiquetas no
/// llevan el código del producto a secas: llevan el código del artículo y, metido
/// en los mismos trece dígitos, el peso de esa pesada o su importe. Por eso al
/// escanearlas no alcanza con buscar el código y ya.
///
/// El formato cambia según la marca y según cómo la configuraron, así que se
/// describe con una máscara en Configuración en vez de quedar fijo acá:
///
///   dígito  tiene que coincidir exactamente (es el prefijo que usa la balanza)
///   C       dígito del código del artículo
///   P       dígito del peso, en GRAMOS
///   I       dígito del importe, en CENTAVOS
///   otro    se ignora (el dígito verificador, relleno)
///
/// La máscara típica es 20CCCCCPPPPPD: prefijo 20, cinco dígitos de código, cinco
/// de peso en gramos y el verificador. Viene vacía de fábrica, o sea apagada,
/// porque un producto común cuyo código empiece en 20 se leería como una pesada.
/// </summary>
public static class EtiquetaBalanza
{
    public const string Clave = "BALANZA_MASCARA";

    /// <summary>Lo que se le sugiere al usuario en Configuración, no lo que se aplica solo.</summary>
    public const string MascaraSugerida = "20CCCCCPPPPPD";

    /// <summary>
    /// Devuelve lo que traía la etiqueta, o null si esto no es una etiqueta de
    /// balanza y hay que buscarlo como un código normal.
    /// </summary>
    public static LecturaBalanza? Leer(string? codigoLeido, string? mascara)
    {
        if (string.IsNullOrWhiteSpace(mascara))
            return null;

        var molde = mascara.Trim().ToUpperInvariant();
        var texto = codigoLeido?.Trim() ?? string.Empty;

        if (texto.Length != molde.Length || !texto.All(char.IsAsciiDigit))
            return null;

        var codigo = new StringBuilder();
        var peso = new StringBuilder();
        var importe = new StringBuilder();

        for (var i = 0; i < molde.Length; i++)
        {
            switch (molde[i])
            {
                case 'C': codigo.Append(texto[i]); break;
                case 'P': peso.Append(texto[i]); break;
                case 'I': importe.Append(texto[i]); break;

                default:
                    // El prefijo es lo que distingue una pesada de un código común:
                    // si no coincide, esto no es una etiqueta de la balanza.
                    if (char.IsAsciiDigit(molde[i]) && molde[i] != texto[i])
                        return null;

                    break;
            }
        }

        // Una máscara sin código, o sin peso ni importe, no describe una etiqueta
        // de balanza: se ignora en vez de leer cualquier cosa.
        if (codigo.Length == 0 || (peso.Length == 0 && importe.Length == 0))
            return null;

        return new LecturaBalanza(
            codigo.ToString(),
            peso.Length > 0 ? Entero(peso) / 1000m : null,
            importe.Length > 0 ? Entero(importe) / 100m : null);
    }

    private static decimal Entero(StringBuilder digitos) =>
        decimal.Parse(digitos.ToString());
}

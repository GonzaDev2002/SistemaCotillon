using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Cotillon.Shared.DTOs;

namespace Cotillon.Desktop.Wpf.Servicios;

/// <summary>
/// Manda a imprimir los tickets que arma <see cref="Ticket"/>.
///
/// Una impresora térmica se instala en Windows como una impresora común, así que
/// no hace falta ninguna librería: alcanza con mandarle el texto en una
/// tipografía de ancho fijo y del tamaño justo para que entren los caracteres
/// del papel. Ese tamaño se mide contra la fuente de verdad en vez de estimarlo,
/// porque cada impresora reporta un ancho distinto.
/// </summary>
public static class Impresion
{
    public const string ClaveAncho = "TICKET_ANCHO";
    public const string ClaveAutomatico = "TICKET_AUTOMATICO";

    /// <summary>
    /// Tope del tamaño de letra. En una térmica el tamaño calculado siempre queda
    /// por debajo; en una hoja A4 sin este tope el ticket saldría con letras
    /// gigantes estiradas de lado a lado, así que se imprime como una tira angosta.
    /// </summary>
    private const double TamanoMaximo = 12.0;

    private static readonly FontFamily Fuente = new("Consolas");

    public static int AnchoConfigurado(IReadOnlyDictionary<string, string>? configuracion) =>
        configuracion is not null
        && configuracion.TryGetValue(ClaveAncho, out var valor)
        && int.TryParse(valor, out var ancho)
        && ancho is >= 24 and <= 80
            ? ancho
            : Ticket.Ancho80mm;

    public static bool AutomaticoAlCobrar(IReadOnlyDictionary<string, string>? configuracion) =>
        configuracion is not null
        && configuracion.TryGetValue(ClaveAutomatico, out var valor)
        && bool.TryParse(valor, out var automatico)
        && automatico;

    /// <summary>
    /// Imprime el texto. Con <paramref name="preguntar"/> en false sale directo
    /// por la impresora predeterminada, que es lo que hace falta al cobrar: un
    /// cuadro de diálogo por venta frena la caja igual que el cartel modal.
    /// Devuelve false si el usuario canceló o si no hay impresora.
    /// </summary>
    public static bool Imprimir(string contenido, int ancho, string titulo, bool preguntar)
    {
        var dialogo = new PrintDialog();

        if (preguntar)
        {
            if (dialogo.ShowDialog() != true)
                return false;
        }
        else if (dialogo.PrintQueue is null)
        {
            return false;
        }

        var documento = Documento(contenido, ancho, dialogo.PrintableAreaWidth);

        dialogo.PrintDocument(((IDocumentPaginatorSource)documento).DocumentPaginator, titulo);

        return true;
    }

    private static FlowDocument Documento(string contenido, int ancho, double anchoPapel)
    {
        const double margen = 4;
        var util = Math.Max(anchoPapel - margen * 2, 1);

        var documento = new FlowDocument(new Paragraph(new Run(contenido)))
        {
            FontFamily = Fuente,
            FontSize = Math.Min(TamanoMaximo, TamanoQueEntra(ancho, util)),
            PagePadding = new Thickness(margen),
            PageWidth = anchoPapel,
            ColumnWidth = double.PositiveInfinity,
            TextAlignment = TextAlignment.Left
        };

        return documento;
    }

    /// <summary>
    /// Mide cuánto ocupan <paramref name="caracteres"/> caracteres a un tamaño de
    /// referencia y escala. Medir en vez de suponer un ancho de carácter evita que
    /// el ticket se corte en una impresora y quede angosto en otra.
    /// </summary>
    private static double TamanoQueEntra(int caracteres, double anchoDisponible)
    {
        const double referencia = 10.0;

        var regla = new FormattedText(
            new string('0', caracteres),
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(Fuente, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
            referencia,
            Brushes.Black,
            pixelsPerDip: 1.0);

        return regla.Width > 0
            ? referencia * anchoDisponible / regla.Width
            : referencia;
    }
}

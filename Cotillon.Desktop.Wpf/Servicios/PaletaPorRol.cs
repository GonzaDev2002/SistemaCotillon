using System.Windows;
using System.Windows.Media;

namespace Cotillon.Desktop.Wpf.Servicios;

/// <summary>
/// Le da un color propio a la pantalla según quién entró.
///
/// No es decoración: en un local con dos puestos, lo que evita que alguien
/// cargue un precio creyendo que está en la caja, o que el dueño deje su
/// sesión abierta sin darse cuenta, es ver de un vistazo con qué usuario está
/// trabajando. El color se nota desde el otro lado del mostrador; el nombre
/// chiquito abajo a la izquierda, no.
///
/// Se aplica ANTES de crear la ventana principal: los StaticResource se
/// resuelven al cargar el XAML, así que pisarlos después no cambiaría nada.
/// </summary>
public static class PaletaPorRol
{
    private sealed record Paleta(
        string Lateral,
        string LateralHover,
        string PieLateral,
        string Primario,
        string PrimarioOscuro,
        string TituloMenu);

    /// <summary>Índigo, el de siempre. El dueño ve el sistema completo.</summary>
    private static readonly Paleta Administracion =
        new("#1E1B4B", "#312E81", "#171449", "#4F46E5", "#4338CA", "#818CF8");

    /// <summary>Verde: la pantalla de quien está atendiendo.</summary>
    private static readonly Paleta Mostrador =
        new("#064E3B", "#065F46", "#053A2C", "#059669", "#047857", "#6EE7B7");

    /// <summary>Ámbar, en el medio: supervisa pero no administra.</summary>
    private static readonly Paleta Supervision =
        new("#451A03", "#78350F", "#331302", "#D97706", "#B45309", "#FCD34D");

    public static void Aplicar(string? rol)
    {
        var paleta = rol?.Trim().ToUpperInvariant() switch
        {
            "ADMIN" => Administracion,
            "SUPERVISOR" => Supervision,
            // Cualquier otro rol entra como mostrador: si mañana se agrega uno
            // nuevo, que arranque con la pantalla del que atiende y no con la del
            // dueño, que es el error más caro de los dos.
            _ => Mostrador
        };

        Pisar("Lateral", paleta.Lateral);
        Pisar("LateralHover", paleta.LateralHover);
        Pisar("PieLateral", paleta.PieLateral);
        Pisar("Primario", paleta.Primario);
        Pisar("PrimarioOscuro", paleta.PrimarioOscuro);
        Pisar("TituloMenu", paleta.TituloMenu);
    }

    private static void Pisar(string clave, string color)
    {
        var brocha = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        brocha.Freeze();

        Application.Current.Resources[clave] = brocha;
    }
}

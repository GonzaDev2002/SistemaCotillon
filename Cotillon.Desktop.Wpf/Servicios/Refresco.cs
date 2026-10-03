using System.Windows;
using System.Windows.Threading;

namespace Cotillon.Desktop.Wpf.Servicios;

/// <summary>
/// Vuelve a pedir los datos cada tanto mientras la pantalla está abierta.
///
/// Los datos ya son de todos: hay un solo servicio escribiendo en la base, así
/// que una venta de la cajera queda registrada en el momento y cualquier
/// pantalla que se abra después la ve. Lo que faltaba era que las pantallas que
/// quedan abiertas se enteren solas, sin que el dueño apriete Actualizar.
///
/// Sólo se usa en las pantallas de mirar. En las de trabajar un refresco
/// automático es un estorbo: mueve la lista mientras se busca algo y pisa lo
/// que se está editando.
/// </summary>
public static class Refresco
{
    /// <summary>
    /// Quince segundos: suficiente para que el dueño vea la venta de la cajera
    /// casi al instante, y nada al lado de una consulta a una base local.
    /// </summary>
    public static readonly TimeSpan Intervalo = TimeSpan.FromSeconds(15);

    public static void Automatico(FrameworkElement pantalla, Func<Task> recargar)
    {
        var reloj = new DispatcherTimer { Interval = Intervalo };
        var enCurso = false;

        reloj.Tick += async (_, _) =>
        {
            // Si la consulta anterior todavía no volvió, no se encima otra: con la
            // red lenta de un local se irían apilando.
            if (enCurso)
                return;

            enCurso = true;

            try
            {
                await recargar();
            }
            catch
            {
                // Un refresco que falla no puede voltear la pantalla. El servicio
                // puede estar reiniciándose o la red del local haberse cortado un
                // segundo; en el tick siguiente se vuelve a intentar.
            }
            finally
            {
                enCurso = false;
            }
        };

        pantalla.Loaded += (_, _) => reloj.Start();
        pantalla.Unloaded += (_, _) => reloj.Stop();
    }
}

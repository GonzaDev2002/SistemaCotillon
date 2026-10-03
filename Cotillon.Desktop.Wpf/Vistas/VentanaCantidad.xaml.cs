using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Cotillon.Shared.DTOs;
using MahApps.Metro.IconPacks;

namespace Cotillon.Desktop.Wpf.Vistas;

/// <summary>
/// Pide cuánto se lleva de un producto que no se vende por unidad.
///
/// Se tipea en la sub-unidad (gramos, centímetros) porque es lo que muestra la
/// balanza: el vendedor lee 350 en el visor y escribe 350, sin convertir a 0,350
/// de cabeza mientras el cliente espera. La equivalencia y el importe se ven
/// mientras tanto, así el error se nota antes de agregar la línea.
/// </summary>
public partial class VentanaCantidad : Window
{
    private readonly UnidadMedida _unidad;
    private readonly decimal _precio;

    public VentanaCantidad(
        string producto, string unidadCodigo, decimal precio, decimal? cantidadActual = null,
        decimal? stockDisponible = null)
    {
        InitializeComponent();

        _unidad = UnidadesMedida.De(unidadCodigo);
        _precio = precio;

        LblProducto.Text = producto;
        LblPrecio.Text = $"{precio.ToString("C2", CultureInfo.CurrentCulture)} por {_unidad.Simbolo}";
        LblCampo.Text = (_unidad.SubUnidad ?? "cantidad").ToUpperInvariant();

        Icono.Kind = _unidad.Codigo switch
        {
            UnidadesMedida.Kilo => PackIconMaterialKind.ScaleBalance,
            UnidadesMedida.Metro => PackIconMaterialKind.TapeMeasure,
            _ => PackIconMaterialKind.Counter
        };

        if (stockDisponible.HasValue)
            LblStock.Text = $"En stock: {_unidad.Formatear(stockDisponible.Value)}";

        ArmarAtajos();

        // Arranca con la cantidad actual (al corregir una línea) o en blanco para
        // escribir directo lo que marca la balanza.
        TxtCantidad.Text = cantidadActual.HasValue
            ? (cantidadActual.Value * _unidad.PorUnidad).ToString("0.##", CultureInfo.CurrentCulture)
            : string.Empty;

        Loaded += (_, _) =>
        {
            TxtCantidad.Focus();
            TxtCantidad.SelectAll();
        };
    }

    /// <summary>Cantidad elegida, ya convertida a la unidad de venta (kg, m, u.).</summary>
    public decimal Cantidad { get; private set; }

    /// <summary>
    /// Un botón por atajo, con su número de orden: F1 a F4 los disparan sin mouse.
    /// </summary>
    private void ArmarAtajos()
    {
        if (_unidad.Atajos.Length == 0)
        {
            PanelAtajos.Visibility = Visibility.Collapsed;
            return;
        }

        for (var i = 0; i < _unidad.Atajos.Length; i++)
        {
            var valor = _unidad.Atajos[i];

            var boton = new Button
            {
                Style = (Style)FindResource("BotonSecundario"),
                Content = $"{Etiqueta(valor)}\n(F{i + 1})",
                Margin = new Thickness(i == 0 ? 0 : 4, 0, i == _unidad.Atajos.Length - 1 ? 0 : 4, 0),
                Padding = new Thickness(0, 7, 0, 7),
                FontSize = 12,
                Tag = valor
            };

            boton.Click += (s, _) => Aplicar((int)((Button)s!).Tag);

            PanelAtajos.Children.Add(boton);
        }
    }

    /// <summary>Muestra 1 kg en vez de 1000 g cuando el atajo es una unidad entera.</summary>
    private string Etiqueta(int subUnidades) =>
        subUnidades % _unidad.PorUnidad == 0
            ? $"{subUnidades / _unidad.PorUnidad} {_unidad.Simbolo}"
            : $"{subUnidades} {_unidad.SubUnidad?[..1] ?? string.Empty}";

    private void Aplicar(int subUnidades)
    {
        TxtCantidad.Text = subUnidades.ToString(CultureInfo.CurrentCulture);
        TxtCantidad.Focus();
        TxtCantidad.SelectAll();
    }

    private void AlCambiarCantidad(object sender, TextChangedEventArgs e)
    {
        // TextChanged se dispara durante InitializeComponent, antes de que existan
        // las etiquetas de abajo.
        if (LblImporte is null)
            return;

        var cantidad = CantidadEnUnidad();

        LblEquivalencia.Text = _unidad.Formatear(cantidad);
        LblImporte.Text = (cantidad * _precio).ToString("C2", CultureInfo.CurrentCulture);

        BtnAceptar.IsEnabled = cantidad > 0;
    }

    private decimal CantidadEnUnidad()
    {
        var escrito = decimal.TryParse(
            TxtCantidad.Text.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out var valor)
            ? valor
            : 0m;

        return escrito / _unidad.PorUnidad;
    }

    private void AlTeclaCantidad(object sender, KeyEventArgs e)
    {
        // F1 a F4 cargan los atajos sin soltar el teclado.
        var atajo = e.Key - Key.F1;

        if (atajo >= 0 && atajo < _unidad.Atajos.Length)
        {
            Aplicar(_unidad.Atajos[atajo]);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Enter)
        {
            AlAceptar(sender, e);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            AlCancelar(sender, e);
            e.Handled = true;
        }
    }

    private void AlAceptar(object sender, RoutedEventArgs e)
    {
        var cantidad = CantidadEnUnidad();

        if (cantidad <= 0)
        {
            TxtCantidad.Focus();
            return;
        }

        Cantidad = cantidad;
        DialogResult = true;
        Close();
    }

    private void AlCancelar(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

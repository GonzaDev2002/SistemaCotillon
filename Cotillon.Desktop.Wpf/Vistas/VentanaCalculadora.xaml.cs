using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Cotillon.Shared.DTOs;

namespace Cotillon.Desktop.Wpf.Vistas;

/// <summary>
/// La calculadora de precios, para que las cuentas del mostrador dejen de
/// hacerse con calculadora y papel.
///
/// La idea es que no haya un "modo": se escribe en el campo que se conoce y los
/// demás se despejan. Si tipeás el margen, sale el precio; si tipeás el precio,
/// sale el margen. El que está editando nunca se sobreescribe, porque pisarle
/// el número a alguien mientras lo escribe es lo que hace que estas pantallas se
/// abandonen y se vuelva al papel.
/// </summary>
public partial class VentanaCalculadora : Window
{
    /// <summary>Evita que recalcular un campo dispare el recálculo del otro en cadena.</summary>
    private bool _recalculando;

    public VentanaCalculadora(decimal costo = 0, decimal precioActual = 0, decimal unidadesPorBulto = 1)
    {
        InitializeComponent();

        TxtCostoBulto.Text = Texto(costo);
        TxtUnidades.Text = Texto(unidadesPorBulto <= 0 ? 1 : unidadesPorBulto);
        TxtPrecioAnterior.Text = Texto(precioActual);

        foreach (var margen in new[] { 30, 50, 80, 100, 150 })
        {
            var boton = new Button
            {
                Content = $"{margen} %",
                Style = (Style)FindResource("BotonSecundario"),
                FontSize = 12,
                Padding = new Thickness(6, 7, 6, 7),
                Margin = new Thickness(2),
                Tag = margen
            };

            boton.Click += (remitente, _) =>
            {
                TxtMargen.Text = ((int)((Button)remitente).Tag).ToString(CultureInfo.CurrentCulture);
                DesdeMargen();
            };

            PanelMargenes.Children.Add(boton);
        }

        TxtCostoBulto.TextChanged += (_, _) => DesdeMargen();
        TxtUnidades.TextChanged += (_, _) => DesdeMargen();
        TxtMargen.TextChanged += (_, _) => DesdeMargen();
        TxtPrecio.TextChanged += (_, _) => DesdePrecio();
        TxtPrecioAnterior.TextChanged += (_, _) => Mostrar();
        CboRedondeo.SelectionChanged += (_, _) => DesdeMargen();

        PreviewKeyDown += AlTecla;

        Loaded += (_, _) =>
        {
            DesdeMargen();
            TxtCostoBulto.Focus();
            TxtCostoBulto.SelectAll();
        };
    }

    /// <summary>Lo que la pantalla de productos toma al confirmar.</summary>
    public decimal CostoUnitario { get; private set; }

    public decimal Precio { get; private set; }

    // ------------------------------------------------------- Las dos cuentas --

    /// <summary>Se tipeó el costo o el margen: el precio sale de ahí.</summary>
    private void DesdeMargen()
    {
        if (_recalculando)
            return;

        _recalculando = true;

        try
        {
            CostoUnitario = CalculoPrecio.CostoUnitario(
                Numero(TxtCostoBulto.Text), Numero(TxtUnidades.Text));

            Precio = CalculoPrecio.Redondear(
                CalculoPrecio.PrecioDesdeMargen(CostoUnitario, Numero(TxtMargen.Text)),
                Redondeo());

            // No se le pisa el campo al que lo está escribiendo.
            if (!TxtPrecio.IsKeyboardFocused)
                TxtPrecio.Text = Texto(Precio);

            Mostrar();
        }
        finally
        {
            _recalculando = false;
        }
    }

    /// <summary>Se tipeó el precio: el margen se despeja de ahí.</summary>
    private void DesdePrecio()
    {
        if (_recalculando)
            return;

        _recalculando = true;

        try
        {
            CostoUnitario = CalculoPrecio.CostoUnitario(
                Numero(TxtCostoBulto.Text), Numero(TxtUnidades.Text));

            Precio = Numero(TxtPrecio.Text);

            if (!TxtMargen.IsKeyboardFocused)
                TxtMargen.Text = Texto(Math.Round(
                    CalculoPrecio.MargenDesdePrecio(CostoUnitario, Precio), 1));

            Mostrar();
        }
        finally
        {
            _recalculando = false;
        }
    }

    private void Mostrar()
    {
        LblCostoUnitario.Text = Moneda(CostoUnitario);
        LblPrecioFinal.Text = Moneda(Precio);

        var ganancia = Precio - CostoUnitario;
        var margen = CalculoPrecio.MargenDesdePrecio(CostoUnitario, Precio);

        LblGanancia.Text = CostoUnitario > 0
            ? $"Ganás {Moneda(ganancia)} por unidad ({margen:0.#} % sobre el costo)."
            : "Cargá el costo para ver cuánto ganás.";

        var anterior = Numero(TxtPrecioAnterior.Text);

        LblComparacion.Text = anterior > 0
            ? $"Antes estaba {Moneda(anterior)}: " +
              $"{(Precio >= anterior ? "sube" : "baja")} {Math.Abs(CalculoPrecio.Variacion(anterior, Precio)):0.#} %."
            : string.Empty;

        // Vender por debajo del costo es el error que esta pantalla tiene que
        // frenar: es la cuenta que hoy se hace en papel y sale mal.
        var enPerdida = CostoUnitario > 0 && Precio < CostoUnitario;

        LblAlerta.Text = enPerdida
            ? $"OJO: a este precio perdés {Moneda(CostoUnitario - Precio)} por unidad."
            : string.Empty;

        LblAlerta.Visibility = enPerdida ? Visibility.Visible : Visibility.Collapsed;
        BtnUsar.IsEnabled = Precio > 0;
    }

    private int Redondeo() =>
        int.TryParse((CboRedondeo.SelectedItem as ComboBoxItem)?.Tag?.ToString(), out var paso) ? paso : 0;

    private void AlTecla(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter when BtnUsar.IsEnabled:
                DialogResult = true;
                break;

            case Key.Escape:
                DialogResult = false;
                break;

            default:
                return;
        }

        e.Handled = true;
    }

    private void AlUsar(object sender, RoutedEventArgs e) => DialogResult = true;

    private void AlCerrar(object sender, RoutedEventArgs e) => DialogResult = false;

    private static string Moneda(decimal importe) =>
        importe.ToString("C2", CultureInfo.CurrentCulture);

    private static string Texto(decimal valor) =>
        valor.ToString("0.##", CultureInfo.CurrentCulture);

    private static decimal Numero(string? texto) =>
        decimal.TryParse(texto?.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out var valor)
            ? valor
            : 0m;
}

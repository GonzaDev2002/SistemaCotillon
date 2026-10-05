using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cotillon.Desktop.Wpf.Vistas;

/// <summary>
/// Pide el motivo de algo que hace desaparecer plata: una venta que se cancela
/// antes de cobrar, por ejemplo.
///
/// El motivo obligatorio no frena a nadie que quiera robar, y no hay que
/// venderlo como que lo frena. Lo que hace es convertir un Escape silencioso en
/// una afirmación escrita, con nombre y hora, que después se puede leer junta.
/// Eso sí cambia el cálculo de quien lo piensa hacer seguido.
/// </summary>
public partial class VentanaMotivo : Window
{
    private const int MinimoCaracteres = 4;

    public VentanaMotivo(string titulo, string detalle, params string[] sugerencias)
    {
        InitializeComponent();

        LblTitulo.Text = titulo;
        LblDetalle.Text = detalle;

        foreach (var sugerencia in sugerencias)
        {
            var boton = new Button
            {
                Content = sugerencia,
                Style = (Style)FindResource("BotonSecundario"),
                FontSize = 12,
                Padding = new Thickness(8, 7, 8, 7),
                Margin = new Thickness(2),
                Tag = sugerencia
            };

            boton.Click += (remitente, _) =>
            {
                TxtMotivo.Text = (string)((Button)remitente).Tag;
                Revisar();
            };

            PanelSugerencias.Children.Add(boton);
        }

        TxtMotivo.TextChanged += (_, _) => Revisar();

        Loaded += (_, _) =>
        {
            Revisar();
            TxtMotivo.Focus();
        };
    }

    public string Motivo => TxtMotivo.Text.Trim();

    private void Revisar()
    {
        var largo = Motivo.Length;

        BtnAceptar.IsEnabled = largo >= MinimoCaracteres;

        LblAviso.Text = largo switch
        {
            0 => "Elegí uno de los motivos o escribí el tuyo.",
            < MinimoCaracteres => "Escribí un motivo que se entienda al leerlo mañana.",
            _ => "Queda registrado con tu nombre y la hora."
        };
    }

    private void AlTecla(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && BtnAceptar.IsEnabled)
        {
            DialogResult = true;
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            DialogResult = false;
            e.Handled = true;
        }
    }

    private void AlAceptar(object sender, RoutedEventArgs e) => DialogResult = true;

    private void AlVolver(object sender, RoutedEventArgs e) => DialogResult = false;
}

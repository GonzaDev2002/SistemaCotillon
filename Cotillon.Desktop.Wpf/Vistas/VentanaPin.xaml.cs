using System.Windows;

namespace Cotillon.Desktop.Wpf.Vistas;

public partial class VentanaPin : Window
{
    public VentanaPin()
    {
        InitializeComponent();

        Owner = Application.Current.MainWindow;
        Loaded += (_, _) => TxtPin.Focus();
    }

    public string Pin { get; private set; } = string.Empty;

    private void AlGuardar(object sender, RoutedEventArgs e)
    {
        var pin = TxtPin.Password.Trim();

        if (pin.Length < 4)
        {
            LblError.Text = "El PIN debe tener al menos 4 dígitos.";
            LblError.Visibility = Visibility.Visible;
            return;
        }

        Pin = pin;
        DialogResult = true;
        Close();
    }

    private void AlCancelar(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

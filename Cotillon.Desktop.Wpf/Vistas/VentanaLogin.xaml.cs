using System.Windows;
using System.Windows.Input;
using Cotillon.Desktop.Wpf.Servicios;

namespace Cotillon.Desktop.Wpf.Vistas;

public partial class VentanaLogin : Window
{
    public VentanaLogin()
    {
        InitializeComponent();

        LblServidor.Text = $"Servidor: {App.Configuracion.UrlApi}";
        Loaded += (_, _) => TxtUsuario.Focus();
    }

    private async void AlIngresar(object sender, RoutedEventArgs e) => await IngresarAsync();

    private async void AlPresionarTecla(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await IngresarAsync();
    }

    private void AlSalir(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private async Task IngresarAsync()
    {
        var usuario = TxtUsuario.Text.Trim();
        var pin = TxtPin.Password.Trim();

        if (string.IsNullOrWhiteSpace(usuario))
        {
            MostrarError("Ingresá el usuario.");
            TxtUsuario.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(pin))
        {
            MostrarError("Ingresá el PIN.");
            TxtPin.Focus();
            return;
        }

        BtnIngresar.IsEnabled = false;
        BtnIngresar.Content = "Ingresando...";
        LblError.Visibility = Visibility.Collapsed;

        try
        {
            var respuesta = await App.Api.LoginAsync(usuario, pin);

            if (!respuesta.Success || respuesta.Data is null)
            {
                MostrarError(respuesta.Message);
                TxtPin.Clear();
                TxtPin.Focus();
                return;
            }

            Sesion.Iniciar(respuesta.Data);

            // Mejor frenarlo acá con un motivo que dejarlo entrar a una ventana
            // sin un solo módulo, donde no tendría forma de entender qué pasó.
            if (!Sesion.TieneAlgunModulo)
            {
                Sesion.Cerrar();
                MostrarError("Tu usuario no tiene ningún módulo habilitado. Pedile acceso al administrador.");
                TxtPin.Clear();
                TxtPin.Focus();
                return;
            }

            DialogResult = true;
            Close();
        }
        finally
        {
            BtnIngresar.IsEnabled = true;
            BtnIngresar.Content = "Ingresar";
        }
    }

    private void MostrarError(string mensaje)
    {
        LblError.Text = mensaje;
        LblError.Visibility = Visibility.Visible;
    }
}

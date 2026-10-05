using System.Windows;
using System.Windows.Controls;
using Cotillon.Desktop.Wpf.Servicios;
using Cotillon.Shared.DTOs;
using Cotillon.Shared.DTOs.Caja;
using Cotillon.Shared.DTOs.Respaldos;
using Cotillon.Shared.DTOs.Permisos;

namespace Cotillon.Desktop.Wpf.Vistas.Paginas;

public partial class PaginaConfiguracion : UserControl
{
    private readonly bool _puedeEditar = Sesion.PuedeEditar(Modulos.Configuracion);

    public PaginaConfiguracion()
    {
        InitializeComponent();

        BtnGuardar.Visibility = _puedeEditar ? Visibility.Visible : Visibility.Collapsed;

        Loaded += async (_, _) => await CargarAsync();
    }

    private async Task CargarAsync()
    {
        TxtUrlApi.Text = App.Configuracion.UrlApi;
        ChkEsServidor.IsChecked = App.Configuracion.EsServidor;

        var respuesta = await App.Api.ConfiguracionAsync();

        if (respuesta.Success && respuesta.Data is not null)
        {
            TxtNombre.Text = Valor(respuesta.Data, "NEGOCIO_NOMBRE");
            TxtCuit.Text = Valor(respuesta.Data, "NEGOCIO_CUIT");
            TxtDireccion.Text = Valor(respuesta.Data, "NEGOCIO_DIRECCION");
            TxtTelefono.Text = Valor(respuesta.Data, "NEGOCIO_TELEFONO");
            TxtPieTicket.Text = Valor(respuesta.Data, "TICKET_PIE");
            ChkVentaSinStock.IsChecked =
                bool.TryParse(Valor(respuesta.Data, "VENTA_PERMITE_SIN_STOCK"), out var permitir) && permitir;
            TxtMascaraBalanza.Text = Valor(respuesta.Data, EtiquetaBalanza.Clave);
            CboAnchoTicket.SelectedValue = Impresion.AnchoConfigurado(respuesta.Data).ToString();
            ChkTicketAutomatico.IsChecked = Impresion.AutomaticoAlCobrar(respuesta.Data);
            ChkArqueoCiego.IsChecked = ArqueoCiego.Activo(respuesta.Data);

            var respaldo = ConfiguracionRespaldoDto.Desde(respuesta.Data);

            ChkRespaldoActivo.IsChecked = respaldo.Activo;
            CboRespaldoDias.SelectedValue = respaldo.CadaDias.ToString();
            CboRespaldoCopias.SelectedValue = respaldo.Copias.ToString();
            TxtCarpetaNube.Text = respaldo.CarpetaNube;
        }

        await CargarRespaldosAsync();
        await VerificarAsync();
    }

    /// <summary>
    /// Trae el estado de los respaldos: cuántas copias hay, de cuándo es la
    /// última y si subieron a la nube. El aviso en rojo aparece cuando hace más
    /// días que la frecuencia elegida, porque un respaldo configurado que dejó de
    /// correr es peor que no tener ninguno: da tranquilidad falsa.
    /// </summary>
    private async Task CargarRespaldosAsync()
    {
        var respuesta = await App.Api.EstadoRespaldosAsync();

        if (!respuesta.Success || respuesta.Data is null)
        {
            LblRespaldos.Text = "No se pudo consultar el estado de los respaldos.";
            return;
        }

        var estado = respuesta.Data;

        LblAvisoRespaldo.Text = estado.Aviso;
        PanelAvisoRespaldo.Visibility = estado.HayQueAvisar ? Visibility.Visible : Visibility.Collapsed;

        var enLaNube = estado.Copias.Count(c => c.EnLaNube);

        LblRespaldos.Text = estado.Copias.Count == 0
            ? $"Todavía no hay copias. Se guardan en {estado.Carpeta}"
            : $"{estado.Copias.Count} copia(s), la última del " +
              $"{estado.Copias[0].Fecha:dd/MM/yyyy HH:mm} ({estado.Copias[0].TamanoLegible})" +
              (enLaNube > 0 ? $" · {enLaNube} en la nube" : " · ninguna en la nube") +
              $"\n{estado.Carpeta}";
    }

    private async void AlRespaldarAhora(object sender, RoutedEventArgs e)
    {
        BtnRespaldarAhora.IsEnabled = false;
        BtnRespaldarAhora.Content = "Respaldando...";

        try
        {
            var respuesta = await App.Api.RespaldarAhoraAsync();
            var resultado = respuesta.Data;

            MessageBox.Show(
                resultado?.Mensaje ?? respuesta.Message, "Cotillón", MessageBoxButton.OK,
                resultado?.Exito == true ? MessageBoxImage.Information : MessageBoxImage.Warning);

            await CargarRespaldosAsync();
        }
        finally
        {
            BtnRespaldarAhora.IsEnabled = true;
            BtnRespaldarAhora.Content = "Respaldar ahora";
        }
    }

    private void AlElegirCarpetaNube(object sender, RoutedEventArgs e)
    {
        var dialogo = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Elegí la carpeta que sincroniza con la nube"
        };

        if (dialogo.ShowDialog() == true)
            TxtCarpetaNube.Text = dialogo.FolderName;
    }

    private static string Valor(Dictionary<string, string> datos, string clave) =>
        datos.TryGetValue(clave, out var valor) ? valor : string.Empty;

    private async void AlGuardar(object sender, RoutedEventArgs e)
    {
        if (!_puedeEditar)
            return;

        BtnGuardar.IsEnabled = false;

        try
        {
            var claves = new Dictionary<string, string?>
            {
                ["NEGOCIO_NOMBRE"] = TxtNombre.Text.Trim(),
                ["NEGOCIO_CUIT"] = TxtCuit.Text.Trim(),
                ["NEGOCIO_DIRECCION"] = TxtDireccion.Text.Trim(),
                ["NEGOCIO_TELEFONO"] = TxtTelefono.Text.Trim(),
                ["TICKET_PIE"] = TxtPieTicket.Text.Trim(),
                ["VENTA_PERMITE_SIN_STOCK"] = (ChkVentaSinStock.IsChecked == true).ToString().ToLowerInvariant(),
                [EtiquetaBalanza.Clave] = TxtMascaraBalanza.Text.Trim().ToUpperInvariant(),
                [Impresion.ClaveAncho] = CboAnchoTicket.SelectedValue as string ?? "42",
                [Impresion.ClaveAutomatico] = (ChkTicketAutomatico.IsChecked == true).ToString().ToLowerInvariant(),
                [ArqueoCiego.Clave] = (ChkArqueoCiego.IsChecked == true).ToString().ToLowerInvariant(),
                [ConfiguracionRespaldoDto.ClaveActivo] = (ChkRespaldoActivo.IsChecked == true).ToString().ToLowerInvariant(),
                [ConfiguracionRespaldoDto.ClaveCadaDias] = CboRespaldoDias.SelectedValue as string ?? "1",
                [ConfiguracionRespaldoDto.ClaveCopias] = CboRespaldoCopias.SelectedValue as string ?? "30",
                [ConfiguracionRespaldoDto.ClaveCarpetaNube] = TxtCarpetaNube.Text.Trim()
            };

            foreach (var (clave, valor) in claves)
            {
                var respuesta = await App.Api.GuardarConfiguracionAsync(clave, valor);

                if (!respuesta.Success)
                {
                    MessageBox.Show(respuesta.Message, "Cotillón", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            var cambioConexion =
                App.Configuracion.UrlApi != TxtUrlApi.Text.Trim() ||
                App.Configuracion.EsServidor != (ChkEsServidor.IsChecked == true);

            if (cambioConexion)
            {
                App.Configuracion.UrlApi = TxtUrlApi.Text.Trim();
                App.Configuracion.EsServidor = ChkEsServidor.IsChecked == true;
                App.Configuracion.Guardar();

                LblReinicio.Visibility = Visibility.Visible;
            }

            MessageBox.Show("Configuración guardada con éxito.", "Cotillón",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        finally
        {
            BtnGuardar.IsEnabled = true;
        }
    }

    private async void AlVerificar(object sender, RoutedEventArgs e) => await VerificarAsync();

    private async Task VerificarAsync()
    {
        var respuesta = await App.Api.Cliente.ObtenerAsync<string>("api/Health/db");

        LblEstadoBase.Text = respuesta.Success
            ? $"✔ {respuesta.Message}"
            : $"✖ {respuesta.Message}";
    }
}

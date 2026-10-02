using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Cotillon.Desktop.Wpf.Servicios;
using Cotillon.Shared.DTOs.Adjuntos;
using MahApps.Metro.IconPacks;
using Microsoft.Win32;

namespace Cotillon.Desktop.Wpf.Controles;

/// <summary>
/// Lista de archivos adjuntos de un registro, con alta, apertura, copia y baja.
/// Es el mismo control en clientes, proveedores, ventas y compras: cada pantalla
/// sólo declara a qué tipo de registro pertenece y le pasa el id seleccionado.
/// </summary>
public partial class PanelAdjuntos : UserControl
{
    private int _entidadId;

    public PanelAdjuntos()
    {
        InitializeComponent();
    }

    /// <summary>CLIENTE, PROVEEDOR, VENTA, COMPRA o PRODUCTO.</summary>
    public string EntidadTipo { get; set; } = "CLIENTE";

    /// <summary>Texto del encabezado, por ejemplo "Facturas del cliente".</summary>
    public string Titulo
    {
        get => LblTitulo.Text;
        set => LblTitulo.Text = value;
    }

    /// <summary>Qué mostrar cuando todavía no se eligió ningún registro.</summary>
    public string AvisoSinSeleccion { get; set; } =
        "Elegí un registro de la lista para ver o agregar sus archivos.";

    private bool _puedeAdjuntar = true;
    private bool _puedeQuitar = true;

    /// <summary>
    /// Ata los botones al permiso del módulo donde está el panel: adjuntar necesita
    /// Editar y quitar necesita Total, porque borrar una factura no tiene vuelta.
    /// </summary>
    public void AplicarPermisos(string modulo)
    {
        _puedeAdjuntar = Sesion.PuedeEditar(modulo);
        _puedeQuitar = Sesion.PuedeTodo(modulo);

        AllowDrop = _puedeAdjuntar;
    }

    /// <summary>
    /// Muestra los archivos del registro indicado. Con id 0 queda en blanco, que es
    /// el estado mientras no hay nada seleccionado o se está dando un alta nueva.
    /// </summary>
    public async Task CargarAsync(int entidadId)
    {
        _entidadId = entidadId;
        BtnAgregar.IsEnabled = entidadId > 0 && _puedeAdjuntar;

        if (entidadId <= 0)
        {
            Lista.ItemsSource = null;
            ChipCantidad.Visibility = Visibility.Collapsed;
            LblAviso.Text = AvisoSinSeleccion;
            return;
        }

        var respuesta = await App.Api.AdjuntosAsync(EntidadTipo, entidadId);

        if (!respuesta.Success)
        {
            Lista.ItemsSource = null;
            LblAviso.Text = respuesta.Message;
            return;
        }

        var archivos = respuesta.Data ?? [];

        Lista.ItemsSource = archivos.Select(a => new Fila(a, _puedeQuitar)).ToList();

        ChipCantidad.Visibility = archivos.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        LblCantidad.Text = archivos.Count == 1 ? "1 archivo" : $"{archivos.Count} archivos";

        LblAviso.Text = archivos.Count == 0
            ? "Sin archivos todavía. Usá Adjuntar o arrastrá acá la factura en PDF o la foto del comprobante."
            : "Doble clic para abrir. También podés arrastrar archivos a esta lista.";
    }

    public Task LimpiarAsync() => CargarAsync(0);

    // ------------------------------------------------------------------- Alta --

    private async void AlAgregar(object sender, RoutedEventArgs e)
    {
        var dialogo = new OpenFileDialog
        {
            Title = "Elegí la factura, el comprobante o la imagen",
            Filter = Adjunto.FiltroDialogo,
            Multiselect = true
        };

        if (dialogo.ShowDialog() != true)
            return;

        await SubirAsync(dialogo.FileNames);
    }

    private void AlArrastrarSobre(object sender, DragEventArgs e)
    {
        // Sin un registro elegido no hay a quién adjuntar: el cursor lo avisa.
        e.Effects = _entidadId > 0 && e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;

        e.Handled = true;
    }

    private async void AlSoltarArchivos(object sender, DragEventArgs e)
    {
        if (_entidadId <= 0 || e.Data.GetData(DataFormats.FileDrop) is not string[] rutas)
            return;

        e.Handled = true;

        await SubirAsync(rutas);
    }

    private async Task SubirAsync(IReadOnlyCollection<string> rutas)
    {
        if (_entidadId <= 0 || rutas.Count == 0)
            return;

        BtnAgregar.IsEnabled = false;

        try
        {
            var fallas = new List<string>();

            foreach (var ruta in rutas.Where(File.Exists))
            {
                LblAviso.Text = $"Subiendo {Path.GetFileName(ruta)}...";

                var respuesta = await App.Api.SubirAdjuntoAsync(EntidadTipo, _entidadId, ruta, null);

                if (!respuesta.Success)
                    fallas.Add($"{Path.GetFileName(ruta)}: {respuesta.Message}");
            }

            await CargarAsync(_entidadId);

            if (fallas.Count > 0)
                MessageBox.Show(string.Join("\n\n", fallas), "No se pudieron adjuntar todos los archivos",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            BtnAgregar.IsEnabled = _entidadId > 0;
        }
    }

    // ---------------------------------------------------------------- Lectura --

    private async void AlAbrirSeleccionado(object sender, MouseButtonEventArgs e)
    {
        if (Lista.SelectedItem is Fila fila)
            await AbrirAsync(fila);
    }

    private async void AlAbrir(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: Fila fila })
            await AbrirAsync(fila);
    }

    private async Task AbrirAsync(Fila fila)
    {
        // Se baja a una carpeta temporal propia y se abre con el programa que el
        // sistema tenga asociado: no hace falta visor de PDF propio.
        var carpeta = Path.Combine(Path.GetTempPath(), "Cotillon");
        Directory.CreateDirectory(carpeta);

        var destino = Path.Combine(carpeta, $"{fila.Dato.AdjuntoId}-{fila.Dato.NombreArchivo}");

        LblAviso.Text = $"Abriendo {fila.Dato.NombreArchivo}...";

        var respuesta = await App.Api.DescargarAdjuntoAsync(fila.Dato.AdjuntoId, destino);

        if (!respuesta.Success)
        {
            LblAviso.Text = respuesta.Message;
            return;
        }

        LblAviso.Text = "Doble clic para abrir. También podés arrastrar archivos a esta lista.";

        Process.Start(new ProcessStartInfo(destino) { UseShellExecute = true });
    }

    private async void AlGuardarComo(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: Fila fila })
            return;

        var dialogo = new SaveFileDialog
        {
            Title = "Guardar una copia del archivo",
            FileName = fila.Dato.NombreArchivo,
            Filter = $"{fila.Dato.Extension}|*{Path.GetExtension(fila.Dato.NombreArchivo)}|Todos|*.*"
        };

        if (dialogo.ShowDialog() != true)
            return;

        var respuesta = await App.Api.DescargarAdjuntoAsync(fila.Dato.AdjuntoId, dialogo.FileName);

        LblAviso.Text = respuesta.Success
            ? $"Copia guardada en {dialogo.FileName}"
            : respuesta.Message;
    }

    // ------------------------------------------------------------------- Baja --

    private async void AlQuitar(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: Fila fila })
            return;

        var confirmar = MessageBox.Show(
            $"¿Quitar «{fila.Dato.NombreArchivo}»?\n\nEl archivo se borra del servidor y no se puede recuperar.",
            "Cotillón", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirmar != MessageBoxResult.Yes)
            return;

        var respuesta = await App.Api.EliminarAdjuntoAsync(fila.Dato.AdjuntoId);

        if (!respuesta.Success)
        {
            MessageBox.Show(respuesta.Message, "Cotillón", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        await CargarAsync(_entidadId);
    }

    /// <summary>
    /// Fila lista para mostrar: el icono y el color ya resueltos según el formato,
    /// así la plantilla del XAML se limita a enlazar propiedades.
    /// </summary>
    private sealed class Fila
    {
        public Fila(AdjuntoDto dato, bool puedeQuitar)
        {
            Dato = dato;
            VisibilidadQuitar = puedeQuitar ? Visibility.Visible : Visibility.Collapsed;

            var esPdf = dato.Extension == "PDF";

            Icono = esPdf ? PackIconMaterialKind.FilePdfBox : PackIconMaterialKind.FileImageOutline;
            Ficha = (Brush)Application.Current.Resources[esPdf ? "FichaRosa" : "FichaCeleste"];

            var partes = new List<string> { dato.Extension, dato.TamanoLegible, $"{dato.FechaAlta:dd/MM/yyyy HH:mm}" };

            if (!string.IsNullOrWhiteSpace(dato.Usuario))
                partes.Add(dato.Usuario);

            if (!string.IsNullOrWhiteSpace(dato.Descripcion))
                partes.Add(dato.Descripcion);

            Detalle = string.Join("  ·  ", partes);
        }

        public AdjuntoDto Dato { get; }
        public PackIconMaterialKind Icono { get; }
        public Brush Ficha { get; }
        public string Detalle { get; }
        public Visibility VisibilidadQuitar { get; }
    }
}

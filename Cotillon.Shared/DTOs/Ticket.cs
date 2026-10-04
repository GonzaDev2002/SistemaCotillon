using System.Globalization;
using System.Text;
using Cotillon.Shared.DTOs.Caja;
using Cotillon.Shared.DTOs.Ventas;

namespace Cotillon.Shared.DTOs;

/// <summary>Datos del negocio que van en el encabezado y al pie del ticket.</summary>
public sealed record DatosTicket(
    string Nombre,
    string Direccion,
    string Telefono,
    string Cuit,
    string Pie)
{
    public const string ClaveNombre = "NEGOCIO_NOMBRE";
    public const string ClaveDireccion = "NEGOCIO_DIRECCION";
    public const string ClaveTelefono = "NEGOCIO_TELEFONO";
    public const string ClaveCuit = "NEGOCIO_CUIT";
    public const string ClavePie = "TICKET_PIE";

    public static DatosTicket Desde(IReadOnlyDictionary<string, string>? configuracion)
    {
        var datos = configuracion ?? new Dictionary<string, string>();

        string Valor(string clave) =>
            datos.TryGetValue(clave, out var valor) ? valor.Trim() : string.Empty;

        var nombre = Valor(ClaveNombre);

        return new DatosTicket(
            nombre.Length > 0 ? nombre : "Cotillón",
            Valor(ClaveDireccion),
            Valor(ClaveTelefono),
            Valor(ClaveCuit),
            Valor(ClavePie));
    }
}

/// <summary>
/// Arma el ticket como texto plano, que es exactamente lo que imprime una
/// térmica: ancho fijo, una tipografía de ancho fijo y nada más. El mismo texto
/// sirve para los 58 mm, para los 80 mm y para una hoja A4; lo único que cambia
/// es en cuántos caracteres entra el renglón.
///
/// Está acá y no en el cliente para poder probarlo: un ticket descuadrado o con
/// un total que no cierra se ve en una prueba, no en el mostrador.
/// </summary>
public static class Ticket
{
    /// <summary>Caracteres por renglón de cada papel.</summary>
    public const int Ancho58mm = 32;
    public const int Ancho80mm = 42;

    public static string Venta(VentaDto venta, DatosTicket negocio, int ancho = Ancho80mm)
    {
        var hoja = new Hoja(ancho);

        Encabezado(hoja, negocio);

        hoja.Centrado(venta.Estado == "ANULADA" ? "*** VENTA ANULADA ***" : "TICKET NO VÁLIDO COMO FACTURA");
        hoja.Separador();

        hoja.Izquierda($"Venta  #{venta.VentaId}");
        hoja.Izquierda($"Fecha   {venta.FechaVenta:dd/MM/yyyy HH:mm}");
        hoja.Izquierda($"Atiende {venta.Usuario}");

        if (venta.ClienteId is > 0)
            hoja.Izquierda($"Cliente {venta.Cliente}");

        hoja.Separador();

        foreach (var linea in venta.Lineas)
        {
            var unidad = UnidadesMedida.De(linea.UnidadMedida);

            // El nombre va en su propio renglón porque no entra al lado de los
            // números, y en una térmica cortarlo es perder de qué producto se trata.
            hoja.Izquierda(linea.NombreProducto);

            hoja.Partido(
                $"  {unidad.Formatear(linea.Cantidad)} x {Pesos(linea.PrecioUnitario)}",
                Pesos(linea.SubtotalLinea + linea.DescuentoLinea));

            if (linea.DescuentoLinea > 0)
                hoja.Partido("  Descuento", $"-{Pesos(linea.DescuentoLinea)}");
        }

        hoja.Separador();

        if (venta.DescuentoTotal > 0)
        {
            hoja.Partido("Subtotal", Pesos(venta.Subtotal));
            hoja.Partido("Descuento", $"-{Pesos(venta.DescuentoTotal)}");
        }

        hoja.Partido("TOTAL", Pesos(venta.TotalVenta));
        hoja.Vacio();

        foreach (var pago in venta.Pagos)
            hoja.Partido(pago.MedioPago, Pesos(pago.Importe));

        if (venta.Vuelto > 0)
            hoja.Partido("Vuelto", Pesos(venta.Vuelto));

        Pie(hoja, negocio);

        return hoja.ToString();
    }

    /// <summary>
    /// Comprobante del cierre de caja. Es el papel que se guarda con la plata, así
    /// que lleva el esperado, el contado y la diferencia con su signo a la vista.
    /// </summary>
    public static string CierreDeCaja(ArqueoCajaDto arqueo, DatosTicket negocio, int ancho = Ancho80mm)
    {
        var hoja = new Hoja(ancho);
        var sesion = arqueo.Sesion;

        Encabezado(hoja, negocio);

        hoja.Centrado("CIERRE DE CAJA");
        hoja.Separador();

        hoja.Izquierda($"Caja    #{sesion.SesionCajaId}");
        hoja.Izquierda($"Abrió   {sesion.FechaApertura:dd/MM/yyyy HH:mm} · {sesion.UsuarioApertura}");

        if (sesion.FechaCierre is { } cierre)
            hoja.Izquierda($"Cerró   {cierre:dd/MM/yyyy HH:mm} · {sesion.UsuarioCierre}");

        hoja.Separador();

        hoja.Partido("Monto inicial", Pesos(sesion.MontoInicial));
        hoja.Partido($"Ventas ({arqueo.CantidadVentas})", Pesos(arqueo.TotalVentas));
        hoja.Partido("Ingresos", Pesos(arqueo.TotalIngresos));
        hoja.Partido("Egresos", $"-{Pesos(arqueo.TotalEgresos)}");

        hoja.Separador();
        hoja.Centrado("POR MEDIO DE PAGO");

        foreach (var medio in arqueo.TotalesPorMedioPago)
            hoja.Partido($"{medio.MedioPago} ({medio.Cantidad})", Pesos(medio.Total));

        hoja.Separador();

        hoja.Partido("EFECTIVO ESPERADO", Pesos(arqueo.EfectivoEsperado));

        if (sesion.MontoContado is { } contado)
        {
            hoja.Partido("Contado", Pesos(contado));

            var diferencia = sesion.Diferencia ?? contado - arqueo.EfectivoEsperado;

            // El signo se escribe siempre: un "0,00" y un "-0,00" se confunden, y
            // acá la diferencia es el número por el que alguien responde.
            hoja.Partido(
                diferencia == 0 ? "Diferencia" : diferencia > 0 ? "SOBRA" : "FALTA",
                Pesos(Math.Abs(diferencia)));
        }

        if (!string.IsNullOrWhiteSpace(sesion.Observaciones))
        {
            hoja.Separador();
            hoja.Izquierda(sesion.Observaciones!);
        }

        hoja.Vacio();
        hoja.Centrado("Firma: ____________________");
        hoja.Vacio();

        return hoja.ToString();
    }

    private static void Encabezado(Hoja hoja, DatosTicket negocio)
    {
        hoja.Centrado(negocio.Nombre.ToUpperInvariant());

        if (negocio.Direccion.Length > 0)
            hoja.Centrado(negocio.Direccion);

        if (negocio.Telefono.Length > 0)
            hoja.Centrado($"Tel. {negocio.Telefono}");

        if (negocio.Cuit.Length > 0)
            hoja.Centrado($"CUIT {negocio.Cuit}");

        hoja.Separador();
    }

    private static void Pie(Hoja hoja, DatosTicket negocio)
    {
        hoja.Separador();

        if (negocio.Pie.Length > 0)
            hoja.Centrado(negocio.Pie);

        hoja.Vacio();
    }

    private static string Pesos(decimal importe) =>
        importe.ToString("N2", CultureInfo.GetCultureInfo("es-AR"));

    /// <summary>Renglones de ancho fijo. Nada entra más largo que el papel.</summary>
    private sealed class Hoja(int ancho)
    {
        private readonly StringBuilder _texto = new();

        public void Vacio() => _texto.AppendLine();

        public void Separador() => _texto.AppendLine(new string('-', ancho));

        public void Izquierda(string texto)
        {
            // Lo que no entra baja al renglón siguiente en vez de cortarse: en un
            // ticket el nombre del producto completo importa más que la prolijidad.
            foreach (var parte in Partir(texto, ancho))
                _texto.AppendLine(parte);
        }

        public void Centrado(string texto)
        {
            foreach (var parte in Partir(texto, ancho))
                _texto.AppendLine(parte.PadLeft((ancho + parte.Length) / 2));
        }

        /// <summary>Etiqueta a la izquierda, importe pegado al borde derecho.</summary>
        public void Partido(string izquierda, string derecha)
        {
            var espacio = ancho - derecha.Length - 1;

            if (espacio < 1)
            {
                Izquierda(izquierda);
                _texto.AppendLine(derecha.PadLeft(ancho));
                return;
            }

            if (izquierda.Length > espacio)
                izquierda = izquierda[..espacio];

            // PadRight deja la etiqueta en espacio+1 caracteres, así que el renglón
            // mide exactamente el ancho del papel y el importe queda contra el borde.
            _texto.AppendLine($"{izquierda.PadRight(espacio + 1)}{derecha}");
        }

        private static IEnumerable<string> Partir(string texto, int ancho)
        {
            texto = texto.Trim();

            if (texto.Length == 0)
                return [string.Empty];

            var partes = new List<string>();

            for (var i = 0; i < texto.Length; i += ancho)
                partes.Add(texto.Substring(i, Math.Min(ancho, texto.Length - i)));

            return partes;
        }

        public override string ToString() => _texto.ToString();
    }
}

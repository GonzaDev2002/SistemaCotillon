namespace Cotillon.Shared.DTOs.Encargues;

public class EncargueDto
{
    public int EncargueId { get; set; }
    public int? ClienteId { get; set; }
    public string NombreContacto { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public DateTime FechaPedido { get; set; }
    public DateTime FechaEntrega { get; set; }
    public decimal Total { get; set; }
    public decimal Sena { get; set; }
    public string Estado { get; set; } = "PENDIENTE";
    public int? VentaId { get; set; }
    public string? Observaciones { get; set; }
    public List<LineaEncargueDto> Lineas { get; set; } = [];

    public decimal Saldo => Total - Sena;

    /// <summary>
    /// Negativo si ya pasó la fecha. Es el número que ordena la lista de lo que
    /// hay que tener listo.
    /// </summary>
    public int DiasParaEntrega => (int)(FechaEntrega.Date - DateTime.Today).TotalDays;

    public bool Atrasado => Estado == "PENDIENTE" && DiasParaEntrega < 0;
}

public class LineaEncargueDto
{
    public int DetalleEncargueId { get; set; }
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public string UnidadMedida { get; set; } = UnidadesMedida.Unidad;

    public decimal Subtotal => Cantidad * PrecioUnitario;

    public string CantidadFormateada => UnidadesMedida.De(UnidadMedida).Formatear(Cantidad);
}

public class CrearEncargueDto
{
    public int UsuarioId { get; set; }
    public int? ClienteId { get; set; }
    public string NombreContacto { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public DateTime FechaEntrega { get; set; }
    public decimal Sena { get; set; }
    public string? Observaciones { get; set; }
    public List<CrearLineaEncargueDto> Lineas { get; set; } = [];
}

public class CrearLineaEncargueDto
{
    public int ProductoId { get; set; }
    public decimal Cantidad { get; set; }
}

/// <summary>
/// Entrega del encargue: se convierte en venta y recién ahí descuenta stock. La
/// seña ya cobrada se descuenta de lo que falta pagar.
/// </summary>
public class EntregarEncargueDto
{
    public int UsuarioId { get; set; }
    public int MedioPagoId { get; set; }

    /// <summary>Lo que el cliente entrega ahora. Debe cubrir el saldo.</summary>
    public decimal Importe { get; set; }
}

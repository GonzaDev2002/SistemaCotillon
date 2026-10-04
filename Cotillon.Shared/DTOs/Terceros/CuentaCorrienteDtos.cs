namespace Cotillon.Shared.DTOs.Terceros;

/// <summary>
/// Un movimiento de la cuenta del cliente. Positivo es deuda que se toma,
/// negativo es plata que entra.
/// </summary>
public class MovimientoCuentaCorrienteDto
{
    public int MovimientoCtaCteId { get; set; }
    public int ClienteId { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }

    /// <summary>VENTA, PAGO, AJUSTE o ANULACION_VENTA.</summary>
    public string Tipo { get; set; } = string.Empty;

    public decimal Importe { get; set; }
    public int? VentaId { get; set; }
    public string? MedioPago { get; set; }
    public string? Observaciones { get; set; }

    /// <summary>Saldo acumulado hasta este movimiento, para leer el resumen de corrido.</summary>
    public decimal SaldoAcumulado { get; set; }

    public string Detalle => Tipo switch
    {
        "VENTA" => $"Venta #{VentaId}",
        "ANULACION_VENTA" => $"Anulación de la venta #{VentaId}",
        "PAGO" => $"Pago{(MedioPago is null ? string.Empty : $" en {MedioPago.ToLowerInvariant()}")}",
        _ => "Ajuste"
    };
}

public class CuentaCorrienteDto
{
    public int ClienteId { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public decimal Saldo { get; set; }
    public DateTime? UltimoMovimiento { get; set; }
    public List<MovimientoCuentaCorrienteDto> Movimientos { get; set; } = [];
}

/// <summary>Una fila de la lista de deudores.</summary>
public class SaldoClienteDto
{
    public int ClienteId { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public decimal Saldo { get; set; }
    public DateTime? UltimoMovimiento { get; set; }

    /// <summary>Días desde el último movimiento: lo que separa a un cliente al día de uno colgado.</summary>
    public int DiasSinMovimiento => UltimoMovimiento is { } fecha
        ? (int)(DateTime.Today - fecha.Date).TotalDays
        : 0;
}

public class RegistrarPagoCuentaDto
{
    public int UsuarioId { get; set; }
    public int ClienteId { get; set; }
    public decimal Importe { get; set; }
    public int MedioPagoId { get; set; }
    public string? Observaciones { get; set; }
}

public class AjustarCuentaDto
{
    public int UsuarioId { get; set; }
    public int ClienteId { get; set; }

    /// <summary>Positivo suma deuda, negativo la perdona.</summary>
    public decimal Importe { get; set; }

    public string Motivo { get; set; } = string.Empty;
}

namespace Cotillon.Shared.DTOs.Caja;

public class SesionCajaDto
{
    public int SesionCajaId { get; set; }
    public int UsuarioAperturaId { get; set; }
    public string UsuarioApertura { get; set; } = string.Empty;
    public int? UsuarioCierreId { get; set; }
    public string? UsuarioCierre { get; set; }
    public DateTime FechaApertura { get; set; }
    public DateTime? FechaCierre { get; set; }
    public decimal MontoInicial { get; set; }
    public decimal? MontoEsperado { get; set; }
    public decimal? MontoContado { get; set; }
    public decimal? Diferencia { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
}

public class AbrirCajaDto
{
    public int UsuarioId { get; set; }
    public decimal MontoInicial { get; set; }
    public string? Observaciones { get; set; }
}

public class CerrarCajaDto
{
    public int UsuarioId { get; set; }
    public decimal MontoContado { get; set; }
    public string? Observaciones { get; set; }
}

public class MovimientoCajaDto
{
    public int MovimientoCajaId { get; set; }
    public int SesionCajaId { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public DateTime FechaMovimiento { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public string? MedioPago { get; set; }
    public decimal Importe { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
}

public class RegistrarMovimientoCajaDto
{
    public int UsuarioId { get; set; }
    /// <summary>INGRESO, EGRESO o RETIRO.</summary>
    public string TipoMovimiento { get; set; } = string.Empty;
    public decimal Importe { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
}

/// <summary>
/// Estado de la caja abierta con el detalle que necesita el arqueo: lo que debería
/// haber en efectivo y cuánto se cobró por cada medio de pago.
/// </summary>
public class ArqueoCajaDto
{
    public SesionCajaDto Sesion { get; set; } = new();
    public decimal TotalVentas { get; set; }
    public int CantidadVentas { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal TotalEgresos { get; set; }
    public decimal EfectivoEsperado { get; set; }
    public List<TotalPorMedioPagoDto> TotalesPorMedioPago { get; set; } = [];
}

public class TotalPorMedioPagoDto
{
    public string MedioPago { get; set; } = string.Empty;
    public bool AfectaEfectivo { get; set; }
    public decimal Total { get; set; }
    public int Cantidad { get; set; }
}

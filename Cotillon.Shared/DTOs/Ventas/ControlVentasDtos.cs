namespace Cotillon.Shared.DTOs.Ventas;

/// <summary>
/// Una venta que se armó y no se cobró.
///
/// Que exista este registro es el punto. Sin él, armar el carrito, cobrarle al
/// cliente y apretar Escape no deja absolutamente nada: la plata se va y el
/// arqueo cierra perfecto porque esa venta nunca existió para el sistema.
/// </summary>
public class CancelarVentaEnCursoDto
{
    public int UsuarioId { get; set; }

    /// <summary>Lo que iba a cobrarse. Es el número que vuelve esto revisable.</summary>
    public decimal Total { get; set; }

    public int CantidadItems { get; set; }

    /// <summary>Qué tenía el carrito, resumido, para poder reconstruir qué pasó.</summary>
    public string Detalle { get; set; } = string.Empty;

    public string Motivo { get; set; } = string.Empty;
}

/// <summary>
/// Cómo cerró cada usuario sus cajas. Un faltante aislado es un error de vuelto;
/// el mismo faltante todos los días es otra cosa, y eso sólo se ve acumulado.
/// </summary>
public class ControlPorUsuarioDto
{
    public int UsuarioId { get; set; }
    public string Usuario { get; set; } = string.Empty;

    public int CajasCerradas { get; set; }
    public int CajasConFaltante { get; set; }
    public decimal FaltanteAcumulado { get; set; }
    public decimal SobranteAcumulado { get; set; }
    public decimal PeorFaltante { get; set; }

    public int VentasCanceladas { get; set; }
    public decimal ImporteCancelado { get; set; }
    public int VentasAnuladas { get; set; }
    public decimal ImporteAnulado { get; set; }

    public decimal TotalVendido { get; set; }

    /// <summary>
    /// Qué proporción de las cajas cerró con faltante. Es el número que distingue
    /// al que se equivoca con el vuelto del que tiene un problema.
    /// </summary>
    public decimal PorcentajeConFaltante => CajasCerradas > 0
        ? (decimal)CajasConFaltante / CajasCerradas * 100m
        : 0m;

    /// <summary>
    /// Cuánto de lo que pasó por sus manos terminó cancelado o anulado. Un valor
    /// alto acá no prueba nada por sí solo, pero dice dónde mirar.
    /// </summary>
    public decimal PorcentajeCancelado => TotalVendido + ImporteCancelado > 0
        ? (ImporteCancelado + ImporteAnulado) / (TotalVendido + ImporteCancelado) * 100m
        : 0m;
}

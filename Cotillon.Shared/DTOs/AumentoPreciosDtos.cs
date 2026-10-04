namespace Cotillon.Shared.DTOs;

/// <summary>
/// A qué productos se les toca el precio. Los filtros se combinan: sección Y
/// categoría Y marca. En blanco, el filtro no limita.
/// </summary>
public class AumentoPreciosDto
{
    public int UsuarioId { get; set; }

    public int? SeccionId { get; set; }
    public int? CategoriaId { get; set; }
    public string? Marca { get; set; }
    public string? Temporada { get; set; }

    /// <summary>Porcentaje a aplicar. Negativo baja precios.</summary>
    public decimal Porcentaje { get; set; }

    /// <summary>
    /// Si también se ajusta el costo. Cuando el aumento viene del proveedor es lo
    /// correcto: si sólo sube el precio, el margen que informa el sistema miente.
    /// </summary>
    public bool AjustarCosto { get; set; } = true;

    /// <summary>A cuánto se redondea el precio nuevo: 0 no redondea, 100 redondea a $100.</summary>
    public int RedondearA { get; set; }
}

/// <summary>Una fila de la vista previa: qué producto queda en cuánto.</summary>
public class CambioPrecioDto
{
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string NombreCategoria { get; set; } = string.Empty;
    public decimal PrecioActual { get; set; }
    public decimal PrecioNuevo { get; set; }
    public decimal CostoActual { get; set; }
    public decimal CostoNuevo { get; set; }

    public decimal Diferencia => PrecioNuevo - PrecioActual;

    /// <summary>
    /// Avisa cuando el precio nuevo queda por debajo del costo nuevo. Es lo único
    /// que hay que mirar de verdad en una lista de doscientas filas.
    /// </summary>
    public bool QuedaEnPerdida => PrecioNuevo < CostoNuevo && CostoNuevo > 0;
}

public class ResultadoAumentoDto
{
    public int Afectados { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}

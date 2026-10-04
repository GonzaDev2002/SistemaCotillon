namespace Cotillon.Shared.DTOs;

public class ProductoDto
{
    public int ProductoId { get; set; }
    public int CategoriaId { get; set; }
    public string CodigoProducto { get; set; } = string.Empty;
    public string NombreProducto { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    // Decimal: lo que se vende por peso o por metro deja fracciones en stock.
    public decimal Stock { get; set; }
    public decimal StockMinimo { get; set; }
    public string CodigoBarras { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public decimal Costo { get; set; }
    public string ImagenUrl { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = string.Empty;
    public bool PermiteVentaSinStock { get; set; }

    /// <summary>
    /// Cuántas unidades trae el bulto cerrado: la bolsa de globos viene de 50.
    /// El stock se lleva siempre en unidades; esto sólo define cuánto se
    /// descuenta cuando se vende un bulto entero. 1 = no se vende por bulto.
    /// </summary>
    public decimal UnidadesPorBulto { get; set; } = 1;

    /// <summary>Precio del bulto cerrado. En cero se cobra el precio unitario por la cantidad.</summary>
    public decimal PrecioBulto { get; set; }

    /// <summary>Código de <see cref="Temporadas"/>. Vacío es todo el año.</summary>
    public string Temporada { get; set; } = string.Empty;

    /// <summary>
    /// Si es un combo (la bolsita sorpresa), no tiene stock propio: se arma en el
    /// momento y al venderlo se descuentan sus componentes.
    /// </summary>
    public bool EsCombo { get; set; }

    public List<ComponenteComboDto> Componentes { get; set; } = [];

    public bool SeVendePorBulto => UnidadesPorBulto > 1;

    /// <summary>
    /// Sólo informativo al leer: el alta y la modificación no lo tocan, la baja
    /// lógica va por su propio endpoint.
    /// </summary>
    public bool Activo { get; set; } = true;
}
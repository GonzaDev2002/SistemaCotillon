namespace Cotillon.Shared.DTOs;

public class ProductoListadoDto
{
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string NombreSeccion { get; set; } = string.Empty;
    public string NombreCategoria { get; set; } = string.Empty;
    public decimal Precio { get; set; }

    // Decimal y no entero: lo que se vende por peso deja medio kilo en stock, y
    // redondearlo a 0 escondería mercadería que está en el mostrador.
    public decimal Stock { get; set; }

    public bool Activo { get; set; }

    /// <summary>
    /// UNIDAD, KILO o METRO. La pantalla de ventas la necesita para saber si al
    /// agregar el producto tiene que preguntar el peso o sumar uno y seguir.
    /// </summary>
    public string UnidadMedida { get; set; } = UnidadesMedida.Unidad;

    /// <summary>Unidades del bulto cerrado. 1 = el producto no se vende por bulto.</summary>
    public decimal UnidadesPorBulto { get; set; } = 1;

    /// <summary>Precio del bulto. En cero se cobra el unitario por la cantidad.</summary>
    public decimal PrecioBulto { get; set; }

    /// <summary>
    /// Un combo no tiene stock propio: el que viaja acá es cuántos se pueden
    /// armar con lo que hay de sus componentes.
    /// </summary>
    public bool EsCombo { get; set; }

    public string Temporada { get; set; } = string.Empty;

    public bool SeVendePorBulto => UnidadesPorBulto > 1;

    public string StockFormateado => EsCombo
        ? $"{Stock:0.##} armables"
        : UnidadesMedida.De(UnidadMedida).Formatear(Stock);
}

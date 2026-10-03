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

    public string StockFormateado => UnidadesMedida.De(UnidadMedida).Formatear(Stock);
}

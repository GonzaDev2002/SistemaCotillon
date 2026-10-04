using Cotillon.Data;
using Cotillon.Shared.DTOs;
using Dapper;

namespace Cotillon.LocalApi.Service.Repositories;

public class AumentoInvalidoException(string mensaje) : Exception(mensaje);

/// <summary>
/// Aumento de precios en masa.
///
/// El proveedor aumenta un 12 % y hay cuatrocientos productos: de a uno son
/// cuatrocientos formularios. Lo importante no es el UPDATE sino poder ver
/// antes en cuánto queda cada cosa, porque un porcentaje mal tipeado aplicado
/// a toda una sección no se nota hasta que alguien compra.
/// </summary>
public class PreciosRepository
{
    private readonly IDbConnectionFactory _factory;

    public PreciosRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public List<CambioPrecioDto> Simular(AumentoPreciosDto aumento)
    {
        Validar(aumento);

        using var conexion = _factory.Crear();

        var filas = conexion.Query<FilaPrecio>(Consulta(aumento), Parametros(aumento));

        return filas.Select(f => new CambioPrecioDto
        {
            ProductoId = f.ProductoId,
            NombreProducto = f.NombreProducto,
            NombreCategoria = f.NombreCategoria,
            PrecioActual = Money.FromCentavos(f.PrecioCentavos),
            PrecioNuevo = Money.FromCentavos(Nuevo(f.PrecioCentavos, aumento)),
            CostoActual = Money.FromCentavos(f.CostoCentavos),
            CostoNuevo = Money.FromCentavos(
                aumento.AjustarCosto ? Nuevo(f.CostoCentavos, aumento) : f.CostoCentavos)
        }).ToList();
    }

    /// <summary>
    /// Aplica el aumento. Recalcula fila por fila en vez de hacer un UPDATE con
    /// aritmética en SQL, para que lo que se guarda sea exactamente lo que el
    /// usuario vio en la vista previa, redondeo incluido.
    /// </summary>
    public int Aplicar(AumentoPreciosDto aumento)
    {
        var cambios = Simular(aumento);

        if (cambios.Count == 0)
            throw new AumentoInvalidoException("Ningún producto coincide con el filtro elegido.");

        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        conexion.Execute(
            """
            UPDATE Productos SET
                PrecioCentavos = @Precio,
                CostoCentavos = @Costo,
                FechaActualizacion = datetime('now', 'localtime')
            WHERE ProductoId = @ProductoId;
            """,
            cambios.Select(c => new
            {
                c.ProductoId,
                Precio = Money.ToCentavos(c.PrecioNuevo),
                Costo = Money.ToCentavos(c.CostoNuevo)
            }), tx);

        Auditoria.Registrar(conexion, tx, aumento.UsuarioId, "Productos", "MODIFICACION", null,
            $"Ajuste de precios del {aumento.Porcentaje:0.##} % sobre {cambios.Count} producto(s)" +
            $"{(aumento.AjustarCosto ? ", costo incluido" : string.Empty)}" +
            $"{(aumento.RedondearA > 0 ? $", redondeado a ${aumento.RedondearA}" : string.Empty)}");

        tx.Commit();

        return cambios.Count;
    }

    private static void Validar(AumentoPreciosDto aumento)
    {
        if (aumento.Porcentaje == 0)
            throw new AumentoInvalidoException("El porcentaje no puede ser cero.");

        if (aumento.Porcentaje <= -100)
            throw new AumentoInvalidoException("Una baja del 100 % o más dejaría los precios en cero o negativos.");

        // Un 1200 % casi siempre es un cero de más. Se frena acá porque deshacerlo
        // después es volver a cargar la lista entera a mano.
        if (aumento.Porcentaje > 300)
            throw new AumentoInvalidoException(
                $"Un aumento del {aumento.Porcentaje:0.##} % parece un error de tipeo. El máximo de una vez es 300 %.");

        if (aumento.RedondearA < 0)
            throw new AumentoInvalidoException("El redondeo no puede ser negativo.");
    }

    private long Nuevo(long centavos, AumentoPreciosDto aumento)
    {
        var ajustado = centavos * (1m + aumento.Porcentaje / 100m);

        if (aumento.RedondearA > 0 && ajustado > 0)
        {
            // El redondeo se piensa en pesos, no en centavos: "a los $100" quiere
            // decir que el precio termine en 00, no que el centavo sea redondo.
            var paso = aumento.RedondearA * 100m;
            ajustado = Math.Round(ajustado / paso, MidpointRounding.AwayFromZero) * paso;

            // Redondear hacia abajo nunca puede dejar el precio en cero.
            if (ajustado < paso)
                ajustado = paso;
        }

        return (long)Math.Round(ajustado, MidpointRounding.AwayFromZero);
    }

    private static string Consulta(AumentoPreciosDto aumento)
    {
        var filtros = new List<string> { "p.Activo = 1" };

        if (aumento.SeccionId is > 0) filtros.Add("s.SeccionId = @SeccionId");
        if (aumento.CategoriaId is > 0) filtros.Add("p.CategoriaId = @CategoriaId");
        if (!string.IsNullOrWhiteSpace(aumento.Marca)) filtros.Add("p.Marca = @Marca");
        if (!string.IsNullOrWhiteSpace(aumento.Temporada)) filtros.Add("p.Temporada = @Temporada");

        return $"""
            SELECT p.ProductoId, p.NombreProducto, p.PrecioCentavos, p.CostoCentavos,
                   COALESCE(c.NombreCategoria, '(Sin categoría)') AS NombreCategoria
            FROM Productos p
            LEFT JOIN Categorias c ON p.CategoriaId = c.CategoriaId
            LEFT JOIN Secciones  s ON c.SeccionId   = s.SeccionId
            WHERE {string.Join(" AND ", filtros)}
            ORDER BY p.NombreProducto;
            """;
    }

    private static object Parametros(AumentoPreciosDto aumento) => new
    {
        aumento.SeccionId,
        aumento.CategoriaId,
        Marca = aumento.Marca?.Trim(),
        Temporada = aumento.Temporada?.Trim()
    };

    private sealed class FilaPrecio
    {
        public int ProductoId { get; init; }
        public string NombreProducto { get; init; } = string.Empty;
        public string NombreCategoria { get; init; } = string.Empty;
        public long PrecioCentavos { get; init; }
        public long CostoCentavos { get; init; }
    }
}

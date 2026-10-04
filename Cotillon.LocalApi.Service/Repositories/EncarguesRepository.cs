using System.Data;
using Cotillon.Data;
using Cotillon.Shared.DTOs.Encargues;
using Cotillon.Shared.DTOs.Ventas;
using Dapper;

namespace Cotillon.LocalApi.Service.Repositories;

public class EncargueInvalidoException(string mensaje) : Exception(mensaje);

/// <summary>
/// Los encargues: "me preparás 60 bolsitas para el 15, te dejo seña".
///
/// Tomar el pedido NO toca el stock. La mercadería sigue en el salón y se puede
/// vender a quien entre por la puerta: reservarla sería esconder stock que
/// existe. El descuento pasa al entregar, que es cuando el encargue se convierte
/// en venta y recién ahí la mercadería sale.
///
/// La seña tampoco es una venta: es plata que entra a la caja contra una entrega
/// futura. Al entregar, lo que falta cobrar es el total menos lo que ya se señó.
/// </summary>
public class EncarguesRepository
{
    private readonly IDbConnectionFactory _factory;
    private readonly VentasRepository _ventas;

    public EncarguesRepository(IDbConnectionFactory factory, VentasRepository ventas)
    {
        _factory = factory;
        _ventas = ventas;
    }

    public int Crear(CrearEncargueDto datos)
    {
        if (datos.Lineas.Count == 0)
            throw new EncargueInvalidoException("El encargue no tiene productos.");

        if (string.IsNullOrWhiteSpace(datos.NombreContacto))
            throw new EncargueInvalidoException("Poné a nombre de quién va el encargue.");

        if (datos.FechaEntrega.Date < DateTime.Today)
            throw new EncargueInvalidoException("La fecha de entrega ya pasó.");

        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        var lineas = new List<(int ProductoId, string Nombre, double Cantidad, long PrecioCentavos)>();

        foreach (var linea in datos.Lineas)
        {
            if (linea.Cantidad <= 0)
                throw new EncargueInvalidoException("La cantidad de cada ítem debe ser mayor a cero.");

            var producto = conexion.QuerySingleOrDefault<FilaProducto>(
                "SELECT ProductoId, NombreProducto, PrecioCentavos, Activo FROM Productos WHERE ProductoId = @Id;",
                new { Id = linea.ProductoId }, tx)
                ?? throw new EncargueInvalidoException($"El producto {linea.ProductoId} no existe.");

            if (!producto.Activo)
                throw new EncargueInvalidoException($"El producto '{producto.NombreProducto}' está dado de baja.");

            lineas.Add((producto.ProductoId, producto.NombreProducto,
                (double)linea.Cantidad, producto.PrecioCentavos));
        }

        var totalCentavos = lineas.Sum(l => Money.ToCentavos(
            Money.FromCentavos(l.PrecioCentavos) * (decimal)l.Cantidad));

        var senaCentavos = Money.ToCentavos(datos.Sena);

        if (senaCentavos < 0)
            throw new EncargueInvalidoException("La seña no puede ser negativa.");

        if (senaCentavos > totalCentavos)
            throw new EncargueInvalidoException(
                $"La seña supera el total del encargue ({Money.FromCentavos(totalCentavos):C2}).");

        var encargueId = conexion.ExecuteScalar<int>(
            """
            INSERT INTO Encargues
                (ClienteId, UsuarioId, NombreContacto, Telefono, FechaEntrega,
                 TotalCentavos, SenaCentavos, Estado, Observaciones)
            VALUES
                (@ClienteId, @UsuarioId, @Nombre, @Telefono, @FechaEntrega,
                 @Total, @Sena, 'PENDIENTE', @Observaciones);
            SELECT last_insert_rowid();
            """,
            new
            {
                ClienteId = datos.ClienteId is > 0 ? datos.ClienteId : null,
                datos.UsuarioId,
                Nombre = datos.NombreContacto.Trim(),
                Telefono = Opcional(datos.Telefono),
                FechaEntrega = datos.FechaEntrega.Date,
                Total = totalCentavos,
                Sena = senaCentavos,
                Observaciones = Opcional(datos.Observaciones)
            }, tx);

        conexion.Execute(
            """
            INSERT INTO DetalleEncargues
                (EncargueId, ProductoId, NombreProducto, Cantidad, PrecioUnitarioCentavos)
            VALUES (@EncargueId, @ProductoId, @Nombre, @Cantidad, @Precio);
            """,
            lineas.Select(l => new
            {
                EncargueId = encargueId,
                l.ProductoId,
                l.Nombre,
                l.Cantidad,
                Precio = l.PrecioCentavos
            }), tx);

        // La seña entra a la caja ahora, no al entregar: es plata que el cliente
        // dejó hoy y el arqueo de hoy la tiene que contar.
        if (senaCentavos > 0)
        {
            var sesion = CajaRepository.ObtenerSesionAbierta(conexion, tx)
                ?? throw new EncargueInvalidoException(
                    "No hay una caja abierta. Abrí la caja para poder tomar la seña.");

            conexion.Execute(
                """
                INSERT INTO MovimientosCaja
                    (SesionCajaId, UsuarioId, TipoMovimiento, ImporteCentavos, Motivo, OrigenTipo, OrigenId)
                VALUES (@SesionId, @UsuarioId, 'INGRESO', @Importe, @Motivo, 'MANUAL', @EncargueId);
                """,
                new
                {
                    SesionId = sesion.SesionCajaId,
                    datos.UsuarioId,
                    Importe = senaCentavos,
                    Motivo = $"Seña del encargue #{encargueId} ({datos.NombreContacto.Trim()})",
                    EncargueId = encargueId
                }, tx);
        }

        Auditoria.Registrar(conexion, tx, datos.UsuarioId, "Encargues", "ALTA", encargueId,
            $"Encargue para el {datos.FechaEntrega:dd/MM/yyyy} por " +
            $"{Money.FromCentavos(totalCentavos):C2}, seña {Money.FromCentavos(senaCentavos):C2}");

        tx.Commit();

        return encargueId;
    }

    /// <summary>
    /// Entrega el encargue: lo convierte en venta, que es lo que descuenta stock y
    /// deja el comprobante. La seña ya cobrada se descuenta del saldo a pagar.
    /// </summary>
    public int Entregar(int encargueId, EntregarEncargueDto datos)
    {
        var encargue = Obtener(encargueId)
            ?? throw new EncargueInvalidoException("No se encontró el encargue.");

        if (encargue.Estado != "PENDIENTE")
            throw new EncargueInvalidoException($"El encargue ya está {encargue.Estado.ToLowerInvariant()}.");

        var saldo = encargue.Saldo;

        if (datos.Importe < saldo)
            throw new EncargueInvalidoException(
                $"Falta cobrar {(saldo - datos.Importe):C2}. El saldo del encargue es {saldo:C2}.");

        // La venta se registra por el saldo y no por el total: la seña ya entró a
        // la caja el día que se tomó el pedido, y volver a cobrarla acá la contaría
        // dos veces en el arqueo.
        var venta = new CrearVentaDto
        {
            UsuarioId = datos.UsuarioId,
            ClienteId = encargue.ClienteId,
            TipoComprobante = "TICKET",
            DescuentoTotal = encargue.Sena,
            Observaciones = $"Entrega del encargue #{encargueId} ({encargue.NombreContacto})",
            Lineas = [.. encargue.Lineas.Select(l => new CrearLineaVentaDto
            {
                ProductoId = l.ProductoId,
                Cantidad = l.Cantidad
            })],
            Pagos = [new CrearPagoVentaDto { MedioPagoId = datos.MedioPagoId, Importe = datos.Importe }]
        };

        var ventaId = _ventas.Crear(venta, permitirVentaSinStockGlobal: false);

        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        conexion.Execute(
            """
            UPDATE Encargues SET Estado = 'ENTREGADO', VentaId = @VentaId
            WHERE EncargueId = @Id AND Estado = 'PENDIENTE';
            """,
            new { Id = encargueId, VentaId = ventaId }, tx);

        Auditoria.Registrar(conexion, tx, datos.UsuarioId, "Encargues", "MODIFICACION", encargueId,
            $"Encargue entregado. Venta #{ventaId}");

        tx.Commit();

        return ventaId;
    }

    /// <summary>
    /// Cancela el pedido. La seña NO se devuelve sola: si corresponde devolverla,
    /// es un egreso de caja que alguien tiene que registrar y firmar.
    /// </summary>
    public void Cancelar(int encargueId, int usuarioId, string motivo)
    {
        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        var estado = conexion.ExecuteScalar<string?>(
            "SELECT Estado FROM Encargues WHERE EncargueId = @Id;", new { Id = encargueId }, tx)
            ?? throw new EncargueInvalidoException("No se encontró el encargue.");

        if (estado != "PENDIENTE")
            throw new EncargueInvalidoException($"El encargue ya está {estado.ToLowerInvariant()}.");

        conexion.Execute(
            """
            UPDATE Encargues SET
                Estado = 'CANCELADO',
                Observaciones = COALESCE(Observaciones || ' · ', '') || @Motivo
            WHERE EncargueId = @Id;
            """,
            new { Id = encargueId, Motivo = $"Cancelado: {motivo}" }, tx);

        Auditoria.Registrar(conexion, tx, usuarioId, "Encargues", "MODIFICACION", encargueId,
            $"Encargue cancelado. Motivo: {motivo}");

        tx.Commit();
    }

    public EncargueDto? Obtener(int encargueId)
    {
        using var conexion = _factory.Crear();

        var encargue = conexion.QuerySingleOrDefault<FilaEncargue>(
            $"{ConsultaEncargues} WHERE e.EncargueId = @Id;", new { Id = encargueId });

        if (encargue is null)
            return null;

        var resultado = AEncargue(encargue);

        resultado.Lineas = conexion.Query<FilaDetalle>(
            """
            SELECT d.DetalleEncargueId, d.ProductoId, d.NombreProducto, d.Cantidad,
                   d.PrecioUnitarioCentavos,
                   COALESCE(p.UnidadMedida, 'UNIDAD') AS UnidadMedida
            FROM DetalleEncargues d
            LEFT JOIN Productos p ON d.ProductoId = p.ProductoId
            WHERE d.EncargueId = @Id
            ORDER BY d.DetalleEncargueId;
            """,
            new { Id = encargueId })
            .Select(d => new LineaEncargueDto
            {
                DetalleEncargueId = d.DetalleEncargueId,
                ProductoId = d.ProductoId,
                NombreProducto = d.NombreProducto,
                Cantidad = (decimal)d.Cantidad,
                PrecioUnitario = Money.FromCentavos(d.PrecioUnitarioCentavos),
                UnidadMedida = d.UnidadMedida
            }).ToList();

        return resultado;
    }

    /// <summary>
    /// La agenda de entregas. Ordena por fecha de entrega y no por fecha de
    /// pedido: lo que importa es qué hay que tener listo primero.
    /// </summary>
    public List<EncargueDto> Listar(DateTime? hasta, bool incluirCerrados)
    {
        using var conexion = _factory.Crear();

        var filtros = new List<string>();

        if (!incluirCerrados)
            filtros.Add("e.Estado = 'PENDIENTE'");

        if (hasta is not null)
            filtros.Add("date(e.FechaEntrega) <= date(@Hasta)");

        var donde = filtros.Count > 0 ? $"WHERE {string.Join(" AND ", filtros)}" : string.Empty;

        return conexion.Query<FilaEncargue>(
                $"""
                {ConsultaEncargues}
                {donde}
                ORDER BY CASE WHEN e.Estado = 'PENDIENTE' THEN 0 ELSE 1 END, e.FechaEntrega, e.EncargueId;
                """,
                new { Hasta = hasta?.Date })
            .Select(AEncargue)
            .ToList();
    }

    private const string ConsultaEncargues =
        """
        SELECT e.EncargueId, e.ClienteId, e.NombreContacto, e.Telefono,
               e.FechaPedido, e.FechaEntrega, e.TotalCentavos, e.SenaCentavos,
               e.Estado, e.VentaId, e.Observaciones,
               COALESCE(u.NombreCompleto, '') AS Usuario
        FROM Encargues e
        LEFT JOIN Usuarios u ON e.UsuarioId = u.UsuarioId
        """;

    private static EncargueDto AEncargue(FilaEncargue f) => new()
    {
        EncargueId = f.EncargueId,
        ClienteId = f.ClienteId,
        NombreContacto = f.NombreContacto,
        Telefono = f.Telefono,
        Usuario = f.Usuario,
        FechaPedido = f.FechaPedido,
        FechaEntrega = f.FechaEntrega,
        Total = Money.FromCentavos(f.TotalCentavos),
        Sena = Money.FromCentavos(f.SenaCentavos),
        Estado = f.Estado,
        VentaId = f.VentaId,
        Observaciones = f.Observaciones
    };

    private static string? Opcional(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    private sealed class FilaProducto
    {
        public int ProductoId { get; init; }
        public string NombreProducto { get; init; } = string.Empty;
        public long PrecioCentavos { get; init; }
        public bool Activo { get; init; }
    }

    private sealed class FilaEncargue
    {
        public int EncargueId { get; init; }
        public int? ClienteId { get; init; }
        public string NombreContacto { get; init; } = string.Empty;
        public string? Telefono { get; init; }
        public string Usuario { get; init; } = string.Empty;
        public DateTime FechaPedido { get; init; }
        public DateTime FechaEntrega { get; init; }
        public long TotalCentavos { get; init; }
        public long SenaCentavos { get; init; }
        public string Estado { get; init; } = string.Empty;
        public int? VentaId { get; init; }
        public string? Observaciones { get; init; }
    }

    private sealed class FilaDetalle
    {
        public int DetalleEncargueId { get; init; }
        public int ProductoId { get; init; }
        public string NombreProducto { get; init; } = string.Empty;
        public double Cantidad { get; init; }
        public long PrecioUnitarioCentavos { get; init; }
        public string UnidadMedida { get; init; } = "UNIDAD";
    }
}

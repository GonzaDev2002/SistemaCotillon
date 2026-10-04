using System.Data;
using Cotillon.Data;
using Cotillon.Shared.DTOs.Terceros;
using Dapper;

namespace Cotillon.LocalApi.Service.Repositories;

public class CuentaCorrienteInvalidaException(string mensaje) : Exception(mensaje);

/// <summary>
/// La cuenta corriente de los clientes que compran en el mes y pagan a fin de
/// mes: jardines, salones, organizadoras de eventos.
///
/// El saldo no se guarda en ninguna parte, se suma de los movimientos. Un total
/// guardado que deja de coincidir con su detalle es el peor lugar donde puede
/// estar un error de plata, porque nadie sabe cuál de los dos números creer.
/// </summary>
public class CuentaCorrienteRepository
{
    /// <summary>
    /// El medio de pago que no mueve plata: queda anotado en la cuenta del
    /// cliente. Se busca por nombre porque el id lo asigna la base al sembrar.
    /// </summary>
    public const string MedioCuentaCorriente = "CUENTA CORRIENTE";

    private readonly IDbConnectionFactory _factory;

    public CuentaCorrienteRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public static int? IdDelMedioCuentaCorriente(IDbConnection conexion, IDbTransaction? tx = null) =>
        conexion.ExecuteScalar<int?>(
            "SELECT MedioPagoId FROM MediosPago WHERE NombreMedioPago = @Nombre;",
            new { Nombre = MedioCuentaCorriente }, tx);

    /// <summary>
    /// Anota en la cuenta del cliente lo que se llevó sin pagar. Lo llama la venta
    /// dentro de su propia transacción: si la venta se cae, la deuda no queda.
    /// </summary>
    public static void RegistrarVenta(
        IDbConnection conexion, IDbTransaction tx,
        int clienteId, int usuarioId, int ventaId, long importeCentavos)
    {
        if (importeCentavos <= 0)
            return;

        conexion.Execute(
            """
            INSERT INTO MovimientosCuentaCorriente
                (ClienteId, UsuarioId, Tipo, ImporteCentavos, VentaId, Observaciones)
            VALUES (@ClienteId, @UsuarioId, 'VENTA', @Importe, @VentaId, NULL);
            """,
            new { ClienteId = clienteId, UsuarioId = usuarioId, Importe = importeCentavos, VentaId = ventaId },
            tx);
    }

    /// <summary>
    /// Da de baja la deuda de una venta anulada. Se anota como un movimiento más
    /// en vez de borrar el original: el resumen del cliente tiene que poder
    /// explicar por qué su saldo bajó.
    /// </summary>
    public static void RegistrarAnulacion(
        IDbConnection conexion, IDbTransaction tx, int usuarioId, int ventaId)
    {
        var original = conexion.QueryFirstOrDefault<FilaDeuda>(
            """
            SELECT ClienteId, ImporteCentavos
            FROM MovimientosCuentaCorriente
            WHERE VentaId = @VentaId AND Tipo = 'VENTA';
            """,
            new { VentaId = ventaId }, tx);

        if (original is null)
            return;

        conexion.Execute(
            """
            INSERT INTO MovimientosCuentaCorriente
                (ClienteId, UsuarioId, Tipo, ImporteCentavos, VentaId, Observaciones)
            VALUES (@ClienteId, @UsuarioId, 'ANULACION_VENTA', @Importe, @VentaId, @Obs);
            """,
            new
            {
                original.ClienteId,
                UsuarioId = usuarioId,
                Importe = -original.ImporteCentavos,
                VentaId = ventaId,
                Obs = $"Anulación de la venta #{ventaId}"
            }, tx);
    }

    public decimal Saldo(int clienteId)
    {
        using var conexion = _factory.Crear();

        return Money.FromCentavos(conexion.ExecuteScalar<long>(
            "SELECT COALESCE(SUM(ImporteCentavos), 0) FROM MovimientosCuentaCorriente WHERE ClienteId = @Id;",
            new { Id = clienteId }));
    }

    public CuentaCorrienteDto Obtener(int clienteId)
    {
        using var conexion = _factory.Crear();

        var cliente = conexion.QueryFirstOrDefault<string>(
            "SELECT NombreCompleto FROM Clientes WHERE ClienteId = @Id;", new { Id = clienteId })
            ?? throw new CuentaCorrienteInvalidaException("No se encontró el cliente.");

        var filas = conexion.Query<FilaMovimiento>(
            """
            SELECT m.MovimientoCtaCteId, m.ClienteId, m.Fecha, m.Tipo, m.ImporteCentavos,
                   m.VentaId, m.Observaciones,
                   COALESCE(u.NombreCompleto, '') AS Usuario,
                   mp.NombreMedioPago AS MedioPago
            FROM MovimientosCuentaCorriente m
            LEFT JOIN Usuarios   u  ON m.UsuarioId   = u.UsuarioId
            LEFT JOIN MediosPago mp ON m.MedioPagoId = mp.MedioPagoId
            WHERE m.ClienteId = @Id
            ORDER BY m.Fecha, m.MovimientoCtaCteId;
            """,
            new { Id = clienteId }).ToList();

        var acumulado = 0L;
        var movimientos = new List<MovimientoCuentaCorrienteDto>();

        foreach (var fila in filas)
        {
            acumulado += fila.ImporteCentavos;

            movimientos.Add(new MovimientoCuentaCorrienteDto
            {
                MovimientoCtaCteId = fila.MovimientoCtaCteId,
                ClienteId = fila.ClienteId,
                Cliente = cliente,
                Usuario = fila.Usuario,
                Fecha = fila.Fecha,
                Tipo = fila.Tipo,
                Importe = Money.FromCentavos(fila.ImporteCentavos),
                VentaId = fila.VentaId,
                MedioPago = fila.MedioPago,
                Observaciones = fila.Observaciones,
                SaldoAcumulado = Money.FromCentavos(acumulado)
            });
        }

        // El más reciente primero: es lo que se mira al atender a alguien.
        movimientos.Reverse();

        return new CuentaCorrienteDto
        {
            ClienteId = clienteId,
            Cliente = cliente,
            Saldo = Money.FromCentavos(acumulado),
            UltimoMovimiento = movimientos.Count > 0 ? movimientos[0].Fecha : null,
            Movimientos = movimientos
        };
    }

    /// <summary>
    /// Los que deben. Con <paramref name="soloConDeuda"/> en false se ven también
    /// los que están en cero o a favor, que es lo que hace falta al buscar un
    /// cliente para registrarle un pago.
    /// </summary>
    public List<SaldoClienteDto> Saldos(bool soloConDeuda = true)
    {
        using var conexion = _factory.Crear();

        var filas = conexion.Query<FilaSaldo>(
            $"""
            SELECT c.ClienteId, c.NombreCompleto AS Cliente, c.Telefono,
                   COALESCE(SUM(m.ImporteCentavos), 0) AS SaldoCentavos,
                   MAX(m.Fecha)                        AS UltimoMovimiento
            FROM Clientes c
            LEFT JOIN MovimientosCuentaCorriente m ON c.ClienteId = m.ClienteId
            WHERE c.Activo = 1
            GROUP BY c.ClienteId, c.NombreCompleto, c.Telefono
            {(soloConDeuda ? "HAVING SaldoCentavos <> 0" : string.Empty)}
            ORDER BY SaldoCentavos DESC, c.NombreCompleto;
            """);

        return filas.Select(f => new SaldoClienteDto
        {
            ClienteId = f.ClienteId,
            Cliente = f.Cliente,
            Telefono = f.Telefono,
            Saldo = Money.FromCentavos(f.SaldoCentavos),
            UltimoMovimiento = f.UltimoMovimiento
        }).ToList();
    }

    /// <summary>
    /// El cliente paga. La plata entra al cajón si el medio es efectivo, igual que
    /// cualquier otro ingreso, así que el arqueo del día la tiene que contar.
    /// </summary>
    public void RegistrarPago(RegistrarPagoCuentaDto pago)
    {
        if (pago.Importe <= 0)
            throw new CuentaCorrienteInvalidaException("El importe del pago tiene que ser mayor a cero.");

        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        var saldoCentavos = conexion.ExecuteScalar<long>(
            "SELECT COALESCE(SUM(ImporteCentavos), 0) FROM MovimientosCuentaCorriente WHERE ClienteId = @Id;",
            new { Id = pago.ClienteId }, tx);

        if (saldoCentavos <= 0)
            throw new CuentaCorrienteInvalidaException("El cliente no tiene deuda para pagar.");

        var importeCentavos = Money.ToCentavos(pago.Importe);

        if (importeCentavos > saldoCentavos)
            throw new CuentaCorrienteInvalidaException(
                $"El pago supera la deuda. El cliente debe {Money.FromCentavos(saldoCentavos):C2}.");

        var medio = conexion.QueryFirstOrDefault<FilaMedio>(
            "SELECT MedioPagoId, NombreMedioPago, AfectaEfectivo FROM MediosPago WHERE MedioPagoId = @Id AND Activo = 1;",
            new { Id = pago.MedioPagoId }, tx)
            ?? throw new CuentaCorrienteInvalidaException("El medio de pago no existe o está inactivo.");

        if (string.Equals(medio.NombreMedioPago, MedioCuentaCorriente, StringComparison.OrdinalIgnoreCase))
            throw new CuentaCorrienteInvalidaException(
                "No se puede pagar la cuenta corriente con cuenta corriente.");

        conexion.Execute(
            """
            INSERT INTO MovimientosCuentaCorriente
                (ClienteId, UsuarioId, Tipo, ImporteCentavos, MedioPagoId, Observaciones)
            VALUES (@ClienteId, @UsuarioId, 'PAGO', @Importe, @MedioPagoId, @Obs);
            """,
            new
            {
                pago.ClienteId,
                pago.UsuarioId,
                Importe = -importeCentavos,
                pago.MedioPagoId,
                Obs = Opcional(pago.Observaciones)
            }, tx);

        if (medio.AfectaEfectivo)
        {
            var sesion = CajaRepository.ObtenerSesionAbierta(conexion, tx)
                ?? throw new CuentaCorrienteInvalidaException(
                    "No hay una caja abierta. Abrí la caja para cobrar en efectivo.");

            conexion.Execute(
                """
                INSERT INTO MovimientosCaja
                    (SesionCajaId, UsuarioId, TipoMovimiento, MedioPagoId, ImporteCentavos, Motivo, OrigenTipo)
                VALUES (@SesionId, @UsuarioId, 'INGRESO', @MedioPagoId, @Importe, @Motivo, 'MANUAL');
                """,
                new
                {
                    SesionId = sesion.SesionCajaId,
                    pago.UsuarioId,
                    pago.MedioPagoId,
                    Importe = importeCentavos,
                    Motivo = $"Pago de cuenta corriente del cliente #{pago.ClienteId}"
                }, tx);
        }

        Auditoria.Registrar(conexion, tx, pago.UsuarioId, "CuentaCorriente", "ALTA", pago.ClienteId,
            $"Pago de {pago.Importe:C2} en {medio.NombreMedioPago}");

        tx.Commit();
    }

    /// <summary>
    /// Corrige la cuenta a mano: un saldo que venía de antes del sistema, una
    /// deuda que se perdona. Queda auditado porque es plata que se mueve sin que
    /// haya habido ni una venta ni un pago.
    /// </summary>
    public void Ajustar(AjustarCuentaDto ajuste)
    {
        if (ajuste.Importe == 0)
            throw new CuentaCorrienteInvalidaException("El ajuste no puede ser cero.");

        if (string.IsNullOrWhiteSpace(ajuste.Motivo))
            throw new CuentaCorrienteInvalidaException("Indicá el motivo del ajuste.");

        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        conexion.Execute(
            """
            INSERT INTO MovimientosCuentaCorriente
                (ClienteId, UsuarioId, Tipo, ImporteCentavos, Observaciones)
            VALUES (@ClienteId, @UsuarioId, 'AJUSTE', @Importe, @Motivo);
            """,
            new
            {
                ajuste.ClienteId,
                ajuste.UsuarioId,
                Importe = Money.ToCentavos(ajuste.Importe),
                ajuste.Motivo
            }, tx);

        Auditoria.Registrar(conexion, tx, ajuste.UsuarioId, "CuentaCorriente", "MODIFICACION", ajuste.ClienteId,
            $"Ajuste de {ajuste.Importe:C2}. Motivo: {ajuste.Motivo}");

        tx.Commit();
    }

    private static string? Opcional(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    private sealed class FilaDeuda
    {
        public int ClienteId { get; init; }
        public long ImporteCentavos { get; init; }
    }

    private sealed class FilaMovimiento
    {
        public int MovimientoCtaCteId { get; init; }
        public int ClienteId { get; init; }
        public DateTime Fecha { get; init; }
        public string Tipo { get; init; } = string.Empty;
        public long ImporteCentavos { get; init; }
        public int? VentaId { get; init; }
        public string? MedioPago { get; init; }
        public string Usuario { get; init; } = string.Empty;
        public string? Observaciones { get; init; }
    }

    private sealed class FilaSaldo
    {
        public int ClienteId { get; init; }
        public string Cliente { get; init; } = string.Empty;
        public string? Telefono { get; init; }
        public long SaldoCentavos { get; init; }
        public DateTime? UltimoMovimiento { get; init; }
    }

    private sealed class FilaMedio
    {
        public int MedioPagoId { get; init; }
        public string NombreMedioPago { get; init; } = string.Empty;
        public bool AfectaEfectivo { get; init; }
    }
}

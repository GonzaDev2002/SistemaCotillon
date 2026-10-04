using System.Data;
using Cotillon.Data;
using Cotillon.Shared.DTOs;
using Dapper;

namespace Cotillon.LocalApi.Service.Repositories;

public class ComboInvalidoException(string mensaje) : Exception(mensaje);

/// <summary>
/// La receta de los combos: qué lleva la bolsita sorpresa y cuánto de cada cosa.
///
/// Un combo no tiene stock propio. No se "arma" en el sistema: se arma en el
/// mostrador, y al vender uno se descuentan sus componentes. Por eso lo que se
/// informa como disponible es cuántos se pueden armar con lo que hay.
/// </summary>
public class CombosRepository
{
    private readonly IDbConnectionFactory _factory;

    public CombosRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public List<ComponenteComboDto> Componentes(int comboProductoId)
    {
        using var conexion = _factory.Crear();

        return Componentes(conexion, comboProductoId);
    }

    private static List<ComponenteComboDto> Componentes(IDbConnection conexion, int comboProductoId)
    {
        var filas = conexion.Query<FilaComponente>(
            """
            SELECT cc.ComponenteProductoId, cc.Cantidad,
                   p.NombreProducto, p.UnidadMedida, p.CostoCentavos, p.Stock
            FROM ComboComponentes cc
            JOIN Productos p ON cc.ComponenteProductoId = p.ProductoId
            WHERE cc.ComboProductoId = @Id
            ORDER BY p.NombreProducto;
            """,
            new { Id = comboProductoId });

        return filas.Select(f => new ComponenteComboDto
        {
            ComponenteProductoId = f.ComponenteProductoId,
            NombreProducto = f.NombreProducto,
            Cantidad = (decimal)f.Cantidad,
            UnidadMedida = f.UnidadMedida,
            Costo = Money.FromCentavos(f.CostoCentavos),
            StockComponente = (decimal)f.Stock
        }).ToList();
    }

    /// <summary>
    /// Reemplaza la receta completa. Se valida que ningún componente sea a su vez
    /// un combo: anidarlos haría que un cambio en el de adentro mueva el stock de
    /// algo que nadie tocó, y nadie podría explicar de dónde salió el descuento.
    /// </summary>
    public void Guardar(int comboProductoId, IEnumerable<ComponenteComboDto> componentes, int usuarioId)
    {
        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        var esCombo = conexion.ExecuteScalar<int?>(
            "SELECT EsCombo FROM Productos WHERE ProductoId = @Id;", new { Id = comboProductoId }, tx)
            ?? throw new ComboInvalidoException("No se encontró el producto.");

        if (esCombo == 0)
            throw new ComboInvalidoException("El producto no está marcado como combo.");

        var lista = componentes
            .Where(c => c.ComponenteProductoId > 0 && c.Cantidad > 0)
            .GroupBy(c => c.ComponenteProductoId)
            // Si el mismo producto viene dos veces se suman: es lo que el usuario
            // quiso decir, y la clave primaria rechazaría la segunda fila.
            .Select(g => new { Id = g.Key, Cantidad = g.Sum(c => c.Cantidad) })
            .ToList();

        if (lista.Count == 0)
            throw new ComboInvalidoException("El combo tiene que llevar al menos un producto.");

        if (lista.Any(c => c.Id == comboProductoId))
            throw new ComboInvalidoException("Un combo no puede contenerse a sí mismo.");

        var anidados = conexion.Query<string>(
            """
            SELECT NombreProducto FROM Productos
            WHERE EsCombo = 1 AND ProductoId IN @Ids;
            """,
            new { Ids = lista.Select(c => c.Id).ToArray() }, tx).ToList();

        if (anidados.Count > 0)
            throw new ComboInvalidoException(
                $"Un combo no puede llevar otro combo adentro: {string.Join(", ", anidados)}.");

        conexion.Execute(
            "DELETE FROM ComboComponentes WHERE ComboProductoId = @Id;",
            new { Id = comboProductoId }, tx);

        conexion.Execute(
            """
            INSERT INTO ComboComponentes (ComboProductoId, ComponenteProductoId, Cantidad)
            VALUES (@Combo, @Componente, @Cantidad);
            """,
            lista.Select(c => new { Combo = comboProductoId, Componente = c.Id, Cantidad = (double)c.Cantidad }),
            tx);

        Auditoria.Registrar(conexion, tx, usuarioId, "Productos", "MODIFICACION", comboProductoId,
            $"Receta del combo actualizada: {lista.Count} componente(s)");

        tx.Commit();
    }

    private sealed class FilaComponente
    {
        public int ComponenteProductoId { get; init; }
        public double Cantidad { get; init; }
        public string NombreProducto { get; init; } = string.Empty;
        public string UnidadMedida { get; init; } = "UNIDAD";
        public long CostoCentavos { get; init; }
        public double Stock { get; init; }
    }
}

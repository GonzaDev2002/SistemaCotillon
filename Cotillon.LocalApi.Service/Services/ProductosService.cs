using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs;
using Cotillon.Shared.Responses;

namespace Cotillon.LocalApi.Service.Services;

public class ProductosService
{
    private readonly ProductosRepository _repository;

    public ProductosService(ProductosRepository repository)
    {
        _repository = repository;
    }

    public ApiResponse<string> Crear(ProductoDto producto)
    {
        if (producto.CategoriaId <= 0)
            return ApiResponse<string>.Fail("La categoría es obligatoria.");

        if (string.IsNullOrWhiteSpace(producto.NombreProducto))
            return ApiResponse<string>.Fail("El nombre del producto es obligatorio.");

        if (producto.Precio < 0)
            return ApiResponse<string>.Fail("El precio no puede ser negativo.");

        if (producto.Stock < 0)
            return ApiResponse<string>.Fail("El stock no puede ser negativo.");

        var ok = _repository.Insertar(producto);

        if (!ok)
            return ApiResponse<string>.Fail("No se pudo guardar el producto.");

        return ApiResponse<string>.Ok("OK", "Producto agregado con éxito.");
    }

    public ApiResponse<List<ProductoListadoDto>> ObtenerTodos(bool incluirInactivos)
    {
        var lista = _repository.ObtenerTodos(incluirInactivos);
        return ApiResponse<List<ProductoListadoDto>>.Ok(lista, "Productos obtenidos correctamente.");
    }

    public ApiResponse<List<ProductoListadoDto>> Buscar(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return ApiResponse<List<ProductoListadoDto>>.Ok([], "Ingresá un texto para buscar.");

        return ApiResponse<List<ProductoListadoDto>>.Ok(
            _repository.Buscar(texto.Trim(), 50), "Productos encontrados.");
    }

    public ApiResponse<ProductoDto> ObtenerPorId(int productoId)
    {
        var producto = _repository.ObtenerPorId(productoId);

        return producto is null
            ? ApiResponse<ProductoDto>.Fail("No se encontró el producto.")
            : ApiResponse<ProductoDto>.Ok(producto, "Producto obtenido correctamente.");
    }

    /// <summary>
    /// Busca por código exacto para la carga con lector. No encontrarlo no es un
    /// error: significa que el código es nuevo y hay que dar de alta el producto,
    /// así que responde Ok con Data en null y la pantalla decide qué hacer.
    /// </summary>
    public ApiResponse<ProductoDto> ObtenerPorCodigo(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return ApiResponse<ProductoDto>.Fail("Escaneá o escribí un código.");

        var producto = _repository.ObtenerPorCodigo(codigo.Trim());

        return producto is null
            ? ApiResponse<ProductoDto>.Ok(null, "El código no está cargado todavía.")
            : ApiResponse<ProductoDto>.Ok(producto, "Producto encontrado.");
    }

    public ApiResponse<string> Actualizar(ProductoDto producto)
    {
        if (producto.ProductoId <= 0)
            return ApiResponse<string>.Fail("El producto es inválido.");

        if (producto.CategoriaId <= 0)
            return ApiResponse<string>.Fail("La categoría es obligatoria.");

        if (string.IsNullOrWhiteSpace(producto.NombreProducto))
            return ApiResponse<string>.Fail("El nombre del producto es obligatorio.");

        if (producto.Precio < 0)
            return ApiResponse<string>.Fail("El precio no puede ser negativo.");

        return _repository.Actualizar(producto)
            ? ApiResponse<string>.Ok("OK", "Producto actualizado con éxito.")
            : ApiResponse<string>.Fail("No se pudo actualizar el producto.");
    }

    public ApiResponse<string> Eliminar(int productoId)
    {
        if (productoId <= 0)
            return ApiResponse<string>.Fail("El producto es inválido.");

        var ok = _repository.EliminarLogico(productoId);

        if (!ok)
            return ApiResponse<string>.Fail("No se pudo eliminar el producto.");

        return ApiResponse<string>.Ok("OK", "Producto eliminado con éxito.");
    }

    public ApiResponse<string> Reactivar(int productoId)
    {
        if (productoId <= 0)
            return ApiResponse<string>.Fail("El producto es inválido.");

        return _repository.Reactivar(productoId)
            ? ApiResponse<string>.Ok("OK", "Producto puesto nuevamente en venta.")
            : ApiResponse<string>.Fail("No se encontró el producto.");
    }
    public ApiResponse<DashboardResumenDto> ObtenerResumenDashboard()
    {
        var resumen = _repository.ObtenerResumenDashboard();
        return ApiResponse<DashboardResumenDto>.Ok(resumen, "Resumen obtenido correctamente.");
    }

    public ApiResponse<List<ProductoDashboardDto>> ObtenerProductosBajoStock()
    {
        var lista = _repository.ObtenerProductosBajoStock();
        return ApiResponse<List<ProductoDashboardDto>>.Ok(lista, "Productos bajo stock obtenidos correctamente.");
    }

    public ApiResponse<List<ProductoDashboardDto>> ObtenerUltimosProductos()
    {
        var lista = _repository.ObtenerUltimosProductos();
        return ApiResponse<List<ProductoDashboardDto>>.Ok(lista, "Últimos productos obtenidos correctamente.");
    }
}
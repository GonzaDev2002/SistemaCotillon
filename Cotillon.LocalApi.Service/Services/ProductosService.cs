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

    public ApiResponse<string> Eliminar(int productoId)
    {
        if (productoId <= 0)
            return ApiResponse<string>.Fail("El producto es inválido.");

        var ok = _repository.EliminarLogico(productoId);

        if (!ok)
            return ApiResponse<string>.Fail("No se pudo eliminar el producto.");

        return ApiResponse<string>.Ok("OK", "Producto eliminado con éxito.");
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
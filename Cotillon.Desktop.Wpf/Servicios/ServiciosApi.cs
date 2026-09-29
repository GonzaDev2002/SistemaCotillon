using Cotillon.Shared.DTOs;
using Cotillon.Shared.DTOs.Auditoria;
using Cotillon.Shared.DTOs.Caja;
using Cotillon.Shared.DTOs.Compras;
using Cotillon.Shared.DTOs.Reportes;
using Cotillon.Shared.DTOs.Stock;
using Cotillon.Shared.DTOs.Terceros;
using Cotillon.Shared.DTOs.Usuarios;
using Cotillon.Shared.DTOs.Ventas;
using Cotillon.Shared.Responses;

namespace Cotillon.Desktop.Wpf.Servicios;

/// <summary>
/// Punto único de acceso a la API. Las pantallas lo usan a través de App.Api.
/// </summary>
public class ServiciosApi
{
    private readonly ApiCliente _cliente;

    public ServiciosApi(ApiCliente cliente)
    {
        _cliente = cliente;
    }

    public ApiCliente Cliente => _cliente;

    private static string Fecha(DateTime fecha) => fecha.ToString("yyyy-MM-dd");

    // ------------------------------------------------------------------- Auth --

    public Task<ApiResponse<LoginResponseDto>> LoginAsync(string usuario, string pin) =>
        _cliente.PublicarAsync<LoginResponseDto>("api/Auth/login",
            new LoginRequestDto { Username = usuario, Pin = pin });

    // -------------------------------------------------------------- Dashboard --

    public Task<ApiResponse<DashboardResumenDto>> ResumenDashboardAsync() =>
        _cliente.ObtenerAsync<DashboardResumenDto>("api/Dashboard/resumen");

    public Task<ApiResponse<List<ProductoDashboardDto>>> BajoStockAsync() =>
        _cliente.ObtenerAsync<List<ProductoDashboardDto>>("api/Dashboard/bajo-stock");

    public Task<ApiResponse<List<ProductoDashboardDto>>> UltimosProductosAsync() =>
        _cliente.ObtenerAsync<List<ProductoDashboardDto>>("api/Dashboard/ultimos-productos");

    // --------------------------------------------------------------- Catálogo --

    public Task<ApiResponse<List<SeccionDto>>> SeccionesAsync() =>
        _cliente.ObtenerAsync<List<SeccionDto>>("api/Secciones");

    public Task<ApiResponse<string>> CrearSeccionAsync(SeccionDto seccion) =>
        _cliente.PublicarAsync<string>("api/Secciones", seccion);

    public Task<ApiResponse<string>> ActualizarSeccionAsync(SeccionDto seccion) =>
        _cliente.ActualizarAsync<string>("api/Secciones", seccion);

    public Task<ApiResponse<List<CategoriaDto>>> CategoriasAsync() =>
        _cliente.ObtenerAsync<List<CategoriaDto>>("api/Categorias");

    public Task<ApiResponse<List<CategoriaDto>>> CategoriasPorSeccionAsync(int seccionId) =>
        _cliente.ObtenerAsync<List<CategoriaDto>>($"api/Categorias/por-seccion/{seccionId}");

    public Task<ApiResponse<string>> CrearCategoriaAsync(CategoriaDto categoria) =>
        _cliente.PublicarAsync<string>("api/Categorias", categoria);

    public Task<ApiResponse<string>> ActualizarCategoriaAsync(CategoriaDto categoria) =>
        _cliente.ActualizarAsync<string>("api/Categorias", categoria);

    // -------------------------------------------------------------- Productos --

    public Task<ApiResponse<List<ProductoListadoDto>>> ProductosAsync(bool incluirInactivos) =>
        _cliente.ObtenerAsync<List<ProductoListadoDto>>(
            $"api/Productos/todos?incluirInactivos={incluirInactivos.ToString().ToLowerInvariant()}");

    public Task<ApiResponse<List<ProductoListadoDto>>> BuscarProductosAsync(string texto) =>
        _cliente.ObtenerAsync<List<ProductoListadoDto>>($"api/Productos/buscar?texto={Uri.EscapeDataString(texto)}");

    public Task<ApiResponse<ProductoDto>> ProductoAsync(int productoId) =>
        _cliente.ObtenerAsync<ProductoDto>($"api/Productos/{productoId}");

    public Task<ApiResponse<string>> CrearProductoAsync(ProductoDto producto) =>
        _cliente.PublicarAsync<string>("api/Productos", producto);

    public Task<ApiResponse<string>> ActualizarProductoAsync(ProductoDto producto) =>
        _cliente.ActualizarAsync<string>("api/Productos", producto);

    public Task<ApiResponse<string>> EliminarProductoAsync(int productoId) =>
        _cliente.EliminarAsync<string>($"api/Productos/{productoId}");

    // ------------------------------------------------------------------- Caja --

    public Task<ApiResponse<SesionCajaDto>> CajaActualAsync() =>
        _cliente.ObtenerAsync<SesionCajaDto>("api/Caja/actual");

    public Task<ApiResponse<SesionCajaDto>> AbrirCajaAsync(AbrirCajaDto datos) =>
        _cliente.PublicarAsync<SesionCajaDto>("api/Caja/abrir", datos);

    public Task<ApiResponse<ArqueoCajaDto>> CerrarCajaAsync(CerrarCajaDto datos) =>
        _cliente.PublicarAsync<ArqueoCajaDto>("api/Caja/cerrar", datos);

    public Task<ApiResponse<ArqueoCajaDto>> ArqueoAsync() =>
        _cliente.ObtenerAsync<ArqueoCajaDto>("api/Caja/arqueo");

    public Task<ApiResponse<string>> MovimientoCajaAsync(RegistrarMovimientoCajaDto datos) =>
        _cliente.PublicarAsync<string>("api/Caja/movimientos", datos);

    public Task<ApiResponse<List<MovimientoCajaDto>>> MovimientosCajaAsync(int sesionCajaId) =>
        _cliente.ObtenerAsync<List<MovimientoCajaDto>>($"api/Caja/movimientos/{sesionCajaId}");

    public Task<ApiResponse<List<SesionCajaDto>>> HistorialCajaAsync(DateTime desde, DateTime hasta) =>
        _cliente.ObtenerAsync<List<SesionCajaDto>>(
            $"api/Caja/historial?desde={Fecha(desde)}&hasta={Fecha(hasta)}");

    // ----------------------------------------------------------------- Ventas --

    public Task<ApiResponse<VentaDto>> CrearVentaAsync(CrearVentaDto venta) =>
        _cliente.PublicarAsync<VentaDto>("api/Ventas", venta);

    public Task<ApiResponse<VentaDto>> VentaAsync(int ventaId) =>
        _cliente.ObtenerAsync<VentaDto>($"api/Ventas/{ventaId}");

    public Task<ApiResponse<List<VentaResumenDto>>> VentasAsync(DateTime desde, DateTime hasta, bool incluirAnuladas) =>
        _cliente.ObtenerAsync<List<VentaResumenDto>>(
            $"api/Ventas?desde={Fecha(desde)}&hasta={Fecha(hasta)}&incluirAnuladas={incluirAnuladas.ToString().ToLowerInvariant()}");

    public Task<ApiResponse<string>> AnularVentaAsync(int ventaId, AnularVentaDto datos) =>
        _cliente.PublicarAsync<string>($"api/Ventas/{ventaId}/anular", datos);

    // --------------------------------------------------------------- Terceros --

    public Task<ApiResponse<List<ClienteDto>>> ClientesAsync(string? texto, bool incluirInactivos) =>
        _cliente.ObtenerAsync<List<ClienteDto>>(
            $"api/Clientes?texto={Uri.EscapeDataString(texto ?? string.Empty)}&incluirInactivos={incluirInactivos.ToString().ToLowerInvariant()}");

    public Task<ApiResponse<ClienteDto>> CrearClienteAsync(ClienteDto cliente) =>
        _cliente.PublicarAsync<ClienteDto>($"api/Clientes?usuarioId={Sesion.UsuarioId}", cliente);

    public Task<ApiResponse<ClienteDto>> ActualizarClienteAsync(ClienteDto cliente) =>
        _cliente.ActualizarAsync<ClienteDto>($"api/Clientes?usuarioId={Sesion.UsuarioId}", cliente);

    public Task<ApiResponse<string>> DesactivarClienteAsync(int clienteId) =>
        _cliente.EliminarAsync<string>($"api/Clientes/{clienteId}?usuarioId={Sesion.UsuarioId}");

    public Task<ApiResponse<List<ProveedorDto>>> ProveedoresAsync(string? texto, bool incluirInactivos) =>
        _cliente.ObtenerAsync<List<ProveedorDto>>(
            $"api/Proveedores?texto={Uri.EscapeDataString(texto ?? string.Empty)}&incluirInactivos={incluirInactivos.ToString().ToLowerInvariant()}");

    public Task<ApiResponse<ProveedorDto>> CrearProveedorAsync(ProveedorDto proveedor) =>
        _cliente.PublicarAsync<ProveedorDto>($"api/Proveedores?usuarioId={Sesion.UsuarioId}", proveedor);

    public Task<ApiResponse<ProveedorDto>> ActualizarProveedorAsync(ProveedorDto proveedor) =>
        _cliente.ActualizarAsync<ProveedorDto>($"api/Proveedores?usuarioId={Sesion.UsuarioId}", proveedor);

    public Task<ApiResponse<string>> DesactivarProveedorAsync(int proveedorId) =>
        _cliente.EliminarAsync<string>($"api/Proveedores/{proveedorId}?usuarioId={Sesion.UsuarioId}");

    // ---------------------------------------------------------------- Compras --

    public Task<ApiResponse<CompraDto>> CrearCompraAsync(CrearCompraDto compra) =>
        _cliente.PublicarAsync<CompraDto>("api/Compras", compra);

    public Task<ApiResponse<CompraDto>> CompraAsync(int compraId) =>
        _cliente.ObtenerAsync<CompraDto>($"api/Compras/{compraId}");

    public Task<ApiResponse<List<CompraResumenDto>>> ComprasAsync(DateTime desde, DateTime hasta) =>
        _cliente.ObtenerAsync<List<CompraResumenDto>>($"api/Compras?desde={Fecha(desde)}&hasta={Fecha(hasta)}");

    public Task<ApiResponse<CompraDto>> RecibirCompraAsync(int compraId, RecibirCompraDto datos) =>
        _cliente.PublicarAsync<CompraDto>($"api/Compras/{compraId}/recibir", datos);

    public Task<ApiResponse<string>> AnularCompraAsync(int compraId, string motivo) =>
        _cliente.PublicarAsync<string>(
            $"api/Compras/{compraId}/anular?usuarioId={Sesion.UsuarioId}&motivo={Uri.EscapeDataString(motivo)}");

    // ------------------------------------------------------------------ Stock --

    public Task<ApiResponse<string>> AjustarStockAsync(AjustarStockDto ajuste) =>
        _cliente.PublicarAsync<string>("api/Stock/ajustar", ajuste);

    public Task<ApiResponse<List<MovimientoStockDto>>> MovimientosStockAsync(
        DateTime desde, DateTime hasta, int? productoId) =>
        _cliente.ObtenerAsync<List<MovimientoStockDto>>(
            $"api/Stock/movimientos?desde={Fecha(desde)}&hasta={Fecha(hasta)}" +
            (productoId.HasValue ? $"&productoId={productoId}" : string.Empty));

    // --------------------------------------------------------------- Reportes --

    public Task<ApiResponse<List<ReporteProductoDto>>> ReporteProductosAsync() =>
        _cliente.ObtenerAsync<List<ReporteProductoDto>>("api/Reportes/productos");

    public Task<ApiResponse<List<ReporteProductoDto>>> ReporteBajoStockAsync() =>
        _cliente.ObtenerAsync<List<ReporteProductoDto>>("api/Reportes/productos-bajo-stock");

    public Task<ApiResponse<List<ReporteVentaDto>>> ReporteVentasAsync(DateTime desde, DateTime hasta) =>
        _cliente.ObtenerAsync<List<ReporteVentaDto>>(
            $"api/Reportes/ventas?desde={Fecha(desde)}&hasta={Fecha(hasta)}");

    // -------------------------------------------------------------- Auditoría --

    public Task<ApiResponse<List<EventoAuditoriaDto>>> AuditoriaAsync(DateTime desde, DateTime hasta) =>
        _cliente.ObtenerAsync<List<EventoAuditoriaDto>>(
            $"api/Auditoria?desde={Fecha(desde)}&hasta={Fecha(hasta)}&limite=500");

    // --------------------------------------------------- Usuarios y catálogos --

    public Task<ApiResponse<List<UsuarioDto>>> UsuariosAsync(bool incluirInactivos) =>
        _cliente.ObtenerAsync<List<UsuarioDto>>(
            $"api/Usuarios?incluirInactivos={incluirInactivos.ToString().ToLowerInvariant()}");

    public Task<ApiResponse<UsuarioDto>> CrearUsuarioAsync(CrearUsuarioDto usuario) =>
        _cliente.PublicarAsync<UsuarioDto>($"api/Usuarios?usuarioEjecutor={Sesion.UsuarioId}", usuario);

    public Task<ApiResponse<UsuarioDto>> ActualizarUsuarioAsync(UsuarioDto usuario) =>
        _cliente.ActualizarAsync<UsuarioDto>($"api/Usuarios?usuarioEjecutor={Sesion.UsuarioId}", usuario);

    public Task<ApiResponse<string>> CambiarPinAsync(CambiarPinDto datos, bool comoAdmin) =>
        _cliente.PublicarAsync<string>(
            $"api/Usuarios/cambiar-pin?esAdmin={comoAdmin.ToString().ToLowerInvariant()}", datos);

    public Task<ApiResponse<List<MedioPagoDto>>> MediosPagoAsync() =>
        _cliente.ObtenerAsync<List<MedioPagoDto>>("api/MediosPago");

    public Task<ApiResponse<Dictionary<string, string>>> ConfiguracionAsync() =>
        _cliente.ObtenerAsync<Dictionary<string, string>>("api/Configuracion");

    public Task<ApiResponse<string>> GuardarConfiguracionAsync(string clave, string? valor) =>
        _cliente.ActualizarAsync<string>($"api/Configuracion?usuarioId={Sesion.UsuarioId}",
            new ConfiguracionItemDto { Clave = clave, Valor = valor });
}

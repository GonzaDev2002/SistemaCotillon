using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs.Terceros;
using Cotillon.Shared.Responses;

namespace Cotillon.LocalApi.Service.Services;

public class ClientesService
{
    private readonly ClientesRepository _repository;

    public ClientesService(ClientesRepository repository)
    {
        _repository = repository;
    }

    public ApiResponse<List<ClienteDto>> Buscar(string? texto, bool incluirInactivos) =>
        ApiResponse<List<ClienteDto>>.Ok(
            _repository.Buscar(texto, incluirInactivos), "Clientes obtenidos correctamente.");

    public ApiResponse<ClienteDto> ObtenerPorId(int clienteId)
    {
        var cliente = _repository.ObtenerPorId(clienteId);

        return cliente is null
            ? ApiResponse<ClienteDto>.Fail("No se encontró el cliente.")
            : ApiResponse<ClienteDto>.Ok(cliente, "Cliente obtenido correctamente.");
    }

    public ApiResponse<ClienteDto> Crear(ClienteDto cliente, int usuarioId)
    {
        var validacion = Validar(cliente, usuarioId);

        if (validacion is not null)
            return validacion;

        var id = _repository.Insertar(cliente, usuarioId);
        var creado = _repository.ObtenerPorId(id);

        return creado is null
            ? ApiResponse<ClienteDto>.Fail("No se pudo guardar el cliente.")
            : ApiResponse<ClienteDto>.Ok(creado, "Cliente agregado con éxito.");
    }

    public ApiResponse<ClienteDto> Actualizar(ClienteDto cliente, int usuarioId)
    {
        if (cliente.ClienteId <= 0)
            return ApiResponse<ClienteDto>.Fail("El cliente es inválido.");

        var validacion = Validar(cliente, usuarioId);

        if (validacion is not null)
            return validacion;

        if (!_repository.Actualizar(cliente, usuarioId))
            return ApiResponse<ClienteDto>.Fail("No se pudo actualizar el cliente.");

        var actualizado = _repository.ObtenerPorId(cliente.ClienteId);

        return ApiResponse<ClienteDto>.Ok(actualizado, "Cliente actualizado con éxito.");
    }

    public ApiResponse<string> Desactivar(int clienteId, int usuarioId)
    {
        if (clienteId <= 0)
            return ApiResponse<string>.Fail("El cliente es inválido.");

        return _repository.Desactivar(clienteId, usuarioId)
            ? ApiResponse<string>.Ok("OK", "Cliente dado de baja con éxito.")
            : ApiResponse<string>.Fail("No se pudo dar de baja el cliente.");
    }

    private ApiResponse<ClienteDto>? Validar(ClienteDto cliente, int usuarioId)
    {
        if (usuarioId <= 0)
            return ApiResponse<ClienteDto>.Fail("El usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(cliente.NombreCompleto))
            return ApiResponse<ClienteDto>.Fail("El nombre del cliente es obligatorio.");

        if (!string.IsNullOrWhiteSpace(cliente.Documento) &&
            _repository.ExisteDocumento(cliente.Documento.Trim(), cliente.ClienteId))
            return ApiResponse<ClienteDto>.Fail($"Ya hay un cliente con el documento {cliente.Documento.Trim()}.");

        return null;
    }
}

public class ProveedoresService
{
    private readonly ProveedoresRepository _repository;

    public ProveedoresService(ProveedoresRepository repository)
    {
        _repository = repository;
    }

    public ApiResponse<List<ProveedorDto>> Buscar(string? texto, bool incluirInactivos) =>
        ApiResponse<List<ProveedorDto>>.Ok(
            _repository.Buscar(texto, incluirInactivos), "Proveedores obtenidos correctamente.");

    public ApiResponse<ProveedorDto> ObtenerPorId(int proveedorId)
    {
        var proveedor = _repository.ObtenerPorId(proveedorId);

        return proveedor is null
            ? ApiResponse<ProveedorDto>.Fail("No se encontró el proveedor.")
            : ApiResponse<ProveedorDto>.Ok(proveedor, "Proveedor obtenido correctamente.");
    }

    public ApiResponse<ProveedorDto> Crear(ProveedorDto proveedor, int usuarioId)
    {
        if (usuarioId <= 0)
            return ApiResponse<ProveedorDto>.Fail("El usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(proveedor.NombreProveedor))
            return ApiResponse<ProveedorDto>.Fail("El nombre del proveedor es obligatorio.");

        var id = _repository.Insertar(proveedor, usuarioId);
        var creado = _repository.ObtenerPorId(id);

        return creado is null
            ? ApiResponse<ProveedorDto>.Fail("No se pudo guardar el proveedor.")
            : ApiResponse<ProveedorDto>.Ok(creado, "Proveedor agregado con éxito.");
    }

    public ApiResponse<ProveedorDto> Actualizar(ProveedorDto proveedor, int usuarioId)
    {
        if (proveedor.ProveedorId <= 0)
            return ApiResponse<ProveedorDto>.Fail("El proveedor es inválido.");

        if (usuarioId <= 0)
            return ApiResponse<ProveedorDto>.Fail("El usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(proveedor.NombreProveedor))
            return ApiResponse<ProveedorDto>.Fail("El nombre del proveedor es obligatorio.");

        if (!_repository.Actualizar(proveedor, usuarioId))
            return ApiResponse<ProveedorDto>.Fail("No se pudo actualizar el proveedor.");

        return ApiResponse<ProveedorDto>.Ok(
            _repository.ObtenerPorId(proveedor.ProveedorId), "Proveedor actualizado con éxito.");
    }

    public ApiResponse<string> Desactivar(int proveedorId, int usuarioId)
    {
        if (proveedorId <= 0)
            return ApiResponse<string>.Fail("El proveedor es inválido.");

        return _repository.Desactivar(proveedorId, usuarioId)
            ? ApiResponse<string>.Ok("OK", "Proveedor dado de baja con éxito.")
            : ApiResponse<string>.Fail("No se pudo dar de baja el proveedor.");
    }
}

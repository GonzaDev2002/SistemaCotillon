using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs;
using Cotillon.Shared.Responses;

namespace Cotillon.LocalApi.Service.Services;

public class AuthService
{
    private readonly AuthRepository _authRepository;

    public AuthService(AuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    public ApiResponse<LoginResponseDto> Login(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return ApiResponse<LoginResponseDto>.Fail("El usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Pin))
            return ApiResponse<LoginResponseDto>.Fail("El PIN es obligatorio.");

        var user = _authRepository.Login(request.Username.Trim(), request.Pin.Trim());

        if (user is null)
            return ApiResponse<LoginResponseDto>.Fail("Usuario o PIN incorrectos.");

        return ApiResponse<LoginResponseDto>.Ok(user, "Login correcto.");
    }
}
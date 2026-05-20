using Cotillon.LocalApi.Service.Services;
using Cotillon.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Cotillon.LocalApi.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequestDto request)
    {
        var result = _authService.Login(request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
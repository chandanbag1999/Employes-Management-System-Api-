using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Identity.DTOs;
using EMS.Application.Modules.Identity.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace EMS.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST api/v1/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Invalid request data."));

        var result = await _authService.RegisterAsync(dto);

        if (result == null)
            return Conflict(ApiResponse<string>.Fail("Email already registered."));

        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Registration successful."));
    }

    // POST api/v1/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Invalid request data."));

        var result = await _authService.LoginAsync(dto);

        if (result == null)
            return Unauthorized(ApiResponse<string>.Fail("Invalid email or password."));

        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login successful."));
    }
}
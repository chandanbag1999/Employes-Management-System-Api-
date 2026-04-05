using EMS.Application.Common.DTOs;
using EMS.Application.Common.Interfaces;
using EMS.Application.Modules.Identity.DTOs;
using EMS.Application.Modules.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        var ip = GetIpAddress();
        var result = await _authService.LoginAsync(dto, ip);

        if (result == null)
            return Unauthorized(ApiResponse<string>.Fail("Invalid credentials or account locked."));

        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login successful."));
    }

    // POST api/v1/auth/refresh
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Invalid request data."));

        var ip = GetIpAddress();
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken, ip);

        if (result == null)
            return Unauthorized(ApiResponse<string>.Fail("Invalid or expired refresh token."));

        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Token refreshed."));
    }

    // POST api/v1/auth/logout
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Invalid request data."));

        var result = await _authService.LogoutAsync(dto.RefreshToken);

        if (!result)
            return BadRequest(ApiResponse<string>.Fail("Invalid or already revoked token."));

        return Ok(ApiResponse<string>.Ok("Logged out.", "Logout successful."));
    }

    // ✅ NEW: POST api/v1/auth/change-password
    // Sirf authenticated user apna password change kar sakta hai
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Invalid request data."));

        // JWT se userId nikalo
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized(ApiResponse<string>.Fail("Invalid token."));

        var result = await _authService.ChangePasswordAsync(userId, dto);

        if (!result.success)
            return BadRequest(ApiResponse<string>.Fail(result.error ?? "Password change failed."));

        return Ok(ApiResponse<string>.Ok(
            "Password changed successfully.",
            "Please login again with your new password."));
    }

    // GET api/v1/auth/test-email?to=youremail@gmail.com
    [HttpGet("test-email")]
    [AllowAnonymous]
    public async Task<IActionResult> TestEmail(
        [FromQuery] string to,
        [FromServices] IEmailService emailService)
    {
        try
        {
            await emailService.SendWelcomeEmailAsync(
                toEmail: to,
                employeeName: "Test Employee",
                temporaryPassword: "Welcome@123",
                loginUrl: "http://localhost:8080"
            );
            return Ok(new { success = true, message = $"Email sent to {to}" });
        }
        catch (Exception ex)
        {
            return Ok(new
            {
                success = false,
                error = ex.Message,
                innerError = ex.InnerException?.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    // ── Private Helper ────────────────────────────────────────────────
    private string GetIpAddress()
    {
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
            return forwarded.ToString();

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
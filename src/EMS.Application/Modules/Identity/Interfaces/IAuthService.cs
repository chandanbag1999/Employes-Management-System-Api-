using EMS.Application.Modules.Identity.DTOs;

namespace EMS.Application.Modules.Identity.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto?> LoginAsync(LoginDto dto, string ipAddress);
    Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken, string ipAddress);
    Task<bool> LogoutAsync(string refreshToken);

    // ✅ NEW
    Task<(bool success, string? error)> ChangePasswordAsync(
        int userId,
        ChangePasswordDto dto);
}
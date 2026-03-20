using EMS.Application.Modules.Identity.DTOs;

namespace EMS.Application.Modules.Identity.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
}
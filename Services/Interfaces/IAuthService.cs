using EmployesManagementSystemApi.DTOs.Auth;

namespace EmployesManagementSystemApi.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    }
}

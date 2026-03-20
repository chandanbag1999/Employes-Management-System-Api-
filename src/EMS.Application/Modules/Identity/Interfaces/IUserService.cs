using EMS.Application.Modules.Identity.DTOs;

namespace EMS.Application.Modules.Identity.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserResponseDto>> GetAllAsync();
    Task<UserResponseDto?> GetByIdAsync(int id);
    Task<bool> DeactivateUserAsync(int id);
}
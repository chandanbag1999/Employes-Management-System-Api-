using EMS.Application.Modules.Identity.DTOs;
using EMS.Domain.Enums;

namespace EMS.Application.Modules.Identity.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserResponseDto>> GetAllAsync();
    Task<UserResponseDto?> GetByIdAsync(int id);
    Task<UserResponseDto?> GetCurrentUserAsync(int userId);
    Task<bool> DeactivateUserAsync(int id);
    Task<bool> ActivateUserAsync(int id);
    Task<bool> ChangeRoleAsync(int id, UserRole newRole);
}
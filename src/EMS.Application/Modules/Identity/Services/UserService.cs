using EMS.Application.Modules.Identity.DTOs;
using EMS.Application.Modules.Identity.Interfaces;
using EMS.Domain.Entities.Identity;

namespace EMS.Application.Modules.Identity.Services;

public class UserService : IUserService
{
    private readonly IAuthRepository _authRepository;

    public UserService(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllAsync()
    {
        var users = await _authRepository.GetAllAsync();
        return users.Select(MapToDto);
    }

    public async Task<UserResponseDto?> GetByIdAsync(int id)
    {
        var user = await _authRepository.GetByIdAsync(id);
        return user == null ? null : MapToDto(user);
    }

    public async Task<bool> DeactivateUserAsync(int id)
    {
        var user = await _authRepository.GetByIdAsync(id);
        if (user == null) return false;

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _authRepository.SaveChangesAsync();
        return true;
    }

    private static UserResponseDto MapToDto(AppUser u) => new()
    {
        Id = u.Id,
        UserName = u.UserName,
        Email = u.Email,
        Role = u.Role.ToString(),
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt,
        LastLogin = u.LastLogin
    };
}
using EMS.Application.Modules.Employees.Interfaces;
using EMS.Application.Modules.Identity.DTOs;
using EMS.Application.Modules.Identity.Interfaces;
using EMS.Domain.Enums;

namespace EMS.Application.Modules.Identity.Services;

public class UserService : IUserService
{
    private readonly IAuthRepository _authRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public UserService(
        IAuthRepository authRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IEmployeeRepository employeeRepository)
    {
        _authRepository = authRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllAsync()
    {
        var users = await _authRepository.GetAllAsync();
        return users.Select(MapToDto);
    }

    public async Task<UserResponseDto?> GetByIdAsync(int id)
    {
        var user = await _authRepository.GetByIdAsync(id);
        return user == null ? null : await MapToDtoWithEmployee(user);
    }

    public async Task<UserResponseDto?> GetCurrentUserAsync(int userId)
    {
        var user = await _authRepository.GetByIdAsync(userId);
        return user == null ? null : await MapToDtoWithEmployee(user);
    }

    public async Task<bool> DeactivateUserAsync(int id)
    {
        var user = await _authRepository.GetByIdAsync(id);
        if (user == null) return false;

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _refreshTokenRepository.RevokeAllUserTokensAsync(id);
        await _authRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ActivateUserAsync(int id)
    {
        var user = await _authRepository.GetByIdAsync(id);
        if (user == null) return false;

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _authRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ChangeRoleAsync(int id, UserRole newRole)
    {
        var user = await _authRepository.GetByIdAsync(id);
        if (user == null) return false;

        user.Role = newRole;
        user.UpdatedAt = DateTime.UtcNow;

        await _refreshTokenRepository.RevokeAllUserTokensAsync(id);
        await _authRepository.SaveChangesAsync();

        return true;
    }

    private static UserResponseDto MapToDto(EMS.Domain.Entities.Identity.AppUser u) => new()
    {
        Id = u.Id,
        UserName = u.UserName,
        Email = u.Email,
        Role = u.Role.ToString(),
        IsActive = u.IsActive,
        IsEmailVerified = u.IsEmailVerified,
        IsFirstLogin = u.IsFirstLogin,
        CreatedAt = u.CreatedAt,
        LastLogin = u.LastLogin,
        IsLockedOut = u.LockoutEnd.HasValue && u.LockoutEnd > DateTime.UtcNow
    };

    private async Task<UserResponseDto> MapToDtoWithEmployee(EMS.Domain.Entities.Identity.AppUser u)
    {
        var employeeId = await _employeeRepository.GetIdByUserIdAsync(u.Id);
        var dto = MapToDto(u);
        dto.EmployeeId = employeeId;
        return dto;
    }
}
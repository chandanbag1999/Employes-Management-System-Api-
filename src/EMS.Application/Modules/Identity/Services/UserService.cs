using EMS.Application.Modules.Identity.DTOs;
using EMS.Application.Modules.Identity.Interfaces;
using EMS.Domain.Enums;

namespace EMS.Application.Modules.Identity.Services;

public class UserService : IUserService
{
    private readonly IAuthRepository _authRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public UserService(
        IAuthRepository authRepository,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _authRepository = authRepository;
        _refreshTokenRepository = refreshTokenRepository;
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

    public async Task<UserResponseDto?> GetCurrentUserAsync(int userId)
    {
        var user = await _authRepository.GetByIdAsync(userId);
        return user == null ? null : MapToDto(user);
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
        CreatedAt = u.CreatedAt,
        LastLogin = u.LastLogin,
        IsLockedOut = u.LockoutEnd.HasValue && u.LockoutEnd > DateTime.UtcNow
    };
}
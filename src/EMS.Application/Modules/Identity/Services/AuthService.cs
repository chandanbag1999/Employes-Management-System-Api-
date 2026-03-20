using EMS.Application.Common.Interfaces;
using EMS.Application.Modules.Identity.DTOs;
using EMS.Application.Modules.Identity.Interfaces;
using EMS.Domain.Entities.Identity;
using EMS.Domain.Enums;

namespace EMS.Application.Modules.Identity.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IJwtService _jwtService;

    public AuthService(IAuthRepository authRepository, IJwtService jwtService)
    {
        _authRepository = authRepository;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
    {
        // Email already exists?
        if (await _authRepository.EmailExistsAsync(dto.Email))
            return null;

        // Role validate karo
        if (!Enum.TryParse<UserRole>(dto.Role, true, out var role))
            role = UserRole.Employee;

        // Password hash karo — plain text KABHI nahi
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = new AppUser
        {
            UserName = dto.UserName,
            Email = dto.Email.ToLower().Trim(),
            PasswordHash = passwordHash,
            Role = role,
            IsActive = true
        };

        var created = await _authRepository.CreateAsync(user);
        await _authRepository.SaveChangesAsync();

        return new AuthResponseDto
        {
            Token = _jwtService.GenerateToken(created),
            UserName = created.UserName,
            Email = created.Email,
            Role = created.Role.ToString(),
            ExpiresAt = _jwtService.GetExpiryTime()
        };
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _authRepository.GetByEmailAsync(dto.Email.ToLower().Trim());

        // User nahi mila ya inactive hai
        if (user == null || !user.IsActive)
            return null;

        // Password verify
        bool isValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!isValid) return null;

        // Last login update
        user.LastLogin = DateTime.UtcNow;
        await _authRepository.SaveChangesAsync();

        return new AuthResponseDto
        {
            Token = _jwtService.GenerateToken(user),
            UserName = user.UserName,
            Email = user.Email,
            Role = user.Role.ToString(),
            ExpiresAt = _jwtService.GetExpiryTime()
        };
    }
}
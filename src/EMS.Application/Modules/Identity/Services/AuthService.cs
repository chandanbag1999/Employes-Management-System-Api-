using EMS.Application.Common.Interfaces;
using EMS.Application.Modules.Identity.DTOs;
using EMS.Application.Modules.Identity.Interfaces;
using EMS.Domain.Entities.Identity;
using EMS.Domain.Enums;

namespace EMS.Application.Modules.Identity.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtService _jwtService;

    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public AuthService(
        IAuthRepository authRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtService jwtService)
    {
        _authRepository = authRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
    {
        if (await _authRepository.EmailExistsAsync(dto.Email))
            return null;

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12);

        var user = new AppUser
        {
            UserName = dto.UserName.Trim(),
            Email = dto.Email.ToLower().Trim(),
            PasswordHash = passwordHash,
            Role = UserRole.Employee,
            IsActive = true,
            IsEmailVerified = false
        };

        var created = await _authRepository.CreateAsync(user);
        await _authRepository.SaveChangesAsync();

        var (accessToken, refreshToken) = await IssueTokensAsync(created, ipAddress: null);

        return BuildAuthResponse(created, accessToken, refreshToken);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto, string ipAddress)
    {
        var email = dto.Email.ToLower().Trim();

        var baseUser = await _authRepository.GetByEmailAsync(email);
        var user = baseUser != null
            ? await _authRepository.GetByIdWithTokensAsync(baseUser.Id)
            : null;

        if (user == null || !user.IsActive)
            return null;

        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            return null;

        bool isValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

        if (!isValid)
        {
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= MaxFailedAttempts)
                user.LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);

            await _authRepository.SaveChangesAsync();
            return null;
        }

        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.LastLogin = DateTime.UtcNow;
        await _authRepository.SaveChangesAsync();

        var (accessToken, refreshToken) = await IssueTokensAsync(user, ipAddress);

        return BuildAuthResponse(user, accessToken, refreshToken);
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (token == null || !token.IsActive)
            return null;

        var user = await _authRepository.GetByIdWithTokensAsync(token.UserId);
        if (user == null || !user.IsActive)
            return null;

        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;

        var (newAccessToken, newRefreshToken) = await IssueTokensAsync(user, ipAddress);
        token.ReplacedByToken = newRefreshToken.Token;

        await _refreshTokenRepository.SaveChangesAsync();

        return BuildAuthResponse(user, newAccessToken, newRefreshToken);
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        if (token == null || !token.IsActive) return false;

        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.SaveChangesAsync();

        return true;
    }

    // ── Private Helpers ───────────────────────────────────────────────

    private async Task<(string accessToken, RefreshToken refreshToken)> IssueTokensAsync(
        AppUser user, string? ipAddress)
    {
        var accessToken = _jwtService.GenerateAccessToken(user);

        var refreshToken = new RefreshToken
        {
            Token = _jwtService.GenerateRefreshToken(),
            ExpiresAt = _jwtService.GetRefreshTokenExpiry(),
            UserId = user.Id,
            CreatedByIp = ipAddress
        };

        await _refreshTokenRepository.AddAsync(refreshToken);
        await _refreshTokenRepository.SaveChangesAsync();

        return (accessToken, refreshToken);
    }

    private AuthResponseDto BuildAuthResponse(
        AppUser user, string accessToken, RefreshToken refreshToken)
        => new()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            UserName = user.UserName,
            Email = user.Email,
            Role = user.Role.ToString(),
            AccessTokenExpiresAt = _jwtService.GetAccessTokenExpiry(),
            RefreshTokenExpiresAt = refreshToken.ExpiresAt
        };
}
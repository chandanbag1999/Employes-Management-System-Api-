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
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;

    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan PasswordResetTokenTtl = TimeSpan.FromHours(1);
    private static readonly TimeSpan EmailVerificationTokenTtl = TimeSpan.FromHours(24);

    public AuthService(
        IAuthRepository authRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IJwtService jwtService,
        IEmailService emailService)
    {
        _authRepository = authRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _jwtService = jwtService;
        _emailService = emailService;
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
            IsEmailVerified = false,
            // ✅ Register se aane wale users ko first login force nahi karte
            IsFirstLogin = false
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

        // ❌ IsFirstLogin yahan mat badlo — ChangePasswordAsync mein badlega
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

    // ✅ NEW: Change Password — First Login + Normal Flow dono handle karta hai
    public async Task<(bool success, string? error)> ChangePasswordAsync(
        int userId,
        ChangePasswordDto dto)
    {
        var user = await _authRepository.GetByIdAsync(userId);
        if (user == null)
            return (false, "User not found.");

        // Current password verify karo
        bool isCurrentValid = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash);
        if (!isCurrentValid)
            return (false, "Current password is incorrect.");

        // New password current se alag honi chahiye
        bool isSamePassword = BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash);
        if (isSamePassword)
            return (false, "New password must be different from current password.");

        // Hash + save
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword, workFactor: 12);

        // ✅ First login complete — flag reset karo
        user.IsFirstLogin = false;

        // Email verified maano — usne login kiya matlab email valid hai
        user.IsEmailVerified = true;

        user.UpdatedAt = DateTime.UtcNow;

        await _authRepository.SaveChangesAsync();

        return (true, null);
    }

    // ── Forgot Password & Email Verification ──────────────────────────

    public async Task<(bool success, string? error)> RequestForgotPasswordAsync(
        string email, string? ipAddress)
    {
        var user = await _authRepository.GetByEmailAsync(email.ToLower().Trim());

        // Security: always return success to prevent email enumeration
        if (user == null)
            return (true, null);

        // Clean up old active tokens
        await _passwordResetTokenRepository.DeleteAllActiveTokensAsync(
            user.Id, TokenPurpose.PasswordReset);

        // Generate token
        var token = GenerateSecureToken();
        var resetToken = new PasswordResetToken
        {
            Token = token,
            UserId = user.Id,
            Purpose = TokenPurpose.PasswordReset,
            ExpiresAt = DateTime.UtcNow.Add(PasswordResetTokenTtl),
            CreatedByIp = ipAddress
        };

        await _passwordResetTokenRepository.AddAsync(resetToken);
        await _passwordResetTokenRepository.SaveChangesAsync();

        // Send email (failure should not affect user experience)
        try
        {
            // frontendUrl ko config se read karo — abhi hardcoded, future mein inject karo
            var frontendUrl = "http://localhost:8080";
            var resetUrl = $"{frontendUrl}/reset-password?token={token}";
            await _emailService.SendPasswordResetEmailAsync(
                user.Email, user.UserName, resetUrl);
        }
        catch (Exception ex)
        {
            // Email fail hone pe log karo, user ko success return karo
            System.Diagnostics.Debug.WriteLine(
                $"[AuthService] Password reset email failed: {ex.Message}");
        }

        return (true, null);
    }

    public async Task<(bool success, string? error)> ResetPasswordAsync(
        ResetPasswordDto dto)
    {
        var token = await _passwordResetTokenRepository.GetByTokenAsync(dto.Token);

        if (token == null)
            return (false, "Invalid or expired token.");

        if (!token.IsActive)
            return (false, "Token has been used or expired.");

        var user = await _authRepository.GetByIdAsync(token.UserId);
        if (user == null || !user.IsActive)
            return (false, "User not found.");

        // New password must be different
        if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash))
            return (false, "New password must be different from current password.");

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword, workFactor: 12);
        user.UpdatedAt = DateTime.UtcNow;
        user.IsEmailVerified = true; // email verified by resetting password

        // Mark token as used
        token.IsUsed = true;
        token.UsedAt = DateTime.UtcNow;

        // Revoke all refresh tokens — force re-login everywhere
        foreach (var rt in user.RefreshTokens.Where(r => !r.IsRevoked))
        {
            rt.IsRevoked = true;
            rt.RevokedAt = DateTime.UtcNow;
        }

        await _authRepository.SaveChangesAsync();
        await _passwordResetTokenRepository.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool success, string? error)> VerifyEmailAsync(
        VerifyEmailDto dto)
    {
        var token = await _passwordResetTokenRepository.GetByTokenAsync(dto.Token);

        if (token == null)
            return (false, "Invalid or expired token.");

        if (!token.IsActive)
            return (false, "Token has been used or expired.");

        var user = await _authRepository.GetByIdAsync(token.UserId);
        if (user == null)
            return (false, "User not found.");

        user.IsEmailVerified = true;
        user.UpdatedAt = DateTime.UtcNow;

        token.IsUsed = true;
        token.UsedAt = DateTime.UtcNow;

        await _authRepository.SaveChangesAsync();
        await _passwordResetTokenRepository.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool success, string? error)> ResendVerificationEmailAsync(
        int userId)
    {
        var user = await _authRepository.GetByIdAsync(userId);
        if (user == null)
            return (false, "User not found.");

        if (user.IsEmailVerified)
            return (false, "Email already verified.");

        // Clean up old tokens
        await _passwordResetTokenRepository.DeleteAllActiveTokensAsync(
            user.Id, TokenPurpose.EmailVerification);

        var token = GenerateSecureToken();
        var verifyToken = new PasswordResetToken
        {
            Token = token,
            UserId = user.Id,
            Purpose = TokenPurpose.EmailVerification,
            ExpiresAt = DateTime.UtcNow.Add(EmailVerificationTokenTtl),
            CreatedByIp = null
        };

        await _passwordResetTokenRepository.AddAsync(verifyToken);
        await _passwordResetTokenRepository.SaveChangesAsync();

        try
        {
            var frontendUrl = "http://localhost:8080";
            var verifyUrl = $"{frontendUrl}/verify-email?token={token}";
            await _emailService.SendEmailVerificationEmailAsync(
                user.Email, user.UserName, verifyUrl);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[AuthService] Verification email failed: {ex.Message}");
        }

        return (true, null);
    }

    private static string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
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
            IsFirstLogin = user.IsFirstLogin,
            AccessTokenExpiresAt = _jwtService.GetAccessTokenExpiry(),
            RefreshTokenExpiresAt = refreshToken.ExpiresAt
        };
}
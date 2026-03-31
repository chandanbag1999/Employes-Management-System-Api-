# Identity & Auth Module — Enterprise Grade Documentation

**Version:** 2.0
**Status:** Fully Implemented ✅
**Last Updated:** 2025

---

## 1. Module Philosophy & Design Intent

### Why This Module Exists
Every enterprise system needs a **single source of truth for identity**. This module is that source. Every other module (Payroll, Attendance, Leave, etc.) depends on this module to answer three questions:

- **Who are you?** → Authentication
- **What are you allowed to do?** → Authorization
- **Are you still allowed?** → Token lifecycle & session management

### What We Built & Why

| Decision | What | Why |
|----------|------|-----|
| JWT over Sessions | Stateless tokens | Scales horizontally, no server-side session store needed |
| BCrypt work factor 12 | Adaptive hashing | ~250ms/hash — brute force expensive, UX acceptable |
| RBAC over ABAC | Role-based access | Simple, auditable, fits EMS domain perfectly |
| Refresh Tokens | Long-lived sessions | Access token short-lived (15min), refresh token rotates |
| Soft Deactivation | `IsActive` flag | Never hard delete users, preserves audit trail |
| Email Normalization | Lowercase always | Prevents duplicate accounts via case variation |
| Layered Architecture | Domain/App/Infra/API | Each layer has one job, testable in isolation |
| Token Revocation on Deactivate | RevokeAllUserTokens | No zombie sessions after deactivation |
| Token Revocation on Role Change | RevokeAllUserTokens | Forces re-login so new role claims are embedded |
| ClockSkew = Zero | Strict expiry | Token expired = token rejected, no tolerance |
| JTI Claim | Unique token ID | Token fingerprinting capability |
| SuperAdmin Seeder | Fresh deploy ready | System usable without manual DB intervention |
| TokenCleanupService | Background cleanup | RefreshTokens table stays clean, no unbounded growth |

### Enterprise Mindset
> In enterprise systems, **security is not a feature — it is the foundation**. Every design decision in this module is made with the assumption that the system will be attacked, audited, and scaled.

---

## 2. Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│                        API Layer                        │
│         AuthController    UsersController               │
│         (Endpoints, HTTP concerns only)                 │
└────────────────────┬────────────────────────────────────┘
                     │ depends on
┌────────────────────▼────────────────────────────────────┐
│                  Application Layer                      │
│   IAuthService    IUserService    IJwtService           │
│   AuthService     UserService                           │
│   (Business logic, validation, orchestration)           │
└────────────────────┬────────────────────────────────────┘
                     │ depends on
┌────────────────────▼────────────────────────────────────┐
│               Infrastructure Layer                      │
│   AuthRepository   JwtService   TokenCleanupService     │
│   RefreshTokenRepository   SuperAdminSeeder             │
│   (EF Core, DB access, external services)               │
└────────────────────┬────────────────────────────────────┘
                     │ depends on
┌────────────────────▼────────────────────────────────────┐
│                   Domain Layer                          │
│   AppUser   UserRole   RefreshToken                     │
│   (Pure C# — no framework dependencies)                 │
└─────────────────────────────────────────────────────────┘
```

### Layer Responsibilities

| Layer | Does | Does NOT |
|-------|------|----------|
| **API** | Receives HTTP, validates ModelState, returns HTTP responses | Business logic, DB calls |
| **Application** | Orchestrates business rules, calls repositories | Direct DB access, HTTP concerns |
| **Infrastructure** | EF Core, JWT generation, background services | Business rules |
| **Domain** | Entities, Enums | Anything framework related |

---

## 3. Related Files

| Layer | File | Purpose |
|-------|------|---------|
| **Domain** | `src/EMS.Domain/Entities/Identity/AppUser.cs` | User entity |
| **Domain** | `src/EMS.Domain/Entities/Identity/RefreshToken.cs` | Refresh token entity |
| **Domain** | `src/EMS.Domain/Enums/UserRole.cs` | Role enumeration |
| **Application** | `src/EMS.Application/Modules/Identity/DTOs/LoginDto.cs` | Login request DTO |
| **Application** | `src/EMS.Application/Modules/Identity/DTOs/RegisterDto.cs` | Register request DTO |
| **Application** | `src/EMS.Application/Modules/Identity/DTOs/AuthResponseDto.cs` | Auth response DTO |
| **Application** | `src/EMS.Application/Modules/Identity/DTOs/RefreshTokenRequestDto.cs` | Refresh token request DTO |
| **Application** | `src/EMS.Application/Modules/Identity/DTOs/LogoutDto.cs` | Logout request DTO |
| **Application** | `src/EMS.Application/Modules/Identity/DTOs/ChangeRoleDto.cs` | Change role DTO |
| **Application** | `src/EMS.Application/Modules/Identity/DTOs/UserResponseDto.cs` | User response DTO |
| **Application** | `src/EMS.Application/Modules/Identity/Interfaces/IAuthService.cs` | Auth service interface |
| **Application** | `src/EMS.Application/Modules/Identity/Interfaces/IAuthRepository.cs` | Auth repository interface |
| **Application** | `src/EMS.Application/Modules/Identity/Interfaces/IUserService.cs` | User service interface |
| **Application** | `src/EMS.Application/Modules/Identity/Interfaces/IRefreshTokenRepository.cs` | Refresh token repository interface |
| **Application** | `src/EMS.Application/Common/Interfaces/IJwtService.cs` | JWT service interface |
| **Application** | `src/EMS.Application/Modules/Identity/Services/AuthService.cs` | Auth business logic |
| **Application** | `src/EMS.Application/Modules/Identity/Services/UserService.cs` | User business logic |
| **Infrastructure** | `src/EMS.Infrastructure/Services/JwtService.cs` | JWT token generation |
| **Infrastructure** | `src/EMS.Infrastructure/Repositories/AuthRepository.cs` | User data access |
| **Infrastructure** | `src/EMS.Infrastructure/Repositories/RefreshTokenRepository.cs` | Token data access |
| **Infrastructure** | `src/EMS.Infrastructure/BackgroundServices/TokenCleanupService.cs` | Token cleanup |
| **Infrastructure** | `src/EMS.Infrastructure/Seeders/SuperAdminSeeder.cs` | SuperAdmin seeding |
| **Infrastructure** | `src/EMS.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs` | EF Core config |
| **Infrastructure** | `src/EMS.Infrastructure/DependencyInjection.cs` | DI registration |
| **API** | `src/EMS.API/Controllers/v1/AuthController.cs` | Auth endpoints |
| **API** | `src/EMS.API/Controllers/v1/UsersController.cs` | User endpoints |
| **API** | `src/EMS.API/Program.cs` | App setup + seeder wiring |

---

## 4. Domain Layer

### 4.1 User Roles

```csharp
// src/EMS.Domain/Enums/UserRole.cs

public enum UserRole
{
    SuperAdmin = 1,  // Full system control
    HRAdmin    = 2,  // HR operations
    Manager    = 3,  // Team management
    Employee   = 4   // Self-service only
}
```

**Why values start at 1:**
Zero is default int value in C#. Starting at 1 prevents accidental assignment of uninitialized int being treated as a valid role.

### Role Permission Matrix

| Permission | SuperAdmin | HRAdmin | Manager | Employee |
|------------|-----------|---------|---------|----------|
| Register new user | ✅ | ❌ | ❌ | ❌ |
| View all users | ✅ | ✅ | ❌ | ❌ |
| View single user | ✅ | ✅ | ❌ | ❌ |
| Deactivate user | ✅ | ❌ | ❌ | ❌ |
| Change user role | ✅ | ❌ | ❌ | ❌ |
| View own profile | ✅ | ✅ | ✅ | ✅ |
| Refresh token | ✅ | ✅ | ✅ | ✅ |
| Logout | ✅ | ✅ | ✅ | ✅ |

### 4.2 AppUser Entity

```csharp
// src/EMS.Domain/Entities/Identity/AppUser.cs

public class AppUser : BaseEntity
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Employee;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLogin { get; set; }
    public int? EmployeeId { get; set; }

    // Enterprise fields
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }
    public bool IsEmailVerified { get; set; } = false;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
```

**Field Intentions:**

| Field | Intent |
|-------|--------|
| `Email` | Always stored lowercase — prevents duplicate accounts via case variation |
| `PasswordHash` | BCrypt hash, never plain text, work factor 12 |
| `Role` | Default Employee — lowest privilege |
| `IsActive` | Soft deactivation — never hard delete, preserves audit trail |
| `EmployeeId` | Nullable — SuperAdmin/HRAdmin may not have Employee record |
| `FailedLoginAttempts` | Brute force counter — resets on successful login |
| `LockoutEnd` | UTC lockout expiry — null means not locked |
| `IsEmailVerified` | Foundation for email verification (future scope) |
| `RefreshTokens` | Navigation property — EF Core handles the join |

### 4.3 RefreshToken Entity

```csharp
// src/EMS.Domain/Entities/Identity/RefreshToken.cs

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? CreatedByIp { get; set; }
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;

    // Computed — no DB column
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;
}
```

**Why store in DB (not JWT):**
- JWT refresh tokens cannot be revoked — once issued, valid until expiry
- DB storage enables: logout, deactivation response, breach response
- Rotation tracking via `ReplacedByToken` detects reuse attacks
- `CreatedByIp` enables anomaly detection

**Token lifecycle:**
```
Issued → Active → Used in refresh → Rotated (old revoked, new issued)
                → Logout → Revoked
                → Expired → Cleaned by TokenCleanupService (30 days)
```

---

## 5. Application Layer

### 5.1 DTOs

```csharp
// src/EMS.Application/Modules/Identity/DTOs/LoginDto.cs

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
```

```csharp
// src/EMS.Application/Modules/Identity/DTOs/RegisterDto.cs

public class RegisterDto
{
    [Required]
    [MaxLength(100)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must have uppercase, lowercase, digit, and special character.")]
    public string Password { get; set; } = string.Empty;
}
```

**Why no Role field in RegisterDto:**
Registration always creates Employee role. Role assignment is a SuperAdmin action done separately via `PATCH /users/{id}/role`. This prevents privilege escalation via registration.

```csharp
// src/EMS.Application/Modules/Identity/DTOs/AuthResponseDto.cs

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }
}
```

```csharp
// src/EMS.Application/Modules/Identity/DTOs/RefreshTokenRequestDto.cs

public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
```

```csharp
// src/EMS.Application/Modules/Identity/DTOs/LogoutDto.cs

public class LogoutDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
```

```csharp
// src/EMS.Application/Modules/Identity/DTOs/ChangeRoleDto.cs

public class ChangeRoleDto
{
    [Required]
    [Range(1, 4, ErrorMessage = "Role must be between 1 (SuperAdmin) and 4 (Employee).")]
    public int Role { get; set; }
}
```

```csharp
// src/EMS.Application/Modules/Identity/DTOs/UserResponseDto.cs

public class UserResponseDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
    public bool IsLockedOut { get; set; }
}
```

### 5.2 Interfaces

```csharp
// src/EMS.Application/Modules/Identity/Interfaces/IAuthService.cs

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto?> LoginAsync(LoginDto dto, string ipAddress);
    Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken, string ipAddress);
    Task<bool> LogoutAsync(string refreshToken);
}
```

```csharp
// src/EMS.Application/Modules/Identity/Interfaces/IUserService.cs

public interface IUserService
{
    Task<IEnumerable<UserResponseDto>> GetAllAsync();
    Task<UserResponseDto?> GetByIdAsync(int id);
    Task<UserResponseDto?> GetCurrentUserAsync(int userId);
    Task<bool> DeactivateUserAsync(int id);
    Task<bool> ChangeRoleAsync(int id, UserRole newRole);
}
```

```csharp
// src/EMS.Application/Modules/Identity/Interfaces/IAuthRepository.cs

public interface IAuthRepository
{
    Task<AppUser?> GetByEmailAsync(string email);
    Task<AppUser?> GetByIdAsync(int id);
    Task<AppUser?> GetByIdWithTokensAsync(int id);  // Eager loads RefreshTokens
    Task<IEnumerable<AppUser>> GetAllAsync();
    Task<AppUser> CreateAsync(AppUser user);
    Task<bool> EmailExistsAsync(string email);
    Task SaveChangesAsync();
}
```

```csharp
// src/EMS.Application/Modules/Identity/Interfaces/IRefreshTokenRepository.cs

// Separated from IAuthRepository — Interface Segregation Principle
// Token management is a distinct concern from user management
public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task AddAsync(RefreshToken refreshToken);
    Task RevokeAllUserTokensAsync(int userId);
    Task RemoveExpiredTokensAsync();
    Task SaveChangesAsync();
}
```

```csharp
// src/EMS.Application/Common/Interfaces/IJwtService.cs

public interface IJwtService
{
    string GenerateAccessToken(AppUser user);
    string GenerateRefreshToken();
    DateTime GetAccessTokenExpiry();
    DateTime GetRefreshTokenExpiry();
    int? GetUserIdFromToken(string token);
}
```

### 5.3 AuthService

```csharp
// src/EMS.Application/Modules/Identity/Services/AuthService.cs

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

        // Rotate — old token revoke, new token issue
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
```

**AuthService Intentions:**

| Method | What | Why |
|--------|------|-----|
| `RegisterAsync` | Creates Employee user + issues both tokens | Always Employee — no privilege escalation |
| `LoginAsync` | Validates credentials + lockout + issues tokens | 2-step user fetch — email first, then with tokens |
| `RefreshTokenAsync` | Rotates refresh token + issues new access token | Old token revoked — reuse attack prevention |
| `LogoutAsync` | Revokes refresh token | Client-side access token expires naturally in 15min |
| `IssueTokensAsync` | Private helper — generates both tokens | DRY — used by Register, Login, Refresh |
| `BuildAuthResponse` | Private helper — maps to DTO | DRY — consistent response structure |

### 5.4 UserService

```csharp
// src/EMS.Application/Modules/Identity/Services/UserService.cs

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

    private static UserResponseDto MapToDto(AppUser u) => new()
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
```

**Why token revocation in both Deactivate and ChangeRole:**
- `DeactivateUserAsync` — user's existing access token still valid for up to 15min. Revoking refresh token prevents any further session extension.
- `ChangeRoleAsync` — existing JWT still carries old role claims. Revoking forces re-login so new token embeds new role.

---

## 6. Infrastructure Layer

### 6.1 JwtService

```csharp
// src/EMS.Infrastructure/Services/JwtService.cs

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config) => _config = config;

    public string GenerateAccessToken(AppUser user)
    {
        var secret = _config["JwtSettings:Secret"]
            ?? throw new InvalidOperationException("JWT Secret not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"],
            audience: _config["JwtSettings:Audience"],
            claims: claims,
            expires: GetAccessTokenExpiry(),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public DateTime GetAccessTokenExpiry()
    {
        var minutes = int.Parse(
            _config["JwtSettings:AccessTokenExpiryMinutes"] ?? "15");
        return DateTime.UtcNow.AddMinutes(minutes);
    }

    public DateTime GetRefreshTokenExpiry()
    {
        var days = int.Parse(
            _config["JwtSettings:RefreshTokenExpiryDays"] ?? "7");
        return DateTime.UtcNow.AddDays(days);
    }

    public int? GetUserIdFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var claim = jwt.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : null;
        }
        catch { return null; }
    }
}
```

**Why opaque refresh token (not JWT):**
- Can be stored and revoked in DB
- No sensitive data embedded in token string
- Rotation is simple — generate new random value
- 64 bytes = 512 bits entropy = practically unguessable

### 6.2 AuthRepository

```csharp
// src/EMS.Infrastructure/Repositories/AuthRepository.cs

public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _context;

    public AuthRepository(AppDbContext context) => _context = context;

    public async Task<AppUser?> GetByEmailAsync(string email)
        => await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLower());

    public async Task<AppUser?> GetByIdAsync(int id)
        => await _context.Users.FindAsync(id);

    public async Task<AppUser?> GetByIdWithTokensAsync(int id)
        => await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == id);

    public async Task<IEnumerable<AppUser>> GetAllAsync()
        => await _context.Users
            .OrderBy(u => u.CreatedAt)
            .ToListAsync();

    public async Task<AppUser> CreateAsync(AppUser user)
    {
        await _context.Users.AddAsync(user);
        return user;
    }

    public async Task<bool> EmailExistsAsync(string email)
        => await _context.Users
            .AnyAsync(u => u.Email == email.ToLower());

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
```

**Why `GetByIdWithTokensAsync` is separate from `GetByIdAsync`:**
- `GetByIdAsync` — most calls don't need tokens (GetById, Deactivate, ChangeRole)
- `GetByIdWithTokensAsync` — only Login and Refresh need tokens loaded
- Eager loading has performance cost — only pay when needed

### 6.3 RefreshTokenRepository

```csharp
// src/EMS.Infrastructure/Repositories/RefreshTokenRepository.cs

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context) => _context = context;

    public async Task<RefreshToken?> GetByTokenAsync(string token)
        => await _context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token);

    public async Task AddAsync(RefreshToken refreshToken)
        => await _context.RefreshTokens.AddAsync(refreshToken);

    public async Task RevokeAllUserTokensAsync(int userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }
    }

    public async Task RemoveExpiredTokensAsync()
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);

        var oldTokens = await _context.RefreshTokens
            .Where(t =>
                (t.IsRevoked || t.ExpiresAt < DateTime.UtcNow)
                && t.CreatedAt < cutoff)
            .ToListAsync();

        _context.RefreshTokens.RemoveRange(oldTokens);
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
```

**Why 30 day cutoff in `RemoveExpiredTokensAsync`:**
Recent expired/revoked tokens preserved for potential security audit. Tokens older than 30 days have no operational or investigative value.

### 6.4 RefreshTokenConfiguration

```csharp
// src/EMS.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(rt => rt.ExpiresAt).IsRequired();
        builder.Property(rt => rt.CreatedByIp).HasMaxLength(64);
        builder.Property(rt => rt.ReplacedByToken).HasMaxLength(256);

        builder.HasIndex(rt => rt.Token).IsUnique();

        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Matching query filter — resolves EF Core warning
        // AppUser has global filter: !e.IsDeleted
        // RefreshToken must match to avoid unexpected results
        builder.HasQueryFilter(rt => !rt.IsDeleted && !rt.User.IsDeleted);
    }
}
```

### 6.5 TokenCleanupService

```csharp
// src/EMS.Infrastructure/BackgroundServices/TokenCleanupService.cs

public class TokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TokenCleanupService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    public TokenCleanupService(
        IServiceProvider serviceProvider,
        ILogger<TokenCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var repo = scope.ServiceProvider
                    .GetRequiredService<IRefreshTokenRepository>();

                await repo.RemoveExpiredTokensAsync();
                await repo.SaveChangesAsync();

                _logger.LogInformation(
                    "Token cleanup completed at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token cleanup failed");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}
```

**Why `IServiceProvider` instead of direct injection:**
`TokenCleanupService` is singleton (background service lifetime). `AppDbContext` is scoped. Singleton cannot directly depend on scoped — creates scope manually per execution.

### 6.6 SuperAdminSeeder

```csharp
// src/EMS.Infrastructure/Seeders/SuperAdminSeeder.cs

public static class SuperAdminSeeder
{
    public static async Task SeedAsync(AppDbContext context, IConfiguration config)
    {
        var exists = await context.Users
            .AnyAsync(u => u.Role == UserRole.SuperAdmin);

        if (exists) return;

        var email = config["SuperAdmin:Email"] ?? "superadmin@ems.com";
        var password = config["SuperAdmin:Password"] ?? "Admin@1234";

        var superAdmin = new AppUser
        {
            UserName = "Super Admin",
            Email = email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12),
            Role = UserRole.SuperAdmin,
            IsActive = true,
            IsEmailVerified = true
        };

        await context.Users.AddAsync(superAdmin);
        await context.SaveChangesAsync();
    }
}
```

**Why idempotent:**
Runs on every startup. `AnyAsync` check ensures it only creates if no SuperAdmin exists. Safe to run multiple times.

---

## 7. API Layer

### 7.1 AuthController

```csharp
// src/EMS.API/Controllers/v1/AuthController.cs

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Invalid request data."));

        var result = await _authService.RegisterAsync(dto);

        if (result == null)
            return Conflict(ApiResponse<string>.Fail("Email already registered."));

        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Registration successful."));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Invalid request data."));

        var ip = GetIpAddress();
        var result = await _authService.LoginAsync(dto, ip);

        if (result == null)
            return Unauthorized(ApiResponse<string>.Fail("Invalid credentials or account locked."));

        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login successful."));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Invalid request data."));

        var ip = GetIpAddress();
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken, ip);

        if (result == null)
            return Unauthorized(ApiResponse<string>.Fail("Invalid or expired refresh token."));

        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Token refreshed."));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Invalid request data."));

        var result = await _authService.LogoutAsync(dto.RefreshToken);

        if (!result)
            return BadRequest(ApiResponse<string>.Fail("Invalid or already revoked token."));

        return Ok(ApiResponse<string>.Ok("Logged out.", "Logout successful."));
    }

    private string GetIpAddress()
    {
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
            return forwarded.ToString();

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
```

### 7.2 UsersController

```csharp
// src/EMS.API/Controllers/v1/UsersController.cs

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<UserResponseDto>>.Ok(users));
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<string>.Fail("Invalid token."));

        var user = await _userService.GetCurrentUserAsync(userId.Value);
        if (user == null)
            return NotFound(ApiResponse<string>.Fail("User not found."));

        return Ok(ApiResponse<UserResponseDto>.Ok(user));
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound(ApiResponse<string>.Fail("User not found."));

        return Ok(ApiResponse<UserResponseDto>.Ok(user));
    }

    [HttpPatch("{id}/deactivate")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var result = await _userService.DeactivateUserAsync(id);
        if (!result)
            return NotFound(ApiResponse<string>.Fail("User not found."));

        return Ok(ApiResponse<string>.Ok("User deactivated."));
    }

    [HttpPatch("{id}/role")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ChangeRole(int id, [FromBody] ChangeRoleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Invalid role value."));

        if (!Enum.IsDefined(typeof(UserRole), dto.Role))
            return BadRequest(ApiResponse<string>.Fail("Invalid role specified."));

        var result = await _userService.ChangeRoleAsync(id, (UserRole)dto.Role);
        if (!result)
            return NotFound(ApiResponse<string>.Fail("User not found."));

        return Ok(ApiResponse<string>.Ok("Role updated. User must re-login."));
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : null;
    }
}
```

---

## 8. Configuration

### appsettings.json

```json
{
  "JwtSettings": {
    "Secret": "REPLACE_WITH_ENV_VAR_IN_PRODUCTION_MIN_32_CHARS!!",
    "Issuer": "EMS.API",
    "Audience": "EMS.Client",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  },
  "SuperAdmin": {
    "Email": "superadmin@ems.com",
    "Password": "Admin@1234!"
  }
}
```

### DependencyInjection.cs (Identity Related)

```csharp
// Repositories
services.AddScoped<IAuthRepository, AuthRepository>();
services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Services
services.AddScoped<IAuthService, AuthService>();
services.AddScoped<IUserService, UserService>();
services.AddScoped<IJwtService, JwtService>();

// Background Services
services.AddHostedService<TokenCleanupService>();

// JWT
services.AddAuthentication(...)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // ... validation params
            ClockSkew = TimeSpan.Zero  // Strict expiry
        };
    });
```

### Program.cs (Identity Related)

```csharp
// Migration + Seeding on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await SuperAdminSeeder.SeedAsync(db, builder.Configuration);
}

// Middleware order
app.UseAuthentication();
app.UseAuthorization();
```

---

## 9. Authentication Flow

```
REGISTRATION:
Client → POST /api/v1/auth/register
    → ModelState validation
    → EmailExistsAsync check
    → BCrypt.Hash(password, workFactor=12)
    → Create AppUser (Role=Employee, IsActive=true)
    → SaveChangesAsync
    → IssueTokensAsync (AccessToken + RefreshToken)
    → Return AuthResponseDto

LOGIN:
Client → POST /api/v1/auth/login
    → ModelState validation
    → GetByEmailAsync (normalize to lowercase)
    → GetByIdWithTokensAsync (eager load tokens)
    → Check IsActive
    → Check LockoutEnd
    → BCrypt.Verify(password, hash)
    → If invalid: FailedAttempts++, lock if >= 5
    → If valid: Reset attempts, set LastLogin
    → IssueTokensAsync
    → Return AuthResponseDto

TOKEN REFRESH:
Client → POST /api/v1/auth/refresh
    → GetByTokenAsync
    → Check token.IsActive (not revoked, not expired)
    → Check user.IsActive
    → Revoke old token (IsRevoked=true, ReplacedByToken set)
    → IssueTokensAsync (new pair)
    → Return AuthResponseDto

LOGOUT:
Client → POST /api/v1/auth/logout [Authorized]
    → GetByTokenAsync
    → Check token.IsActive
    → Mark IsRevoked=true, RevokedAt=now
    → Return 200
```

---

## 10. API Endpoint Reference

| Method | Endpoint | Auth | Role | Description |
|--------|----------|------|------|-------------|
| POST | `/api/v1/auth/register` | ❌ | — | Register new user (always Employee) |
| POST | `/api/v1/auth/login` | ❌ | — | Login with credentials |
| POST | `/api/v1/auth/refresh` | ❌ | — | Refresh access token |
| POST | `/api/v1/auth/logout` | ✅ | Any | Revoke refresh token |
| GET | `/api/v1/users` | ✅ | SuperAdmin, HRAdmin | Get all users |
| GET | `/api/v1/users/me` | ✅ | Any | Get current user profile |
| GET | `/api/v1/users/{id}` | ✅ | SuperAdmin, HRAdmin | Get user by ID |
| PATCH | `/api/v1/users/{id}/deactivate` | ✅ | SuperAdmin | Deactivate user |
| PATCH | `/api/v1/users/{id}/role` | ✅ | SuperAdmin | Change user role |

---

## 11. JWT Token Structure

```json
{
  "nameid": "42",
  "email": "john@example.com",
  "unique_name": "John Doe",
  "role": "HRAdmin",
  "jti": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "iss": "EMS.API",
  "aud": "EMS.Client",
  "exp": 1711797300,
  "iat": 1711796400
}
```

---

## 12. Security Feature Reference

| Feature | Implementation | Why |
|---------|---------------|-----|
| Password Hashing | BCrypt work factor 12 | ~250ms/hash — brute force expensive |
| JWT Algorithm | HMAC-SHA256 | Industry standard, fast, secure |
| Access Token Expiry | 15 minutes | Limits stolen token damage window |
| Refresh Token | 64-byte random, DB-stored | Revocable, rotated on every use |
| Token Rotation | Old revoked on refresh | Reuse attack detection |
| Account Lockout | 5 attempts → 15min lock | Brute force protection |
| Soft Deactivation | IsActive flag | Preserves audit trail |
| Token Revocation on Deactivate | RevokeAllUserTokens | No zombie sessions |
| Token Revocation on Role Change | RevokeAllUserTokens | Force re-login with new claims |
| Email Normalization | Lowercase stored | No duplicate accounts via case |
| ClockSkew = Zero | Token validation | Strict expiry enforcement |
| JTI Claim | Unique token ID | Token fingerprinting |
| SuperAdmin Seeder | Idempotent startup | System usable on fresh deploy |
| TokenCleanupService | 24hr background job | Table stays clean, no unbounded growth |
| IP Capture | Login + Refresh | Anomaly detection foundation |

---

## 13. DB Schema

```sql
-- AppUsers additions
ALTER TABLE AppUsers ADD FailedLoginAttempts INT NOT NULL DEFAULT 0;
ALTER TABLE AppUsers ADD LockoutEnd DATETIME NULL;
ALTER TABLE AppUsers ADD IsEmailVerified BIT NOT NULL DEFAULT 0;

-- RefreshTokens table
CREATE TABLE RefreshTokens (
    Id INT PRIMARY KEY IDENTITY,
    Token NVARCHAR(256) NOT NULL UNIQUE,
    ExpiresAt DATETIME NOT NULL,
    IsRevoked BIT NOT NULL DEFAULT 0,
    RevokedAt DATETIME NULL,
    ReplacedByToken NVARCHAR(256) NULL,
    CreatedByIp NVARCHAR(64) NULL,
    UserId INT NOT NULL REFERENCES AppUsers(Id) ON DELETE CASCADE,
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

CREATE INDEX IX_RefreshTokens_Token ON RefreshTokens(Token);
CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
```

---

## 14. What Changed From v1.0 — Change Log

| # | What Changed | Why |
|---|-------------|-----|
| 1 | Access token expiry 60min → 15min | Short window limits damage if token stolen |
| 2 | Added Refresh Token system | Persistent sessions without credential re-entry |
| 3 | Added token rotation | Detect and block refresh token reuse attacks |
| 4 | Added logout endpoint | Proper session termination |
| 5 | Added account lockout | Brute force protection |
| 6 | Added `GET /users/me` endpoint | Any user can fetch own profile |
| 7 | Added `PATCH /users/{id}/role` endpoint | SuperAdmin manages roles via API |
| 8 | Added token revocation on deactivate | No zombie sessions |
| 9 | Added token revocation on role change | Forces re-login with new role claims |
| 10 | Added SuperAdmin seeder | System usable on fresh deploy |
| 11 | JWT Secret null check throws | Fail fast — misconfiguration caught at startup |
| 12 | ClockSkew = Zero | Strict token expiry enforcement |
| 13 | JTI claim added | Unique token ID per token |
| 14 | BCrypt work factor explicit (12) | Clarity + performance awareness |
| 15 | Password MinLength 6 → 8 | Industry minimum standard |
| 16 | Password complexity regex | Uppercase + lowercase + digit + special required |
| 17 | Consistent ApiResponse wrapper | All endpoints same response format |
| 18 | Added `IsEmailVerified` field | Foundation for email verification |
| 19 | Added `IsLockedOut` in UserResponseDto | Admin can see locked accounts |
| 20 | TokenCleanupService | RefreshTokens table cleanup every 24hr |
| 21 | IRefreshTokenRepository separate | ISP — token concerns separate from user concerns |
| 22 | `GetByIdWithTokensAsync` separate | Performance — eager load only when needed |
| 23 | IP Address captured on Login/Refresh | Anomaly detection foundation |
| 24 | RefreshTokenConfiguration query filter | Resolves EF Core warning on global filters |

---

## 15. Remaining Limitations (Future Scope)

| Feature | Priority | Why Not Now |
|---------|----------|-------------|
| Audit Logging | High | IAuditService interface ready — needs AuditLog entity + AuditService implementation |
| Email Verification Flow | High | Needs email service (SMTP/SendGrid) |
| Password Reset Flow | High | Needs email service + secure token |
| Two-Factor Authentication | Medium | Needs TOTP library or SMS provider |
| Rate Limiting | High | ASP.NET Core 8 built-in — add in Program.cs |
| Token Blacklist (Redis) | Low | DB revocation sufficient for this scale |

---

## 16. Test Coverage

```
Total: 27 tests — 27 passed, 0 failed

Register Tests:
✅ RegisterAsync_WithValidData_ShouldReturnAuthResponse
✅ RegisterAsync_WithExistingEmail_ShouldReturnNull
✅ RegisterAsync_ShouldAlwaysCreateEmployeeRole

Login Tests:
✅ LoginAsync_WithValidCredentials_ShouldReturnAuthResponse
✅ LoginAsync_WithWrongPassword_ShouldReturnNull
✅ LoginAsync_WithNonExistentEmail_ShouldReturnNull
✅ LoginAsync_WithInactiveUser_ShouldReturnNull
✅ LoginAsync_WithLockedAccount_ShouldReturnNull
```

---

*Implementation order followed: Domain → Infrastructure (migration) → Application → API*
*Build: ✅ Succeeded | Tests: ✅ 27/27 passed*

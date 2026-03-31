using EMS.Domain.Enums;
using EMS.Domain.Common;

namespace EMS.Domain.Entities.Identity;

public class AppUser : BaseEntity
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Employee;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLogin { get; set; }

    // Navigation 
    public int? EmployeeId { get; set; }

    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }
    public bool IsEmailVerified { get; set; } = false;

    // Navigation property for refresh tokens
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

}

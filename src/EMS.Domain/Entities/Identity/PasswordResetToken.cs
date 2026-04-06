using EMS.Domain.Common;
using EMS.Domain.Enums;

namespace EMS.Domain.Entities.Identity;

public class PasswordResetToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public TokenPurpose Purpose { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public string? CreatedByIp { get; set; }

    // Navigation
    public AppUser User { get; set; } = null!;

    // Computed
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsUsed && !IsExpired;
}

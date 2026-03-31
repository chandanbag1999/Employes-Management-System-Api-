using EMS.Domain.Common;

namespace EMS.Domain.Entities.Identity;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = string.Empty; 
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false; // Indicates if the token has been revoked
    public DateTime? RevokedAt { get; set; } // Timestamp of when the token was revoked
    public string? ReplacedByToken { get; set; } // If the token was replaced by a new token, store the new token here

    public string? CreatedByIp { get; set; } // IP address from which the token was created 
    public int UserId { get; set; } // Foreign key to AppUser
    public AppUser User { get; set; } = null!; // Navigation property to AppUser


    // ── Computed Properties (no DB column)
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt; // Indicates if the token has expired
    public bool IsActive => !IsRevoked && !IsExpired; // Indicates if the token is active
    
}
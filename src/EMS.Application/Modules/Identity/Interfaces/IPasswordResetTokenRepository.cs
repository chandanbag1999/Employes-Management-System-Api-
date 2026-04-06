using EMS.Domain.Entities.Identity;
using EMS.Domain.Enums;

namespace EMS.Application.Modules.Identity.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetByTokenAsync(string token);
    Task<PasswordResetToken?> GetActiveTokenAsync(int userId, TokenPurpose purpose);
    Task<int> DeleteAllActiveTokensAsync(int userId, TokenPurpose purpose);
    Task AddAsync(PasswordResetToken token);
    Task RemoveExpiredTokensAsync();
    Task SaveChangesAsync();
}

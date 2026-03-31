using EMS.Domain.Entities.Identity;

namespace EMS.Application.Modules.Identity.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task AddAsync(RefreshToken refreshToken);
    Task RevokeAllUserTokensAsync(int userId);
    Task RemoveExpiredTokensAsync();
    Task SaveChangesAsync();
}
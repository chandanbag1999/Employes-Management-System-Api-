using EMS.Application.Modules.Identity.Interfaces;
using EMS.Domain.Entities.Identity;
using EMS.Domain.Enums;
using EMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EMS.Infrastructure.Repositories;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly AppDbContext _context;

    public PasswordResetTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PasswordResetToken?> GetByTokenAsync(string token)
        => await _context.PasswordResetTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Token == token);

    public async Task<PasswordResetToken?> GetActiveTokenAsync(int userId, TokenPurpose purpose)
        => await _context.PasswordResetTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.UserId == userId &&
                t.Purpose == purpose &&
                !t.IsUsed &&
                !t.IsDeleted &&
                t.ExpiresAt > DateTime.UtcNow);

    public async Task<int> DeleteAllActiveTokensAsync(int userId, TokenPurpose purpose)
    {
        var activeTokens = await _context.PasswordResetTokens
            .Where(t => t.UserId == userId && t.Purpose == purpose && !t.IsUsed && !t.IsDeleted)
            .ToListAsync();

        if (activeTokens.Count > 0)
        {
            _context.PasswordResetTokens.RemoveRange(activeTokens);
        }

        return activeTokens.Count;
    }

    public async Task AddAsync(PasswordResetToken token)
        => await _context.PasswordResetTokens.AddAsync(token);

    public async Task RemoveExpiredTokensAsync()
    {
        var expiredTokens = await _context.PasswordResetTokens
            .Where(t => t.ExpiresAt < DateTime.UtcNow || t.IsUsed)
            .ToListAsync();

        if (expiredTokens.Count > 0)
        {
            _context.PasswordResetTokens.RemoveRange(expiredTokens);
        }
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}

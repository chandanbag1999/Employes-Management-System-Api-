using EMS.Application.Modules.Identity.Interfaces;
using EMS.Domain.Entities.Identity;
using EMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EMS.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

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
        // 30 din purane expired/revoked tokens hard delete karo
        // Recent tokens audit ke liye preserve karo
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
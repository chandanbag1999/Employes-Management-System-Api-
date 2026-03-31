using EMS.Application.Modules.Identity.Interfaces;
using EMS.Domain.Entities.Identity;
using EMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EMS.Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _context;

    public AuthRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppUser?> GetByEmailAsync(string email)
        => await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLower());

    public async Task<AppUser?> GetByIdAsync(int id)
        => await _context.Users.FindAsync(id);

    // NEW — eager loads RefreshTokens
    // Login aur refresh flow mein zarurat hoti hai
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
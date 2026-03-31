using EMS.Domain.Entities.Identity;
using EMS.Domain.Enums;
using EMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EMS.Infrastructure.Seeders;


public static class SuperAdminSeeder
{
    public static async Task SeedAsync(AppDbContext context, IConfiguration config)
    {
        // Already exists? Skip karo
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
            IsEmailVerified = true  // Pre-verified — seeded account
        };

        await context.Users.AddAsync(superAdmin);
        await context.SaveChangesAsync();
    }
}
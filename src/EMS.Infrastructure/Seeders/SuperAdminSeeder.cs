using EMS.Domain.Entities.Employee;
using EMS.Domain.Entities.Identity;
using EMS.Domain.Entities.Organization;
using EMS.Domain.Enums;
using EMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EMS.Infrastructure.Seeders;


public static class SuperAdminSeeder
{
    public static async Task SeedAsync(AppDbContext context, IConfiguration config)
    {
        var email = config["SuperAdmin:Email"] ?? "superadmin@ems.com";
        var password = config["SuperAdmin:Password"] ?? "Admin@1234";

        // ── SuperAdmin User ──────────────────────────────
        var superAdmin = await context.Users
            .FirstOrDefaultAsync(u => u.Role == UserRole.SuperAdmin);

        if (superAdmin == null)
        {
            superAdmin = new AppUser
            {
                UserName = "Super Admin",
                Email = email.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12),
                Role = UserRole.SuperAdmin,
                IsActive = true,
                IsEmailVerified = true,
                IsFirstLogin = true
            };

            await context.Users.AddAsync(superAdmin);
            await context.SaveChangesAsync();
        }

        // ── EmployeeProfile for SuperAdmin ───────────────
        var hasProfile = await context.Employees
            .AnyAsync(e => e.UserId == superAdmin.Id);

        if (hasProfile) return;

        // Ensure default department exists
        var adminDept = await context.Departments
            .FirstOrDefaultAsync(d => d.Name == "Administration");

        if (adminDept == null)
        {
            adminDept = new Department
            {
                Name = "Administration",
                Code = "ADMIN"
            };

            await context.Departments.AddAsync(adminDept);
            await context.SaveChangesAsync();

            // Reload to get the ID
            adminDept = await context.Departments
                .FirstOrDefaultAsync(d => d.Name == "Administration");
        }

        if (adminDept != null)
        {
            var superAdminProfile = new EmployeeProfile
            {
                EmployeeCode = "EMP000",
                FirstName = "Super",
                LastName = "Admin",
                Email = email.ToLower(),
                Phone = string.Empty,
                Gender = Gender.Other,
                DateOfBirth = DateTime.SpecifyKind(new DateTime(1990, 1, 1), DateTimeKind.Utc),
                JoiningDate = DateTime.UtcNow,
                DepartmentId = adminDept.Id,
                Status = EmploymentStatus.Active,
                UserId = superAdmin.Id,
                CreatedAt = DateTime.UtcNow
            };

            await context.Employees.AddAsync(superAdminProfile);
            await context.SaveChangesAsync();
        }
    }
}

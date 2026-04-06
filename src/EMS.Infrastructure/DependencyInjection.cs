using EMS.Application.Common.Interfaces;
using EMS.Application.Modules.Attendance.Interfaces;
using EMS.Application.Modules.Attendance.Services;
using EMS.Application.Modules.Dashboard.Interfaces;
using EMS.Application.Modules.Employees.Interfaces;
using EMS.Application.Modules.Employees.Services;
using EMS.Application.Modules.Identity.Interfaces;
using EMS.Application.Modules.Identity.Services;
using EMS.Application.Modules.Leave.Interfaces;
using EMS.Application.Modules.Leave.Services;
using EMS.Application.Modules.Organization.Interfaces;
using EMS.Application.Modules.Organization.Services;
using EMS.Application.Modules.Payroll.Interfaces;
using EMS.Application.Modules.Payroll.Services;
using EMS.Application.Modules.Performance.Interfaces;
using EMS.Application.Modules.Performance.Services;
using EMS.Application.Modules.Reports.Interfaces;
using EMS.Infrastructure.BackgroundServices;  
using EMS.Infrastructure.Persistence;
using EMS.Infrastructure.Repositories;
using EMS.Infrastructure.Services;
using EMS.Infrastructure.Services.Dashboard;
using EMS.Infrastructure.Services.Reports;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // PostgreSQL
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("EMS.Infrastructure"));
            // Suppress pending model changes warning/make it non-fatal
            options.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        // UnitOfWork
        services.AddScoped<EMS.Infrastructure.UnitOfWork.UnitOfWork>();

        // ── Repositories ──────────────────────────────────────────
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>(); // NEW
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IDesignationRepository, DesignationRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<ILeaveRepository, LeaveRepository>();
        services.AddScoped<IPayrollRepository, PayrollRepository>();
        services.AddScoped<IPerformanceRepository, PerformanceRepository>();

        // ── Services ──────────────────────────────────────────────
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IDesignationService, DesignationService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<ILeaveService, LeaveService>();
        services.AddScoped<IPayrollService, PayrollService>();
        services.AddScoped<IPerformanceService, PerformanceService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();

        // ── Email Service ───────────────────────────────────────────
        services.AddScoped<IEmailService, EmailService>();

        // ── Background Services ───────────────────────────────────
        services.AddHostedService<TokenCleanupService>();

        // ✅ NEW — Auto absent marking
        services.AddHostedService<AutoAbsentMarkingService>();

        // ── JWT Authentication ────────────────────────────────────
        var secret = configuration["JwtSettings:Secret"]
            ?? throw new InvalidOperationException("JWT Secret is required.");
        var key = Encoding.UTF8.GetBytes(secret);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme =
                JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme =
                JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["JwtSettings:Issuer"],
                ValidAudience = configuration["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.FromMinutes(2)  // Small buffer for network clock differences
            };
        });

        return services;
    }
}
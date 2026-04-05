// src/EMS.Application/Modules/Employees/Services/EmployeeService.cs

using EMS.Application.Common.DTOs;
using EMS.Application.Common.Interfaces;
using EMS.Application.Modules.Employees.DTOs;
using EMS.Application.Modules.Employees.Interfaces;
using EMS.Application.Modules.Identity.Interfaces;
using EMS.Domain.Entities.Employee;
using EMS.Domain.Entities.Identity;
using EMS.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EMS.Application.Modules.Employees.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAuthRepository _authRepository;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(
        IEmployeeRepository employeeRepository,
        IAuthRepository authRepository,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<EmployeeService> logger)
    {
        _employeeRepository = employeeRepository;
        _authRepository = authRepository;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    // ── Get All ───────────────────────────────────────────────────────────────

    public async Task<PaginatedResult<EmployeeResponseDto>> GetAllAsync(EmployeeFilterDto filter)
    {
        var result = await _employeeRepository.GetAllAsync(filter);
        return new PaginatedResult<EmployeeResponseDto>
        {
            Data = result.Data.Select(MapToDto),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    // ── Get By Id ─────────────────────────────────────────────────────────────

    public async Task<EmployeeResponseDto?> GetByIdAsync(int id)
    {
        var emp = await _employeeRepository.GetByIdAsync(id);
        return emp == null ? null : MapToDto(emp);
    }

    // ── Create ────────────────────────────────────────────────────────────────

    public async Task<(EmployeeResponseDto? result, string? error)> CreateAsync(
        CreateEmployeeDto dto)
    {
        // ── Step 1: Email duplicate checks ───────────────────────────────────
        if (await _employeeRepository.EmailExistsAsync(dto.Email))
            return (null, "Email already registered as employee.");

        if (await _authRepository.EmailExistsAsync(dto.Email))
            return (null, "Email already registered as user.");

        // ── Step 2: Auto EmployeeCode generate ───────────────────────────────
        var totalCount = await _employeeRepository.GetTotalCountAsync();
        var employeeCode = $"EMP{(totalCount + 1):D3}";

        // ── Step 3: Password decide karo ─────────────────────────────────────
        var plainPassword = !string.IsNullOrWhiteSpace(dto.UserPassword)
            ? dto.UserPassword
            : "Welcome@123";

        // ── Step 4: AppUser create karo ───────────────────────────────────────
        var newUser = new AppUser
        {
            UserName = $"{dto.FirstName.Trim()} {dto.LastName.Trim()}",
            Email = dto.Email.ToLower().Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: 12),
            Role = UserRole.Employee,
            IsActive = true,
            IsEmailVerified = false,
            IsFirstLogin = true,
            CreatedAt = DateTime.UtcNow,
        };

        var createdUser = await _authRepository.CreateAsync(newUser);

        // ── Step 5: Save AppUser immediately so Id is generated ──────────────
        await _authRepository.SaveChangesAsync();

        // ── Step 6: EmployeeProfile create karo — ab createdUser.Id guaranteed real hai ──
        var employee = new EmployeeProfile
        {
            EmployeeCode = employeeCode,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.ToLower().Trim(),
            Phone = dto.Phone?.Trim() ?? string.Empty,
            Gender = dto.Gender,
            DateOfBirth = ToUtc(dto.DateOfBirth),
            JoiningDate = ToUtc(dto.JoiningDate),
            DepartmentId = dto.DepartmentId,
            DesignationId = dto.DesignationId,
            ReportingManagerId = dto.ReportingManagerId,
            Status = EmploymentStatus.Active,
            ProbationEndDate = dto.ProbationEndDate?.Date,
            UserId = createdUser.Id,
            CreatedAt = DateTime.UtcNow,
        };

        var created = await _employeeRepository.CreateAsync(employee);

        // ── Step 7: Welcome Email — fire and forget ───────────────────────────
        _ = SendWelcomeEmailSafelyAsync(
            toEmail: dto.Email,
            employeeName: $"{dto.FirstName.Trim()} {dto.LastName.Trim()}",
            temporaryPassword: plainPassword
        );

        return (MapToDto(created), null);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public async Task<(EmployeeResponseDto? result, string? error)> UpdateAsync(
        int id, UpdateEmployeeDto dto)
    {
        var existing = await _employeeRepository.GetByIdAsync(id);
        if (existing == null)
            return (null, "Employee not found.");

        if (existing.Email != dto.Email.ToLower().Trim() &&
            await _employeeRepository.EmailExistsAsync(dto.Email, excludeId: id))
            return (null, "Email already in use.");

        existing.FirstName = dto.FirstName.Trim();
        existing.LastName = dto.LastName.Trim();
        existing.Email = dto.Email.ToLower().Trim();
        existing.Phone = dto.Phone?.Trim() ?? string.Empty;
        existing.Gender = dto.Gender;
        existing.DateOfBirth = ToUtc(dto.DateOfBirth);
        existing.JoiningDate = ToUtc(dto.JoiningDate);
        existing.DepartmentId = dto.DepartmentId;
        existing.DesignationId = dto.DesignationId;
        existing.ReportingManagerId = dto.ReportingManagerId;
        existing.Status = dto.Status;
        existing.ProbationEndDate = dto.ProbationEndDate?.Date;
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _employeeRepository.UpdateAsync(id, existing);
        return updated == null ? (null, "Update failed.") : (MapToDto(updated), null);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    public async Task<(bool success, string? error)> DeleteAsync(int id)
    {
        var emp = await _employeeRepository.GetByIdAsync(id);
        if (emp == null) return (false, "Employee not found.");

        var deleted = await _employeeRepository.DeleteAsync(id);
        return deleted ? (true, null) : (false, "Delete failed.");
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Email send karo safely — exception swallow karta hai
    /// taaki employee creation fail na ho email issue ki wajah se
    /// </summary>
    private async Task SendWelcomeEmailSafelyAsync(
        string toEmail,
        string employeeName,
        string temporaryPassword)
    {
        try
        {
            var loginUrl = _configuration["AppSettings:FrontendUrl"]
                           ?? "http://localhost:8080";

            await _emailService.SendWelcomeEmailAsync(
                toEmail,
                employeeName,
                temporaryPassword,
                loginUrl
            );

            _logger.LogInformation(
                "[EmployeeService] Welcome email sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            // Email fail — log karo, employee creation crash nahi hoga
            _logger.LogError(ex,
                "[EmployeeService] Welcome email FAILED for {Email}. " +
                "Employee was created successfully.", toEmail);
        }
    }

    private static DateTime ToUtc(DateTime dt)
        => dt.Kind == DateTimeKind.Utc ? dt : dt.ToUniversalTime();

    private static EmployeeResponseDto MapToDto(EmployeeProfile e) => new()
    {
        Id = e.Id,
        EmployeeCode = e.EmployeeCode,
        FirstName = e.FirstName,
        LastName = e.LastName,
        Email = e.Email,
        Phone = e.Phone,
        Gender = e.Gender.ToString(),
        DateOfBirth = e.DateOfBirth,
        JoiningDate = e.JoiningDate,
        Status = e.Status.ToString(),
        ProfilePhotoUrl = e.ProfilePhotoUrl,
        DepartmentId = e.DepartmentId,
        DepartmentName = e.Department?.Name ?? "Unknown",
        DesignationId = e.DesignationId,
        DesignationTitle = e.Designation?.Title,
        ProbationEndDate = e.ProbationEndDate,
        ReportingManagerId = e.ReportingManagerId,
        ReportingManagerName = e.ReportingManager != null
            ? $"{e.ReportingManager.FirstName} {e.ReportingManager.LastName}"
            : null,
        UserId = e.UserId,
        CreatedAt = e.CreatedAt
    };
}
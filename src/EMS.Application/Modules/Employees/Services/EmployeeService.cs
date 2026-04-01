using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Employees.DTOs;
using EMS.Application.Modules.Employees.Interfaces;
using EMS.Domain.Entities.Employee;
using EMS.Domain.Enums;

namespace EMS.Application.Modules.Employees.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

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

    public async Task<EmployeeResponseDto?> GetByIdAsync(int id)
    {
        var emp = await _employeeRepository.GetByIdAsync(id);
        return emp == null ? null : MapToDto(emp);
    }

    public async Task<(EmployeeResponseDto? result, string? error)> CreateAsync(
        CreateEmployeeDto dto)
    {
        // Email unique check
        if (await _employeeRepository.EmailExistsAsync(dto.Email))
            return (null, "Email already registered.");

        // Auto EmployeeCode generate — EMP001, EMP002...
        var totalCount = await _employeeRepository.GetTotalCountAsync();
        var employeeCode = $"EMP{(totalCount + 1):D3}";  // EMP001 format

        var employee = new EmployeeProfile
        {
            EmployeeCode = employeeCode,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.ToLower().Trim(),
            Phone = dto.Phone.Trim(),
            Gender = dto.Gender,
            DateOfBirth = DateTime.SpecifyKind(dto.DateOfBirth, DateTimeKind.Utc),
            JoiningDate = DateTime.SpecifyKind(dto.JoiningDate, DateTimeKind.Utc),
            DepartmentId = dto.DepartmentId,
            DesignationId = dto.DesignationId,
            ReportingManagerId = dto.ReportingManagerId,
            Status = EmploymentStatus.Active
        };

        var created = await _employeeRepository.CreateAsync(employee);
        return (MapToDto(created), null);
    }

    public async Task<(EmployeeResponseDto? result, string? error)> UpdateAsync(
        int id, UpdateEmployeeDto dto)
    {
        var existing = await _employeeRepository.GetByIdAsync(id);
        if (existing == null)
            return (null, "Employee not found.");

        // Email change ho rahi hai toh check karo
        if (existing.Email != dto.Email.ToLower().Trim() &&
            await _employeeRepository.EmailExistsAsync(dto.Email, excludeId: id))
            return (null, "Email already in use.");

        existing.FirstName = dto.FirstName.Trim();
        existing.LastName = dto.LastName.Trim();
        existing.Email = dto.Email.ToLower().Trim();
        existing.Phone = dto.Phone.Trim();
        existing.Gender = dto.Gender;
        existing.DateOfBirth = DateTime.SpecifyKind(dto.DateOfBirth, DateTimeKind.Utc);
        existing.JoiningDate = DateTime.SpecifyKind(dto.JoiningDate, DateTimeKind.Utc);
        existing.DepartmentId = dto.DepartmentId;
        existing.DesignationId = dto.DesignationId;
        existing.ReportingManagerId = dto.ReportingManagerId;
        existing.Status = dto.Status;
        existing.UserId = dto.UserId;  // ✅ NEW: Link employee to user account
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _employeeRepository.UpdateAsync(id, existing);
        return updated == null ? (null, "Update failed.") : (MapToDto(updated), null);
    }

    public async Task<(bool success, string? error)> DeleteAsync(int id)
    {
        var emp = await _employeeRepository.GetByIdAsync(id);
        if (emp == null) return (false, "Employee not found.");

        var deleted = await _employeeRepository.DeleteAsync(id);
        return deleted ? (true, null) : (false, "Delete failed.");
    }

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
        ReportingManagerId = e.ReportingManagerId,
        ReportingManagerName = e.ReportingManager != null
            ? $"{e.ReportingManager.FirstName} {e.ReportingManager.LastName}"
            : null,
        UserId = e.UserId,
        CreatedAt = e.CreatedAt
    };
}
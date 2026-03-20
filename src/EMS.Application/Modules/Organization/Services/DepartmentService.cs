using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Organization.DTOs;
using EMS.Application.Modules.Organization.Interfaces;
using EMS.Domain.Entities.Organization;

namespace EMS.Application.Modules.Organization.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;

    public DepartmentService(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<PaginatedResult<DepartmentResponseDto>> GetAllAsync(
        int page, int pageSize, string? search)
    {
        var result = await _departmentRepository.GetAllAsync(page, pageSize, search);
        return new PaginatedResult<DepartmentResponseDto>
        {
            Data = result.Data.Select(MapToDto),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<DepartmentResponseDto?> GetByIdAsync(int id)
    {
        var dept = await _departmentRepository.GetByIdAsync(id);
        return dept == null ? null : MapToDto(dept);
    }

    public async Task<(DepartmentResponseDto? result, string? error)> CreateAsync(
        CreateDepartmentDto dto)
    {
        // Name unique check
        if (await _departmentRepository.NameExistsAsync(dto.Name))
            return (null, "Department name already exists.");

        var dept = new Department
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Code = dto.Code?.ToUpper().Trim(),
            IsActive = true
        };

        var created = await _departmentRepository.CreateAsync(dept);
        return (MapToDto(created), null);
    }

    public async Task<(DepartmentResponseDto? result, string? error)> UpdateAsync(
        int id, UpdateDepartmentDto dto)
    {
        if (!await _departmentRepository.ExistsAsync(id))
            return (null, "Department not found.");

        if (await _departmentRepository.NameExistsAsync(dto.Name, excludeId: id))
            return (null, "Department name already taken.");

        var dept = new Department
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Code = dto.Code?.ToUpper().Trim(),
            IsActive = dto.IsActive
        };

        var updated = await _departmentRepository.UpdateAsync(id, dept);
        return updated == null ? (null, "Update failed.") : (MapToDto(updated), null);
    }

    public async Task<(bool success, string? error)> DeleteAsync(int id)
    {
        var dept = await _departmentRepository.GetByIdAsync(id);
        if (dept == null) return (false, "Department not found.");

        if (dept.Employees.Any())
            return (false, "Cannot delete department with active employees.");

        var deleted = await _departmentRepository.DeleteAsync(id);
        return deleted ? (true, null) : (false, "Delete failed.");
    }

    private static DepartmentResponseDto MapToDto(Department d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        Description = d.Description,
        Code = d.Code,
        IsActive = d.IsActive,
        EmployeeCount = d.Employees?.Count ?? 0,
        CreatedAt = d.CreatedAt
    };
}
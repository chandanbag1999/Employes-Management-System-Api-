using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Organization.DTOs;

namespace EMS.Application.Modules.Organization.Interfaces;

public interface IDepartmentService
{
    Task<PaginatedResult<DepartmentResponseDto>> GetAllAsync(int page, int pageSize, string? search);
    Task<DepartmentResponseDto?> GetByIdAsync(int id);
    Task<(DepartmentResponseDto? result, string? error)> CreateAsync(CreateDepartmentDto dto);
    Task<(DepartmentResponseDto? result, string? error)> UpdateAsync(int id, UpdateDepartmentDto dto);
    Task<(bool success, string? error)> DeleteAsync(int id);
}
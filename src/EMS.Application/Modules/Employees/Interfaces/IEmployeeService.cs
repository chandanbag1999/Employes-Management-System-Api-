using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Employees.DTOs;

namespace EMS.Application.Modules.Employees.Interfaces;

public interface IEmployeeService
{
    Task<PaginatedResult<EmployeeResponseDto>> GetAllAsync(EmployeeFilterDto filter);
    Task<EmployeeResponseDto?> GetByIdAsync(int id);
    Task<(EmployeeResponseDto? result, string? error)> CreateAsync(CreateEmployeeDto dto);
    Task<(EmployeeResponseDto? result, string? error)> UpdateAsync(int id, UpdateEmployeeDto dto);
    Task<(bool success, string? error)> DeleteAsync(int id);
}
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Employees.DTOs;
using EMS.Domain.Entities.Employee;

namespace EMS.Application.Modules.Employees.Interfaces;

public interface IEmployeeRepository
{
    Task<PaginatedResult<EmployeeProfile>> GetAllAsync(EmployeeFilterDto filter);
    Task<EmployeeProfile?> GetByIdAsync(int id);
    Task<EmployeeProfile?> GetByEmailAsync(string email);
    Task<EmployeeProfile> CreateAsync(EmployeeProfile employee);
    Task<EmployeeProfile?> UpdateAsync(int id, EmployeeProfile employee);
    Task<bool> DeleteAsync(int id);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
    Task<int> GetTotalCountAsync();   // EmployeeCode generate karne ke liye
}
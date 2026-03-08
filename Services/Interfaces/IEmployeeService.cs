using EmployesManagementSystemApi.DTOs.Employee;

namespace EmployesManagementSystemApi.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeResponseDto>> GetAllAsync();
        Task<EmployeeResponseDto?> GetByIdAsync(int id);
        Task<IEnumerable<EmployeeResponseDto>> GetByDepartmentAsync(int departmentId);
        Task<EmployeeResponseDto> CreateAsync(CreateEmployeeDto dto);
        Task<EmployeeResponseDto?> UpdateAsync(int id, UpdateEmployeeDto dto);
        Task<bool> DeleteAsync(int id);
    }
}

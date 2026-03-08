using EmployesManagementSystemApi.DTOs.Department;

namespace EmployesManagementSystemApi.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentResponseDto>> GetAllAsync();
        Task<DepartmentResponseDto?> GetByIdAsync(int id);
        Task<DepartmentResponseDto> CreateAsync(CreateDepartmentDto dto);
        Task<DepartmentResponseDto?> UpdateAsync(int id, CreateDepartmentDto dto);
        Task<bool> DeleteAsync(int id);
    }
}

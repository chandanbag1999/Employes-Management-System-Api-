using EmployesManagementSystemApi.Models;

namespace EmployesManagementSystemApi.Repositories.Interfaces
{
    public interface IDepartmentRepository
    {
        Task<IEnumerable<Department>> GetAllAsync();
        Task<Department?> GetByIdAsync(int id);
        Task<Department> CreateAsync(Department  department);
        Task<Department?> UpdateAsync(int id, Department department);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
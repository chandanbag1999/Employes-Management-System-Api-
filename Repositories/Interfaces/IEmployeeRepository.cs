using EmployesManagementSystemApi.Models;

namespace EmployesManagementSystemApi.Repositories.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(int id);
        Task<IEnumerable<Employee>> GetByDepartmentAsync(int departmentId);
        Task<Employee> CreateAsync(Employee employee);
        Task<Employee?> UpdateAsync(int id, Employee employee);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}

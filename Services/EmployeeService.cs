using EmployesManagementSystemApi.DTOs.Employee;
using EmployesManagementSystemApi.Models;
using EmployesManagementSystemApi.Repositories.Interfaces;
using EmployesManagementSystemApi.Services.Interfaces;

namespace EmployesManagementSystemApi.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<IEnumerable<EmployeeResponseDto>> GetAllAsync()
        {
            var employees = await _employeeRepository.GetAllAsync();
            return employees.Select(e => MapToDto(e));
        }

        public async Task<EmployeeResponseDto?> GetByIdAsync(int id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null) return null;
            return MapToDto(employee);
        }

        public async Task<IEnumerable<EmployeeResponseDto>> GetByDepartmentAsync(int departmentId)
        {
            var employees = await _employeeRepository.GetByDepartmentAsync(departmentId);
            return employees.Select(e => MapToDto(e));
        }

        public async Task<EmployeeResponseDto> CreateAsync(CreateEmployeeDto dto)
        {
            var employee = new Employee
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                Position = dto.Position,
                Salary = dto.Salary,
                HireDate = DateTime.SpecifyKind(dto.HireDate, DateTimeKind.Utc),
                DepartmentId = dto.DepartmentId,
                IsActive = true
            };

            var created = await _employeeRepository.CreateAsync(employee);

            // Reload with Department name
            var withDept = await _employeeRepository.GetByIdAsync(created.Id);
            return MapToDto(withDept!);
        }

        public async Task<EmployeeResponseDto?> UpdateAsync(int id, UpdateEmployeeDto dto)
        {
            var employee = new Employee
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                Position = dto.Position,
                Salary = dto.Salary,
                DepartmentId = dto.DepartmentId,
                IsActive = dto.IsActive
            };

            var updated = await _employeeRepository.UpdateAsync(id, employee);
            if (updated == null) return null;

            var withDept = await _employeeRepository.GetByIdAsync(updated.Id);
            return MapToDto(withDept!);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _employeeRepository.DeleteAsync(id);
        }

        private EmployeeResponseDto MapToDto(Employee e)
        {
            return new EmployeeResponseDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                Phone = e.Phone,
                Position = e.Position,
                Salary = e.Salary,
                HireDate = e.HireDate,
                IsActive = e.IsActive,
                DepartmentName = e.Department?.Name ?? "Unknown"
            };
        }
    }
}

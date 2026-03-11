using EmployesManagementSystemApi.DTOs.Department;
using EmployesManagementSystemApi.Models;
using EmployesManagementSystemApi.Repositories.Interfaces;
using EmployesManagementSystemApi.Services.Interfaces;

namespace EmployesManagementSystemApi.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;

        public DepartmentService(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        public async Task<IEnumerable<DepartmentResponseDto>> GetAllAsync()
        {
            var departments = await _departmentRepository.GetAllAsync();
            return departments.Select(d => MapToDto(d));
        }

        public async Task<DepartmentResponseDto?> GetByIdAsync(int id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null) return null;
            return MapToDto(department);
        }

        public async Task<DepartmentResponseDto> CreateAsync(CreateDepartmentDto dto)
        {
            var department = new Department
            {
                Name = dto.Name,
                Description = dto.Description
            };

            var created = await _departmentRepository.CreateAsync(department);
            return MapToDto(created);
        }

        public async Task<DepartmentResponseDto?> UpdateAsync(int id, CreateDepartmentDto dto)
        {
            var department = new Department
            {
                Name = dto.Name,
                Description = dto.Description
            };

            var updated = await _departmentRepository.UpdateAsync(id, department);
            if (updated == null) return null;
            return MapToDto(updated);
        }

        public async Task<DepartmentDeleteStatus> DeleteAsync(int id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null)
                return DepartmentDeleteStatus.NotFound;

            if (department.Employees.Any())
                return DepartmentDeleteStatus.HasEmployees;

            var deleted = await _departmentRepository.DeleteAsync(id);
            return deleted ? DepartmentDeleteStatus.Success : DepartmentDeleteStatus.NotFound;
        }


        // Private helper — maps Model to DTO
        private DepartmentResponseDto MapToDto(Department d)
        {
            return new DepartmentResponseDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                EmployeeCount = d.Employees?.Count ?? 0
            };
        }
    }
}

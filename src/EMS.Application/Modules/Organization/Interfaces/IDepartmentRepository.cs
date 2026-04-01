using EMS.Application.Common.DTOs;
using EMS.Domain.Entities.Organization;

namespace EMS.Application.Modules.Organization.Interfaces;

public interface IDepartmentRepository
{
    Task<PaginatedResult<Department>> GetAllAsync(int page, int pageSize, string? search);
    Task<Department?> GetByIdAsync(int id);
    Task<Department> CreateAsync(Department department);
    Task<Department?> UpdateAsync(int id, Department department);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> NameExistsAsync(string name, int? excludeId = null);

    // ── Soft Delete Management ──────────────────────────────────────
    Task<IEnumerable<Department>> GetDeletedAsync();
    Task<bool> RestoreAsync(int id);
    Task<int> PurgeAsync(int months);
}
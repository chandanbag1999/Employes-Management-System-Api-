using EMS.Domain.Entities.Organization;

namespace EMS.Application.Modules.Organization.Interfaces;

public interface IDesignationRepository
{
    Task<IEnumerable<Designation>> GetAllAsync(int? departmentId);
    Task<IEnumerable<Designation>> GetAllDeletedAsync();
    Task<Designation?> GetByIdAsync(int id);
    Task<Designation> CreateAsync(Designation designation);
    Task<Designation?> UpdateAsync(int id, Designation designation);
    Task<bool> DeleteAsync(int id);
    Task<bool> RestoreAsync(int id);
    Task<int> PurgeOldDeletedAsync(int olderThanMonths = 12);
}

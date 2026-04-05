using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Organization.DTOs;

namespace EMS.Application.Modules.Organization.Interfaces;

public interface IDesignationService
{
    Task<PaginatedResult<DesignationResponseDto>> GetAllAsync(int page, int pageSize, int? departmentId = null);
    Task<IEnumerable<DesignationResponseDto>> GetAllDeletedAsync();
    Task<DesignationResponseDto?> GetByIdAsync(int id);
    Task<DesignationResponseDto> CreateAsync(CreateDesignationDto dto);
    Task<DesignationResponseDto?> UpdateAsync(int id, CreateDesignationDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> RestoreAsync(int id);
    Task<int> PurgeOldDeletedAsync(int olderThanMonths = 12);
}

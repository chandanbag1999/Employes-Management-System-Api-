using EMS.Application.Modules.Organization.DTOs;

namespace EMS.Application.Modules.Organization.Interfaces;

public interface IDesignationService
{
    Task<IEnumerable<DesignationResponseDto>> GetAllAsync(int? departmentId);
    Task<DesignationResponseDto?> GetByIdAsync(int id);
    Task<DesignationResponseDto> CreateAsync(CreateDesignationDto dto);
    Task<DesignationResponseDto?> UpdateAsync(int id, CreateDesignationDto dto);
    Task<bool> DeleteAsync(int id);
}
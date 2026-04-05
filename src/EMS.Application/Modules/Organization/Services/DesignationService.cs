using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Organization.DTOs;
using EMS.Application.Modules.Organization.Interfaces;
using EMS.Domain.Entities.Organization;

namespace EMS.Application.Modules.Organization.Services;

public class DesignationService : IDesignationService
{
    private readonly IDesignationRepository _repo;

    public DesignationService(IDesignationRepository repo)
    {
        _repo = repo;
    }

    public async Task<PaginatedResult<DesignationResponseDto>> GetAllAsync(
        int page, int pageSize, int? departmentId = null)
    {
        var result = await _repo.GetAllAsync(page, pageSize, departmentId);
        return new PaginatedResult<DesignationResponseDto>
        {
            Data = result.Data.Select(MapToDto),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<IEnumerable<DesignationResponseDto>> GetAllDeletedAsync()
    {
        var list = await _repo.GetAllDeletedAsync();
        return list.Select(MapToDto);
    }

    public async Task<DesignationResponseDto?> GetByIdAsync(int id)
    {
        var d = await _repo.GetByIdAsync(id);
        return d == null ? null : MapToDto(d);
    }

    public async Task<DesignationResponseDto> CreateAsync(CreateDesignationDto dto)
    {
        var designation = new Designation
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            DepartmentId = dto.DepartmentId
        };
        var created = await _repo.CreateAsync(designation);
        return MapToDto(created);
    }

    public async Task<DesignationResponseDto?> UpdateAsync(int id, CreateDesignationDto dto)
    {
        var designation = new Designation
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            DepartmentId = dto.DepartmentId
        };
        var updated = await _repo.UpdateAsync(id, designation);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
        => await _repo.DeleteAsync(id);

    public async Task<bool> RestoreAsync(int id)
        => await _repo.RestoreAsync(id);

    public async Task<int> PurgeOldDeletedAsync(int olderThanMonths = 12)
        => await _repo.PurgeOldDeletedAsync(olderThanMonths);

    private static DesignationResponseDto MapToDto(Designation d) => new()
    {
        Id = d.Id,
        Title = d.Title,
        Description = d.Description,
        DepartmentId = d.DepartmentId,
        DepartmentName = d.Department?.Name,
        CreatedAt = d.CreatedAt
    };
}

using EMS.Application.Modules.Dashboard.DTOs;

namespace EMS.Application.Modules.Dashboard.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync();
    Task<IEnumerable<DepartmentHeadcountDto>> GetDepartmentHeadcountAsync();
    Task<IEnumerable<RecentActivityDto>> GetRecentActivitiesAsync(int count = 10);
}
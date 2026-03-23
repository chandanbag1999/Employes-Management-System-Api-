using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Dashboard.DTOs;
using EMS.Application.Modules.Dashboard.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMS.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "SuperAdmin,HRAdmin,Manager")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    // GET api/v1/dashboard/stats
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _service.GetStatsAsync();
        return Ok(ApiResponse<DashboardStatsDto>.Ok(result));
    }

    // GET api/v1/dashboard/headcount
    [HttpGet("headcount")]
    public async Task<IActionResult> GetHeadcount()
    {
        var result = await _service.GetDepartmentHeadcountAsync();
        return Ok(ApiResponse<IEnumerable<DepartmentHeadcountDto>>.Ok(result));
    }

    // GET api/v1/dashboard/activities?count=10
    [HttpGet("activities")]
    public async Task<IActionResult> GetActivities([FromQuery] int count = 10)
    {
        var result = await _service.GetRecentActivitiesAsync(count);
        return Ok(ApiResponse<IEnumerable<RecentActivityDto>>.Ok(result));
    }
}
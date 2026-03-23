using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Reports.DTOs;
using EMS.Application.Modules.Reports.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMS.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "SuperAdmin,HRAdmin")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;

    public ReportsController(IReportService service)
    {
        _service = service;
    }

    // GET api/v1/reports/attendance?month=3&year=2026&departmentId=1
    [HttpGet("attendance")]
    public async Task<IActionResult> GetAttendanceReport(
        [FromQuery] int month,
        [FromQuery] int year,
        [FromQuery] int? departmentId = null)
    {
        if (month < 1 || month > 12)
            return BadRequest(ApiResponse<string>.Fail("Invalid month."));

        var result = await _service.GetAttendanceReportAsync(
            month, year, departmentId);
        return Ok(ApiResponse<IEnumerable<AttendanceReportDto>>.Ok(result));
    }

    // GET api/v1/reports/payroll?month=3&year=2026
    [HttpGet("payroll")]
    public async Task<IActionResult> GetPayrollReport(
        [FromQuery] int month,
        [FromQuery] int year,
        [FromQuery] int? departmentId = null)
    {
        if (month < 1 || month > 12)
            return BadRequest(ApiResponse<string>.Fail("Invalid month."));

        var result = await _service.GetPayrollReportAsync(
            month, year, departmentId);
        return Ok(ApiResponse<IEnumerable<PayrollReportDto>>.Ok(result));
    }

    // GET api/v1/reports/headcount
    [HttpGet("headcount")]
    public async Task<IActionResult> GetHeadcountReport()
    {
        var result = await _service.GetHeadcountReportAsync();
        return Ok(ApiResponse<IEnumerable<HeadcountReportDto>>.Ok(result));
    }
}
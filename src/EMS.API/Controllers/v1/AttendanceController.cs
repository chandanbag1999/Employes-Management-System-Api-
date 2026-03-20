using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Attendance.DTOs;
using EMS.Application.Modules.Attendance.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMS.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _service;

    public AttendanceController(IAttendanceService service)
    {
        _service = service;
    }

    // POST api/v1/attendance/clock-in
    [HttpPost("clock-in")]
    public async Task<IActionResult> ClockIn([FromBody] ClockInDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (result, error) = await _service.ClockInAsync(dto);
        if (error != null)
            return BadRequest(ApiResponse<string>.Fail(error));

        return Ok(ApiResponse<AttendanceResponseDto>.Ok(result!, "Clocked in successfully."));
    }

    // POST api/v1/attendance/clock-out
    [HttpPost("clock-out")]
    public async Task<IActionResult> ClockOut([FromBody] ClockOutDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (result, error) = await _service.ClockOutAsync(dto);
        if (error != null)
            return BadRequest(ApiResponse<string>.Fail(error));

        return Ok(ApiResponse<AttendanceResponseDto>.Ok(result!, "Clocked out successfully."));
    }

    // GET api/v1/attendance?employeeId=1&fromDate=2026-03-01&toDate=2026-03-31
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,HRAdmin,Manager")]
    public async Task<IActionResult> GetAll([FromQuery] AttendanceFilterDto filter)
    {
        var result = await _service.GetAllAsync(filter);
        return Ok(ApiResponse<PaginatedResult<AttendanceResponseDto>>.Ok(result));
    }

    // GET api/v1/attendance/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("Record not found."));
        return Ok(ApiResponse<AttendanceResponseDto>.Ok(result));
    }

    // GET api/v1/attendance/today/5
    [HttpGet("today/{employeeId}")]
    public async Task<IActionResult> GetToday(int employeeId)
    {
        var result = await _service.GetTodayRecordAsync(employeeId);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("No attendance record for today."));
        return Ok(ApiResponse<AttendanceResponseDto>.Ok(result));
    }

    // GET api/v1/attendance/summary/5?month=3&year=2026
    [HttpGet("summary/{employeeId}")]
    public async Task<IActionResult> GetMonthlySummary(
        int employeeId,
        [FromQuery] int month,
        [FromQuery] int year)
    {
        if (month < 1 || month > 12)
            return BadRequest(ApiResponse<string>.Fail("Invalid month."));

        var result = await _service.GetMonthlySummaryAsync(employeeId, month, year);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("No records found."));

        return Ok(ApiResponse<MonthlyAttendanceSummaryDto>.Ok(result));
    }

    // POST api/v1/attendance/manual  (Admin only)
    [HttpPost("manual")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> MarkManual([FromBody] ManualAttendanceDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (result, error) = await _service.MarkManualAsync(dto);
        if (error != null)
            return BadRequest(ApiResponse<string>.Fail(error));

        return Ok(ApiResponse<AttendanceResponseDto>.Ok(
            result!, "Attendance marked manually."));
    }
}
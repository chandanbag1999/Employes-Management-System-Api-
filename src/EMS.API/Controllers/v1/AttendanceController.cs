using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Attendance.DTOs;
using EMS.Application.Modules.Attendance.Interfaces;
using EMS.Application.Modules.Employees.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EMS.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _service;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly ILogger<AttendanceController> _logger;

    public AttendanceController(
        IAttendanceService service,
        IEmployeeRepository employeeRepo,
        ILogger<AttendanceController> logger)
    {
        _service = service;
        _employeeRepo = employeeRepo;
        _logger = logger;
    }

    // ✅ FIXED: ClaimTypes.NameIdentifier use karo — "sub" nahi
    private async Task<(int employeeId, string? error)> GetEmployeeIdFromJwtAsync()
    {
        // JWT mein ClaimTypes.NameIdentifier = user.Id
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        _logger.LogInformation("[Attendance] JWT ClaimTypes.NameIdentifier = '{userIdClaim}'", userIdClaim ?? "NULL");

        if (string.IsNullOrEmpty(userIdClaim) ||
            !int.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("[Attendance] Invalid or missing userId claim. Value: '{userIdClaim}'", userIdClaim);
            return (0, "Invalid token. Please login again.");
        }

        // UserId se linked EmployeeProfile nikalo
        var employeeId = await _employeeRepo.GetIdByUserIdAsync(userId);

        _logger.LogInformation("[Attendance] userId={userId} → employeeId={employeeId}", userId, employeeId?.ToString() ?? "NULL");

        if (employeeId == null)
        {
            _logger.LogWarning("[Attendance] No employee profile linked for userId={userId}", userId);
            return (0,
                $"No employee profile linked to your account (userId={userId}). " +
                "Please contact HR to link your profile.");
        }

        return (employeeId.Value, null);
    }

    // POST api/v1/attendance/clock-in
    [HttpPost("clock-in")]
    public async Task<IActionResult> ClockIn([FromBody] ClockInDto dto)
    {
        _logger.LogInformation("[Attendance] POST /clock-in");
        var (employeeId, err) = await GetEmployeeIdFromJwtAsync();
        if (err != null)
            return BadRequest(ApiResponse<string>.Fail(err));

        var (result, error) = await _service.ClockInAsync(dto, employeeId);
        if (error != null)
            return BadRequest(ApiResponse<string>.Fail(error));

        return Ok(ApiResponse<AttendanceResponseDto>.Ok(
            result!, "Clocked in successfully."));
    }

    // POST api/v1/attendance/clock-out
    [HttpPost("clock-out")]
    public async Task<IActionResult> ClockOut([FromBody] ClockOutDto dto)
    {
        _logger.LogInformation("[Attendance] POST /clock-out");
        var (employeeId, err) = await GetEmployeeIdFromJwtAsync();
        if (err != null)
            return BadRequest(ApiResponse<string>.Fail(err));

        var (result, error) = await _service.ClockOutAsync(dto, employeeId);
        if (error != null)
            return BadRequest(ApiResponse<string>.Fail(error));

        return Ok(ApiResponse<AttendanceResponseDto>.Ok(
            result!, "Clocked out successfully."));
    }

    // GET api/v1/attendance/my/today
    [HttpGet("my/today")]
    public async Task<IActionResult> GetMyToday()
    {
        _logger.LogInformation("[Attendance] GET /my/today");
        var (employeeId, err) = await GetEmployeeIdFromJwtAsync();
        if (err != null)
            return BadRequest(ApiResponse<string>.Fail(err));

        var result = await _service.GetTodayRecordAsync(employeeId);
        return Ok(ApiResponse<AttendanceResponseDto?>.Ok(result));
    }

    // GET api/v1/attendance/my/summary?month=4&year=2026
    [HttpGet("my/summary")]
    public async Task<IActionResult> GetMySummary(
        [FromQuery] int month,
        [FromQuery] int year)
    {
        _logger.LogInformation("[Attendance] GET /my/summary — month={month}, year={year}", month, year);
        if (month < 1 || month > 12)
            return BadRequest(ApiResponse<string>.Fail("Invalid month."));

        var (employeeId, err) = await GetEmployeeIdFromJwtAsync();
        if (err != null)
            return BadRequest(ApiResponse<string>.Fail(err));

        var result = await _service.GetMonthlySummaryAsync(
            employeeId, month, year);
        return Ok(ApiResponse<MonthlyAttendanceSummaryDto?>.Ok(result));
    }

    // GET api/v1/attendance — Admin/Manager
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,HRAdmin,Manager")]
    public async Task<IActionResult> GetAll([FromQuery] AttendanceFilterDto filter)
    {
        var result = await _service.GetAllAsync(filter);
        return Ok(ApiResponse<PaginatedResult<AttendanceResponseDto>>.Ok(result));
    }

    // GET api/v1/attendance/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("Record not found."));
        return Ok(ApiResponse<AttendanceResponseDto>.Ok(result));
    }

    // GET api/v1/attendance/today/{employeeId} — Admin specific employee
    [HttpGet("today/{employeeId:int}")]
    [Authorize(Roles = "SuperAdmin,HRAdmin,Manager")]
    public async Task<IActionResult> GetToday(int employeeId)
    {
        var result = await _service.GetTodayRecordAsync(employeeId);
        return Ok(ApiResponse<AttendanceResponseDto?>.Ok(result));
    }

    // GET api/v1/attendance/summary/{employeeId}
    [HttpGet("summary/{employeeId:int}")]
    public async Task<IActionResult> GetMonthlySummary(
        int employeeId,
        [FromQuery] int month,
        [FromQuery] int year)
    {
        if (month < 1 || month > 12)
            return BadRequest(ApiResponse<string>.Fail("Invalid month."));

        var result = await _service.GetMonthlySummaryAsync(
            employeeId, month, year);
        return Ok(ApiResponse<MonthlyAttendanceSummaryDto?>.Ok(result));
    }

    // POST api/v1/attendance/manual — Admin only
    [HttpPost("manual")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> MarkManual([FromBody] ManualAttendanceDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (result, error) = await _service.MarkManualAsync(dto);
        if (error != null)
            return BadRequest(ApiResponse<string>.Fail(error));

        return Ok(ApiResponse<AttendanceResponseDto>.Ok(
            result!, "Attendance marked manually."));
    }
}
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Leave.DTOs;
using EMS.Application.Modules.Leave.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EMS.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class LeaveController : ControllerBase
{
    private readonly ILeaveService _service;

    public LeaveController(ILeaveService service)
    {
        _service = service;
    }

    private async Task<int?> GetEmployeeIdFromJwtAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return null;

        // EmployeeProfile linked hai?
        var employeeRepo = HttpContext.RequestServices.GetRequiredService<EMS.Application.Modules.Employees.Interfaces.IEmployeeRepository>();
        return await employeeRepo.GetIdByUserIdAsync(userId);
    }

    // GET api/v1/leave?status=Pending&employeeId=1
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] LeaveFilterDto filter)
    {
        var result = await _service.GetAllAsync(filter);
        return Ok(ApiResponse<PaginatedResult<LeaveResponseDto>>.Ok(result));
    }

    // GET api/v1/leave/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("Leave application not found."));
        return Ok(ApiResponse<LeaveResponseDto>.Ok(result));
    }

    // POST api/v1/leave/apply
    [HttpPost("apply")]
    public async Task<IActionResult> Apply([FromBody] ApplyLeaveDto dto)
    {
        var employeeId = await GetEmployeeIdFromJwtAsync();
        if (employeeId == null)
            return UnprocessableEntity(ApiResponse<string>.Fail(
                "Your account is not linked to an employee profile."));

        // EmployeeId JWT se inject karo
        dto.EmployeeId = employeeId.Value;

        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (result, error) = await _service.ApplyAsync(dto);
        if (error != null)
            return BadRequest(ApiResponse<string>.Fail(error));

        return CreatedAtAction(nameof(GetById),
            new { id = result!.Id },
            ApiResponse<LeaveResponseDto>.Ok(result, "Leave applied successfully."));
    }

    // PATCH api/v1/leave/5/action
    [HttpPatch("{id}/action")]
    [Authorize(Roles = "SuperAdmin,HRAdmin,Manager")]
    public async Task<IActionResult> ApproveOrReject(
        int id, [FromBody] LeaveActionDto dto)
    {
        // ✅ Auto-resolve actionById from JWT agar 0 ho
        if (dto.ActionById == 0)
        {
            var approverEmployeeId = await GetEmployeeIdFromJwtAsync();
            if (approverEmployeeId == null)
                return UnprocessableEntity(ApiResponse<string>.Fail(
                    "Your account is not linked to an employee profile."));
            dto.ActionById = approverEmployeeId.Value;
        }

        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (result, error) = await _service.ApproveOrRejectAsync(id, dto);
        if (error != null)
            return error.Contains("not found")
                ? NotFound(ApiResponse<string>.Fail(error))
                : BadRequest(ApiResponse<string>.Fail(error));

        var msg = result!.Status == "Approved"
            ? "Leave approved." : "Leave rejected.";
        return Ok(ApiResponse<LeaveResponseDto>.Ok(result, msg));
    }

    // PATCH api/v1/leave/5/cancel
    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id, [FromQuery] int employeeId)
    {
        var (success, error) = await _service.CancelAsync(id, employeeId);
        if (!success)
            return error!.Contains("not found")
                ? NotFound(ApiResponse<string>.Fail(error))
                : BadRequest(ApiResponse<string>.Fail(error));

        return Ok(ApiResponse<string>.Ok("Cancelled", "Leave cancelled."));
    }

    // GET api/v1/leave/balance/5?year=2026
    [HttpGet("balance/{employeeId}")]
    public async Task<IActionResult> GetBalance(
        int employeeId, [FromQuery] int year = 0)
    {
        if (year == 0) year = DateTime.Today.Year;

        var result = await _service.GetBalanceAsync(employeeId, year);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("No data found."));

        return Ok(ApiResponse<LeaveBalanceDto>.Ok(result));
    }

    // GET api/v1/leave/my/balance/{employeeId} — Employee self-service
    [HttpGet("my/balance/{employeeId}")]
    public async Task<IActionResult> GetMyBalance(int employeeId, [FromQuery] int year = 0)
    {
        // Verify the employee is the logged-in user
        var currentEmployeeId = await GetEmployeeIdFromJwtAsync();
        if (currentEmployeeId != employeeId)
            return Forbid();

        if (year == 0) year = DateTime.Today.Year;

        var result = await _service.GetBalanceAsync(employeeId, year);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("No data found."));

        return Ok(ApiResponse<LeaveBalanceDto>.Ok(result));
    }

    // GET api/v1/leave/types
    [HttpGet("types")]
    public async Task<IActionResult> GetLeaveTypes()
    {
        var result = await _service.GetAllLeaveTypesAsync();
        return Ok(ApiResponse<IEnumerable<LeaveTypeResponseDto>>.Ok(result));
    }

    // POST api/v1/leave/types
    [HttpPost("types")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> CreateLeaveType([FromBody] CreateLeaveTypeDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.CreateLeaveTypeAsync(dto);
        return Ok(ApiResponse<LeaveTypeResponseDto>.Ok(result, "Leave type created."));
    }
}
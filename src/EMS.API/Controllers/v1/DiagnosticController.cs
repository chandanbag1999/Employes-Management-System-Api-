using EMS.Application.Modules.Attendance.Interfaces;
using EMS.Application.Modules.Employees.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EMS.API.Controllers.v1;

/// <summary>
/// Diagnostic endpoints for troubleshooting employee-user linkage issues.
/// These endpoints should be removed or restricted in production.
/// </summary>
[ApiController]
[Route("api/v1/diagnostic")]
[Authorize(Roles = "SuperAdmin,HRAdmin")] // Restrict to admin roles only
public class DiagnosticController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;
    private readonly IEmployeeRepository _employeeRepository;

    public DiagnosticController(
        IAttendanceService attendanceService,
        IEmployeeRepository employeeRepository)
    {
        _attendanceService = attendanceService;
        _employeeRepository = employeeRepository;
    }

    /// <summary>
    /// Get current user's JWT claims and employee linkage status
    /// </summary>
    [HttpGet("my-status")]
    public async Task<IActionResult> GetMyStatus()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Ok(new
            {
                success = true,
                data = new
                {
                    isAuthenticated = false,
                    message = "Invalid or missing user ID in token"
                }
            });
        }

        var employeeId = await _employeeRepository.GetIdByUserIdAsync(userId);
        var employee = employeeId.HasValue
            ? await _employeeRepository.GetByIdAsync(employeeId.Value)
            : null;

        return Ok(new
        {
            success = true,
            data = new
            {
                isAuthenticated = true,
                userId = userId,
                employeeId = employeeId,
                isLinked = employeeId.HasValue && employee != null,
                employeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : null,
                employeeEmail = employee?.Email,
                message = employeeId.HasValue
                    ? "Employee profile linked"
                    : "No employee profile linked to your account"
            }
        });
    }

    /// <summary>
    /// Get all employees with missing UserId links (orphaned records)
    /// </summary>
    [HttpGet("orphaned-employees")]
    public async Task<IActionResult> GetOrphanedEmployees()
    {
        // This requires a custom query since repository doesn't have this method
        // We'll return a simplified response
        return Ok(new
        {
            success = true,
            message = "This endpoint needs implementation in EmployeeRepository. " +
                     "Run this SQL: SELECT Id, EmployeeCode, FirstName, LastName, Email FROM \"Employees\" WHERE \"UserId\" IS NULL OR \"UserId\" = 0;",
            sqlQuery = "SELECT Id, EmployeeCode, FirstName, LastName, Email, UserId FROM \"Employees\" WHERE \"UserId\" IS NULL OR \"UserId\" = 0;"
        });
    }

    /// <summary>
    /// Manually link a user to an employee (emergency fix)
    /// </summary>
    [HttpPost("link-employee")]
    public async Task<IActionResult> LinkEmployee([FromBody] LinkEmployeeRequest request)
    {
        if (request.UserId <= 0 || request.EmployeeId <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Invalid userId or employeeId"
            });
        }

        // Check if employee exists
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);
        if (employee == null)
        {
            return NotFound(new
            {
                success = false,
                message = $"Employee with ID {request.EmployeeId} not found"
            });
        }

        // Check if already linked
        var existingLink = await _employeeRepository.GetIdByUserIdAsync(request.UserId);
        if (existingLink.HasValue && existingLink.Value != request.EmployeeId)
        {
            return BadRequest(new
            {
                success = false,
                message = $"User {request.UserId} is already linked to employee {existingLink.Value}"
            });
        }

        if (existingLink.HasValue && existingLink.Value == request.EmployeeId)
        {
            return Ok(new
            {
                success = true,
                message = "User already linked to this employee"
            });
        }

        // Update employee's UserId
        employee.UserId = request.UserId;
        var updated = await _employeeRepository.UpdateAsync(request.EmployeeId, employee);

        if (updated == null)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to update employee"
            });
        }

        return Ok(new
        {
            success = true,
            message = $"Successfully linked user {request.UserId} to employee {request.EmployeeId}",
            data = new
            {
                employeeId = updated.Id,
                userId = updated.UserId,
                employeeName = $"{updated.FirstName} {updated.LastName}"
            }
        });
    }
}

public class LinkEmployeeRequest
{
    public int UserId { get; set; }
    public int EmployeeId { get; set; }
}

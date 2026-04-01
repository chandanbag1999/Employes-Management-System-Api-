using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Organization.DTOs;
using EMS.Application.Modules.Organization.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMS.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _service;

    public DepartmentsController(IDepartmentService service)
    {
        _service = service;
    }

    // GET api/v1/departments
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await _service.GetAllAsync(page, pageSize, search);
        return Ok(ApiResponse<PaginatedResult<DepartmentResponseDto>>.Ok(result));
    }

    // GET api/v1/departments/deleted
    // SuperAdmin only — deleted departments dekhna
    [HttpGet("deleted")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetDeleted()
    {
        var result = await _service.GetDeletedAsync();
        return Ok(ApiResponse<IEnumerable<DepartmentResponseDto>>.Ok(result));
    }

    // GET api/v1/departments/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("Department not found."));
        return Ok(ApiResponse<DepartmentResponseDto>.Ok(result));
    }

    // POST api/v1/departments
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Invalid request data."));

        var (result, error) = await _service.CreateAsync(dto);
        if (error != null)
            return Conflict(ApiResponse<string>.Fail(error));

        return CreatedAtAction(nameof(GetById),
            new { id = result!.Id },
            ApiResponse<DepartmentResponseDto>.Ok(result, "Department created."));
    }

    // PUT api/v1/departments/5
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Invalid request data."));

        var (result, error) = await _service.UpdateAsync(id, dto);
        if (error != null)
            return error.Contains("not found")
                ? NotFound(ApiResponse<string>.Fail(error))
                : Conflict(ApiResponse<string>.Fail(error));

        return Ok(ApiResponse<DepartmentResponseDto>.Ok(result!, "Department updated."));
    }

    // DELETE api/v1/departments/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, error) = await _service.DeleteAsync(id);
        if (!success)
            return error!.Contains("not found")
                ? NotFound(ApiResponse<string>.Fail(error))
                : BadRequest(ApiResponse<string>.Fail(error));

        return Ok(ApiResponse<string>.Ok("Deleted", "Department soft deleted."));
    }

    // POST api/v1/departments/5/restore
    // SuperAdmin only — deleted department restore karna
    [HttpPost("{id}/restore")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Restore(int id)
    {
        var (success, error) = await _service.RestoreAsync(id);
        if (!success)
            return NotFound(ApiResponse<string>.Fail(error!));

        return Ok(ApiResponse<string>.Ok("Restored", "Department restored successfully."));
    }

    // DELETE api/v1/departments/purge?months=12
    // SuperAdmin only — permanently delete old soft-deleted departments
    [HttpDelete("purge")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Purge([FromQuery] int months = 12)
    {
        var count = await _service.PurgeAsync(months);
        return Ok(ApiResponse<int>.Ok(count,
            $"Purged {count} departments deleted more than {months} months ago."));
    }
}
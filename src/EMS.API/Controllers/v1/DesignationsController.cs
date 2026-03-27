using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Organization.DTOs;
using EMS.Application.Modules.Organization.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMS.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DesignationsController : ControllerBase
{
    private readonly IDesignationService _service;

    public DesignationsController(IDesignationService service)
    {
        _service = service;
    }

    // GET api/v1/designations
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? departmentId = null)
    {
        var result = await _service.GetAllAsync(departmentId);
        return Ok(ApiResponse<IEnumerable<DesignationResponseDto>>.Ok(result));
    }

    // GET api/v1/designations/deleted
    [HttpGet("deleted")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> GetAllDeleted()
    {
        var result = await _service.GetAllDeletedAsync();
        return Ok(ApiResponse<IEnumerable<DesignationResponseDto>>.Ok(result));
    }

    // GET api/v1/designations/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("Designation not found."));
        return Ok(ApiResponse<DesignationResponseDto>.Ok(result));
    }

    // POST api/v1/designations
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateDesignationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById),
            new { id = result.Id },
            ApiResponse<DesignationResponseDto>.Ok(result, "Designation created."));
    }

    // PUT api/v1/designations/5
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateDesignationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.UpdateAsync(id, dto);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("Designation not found."));
        return Ok(ApiResponse<DesignationResponseDto>.Ok(result, "Designation updated."));
    }

    // DELETE api/v1/designations/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result)
            return NotFound(ApiResponse<string>.Fail("Designation not found."));
        return Ok(ApiResponse<string>.Ok("Deleted", "Designation deleted."));
    }

    // POST api/v1/designations/5/restore
    [HttpPost("{id}/restore")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Restore(int id)
    {
        var result = await _service.RestoreAsync(id);
        if (!result)
            return NotFound(ApiResponse<string>.Fail("Deleted designation not found."));
        return Ok(ApiResponse<string>.Ok("Restored", "Designation restored successfully."));
    }

    // DELETE api/v1/designations/purge?months=12
    [HttpDelete("purge")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> PurgeOld([FromQuery] int months = 12)
    {
        var count = await _service.PurgeOldDeletedAsync(months);
        return Ok(ApiResponse<string>.Ok($"{count}", $"Purged {count} old deleted designations."));
    }
}

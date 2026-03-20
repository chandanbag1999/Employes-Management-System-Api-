using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Employees.DTOs;
using EMS.Application.Modules.Employees.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMS.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _service;

    public EmployeesController(IEmployeeService service)
    {
        _service = service;
    }

    // GET api/v1/employees?page=1&pageSize=10&search=john&departmentId=1
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EmployeeFilterDto filter)
    {
        var result = await _service.GetAllAsync(filter);
        return Ok(ApiResponse<PaginatedResult<EmployeeResponseDto>>.Ok(result));
    }

    // GET api/v1/employees/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("Employee not found."));
        return Ok(ApiResponse<EmployeeResponseDto>.Ok(result));
    }

    // POST api/v1/employees
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (result, error) = await _service.CreateAsync(dto);
        if (error != null)
            return Conflict(ApiResponse<string>.Fail(error));

        return CreatedAtAction(nameof(GetById),
            new { id = result!.Id },
            ApiResponse<EmployeeResponseDto>.Ok(result, "Employee created successfully."));
    }

    // PUT api/v1/employees/5
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (result, error) = await _service.UpdateAsync(id, dto);
        if (error != null)
            return error.Contains("not found")
                ? NotFound(ApiResponse<string>.Fail(error))
                : Conflict(ApiResponse<string>.Fail(error));

        return Ok(ApiResponse<EmployeeResponseDto>.Ok(result!, "Employee updated."));
    }

    // DELETE api/v1/employees/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, error) = await _service.DeleteAsync(id);
        if (!success)
            return NotFound(ApiResponse<string>.Fail(error!));

        return Ok(ApiResponse<string>.Ok("Deleted", "Employee deleted."));
    }
}
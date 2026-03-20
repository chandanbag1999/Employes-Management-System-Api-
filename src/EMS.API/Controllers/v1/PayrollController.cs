using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Payroll.DTOs;
using EMS.Application.Modules.Payroll.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMS.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PayrollController : ControllerBase
{
    private readonly IPayrollService _service;

    public PayrollController(IPayrollService service)
    {
        _service = service;
    }

    // POST api/v1/payroll/salary-structure
    [HttpPost("salary-structure")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> CreateSalaryStructure(
        [FromBody] CreateSalaryStructureDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.CreateSalaryStructureAsync(dto);
        return Ok(ApiResponse<SalaryStructureResponseDto>.Ok(
            result, "Salary structure created."));
    }

    // GET api/v1/payroll/salary-structure?employeeId=1
    [HttpGet("salary-structure")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> GetSalaryStructures(
        [FromQuery] int? employeeId = null)
    {
        var result = await _service.GetSalaryStructuresAsync(employeeId);
        return Ok(ApiResponse<IEnumerable<SalaryStructureResponseDto>>.Ok(result));
    }

    // POST api/v1/payroll/run
    [HttpPost("run")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> RunPayroll([FromBody] RunPayrollDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (results, error) = await _service.RunPayrollAsync(dto);
        if (error != null && !results.Any())
            return BadRequest(ApiResponse<string>.Fail(error));

        return Ok(ApiResponse<List<PayrollRecordResponseDto>>.Ok(
            results,
            $"Payroll processed for {results.Count} employees."));
    }

    // GET api/v1/payroll?month=3&year=2026
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> GetAll([FromQuery] PayrollFilterDto filter)
    {
        var result = await _service.GetAllAsync(filter);
        return Ok(ApiResponse<PaginatedResult<PayrollRecordResponseDto>>.Ok(result));
    }

    // GET api/v1/payroll/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("Payroll record not found."));
        return Ok(ApiResponse<PayrollRecordResponseDto>.Ok(result));
    }

    // GET api/v1/payroll/payslip/1?month=3&year=2026
    [HttpGet("payslip/{employeeId}")]
    public async Task<IActionResult> GetMyPayslip(
        int employeeId,
        [FromQuery] int month,
        [FromQuery] int year)
    {
        var result = await _service.GetMyPayslipAsync(employeeId, month, year);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("Payslip not found."));
        return Ok(ApiResponse<PayrollRecordResponseDto>.Ok(result));
    }

    // GET api/v1/payroll/payslips/1
    [HttpGet("payslips/{employeeId}")]
    public async Task<IActionResult> GetEmployeePayslips(int employeeId)
    {
        var result = await _service.GetEmployeePayslipsAsync(employeeId);
        return Ok(ApiResponse<List<PayrollRecordResponseDto>>.Ok(result));
    }

    // PATCH api/v1/payroll/5/mark-paid
    [HttpPatch("{id}/mark-paid")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> MarkAsPaid(int id)
    {
        var (result, error) = await _service.MarkAsPaidAsync(id);
        if (error != null)
            return error.Contains("not found")
                ? NotFound(ApiResponse<string>.Fail(error))
                : BadRequest(ApiResponse<string>.Fail(error));

        return Ok(ApiResponse<PayrollRecordResponseDto>.Ok(
            result!, "Payroll marked as paid."));
    }
}
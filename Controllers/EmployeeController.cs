using EmployesManagementSystemApi.DTOs.Employee;
using EmployesManagementSystemApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmployesManagementSystemApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]   // ← All endpoints require login
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // GET api/employee
        // Any logged-in user can view employees
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var employees = await _employeeService.GetAllAsync();
            return Ok(employees);
        }

        // GET api/employee/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var employee = await _employeeService.GetByIdAsync(id);

            if (employee == null)
                return NotFound(new { message = $"Employee with ID {id} not found." });

            return Ok(employee);
        }

        // GET api/employee/department/3
        [HttpGet("department/{departmentId}")]
        public async Task<IActionResult> GetByDepartment(int departmentId)
        {
            var employees = await _employeeService.GetByDepartmentAsync(departmentId);
            return Ok(employees);
        }

        // POST api/employee
        // Only Admin can create, update, delete
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
        {
            var created = await _employeeService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT api/employee/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
        {
            var updated = await _employeeService.UpdateAsync(id, dto);

            if (updated == null)
                return NotFound(new { message = $"Employee with ID {id} not found." });

            return Ok(updated);
        }

        // DELETE api/employee/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _employeeService.DeleteAsync(id);

            if (!deleted)
                return NotFound(new { message = $"Employee with ID {id} not found." });

            return Ok(new { message = "Employee deleted successfully." });
        }
    }
}

using EmployesManagementSystemApi.DTOs.Department;
using EmployesManagementSystemApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployesManagementSystemApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]   // ← All endpoints require login by default
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        // GET api/department
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var departments = await _departmentService.GetAllAsync();
            return Ok(departments);
        }

        // GET api/department/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var department = await _departmentService.GetByIdAsync(id);

            if (department == null)
                return NotFound(new { message = $"Department with ID {id} not found." });

            return Ok(department);
        }

        // POST api/department
        // Only Admin can create, update, delete
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
        {
            var created = await _departmentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT api/department/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateDepartmentDto dto)
        {
            var updated = await _departmentService.UpdateAsync(id, dto);

            if (updated == null)
                return NotFound(new { message = $"Department with ID {id} not found." });

            return Ok(updated);
        }

        // DELETE api/department/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _departmentService.DeleteAsync(id);

            return result switch
            {
                DepartmentDeleteStatus.NotFound => NotFound(new { message = $"Department with ID {id} not found." }),
                DepartmentDeleteStatus.HasEmployees => Conflict(new { message = "Cannot delete department because employees are assigned to it." }),
                _ => Ok(new { message = "Department deleted successfully." })
            };
        }

    }
}

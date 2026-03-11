using Microsoft.AspNetCore.Mvc;

namespace EmployesManagementSystemApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult ping()
    {
        return Ok(new { message = "API is running" });
    }
}
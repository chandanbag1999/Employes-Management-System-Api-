using EMS.Application.Common.DTOs;
using EMS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace EMS.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;

    public HealthController(AppDbContext context)
    {
        _context = context;
    }

    // GET api/v1/health/ping
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(ApiResponse<object>.Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            environment = Environment.GetEnvironmentVariable(
                "ASPNETCORE_ENVIRONMENT") ?? "Production"
        }));
    }

    // GET api/v1/health/db
    [HttpGet("db")]
    public async Task<IActionResult> CheckDb()
    {
        try
        {
            await _context.Database.CanConnectAsync();
            return Ok(ApiResponse<object>.Ok(new
            {
                database = "connected",
                timestamp = DateTime.UtcNow
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(503, ApiResponse<string>.Fail(
                $"Database connection failed: {ex.Message}"));
        }
    }
}
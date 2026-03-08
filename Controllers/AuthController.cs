using EmployesManagementSystemApi.DTOs.Auth;
using EmployesManagementSystemApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmployesManagementSystemApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]   // ← entire controller is public
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            if (result == null)
                return BadRequest(new { message = "Email already exists." });

            return Ok(result);
        }

        // POST api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            if (result == null)
                return Unauthorized(new { message = "Invalid email or password." });

            return Ok(result);
        }
    }
}

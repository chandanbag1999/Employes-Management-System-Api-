using EMS.Application.Modules.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMS.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // GET api/v1/users
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    // GET api/v1/users/5
    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound(new { message = "User not found." });
        return Ok(user);
    }

    // PATCH api/v1/users/5/deactivate
    [HttpPatch("{id}/deactivate")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var result = await _userService.DeactivateUserAsync(id);
        if (!result) return NotFound(new { message = "User not found." });
        return Ok(new { message = "User deactivated successfully." });
    }
}
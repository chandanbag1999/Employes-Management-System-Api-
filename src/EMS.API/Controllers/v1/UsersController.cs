using System.Security.Claims;
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Identity.DTOs;
using EMS.Application.Modules.Identity.Interfaces;
using EMS.Domain.Enums;
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
        return Ok(ApiResponse<IEnumerable<UserResponseDto>>.Ok(users));
    }

    // GET api/v1/users/me
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<string>.Fail("Invalid token."));

        var user = await _userService.GetCurrentUserAsync(userId.Value);
        if (user == null)
            return NotFound(ApiResponse<string>.Fail("User not found."));

        return Ok(ApiResponse<UserResponseDto>.Ok(user));
    }

    // GET api/v1/users/5
    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound(ApiResponse<string>.Fail("User not found."));

        return Ok(ApiResponse<UserResponseDto>.Ok(user));
    }

    // PATCH api/v1/users/5/deactivate
    [HttpPatch("{id}/deactivate")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var result = await _userService.DeactivateUserAsync(id);
        if (!result)
            return NotFound(ApiResponse<string>.Fail("User not found."));

        return Ok(ApiResponse<string>.Ok("User deactivated."));
    }

    // PATCH api/v1/users/5/activate
    [HttpPatch("{id}/activate")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Activate(int id)
    {
        var result = await _userService.ActivateUserAsync(id);
        if (!result)
            return NotFound(ApiResponse<string>.Fail("User not found."));

        return Ok(ApiResponse<string>.Ok("User activated."));
    }

    // PATCH api/v1/users/5/role
    [HttpPatch("{id}/role")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ChangeRole(int id, [FromBody] ChangeRoleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Invalid role value."));

        if (!Enum.IsDefined(typeof(UserRole), dto.Role))
            return BadRequest(ApiResponse<string>.Fail("Invalid role specified."));

        var result = await _userService.ChangeRoleAsync(id, (UserRole)dto.Role);
        if (!result)
            return NotFound(ApiResponse<string>.Fail("User not found."));

        return Ok(ApiResponse<string>.Ok("Role updated. User must re-login."));
    }

    // ── Private Helper ────────────────────────────────────────────────
    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : null;
    }
}
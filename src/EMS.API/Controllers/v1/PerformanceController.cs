using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Performance.DTOs;
using EMS.Application.Modules.Performance.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMS.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PerformanceController : ControllerBase
{
    private readonly IPerformanceService _service;

    public PerformanceController(IPerformanceService service)
    {
        _service = service;
    }

    // ── Goals ─────────────────────────────────────────────────────

    // GET api/v1/performance/goals?employeeId=1&reviewCycle=2026-Q1
    [HttpGet("goals")]
    public async Task<IActionResult> GetGoals(
        [FromQuery] int? employeeId,
        [FromQuery] string? reviewCycle)
    {
        var result = await _service.GetGoalsAsync(employeeId, reviewCycle);
        return Ok(ApiResponse<IEnumerable<GoalResponseDto>>.Ok(result));
    }

    // GET api/v1/performance/goals/5
    [HttpGet("goals/{id}")]
    public async Task<IActionResult> GetGoalById(int id)
    {
        var result = await _service.GetGoalByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("Goal not found."));
        return Ok(ApiResponse<GoalResponseDto>.Ok(result));
    }

    // POST api/v1/performance/goals
    [HttpPost("goals")]
    [Authorize(Roles = "SuperAdmin,HRAdmin,Manager")]
    public async Task<IActionResult> CreateGoal([FromBody] CreateGoalDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.CreateGoalAsync(dto);
        return CreatedAtAction(nameof(GetGoalById),
            new { id = result.Id },
            ApiResponse<GoalResponseDto>.Ok(result, "Goal created."));
    }

    // PUT api/v1/performance/goals/5
    [HttpPut("goals/{id}")]
    [Authorize(Roles = "SuperAdmin,HRAdmin,Manager")]
    public async Task<IActionResult> UpdateGoal(int id, [FromBody] UpdateGoalDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (result, error) = await _service.UpdateGoalAsync(id, dto);
        if (error != null)
            return error.Contains("not found")
                ? NotFound(ApiResponse<string>.Fail(error))
                : BadRequest(ApiResponse<string>.Fail(error));

        return Ok(ApiResponse<GoalResponseDto>.Ok(result, "Goal updated."));
    }

    // PATCH api/v1/performance/goals/5/progress
    [HttpPatch("goals/{id}/progress")]
    public async Task<IActionResult> UpdateProgress(
        int id, [FromBody] UpdateGoalProgressDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (result, error) = await _service.UpdateProgressAsync(id, dto);
        if (error != null)
            return error.Contains("not found")
                ? NotFound(ApiResponse<string>.Fail(error))
                : BadRequest(ApiResponse<string>.Fail(error));

        return Ok(ApiResponse<GoalResponseDto>.Ok(result!, "Progress updated."));
    }

    // DELETE api/v1/performance/goals/5
    [HttpDelete("goals/{id}")]
    [Authorize(Roles = "SuperAdmin,HRAdmin,Manager")]
    public async Task<IActionResult> DeleteGoal(int id)
    {
        var (success, error) = await _service.DeleteGoalAsync(id);
        if (!success)
            return NotFound(ApiResponse<string>.Fail(error!));
        return Ok(ApiResponse<string>.Ok("Deleted", "Goal deleted."));
    }

    // ── Reviews ───────────────────────────────────────────────────

    // GET api/v1/performance/reviews?year=2026&quarter=Q1
    [HttpGet("reviews")]
    public async Task<IActionResult> GetAllReviews(
        [FromQuery] PerformanceFilterDto filter)
    {
        var result = await _service.GetAllReviewsAsync(filter);
        return Ok(ApiResponse<PaginatedResult<ReviewResponseDto>>.Ok(result));
    }

    // GET api/v1/performance/reviews/5
    [HttpGet("reviews/{id}")]
    public async Task<IActionResult> GetReviewById(int id)
    {
        var result = await _service.GetReviewByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("Review not found."));
        return Ok(ApiResponse<ReviewResponseDto>.Ok(result));
    }

    // POST api/v1/performance/reviews
    [HttpPost("reviews")]
    [Authorize(Roles = "SuperAdmin,HRAdmin,Manager")]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (result, error) = await _service.CreateReviewAsync(dto);
        if (error != null)
            return Conflict(ApiResponse<string>.Fail(error));

        return CreatedAtAction(nameof(GetReviewById),
            new { id = result!.Id },
            ApiResponse<ReviewResponseDto>.Ok(result, "Review created."));
    }

    // PATCH api/v1/performance/reviews/5/self-comment
    [HttpPatch("reviews/{id}/self-comment")]
    public async Task<IActionResult> AddSelfComment(
        int id, [FromBody] SelfCommentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (result, error) = await _service.AddSelfCommentAsync(id, dto);
        if (error != null)
            return error.Contains("not found")
                ? NotFound(ApiResponse<string>.Fail(error))
                : BadRequest(ApiResponse<string>.Fail(error));

        return Ok(ApiResponse<ReviewResponseDto>.Ok(
            result!, "Self comment added."));
    }

    // PATCH api/v1/performance/reviews/5/submit
    [HttpPatch("reviews/{id}/submit")]
    [Authorize(Roles = "SuperAdmin,HRAdmin,Manager")]
    public async Task<IActionResult> SubmitReview(int id)
    {
        var (result, error) = await _service.SubmitReviewAsync(id);
        if (error != null)
            return error.Contains("not found")
                ? NotFound(ApiResponse<string>.Fail(error))
                : BadRequest(ApiResponse<string>.Fail(error));

        return Ok(ApiResponse<ReviewResponseDto>.Ok(result!, "Review submitted."));
    }

    // ── Summary ───────────────────────────────────────────────────

    // GET api/v1/performance/summary/1?year=2026
    [HttpGet("summary/{employeeId}")]
    public async Task<IActionResult> GetSummary(
        int employeeId, [FromQuery] int year = 0)
    {
        if (year == 0) year = DateTime.Today.Year;

        var result = await _service.GetEmployeeSummaryAsync(employeeId, year);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("No performance data found."));

        return Ok(ApiResponse<EmployeePerformanceSummaryDto>.Ok(result));
    }
}
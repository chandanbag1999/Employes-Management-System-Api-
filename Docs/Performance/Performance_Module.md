# Performance Module

**Status:** Fully Implemented ✅

---

## 1. Module Overview

This module handles:
- Goal setting and tracking for employees
- Performance reviews with ratings (1-5 scale)
- Self-comments by employees on reviews
- Performance summary dashboard
- Review cycle management (Quarterly/Annual)

### Related Files

| Layer | File | Purpose |
|-------|------|---------|
| **Domain** | `src/EMS.Domain/Entities/Performance/Goal.cs` | Goal entity |
| **Domain** | `src/EMS.Domain/Entities/Performance/PerformanceReview.cs` | Review entity |
| **Application** | `src/EMS.Application/Modules/Performance/DTOs/CreateGoalDto.cs` | Create goal DTO |
| **Application** | `src/EMS.Application/Modules/Performance/DTOs/CreateReviewDto.cs` | Create review DTO |
| **Application** | `src/EMS.Application/Modules/Performance/DTOs/EmployeePerformanceSummaryDto.cs` | Summary DTO |
| **Application** | `src/EMS.Application/Modules/Performance/DTOs/GoalResponseDto.cs` | Goal response DTO |
| **Application** | `src/EMS.Application/Modules/Performance/DTOs/PerformanceFilterDto.cs` | Filter DTO |
| **Application** | `src/EMS.Application/Modules/Performance/DTOs/ReviewResponseDto.cs` | Review response DTO |
| **Application** | `src/EMS.Application/Modules/Performance/DTOs/SelfCommentDto.cs` | Self comment DTO |
| **Application** | `src/EMS.Application/Modules/Performance/DTOs/UpdateGoalProgressDto.cs` | Update progress DTO |
| **Application** | `src/EMS.Application/Modules/Performance/Interfaces/IPerformanceService.cs` | Service interface |
| **Application** | `src/EMS.Application/Modules/Performance/Interfaces/IPerformanceRepository.cs` | Repository interface |
| **Application** | `src/EMS.Application/Modules/Performance/Services/PerformanceService.cs` | Business logic |
| **Infrastructure** | `src/EMS.Infrastructure/Repositories/PerformanceRepository.cs` | Data access |
| **API** | `src/EMS.API/Controllers/v1/PerformanceController.cs` | Endpoints |

---

## 2. Entities

### 2.1 Goal
```csharp
// src/EMS.Domain/Entities/Performance/Goal.cs
using EMS.Domain.Common;
using EMS.Domain.Entities.Employee;

public class Goal : BaseEntity
{
    public int EmployeeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Deadline { get; set; }
    public int ProgressPercent { get; set; } = 0;
    public string ReviewCycle { get; set; } = string.Empty; // "2026-Q1"
    public string Status { get; set; } = "Active"; // Active, Completed, Cancelled
    public int? SetByManagerId { get; set; }

    // Navigation
    public EmployeeProfile? Employee { get; set; }
    public EmployeeProfile? SetByManager { get; set; }
}
```

### 2.2 PerformanceReview
```csharp
// src/EMS.Domain/Entities/Performance/PerformanceReview.cs
using EMS.Domain.Common;
using EMS.Domain.Entities.Employee;

public class PerformanceReview : BaseEntity
{
    public int EmployeeId { get; set; }
    public int ReviewerId { get; set; }         // Manager/HR
    public string ReviewCycle { get; set; } = string.Empty;  // "2026-Q1"
    public int Year { get; set; }
    public string Quarter { get; set; } = string.Empty;  // "Q1","Q2","Q3","Q4","Annual"

    // Ratings (1-5 scale)
    public decimal TechnicalSkillRating { get; set; }
    public decimal CommunicationRating { get; set; }
    public decimal TeamworkRating { get; set; }
    public decimal LeadershipRating { get; set; }
    public decimal PunctualityRating { get; set; }
    public decimal OverallRating { get; set; }  // Auto-calculated average

    // Feedback
    public string? Strengths { get; set; }
    public string? AreasOfImprovement { get; set; }
    public string? ReviewerComments { get; set; }
    public string? EmployeeSelfComment { get; set; }

    public string Status { get; set; } = "Draft"; // Draft, Submitted, Acknowledged

    // Navigation
    public EmployeeProfile? Employee { get; set; }
    public EmployeeProfile? Reviewer { get; set; }
}
```

---

## 3. DTOs

### 3.1 CreateGoalDto
```csharp
// src/EMS.Application/Modules/Performance/DTOs/CreateGoalDto.cs
using System.ComponentModel.DataAnnotations;

public class CreateGoalDto
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime Deadline { get; set; }

    [Required]
    [MaxLength(20)]
    public string ReviewCycle { get; set; } = string.Empty; // "2026-Q1"

    public int? SetByManagerId { get; set; }
}
```

### 3.2 UpdateGoalProgressDto
```csharp
// src/EMS.Application/Modules/Performance/DTOs/UpdateGoalProgressDto.cs
using System.ComponentModel.DataAnnotations;

public class UpdateGoalProgressDto
{
    [Required]
    [Range(0, 100)]
    public int ProgressPercent { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Active"; // Active, Completed, Cancelled
}
```

### 3.3 CreateReviewDto
```csharp
// src/EMS.Application/Modules/Performance/DTOs/CreateReviewDto.cs
using System.ComponentModel.DataAnnotations;

public class CreateReviewDto
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public int ReviewerId { get; set; }

    [Required]
    [MaxLength(20)]
    public string ReviewCycle { get; set; } = string.Empty; // "2026-Q1"

    [Required]
    public int Year { get; set; }

    [Required]
    [MaxLength(10)]
    public string Quarter { get; set; } = string.Empty; // Q1,Q2,Q3,Q4,Annual

    // Ratings 1-5
    [Required]
    [Range(1, 5)]
    public decimal TechnicalSkillRating { get; set; }

    [Required]
    [Range(1, 5)]
    public decimal CommunicationRating { get; set; }

    [Required]
    [Range(1, 5)]
    public decimal TeamworkRating { get; set; }

    [Required]
    [Range(1, 5)]
    public decimal LeadershipRating { get; set; }

    [Required]
    [Range(1, 5)]
    public decimal PunctualityRating { get; set; }

    [MaxLength(1000)]
    public string? Strengths { get; set; }

    [MaxLength(1000)]
    public string? AreasOfImprovement { get; set; }

    [MaxLength(1000)]
    public string? ReviewerComments { get; set; }
}
```

### 3.4 PerformanceFilterDto
```csharp
// src/EMS.Application/Modules/Performance/DTOs/PerformanceFilterDto.cs
public class PerformanceFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? EmployeeId { get; set; }
    public int? DepartmentId { get; set; }
    public string? ReviewCycle { get; set; }
    public int? Year { get; set; }
    public string? Quarter { get; set; }
    public string? Status { get; set; }
}
```

### 3.5 SelfCommentDto
```csharp
// src/EMS.Application/Modules/Performance/DTOs/SelfCommentDto.cs
using System.ComponentModel.DataAnnotations;

// Employee apna comment add karta hai
public class SelfCommentDto
{
    [Required]
    [MaxLength(1000)]
    public string SelfComment { get; set; } = string.Empty;
}
```

### 3.6 GoalResponseDto
```csharp
// src/EMS.Application/Modules/Performance/DTOs/GoalResponseDto.cs
public class GoalResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Deadline { get; set; }
    public int ProgressPercent { get; set; }
    public string ReviewCycle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? SetByManagerId { get; set; }
    public string? SetByManagerName { get; set; }
    public bool IsOverdue => Status == "Active" && Deadline.Date < DateTime.Today;
    public DateTime CreatedAt { get; set; }
}
```

### 3.7 ReviewResponseDto
```csharp
// src/EMS.Application/Modules/Performance/DTOs/ReviewResponseDto.cs
public class ReviewResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int ReviewerId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string ReviewCycle { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Quarter { get; set; } = string.Empty;

    // Ratings
    public decimal TechnicalSkillRating { get; set; }
    public decimal CommunicationRating { get; set; }
    public decimal TeamworkRating { get; set; }
    public decimal LeadershipRating { get; set; }
    public decimal PunctualityRating { get; set; }
    public decimal OverallRating { get; set; }
    public string RatingLabel => OverallRating switch
    {
        >= 4.5m => "Outstanding",
        >= 3.5m => "Exceeds Expectations",
        >= 2.5m => "Meets Expectations",
        >= 1.5m => "Needs Improvement",
        _ => "Unsatisfactory"
    };

    // Feedback
    public string? Strengths { get; set; }
    public string? AreasOfImprovement { get; set; }
    public string? ReviewerComments { get; set; }
    public string? EmployeeSelfComment { get; set; }

    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

### 3.8 EmployeePerformanceSummaryDto
```csharp
// src/EMS.Application/Modules/Performance/DTOs/EmployeePerformanceSummaryDto.cs
public class EmployeePerformanceSummaryDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;

    // Goals Summary
    public int TotalGoals { get; set; }
    public int CompletedGoals { get; set; }
    public int ActiveGoals { get; set; }
    public double GoalCompletionRate =>
        TotalGoals > 0
            ? Math.Round((double)CompletedGoals / TotalGoals * 100, 1)
            : 0;

    // Latest Review
    public decimal? LatestOverallRating { get; set; }
    public string? LatestRatingLabel { get; set; }
    public string? LatestReviewCycle { get; set; }

    // Average Rating (all reviews)
    public decimal? AverageRating { get; set; }
}
```

---

## 4. Interfaces

### 4.1 IPerformanceService
```csharp
// src/EMS.Application/Modules/Performance/Interfaces/IPerformanceService.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Performance.DTOs;

public interface IPerformanceService
{
    // Goals
    Task<IEnumerable<GoalResponseDto>> GetGoalsAsync(
        int? employeeId, string? reviewCycle);
    Task<GoalResponseDto?> GetGoalByIdAsync(int id);
    Task<GoalResponseDto> CreateGoalAsync(CreateGoalDto dto);
    Task<(GoalResponseDto? result, string? error)> UpdateProgressAsync(
        int id, UpdateGoalProgressDto dto);
    Task<(bool success, string? error)> DeleteGoalAsync(int id);

    // Reviews
    Task<PaginatedResult<ReviewResponseDto>> GetAllReviewsAsync(
        PerformanceFilterDto filter);
    Task<ReviewResponseDto?> GetReviewByIdAsync(int id);
    Task<(ReviewResponseDto? result, string? error)> CreateReviewAsync(
        CreateReviewDto dto);
    Task<(ReviewResponseDto? result, string? error)> AddSelfCommentAsync(
        int id, SelfCommentDto dto);
    Task<(ReviewResponseDto? result, string? error)> SubmitReviewAsync(int id);

    // Summary
    Task<EmployeePerformanceSummaryDto?> GetEmployeeSummaryAsync(
        int employeeId, int year);
}
```

### 4.2 IPerformanceRepository
```csharp
// src/EMS.Application/Modules/Performance/Interfaces/IPerformanceRepository.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Performance.DTOs;
using EMS.Domain.Entities.Performance;

public interface IPerformanceRepository
{
    // Goals
    Task<IEnumerable<Goal>> GetGoalsAsync(int? employeeId, string? reviewCycle);
    Task<Goal?> GetGoalByIdAsync(int id);
    Task<Goal> CreateGoalAsync(Goal goal);
    Task<Goal?> UpdateGoalAsync(int id, Goal goal);
    Task<bool> DeleteGoalAsync(int id);

    // Reviews
    Task<PaginatedResult<PerformanceReview>> GetAllReviewsAsync(
        PerformanceFilterDto filter);
    Task<PerformanceReview?> GetReviewByIdAsync(int id);
    Task<PerformanceReview?> GetReviewByCycleAsync(int employeeId, string reviewCycle);
    Task<PerformanceReview> CreateReviewAsync(PerformanceReview review);
    Task<PerformanceReview?> UpdateReviewAsync(int id, PerformanceReview review);
    Task<PerformanceReview?> AddSelfCommentAsync(int id, string comment);
    Task<PerformanceReview?> SubmitReviewAsync(int id);

    // Summary
    Task<List<Goal>> GetEmployeeGoalsAsync(int employeeId, int? year);
    Task<List<PerformanceReview>> GetEmployeeReviewsAsync(int employeeId);
}
```

---

## 5. Service Implementation

### PerformanceService
```csharp
// src/EMS.Application/Modules/Performance/Services/PerformanceService.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Performance.DTOs;
using EMS.Application.Modules.Performance.Interfaces;
using EMS.Domain.Entities.Performance;

public class PerformanceService : IPerformanceService
{
    private readonly IPerformanceRepository _repo;

    public PerformanceService(IPerformanceRepository repo)
    {
        _repo = repo;
    }

    // ── Goals ─────────────────────────────────────────────────────

    public async Task<IEnumerable<GoalResponseDto>> GetGoalsAsync(
        int? employeeId, string? reviewCycle)
    {
        var goals = await _repo.GetGoalsAsync(employeeId, reviewCycle);
        return goals.Select(MapGoalToDto);
    }

    public async Task<GoalResponseDto?> GetGoalByIdAsync(int id)
    {
        var goal = await _repo.GetGoalByIdAsync(id);
        return goal == null ? null : MapGoalToDto(goal);
    }

    public async Task<GoalResponseDto> CreateGoalAsync(CreateGoalDto dto)
    {
        var goal = new Goal
        {
            EmployeeId = dto.EmployeeId,
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            Deadline = DateTime.SpecifyKind(dto.Deadline, DateTimeKind.Utc),
            ReviewCycle = dto.ReviewCycle.Trim().ToUpper(),
            SetByManagerId = dto.SetByManagerId,
            ProgressPercent = 0,
            Status = "Active"
        };

        var created = await _repo.CreateGoalAsync(goal);
        return MapGoalToDto(created);
    }

    public async Task<(GoalResponseDto? result, string? error)> UpdateProgressAsync(
        int id, UpdateGoalProgressDto dto)
    {
        var goal = await _repo.GetGoalByIdAsync(id);
        if (goal == null) return (null, "Goal not found.");

        goal.ProgressPercent = dto.ProgressPercent;
        goal.Status = dto.ProgressPercent == 100 ? "Completed" : dto.Status;
        goal.UpdatedAt = DateTime.UtcNow;

        var updated = await _repo.UpdateGoalAsync(id, goal);
        return updated == null
            ? (null, "Update failed.")
            : (MapGoalToDto(updated), null);
    }

    public async Task<(bool success, string? error)> DeleteGoalAsync(int id)
    {
        var goal = await _repo.GetGoalByIdAsync(id);
        if (goal == null) return (false, "Goal not found.");

        var deleted = await _repo.DeleteGoalAsync(id);
        return deleted ? (true, null) : (false, "Delete failed.");
    }

    // ── Reviews ───────────────────────────────────────────────────

    public async Task<PaginatedResult<ReviewResponseDto>> GetAllReviewsAsync(
        PerformanceFilterDto filter)
    {
        var result = await _repo.GetAllReviewsAsync(filter);
        return new PaginatedResult<ReviewResponseDto>
        {
            Data = result.Data.Select(MapReviewToDto),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<ReviewResponseDto?> GetReviewByIdAsync(int id)
    {
        var review = await _repo.GetReviewByIdAsync(id);
        return review == null ? null : MapReviewToDto(review);
    }

    public async Task<(ReviewResponseDto? result, string? error)> CreateReviewAsync(
        CreateReviewDto dto)
    {
        // Same cycle mein already review hai?
        var existing = await _repo.GetReviewByCycleAsync(
            dto.EmployeeId, dto.ReviewCycle.ToUpper());
        if (existing != null)
            return (null,
                $"Review already exists for cycle {dto.ReviewCycle}.");

        // Overall rating = average of all 5 ratings
        var overallRating = Math.Round(
            (dto.TechnicalSkillRating +
             dto.CommunicationRating +
             dto.TeamworkRating +
             dto.LeadershipRating +
             dto.PunctualityRating) / 5, 1);

        var review = new PerformanceReview
        {
            EmployeeId = dto.EmployeeId,
            ReviewerId = dto.ReviewerId,
            ReviewCycle = dto.ReviewCycle.Trim().ToUpper(),
            Year = dto.Year,
            Quarter = dto.Quarter.Trim().ToUpper(),
            TechnicalSkillRating = dto.TechnicalSkillRating,
            CommunicationRating = dto.CommunicationRating,
            TeamworkRating = dto.TeamworkRating,
            LeadershipRating = dto.LeadershipRating,
            PunctualityRating = dto.PunctualityRating,
            OverallRating = overallRating,
            Strengths = dto.Strengths?.Trim(),
            AreasOfImprovement = dto.AreasOfImprovement?.Trim(),
            ReviewerComments = dto.ReviewerComments?.Trim(),
            Status = "Draft"
        };

        var created = await _repo.CreateReviewAsync(review);
        return (MapReviewToDto(created), null);
    }

    public async Task<(ReviewResponseDto? result, string? error)> AddSelfCommentAsync(
        int id, SelfCommentDto dto)
    {
        var review = await _repo.GetReviewByIdAsync(id);
        if (review == null) return (null, "Review not found.");

        if (review.Status == "Submitted")
            return (null, "Review already submitted, cannot add comment.");

        var updated = await _repo.AddSelfCommentAsync(id, dto.SelfComment.Trim());
        return updated == null
            ? (null, "Failed to add comment.")
            : (MapReviewToDto(updated), null);
    }

    public async Task<(ReviewResponseDto? result, string? error)> SubmitReviewAsync(
        int id)
    {
        var review = await _repo.GetReviewByIdAsync(id);
        if (review == null) return (null, "Review not found.");

        if (review.Status == "Submitted")
            return (null, "Review already submitted.");

        var updated = await _repo.SubmitReviewAsync(id);
        return updated == null
            ? (null, "Submit failed.")
            : (MapReviewToDto(updated), null);
    }

    // ── Summary ───────────────────────────────────────────────────

    public async Task<EmployeePerformanceSummaryDto?> GetEmployeeSummaryAsync(
        int employeeId, int year)
    {
        var goals = await _repo.GetEmployeeGoalsAsync(employeeId, year);
        var reviews = await _repo.GetEmployeeReviewsAsync(employeeId);

        if (!goals.Any() && !reviews.Any()) return null;

        var latestReview = reviews
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Quarter)
            .FirstOrDefault();

        var avgRating = reviews.Any()
            ? Math.Round(reviews.Average(r => r.OverallRating), 1)
            : (decimal?)null;

        // Employee info from first goal or review
        var empName = goals.FirstOrDefault()?.Employee != null
            ? $"{goals.First().Employee!.FirstName} {goals.First().Employee!.LastName}"
            : reviews.FirstOrDefault()?.Employee != null
                ? $"{reviews.First().Employee!.FirstName} {reviews.First().Employee!.LastName}"
                : "Unknown";

        var empCode = goals.FirstOrDefault()?.Employee?.EmployeeCode
            ?? reviews.FirstOrDefault()?.Employee?.EmployeeCode
            ?? "";

        var deptName = goals.FirstOrDefault()?.Employee?.Department?.Name
            ?? reviews.FirstOrDefault()?.Employee?.Department?.Name
            ?? "";

        return new EmployeePerformanceSummaryDto
        {
            EmployeeId = employeeId,
            EmployeeName = empName,
            EmployeeCode = empCode,
            DepartmentName = deptName,
            TotalGoals = goals.Count,
            CompletedGoals = goals.Count(g => g.Status == "Completed"),
            ActiveGoals = goals.Count(g => g.Status == "Active"),
            LatestOverallRating = latestReview?.OverallRating,
            LatestRatingLabel = latestReview != null
                ? GetRatingLabel(latestReview.OverallRating)
                : null,
            LatestReviewCycle = latestReview?.ReviewCycle,
            AverageRating = avgRating
        };
    }

    // Helpers
    private static string GetRatingLabel(decimal rating) => rating switch
    {
        >= 4.5m => "Outstanding",
        >= 3.5m => "Exceeds Expectations",
        >= 2.5m => "Meets Expectations",
        >= 1.5m => "Needs Improvement",
        _ => "Unsatisfactory"
    };

    private static GoalResponseDto MapGoalToDto(Goal g) => new()
    {
        Id = g.Id,
        EmployeeId = g.EmployeeId,
        EmployeeName = g.Employee != null
            ? $"{g.Employee.FirstName} {g.Employee.LastName}"
            : "Unknown",
        EmployeeCode = g.Employee?.EmployeeCode ?? "",
        Title = g.Title,
        Description = g.Description,
        Deadline = g.Deadline,
        ProgressPercent = g.ProgressPercent,
        ReviewCycle = g.ReviewCycle,
        Status = g.Status,
        SetByManagerId = g.SetByManagerId,
        SetByManagerName = g.SetByManager != null
            ? $"{g.SetByManager.FirstName} {g.SetByManager.LastName}"
            : null,
        CreatedAt = g.CreatedAt
    };

    private static ReviewResponseDto MapReviewToDto(PerformanceReview r) => new()
    {
        Id = r.Id,
        EmployeeId = r.EmployeeId,
        EmployeeName = r.Employee != null
            ? $"{r.Employee.FirstName} {r.Employee.LastName}"
            : "Unknown",
        EmployeeCode = r.Employee?.EmployeeCode ?? "",
        DepartmentName = r.Employee?.Department?.Name ?? "",
        ReviewerId = r.ReviewerId,
        ReviewerName = r.Reviewer != null
            ? $"{r.Reviewer.FirstName} {r.Reviewer.LastName}"
            : "Unknown",
        ReviewCycle = r.ReviewCycle,
        Year = r.Year,
        Quarter = r.Quarter,
        TechnicalSkillRating = r.TechnicalSkillRating,
        CommunicationRating = r.CommunicationRating,
        TeamworkRating = r.TeamworkRating,
        LeadershipRating = r.LeadershipRating,
        PunctualityRating = r.PunctualityRating,
        OverallRating = r.OverallRating,
        Strengths = r.Strengths,
        AreasOfImprovement = r.AreasOfImprovement,
        ReviewerComments = r.ReviewerComments,
        EmployeeSelfComment = r.EmployeeSelfComment,
        Status = r.Status,
        CreatedAt = r.CreatedAt
    };
}
```

---

## 6. API Controller

### PerformanceController
```csharp
// src/EMS.API/Controllers/v1/PerformanceController.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Performance.DTOs;
using EMS.Application.Modules.Performance.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
```

---

## 7. API Endpoints Summary

| Method | Endpoint | Authorization | Description |
|--------|----------|---------------|-------------|
| GET | `/api/v1/performance/goals` | Authenticated | List goals |
| GET | `/api/v1/performance/goals/{id}` | Authenticated | Get goal by ID |
| POST | `/api/v1/performance/goals` | SuperAdmin, HRAdmin, Manager | Create goal |
| PATCH | `/api/v1/performance/goals/{id}/progress` | Authenticated | Update progress |
| DELETE | `/api/v1/performance/goals/{id}` | SuperAdmin, HRAdmin, Manager | Delete goal |
| GET | `/api/v1/performance/reviews` | Authenticated | List reviews |
| GET | `/api/v1/performance/reviews/{id}` | Authenticated | Get review by ID |
| POST | `/api/v1/performance/reviews` | SuperAdmin, HRAdmin, Manager | Create review |
| PATCH | `/api/v1/performance/reviews/{id}/self-comment` | Authenticated | Add self comment |
| PATCH | `/api/v1/performance/reviews/{id}/submit` | SuperAdmin, HRAdmin, Manager | Submit review |
| GET | `/api/v1/performance/summary/{employeeId}` | Authenticated | Get performance summary |

---

## 8. Features

### 8.1 Goal Management
- Create goals with deadline and review cycle
- Track progress (0-100%)
- Auto-complete when progress = 100%
- Overdue detection
- Manager can set goals for employees

### 8.2 Performance Reviews
- 5-point rating scale for:
  - Technical Skill
  - Communication
  - Teamwork
  - Leadership
  - Punctuality
- Auto-calculated overall rating (average)
- Strengths and Areas of Improvement feedback
- One review per employee per cycle
- Draft → Submitted workflow

### 8.3 Employee Self-Comment
- Employee can add self-comment to draft reviews
- Cannot modify after submission

### 8.4 Performance Summary
- Goal completion rate
- Latest overall rating with label
- Average rating across all reviews
- Combined goals and reviews data

### 8.5 Rating Labels
| Rating Range | Label |
|-------------|-------|
| 4.5 - 5.0 | Outstanding |
| 3.5 - 4.4 | Exceeds Expectations |
| 2.5 - 3.4 | Meets Expectations |
| 1.5 - 2.4 | Needs Improvement |
| 1.0 - 1.4 | Unsatisfactory |

---

## 9. API Usage Examples

### Create Goal
```bash
POST /api/v1/performance/goals
Authorization: Bearer <token>
Content-Type: application/json

{
  "employeeId": 5,
  "title": "Complete AWS Certification",
  "description": "Complete AWS Solutions Architect certification by Q2",
  "deadline": "2026-06-30",
  "reviewCycle": "2026-Q1",
  "setByManagerId": 1
}

# Response 201:
{
  "success": true,
  "message": "Goal created.",
  "data": {
    "id": 10,
    "employeeId": 5,
    "employeeName": "John Doe",
    "title": "Complete AWS Certification",
    "deadline": "2026-06-30T00:00:00Z",
    "progressPercent": 0,
    "reviewCycle": "2026-Q1",
    "status": "Active",
    "isOverdue": false
  }
}
```

### Update Goal Progress
```bash
PATCH /api/v1/performance/goals/10/progress
Authorization: Bearer <token>
Content-Type: application/json

{
  "progressPercent": 50,
  "status": "Active"
}

# Response 200:
{
  "success": true,
  "message": "Progress updated.",
  "data": { ... }
}
```

### Create Performance Review
```bash
POST /api/v1/performance/reviews
Authorization: Bearer <token>
Content-Type: application/json

{
  "employeeId": 5,
  "reviewerId": 1,
  "reviewCycle": "2026-Q1",
  "year": 2026,
  "quarter": "Q1",
  "technicalSkillRating": 4.0,
  "communicationRating": 3.5,
  "teamworkRating": 4.5,
  "leadershipRating": 3.0,
  "punctualityRating": 5.0,
  "strengths": "Good technical skills, reliable team member",
  "areasOfImprovement": "Could improve leadership skills",
  "reviewerComments": "Solid performance in Q1"
}

# Response 201:
{
  "success": true,
  "message": "Review created.",
  "data": {
    "id": 5,
    "overallRating": 4.0,
    "ratingLabel": "Exceeds Expectations",
    "status": "Draft"
  }
}
```

### Add Self Comment
```bash
PATCH /api/v1/performance/reviews/5/self-comment
Authorization: Bearer <token>
Content-Type: application/json

{
  "selfComment": "I agree with the feedback. Working on leadership skills."
}

# Response 200:
{
  "success": true,
  "message": "Self comment added.",
  "data": { ... }
}
```

### Get Performance Summary
```bash
GET /api/v1/performance/summary/5?year=2026
Authorization: Bearer <token>

# Response 200:
{
  "success": true,
  "data": {
    "employeeId": 5,
    "employeeName": "John Doe",
    "employeeCode": "EMP005",
    "departmentName": "Engineering",
    "totalGoals": 3,
    "completedGoals": 2,
    "activeGoals": 1,
    "goalCompletionRate": 66.7,
    "latestOverallRating": 4.0,
    "latestRatingLabel": "Exceeds Expectations",
    "latestReviewCycle": "2026-Q1",
    "averageRating": 4.0
  }
}
```

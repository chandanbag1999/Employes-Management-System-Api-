using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Performance.DTOs;
using EMS.Application.Modules.Performance.Interfaces;
using EMS.Domain.Entities.Performance;
using EMS.Domain.Enums;

namespace EMS.Application.Modules.Performance.Services;

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
            StartDate = dto.StartDate.HasValue
                ? DateTime.SpecifyKind(dto.StartDate.Value, DateTimeKind.Utc)
                : DateTime.UtcNow,
            Deadline = DateTime.SpecifyKind(dto.Deadline, DateTimeKind.Utc),
            ReviewCycle = dto.ReviewCycle.Trim().ToUpper(),
            Priority = string.IsNullOrEmpty(dto.Priority)
                ? GoalPriority.Medium
                : Enum.Parse<GoalPriority>(dto.Priority),
            Category = string.IsNullOrEmpty(dto.Category)
                ? GoalCategory.Technical
                : Enum.Parse<GoalCategory>(dto.Category),
            Tags = dto.Tags?.Trim(),
            SetByManagerId = dto.SetByManagerId,
            ProgressPercent = 0,
            Status = GoalStatus.Active
        };

        var created = await _repo.CreateGoalAsync(goal);
        return MapGoalToDto(created);
    }

    public async Task<(GoalResponseDto? result, string? error)> UpdateGoalAsync(
        int id, UpdateGoalDto dto)
    {
        var goal = await _repo.GetGoalByIdAsync(id);
        if (goal == null) return (null, "Goal not found.");

        if (!string.IsNullOrEmpty(dto.Title)) goal.Title = dto.Title.Trim();
        if (dto.Description != null) goal.Description = dto.Description.Trim();
        if (dto.Deadline.HasValue) goal.Deadline = DateTime.SpecifyKind(dto.Deadline.Value, DateTimeKind.Utc);
        if (!string.IsNullOrEmpty(dto.ReviewCycle)) goal.ReviewCycle = dto.ReviewCycle.Trim().ToUpper();
        if (!string.IsNullOrEmpty(dto.Priority))
            goal.Priority = Enum.Parse<GoalPriority>(dto.Priority);
        if (!string.IsNullOrEmpty(dto.Category))
            goal.Category = Enum.Parse<GoalCategory>(dto.Category);
        if (dto.Tags != null) goal.Tags = dto.Tags.Trim();
        if (dto.SetByManagerId.HasValue) goal.SetByManagerId = dto.SetByManagerId.Value;

        goal.UpdatedAt = DateTime.UtcNow;

        var updated = await _repo.UpdateGoalAsync(id, goal);
        return updated == null
            ? (null, "Update failed.")
            : (MapGoalToDto(updated), null);
    }

    public async Task<(GoalResponseDto? result, string? error)> UpdateProgressAsync(
        int id, UpdateGoalProgressDto dto)
    {
        var goal = await _repo.GetGoalByIdAsync(id);
        if (goal == null) return (null, "Goal not found.");

        goal.ProgressPercent = dto.ProgressPercent;
        if (dto.ProgressPercent == 100 && goal.Status == GoalStatus.Active)
            goal.Status = GoalStatus.Completed;
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
        var existing = await _repo.GetReviewByCycleAsync(
            dto.EmployeeId, dto.ReviewCycle.ToUpper());
        if (existing != null)
            return (null,
                $"Review already exists for cycle {dto.ReviewCycle}.");

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
            Strengths = dto.Strengths?.Trim(),
            AreasOfImprovement = dto.AreasOfImprovement?.Trim(),
            ReviewerComments = dto.ReviewerComments?.Trim(),
            Status = ReviewStatus.Draft
        };

        review.RecalculateOverallRating();

        var created = await _repo.CreateReviewAsync(review);
        return (MapReviewToDto(created), null);
    }

    public async Task<(ReviewResponseDto? result, string? error)> AddSelfCommentAsync(
        int id, SelfCommentDto dto)
    {
        var review = await _repo.GetReviewByIdAsync(id);
        if (review == null) return (null, "Review not found.");

        if (review.Status == ReviewStatus.Submitted)
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

        if (review.Status == ReviewStatus.Submitted)
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
            CompletedGoals = goals.Count(g => g.Status == GoalStatus.Completed),
            ActiveGoals = goals.Count(g => g.Status == GoalStatus.Active),
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
        StartDate = g.StartDate,
        Deadline = g.Deadline,
        ProgressPercent = g.ProgressPercent,
        ReviewCycle = g.ReviewCycle,
        Status = g.Status.ToString(),
        Priority = g.Priority.ToString(),
        Category = g.Category.ToString(),
        Tags = g.Tags,
        ManagerComments = g.ManagerComments,
        EmployeeSelfAssessment = g.EmployeeSelfAssessment,
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
        Status = r.Status.ToString(),
        CreatedAt = r.CreatedAt
    };
}

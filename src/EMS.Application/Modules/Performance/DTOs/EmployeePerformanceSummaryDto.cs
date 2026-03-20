namespace EMS.Application.Modules.Performance.DTOs;

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
using EMS.Domain.Common;
using EMS.Domain.Entities.Employee;
using EMS.Domain.Enums;

namespace EMS.Domain.Entities.Performance;

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

    public ReviewStatus Status { get; set; } = ReviewStatus.Draft;

    public void RecalculateOverallRating()
    {
        OverallRating = Math.Round(
            (TechnicalSkillRating + CommunicationRating + TeamworkRating + LeadershipRating + PunctualityRating) / 5, 1);
    }

    // Navigation
    public EmployeeProfile? Employee { get; set; }
    public EmployeeProfile? Reviewer { get; set; }
}
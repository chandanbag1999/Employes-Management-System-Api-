namespace EMS.Application.Modules.Performance.DTOs;

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
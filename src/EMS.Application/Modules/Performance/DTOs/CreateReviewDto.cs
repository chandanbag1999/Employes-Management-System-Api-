using System.ComponentModel.DataAnnotations;

namespace EMS.Application.Modules.Performance.DTOs;

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
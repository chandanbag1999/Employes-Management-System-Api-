using System.ComponentModel.DataAnnotations;

namespace EMS.Application.Modules.Performance.DTOs;

public class CreateGoalDto
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }

    [Required]
    public DateTime Deadline { get; set; }

    [Required]
    [MaxLength(20)]
    public string ReviewCycle { get; set; } = string.Empty;

    public string? Priority { get; set; }

    public string? Category { get; set; }

    [MaxLength(500)]
    public string? Tags { get; set; }

    public int? SetByManagerId { get; set; }
}

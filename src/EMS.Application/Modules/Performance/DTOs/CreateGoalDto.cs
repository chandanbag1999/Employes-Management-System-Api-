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

    [Required]
    public DateTime Deadline { get; set; }

    [Required]
    [MaxLength(20)]
    public string ReviewCycle { get; set; } = string.Empty; // "2026-Q1"

    public int? SetByManagerId { get; set; }
}
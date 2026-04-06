using System.ComponentModel.DataAnnotations;

namespace EMS.Application.Modules.Performance.DTOs;

public class UpdateGoalDto
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime? Deadline { get; set; }

    [MaxLength(20)]
    public string? ReviewCycle { get; set; }

    public string? Priority { get; set; }

    public string? Category { get; set; }

    [MaxLength(500)]
    public string? Tags { get; set; }

    public int? SetByManagerId { get; set; }
}

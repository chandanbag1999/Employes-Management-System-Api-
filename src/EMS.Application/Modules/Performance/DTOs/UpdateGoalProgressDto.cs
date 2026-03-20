using System.ComponentModel.DataAnnotations;

namespace EMS.Application.Modules.Performance.DTOs;

public class UpdateGoalProgressDto
{
    [Required]
    [Range(0, 100)]
    public int ProgressPercent { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Active"; // Active, Completed, Cancelled
}
using System.ComponentModel.DataAnnotations;
using EMS.Domain.Enums;

namespace EMS.Application.Modules.Performance.DTOs;

public class UpdateGoalProgressDto
{
    [Required]
    [Range(0, 100)]
    public int ProgressPercent { get; set; }

    public GoalStatus Status { get; set; }
}
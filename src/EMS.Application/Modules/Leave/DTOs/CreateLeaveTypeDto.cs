using System.ComponentModel.DataAnnotations;

namespace EMS.Application.Modules.Leave.DTOs;

public class CreateLeaveTypeDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(1, 365)]
    public int MaxDaysPerYear { get; set; }

    public bool IsCarryForwardAllowed { get; set; } = false;
    public bool IsPaid { get; set; } = true;
}
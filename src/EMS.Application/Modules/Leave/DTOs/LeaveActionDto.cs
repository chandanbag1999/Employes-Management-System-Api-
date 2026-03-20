using System.ComponentModel.DataAnnotations;

namespace EMS.Application.Modules.Leave.DTOs;

// Manager/HR approve ya reject karne ke liye
public class LeaveActionDto
{
    [Required]
    public int ActionById { get; set; }  // Manager/HR ka EmployeeId

    [Required]
    public bool IsApproved { get; set; }

    [MaxLength(500)]
    public string? RejectionReason { get; set; }
}
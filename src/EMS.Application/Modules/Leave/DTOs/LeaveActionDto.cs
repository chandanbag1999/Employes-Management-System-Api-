using System.ComponentModel.DataAnnotations;

namespace EMS.Application.Modules.Leave.DTOs;

// Manager/HR approve ya reject karne ke liye
public class LeaveActionDto
{
    // Optional — Controller se JWT resolve karega agar 0 ho
    public int ActionById { get; set; }

    [Required]
    public bool IsApproved { get; set; }

    [MaxLength(500)]
    public string? RejectionReason { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace EMS.Application.Modules.Attendance.DTOs;

public class ClockInDto
{
    [Required]
    public int EmployeeId { get; set; }

    // Optional — agar manually time dena ho (admin ke liye)
    public DateTime? ClockInTime { get; set; }

    [MaxLength(200)]
    public string? Remarks { get; set; }
}
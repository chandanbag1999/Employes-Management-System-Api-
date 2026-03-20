using System.ComponentModel.DataAnnotations;
using EMS.Domain.Enums;

namespace EMS.Application.Modules.Attendance.DTOs;

// Admin use kare — manually attendance mark karne ke liye
public class ManualAttendanceDto
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public AttendanceStatus Status { get; set; }

    public TimeSpan? ClockIn { get; set; }
    public TimeSpan? ClockOut { get; set; }

    [MaxLength(200)]
    public string? Remarks { get; set; }
}
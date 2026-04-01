using System.ComponentModel.DataAnnotations;

namespace EMS.Application.Modules.Attendance.DTOs;

public class ClockOutDto
{
    // ✅ Ab required nahi — JWT se fill hoga
    public int? EmployeeId { get; set; }

    public DateTime? ClockOutTime { get; set; }

    [MaxLength(200)]
    public string? Remarks { get; set; }
}
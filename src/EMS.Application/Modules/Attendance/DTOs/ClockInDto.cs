using System.ComponentModel.DataAnnotations;

namespace EMS.Application.Modules.Attendance.DTOs;

public class ClockInDto
{
    // ✅ Ab required nahi — JWT se fill hoga
    public int? EmployeeId { get; set; }

    // Optional — admin manually time de sakta hai
    public DateTime? ClockInTime { get; set; }

    [MaxLength(200)]
    public string? Remarks { get; set; }
}
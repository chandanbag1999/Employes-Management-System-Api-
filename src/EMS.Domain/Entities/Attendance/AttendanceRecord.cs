using EMS.Domain.Common;
using EMS.Domain.Enums;
using EMS.Domain.Entities.Employee;

namespace EMS.Domain.Entities.Attendance;

public class AttendanceRecord : BaseEntity
{
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan? ClockIn { get; set; }
    public TimeSpan? ClockOut { get; set; }
    public AttendanceStatus Status { get; set; }
    public double? WorkingHours { get; set; }
    public string? Remarks { get; set; }

    // Navigation
    public EmployeeProfile? Employee { get; set; }
}
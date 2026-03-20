namespace EMS.Application.Modules.Attendance.DTOs;

public class AttendanceResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan? ClockIn { get; set; }
    public TimeSpan? ClockOut { get; set; }
    public string Status { get; set; } = string.Empty;
    public double? WorkingHours { get; set; }
    public string? Remarks { get; set; }

    // Computed display fields
    public string ClockInDisplay => ClockIn.HasValue
        ? DateTime.Today.Add(ClockIn.Value).ToString("hh:mm tt")
        : "--";

    public string ClockOutDisplay => ClockOut.HasValue
        ? DateTime.Today.Add(ClockOut.Value).ToString("hh:mm tt")
        : "--";

    public string WorkingHoursDisplay => WorkingHours.HasValue
        ? $"{(int)WorkingHours}h {(int)((WorkingHours.Value % 1) * 60)}m"
        : "--";
}
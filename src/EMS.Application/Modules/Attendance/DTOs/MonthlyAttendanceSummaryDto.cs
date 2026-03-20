namespace EMS.Application.Modules.Attendance.DTOs;

public class MonthlyAttendanceSummaryDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public int TotalWorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int HalfDays { get; set; }
    public int LeaveDays { get; set; }
    public int HolidayDays { get; set; }
    public double TotalWorkingHours { get; set; }
    public double AverageWorkingHours =>
        PresentDays > 0
            ? Math.Round(TotalWorkingHours / PresentDays, 2)
            : 0;
}
namespace EMS.Application.Modules.Reports.DTOs;

public class AttendanceReportDto
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public int TotalWorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int HalfDays { get; set; }
    public int LeaveDays { get; set; }
    public double TotalHours { get; set; }
    public double AttendancePercent =>
        TotalWorkingDays > 0
            ? Math.Round((double)PresentDays / TotalWorkingDays * 100, 1)
            : 0;
}
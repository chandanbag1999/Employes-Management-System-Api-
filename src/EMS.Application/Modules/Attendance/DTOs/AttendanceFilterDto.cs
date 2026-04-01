namespace EMS.Application.Modules.Attendance.DTOs;

public class AttendanceFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 31; // 1 month default
    public int? EmployeeId { get; set; }
    public int? DepartmentId { get; set; }
    // ✅ FIX: DateTime? ki jagah DateOnly use karo
    // PostgreSQL timestamp issue completely solve
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public string? Status { get; set; }  // "Present", "Absent", etc.
}
namespace EMS.Application.Modules.Leave.DTOs;

public class LeaveFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? EmployeeId { get; set; }
    public int? DepartmentId { get; set; }
    public int? LeaveTypeId { get; set; }
    public string? Status { get; set; }   // "Pending","Approved","Rejected"
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
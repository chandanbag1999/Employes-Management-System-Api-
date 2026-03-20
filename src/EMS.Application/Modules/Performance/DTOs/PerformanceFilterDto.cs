namespace EMS.Application.Modules.Performance.DTOs;

public class PerformanceFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? EmployeeId { get; set; }
    public int? DepartmentId { get; set; }
    public string? ReviewCycle { get; set; }
    public int? Year { get; set; }
    public string? Quarter { get; set; }
    public string? Status { get; set; }
}
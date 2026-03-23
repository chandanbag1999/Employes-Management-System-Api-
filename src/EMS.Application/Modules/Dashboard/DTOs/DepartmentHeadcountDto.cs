namespace EMS.Application.Modules.Dashboard.DTOs;

public class DepartmentHeadcountDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? DepartmentCode { get; set; }
    public int EmployeeCount { get; set; }
    public int ActiveCount { get; set; }
}
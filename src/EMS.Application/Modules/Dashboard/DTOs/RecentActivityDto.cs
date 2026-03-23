namespace EMS.Application.Modules.Dashboard.DTOs;

public class RecentActivityDto
{
    public string Type { get; set; } = string.Empty;  // "NewEmployee","Leave","Payroll"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? EmployeeName { get; set; }
}
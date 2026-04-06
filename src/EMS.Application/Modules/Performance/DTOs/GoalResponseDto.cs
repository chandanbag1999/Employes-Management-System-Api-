namespace EMS.Application.Modules.Performance.DTOs;

public class GoalResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime Deadline { get; set; }
    public int ProgressPercent { get; set; }
    public string ReviewCycle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Tags { get; set; }
    public string? ManagerComments { get; set; }
    public string? EmployeeSelfAssessment { get; set; }
    public int? SetByManagerId { get; set; }
    public string? SetByManagerName { get; set; }
    public bool IsOverdue => Status == "Active" && Deadline.Date < DateTime.Today;
    public DateTime CreatedAt { get; set; }
}

using EMS.Domain.Common;
using EMS.Domain.Entities.Employee;
using EMS.Domain.Enums;

namespace EMS.Domain.Entities.Performance;

public class Goal : BaseEntity
{
    public int EmployeeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime Deadline { get; set; }
    public int ProgressPercent { get; set; } = 0;
    public string ReviewCycle { get; set; } = string.Empty; // "2026-Q1"
    public GoalStatus Status { get; set; } = GoalStatus.Active;
    public GoalPriority Priority { get; set; } = GoalPriority.Medium;
    public GoalCategory Category { get; set; } = GoalCategory.Technical;
    public string? Tags { get; set; } // comma-separated: "React,TypeScript,Cert"
    public string? ManagerComments { get; set; }
    public string? EmployeeSelfAssessment { get; set; }
    public int? SetByManagerId { get; set; }

    // Navigation
    public EmployeeProfile? Employee { get; set; }
    public EmployeeProfile? SetByManager { get; set; }
}

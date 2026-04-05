using EMS.Domain.Common;
using EMS.Domain.Entities.Employee;
using EMS.Domain.Enums;

namespace EMS.Domain.Entities.Performance;

public class Goal : BaseEntity
{
    public int EmployeeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Deadline { get; set; }
    public int ProgressPercent { get; set; } = 0;
    public string ReviewCycle { get; set; } = string.Empty; // "2026-Q1"
    public GoalStatus Status { get; set; } = GoalStatus.Active;
    public int? SetByManagerId { get; set; }

    // Navigation
    public EmployeeProfile? Employee { get; set; }
    public EmployeeProfile? SetByManager { get; set; }
}
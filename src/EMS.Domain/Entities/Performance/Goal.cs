using EMS.Domain.Common;

namespace EMS.Domain.Entities.Performance;

public class Goal : BaseEntity
{
    public int EmployeeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Deadline { get; set; }
    public int ProgressPercent { get; set; } = 0;   // 0-100
    public string? ReviewCycleYear { get; set; }     // "2026-Q1"
} 
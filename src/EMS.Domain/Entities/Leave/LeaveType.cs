using EMS.Domain.Common;

namespace EMS.Domain.Entities.Leave;

public class LeaveType : BaseEntity
{
    public string Name { get; set; } = string.Empty;   // "Casual", "Sick", "Earned"
    public int MaxDaysPerYear { get; set; }
    public bool IsCarryForwardAllowed { get; set; }
    public bool IsPaid { get; set; } = true;
}
using EMS.Domain.Common;
using EMS.Domain.Enums;
using EMS.Domain.Entities.Employee;

namespace EMS.Domain.Entities.Leave;

public class LeaveApplication : BaseEntity
{
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public int? ApprovedById { get; set; }
    public string? RejectionReason { get; set; }

    // Navigation
    public LeaveType LeaveType { get; set; } = null!;
    public EmployeeProfile? Employee { get; set; }
    public EmployeeProfile? ApprovedBy { get; set; }
}
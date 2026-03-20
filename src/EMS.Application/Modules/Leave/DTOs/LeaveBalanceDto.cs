namespace EMS.Application.Modules.Leave.DTOs;

public class LeaveBalanceDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public List<LeaveBalanceItemDto> Balances { get; set; } = new();
}

public class LeaveBalanceItemDto
{
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public int TotalAllowed { get; set; }
    public int Used { get; set; }
    public int Pending { get; set; }
    public int Remaining => TotalAllowed - Used - Pending;
}
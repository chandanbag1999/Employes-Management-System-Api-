namespace EMS.Application.Modules.Leave.DTOs;

public class LeaveTypeResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MaxDaysPerYear { get; set; }
    public bool IsCarryForwardAllowed { get; set; }
    public bool IsPaid { get; set; }
}
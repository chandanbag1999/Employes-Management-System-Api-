namespace EMS.Application.Modules.Payroll.DTOs;

public class PayrollFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? EmployeeId { get; set; }
    public int? DepartmentId { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
    public string? Status { get; set; }  // "Generated","Paid"
}
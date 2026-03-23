namespace EMS.Application.Modules.Reports.DTOs;

public class PayrollReportDto
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal GrossEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public string Status { get; set; } = string.Empty;
}
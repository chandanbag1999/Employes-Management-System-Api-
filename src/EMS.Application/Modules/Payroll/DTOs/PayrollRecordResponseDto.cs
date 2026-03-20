namespace EMS.Application.Modules.Payroll.DTOs;

public class PayrollRecordResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public string MonthYear => $"{new DateTime(Year, Month, 1):MMMM yyyy}";

    // Earnings
    public decimal BasicSalary { get; set; }
    public decimal HRA { get; set; }
    public decimal TransportAllowance { get; set; }
    public decimal MedicalAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal GrossEarnings { get; set; }

    // Deductions
    public decimal PfDeduction { get; set; }      // 12% of Basic
    public decimal TaxDeduction { get; set; }      // TDS
    public decimal OtherDeductions { get; set; }
    public decimal TotalDeductions { get; set; }

    // Net Pay
    public decimal NetSalary { get; set; }

    // Attendance based
    public int WorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int LeaveDays { get; set; }
    public int LopDays { get; set; }       // Loss of Pay

    public string Status { get; set; } = string.Empty;
    public DateTime? PaidOn { get; set; }
    public DateTime CreatedAt { get; set; }
}
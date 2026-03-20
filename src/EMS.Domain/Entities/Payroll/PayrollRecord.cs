using EMS.Domain.Common;
using EMS.Domain.Entities.Employee;

namespace EMS.Domain.Entities.Payroll;

public class PayrollRecord : BaseEntity
{
    public int EmployeeId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }

    // Earnings
    public decimal BasicSalary { get; set; }
    public decimal HRA { get; set; }
    public decimal TransportAllowance { get; set; }
    public decimal MedicalAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal GrossEarnings { get; set; }

    // Deductions
    public decimal PfDeduction { get; set; }
    public decimal TaxDeduction { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal TotalDeductions { get; set; }

    // Net
    public decimal NetSalary { get; set; }

    // Attendance
    public int WorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int LeaveDays { get; set; }
    public int LopDays { get; set; }

    public string Status { get; set; } = "Generated";  // Generated, Paid
    public DateTime? PaidOn { get; set; }

    // Navigation
    public EmployeeProfile? Employee { get; set; }
}
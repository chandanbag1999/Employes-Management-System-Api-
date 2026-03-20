using EMS.Domain.Common;
using EMS.Domain.Entities.Employee;

namespace EMS.Domain.Entities.Payroll;

public class SalaryStructure : BaseEntity
{
    public int EmployeeId { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal HRA { get; set; }
    public decimal TransportAllowance { get; set; }
    public decimal MedicalAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal GrossSalary { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public EmployeeProfile? Employee { get; set; }
}
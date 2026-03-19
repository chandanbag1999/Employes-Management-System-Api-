using EMS.Domain.Common;

namespace EMS.Domain.Entities.Payroll;

public class SalaryStructure : BaseEntity
{
    public int EmployeeId { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal HRA { get; set; }              // House Rent Allowance
    public decimal TransportAllowance { get; set; }
    public decimal MedicalAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal GrossSalary { get; set; }      // Calculated
    public DateTime EffectiveFrom { get; set; }
    public bool IsActive { get; set; } = true;
}
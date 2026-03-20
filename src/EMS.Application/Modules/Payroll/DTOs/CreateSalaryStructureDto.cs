using System.ComponentModel.DataAnnotations;

namespace EMS.Application.Modules.Payroll.DTOs;

public class CreateSalaryStructureDto
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    [Range(1, double.MaxValue)]
    public decimal BasicSalary { get; set; }

    [Range(0, double.MaxValue)]
    public decimal HRA { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TransportAllowance { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MedicalAllowance { get; set; }

    [Range(0, double.MaxValue)]
    public decimal OtherAllowances { get; set; }

    [Required]
    public DateTime EffectiveFrom { get; set; }
}
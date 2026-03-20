using System.ComponentModel.DataAnnotations;

namespace EMS.Application.Modules.Payroll.DTOs;

// HR/Admin monthly payroll run karne ke liye
public class RunPayrollDto
{
    [Required]
    [Range(1, 12)]
    public int Month { get; set; }

    [Required]
    [Range(2020, 2100)]
    public int Year { get; set; }

    // Specific employees ke liye ya sab ke liye
    public List<int>? EmployeeIds { get; set; }
}
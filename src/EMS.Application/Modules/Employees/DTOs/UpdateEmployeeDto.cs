using System.ComponentModel.DataAnnotations;
using EMS.Domain.Enums;

namespace EMS.Application.Modules.Employees.DTOs;

public class UpdateEmployeeDto
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    public Gender Gender { get; set; }

    public DateTime DateOfBirth { get; set; }

    public DateTime JoiningDate { get; set; }

    [Required]
    public int DepartmentId { get; set; }

    public int? DesignationId { get; set; }

    public int? ReportingManagerId { get; set; }

    public EmploymentStatus Status { get; set; }

    public DateTime? ProbationEndDate { get; set; }

    public int? UserId { get; set; }
}

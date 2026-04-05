using System.ComponentModel.DataAnnotations;
using EMS.Domain.Enums;

namespace EMS.Application.Modules.Employees.DTOs;

public class CreateEmployeeDto
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

    [Required]
    public Gender Gender { get; set; }

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public DateTime JoiningDate { get; set; }

    [Required]
    public int DepartmentId { get; set; }

    public int? DesignationId { get; set; }

    public int? ReportingManagerId { get; set; }

    // Optional — agar user account bhi banana hai saath mein
    public string? UserPassword { get; set; }

    public DateTime? ProbationEndDate { get; set; }
}
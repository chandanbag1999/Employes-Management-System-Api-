using EMS.Domain.Common;
using EMS.Domain.Enums;
using EMS.Domain.Entities.Organization;

namespace EMS.Domain.Entities.Employee;

public class EmployeeProfile : BaseEntity
{
    // Basic Info
    public string EmployeeCode { get; set; } = string.Empty;  // EMP001
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? ProfilePhotoUrl { get; set; }

    // Employment Info
    public DateTime JoiningDate { get; set; }
    public EmploymentStatus Status { get; set; } = EmploymentStatus.Active;
    public string? ProbationEndDate { get; set; }

    // Organization
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public int? DesignationId { get; set; }
    public Designation? Designation { get; set; }
    public int? ReportingManagerId { get; set; }    // Self-reference

    // Linked User Account
    public int? UserId { get; set; }
}
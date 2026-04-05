using EMS.Domain.Common;
using EMS.Domain.Enums;
using EMS.Domain.Entities.Organization;

namespace EMS.Domain.Entities.Employee;

public class EmployeeProfile : BaseEntity
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public DateTime JoiningDate { get; set; }
    public EmploymentStatus Status { get; set; } = EmploymentStatus.Active;
    public DateTime? ProbationEndDate { get; set; }

    // FKs
    public int DepartmentId { get; set; }
    public int? DesignationId { get; set; }
    public int? ReportingManagerId { get; set; }
    public int? UserId { get; set; }

    // Navigation Properties
    public Department Department { get; set; } = null!;
    public Designation? Designation { get; set; }
    public EmployeeProfile? ReportingManager { get; set; }  // Self-reference
}
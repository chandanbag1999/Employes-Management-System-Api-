using EMS.Domain.Enums;

namespace EMS.Application.Modules.Employees.DTOs;

public class EmployeeResponseDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public int Age => (int)((DateTime.Today - DateOfBirth).TotalDays / 365.25);
    public DateTime? ProbationEndDate { get; set; }
    public DateTime JoiningDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ProfilePhotoUrl { get; set; }

    // Organization
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int? DesignationId { get; set; }
    public string? DesignationTitle { get; set; }
    public int? ReportingManagerId { get; set; }
    public string? ReportingManagerName { get; set; }

    // Linked account
    public int? UserId { get; set; }

    public DateTime CreatedAt { get; set; }
}
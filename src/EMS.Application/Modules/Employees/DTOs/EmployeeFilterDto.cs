using EMS.Domain.Enums;

namespace EMS.Application.Modules.Employees.DTOs;

// Query params ke liye — search, filter, pagination sab ek jagah
public class EmployeeFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }           // Name ya Email se
    public int? DepartmentId { get; set; }
    public int? DesignationId { get; set; }
    public EmploymentStatus? Status { get; set; }
    public Gender? Gender { get; set; }
}
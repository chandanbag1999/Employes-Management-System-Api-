using EMS.Domain.Common;

namespace EMS.Domain.Entities.Organization;

public class Designation : BaseEntity
{
    public string Title { get; set; } = string.Empty;   // e.g. "Software Engineer"
    public string? Description { get; set; }
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
}
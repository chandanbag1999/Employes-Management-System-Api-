using EMS.Domain.Common;
using EMS.Domain.Entities.Employee;

namespace EMS.Domain.Entities.Organization;

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Code { get; set; }        // e.g. "ENG", "HR"
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<EmployeeProfile> Employees { get; set; } = new List<EmployeeProfile>();
}

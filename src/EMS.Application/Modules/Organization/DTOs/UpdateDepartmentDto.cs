using System.ComponentModel.DataAnnotations;

namespace EMS.Application.Modules.Organization.DTOs;

public class UpdateDepartmentDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(10)]
    public string? Code { get; set; }

    public bool IsActive { get; set; } = true;
}
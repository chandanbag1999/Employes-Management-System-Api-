using System.ComponentModel.DataAnnotations;

namespace EMS.Application.Modules.Organization.DTOs;

public class CreateDesignationDto
{
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int? DepartmentId { get; set; }
}
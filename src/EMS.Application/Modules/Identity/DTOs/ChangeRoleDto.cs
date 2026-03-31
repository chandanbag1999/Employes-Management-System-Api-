using System.ComponentModel.DataAnnotations;

namespace EMS.Application.Modules.Identity.DTOs;

public class ChangeRoleDto
{
    [Required]
    [Range(1, 4, ErrorMessage = "Role must be between 1 (SuperAdmin) and 4 (Employee).")]
    public int Role { get; set; }
}
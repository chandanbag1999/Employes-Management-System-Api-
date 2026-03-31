using System.ComponentModel.DataAnnotations;

namespace EMS.Application.Modules.Identity.DTOs;

public class LogoutDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
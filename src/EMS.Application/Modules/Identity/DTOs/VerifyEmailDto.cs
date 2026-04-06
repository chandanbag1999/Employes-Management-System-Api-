using System.ComponentModel.DataAnnotations;

namespace EMS.Application.Modules.Identity.DTOs;

public class VerifyEmailDto
{
    [Required]
    public string Token { get; set; } = string.Empty;
}

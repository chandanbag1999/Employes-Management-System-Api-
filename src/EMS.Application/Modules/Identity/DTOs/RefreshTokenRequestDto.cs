using System.ComponentModel.DataAnnotations;

namespace EMS.Application.Modules.Identity.DTOs;

public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
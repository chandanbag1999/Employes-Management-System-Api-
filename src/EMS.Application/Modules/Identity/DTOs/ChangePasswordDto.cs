using System.ComponentModel.DataAnnotations;

namespace EMS.Application.Modules.Identity.DTOs;

public class ChangePasswordDto
{
    [Required]
    [MaxLength(100)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    [MaxLength(100)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

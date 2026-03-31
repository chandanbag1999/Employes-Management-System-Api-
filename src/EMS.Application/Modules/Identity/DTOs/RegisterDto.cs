using System.ComponentModel.DataAnnotations;

namespace EMS.Application.Modules.Identity.DTOs;

public class RegisterDto
{
    [Required]
    [MaxLength(100)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must have uppercase, lowercase, digit, and special character.")]
    public string Password { get; set; } = string.Empty;

}
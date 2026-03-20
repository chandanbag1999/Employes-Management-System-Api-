using System.ComponentModel.DataAnnotations;

namespace EMS.Application.Modules.Performance.DTOs;

// Employee apna comment add karta hai
public class SelfCommentDto
{
    [Required]
    [MaxLength(1000)]
    public string SelfComment { get; set; } = string.Empty;
}
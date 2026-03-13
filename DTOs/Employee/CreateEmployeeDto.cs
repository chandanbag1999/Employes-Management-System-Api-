using System.ComponentModel.DataAnnotations;

namespace EmployesManagementSystemApi.DTOs.Employee
{
    public class CreateEmployeeDto
    {
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20, MinimumLength = 10)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Position { get; set; } = string.Empty;

        [Range(0.01, 1000000000)]
        public decimal Salary { get; set; }

        [DataType(DataType.Date)]
        public DateTime HireDate { get; set; }

        [Range(1, int.MaxValue)]
        public int DepartmentId { get; set; }
    }
}

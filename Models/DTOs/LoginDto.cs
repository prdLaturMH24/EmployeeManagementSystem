using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Models.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MaxLength(12, ErrorMessage = "Password cannot exceed 12 characters.")]
        [MinLength(4, ErrorMessage = "Password must be at least 4 characters long.")]
        public string Password { get; set; } = string.Empty;
    }
}

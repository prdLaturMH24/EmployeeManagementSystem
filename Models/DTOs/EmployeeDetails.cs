using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Models.DTOs
{
    public class EmployeeDetails : Details
    {
        [Required(ErrorMessage = "Employee ID is required.")]
        public string EmployeeId { get; set; } = string.Empty;      
    }
}

namespace EmployeeManagementSystem.Models.DTOs
{
    public class EmployeeDetailsDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string Position { get; set; } = string.Empty;
        public decimal Salary { get; set; }
    }
}

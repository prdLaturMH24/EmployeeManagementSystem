namespace EmployeeManagementSystem.Models
{
    public class EmployeeDetail
    {
        public int EmployeeDetailId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string Position { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public Employee Employee { get; set; }
}
}

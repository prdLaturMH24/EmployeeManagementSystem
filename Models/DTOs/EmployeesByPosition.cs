namespace EmployeeManagementSystem.Models.DTOs
{
    public class EmployeesByPosition
    {
        public string Position { get; set; } = string.Empty;
        public IEnumerable<EmployeeDetails> Employees { get; set; } = new List<EmployeeDetails>();
    }
}

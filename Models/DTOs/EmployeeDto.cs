namespace EmployeeManagementSystem.Models.DTOs
{
    public class EmployeeDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        public override bool Equals(object obj)
        {
            EmployeeDto other = (EmployeeDto)obj;
            return this.EmployeeId == other.EmployeeId;
        }

        public override int GetHashCode()
        {
            return EmployeeId.GetHashCode();
        }
    }
}

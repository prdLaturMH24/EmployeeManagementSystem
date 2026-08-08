namespace EmployeeManagementSystem.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        //FK
        public int EmployeeDetailId { get; set; }
        public EmployeeDetail? EmployeeDetail { get; set; }

        //One to Many Relationship between Employee and EmployeeDetail
        //public ICollection<EmployeeDetail> EmployeeDetails { get; set; } = new List<EmployeeDetail>();
    }
}

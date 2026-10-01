namespace EmployeeManagementSystem.Models
{
    /// <summary>
    /// Represents a many-to-many relationship between AppUser and Role.
    /// </summary>
    public class UserRole
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public AppUser? AppUser { get; set; }
        public Role? Role { get; set; }
    }
}

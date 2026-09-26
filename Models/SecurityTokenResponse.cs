namespace EmployeeManagementSystem.Models
{
    public class SecurityTokenResponse
    {
        public string UserName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public SecurityTokenResponse() { }
    }
}

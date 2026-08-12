using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Repository
{
    public interface IEmployeeRepository
    {
        public Task<List<Employee>> GetAllEmployeesAsync();
        public Task<List<Employee>> GetAllEmployeesWithDetailsAsync();
        public Task<Dictionary<string, List<Employee>>> GetAllEmployeesByPositionAsync();
    }
}

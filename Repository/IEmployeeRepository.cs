using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Repository
{
    public interface IEmployeeRepository
    {
        Task<List<Employee>> GetAllEmployeesAsync();
        Task<List<Employee>> GetAllEmployeesWithDetailsAsync();
        Task<Dictionary<string, List<Employee>>> GetAllEmployeesByPositionAsync();
        Task<bool> EmployeeExistsAsync(string employeeId);
        Task<Employee?> GetEmployeeByIdAsync(string employeeId);
        Task<bool> AddEmployeeAsync(Employee employee);
        Task<bool> UpdateEmployeeAsync(Employee employee);
        Task<bool> DeleteEmployeeAsync(Employee employee);
    }
}

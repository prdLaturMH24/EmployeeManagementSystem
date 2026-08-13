using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Repository
{
    public class EmployeeRepository(EmployeeDbContext context) : IEmployeeRepository
    {
        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            return await context.Employees
                .Where(e => !string.IsNullOrWhiteSpace(e.FirstName) && !string.IsNullOrWhiteSpace(e.LastName))
                .ToListAsync();
        }

        public async Task<List<Employee>> GetAllEmployeesWithDetailsAsync()
        {
            return await context.Employees
                .Include(e => e.EmployeeDetail)
                .Where(e => !string.IsNullOrWhiteSpace(e.FirstName)
                    && !string.IsNullOrWhiteSpace(e.LastName) 
                    && e.EmployeeDetail != null)
                .ToListAsync();
        }

        public async Task<Dictionary<string, List<Employee>>> GetAllEmployeesByPositionAsync()
        {
            return await context.Employees
                .Include(e => e.EmployeeDetail)
                .Where(e => e.EmployeeDetail != null && !string.IsNullOrWhiteSpace(e.EmployeeDetail.Position))
                .GroupBy(e => e.EmployeeDetail!.Position)
                .ToDictionaryAsync(g => g.Key, g => g.ToList());
        }
    }
}

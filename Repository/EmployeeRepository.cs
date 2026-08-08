using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Repository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly EmployeeDbContext _employeeDbcontext;

        public EmployeeRepository(EmployeeDbContext context)
        {
            _employeeDbcontext = context;
        }

        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            return await _employeeDbcontext.Employees.ToListAsync();
        }

        public async Task<List<Employee>> GetAllEmployeesWithDetailsAsync()
        {
            return await _employeeDbcontext.Employees
                .Include(e => e.EmployeeDetail)
                .ToListAsync();
        }
    }
}

using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Repository
{
    public class EmployeeRepository(EmployeeDbContext context) : IEmployeeRepository
    {
        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            //Read Only, no tracking, filter out employees with null or empty first name or last name
            return await context.Employees
                .AsNoTracking()
                .Where(e => !string.IsNullOrWhiteSpace(e.FirstName) && !string.IsNullOrWhiteSpace(e.LastName))
                .ToListAsync();
        }

        public async Task<List<Employee>> GetAllEmployeesWithDetailsAsync()
        {
            //Read Only, no tracking, include EmployeeDetail,
            //filter out employees with null or empty first name or last name,
            //and filter out employees with null EmployeeDetail
            return await context.Employees
                .AsNoTracking()
                .Include(e => e.EmployeeDetail)
                .Where(e => !string.IsNullOrWhiteSpace(e.FirstName)
                    && !string.IsNullOrWhiteSpace(e.LastName) 
                    && e.EmployeeDetail != null)
                .ToListAsync();
        }

        public async Task<Employee?> GetEmployeeByIdAsync(string employeeId)
        {
            var employee = await context.Employees
                .Include(e => e.EmployeeDetail)
                .FirstOrDefaultAsync(e => e.AssociateId == employeeId);
            //var changeTracking = context.Employees.Entry(employee);
            return employee;
        }

        public async Task<bool> EmployeeExistsAsync(string employeeId)
        {
            return await context.Employees.AnyAsync(e => e.AssociateId == employeeId);
        }

        public async Task<Dictionary<string, List<Employee>>> GetAllEmployeesByPositionAsync()
        {
            return await context.Employees
                .Include(e => e.EmployeeDetail)
                .Where(e => e.EmployeeDetail != null && !string.IsNullOrWhiteSpace(e.EmployeeDetail.Position))
                .GroupBy(e => e.EmployeeDetail!.Position)
                .ToDictionaryAsync(g => g.Key, g => g.ToList());
        }

        public async Task<bool> AddEmployeeAsync(Employee employee)
        {
            await context.Employees.AddAsync(employee);
            var addedEmployee = await context.SaveChangesAsync();
            return addedEmployee > 0;
        }

        public async Task<bool> UpdateEmployeeAsync(Employee employee)
        {
            //var changeTracking = context.Employees.Entry(employee);
            context.Employees.Update(employee);
            var updatedEmployee = await context.SaveChangesAsync();
            return updatedEmployee > 0;
        }

        public async Task<bool> DeleteEmployeeAsync(Employee employee)
        {
            context.Employees.Remove(employee);
            //State = Deleted
            //var changeTracking = context.Employees.Entry(employee);
            var deletedEmployee = await context.SaveChangesAsync();
            return deletedEmployee > 0;
        }
    }
}

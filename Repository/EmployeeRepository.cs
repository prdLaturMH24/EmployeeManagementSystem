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
            //Query Syntax
            /*return await (from employee in _employeeDbcontext.Employees
                        join detail in _employeeDbcontext.EmployeeDetails
                        on employee.Id equals detail.EmployeeId
                        select new Employee
                        {
                            Id = employee.Id,
                            AssociateId = employee.AssociateId,
                            FirstName = employee.FirstName,
                            LastName = employee.LastName,
                            EmployeeDetail = new EmployeeDetail
                            {
                                EmployeeDetailId = detail.EmployeeDetailId,
                                Email = detail.Email,
                                PhoneNumber = detail.PhoneNumber,
                                Address = detail.Address,
                                DateOfBirth = detail.DateOfBirth,
                                Position = detail.Position,
                                Salary = detail.Salary,
                                EmployeeId = detail.EmployeeId
                            }
                        }).ToListAsync();*/

            //Method Syntax (Fluent Syntax)
            /*return await _employeeDbcontext.Employees
                .Join(_employeeDbcontext.EmployeeDetails,
                    employee => employee.Id,
                    detail => detail.EmployeeId,
                    (employee, detail) => new Employee
                    {
                        Id = employee.Id,
                        AssociateId = employee.AssociateId,
                        FirstName = employee.FirstName,
                        LastName = employee.LastName,
                        EmployeeDetail = new EmployeeDetail
                        {
                            EmployeeDetailId = detail.EmployeeDetailId,
                            Email = detail.Email,
                            PhoneNumber = detail.PhoneNumber,
                            Address = detail.Address,
                            DateOfBirth = detail.DateOfBirth,
                            Position = detail.Position,
                            Salary = detail.Salary,
                            EmployeeId = detail.EmployeeId
                        }
                    }).ToListAsync();*/
          
            return await _employeeDbcontext.Employees
                .Include(e => e.EmployeeDetail)
                .ToListAsync();
        }

        public async Task<Dictionary<string, List<Employee>>> GetAllEmployeesByPositionAsync()
        {
            //return await (
            //    from employee in _employeeDbcontext.Employees
            //    join detail in _employeeDbcontext.EmployeeDetails
            //    on employee.Id equals detail.EmployeeId
            //    orderby detail.Position
            //    group employee by detail.Position into g
            //    select new { Position = g.Key, Employees = g.ToList() }
            //    ).ToDictionaryAsync(x => x.Position, x => x.Employees);

            return await _employeeDbcontext.Employees
                .Include(e => e.EmployeeDetail)
                .GroupBy(e => e.EmployeeDetail!.Position)
                .ToDictionaryAsync(g => g.Key, g => g.ToList());
        }
    }
}

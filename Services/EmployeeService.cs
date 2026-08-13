using EmployeeManagementSystem.Models.DTOs;
using EmployeeManagementSystem.Repository;

namespace EmployeeManagementSystem.Services
{
    public class EmployeeService(IEmployeeRepository employeeRepository)
    {
        public async Task<List<EmployeeDto>> GetEmployeesAsync()
        {
           var employees = await employeeRepository.GetAllEmployeesAsync();
           return employees
                .Select(e => new EmployeeDto
                   {
                       EmployeeId = e.AssociateId,
                       FullName = e.FirstName + " " + e.LastName
                   }).ToList();
        }

        public async Task<List<EmployeeDetails>> GetEmployeeDetailsAsync()
        {
            var employees = await employeeRepository.GetAllEmployeesWithDetailsAsync();
            return employees
                .Select(e =>
                {
                    var details = e.EmployeeDetail!;
                    var employeeDetails = new EmployeeDetails
                    {
                        EmployeeId = e.AssociateId,
                        FirstName = e.FirstName,
                        LastName = e.LastName,
                        Email = details.Email,
                        PhoneNumber = details.PhoneNumber,
                        Address = details.Address,
                        DateOfBirth = details.DateOfBirth,
                        Position = details.Position,
                        Salary = details.Salary
                    };
                    return employeeDetails;
                }).ToList();
        }
    }
}

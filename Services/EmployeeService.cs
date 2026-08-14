using EmployeeManagementSystem.Models;
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

        public async Task<bool> AddEmployeeAsync(EmployeeDetails employeeDetails)
        {
            var employeeDetail = new EmployeeDetail
            {
                Email = employeeDetails.Email,
                PhoneNumber = employeeDetails.PhoneNumber,
                Address = employeeDetails.Address,
                DateOfBirth = employeeDetails.DateOfBirth,
                Position = employeeDetails.Position,
                Salary = employeeDetails.Salary
            };

            var employee = new Employee
            {
                AssociateId = employeeDetails.EmployeeId,
                FirstName = employeeDetails.FirstName,
                LastName = employeeDetails.LastName,
                EmployeeDetail = employeeDetail
            };

            return await employeeRepository.AddEmployeeAsync(employee);
        }

        public async Task<bool> EmployeeExistsAsync(string employeeId)
        {
            return await employeeRepository.EmployeeExistsAsync(employeeId);
        }

        public async Task<bool> UpdateEmployeeAsync(string employeeId, Details employeeDetails)
        {
            var existingEmployee = await employeeRepository.GetEmployeeByIdAsync(employeeId);
            //State = Unchanged
            if (existingEmployee == null)
            {
                return false; // Employee not found
            }
            existingEmployee.FirstName = employeeDetails.FirstName;
            existingEmployee.LastName = employeeDetails.LastName;
            if (existingEmployee.EmployeeDetail != null)
            {
                existingEmployee.EmployeeDetail.Email = employeeDetails.Email;
                existingEmployee.EmployeeDetail.PhoneNumber = employeeDetails.PhoneNumber;
                existingEmployee.EmployeeDetail.Address = employeeDetails.Address;
                existingEmployee.EmployeeDetail.DateOfBirth = employeeDetails.DateOfBirth;
                existingEmployee.EmployeeDetail.Position = employeeDetails.Position;
                existingEmployee.EmployeeDetail.Salary = employeeDetails.Salary;
            }
            //State = Modified
            return await employeeRepository.UpdateEmployeeAsync(existingEmployee);
        }

        public async Task<bool> DeleteEmployeeAsync(string employeeId)
        {
            var existingEmployee = await employeeRepository.GetEmployeeByIdAsync(employeeId);
            if (existingEmployee == null)
            {
                return false; // Employee not found
            }
            return await employeeRepository.DeleteEmployeeAsync(existingEmployee);
        }
    }
}

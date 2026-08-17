using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Models.DTOs;
using EmployeeManagementSystem.Repository;
using EmployeeManagementSystem.Utilities.Delegates;

namespace EmployeeManagementSystem.Services
{
    /// <summary>
    /// Employee service demonstrating practical use of C# delegates.
    /// 
    /// Delegates enable:
    /// - Flexible validation pipelines without code modification
    /// - Event notifications and audit logging
    /// - Composable salary processing with callbacks
    /// - Custom filtering predicates for employee queries
    /// 
    /// See EmployeeValidationDelegate, EmployeeNotificationDelegate, 
    /// and EmployeeProcessingDelegate for implementation details.
    /// </summary>
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

        // ========== DELEGATE-BASED METHODS ==========
        // The following methods demonstrate practical use of C# delegates

        /// <summary>
        /// Filters employees using a custom predicate delegate.
        /// 
        /// USE CASE: Flexible filtering without hardcoding filter methods
        /// DELEGATE CONCEPT: Func<Employee, bool> (Predicate<T>)
        /// 
        /// Example predicates:
        /// - emp => emp.FirstName.StartsWith("J")
        /// - emp => emp.EmployeeDetail?.Salary > 50000
        /// - emp => emp.EmployeeDetail?.Position == "Manager"
        /// </summary>
        public async Task<List<Employee>> FilterEmployeesAsync(
            Func<Employee, bool> filterPredicate)
        {
            var employees = await employeeRepository.GetAllEmployeesAsync();
            return employees.Where(filterPredicate).ToList();
        }

        /// <summary>
        /// Filters employees with details using a custom predicate.
        /// 
        /// USE CASE: Complex filtering that requires employee details
        /// DELEGATE CONCEPT: Func<Employee, bool> for flexible predicates
        /// 
        /// Example: FilterEmployeesWithDetailsAsync(e => e.EmployeeDetail?.Salary > 50000)
        /// </summary>
        public async Task<List<Employee>> FilterEmployeesWithDetailsAsync(
            Func<Employee, bool> filterPredicate)
        {
            var employees = await employeeRepository.GetAllEmployeesWithDetailsAsync();
            return employees.Where(filterPredicate).ToList();
        }

        /// <summary>
        /// Validates employee before adding to system.
        /// 
        /// USE CASE: Extensible validation pipeline
        /// DELEGATE CONCEPT: Custom EmployeeValidationDelegate.EmployeeValidationRule
        /// 
        /// Benefits:
        /// - Add validation rules without modifying AddEmployeeAsync
        /// - Each validation rule is independent
        /// - Compose validations at runtime
        /// </summary>
        public async Task<(bool success, List<string> errors)> ValidateAndAddEmployeeAsync(
            EmployeeDetailsDto employeeDetails,
            EmployeeValidationDelegate.EmployeeValidationPipeline? validationPipeline = null)
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

            // Use standard validation if none provided
            if (validationPipeline == null)
            {
                validationPipeline = EmployeeValidationDelegate.EmployeeValidationPipeline
                    .CreateStandardPipeline();
            }

            var (isValid, errors) = validationPipeline.ValidateEmployee(employee);

            if (!isValid)
            {
                return (false, errors);
            }

            var addSuccess = await employeeRepository.AddEmployeeAsync(employee);
            return (addSuccess, []);
        }

        /// <summary>
        /// Searches employees using a custom search predicate.
        /// 
        /// USE CASE: Flexible search without multiple SearchByXXX methods
        /// DELEGATE CONCEPT: Func<Employee, bool> for search logic
        /// 
        /// Examples:
        /// - searchPredicate: e => e.FirstName.Contains("John")
        /// - searchPredicate: e => e.AssociateId == "EMP123"
        /// - searchPredicate: e => e.EmployeeDetail?.Position.Contains("Manager")
        /// </summary>
        public async Task<List<Employee>> SearchEmployeesAsync(
            Func<Employee, bool> searchPredicate)
        {
            var employees = await employeeRepository.GetAllEmployeesWithDetailsAsync();
            return employees.Where(searchPredicate).ToList();
        }

        /// <summary>
        /// Gets employees filtered by a custom validation rule.
        /// 
        /// USE CASE: Retrieve only employees meeting specific business rules
        /// DELEGATE CONCEPT: Func<Employee, bool> validation predicate
        /// 
        /// Example: GetValidEmployeesAsync(e => e.FirstName != null && e.LastName != null)
        /// </summary>
        public async Task<List<EmployeeDetails>> GetValidEmployeesAsync(
            Func<Employee, bool> validationRule)
        {
            var employees = await employeeRepository.GetAllEmployeesWithDetailsAsync();
            var validEmployees = employees.Where(validationRule).ToList();

            return validEmployees
                .Select(e =>
                {
                    var details = e.EmployeeDetail!;
                    return new EmployeeDetails
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
                }).ToList();
        }

        /// <summary>
        /// Transforms employees using a custom transformation delegate.
        /// 
        /// USE CASE: Flexible projection/mapping logic
        /// DELEGATE CONCEPT: Func<Employee, T> for transformations
        /// 
        /// Example: TransformEmployeesAsync(e => e.FirstName + " " + e.LastName)
        /// 
        /// This demonstrates:
        /// - Generic delegates with type parameters
        /// - Separation of transformation logic
        /// - Reusable projection patterns
        /// </summary>
        public async Task<List<T>> TransformEmployeesAsync<T>(
            Func<Employee, T> transformFunction)
        {
            var employees = await employeeRepository.GetAllEmployeesAsync();
            return employees.Select(transformFunction).ToList();
        }

        /// <summary>
        /// Executes an action on each employee without modifying them.
        /// 
        /// USE CASE: Side effects like logging, notifications, validation
        /// DELEGATE CONCEPT: Action<T> for operations without return value
        /// 
        /// Examples:
        /// - action: e => Console.WriteLine(e.FirstName)
        /// - action: e => LogEmployeeEvent(e)
        /// - action: e => SendNotification(e)
        /// </summary>
        public async Task ForEachEmployeeAsync(
            Action<Employee> action)
        {
            var employees = await employeeRepository.GetAllEmployeesAsync();
            foreach (var employee in employees)
            {
                action(employee);
            }
        }

        /// <summary>
        /// Finds first employee matching predicate or returns null.
        /// 
        /// USE CASE: Dynamic employee lookup
        /// DELEGATE CONCEPT: Func<Employee, bool> predicate
        /// 
        /// Example: FindEmployeeAsync(e => e.FirstName == "John" && e.LastName == "Doe")
        /// </summary>
        public async Task<Employee?> FindEmployeeAsync(
            Func<Employee, bool> predicate)
        {
            var employees = await employeeRepository.GetAllEmployeesWithDetailsAsync();
            return employees.FirstOrDefault(predicate);
        }

        /// <summary>
        /// Gets employees meeting criteria using a composition of predicates.
        /// 
        /// USE CASE: Complex filtering with multiple conditions
        /// DELEGATE CONCEPT: Composing multiple Func<Employee, bool> predicates
        /// 
        /// Demonstrates:
        /// - Combining predicates with logical operators
        /// - Reusable predicate composition
        /// - Complex queries without LINQ complexity
        /// </summary>
        public async Task<List<Employee>> GetEmployeesByCriteriaAsync(
            params Func<Employee, bool>[] criteria)
        {
            var employees = await employeeRepository.GetAllEmployeesWithDetailsAsync();

            // Combine all criteria with AND logic
            return employees.Where(emp =>
                criteria.All(predicate => predicate(emp))
            ).ToList();
        }

        /// <summary>
        /// Gets employees by position using a predefined validation predicate.
        /// 
        /// USE CASE: Position-based filtering with validation
        /// DELEGATE CONCEPT: Reusing Func<Employee, bool> predicates
        /// 
        /// Uses the ValidationPredicates from EmployeeValidationDelegate.
        /// </summary>
        public async Task<List<Employee>> GetEmployeesByPositionAsync(
            string position)
        {
            // Combine position filter with validation predicates
            return await FilterEmployeesWithDetailsAsync(emp =>
                EmployeeValidationDelegate.ValidationPredicates.HasEmployeeDetails(emp) &&
                emp.EmployeeDetail!.Position.Equals(position, StringComparison.OrdinalIgnoreCase)
            );
        }
    }
}

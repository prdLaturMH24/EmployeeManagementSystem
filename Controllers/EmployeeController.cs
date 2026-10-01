using EmployeeManagementSystem.Models.DTOs;
using EmployeeManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.Controllers
{
    /// <summary>
    /// Employee management endpoints with role-based authorization.
    /// All endpoints require authentication (Bearer token).
    /// - GET endpoints: AssociateOrHigher (all roles)
    /// - POST/PUT endpoints: ManagerOrHigher
    /// - DELETE endpoints: AdminOnly
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController(EmployeeService employeeService) : ControllerBase
    {
        /// <summary>
        /// Get all employees. Accessible by all authenticated users.
        /// </summary>
        [Authorize(Policy = "AssociateOrHigher")]
        [HttpGet("employees")]
        [ProducesResponseType(typeof(List<EmployeeDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<List<EmployeeDto>>> GetEmployees()
        {
            var employees = await employeeService.GetEmployeesAsync();
            return Ok(employees);
        }

        /// <summary>
        /// Get detailed employee information. Accessible by all authenticated users.
        /// </summary>
        [Authorize(Policy = "AssociateOrHigher")]
        [HttpGet("employees/details")]
        [ProducesResponseType(typeof(List<EmployeeDetails>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<List<EmployeeDetails>>> GetEmployeeDetails()
        {
            var employeeDetails = await employeeService.GetEmployeeDetailsAsync();
            return Ok(employeeDetails);
        }

        /// <summary>
        /// Add a new employee. Only Managers and Admins can create employees.
        /// </summary>
        [Authorize(Policy = "ManagerOrHigher")]
        [HttpPost("employees")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult> AddEmployee([FromBody] EmployeeDetails employeeDetails)
        {
            if (employeeDetails == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid employee details." });
            }
            var employeeExists = await employeeService.EmployeeExistsAsync(employeeDetails.EmployeeId);
            if (employeeExists)
            {
                return BadRequest(new { message = "Employee with the specified ID already exists." });
            }

            var result = await employeeService.AddEmployeeAsync(employeeDetails);

            if (result)
            {
                return Created($"/employees/{employeeDetails.EmployeeId}", new { message = "Employee added successfully." });
            }
            return Problem($"An error occurred while adding the employee.");
        }

        /// <summary>
        /// Add a new employee with extended details. Only Managers and Admins can create employees.
        /// </summary>
        [Authorize(Policy = "ManagerOrHigher")]
        [HttpPost("employees/add")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult> AddEmployeeExtra([FromBody] EmployeeDetailsDto employeeDetails)
        {
            if (employeeDetails == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid employee details." });
            }
            var employeeExists = await employeeService.EmployeeExistsAsync(employeeDetails.EmployeeId);
            if (employeeExists)
            {
                return BadRequest(new { message = "Employee with the specified ID already exists." });
            }

            var result = await employeeService.ValidateAndAddEmployeeAsync(employeeDetails);
            if (result.success)
            {
                return Created($"/employees/{employeeDetails.EmployeeId}", new { message = "Employee added successfully." });
            }
            return Problem($"An error occurred while adding the employee. {string.Join(", ", result.errors)}");
        }

        /// <summary>
        /// Update an existing employee. Only Managers and Admins can update employees.
        /// </summary>
        [Authorize(Policy = "ManagerOrHigher")]
        [HttpPut("employees/{employeeId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult> UpdateEmployee(
            [FromRoute] string employeeId,
            [FromBody] Details employeeDetails)
        {
            if (employeeDetails == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid employee details." });
            }

            var employeeExists = await employeeService.EmployeeExistsAsync(employeeId);
            if (!employeeExists)
            {
                return NotFound(new { message = "Employee not found." });
            }

            var result = await employeeService.UpdateEmployeeAsync(employeeId, employeeDetails);
            if (result)
            {
                return Ok(new { message = "Employee updated successfully." });
            }
            return Problem("An error occurred while updating the employee.");
        }

        /// <summary>
        /// Delete an employee. Only Admins can delete employees.
        /// </summary>
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("employees/{employeeId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult> DeleteEmployee([FromRoute] string employeeId)
        {
            var employeeExists = await employeeService.EmployeeExistsAsync(employeeId);
            if (!employeeExists)
            {
                return NotFound(new { message = "Employee not found." });
            }

            var result = await employeeService.DeleteEmployeeAsync(employeeId);
            if (result)
            {
                return NoContent();
            }
            return Problem("An error occurred while deleting the employee.");
        }
    }
}

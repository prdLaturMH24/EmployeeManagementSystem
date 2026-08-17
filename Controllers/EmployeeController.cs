using EmployeeManagementSystem.Models.DTOs;
using EmployeeManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController(EmployeeService employeeService) : ControllerBase
    {
        [HttpGet("employees")]
        public async Task<ActionResult<List<EmployeeDto>>> GetEmployees()
        {
            var employees = await employeeService.GetEmployeesAsync();
            return Ok(employees);
        }

        [HttpGet("employees/details")]
        public async Task<ActionResult<List<EmployeeDetails>>> GetEmployeeDetails()
        {
            var employeeDetails = await employeeService.GetEmployeeDetailsAsync();
            return Ok(employeeDetails);
        }

        [HttpPost("employees")]
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

        [HttpPost("employees/add")]
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

        [HttpPut("employees/{employeeId}")]
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

        [HttpDelete("employees/{employeeId}")]
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

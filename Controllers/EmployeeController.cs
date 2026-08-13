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
    }
}

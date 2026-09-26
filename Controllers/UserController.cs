using EmployeeManagementSystem.Models.DTOs;
using EmployeeManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _accountService;
        private readonly SecurityTokenService _tokenService;

        public UserController(UserService accountService, SecurityTokenService tokenService)
        {
            _accountService = accountService;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto userDto)
        {

            var user = await _accountService.RegisterUserAsync(userDto);
            if (user == null)
            {
                return BadRequest("User already exists.");
            }
            var token = await _tokenService.GenerateSecurityTokenAsync(user);
            return Ok(token);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _accountService.LoginUserAsync(loginDto);
            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }
            var token = await _tokenService.GenerateSecurityTokenAsync(user);
            return Ok(token);
        }
    }
}

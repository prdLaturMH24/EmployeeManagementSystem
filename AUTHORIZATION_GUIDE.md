# Role-Based Authorization Guide

## Overview

This Employee Management System implements role-based authorization using ASP.NET Core's built-in `IAuthorizationService` with policy-based authorization. Three roles are supported: **Admin**, **Manager**, and **Associate**.

## Role Hierarchy

The system enforces a hierarchical role structure where higher-level roles can perform lower-level operations:

```
Admin (Level 3)
  ↓
Manager (Level 2)
  ↓
Associate (Level 1)
```

- **Admin**: Full system access, can manage all resources and users
- **Manager**: Can manage resources and Associates, but not other Managers or Admins
- **Associate**: Limited access, primarily to own resources

## Authorization Policies

Six authorization policies are configured in `Program.cs`:

### Simple Role Policies (Exact Role Match)

```csharp
// AdminOnly - Only users with Admin role
[Authorize(Policy = "AdminOnly")]
public IActionResult AdminFunction() { ... }

// ManagerOnly - Only users with Manager role
[Authorize(Policy = "ManagerOnly")]
public IActionResult ManagerFunction() { ... }

// AssociateOnly - Only users with Associate role
[Authorize(Policy = "AssociateOnly")]
public IActionResult AssociateFunction() { ... }
```

### Hierarchical Policies (Role Level or Higher)

```csharp
// AdminOrHigher - Only Admins (usually just Admin level)
[Authorize(Policy = "AdminOrHigher")]
public IActionResult AdminOnlyAction() { ... }

// ManagerOrHigher - Managers and Admins (levels 2-3)
[Authorize(Policy = "ManagerOrHigher")]
public IActionResult ManagerLevelAction() { ... }

// AssociateOrHigher - All roles (levels 1-3)
[Authorize(Policy = "AssociateOrHigher")]
public IActionResult AllAuthenticatedUsers() { ... }
```

### Built-in Role Attribute

Use the built-in `[Authorize(Roles = "...")]` for direct role checking:

```csharp
// Check specific roles
[Authorize(Roles = "Admin,Manager")]
public IActionResult SensitiveData() { ... }

// Require authentication but allow any role
[Authorize]
public IActionResult UserProfile() { ... }
```

## Controller Examples

### Employee Controller with Authorization

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmployeeManagementSystem.Services;

namespace EmployeeManagementSystem.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class EmployeeController : ControllerBase
	{
		private readonly EmployeeService _employeeService;

		public EmployeeController(EmployeeService employeeService)
		{
			_employeeService = employeeService;
		}

		// Any authenticated user can view their own data
		[Authorize(Policy = "AssociateOrHigher")]
		[HttpGet("{id}")]
		public async Task<IActionResult> GetEmployee(int id)
		{
			var employee = await _employeeService.GetEmployeeAsync(id);
			return Ok(employee);
		}

		// Only Managers and above can create employees
		[Authorize(Policy = "ManagerOrHigher")]
		[HttpPost]
		public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDto dto)
		{
			var employee = await _employeeService.CreateEmployeeAsync(dto);
			return Created($"api/employee/{employee.Id}", employee);
		}

		// Only Admins can update employee data
		[Authorize(Policy = "AdminOnly")]
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto dto)
		{
			var employee = await _employeeService.UpdateEmployeeAsync(id, dto);
			return Ok(employee);
		}

		// Only Admins can delete employees
		[Authorize(Policy = "AdminOnly")]
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteEmployee(int id)
		{
			await _employeeService.DeleteEmployeeAsync(id);
			return NoContent();
		}

		// Public endpoint - no authorization needed
		[HttpGet("count")]
		public async Task<IActionResult> GetEmployeeCount()
		{
			var count = await _employeeService.GetEmployeeCountAsync();
			return Ok(count);
		}
	}
}
```

### User Management Controller

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]  // All endpoints require Admin role
public class UserManagementController : ControllerBase
{
	private readonly IUserRepository _userRepository;
	private readonly IRoleRepository _roleRepository;

	public UserManagementController(IUserRepository userRepository, IRoleRepository roleRepository)
	{
		_userRepository = userRepository;
		_roleRepository = roleRepository;
	}

	[HttpGet]
	public async Task<IActionResult> GetAllUsers()
	{
		var users = await _userRepository.ListUsersAsync();
		return Ok(users);
	}

	[HttpPost("{userId}/roles/{roleId}")]
	public async Task<IActionResult> AssignRoleToUser(int userId, int roleId)
	{
		var success = await _roleRepository.AssignRoleToUserAsync(userId, roleId);
		if (!success)
			return BadRequest("Role assignment failed");
		return Ok();
	}

	[HttpDelete("{userId}/roles/{roleId}")]
	public async Task<IActionResult> RemoveRoleFromUser(int userId, int roleId)
	{
		var success = await _roleRepository.RemoveRoleFromUserAsync(userId, roleId);
		if (!success)
			return BadRequest("Role removal failed");
		return Ok();
	}
}
```

## Imperative Authorization

For complex authorization logic within action methods, use the built-in `IAuthorizationService`:

```csharp
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class DocumentController : ControllerBase
{
	private readonly IAuthorizationService _authorizationService;
	private readonly IDocumentRepository _documentRepository;

	public DocumentController(
		IAuthorizationService authorizationService,
		IDocumentRepository documentRepository)
	{
		_authorizationService = authorizationService;
		_documentRepository = documentRepository;
	}

	[Authorize]
	[HttpGet("{id}")]
	public async Task<IActionResult> GetDocument(int id)
	{
		var document = await _documentRepository.GetDocumentAsync(id);

		// Check if user has ManagerOrHigher policy
		var authResult = await _authorizationService.AuthorizeAsync(
			User, "ManagerOrHigher");

		if (authResult.Succeeded)
		{
			// Manager or Admin can see all document details
			return Ok(document);
		}

		// Associates can only see public information
		return Ok(new { document.Id, document.Title });
	}
}
```

## JWT Token & Claims

When a user logs in, the JWT token includes role claims:

```json
{
  "iss": "YourIssuer",
  "sub": "1",
  "name": "john.doe",
  "email": "john@example.com",
  "role": ["Admin", "Manager"],
  "iat": 1234567890,
  "exp": 1234654290
}
```

The built-in authorization middleware automatically:
1. Extracts role claims from the token
2. Validates them against policies
3. Denies access if the user lacks required roles

## Testing Authorization

### Unit Testing with Mock Authorization

```csharp
[Fact]
public async Task UpdateEmployee_WithManagerRole_ReturnsOk()
{
	// Arrange
	var mockAuthService = new Mock<IAuthorizationService>();
	mockAuthService
		.Setup(a => a.AuthorizeAsync(User, "ManagerOrHigher"))
		.ReturnsAsync(AuthorizationResult.Success());

	var controller = new EmployeeController(mockAuthService.Object, employeeService);

	// Act
	var result = await controller.UpdateEmployee(1, dto);

	// Assert
	Assert.IsType<OkResult>(result);
}
```

## Troubleshooting

### 401 Unauthorized
- User is not authenticated (missing/invalid JWT token)
- Check: Is the user logged in? Is the token valid?

### 403 Forbidden
- User is authenticated but lacks required role
- Check: User has the required role assigned in the database
- Check: Role claims are included in the JWT token

### Policy Not Working
- Verify policy name matches exactly (case-sensitive)
- Ensure `[Authorize]` attribute is on the action/controller
- Check Program.cs for policy configuration

## Database Setup

To assign roles to users:

```sql
-- Assign Admin role to user
INSERT INTO Employee.UserRole (UserId, RoleId)
SELECT u.Id, r.Id
FROM Employee.AppUser u, Employee.Role r
WHERE u.UserName = 'admin.user' AND r.Name = 'Admin';

-- Assign Manager role to user
INSERT INTO Employee.UserRole (UserId, RoleId)
SELECT u.Id, r.Id
FROM Employee.AppUser u, Employee.Role r
WHERE u.UserName = 'manager.user' AND r.Name = 'Manager';
```

Or use the UserManagementController API endpoint to manage role assignments.

## Summary

- **Use `[Authorize(Policy = "...")]`** for declarative authorization on controllers/actions
- **Use `IAuthorizationService`** for imperative authorization within methods
- **Role hierarchy ensures consistency**: Admin > Manager > Associate
- **JWT tokens include role claims** automatically via SecurityTokenService
- **Simple policies** (AdminOnly, ManagerOnly) for exact role matching
- **Hierarchical policies** (AdminOrHigher, ManagerOrHigher) for role level checking

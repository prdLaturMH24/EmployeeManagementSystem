# ASP.NET Core Built-in RBAC Implementation - Quick Reference

## What Changed

We've refactored from a custom authorization service to **ASP.NET Core's built-in `IAuthorizationService`** with policy-based authorization and custom handlers for hierarchical role checking.

## Key Components

### 1. **RoleConstants.cs** (`Common/Constants/`)
```csharp
public static class RoleConstants
{
	public const string Admin = "Admin";
	public const string Manager = "Manager";
	public const string Associate = "Associate";
}
```

### 2. **IRoleRepository.cs** & **RoleRepository.cs** (`Repository/`)
- Data access layer for roles
- Methods: `GetUserRolesAsync()`, `UserHasRoleAsync()`, `AssignRoleToUserAsync()`, etc.
- ✅ **Kept** (no naming conflict with built-in service)

### 3. **HierarchicalRoleRequirement.cs** (`Authorization/Requirements/`)
Custom requirement class implementing `IAuthorizationRequirement`
```csharp
public class HierarchicalRoleRequirement : IAuthorizationRequirement
{
	public string MinimumRole { get; }
	public HierarchicalRoleRequirement(string minimumRole) { ... }
}
```

### 4. **HierarchicalRoleAuthorizationHandler.cs** (`Authorization/Handlers/`)
Custom handler implementing `AuthorizationHandler<HierarchicalRoleRequirement>`
```csharp
public class HierarchicalRoleAuthorizationHandler 
	: AuthorizationHandler<HierarchicalRoleRequirement>
{
	protected override async Task HandleRequirementAsync(
		AuthorizationHandlerContext context,
		HierarchicalRoleRequirement requirement) { ... }
}
```

### 5. **Program.cs** Configuration
```csharp
// Configure authorization policies
builder.Services.AddAuthorizationBuilder()
	.AddPolicy("AdminOnly", policy => policy.RequireRole(RoleConstants.Admin))
	.AddPolicy("ManagerOnly", policy => policy.RequireRole(RoleConstants.Manager))
	.AddPolicy("AssociateOnly", policy => policy.RequireRole(RoleConstants.Associate))
	.AddPolicy("AdminOrHigher", policy =>
		policy.Requirements.Add(new HierarchicalRoleRequirement(RoleConstants.Admin)))
	.AddPolicy("ManagerOrHigher", policy =>
		policy.Requirements.Add(new HierarchicalRoleRequirement(RoleConstants.Manager)))
	.AddPolicy("AssociateOrHigher", policy =>
		policy.Requirements.Add(new HierarchicalRoleRequirement(RoleConstants.Associate)));

// Register handler
builder.Services.AddScoped<IAuthorizationHandler, HierarchicalRoleAuthorizationHandler>();
```

## Removed Files

- ❌ `Services/IAuthorizationService.cs` - Use ASP.NET Core's built-in interface
- ❌ `Services/AuthorizationService.cs` - Use built-in `IAuthorizationService` with policies

## Authorization Policies Available

| Policy | Description | Use Case |
|--------|-------------|----------|
| `AdminOnly` | Only Admin role | Admin-only operations |
| `ManagerOnly` | Only Manager role | Manager-only operations |
| `AssociateOnly` | Only Associate role | Associate-only operations |
| `AdminOrHigher` | Admin level or above | Sensitive operations |
| `ManagerOrHigher` | Manager level or above (Admin + Manager) | Resource management |
| `AssociateOrHigher` | Any authenticated user (all roles) | Standard operations |

## Usage Examples

### Decorator-Based (Recommended)
```csharp
[Authorize(Policy = "ManagerOrHigher")]
public async Task<IActionResult> CreateEmployee(EmployeeDto dto)
{
	// Only Managers and Admins can access
	return Ok();
}

[Authorize(Policy = "AdminOnly")]
public async Task<IActionResult> DeleteEmployee(int id)
{
	// Only Admins can access
	return Ok();
}

[Authorize]
public async Task<IActionResult> GetEmployee(int id)
{
	// Any authenticated user (all roles)
	return Ok();
}
```

### Imperative (Within Methods)
```csharp
public async Task<IActionResult> UpdateEmployee(int id, EmployeeDto dto)
{
	var result = await _authorizationService.AuthorizeAsync(
		User, "ManagerOrHigher");

	if (!result.Succeeded)
		return Forbid();

	// Perform update
	return Ok();
}
```

## How It Works

### Built-in Flow
1. Request arrives with JWT Bearer token
2. `[Authorize]` attribute intercepts
3. `AuthorizationMiddleware` validates policy
4. For simple roles: `policy.RequireRole()` checks token claims
5. For hierarchical: Custom `HierarchicalRoleAuthorizationHandler` runs
   - Extracts user ID from claims
   - Queries database for user's roles
   - Validates against role hierarchy
   - Calls `context.Succeed()` if authorized

### Response Codes
- **200**: Authorized ✅
- **401 Unauthorized**: No token or invalid token
- **403 Forbidden**: Authenticated but lacks required role

## JWT Token Claims

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

Role claims are automatically added by `SecurityTokenService` when generating tokens.

## Migration Guide

If you have controllers using the old custom service:

### Before (Old Custom Service)
```csharp
private readonly IAuthorizationService _authService;

public async Task<bool> IsAdmin(int userId)
{
	return await _authService.IsUserAdminAsync(userId);
}
```

### After (Built-in Service)
```csharp
private readonly IAuthorizationService _authService;

public async Task<bool> IsAdmin()
{
	var result = await _authService.AuthorizeAsync(User, "AdminOnly");
	return result.Succeeded;
}
```

## Testing

```csharp
[Fact]
public async Task CreateEmployee_WithAdminRole_ReturnsCreated()
{
	// Arrange
	var claims = new List<Claim>
	{
		new Claim(ClaimTypes.NameIdentifier, "1"),
		new Claim(ClaimTypes.Role, "Admin")
	};
	var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

	// Act
	var result = await controller.CreateEmployee(dto);

	// Assert
	Assert.IsType<CreatedResult>(result);
}
```

## Advantages of Built-in Service

✅ **Standard ASP.NET Core pattern** - Use what the framework provides
✅ **Less code** - No custom service layer to maintain
✅ **Better integration** - Works seamlessly with middleware
✅ **Powerful policies** - Combine multiple requirements easily
✅ **Custom handlers** - Extend for complex scenarios
✅ **Testable** - Mock `IAuthorizationService` directly
✅ **Performance** - No reflection, compiled policies

## Resources

- AUTHORIZATION_GUIDE.md - Comprehensive guide with examples
- Program.cs - Configuration example
- Controllers/EmployeeController.cs - Real-world usage example
- Authorization/ - Custom requirements and handlers

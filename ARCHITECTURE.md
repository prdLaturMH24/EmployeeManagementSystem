# Role-Based Authorization Architecture

## System Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        HTTP Request (JWT Token)                 │
└────────────────┬────────────────────────────────────────────────┘
				 │
				 ▼
┌─────────────────────────────────────────────────────────────────┐
│              ASP.NET Core Authentication Middleware              │
│          (Validates JWT & Extracts Claims including Roles)       │
└────────────────┬────────────────────────────────────────────────┘
				 │
				 ▼
┌─────────────────────────────────────────────────────────────────┐
│                    [Authorize] Attribute Found                   │
│            (Checks if Authorization Policy is Specified)         │
└────────────────┬────────────────────────────────────────────────┘
				 │
		 ┌───────┴──────────┐
		 │                  │
		 ▼                  ▼
	Simple Role      Complex Hierarchical
	[Authorize]      [Authorize(Policy="...")]
		 │                  │
		 ▼                  ▼
	┌─────────────┐  ┌──────────────────────────┐
	│RequireRole()│  │HierarchicalRoleRequirement│
	│             │  │         Handler            │
	│ Checks JWT  │  │   1. Extract User ID      │
	│  Claims     │  │   2. Query Database       │
	│ Directly    │  │   3. Apply Hierarchy      │
	└──────┬──────┘  └───────────┬────────────────┘
		   │                      │
		   │                      ▼
		   │          ┌────────────────────────┐
		   │          │  IRoleRepository       │
		   │          │  ├─ GetUserRolesAsync()│
		   │          │  └─ ...                │
		   │          │        ↓               │
		   │          │    Database Query      │
		   │          └────────────┬───────────┘
		   │                       │
		   └───────────┬───────────┘
					   │
					   ▼
		┌──────────────────────────────┐
		│  Authorization Result        │
		├──────────────────────────────┤
		│  ✅ Succeed  →  200 OK       │
		│  ❌ Fail     →  403 Forbid   │
		│  ❌ No Auth  →  401 Unauth   │
		└──────────────────────────────┘
					   │
					   ▼
		┌──────────────────────────────┐
		│  Action Method Executed      │
		│  (if authorized)             │
		└──────────────────────────────┘
```

## Role Hierarchy

```
					┌─────────────────┐
					│   Admin (3)     │
					│ - Full Access   │
					│ - Manage Users  │
					│ - Manage All    │
					└────────┬────────┘
							 │ (inherits from)
							 ▼
					┌─────────────────┐
					│  Manager (2)    │
					│ - Manage Emps   │
					│ - Create/Update │
					│ - Team Access   │
					└────────┬────────┘
							 │ (inherits from)
							 ▼
					┌─────────────────┐
					│ Associate (1)   │
					│ - Basic Access  │
					│ - Read Own Data │
					│ - Limited Edit  │
					└─────────────────┘
```

## Data Flow: Authorization Check

```
User Login Request
	↓
[UserService.LoginUserAsync()]
	↓
Generate JWT Token
	↓
[SecurityTokenService.GenerateSecurityTokenAsync()]
	├─ Inject IRoleRepository
	├─ Query user's roles
	├─ Add role claims to token
	└─ Return token to client
		 ↓
	Client stores token
		 ↓
	Client includes token in Authorization header
		 ↓
API Request with token
	↓
[AuthenticationMiddleware]
	├─ Extract JWT
	├─ Validate signature
	├─ Parse claims (including roles)
	└─ Set HttpContext.User
		 ↓
	Check [Authorize(Policy="...")] attribute
		 ↓
	┌────────────────────────────────┐
	│ Simple Role Policy?            │
	│ Resolved by RequireRole()       │
	│ (checks token claims directly) │
	└────────────────────────────────┘
			  OR
	┌────────────────────────────────┐
	│ Hierarchical Policy?           │
	│ Handled by Custom Handler      │
	│ (queries database for levels)  │
	└────────────────────────────────┘
		 ↓
	Return Authorization Result
	(Succeed/Fail/None)
		 ↓
	Middleware decision:
	200 OK → Execute action
	403 Forbid → Return error
	401 Unauth → Return error
```

## File Structure

```
EmployeeManagementSystem/
├── Authorization/
│   ├── Handlers/
│   │   └── HierarchicalRoleAuthorizationHandler.cs
│   │       └─ Implements AuthorizationHandler<HierarchicalRoleRequirement>
│   │       └─ Queries database for role hierarchy
│   └── Requirements/
│       └── HierarchicalRoleRequirement.cs
│           └─ Implements IAuthorizationRequirement
│           └─ Specifies minimum required role
├── Common/
│   ├── Constants/
│   │   └── RoleConstants.cs
│   │       └─ Admin = "Admin"
│   │       └─ Manager = "Manager"
│   │       └─ Associate = "Associate"
│   └── Exceptions/
├── Controllers/
│   ├── EmployeeController.cs
│   │   └─ [Authorize(Policy = "...")]
│   ├── UserController.cs
│   │   └─ Public (no auth)
│   └── ...
├── Models/
│   ├── AppUser.cs
│   ├── Role.cs
│   ├── UserRole.cs (many-to-many join)
│   └── ...
├── Repository/
│   ├── IRoleRepository.cs
│   ├── RoleRepository.cs ← Data access for roles
│   ├── IUserRepository.cs
│   ├── UserRepository.cs ← Includes eager-loading of roles
│   └── ...
├── Services/
│   ├── SecurityTokenService.cs ← Generates JWT with role claims
│   ├── UserService.cs
│   ├── EmployeeService.cs
│   └── ...
├── Program.cs ← Configures policies
├── Data/
│   └── EmployeeDbContext.cs
```

## Configuration (Program.cs)

```csharp
// 1. Register services
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<UserService>();

// 2. Configure authorization policies
builder.Services.AddAuthorizationBuilder()
	// Simple role checks
	.AddPolicy("AdminOnly", p => p.RequireRole("Admin"))
	.AddPolicy("ManagerOnly", p => p.RequireRole("Manager"))
	.AddPolicy("AssociateOnly", p => p.RequireRole("Associate"))

	// Hierarchical checks
	.AddPolicy("AdminOrHigher", p =>
		p.Requirements.Add(new HierarchicalRoleRequirement("Admin")))
	.AddPolicy("ManagerOrHigher", p =>
		p.Requirements.Add(new HierarchicalRoleRequirement("Manager")))
	.AddPolicy("AssociateOrHigher", p =>
		p.Requirements.Add(new HierarchicalRoleRequirement("Associate")));

// 3. Register custom handler
builder.Services.AddScoped<IAuthorizationHandler, 
	HierarchicalRoleAuthorizationHandler>();

// 4. Configure authentication (JWT)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options => { ... });
```

## Usage Pattern

```csharp
[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
	// Public endpoint - no auth required
	[HttpGet("count")]
	public async Task<IActionResult> GetCount()
	{
		return Ok(count);
	}

	// Any authenticated user
	[Authorize]
	[HttpGet("{id}")]
	public async Task<IActionResult> GetEmployee(int id)
	{
		return Ok(employee);
	}

	// All authenticated users with hierarchical check
	[Authorize(Policy = "AssociateOrHigher")]
	[HttpGet]
	public async Task<IActionResult> List()
	{
		return Ok(employees);
	}

	// Manager level or higher (Admin + Manager)
	[Authorize(Policy = "ManagerOrHigher")]
	[HttpPost]
	public async Task<IActionResult> Create(CreateDto dto)
	{
		return Created(...);
	}

	// Admin only
	[Authorize(Policy = "AdminOnly")]
	[HttpDelete("{id}")]
	public async Task<IActionResult> Delete(int id)
	{
		return NoContent();
	}
}
```

## Database Schema

```sql
-- Roles table
CREATE TABLE [Employee].[Role] (
	[Id] INT PRIMARY KEY IDENTITY,
	[Name] NVARCHAR(50) NOT NULL UNIQUE
);

-- Users table
CREATE TABLE [Employee].[AppUser] (
	[Id] INT PRIMARY KEY IDENTITY,
	[UserName] NVARCHAR(100) NOT NULL UNIQUE,
	[Email] NVARCHAR(100) NOT NULL UNIQUE,
	[PasswordHash] VARBINARY(MAX) NOT NULL,
	[PasswordSalt] VARBINARY(MAX) NOT NULL
);

-- Junction table (many-to-many)
CREATE TABLE [Employee].[UserRole] (
	[UserId] INT NOT NULL FOREIGN KEY REFERENCES [AppUser]([Id]) ON DELETE CASCADE,
	[RoleId] INT NOT NULL FOREIGN KEY REFERENCES [Role]([Id]) ON DELETE CASCADE,
	PRIMARY KEY ([UserId], [RoleId])
);

-- Example data
INSERT INTO [Employee].[Role] VALUES ('Admin'), ('Manager'), ('Associate');
INSERT INTO [Employee].[AppUser] VALUES ('admin.user', 'admin@example.com', ..., ...);
INSERT INTO [Employee].[UserRole] SELECT u.Id, r.Id FROM [AppUser] u, [Role] r 
  WHERE u.UserName = 'admin.user' AND r.Name = 'Admin';
```

## Key Design Decisions

1. **ASP.NET Core's Built-in Service**
   - Standard pattern
   - No custom abstraction layer
   - Better performance & integration

2. **Policy-Based Over Attribute-Based**
   - Centralized configuration
   - Consistent across app
   - Easy to modify/test

3. **Custom Handler for Hierarchy**
   - Database lookups for dynamic role levels
   - Enables future role extensions
   - Flexible authorization logic

4. **Eager Loading in Repository**
   - UserRepository includes roles when fetching users
   - SecurityTokenService has access to roles immediately
   - Prevents N+1 query problems

5. **Role Claims in JWT**
   - Security: roles embedded in token
   - Performance: no db hit for simple role checks
   - Flexibility: both token claims and db queries supported

## Error Handling

| Status | Meaning | Fix |
|--------|---------|-----|
| 400 Bad Request | Invalid input | Check request body |
| 401 Unauthorized | No/invalid token | Include valid JWT in Authorization header |
| 403 Forbidden | Lacks required role | User is authenticated but doesn't have the role |
| 404 Not Found | Resource doesn't exist | Check resource ID |
| 500 Server Error | Unhandled exception | Check logs & debug |

## Performance Considerations

✅ **Fast Path (JWT Claims)**
- Simple roles checked against token claims
- No database hit
- Microseconds

⚠️ **Slow Path (Database Query)**
- Hierarchical roles require db lookup
- Queries cached per request
- Milliseconds (acceptable)

## Security Checklist

- ✅ JWT signed with secure key
- ✅ Role claims included in token
- ✅ Roles validated on every request
- ✅ Password hashing with salt
- ✅ Authorization middleware active
- ✅ HTTPS required in production
- ✅ CORS configured restrictively
- ✅ No sensitive data in claims

# Configuration Reference - Complete Setup

## Program.cs Configuration

### Namespace Imports
```csharp
using EmployeeManagementSystem.Common;
using EmployeeManagementSystem.Authorization.Handlers;
using EmployeeManagementSystem.Authorization.Requirements;
using EmployeeManagementSystem.Common.Constants;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Repository;
using EmployeeManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
```

### Service Registration (Dependency Injection)
```csharp
// Database Configuration
var connectionString = builder.Configuration.GetConnectionString("EmployeeDatabaseConnection");
builder.Services.AddDbContext<EmployeeDbContext>(options =>
{
	options.UseSqlServer(connectionString);
	options.EnableSensitiveDataLogging();
	options.LogTo(Console.WriteLine);
});

// Caching & Token Service
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<SecurityTokenService>();

// Repository Services (Data Access)
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

// Business Logic Services
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<UserService>();

// Controllers
builder.Services.AddControllers();
```

### Authorization Configuration
```csharp
// Configure authorization policies using AddAuthorizationBuilder
builder.Services.AddAuthorizationBuilder()

	// 1. SIMPLE ROLE POLICIES (Exact Role Match)
	// =========================================
	// Check JWT claims for exact role
	.AddPolicy("AdminOnly", policy => 
		policy.RequireRole(RoleConstants.Admin))

	.AddPolicy("ManagerOnly", policy => 
		policy.RequireRole(RoleConstants.Manager))

	.AddPolicy("AssociateOnly", policy => 
		policy.RequireRole(RoleConstants.Associate))


	// 2. HIERARCHICAL POLICIES (Role Level or Higher)
	// ===============================================
	// Query database for role hierarchy

	.AddPolicy("AdminOrHigher", policy =>
		policy.Requirements.Add(
			new HierarchicalRoleRequirement(RoleConstants.Admin)))

	.AddPolicy("ManagerOrHigher", policy =>
		policy.Requirements.Add(
			new HierarchicalRoleRequirement(RoleConstants.Manager)))

	.AddPolicy("AssociateOrHigher", policy =>
		policy.Requirements.Add(
			new HierarchicalRoleRequirement(RoleConstants.Associate)));

// Register the custom hierarchical role authorization handler
builder.Services.AddScoped<IAuthorizationHandler, 
	HierarchicalRoleAuthorizationHandler>();
```

### Authentication Configuration (JWT)
```csharp
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = false,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = jwtSettingsSection["Issuer"],
			IssuerSigningKey = new SymmetricSecurityKey(
				System.Text.Encoding.UTF8.GetBytes(
					jwtSettingsSection["SuperSecretKey"] ?? 
					throw new ArgumentNullException("SuperSecretKey configuration is missing."))),
			ClockSkew = TimeSpan.Zero
		};
	});
```

### Middleware Pipeline (app.UseXxx() Order)
```csharp
// 1. Development-only: API documentation
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.UseSwagger();
	app.UseSwaggerUI();
	app.UseDeveloperExceptionPage();
}

// 2. HSTS - HTTPS Strict Transport Security (production only)
if (!app.Environment.IsDevelopment())
{
	app.UseHsts();
}

// 3. Global exception handler
app.UseExceptionHandler(GlobalExceptionHandler.HandleException);

// 4. Redirect HTTP to HTTPS
app.UseHttpsRedirection();

// 5. Routing - match URL to endpoints
app.UseRouting();

// 6. CORS - Cross-Origin Resource Sharing
app.UseCors("AllowAll");

// 7. Authentication - identify the user
app.UseAuthentication();

// 8. Authorization - validate roles/claims
app.UseAuthorization();

// 9. Map endpoints
app.UseEndpoints(endpoints => endpoints.MapControllers());

app.Run();
```

## appsettings.json Configuration

```json
{
  "ConnectionStrings": {
	"EmployeeDatabaseConnection": "Server=.;Database=EmployeeManagementDb;Trusted_Connection=true;Encrypt=false;"
  },

  "JwtSettings": {
	"SuperSecretKey": "your-secret-key-min-32-chars-long!@#",
	"Issuer": "EmployeeManagementSystem",
	"Audience": "EmployeeManagementSystemUsers"
  },

  "Logging": {
	"LogLevel": {
	  "Default": "Information",
	  "Microsoft.EntityFrameworkCore": "Warning"
	}
  }
}
```

## appSettings.Development.json Override

```json
{
  "Logging": {
	"LogLevel": {
	  "Default": "Debug",
	  "Microsoft.EntityFrameworkCore": "Information"
	}
  }
}
```

## CORS Policy Configuration

```csharp
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
	{
		policy.AllowAnyOrigin();
		policy.AllowAnyMethod();
		policy.AllowAnyHeader();
	});
});

// Later in middleware:
app.UseCors("AllowAll");
```

For Production, replace with restrictive policy:
```csharp
options.AddPolicy("AllowSpecific", policy =>
{
	policy.WithOrigins("https://yourdomain.com")
		  .AllowAnyMethod()
		  .AllowAnyHeader()
		  .AllowCredentials();
});
```

## Swagger/OpenAPI Configuration

```csharp
builder.Services.AddSwaggerGen(option =>
{
	option.SwaggerDoc("v1", new OpenApiInfo
	{
		Version = "v1",
		Title = "Employee Management System API",
		Description = "An ASP.NET Core Web API for managing employees.",
		Contact = new OpenApiContact
		{
			Name = "Your Name",
			Email = "your.email@example.com"
		}
	});

	option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		In = ParameterLocation.Header,
		Description = "Please enter a valid JWT bearer token",
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		BearerFormat = "JWT",
		Scheme = "Bearer"
	});
});

builder.Services.AddOpenApi();
```

## Entity Framework DbContext Configuration

```csharp
public class EmployeeDbContext : DbContext
{
	public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options) 
		: base(options) { }

	public DbSet<Employee> Employees { get; set; }
	public DbSet<EmployeeDetail> EmployeeDetails { get; set; }
	public DbSet<AppUser> AppUsers { get; set; }
	public DbSet<Role> Roles { get; set; }
	public DbSet<UserRole> UserRoles { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// User-Role Junction Table Configuration
		modelBuilder.Entity<UserRole>()
			.ToTable("UserRole", "Employee")
			.HasKey(ur => new { ur.UserId, ur.RoleId });

		modelBuilder.Entity<UserRole>()
			.HasOne(ur => ur.AppUser)
			.WithMany(u => u.UserRoles)
			.HasForeignKey(ur => ur.UserId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<UserRole>()
			.HasOne(ur => ur.Role)
			.WithMany(r => r.UserRoles)
			.HasForeignKey(ur => ur.RoleId)
			.OnDelete(DeleteBehavior.Cascade);

		// Role Table Configuration
		modelBuilder.Entity<Role>()
			.ToTable("Role", "Employee")
			.HasKey(r => r.Id);

		// AppUser Table Configuration
		modelBuilder.Entity<AppUser>()
			.ToTable("AppUser", "Employee")
			.HasKey(u => u.Id);
	}
}
```

## Testing JWT Token Configuration

For local testing with Swagger:

1. Call `/api/user/login` with credentials
2. Copy the returned `token` value
3. Click "Authorize" button in Swagger UI
4. Enter: `Bearer {token}`
5. All authorized endpoints now include the token

Example request:
```bash
curl -X POST "https://localhost:7000/api/user/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"admin.user","password":"password123"}'
```

Response:
```json
{
  "userName": "admin.user",
  "token": "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2024-01-15T10:30:00Z"
}
```

## Checklist for Implementation

- [ ] DbContext includes AppUser, Role, UserRole entities
- [ ] Program.cs imports authorization namespaces
- [ ] AddAuthorizationBuilder() configured with 6 policies
- [ ] HierarchicalRoleAuthorizationHandler registered
- [ ] Authentication middleware added (JWT Bearer)
- [ ] [Authorize] attributes applied to controllers/actions
- [ ] SecurityTokenService injects IRoleRepository
- [ ] appsettings.json has JwtSettings section
- [ ] Database schema includes User-Role junction table
- [ ] Roles created in database (Admin, Manager, Associate)
- [ ] Test users assigned with roles
- [ ] CORS policy appropriate for environment
- [ ] HTTPS configured for production
- [ ] JWT secret key secure and not in source control

## Troubleshooting

### 401 Unauthorized
```
→ Missing or invalid JWT token
→ Token has expired
→ Token signature is invalid
Solution: Get new token via login endpoint
```

### 403 Forbidden
```
→ User authenticated but lacks required role
→ User role not assigned in database
→ Role claim not included in JWT
Solution: Verify user role in database, regenerate token
```

### Policy Not Working
```
→ Policy name mismatch (case-sensitive)
→ [Authorize] attribute missing
→ Handler not registered
→ Requirement not added to policy
Solution: Check policy configuration in Program.cs
```

### Database Role Queries Not Working
```
→ IRoleRepository not injected in handler
→ User ID not extractable from claims
→ Roles not eager-loaded in repository
Solution: Verify repository injection and claim structure
```

## Production Checklist

- [ ] Change JWT secret key to secure, random value
- [ ] Configure HTTPS certificates
- [ ] Set RestrictedCORS policy instead of AllowAll
- [ ] Enable HSTS headers
- [ ] Use secure database connection (Encrypt=true)
- [ ] Implement rate limiting
- [ ] Add request logging
- [ ] Configure monitoring & alerts
- [ ] Implement audit trails for role changes
- [ ] Regular security audits
- [ ] Keep dependencies updated

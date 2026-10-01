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

namespace EmployeeManagementSystem
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });
            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("EmployeeDatabaseConnection")
                ?? throw new InvalidOperationException("Connection string" + "'EmployeeDatabaseConnection' not found.");

            builder.Services.AddDbContext<EmployeeDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
                options.EnableSensitiveDataLogging();
                options.LogTo(Console.WriteLine);
            });

            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<SecurityTokenService>();
            builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            builder.Services.AddScoped<EmployeeService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddControllers();

            // Configure authorization policies
            builder.Services.AddAuthorizationBuilder()
                // Simple role-based policies using RequireRole
                .AddPolicy("AdminOnly", policy => policy.RequireRole(RoleConstants.Admin))
                .AddPolicy("ManagerOnly", policy => policy.RequireRole(RoleConstants.Manager))
                .AddPolicy("AssociateOnly", policy => policy.RequireRole(RoleConstants.Associate))

                // Hierarchical policies - Admin or higher
                .AddPolicy("AdminOrHigher", policy =>
                    policy.Requirements.Add(new HierarchicalRoleRequirement(RoleConstants.Admin)))

                // Hierarchical policies - Manager or higher (includes Admin and Manager)
                .AddPolicy("ManagerOrHigher", policy =>
                    policy.Requirements.Add(new HierarchicalRoleRequirement(RoleConstants.Manager)))

                // Hierarchical policies - Associate or higher (includes all roles)
                .AddPolicy("AssociateOrHigher", policy =>
                    policy.Requirements.Add(new HierarchicalRoleRequirement(RoleConstants.Associate)));

            // Register the hierarchical role authorization handler
            builder.Services.AddScoped<IAuthorizationHandler, HierarchicalRoleAuthorizationHandler>();

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
                        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettingsSection["SuperSecretKey"] ?? throw new ArgumentNullException("SuperSecretKey configuration is missing."))),
                        ClockSkew = TimeSpan.Zero // Optional: Set clock skew to zero for immediate expiration
                    };
                });

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
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
                option.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter word Bearer followed by a space and a valid JWT bearer token in the value field.",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = JwtBearerDefaults.AuthenticationScheme
                });
                option.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document)] = []
                });
            });
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin();
                    policy.AllowAnyMethod();
                    policy.AllowAnyHeader();
                });
            });
            var app = builder.Build();
           
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Employee Management System API v1");
                    options.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
                });
                //Developer exception page middleware (UseDeveloperExceptionPage) reports app runtime errors.
                app.UseDeveloperExceptionPage();
            }
            //Enforces secure connections
            // Enable HSTS ONLY in production environments
            if (!app.Environment.IsDevelopment())
            {
                // Strict Transport Security (HSTS) 
                app.UseHsts();
            }

            app.UseExceptionHandler(GlobalExceptionHandler.HandleException);
            //Redirects HTTP requests to HTTPS
            app.UseHttpsRedirection();

            //Matches the request URL to an endpoint matching pattern
            app.UseRouting();

            // Applies cross-origin browser resource rules
            app.UseCors();

            //Identifies the user
            app.UseAuthentication();

            //Validates roles/claims
            app.UseAuthorization();

            //Runs Controllers, Minimal APIs, or Razor Pages
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //app.Run(async context =>
            //{
            //    context.Response.Headers.Append("Message", "Thank you!");
            //});
            app.Run();
        }
    }
}

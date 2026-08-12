using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

namespace EmployeeManagementSystem
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();
            var connectionString = builder.Configuration.GetConnectionString("EmployeeDatabaseConnection")
                ?? throw new InvalidOperationException("Connection string" + "'EmployeeDatabaseConnection' not found.");

            builder.Services.AddDbContext<EmployeeDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
                options.EnableSensitiveDataLogging();
                options.LogTo(Console.WriteLine);
            });

            builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
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
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            var summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
                };

            app.MapGet("/weatherforecast", (HttpContext httpContext) =>
            {
                var forecast = Enumerable.Range(1, 5).Select(index =>
                    new WeatherForecast
                    {
                        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        TemperatureC = Random.Shared.Next(-20, 55),
                        Summary = summaries[Random.Shared.Next(summaries.Length)]
                    })
                    .ToArray();
                return forecast;
            })
            .WithName("GetWeatherForecast");

            app.MapGet("/api/employees", async (IEmployeeRepository employeeRepository) =>
            {
                var employees = await employeeRepository.GetAllEmployeesAsync();
                return Results.Ok(employees);
            }).WithName("GetAllEmployees");

            app.MapGet("/api/employees/details", async (IEmployeeRepository employeeRepository) =>
            {
                var employees = await employeeRepository.GetAllEmployeesWithDetailsAsync();
                var employeeDetailsDtos = employees.Select(e => new Models.DTOs.EmployeeDetails
                {
                    EmployeeId = e.AssociateId,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Email = e.EmployeeDetail?.Email ?? string.Empty,
                    PhoneNumber = e.EmployeeDetail?.PhoneNumber ?? string.Empty,
                    Address = e.EmployeeDetail?.Address ?? string.Empty,
                    DateOfBirth = e.EmployeeDetail?.DateOfBirth ?? default(DateOnly),
                    Position = e.EmployeeDetail?.Position ?? string.Empty,
                    Salary = e.EmployeeDetail?.Salary ?? 0
                }).ToList();
                return Results.Ok(employeeDetailsDtos);
            }).WithName("GetAllEmployeesWithDetails");

            app.MapGet("/api/employees/by-position", async (IEmployeeRepository employeeRepository) =>
            {
                var employeesByPosition = await employeeRepository.GetAllEmployeesByPositionAsync();
                var employeesByPositionDtos = employeesByPosition.Select(kvp => new Models.DTOs.EmployeesByPosition
                {
                    Position = kvp.Key,
                    Employees = kvp.Value.Select(e => new Models.DTOs.EmployeeDetails
                    {
                        EmployeeId = e.AssociateId,
                        FirstName = e.FirstName,
                        LastName = e.LastName,
                        Email = e.EmployeeDetail?.Email ?? string.Empty,
                        PhoneNumber = e.EmployeeDetail?.PhoneNumber ?? string.Empty,
                        Address = e.EmployeeDetail?.Address ?? string.Empty,
                        DateOfBirth = e.EmployeeDetail?.DateOfBirth ?? default(DateOnly),
                        Position = e.EmployeeDetail?.Position ?? string.Empty,
                        Salary = e.EmployeeDetail?.Salary ?? 0
                    }).ToList()
                }).ToList();
                return Results.Ok(employeesByPositionDtos);
            }).WithName("GetAllEmployeesByPosition");

            app.Run();
        }
    }
}

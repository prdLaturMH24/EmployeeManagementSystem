using EmployeeManagementSystem.Common;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Repository;
using EmployeeManagementSystem.Services;
using Microsoft.EntityFrameworkCore;
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

            builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            builder.Services.AddScoped<EmployeeService>();
            builder.Services.AddControllers();
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

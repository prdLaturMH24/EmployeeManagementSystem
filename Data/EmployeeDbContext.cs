using EmployeeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Data
{
    public class EmployeeDbContext : DbContext
    {
        public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options) : base(options)
        {
            
        }

        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                .ToTable("Employee", "Employee")
                .HasKey(o => o.Id);
        }

        //Select * FROM [DatabaseName].[SchemaName].[TableName]
        //Select * FROM [EmployeeDatabase].[Employee].[Employee]
    }
}

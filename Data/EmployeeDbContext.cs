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
        public DbSet<EmployeeDetail> EmployeeDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Employee Table and Primary Key Configuration
            modelBuilder.Entity<Employee>()
                .ToTable("Employee", "Employee")
                .HasKey(o => o.Id);

            //EmployeeDetail Table and Primary Key Configuration
            modelBuilder.Entity<EmployeeDetail>()
                .ToTable("EmployeeDetail", "Employee")
                .HasKey(o => o.EmployeeDetailId);

            modelBuilder.Entity<EmployeeDetail>()
                .Property(o => o.Salary)
                .HasColumnType("decimal(18, 2)")
                .HasPrecision(18, 2);

            //One to One Relationship between Employee and EmployeeDetail
            //modelBuilder.Entity<Employee>()
            //    .HasOne(e => e.EmployeeDetail)
            //    .WithOne(ed => ed.Employee)
            //    .HasForeignKey<EmployeeDetail>(e => e.EmployeeId)
            //    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeDetail>()
                .HasOne(ed => ed.Employee)
                .WithOne(e => e.EmployeeDetail)
                .HasForeignKey<EmployeeDetail>(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

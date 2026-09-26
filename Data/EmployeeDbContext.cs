using EmployeeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Data
{
    public class EmployeeDbContext : DbContext
    {
        private const string SchemaName = "Employee";
        public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options) : base(options)
        {
            
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeDetail> EmployeeDetails { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Employee Table and Primary Key Configuration
            modelBuilder.Entity<Employee>()
                .ToTable("Employee", SchemaName)
                .HasKey(o => o.Id);

            //EmployeeDetail Table and Primary Key Configuration
            modelBuilder.Entity<EmployeeDetail>()
                .ToTable("EmployeeDetail", SchemaName)
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

            modelBuilder.Entity<AppUser>()
                .ToTable("AppUser", SchemaName)
                .HasKey(o => o.Id);

            modelBuilder.Entity<Role>()
                .ToTable("Role", SchemaName)
                .HasKey(o => o.Id);

            modelBuilder.Entity<UserRole>()
                .ToTable("UserRole", SchemaName)
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
        }
    }
}

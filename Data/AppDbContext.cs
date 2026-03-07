using EmployesManagementSystemApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployesManagementSystemApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Employee -> Depertment relationship
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Configure decimal precision for Salary
            modelBuilder.Entity<Employee>()
                .Property(e => e.Salary)
                .HasColumnType("numeric(18,2)");
        }
    }
}

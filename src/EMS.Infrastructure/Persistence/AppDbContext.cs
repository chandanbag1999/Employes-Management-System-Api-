using EMS.Domain.Entities.Attendance;
using EMS.Domain.Entities.Employee;
using EMS.Domain.Entities.Identity;
using EMS.Domain.Entities.Leave;
using EMS.Domain.Entities.Organization;
using EMS.Domain.Entities.Payroll;
using EMS.Domain.Entities.Performance;
using Microsoft.EntityFrameworkCore;


namespace EMS.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Identity
    public DbSet<AppUser> Users => Set<AppUser>();

    // Organization
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Designation> Designations => Set<Designation>();

    // Employee
    public DbSet<EmployeeProfile> Employees => Set<EmployeeProfile>();

    // Attendance
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();

    // Leave
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveApplication> LeaveApplications => Set<LeaveApplication>();

    // Payroll
    public DbSet<SalaryStructure> SalaryStructures => Set<SalaryStructure>();
    public DbSet<PayrollRecord> PayrollRecords => Set<PayrollRecord>();


    // Performance
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<PerformanceReview> PerformanceReviews => Set<PerformanceReview>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Saari configurations ek saath apply hogi
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
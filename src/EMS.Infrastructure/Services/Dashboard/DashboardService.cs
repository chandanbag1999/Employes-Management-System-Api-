using EMS.Application.Modules.Dashboard.DTOs;
using EMS.Application.Modules.Dashboard.Interfaces;
using EMS.Domain.Enums;
using EMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EMS.Infrastructure.Services.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var currentMonth = DateTime.UtcNow.Month;
        var currentYear = DateTime.UtcNow.Year;

        var totalEmployees = await _context.Employees.CountAsync();
        var activeEmployees = await _context.Employees
            .CountAsync(e => e.Status == EmploymentStatus.Active);
        var newJoinees = await _context.Employees
            .CountAsync(e =>
                e.JoiningDate.Month == currentMonth &&
                e.JoiningDate.Year == currentYear);
        var onProbation = await _context.Employees
            .CountAsync(e => e.Status == EmploymentStatus.OnProbation);

        var presentToday = await _context.AttendanceRecords
            .CountAsync(a =>
                a.Date == today &&
                (a.Status == AttendanceStatus.Present ||
                 a.Status == AttendanceStatus.HalfDay));
        var onLeaveToday = await _context.AttendanceRecords
            .CountAsync(a =>
                a.Date == today &&
                a.Status == AttendanceStatus.OnLeave);
        var absentToday = Math.Max(0,
            activeEmployees - presentToday - onLeaveToday);

        var pendingLeaves = await _context.LeaveApplications
            .CountAsync(l => l.Status == LeaveStatus.Pending);
        var approvedLeavesThisMonth = await _context.LeaveApplications
            .CountAsync(l =>
                l.Status == LeaveStatus.Approved &&
                l.FromDate.Month == currentMonth &&
                l.FromDate.Year == currentYear);

        var payrollThisMonth = await _context.PayrollRecords
            .Where(p => p.Month == currentMonth && p.Year == currentYear)
            .SumAsync(p => (decimal?)p.NetSalary) ?? 0;
        var payrollCount = await _context.PayrollRecords
            .CountAsync(p =>
                p.Month == currentMonth && p.Year == currentYear);

        var totalDepts = await _context.Departments.CountAsync();
        var totalDesignations = await _context.Designations.CountAsync();

        var pendingReviews = await _context.PerformanceReviews
            .CountAsync(r => r.Status == "Draft");
        var goalsCompleted = await _context.Goals
            .CountAsync(g =>
                g.Status == "Completed" &&
                g.ReviewCycle.StartsWith(currentYear.ToString()));

        return new DashboardStatsDto
        {
            TotalEmployees = totalEmployees,
            ActiveEmployees = activeEmployees,
            NewJoineesThisMonth = newJoinees,
            OnProbation = onProbation,
            PresentToday = presentToday,
            AbsentToday = absentToday,
            OnLeaveToday = onLeaveToday,
            PendingLeaveRequests = pendingLeaves,
            ApprovedLeavesThisMonth = approvedLeavesThisMonth,
            TotalPayrollThisMonth = payrollThisMonth,
            PayrollProcessedCount = payrollCount,
            TotalDepartments = totalDepts,
            TotalDesignations = totalDesignations,
            PendingReviews = pendingReviews,
            GoalsCompletedThisQuarter = goalsCompleted
        };
    }

    public async Task<IEnumerable<DepartmentHeadcountDto>>
        GetDepartmentHeadcountAsync()
    {
        return await _context.Departments
            .Include(d => d.Employees)
            .Select(d => new DepartmentHeadcountDto
            {
                DepartmentId = d.Id,
                DepartmentName = d.Name,
                DepartmentCode = d.Code,
                EmployeeCount = d.Employees.Count,
                ActiveCount = d.Employees
                    .Count(e => e.Status == EmploymentStatus.Active)
            })
            .OrderByDescending(d => d.EmployeeCount)
            .ToListAsync();
    }

    public async Task<IEnumerable<RecentActivityDto>> GetRecentActivitiesAsync(
        int count = 10)
    {
        var activities = new List<RecentActivityDto>();

        var newEmployees = await _context.Employees
            .OrderByDescending(e => e.CreatedAt)
            .Take(3)
            .Select(e => new RecentActivityDto
            {
                Type = "NewEmployee",
                Title = "New Employee Joined",
                Description =
                    $"{e.FirstName} {e.LastName} joined as {e.EmployeeCode}",
                Timestamp = e.CreatedAt,
                EmployeeName = $"{e.FirstName} {e.LastName}"
            })
            .ToListAsync();

        var recentLeaves = await _context.LeaveApplications
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .OrderByDescending(l => l.CreatedAt)
            .Take(3)
            .Select(l => new RecentActivityDto
            {
                Type = "Leave",
                Title = "Leave Application",
                Description =
                    $"{l.Employee!.FirstName} applied for " +
                    $"{l.LeaveType.Name} ({l.TotalDays} days) — {l.Status}",
                Timestamp = l.CreatedAt,
                EmployeeName =
                    $"{l.Employee.FirstName} {l.Employee.LastName}"
            })
            .ToListAsync();

        var recentPayroll = await _context.PayrollRecords
            .Include(p => p.Employee)
            .OrderByDescending(p => p.CreatedAt)
            .Take(2)
            .Select(p => new RecentActivityDto
            {
                Type = "Payroll",
                Title = "Payroll Processed",
                Description =
                    $"Payroll for {p.Employee!.FirstName} — " +
                    $"{new DateTime(p.Year, p.Month, 1):MMMM yyyy} " +
                    $"(₹{p.NetSalary:N0})",
                Timestamp = p.CreatedAt,
                EmployeeName =
                    $"{p.Employee.FirstName} {p.Employee.LastName}"
            })
            .ToListAsync();

        activities.AddRange(newEmployees);
        activities.AddRange(recentLeaves);
        activities.AddRange(recentPayroll);

        return activities
            .OrderByDescending(a => a.Timestamp)
            .Take(count);
    }
}
using EMS.Application.Modules.Reports.DTOs;
using EMS.Application.Modules.Reports.Interfaces;
using EMS.Domain.Enums;
using EMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EMS.Infrastructure.Services.Reports;

public class ReportService : IReportService
{
    private readonly AppDbContext _context;

    public ReportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AttendanceReportDto>> GetAttendanceReportAsync(
        int month, int year, int? departmentId)
    {
        var query = _context.Employees
            .Include(e => e.Department)
            .Where(e => e.Status == EmploymentStatus.Active);

        if (departmentId.HasValue)
            query = query.Where(e => e.DepartmentId == departmentId);

        var employees = await query.ToListAsync();
        var result = new List<AttendanceReportDto>();
        var totalWorkingDays = GetWorkingDays(month, year);

        foreach (var emp in employees)
        {
            var records = await _context.AttendanceRecords
                .Where(a =>
                    a.EmployeeId == emp.Id &&
                    a.Date.Month == month &&
                    a.Date.Year == year)
                .ToListAsync();

            result.Add(new AttendanceReportDto
            {
                EmployeeCode = emp.EmployeeCode,
                EmployeeName = $"{emp.FirstName} {emp.LastName}",
                DepartmentName = emp.Department?.Name ?? "",
                Month = month,
                Year = year,
                TotalWorkingDays = totalWorkingDays,
                PresentDays = records.Count(r =>
                    r.Status == AttendanceStatus.Present),
                AbsentDays = records.Count(r =>
                    r.Status == AttendanceStatus.Absent),
                HalfDays = records.Count(r =>
                    r.Status == AttendanceStatus.HalfDay),
                LeaveDays = records.Count(r =>
                    r.Status == AttendanceStatus.OnLeave),
                TotalHours = records
                    .Where(r => r.WorkingHours.HasValue)
                    .Sum(r => r.WorkingHours!.Value)
            });
        }

        return result.OrderBy(r => r.DepartmentName)
                     .ThenBy(r => r.EmployeeCode);
    }

    public async Task<IEnumerable<PayrollReportDto>> GetPayrollReportAsync(
        int month, int year, int? departmentId)
    {
        var query = _context.PayrollRecords
            .Include(p => p.Employee)
                .ThenInclude(e => e!.Department)
            .Include(p => p.Employee)
                .ThenInclude(e => e!.Designation)
            .Where(p => p.Month == month && p.Year == year);

        if (departmentId.HasValue)
            query = query.Where(p =>
                p.Employee!.DepartmentId == departmentId);

        var records = await query
            .OrderBy(p => p.Employee!.Department!.Name)
            .ThenBy(p => p.Employee!.EmployeeCode)
            .ToListAsync();

        return records.Select(p => new PayrollReportDto
        {
            EmployeeCode = p.Employee?.EmployeeCode ?? "",
            EmployeeName = p.Employee != null
                ? $"{p.Employee.FirstName} {p.Employee.LastName}"
                : "Unknown",
            DepartmentName = p.Employee?.Department?.Name ?? "",
            Designation = p.Employee?.Designation?.Title ?? "",
            Month = p.Month,
            Year = p.Year,
            GrossEarnings = p.GrossEarnings,
            TotalDeductions = p.TotalDeductions,
            NetSalary = p.NetSalary,
            Status = p.Status.ToString()
        });
    }

    public async Task<IEnumerable<HeadcountReportDto>> GetHeadcountReportAsync()
    {
        var departments = await _context.Departments
            .Include(d => d.Employees)
            .ToListAsync();

        return departments.Select(d => new HeadcountReportDto
        {
            DepartmentName = d.Name,
            DepartmentCode = d.Code,
            TotalEmployees = d.Employees.Count,
            Active = d.Employees
                .Count(e => e.Status == EmploymentStatus.Active),
            OnProbation = d.Employees
                .Count(e => e.Status == EmploymentStatus.OnProbation),
            Resigned = d.Employees
                .Count(e => e.Status == EmploymentStatus.Resigned),
            Terminated = d.Employees
                .Count(e => e.Status == EmploymentStatus.Terminated),
            MaleCount = d.Employees
                .Count(e => e.Gender == Gender.Male),
            FemaleCount = d.Employees
                .Count(e => e.Gender == Gender.Female)
        }).OrderBy(d => d.DepartmentName);
    }

    private static int GetWorkingDays(int month, int year)
    {
        var days = DateTime.DaysInMonth(year, month);
        var count = 0;
        for (var d = 1; d <= days; d++)
        {
            var date = new DateTime(year, month, d);
            if (date.DayOfWeek != DayOfWeek.Saturday &&
                date.DayOfWeek != DayOfWeek.Sunday)
                count++;
        }
        return count;
    }
}
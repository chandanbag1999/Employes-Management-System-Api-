using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Attendance.DTOs;
using EMS.Application.Modules.Attendance.Interfaces;
using EMS.Domain.Entities.Attendance;
using EMS.Domain.Enums;
using EMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EMS.Infrastructure.Repositories;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly AppDbContext _context;

    public AttendanceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<AttendanceRecord>> GetAllAsync(
        AttendanceFilterDto filter)
    {
        var query = _context.AttendanceRecords
            .Include(a => a.Employee)
            .ThenInclude(e => e!.Department)
            .AsQueryable();

        if (filter.EmployeeId.HasValue)
            query = query.Where(a => a.EmployeeId == filter.EmployeeId);

        if (filter.DepartmentId.HasValue)
            query = query.Where(
                a => a.Employee!.DepartmentId == filter.DepartmentId);

        if (filter.FromDate.HasValue)
            query = query.Where(a =>
                a.Date >= filter.FromDate.Value.ToDateTime(TimeOnly.MinValue));

        if (filter.ToDate.HasValue)
            query = query.Where(a =>
                a.Date <= filter.ToDate.Value.ToDateTime(TimeOnly.MaxValue));

        if (!string.IsNullOrWhiteSpace(filter.Status) &&
            Enum.TryParse<AttendanceStatus>(filter.Status, true, out var status))
            query = query.Where(a => a.Status == status);

        var total = await query.CountAsync();
        var data = await query
            .OrderByDescending(a => a.Date)
            .ThenBy(a => a.Employee!.EmployeeCode)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PaginatedResult<AttendanceRecord>
        {
            Data = data,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<AttendanceRecord?> GetByIdAsync(int id)
        => await _context.AttendanceRecords
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<AttendanceRecord?> GetTodayRecordAsync(int employeeId)
    {
        // ✅ IST date use karo
        var today = GetIstToday();
        return await _context.AttendanceRecords
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a =>
                a.EmployeeId == employeeId &&
                a.Date == today);
    }

    // ✅ NEW
    public async Task<AttendanceRecord?> GetByEmployeeAndDateAsync(
        int employeeId, DateTime date)
        => await _context.AttendanceRecords
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a =>
                a.EmployeeId == employeeId &&
                a.Date == date.Date);

    public async Task<IEnumerable<AttendanceRecord>> GetMonthlyAsync(
        int employeeId, int month, int year)
        => await _context.AttendanceRecords
            .Include(a => a.Employee)
            .Where(a =>
                a.EmployeeId == employeeId &&
                a.Date.Month == month &&
                a.Date.Year == year)
            .OrderBy(a => a.Date)
            .ToListAsync();

    public async Task<AttendanceRecord> CreateAsync(AttendanceRecord record)
    {
        await _context.AttendanceRecords.AddAsync(record);
        await _context.SaveChangesAsync();
        return (await GetByIdAsync(record.Id))!;
    }

    public async Task<AttendanceRecord?> UpdateAsync(int id, AttendanceRecord record)
    {
        var existing = await _context.AttendanceRecords.FindAsync(id);
        if (existing == null) return null;

        existing.ClockIn = record.ClockIn;
        existing.ClockOut = record.ClockOut;
        existing.WorkingHours = record.WorkingHours;
        existing.Status = record.Status;
        existing.Remarks = record.Remarks;
        existing.UpdatedAt = record.UpdatedAt ?? DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> HasClockedInTodayAsync(int employeeId)
    {
        var today = GetIstToday();
        return await _context.AttendanceRecords
            .AnyAsync(a =>
                a.EmployeeId == employeeId &&
                a.Date == today &&
                a.ClockIn.HasValue);
    }

    public async Task<bool> HasClockedOutTodayAsync(int employeeId)
    {
        var today = GetIstToday();
        return await _context.AttendanceRecords
            .AnyAsync(a =>
                a.EmployeeId == employeeId &&
                a.Date == today &&
                a.ClockOut.HasValue);
    }

    // ✅ NEW — Auto absent marking
    public async Task<int> MarkAbsentForDateAsync(DateTime date)
    {
        var targetDate = date.Date;

        // Sab active employees nikalo
        var allEmployeeIds = await _context.Employees
            .Where(e => !e.IsDeleted)
            .Select(e => e.Id)
            .ToListAsync();

        // Jo already record hai unhe nikalo
        var presentIds = await _context.AttendanceRecords
            .Where(a => a.Date == targetDate)
            .Select(a => a.EmployeeId)
            .ToListAsync();

        // Jo absent hain
        var absentIds = allEmployeeIds.Except(presentIds).ToList();

        if (!absentIds.Any()) return 0;

        // Bulk insert — Absent records banao
        var absentRecords = absentIds.Select(empId => new AttendanceRecord
        {
            EmployeeId = empId,
            Date = targetDate,
            Status = AttendanceStatus.Absent,
            Remarks = "Auto-marked absent"
        }).ToList();

        await _context.AttendanceRecords.AddRangeAsync(absentRecords);
        await _context.SaveChangesAsync();

        return absentIds.Count;
    }

    // ✅ IST date helper
    private static DateTime GetIstToday()
    {
        var istZone = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows()
                ? "India Standard Time"
                : "Asia/Kolkata");
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone).Date;
    }
}
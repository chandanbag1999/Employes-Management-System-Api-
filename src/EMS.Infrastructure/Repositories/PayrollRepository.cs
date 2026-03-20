using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Payroll.DTOs;
using EMS.Application.Modules.Payroll.Interfaces;
using EMS.Domain.Entities.Payroll;
using EMS.Domain.Enums;
using EMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EMS.Infrastructure.Repositories;

public class PayrollRepository : IPayrollRepository
{
    private readonly AppDbContext _context;

    public PayrollRepository(AppDbContext context)
    {
        _context = context;
    }

    // ── Salary Structure ──────────────────────────────────────────

    public async Task<SalaryStructure?> GetActiveSalaryStructureAsync(int employeeId)
        => await _context.SalaryStructures
            .Include(s => s.Employee)
            .FirstOrDefaultAsync(s =>
                s.EmployeeId == employeeId && s.IsActive);

    public async Task<SalaryStructure?> GetSalaryStructureByIdAsync(int id)
        => await _context.SalaryStructures
            .Include(s => s.Employee)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<IEnumerable<SalaryStructure>> GetAllSalaryStructuresAsync(
        int? employeeId)
    {
        var query = _context.SalaryStructures
            .Include(s => s.Employee)
            .AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(s => s.EmployeeId == employeeId);

        return await query
            .OrderByDescending(s => s.EffectiveFrom)
            .ToListAsync();
    }

    public async Task<SalaryStructure> CreateSalaryStructureAsync(
        SalaryStructure structure)
    {
        await _context.SalaryStructures.AddAsync(structure);
        await _context.SaveChangesAsync();
        return (await GetSalaryStructureByIdAsync(structure.Id))!;
    }

    public async Task DeactivateAllStructuresAsync(int employeeId)
    {
        var structures = await _context.SalaryStructures
            .Where(s => s.EmployeeId == employeeId && s.IsActive)
            .ToListAsync();

        foreach (var s in structures)
        {
            s.IsActive = false;
            s.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    // ── Payroll Records ───────────────────────────────────────────

    public async Task<PaginatedResult<PayrollRecord>> GetAllAsync(
        PayrollFilterDto filter)
    {
        var query = _context.PayrollRecords
            .Include(p => p.Employee)
                .ThenInclude(e => e!.Department)
            .AsQueryable();

        if (filter.EmployeeId.HasValue)
            query = query.Where(p => p.EmployeeId == filter.EmployeeId);

        if (filter.DepartmentId.HasValue)
            query = query.Where(p =>
                p.Employee!.DepartmentId == filter.DepartmentId);

        if (filter.Month.HasValue)
            query = query.Where(p => p.Month == filter.Month);

        if (filter.Year.HasValue)
            query = query.Where(p => p.Year == filter.Year);

        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(p => p.Status == filter.Status);

        var total = await query.CountAsync();
        var data = await query
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ThenBy(p => p.Employee!.EmployeeCode)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PaginatedResult<PayrollRecord>
        {
            Data = data,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<PayrollRecord?> GetByIdAsync(int id)
        => await _context.PayrollRecords
            .Include(p => p.Employee)
                .ThenInclude(e => e!.Department)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<PayrollRecord?> GetByEmployeeMonthAsync(
        int employeeId, int month, int year)
        => await _context.PayrollRecords
            .Include(p => p.Employee)
                .ThenInclude(e => e!.Department)
            .FirstOrDefaultAsync(p =>
                p.EmployeeId == employeeId &&
                p.Month == month &&
                p.Year == year);

    public async Task<PayrollRecord> CreateAsync(PayrollRecord record)
    {
        await _context.PayrollRecords.AddAsync(record);
        await _context.SaveChangesAsync();
        return (await GetByIdAsync(record.Id))!;
    }

    public async Task<PayrollRecord?> MarkAsPaidAsync(int id)
    {
        var record = await _context.PayrollRecords.FindAsync(id);
        if (record == null) return null;

        record.Status = "Paid";
        record.PaidOn = DateTime.UtcNow;
        record.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> ExistsAsync(int employeeId, int month, int year)
        => await _context.PayrollRecords
            .AnyAsync(p =>
                p.EmployeeId == employeeId &&
                p.Month == month &&
                p.Year == year);

    public async Task<int> GetPresentDaysAsync(int employeeId, int month, int year)
        => await _context.AttendanceRecords
            .CountAsync(a =>
                a.EmployeeId == employeeId &&
                a.Date.Month == month &&
                a.Date.Year == year &&
                (a.Status == AttendanceStatus.Present ||
                 a.Status == AttendanceStatus.HalfDay));

    public async Task<int> GetLeaveDaysAsync(int employeeId, int month, int year)
        => await _context.AttendanceRecords
            .CountAsync(a =>
                a.EmployeeId == employeeId &&
                a.Date.Month == month &&
                a.Date.Year == year &&
                a.Status == AttendanceStatus.OnLeave);

    public async Task<List<int>> GetActiveEmployeeIdsAsync()
        => await _context.Employees
            .Where(e => e.Status == EmploymentStatus.Active)
            .Select(e => e.Id)
            .ToListAsync();
}
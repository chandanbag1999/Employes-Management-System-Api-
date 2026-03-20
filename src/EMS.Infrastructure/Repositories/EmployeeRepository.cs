using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Employees.DTOs;
using EMS.Application.Modules.Employees.Interfaces;
using EMS.Domain.Entities.Employee;
using EMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EMS.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<EmployeeProfile>> GetAllAsync(EmployeeFilterDto filter)
    {
        var query = _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.ReportingManager)
            .AsQueryable();

        // Search — name ya email
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.ToLower();
            query = query.Where(e =>
                e.FirstName.ToLower().Contains(s) ||
                e.LastName.ToLower().Contains(s) ||
                e.Email.ToLower().Contains(s) ||
                e.EmployeeCode.ToLower().Contains(s));
        }

        // Filters
        if (filter.DepartmentId.HasValue)
            query = query.Where(e => e.DepartmentId == filter.DepartmentId);

        if (filter.DesignationId.HasValue)
            query = query.Where(e => e.DesignationId == filter.DesignationId);

        if (filter.Status.HasValue)
            query = query.Where(e => e.Status == filter.Status);

        if (filter.Gender.HasValue)
            query = query.Where(e => e.Gender == filter.Gender);

        var total = await query.CountAsync();

        var data = await query
            .OrderBy(e => e.EmployeeCode)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PaginatedResult<EmployeeProfile>
        {
            Data = data,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<EmployeeProfile?> GetByIdAsync(int id)
        => await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.ReportingManager)
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<EmployeeProfile?> GetByEmailAsync(string email)
        => await _context.Employees
            .FirstOrDefaultAsync(e => e.Email == email.ToLower());

    public async Task<EmployeeProfile> CreateAsync(EmployeeProfile employee)
    {
        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync();

        // Reload with navigations
        return (await GetByIdAsync(employee.Id))!;
    }

    public async Task<EmployeeProfile?> UpdateAsync(int id, EmployeeProfile employee)
    {
        var existing = await _context.Employees.FindAsync(id);
        if (existing == null) return null;

        // Copy updated fields
        _context.Entry(existing).CurrentValues.SetValues(employee);
        existing.Id = id; // ID preserve karo
        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var emp = await _context.Employees.FindAsync(id);
        if (emp == null) return false;

        emp.IsDeleted = true;
        emp.Status = Domain.Enums.EmploymentStatus.Inactive;
        emp.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        => await _context.Employees.AnyAsync(e =>
            e.Email == email.ToLower() &&
            (excludeId == null || e.Id != excludeId));

    public async Task<int> GetTotalCountAsync()
        => await _context.Employees.IgnoreQueryFilters().CountAsync();
}
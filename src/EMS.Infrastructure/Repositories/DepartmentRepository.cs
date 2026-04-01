using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Organization.Interfaces;
using EMS.Domain.Entities.Organization;
using EMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EMS.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _context;

    public DepartmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<Department>> GetAllAsync(
        int page, int pageSize, string? search)
    {
        var query = _context.Departments
            .Include(d => d.Employees)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d =>
                d.Name.ToLower().Contains(search.ToLower()) ||
                (d.Code != null && d.Code.ToLower().Contains(search.ToLower())));

        var total = await query.CountAsync();

        var data = await query
            .OrderBy(d => d.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<Department>
        {
            Data = data,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Department?> GetByIdAsync(int id)
        => await _context.Departments
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == id);

    public async Task<Department> CreateAsync(Department department)
    {
        await _context.Departments.AddAsync(department);
        await _context.SaveChangesAsync();
        return department;
    }

    public async Task<Department?> UpdateAsync(int id, Department department)
    {
        var existing = await _context.Departments.FindAsync(id);
        if (existing == null) return null;

        existing.Name = department.Name;
        existing.Description = department.Description;
        existing.Code = department.Code;
        existing.IsActive = department.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var dept = await _context.Departments.FindAsync(id);
        if (dept == null) return false;

        dept.IsDeleted = true;
        dept.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
        => await _context.Departments.AnyAsync(d => d.Id == id);

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        => await _context.Departments.AnyAsync(d =>
            d.Name.ToLower() == name.ToLower() &&
            (excludeId == null || d.Id != excludeId));

    // ── Soft Delete Management ──────────────────────────────────────
    public async Task<IEnumerable<Department>> GetDeletedAsync()
        => await _context.Departments
            .IgnoreQueryFilters()
            .Include(d => d.Employees)
            .Where(d => d.IsDeleted)
            .OrderByDescending(d => d.UpdatedAt)
            .ToListAsync();

    // Restore — IsDeleted = false karo
    public async Task<bool> RestoreAsync(int id)
    {
        var dept = await _context.Departments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Id == id && d.IsDeleted);

        if (dept == null) return false;

        dept.IsDeleted = false;
        dept.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    // Purge — permanently delete records older than X months
    public async Task<int> PurgeAsync(int months)
    {
        var cutoff = DateTime.UtcNow.AddMonths(-months);

        var oldDeleted = await _context.Departments
            .IgnoreQueryFilters()
            .Where(d => d.IsDeleted && d.UpdatedAt < cutoff)
            .ToListAsync();

        if (oldDeleted.Count == 0) return 0;

        _context.Departments.RemoveRange(oldDeleted);
        await _context.SaveChangesAsync();

        return oldDeleted.Count;
    }
}
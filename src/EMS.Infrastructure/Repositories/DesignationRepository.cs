using EMS.Application.Modules.Organization.Interfaces;
using EMS.Domain.Entities.Organization;
using EMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EMS.Infrastructure.Repositories;

public class DesignationRepository : IDesignationRepository
{
    private readonly AppDbContext _context;

    public DesignationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Designation>> GetAllAsync(int? departmentId)
    {
        var query = _context.Designations
            .Include(d => d.Department)
            .AsQueryable();

        if (departmentId.HasValue)
            query = query.Where(d => d.DepartmentId == departmentId);

        return await query.OrderBy(d => d.Title).ToListAsync();
    }

    public async Task<Designation?> GetByIdAsync(int id)
        => await _context.Designations
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.Id == id);

    public async Task<Designation> CreateAsync(Designation designation)
    {
        await _context.Designations.AddAsync(designation);
        await _context.SaveChangesAsync();
        return designation;
    }

    public async Task<Designation?> UpdateAsync(int id, Designation designation)
    {
        var existing = await _context.Designations.FindAsync(id);
        if (existing == null) return null;

        existing.Title = designation.Title;
        existing.Description = designation.Description;
        existing.DepartmentId = designation.DepartmentId;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var designation = await _context.Designations.FindAsync(id);
        if (designation == null) return false;

        designation.IsDeleted = true;
        designation.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
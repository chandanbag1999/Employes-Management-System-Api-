using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Leave.DTOs;
using EMS.Application.Modules.Leave.Interfaces;
using EMS.Domain.Entities.Leave;
using EMS.Domain.Enums;
using EMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EMS.Infrastructure.Repositories;

public class LeaveRepository : ILeaveRepository
{
    private readonly AppDbContext _context;

    public LeaveRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<LeaveApplication>> GetAllAsync(
        LeaveFilterDto filter)
    {
        var query = _context.LeaveApplications
            .Include(l => l.Employee)
                .ThenInclude(e => e!.Department)
            .Include(l => l.LeaveType)
            .Include(l => l.ApprovedBy)
            .AsQueryable();

        if (filter.EmployeeId.HasValue)
            query = query.Where(l => l.EmployeeId == filter.EmployeeId);

        if (filter.DepartmentId.HasValue)
            query = query.Where(l =>
                l.Employee!.DepartmentId == filter.DepartmentId);

        if (filter.LeaveTypeId.HasValue)
            query = query.Where(l => l.LeaveTypeId == filter.LeaveTypeId);

        if (!string.IsNullOrWhiteSpace(filter.Status) &&
            Enum.TryParse<LeaveStatus>(filter.Status, true, out var status))
            query = query.Where(l => l.Status == status);

        if (filter.FromDate.HasValue)
            query = query.Where(l => l.FromDate >= filter.FromDate.Value.Date);

        if (filter.ToDate.HasValue)
            query = query.Where(l => l.ToDate <= filter.ToDate.Value.Date);

        var total = await query.CountAsync();
        var data = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PaginatedResult<LeaveApplication>
        {
            Data = data,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<LeaveApplication?> GetByIdAsync(int id)
        => await _context.LeaveApplications
            .Include(l => l.Employee)
                .ThenInclude(e => e!.Department)
            .Include(l => l.LeaveType)
            .Include(l => l.ApprovedBy)
            .FirstOrDefaultAsync(l => l.Id == id);

    public async Task<LeaveApplication> CreateAsync(LeaveApplication application)
    {
        await _context.LeaveApplications.AddAsync(application);
        await _context.SaveChangesAsync();
        return (await GetByIdAsync(application.Id))!;
    }

    public async Task<LeaveApplication?> UpdateStatusAsync(
        int id, LeaveStatus status, int? approvedById, string? rejectionReason)
    {
        var app = await _context.LeaveApplications.FindAsync(id);
        if (app == null) return null;

        app.Status = status;
        app.ApprovedById = approvedById;
        app.RejectionReason = rejectionReason;
        app.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<LeaveApplication?> CancelAsync(int id)
    {
        var app = await _context.LeaveApplications.FindAsync(id);
        if (app == null) return null;

        app.Status = LeaveStatus.Cancelled;
        app.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<int> GetUsedDaysAsync(
        int employeeId, int leaveTypeId, int year)
        => await _context.LeaveApplications
            .Where(l =>
                l.EmployeeId == employeeId &&
                l.LeaveTypeId == leaveTypeId &&
                l.Status == LeaveStatus.Approved &&
                l.FromDate.Year == year)
            .SumAsync(l => l.TotalDays);

    public async Task<int> GetPendingDaysAsync(
        int employeeId, int leaveTypeId, int year)
        => await _context.LeaveApplications
            .Where(l =>
                l.EmployeeId == employeeId &&
                l.LeaveTypeId == leaveTypeId &&
                l.Status == LeaveStatus.Pending &&
                l.FromDate.Year == year)
            .SumAsync(l => l.TotalDays);

    public async Task<bool> HasOverlappingLeaveAsync(
        int employeeId, DateTime from, DateTime to, int? excludeId = null)
        => await _context.LeaveApplications
            .AnyAsync(l =>
                l.EmployeeId == employeeId &&
                l.Status != LeaveStatus.Rejected &&
                l.Status != LeaveStatus.Cancelled &&
                l.FromDate.Date <= to &&
                l.ToDate.Date >= from &&
                (excludeId == null || l.Id != excludeId));

    public async Task<IEnumerable<LeaveType>> GetAllLeaveTypesAsync()
        => await _context.LeaveTypes
            .OrderBy(lt => lt.Name)
            .ToListAsync();

    public async Task<LeaveType?> GetLeaveTypeByIdAsync(int id)
        => await _context.LeaveTypes.FindAsync(id);

    public async Task<LeaveType> CreateLeaveTypeAsync(LeaveType leaveType)
    {
        await _context.LeaveTypes.AddAsync(leaveType);
        await _context.SaveChangesAsync();
        return leaveType;
    }
}
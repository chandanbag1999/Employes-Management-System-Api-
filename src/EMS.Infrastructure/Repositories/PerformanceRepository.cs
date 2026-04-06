using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Performance.DTOs;
using EMS.Application.Modules.Performance.Interfaces;
using EMS.Domain.Entities.Performance;
using EMS.Domain.Enums;
using EMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EMS.Infrastructure.Repositories;

public class PerformanceRepository : IPerformanceRepository
{
    private readonly AppDbContext _context;

    public PerformanceRepository(AppDbContext context)
    {
        _context = context;
    }

    // ── Goals ─────────────────────────────────────────────────────

    public async Task<IEnumerable<Goal>> GetGoalsAsync(
        int? employeeId, string? reviewCycle)
    {
        var query = _context.Goals
            .Include(g => g.Employee)
                .ThenInclude(e => e!.Department)
            .Include(g => g.SetByManager)
            .Where(g => !g.IsDeleted)
            .AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(g => g.EmployeeId == employeeId);

        if (!string.IsNullOrWhiteSpace(reviewCycle))
            query = query.Where(g =>
                g.ReviewCycle == reviewCycle.ToUpper());

        return await query
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<Goal?> GetGoalByIdAsync(int id)
        => await _context.Goals
            .Include(g => g.Employee)
                .ThenInclude(e => e!.Department)
            .Include(g => g.SetByManager)
            .FirstOrDefaultAsync(g => g.Id == id);

    public async Task<Goal> CreateGoalAsync(Goal goal)
    {
        await _context.Goals.AddAsync(goal);
        await _context.SaveChangesAsync();
        return (await GetGoalByIdAsync(goal.Id))!;
    }

    public async Task<Goal?> UpdateGoalAsync(int id, Goal goal)
    {
        var existing = await _context.Goals.FindAsync(id);
        if (existing == null) return null;

        existing.Title = goal.Title;
        existing.Description = goal.Description;
        existing.Deadline = goal.Deadline;
        existing.ReviewCycle = goal.ReviewCycle;
        existing.Priority = goal.Priority;
        existing.Category = goal.Category;
        existing.Tags = goal.Tags;
        existing.SetByManagerId = goal.SetByManagerId;
        existing.ProgressPercent = goal.ProgressPercent;
        existing.Status = goal.Status;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetGoalByIdAsync(id);
    }

    public async Task<bool> DeleteGoalAsync(int id)
    {
        var goal = await _context.Goals.FindAsync(id);
        if (goal == null) return false;

        goal.IsDeleted = true;
        goal.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Goal>> GetEmployeeGoalsAsync(int employeeId, int? year)
    {
        var query = _context.Goals
            .Include(g => g.Employee)
                .ThenInclude(e => e!.Department)
            .Where(g => g.EmployeeId == employeeId);

        if (year.HasValue)
            query = query.Where(g => g.ReviewCycle.StartsWith(year.ToString()!));

        return await query.OrderByDescending(g => g.CreatedAt).ToListAsync();
    }

    // ── Reviews ───────────────────────────────────────────────────

    public async Task<PaginatedResult<PerformanceReview>> GetAllReviewsAsync(
        PerformanceFilterDto filter)
    {
        var query = _context.PerformanceReviews
            .Include(r => r.Employee)
                .ThenInclude(e => e!.Department)
            .Include(r => r.Reviewer)
            .AsQueryable();

        if (filter.EmployeeId.HasValue)
            query = query.Where(r => r.EmployeeId == filter.EmployeeId);

        if (filter.DepartmentId.HasValue)
            query = query.Where(r =>
                r.Employee!.DepartmentId == filter.DepartmentId);

        if (!string.IsNullOrWhiteSpace(filter.ReviewCycle))
            query = query.Where(r =>
                r.ReviewCycle == filter.ReviewCycle.ToUpper());

        if (filter.Year.HasValue)
            query = query.Where(r => r.Year == filter.Year);

        if (!string.IsNullOrWhiteSpace(filter.Quarter))
            query = query.Where(r =>
                r.Quarter == filter.Quarter.ToUpper());

        if (Enum.TryParse<ReviewStatus>(filter.Status, out var reviewStatus))
            query = query.Where(r => r.Status == reviewStatus);

        var total = await query.CountAsync();
        var data = await query
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Quarter)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PaginatedResult<PerformanceReview>
        {
            Data = data,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<PerformanceReview?> GetReviewByIdAsync(int id)
        => await _context.PerformanceReviews
            .Include(r => r.Employee)
                .ThenInclude(e => e!.Department)
            .Include(r => r.Reviewer)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<PerformanceReview?> GetReviewByCycleAsync(
        int employeeId, string reviewCycle)
        => await _context.PerformanceReviews
            .FirstOrDefaultAsync(r =>
                r.EmployeeId == employeeId &&
                r.ReviewCycle == reviewCycle);

    public async Task<PerformanceReview> CreateReviewAsync(PerformanceReview review)
    {
        await _context.PerformanceReviews.AddAsync(review);
        await _context.SaveChangesAsync();
        return (await GetReviewByIdAsync(review.Id))!;
    }

    public async Task<PerformanceReview?> UpdateReviewAsync(
        int id, PerformanceReview review)
    {
        var existing = await _context.PerformanceReviews.FindAsync(id);
        if (existing == null) return null;

        existing.TechnicalSkillRating = review.TechnicalSkillRating;
        existing.CommunicationRating = review.CommunicationRating;
        existing.TeamworkRating = review.TeamworkRating;
        existing.LeadershipRating = review.LeadershipRating;
        existing.PunctualityRating = review.PunctualityRating;
        existing.OverallRating = review.OverallRating;
        existing.Strengths = review.Strengths;
        existing.AreasOfImprovement = review.AreasOfImprovement;
        existing.ReviewerComments = review.ReviewerComments;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetReviewByIdAsync(id);
    }

    public async Task<PerformanceReview?> AddSelfCommentAsync(int id, string comment)
    {
        var review = await _context.PerformanceReviews.FindAsync(id);
        if (review == null) return null;

        review.EmployeeSelfComment = comment;
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetReviewByIdAsync(id);
    }

    public async Task<PerformanceReview?> SubmitReviewAsync(int id)
    {
        var review = await _context.PerformanceReviews.FindAsync(id);
        if (review == null) return null;

        review.Status = ReviewStatus.Submitted;
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetReviewByIdAsync(id);
    }

    public async Task<List<PerformanceReview>> GetEmployeeReviewsAsync(int employeeId)
        => await _context.PerformanceReviews
            .Include(r => r.Employee)
                .ThenInclude(e => e!.Department)
            .Include(r => r.Reviewer)
            .Where(r => r.EmployeeId == employeeId)
            .OrderByDescending(r => r.Year)
            .ToListAsync();
}
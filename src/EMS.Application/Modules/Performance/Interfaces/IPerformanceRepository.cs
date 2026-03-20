using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Performance.DTOs;
using EMS.Domain.Entities.Performance;

namespace EMS.Application.Modules.Performance.Interfaces;

public interface IPerformanceRepository
{
    // Goals
    Task<IEnumerable<Goal>> GetGoalsAsync(int? employeeId, string? reviewCycle);
    Task<Goal?> GetGoalByIdAsync(int id);
    Task<Goal> CreateGoalAsync(Goal goal);
    Task<Goal?> UpdateGoalAsync(int id, Goal goal);
    Task<bool> DeleteGoalAsync(int id);

    // Reviews
    Task<PaginatedResult<PerformanceReview>> GetAllReviewsAsync(
        PerformanceFilterDto filter);
    Task<PerformanceReview?> GetReviewByIdAsync(int id);
    Task<PerformanceReview?> GetReviewByCycleAsync(int employeeId, string reviewCycle);
    Task<PerformanceReview> CreateReviewAsync(PerformanceReview review);
    Task<PerformanceReview?> UpdateReviewAsync(int id, PerformanceReview review);
    Task<PerformanceReview?> AddSelfCommentAsync(int id, string comment);
    Task<PerformanceReview?> SubmitReviewAsync(int id);

    // Summary
    Task<List<Goal>> GetEmployeeGoalsAsync(int employeeId, int? year);
    Task<List<PerformanceReview>> GetEmployeeReviewsAsync(int employeeId);
}
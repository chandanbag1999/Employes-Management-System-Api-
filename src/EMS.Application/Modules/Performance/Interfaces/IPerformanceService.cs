using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Performance.DTOs;

namespace EMS.Application.Modules.Performance.Interfaces;

public interface IPerformanceService
{
    // Goals
    Task<IEnumerable<GoalResponseDto>> GetGoalsAsync(
        int? employeeId, string? reviewCycle);
    Task<GoalResponseDto?> GetGoalByIdAsync(int id);
    Task<GoalResponseDto> CreateGoalAsync(CreateGoalDto dto);
    Task<(GoalResponseDto? result, string? error)> UpdateProgressAsync(
        int id, UpdateGoalProgressDto dto);
    Task<(bool success, string? error)> DeleteGoalAsync(int id);

    // Reviews
    Task<PaginatedResult<ReviewResponseDto>> GetAllReviewsAsync(
        PerformanceFilterDto filter);
    Task<ReviewResponseDto?> GetReviewByIdAsync(int id);
    Task<(ReviewResponseDto? result, string? error)> CreateReviewAsync(
        CreateReviewDto dto);
    Task<(ReviewResponseDto? result, string? error)> AddSelfCommentAsync(
        int id, SelfCommentDto dto);
    Task<(ReviewResponseDto? result, string? error)> SubmitReviewAsync(int id);

    // Summary
    Task<EmployeePerformanceSummaryDto?> GetEmployeeSummaryAsync(
        int employeeId, int year);
}
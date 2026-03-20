using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Leave.DTOs;

namespace EMS.Application.Modules.Leave.Interfaces;

public interface ILeaveService
{
    // Leave Applications
    Task<(LeaveResponseDto? result, string? error)> ApplyAsync(ApplyLeaveDto dto);
    Task<(LeaveResponseDto? result, string? error)> ApproveOrRejectAsync(
        int id, LeaveActionDto dto);
    Task<(bool success, string? error)> CancelAsync(int id, int employeeId);
    Task<PaginatedResult<LeaveResponseDto>> GetAllAsync(LeaveFilterDto filter);
    Task<LeaveResponseDto?> GetByIdAsync(int id);

    // Leave Balance
    Task<LeaveBalanceDto?> GetBalanceAsync(int employeeId, int year);

    // Leave Types
    Task<IEnumerable<LeaveTypeResponseDto>> GetAllLeaveTypesAsync();
    Task<LeaveTypeResponseDto> CreateLeaveTypeAsync(CreateLeaveTypeDto dto);
}
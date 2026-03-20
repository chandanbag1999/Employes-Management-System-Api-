using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Leave.DTOs;
using EMS.Domain.Entities.Leave;
using EMS.Domain.Enums;

namespace EMS.Application.Modules.Leave.Interfaces;

public interface ILeaveRepository
{
    Task<PaginatedResult<LeaveApplication>> GetAllAsync(LeaveFilterDto filter);
    Task<LeaveApplication?> GetByIdAsync(int id);
    Task<LeaveApplication> CreateAsync(LeaveApplication application);
    Task<LeaveApplication?> UpdateStatusAsync(int id, LeaveStatus status,
        int? approvedById, string? rejectionReason);
    Task<LeaveApplication?> CancelAsync(int id);
    Task<int> GetUsedDaysAsync(int employeeId, int leaveTypeId, int year);
    Task<int> GetPendingDaysAsync(int employeeId, int leaveTypeId, int year);
    Task<bool> HasOverlappingLeaveAsync(int employeeId, DateTime from, DateTime to,
        int? excludeId = null);

    // Leave Types
    Task<IEnumerable<LeaveType>> GetAllLeaveTypesAsync();
    Task<LeaveType?> GetLeaveTypeByIdAsync(int id);
    Task<LeaveType> CreateLeaveTypeAsync(LeaveType leaveType);
}
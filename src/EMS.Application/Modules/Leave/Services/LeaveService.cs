using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Leave.DTOs;
using EMS.Application.Modules.Leave.Interfaces;
using EMS.Domain.Entities.Leave;
using EMS.Domain.Enums;

namespace EMS.Application.Modules.Leave.Services;

public class LeaveService : ILeaveService
{
    private readonly ILeaveRepository _leaveRepo;

    public LeaveService(ILeaveRepository leaveRepo)
    {
        _leaveRepo = leaveRepo;
    }

    public async Task<(LeaveResponseDto? result, string? error)> ApplyAsync(
        ApplyLeaveDto dto)
    {
        // Date validation
        var from = dto.FromDate.Date;
        var to = dto.ToDate.Date;

        if (from > to)
            return (null, "From date cannot be after To date.");

        if (from < DateTime.Today)
            return (null, "Cannot apply leave for past dates.");

        // Total days calculate — weekends count bhi hote hain basic mein
        var totalDays = (int)(to - from).TotalDays + 1;

        // Overlapping leave check
        if (await _leaveRepo.HasOverlappingLeaveAsync(dto.EmployeeId, from, to))
            return (null, "You already have a leave application for these dates.");

        // Leave type exist karta hai?
        var leaveType = await _leaveRepo.GetLeaveTypeByIdAsync(dto.LeaveTypeId);
        if (leaveType == null)
            return (null, "Invalid leave type.");

        // Balance check
        var year = from.Year;
        var usedDays = await _leaveRepo.GetUsedDaysAsync(
            dto.EmployeeId, dto.LeaveTypeId, year);
        var pendingDays = await _leaveRepo.GetPendingDaysAsync(
            dto.EmployeeId, dto.LeaveTypeId, year);

        var remaining = leaveType.MaxDaysPerYear - usedDays - pendingDays;
        if (totalDays > remaining)
            return (null,
                $"Insufficient leave balance. Remaining: {remaining} days.");

        var application = new LeaveApplication
        {
            EmployeeId = dto.EmployeeId,
            LeaveTypeId = dto.LeaveTypeId,
            FromDate = DateTime.SpecifyKind(from, DateTimeKind.Utc),
            ToDate = DateTime.SpecifyKind(to, DateTimeKind.Utc),
            TotalDays = totalDays,
            Reason = dto.Reason.Trim(),
            Status = LeaveStatus.Pending
        };

        var created = await _leaveRepo.CreateAsync(application);
        return (MapToDto(created), null);
    }

    public async Task<(LeaveResponseDto? result, string? error)> ApproveOrRejectAsync(
        int id, LeaveActionDto dto)
    {
        var application = await _leaveRepo.GetByIdAsync(id);
        if (application == null)
            return (null, "Leave application not found.");

        if (application.Status != LeaveStatus.Pending)
            return (null, "Only pending applications can be approved/rejected.");

        if (!dto.IsApproved && string.IsNullOrWhiteSpace(dto.RejectionReason))
            return (null, "Rejection reason is required.");

        var newStatus = dto.IsApproved
            ? LeaveStatus.Approved
            : LeaveStatus.Rejected;

        var updated = await _leaveRepo.UpdateStatusAsync(
            id, newStatus, dto.ActionById, dto.RejectionReason);

        return updated == null
            ? (null, "Action failed.")
            : (MapToDto(updated), null);
    }

    public async Task<(bool success, string? error)> CancelAsync(
        int id, int employeeId)
    {
        var application = await _leaveRepo.GetByIdAsync(id);
        if (application == null)
            return (false, "Leave application not found.");

        if (application.EmployeeId != employeeId)
            return (false, "You can only cancel your own leave.");

        if (application.Status == LeaveStatus.Approved &&
            application.FromDate.Date <= DateTime.Today)
            return (false, "Cannot cancel leave that has already started.");

        if (application.Status == LeaveStatus.Rejected)
            return (false, "Cannot cancel a rejected leave.");

        var cancelled = await _leaveRepo.CancelAsync(id);
        return cancelled != null
            ? (true, null)
            : (false, "Cancel failed.");
    }

    public async Task<PaginatedResult<LeaveResponseDto>> GetAllAsync(
        LeaveFilterDto filter)
    {
        var result = await _leaveRepo.GetAllAsync(filter);
        return new PaginatedResult<LeaveResponseDto>
        {
            Data = result.Data.Select(MapToDto),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<LeaveResponseDto?> GetByIdAsync(int id)
    {
        var app = await _leaveRepo.GetByIdAsync(id);
        return app == null ? null : MapToDto(app);
    }

    public async Task<LeaveBalanceDto?> GetBalanceAsync(int employeeId, int year)
    {
        var leaveTypes = await _leaveRepo.GetAllLeaveTypesAsync();
        var items = new List<LeaveBalanceItemDto>();

        foreach (var lt in leaveTypes)
        {
            var used = await _leaveRepo.GetUsedDaysAsync(employeeId, lt.Id, year);
            var pending = await _leaveRepo.GetPendingDaysAsync(employeeId, lt.Id, year);

            items.Add(new LeaveBalanceItemDto
            {
                LeaveTypeId = lt.Id,
                LeaveTypeName = lt.Name,
                TotalAllowed = lt.MaxDaysPerYear,
                Used = used,
                Pending = pending
            });
        }

        return new LeaveBalanceDto
        {
            EmployeeId = employeeId,
            Year = year,
            Balances = items
        };
    }

    public async Task<IEnumerable<LeaveTypeResponseDto>> GetAllLeaveTypesAsync()
    {
        var types = await _leaveRepo.GetAllLeaveTypesAsync();
        return types.Select(lt => new LeaveTypeResponseDto
        {
            Id = lt.Id,
            Name = lt.Name,
            MaxDaysPerYear = lt.MaxDaysPerYear,
            IsCarryForwardAllowed = lt.IsCarryForwardAllowed,
            IsPaid = lt.IsPaid
        });
    }

    public async Task<LeaveTypeResponseDto> CreateLeaveTypeAsync(CreateLeaveTypeDto dto)
    {
        var leaveType = new LeaveType
        {
            Name = dto.Name.Trim(),
            MaxDaysPerYear = dto.MaxDaysPerYear,
            IsCarryForwardAllowed = dto.IsCarryForwardAllowed,
            IsPaid = dto.IsPaid
        };

        var created = await _leaveRepo.CreateLeaveTypeAsync(leaveType);
        return new LeaveTypeResponseDto
        {
            Id = created.Id,
            Name = created.Name,
            MaxDaysPerYear = created.MaxDaysPerYear,
            IsCarryForwardAllowed = created.IsCarryForwardAllowed,
            IsPaid = created.IsPaid
        };
    }

    private static LeaveResponseDto MapToDto(LeaveApplication a) => new()
    {
        Id = a.Id,
        EmployeeId = a.EmployeeId,
        EmployeeName = a.Employee != null
            ? $"{a.Employee.FirstName} {a.Employee.LastName}"
            : "Unknown",
        EmployeeCode = a.Employee?.EmployeeCode ?? "",
        DepartmentName = a.Employee?.Department?.Name ?? "",
        LeaveTypeId = a.LeaveTypeId,
        LeaveTypeName = a.LeaveType?.Name ?? "",
        FromDate = a.FromDate,
        ToDate = a.ToDate,
        TotalDays = a.TotalDays,
        Reason = a.Reason,
        Status = a.Status.ToString(),
        ApprovedById = a.ApprovedById,
        ApprovedByName = a.ApprovedBy != null
            ? $"{a.ApprovedBy.FirstName} {a.ApprovedBy.LastName}"
            : null,
        RejectionReason = a.RejectionReason,
        CreatedAt = a.CreatedAt
    };
}
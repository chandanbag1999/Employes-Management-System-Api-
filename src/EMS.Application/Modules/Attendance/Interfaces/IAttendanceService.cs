using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Attendance.DTOs;

namespace EMS.Application.Modules.Attendance.Interfaces;

public interface IAttendanceService
{
    // Employee actions
    Task<(AttendanceResponseDto? result, string? error)> ClockInAsync(ClockInDto dto);
    Task<(AttendanceResponseDto? result, string? error)> ClockOutAsync(ClockOutDto dto);

    // Read operations
    Task<PaginatedResult<AttendanceResponseDto>> GetAllAsync(AttendanceFilterDto filter);
    Task<AttendanceResponseDto?> GetByIdAsync(int id);
    Task<AttendanceResponseDto?> GetTodayRecordAsync(int employeeId);

    // Monthly summary
    Task<MonthlyAttendanceSummaryDto?> GetMonthlySummaryAsync(
        int employeeId, int month, int year);

    // Admin operations
    Task<(AttendanceResponseDto? result, string? error)> MarkManualAsync(
        ManualAttendanceDto dto);
}
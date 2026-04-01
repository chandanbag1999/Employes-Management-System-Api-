using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Attendance.DTOs;

namespace EMS.Application.Modules.Attendance.Interfaces;

public interface IAttendanceService
{
    Task<(AttendanceResponseDto? result, string? error)> ClockInAsync(
        ClockInDto dto, int employeeId);

    Task<(AttendanceResponseDto? result, string? error)> ClockOutAsync(
        ClockOutDto dto, int employeeId);

    Task<PaginatedResult<AttendanceResponseDto>> GetAllAsync(AttendanceFilterDto filter);

    Task<AttendanceResponseDto?> GetByIdAsync(int id);

    Task<AttendanceResponseDto?> GetTodayRecordAsync(int employeeId);

    Task<MonthlyAttendanceSummaryDto?> GetMonthlySummaryAsync(
        int employeeId, int month, int year);

    Task<(AttendanceResponseDto? result, string? error)> MarkManualAsync(
        ManualAttendanceDto dto);

    // ✅ NEW — Auto absent marking ke liye
    Task<int> MarkAbsentForDateAsync(DateTime date);
}
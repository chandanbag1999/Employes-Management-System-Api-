using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Attendance.DTOs;
using EMS.Application.Modules.Attendance.Interfaces;
using EMS.Domain.Entities.Attendance;
using EMS.Domain.Enums;

namespace EMS.Application.Modules.Attendance.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _attendanceRepo;

    public AttendanceService(IAttendanceRepository attendanceRepo)
    {
        _attendanceRepo = attendanceRepo;
    }

    public async Task<(AttendanceResponseDto? result, string? error)> ClockInAsync(
        ClockInDto dto)
    {
        // Already clocked in today?
        if (await _attendanceRepo.HasClockedInTodayAsync(dto.EmployeeId))
            return (null, "Already clocked in today.");

        var now = dto.ClockInTime ?? DateTime.UtcNow;

        var record = new AttendanceRecord
        {
            EmployeeId = dto.EmployeeId,
            Date = now.Date,
            ClockIn = now.TimeOfDay,
            Status = AttendanceStatus.Present,
            Remarks = dto.Remarks
        };

        var created = await _attendanceRepo.CreateAsync(record);
        return (MapToDto(created), null);
    }

    public async Task<(AttendanceResponseDto? result, string? error)> ClockOutAsync(
        ClockOutDto dto)
    {
        var todayRecord = await _attendanceRepo.GetTodayRecordAsync(dto.EmployeeId);

        if (todayRecord == null)
            return (null, "No clock-in found for today.");

        if (await _attendanceRepo.HasClockedOutTodayAsync(dto.EmployeeId))
            return (null, "Already clocked out today.");

        var now = dto.ClockOutTime ?? DateTime.UtcNow;
        var clockOut = now.TimeOfDay;

        // Working hours calculate karo
        double workingHours = 0;
        if (todayRecord.ClockIn.HasValue)
        {
            var diff = clockOut - todayRecord.ClockIn.Value;
            workingHours = Math.Round(diff.TotalHours, 2);
        }

        // HalfDay check — agar 4 ghante se kam kaam kiya
        var status = workingHours >= 4
            ? AttendanceStatus.Present
            : AttendanceStatus.HalfDay;

        todayRecord.ClockOut = clockOut;
        todayRecord.WorkingHours = workingHours;
        todayRecord.Status = status;
        todayRecord.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(dto.Remarks))
            todayRecord.Remarks = dto.Remarks;

        var updated = await _attendanceRepo.UpdateAsync(todayRecord.Id, todayRecord);
        return updated == null
            ? (null, "Clock out failed.")
            : (MapToDto(updated), null);
    }

    public async Task<PaginatedResult<AttendanceResponseDto>> GetAllAsync(
        AttendanceFilterDto filter)
    {
        var result = await _attendanceRepo.GetAllAsync(filter);
        return new PaginatedResult<AttendanceResponseDto>
        {
            Data = result.Data.Select(MapToDto),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<AttendanceResponseDto?> GetByIdAsync(int id)
    {
        var record = await _attendanceRepo.GetByIdAsync(id);
        return record == null ? null : MapToDto(record);
    }

    public async Task<AttendanceResponseDto?> GetTodayRecordAsync(int employeeId)
    {
        var record = await _attendanceRepo.GetTodayRecordAsync(employeeId);
        return record == null ? null : MapToDto(record);
    }

    public async Task<MonthlyAttendanceSummaryDto?> GetMonthlySummaryAsync(
        int employeeId, int month, int year)
    {
        var records = await _attendanceRepo.GetMonthlyAsync(employeeId, month, year);
        var list = records.ToList();

        if (!list.Any()) return null;

        var firstRecord = list.First();
        var employeeName = firstRecord.Employee != null
            ? $"{firstRecord.Employee.FirstName} {firstRecord.Employee.LastName}"
            : "Unknown";
        var employeeCode = firstRecord.Employee?.EmployeeCode ?? "";

        return new MonthlyAttendanceSummaryDto
        {
            EmployeeId = employeeId,
            EmployeeName = employeeName,
            EmployeeCode = employeeCode,
            Month = month,
            Year = year,
            TotalWorkingDays = list.Count,
            PresentDays = list.Count(r => r.Status == AttendanceStatus.Present),
            AbsentDays = list.Count(r => r.Status == AttendanceStatus.Absent),
            HalfDays = list.Count(r => r.Status == AttendanceStatus.HalfDay),
            LeaveDays = list.Count(r => r.Status == AttendanceStatus.OnLeave),
            HolidayDays = list.Count(r => r.Status == AttendanceStatus.Holiday),
            TotalWorkingHours = list
                .Where(r => r.WorkingHours.HasValue)
                .Sum(r => r.WorkingHours!.Value)
        };
    }

    public async Task<(AttendanceResponseDto? result, string? error)> MarkManualAsync(
        ManualAttendanceDto dto)
    {
        var date = dto.Date.Date;

        // Working hours calculate karo agar dono time hain
        double? workingHours = null;
        if (dto.ClockIn.HasValue && dto.ClockOut.HasValue)
        {
            var diff = dto.ClockOut.Value - dto.ClockIn.Value;
            workingHours = Math.Round(diff.TotalHours, 2);
        }

        var record = new AttendanceRecord
        {
            EmployeeId = dto.EmployeeId,
            Date = date,
            ClockIn = dto.ClockIn,
            ClockOut = dto.ClockOut,
            Status = dto.Status,
            WorkingHours = workingHours,
            Remarks = dto.Remarks
        };

        var created = await _attendanceRepo.CreateAsync(record);
        return (MapToDto(created), null);
    }

    private static AttendanceResponseDto MapToDto(AttendanceRecord r) => new()
    {
        Id = r.Id,
        EmployeeId = r.EmployeeId,
        EmployeeName = r.Employee != null
            ? $"{r.Employee.FirstName} {r.Employee.LastName}"
            : "Unknown",
        EmployeeCode = r.Employee?.EmployeeCode ?? "",
        Date = r.Date,
        ClockIn = r.ClockIn,
        ClockOut = r.ClockOut,
        Status = r.Status.ToString(),
        WorkingHours = r.WorkingHours,
        Remarks = r.Remarks
    };
}
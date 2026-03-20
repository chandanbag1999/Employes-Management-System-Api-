using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Attendance.DTOs;
using EMS.Domain.Entities.Attendance;

namespace EMS.Application.Modules.Attendance.Interfaces;

public interface IAttendanceRepository
{
    Task<PaginatedResult<AttendanceRecord>> GetAllAsync(AttendanceFilterDto filter);
    Task<AttendanceRecord?> GetByIdAsync(int id);
    Task<AttendanceRecord?> GetTodayRecordAsync(int employeeId);
    Task<IEnumerable<AttendanceRecord>> GetMonthlyAsync(int employeeId, int month, int year);
    Task<AttendanceRecord> CreateAsync(AttendanceRecord record);
    Task<AttendanceRecord?> UpdateAsync(int id, AttendanceRecord record);
    Task<bool> HasClockedInTodayAsync(int employeeId);
    Task<bool> HasClockedOutTodayAsync(int employeeId);
}
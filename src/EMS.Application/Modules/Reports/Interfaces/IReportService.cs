using EMS.Application.Modules.Reports.DTOs;

namespace EMS.Application.Modules.Reports.Interfaces;

public interface IReportService
{
    Task<IEnumerable<AttendanceReportDto>> GetAttendanceReportAsync(
        int month, int year, int? departmentId);
    Task<IEnumerable<PayrollReportDto>> GetPayrollReportAsync(
        int month, int year, int? departmentId);
    Task<IEnumerable<HeadcountReportDto>> GetHeadcountReportAsync();
}
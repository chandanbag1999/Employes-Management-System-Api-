using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Payroll.DTOs;

namespace EMS.Application.Modules.Payroll.Interfaces;

public interface IPayrollService
{
    // Salary Structure
    Task<SalaryStructureResponseDto> CreateSalaryStructureAsync(
        CreateSalaryStructureDto dto);
    Task<IEnumerable<SalaryStructureResponseDto>> GetSalaryStructuresAsync(
        int? employeeId);

    // Payroll
    Task<(List<PayrollRecordResponseDto> results, string? error)> RunPayrollAsync(
        RunPayrollDto dto);
    Task<PaginatedResult<PayrollRecordResponseDto>> GetAllAsync(PayrollFilterDto filter);
    Task<PayrollRecordResponseDto?> GetByIdAsync(int id);
    Task<PayrollRecordResponseDto?> GetMyPayslipAsync(int employeeId, int month, int year);
    Task<PaginatedResult<PayrollRecordResponseDto>> GetMyPayslipsAsync(
        int employeeId, int? month, int? year);
    Task<(PayrollRecordResponseDto? result, string? error)> MarkAsPaidAsync(int id);
    Task<List<PayrollRecordResponseDto>> GetEmployeePayslipsAsync(int employeeId);
}
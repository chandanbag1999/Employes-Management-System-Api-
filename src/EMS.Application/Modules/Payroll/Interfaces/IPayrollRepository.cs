using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Payroll.DTOs;
using EMS.Domain.Entities.Payroll;

namespace EMS.Application.Modules.Payroll.Interfaces;

public interface IPayrollRepository
{
    // Salary Structure
    Task<SalaryStructure?> GetActiveSalaryStructureAsync(int employeeId);
    Task<SalaryStructure?> GetSalaryStructureByIdAsync(int id);
    Task<IEnumerable<SalaryStructure>> GetAllSalaryStructuresAsync(int? employeeId);
    Task<SalaryStructure> CreateSalaryStructureAsync(SalaryStructure structure);
    Task DeactivateAllStructuresAsync(int employeeId);

    // Payroll Records
    Task<PaginatedResult<PayrollRecord>> GetAllAsync(PayrollFilterDto filter);
    Task<PayrollRecord?> GetByIdAsync(int id);
    Task<PayrollRecord?> GetByEmployeeMonthAsync(int employeeId, int month, int year);
    Task<PayrollRecord> CreateAsync(PayrollRecord record);
    Task<PayrollRecord?> MarkAsPaidAsync(int id);
    Task<bool> ExistsAsync(int employeeId, int month, int year);

    // For payroll calculation
    Task<int> GetPresentDaysAsync(int employeeId, int month, int year);
    Task<int> GetLeaveDaysAsync(int employeeId, int month, int year);
    Task<List<int>> GetActiveEmployeeIdsAsync();
}
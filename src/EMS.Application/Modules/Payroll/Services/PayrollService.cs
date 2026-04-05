using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Payroll.DTOs;
using EMS.Application.Modules.Payroll.Interfaces;
using EMS.Domain.Entities.Payroll;
using EMS.Domain.Enums;

namespace EMS.Application.Modules.Payroll.Services;

public class PayrollService : IPayrollService
{
    private readonly IPayrollRepository _payrollRepo;

    // PF rate — 12% of Basic (India standard)
    private const decimal PfRate = 0.12m;

    // Tax slab — simplified (real mein complex hota hai)
    private const decimal TaxRate = 0.10m;

    public PayrollService(IPayrollRepository payrollRepo)
    {
        _payrollRepo = payrollRepo;
    }

    public async Task<SalaryStructureResponseDto> CreateSalaryStructureAsync(
        CreateSalaryStructureDto dto)
    {
        // Purani structures deactivate karo
        await _payrollRepo.DeactivateAllStructuresAsync(dto.EmployeeId);

        var gross = dto.BasicSalary + dto.HRA +
                    dto.TransportAllowance +
                    dto.MedicalAllowance +
                    dto.OtherAllowances;

        var structure = new SalaryStructure
        {
            EmployeeId = dto.EmployeeId,
            BasicSalary = dto.BasicSalary,
            HRA = dto.HRA,
            TransportAllowance = dto.TransportAllowance,
            MedicalAllowance = dto.MedicalAllowance,
            OtherAllowances = dto.OtherAllowances,
            GrossSalary = gross,
            EffectiveFrom = DateTime.SpecifyKind(dto.EffectiveFrom, DateTimeKind.Utc),
            IsActive = true
        };

        var created = await _payrollRepo.CreateSalaryStructureAsync(structure);
        return MapStructureToDto(created);
    }

    public async Task<IEnumerable<SalaryStructureResponseDto>> GetSalaryStructuresAsync(
        int? employeeId)
    {
        var structures = await _payrollRepo.GetAllSalaryStructuresAsync(employeeId);
        return structures.Select(MapStructureToDto);
    }

    public async Task<(List<PayrollRecordResponseDto> results, string? error)>
        RunPayrollAsync(RunPayrollDto dto)
    {
        // Employees determine karo
        var employeeIds = dto.EmployeeIds?.Any() == true
            ? dto.EmployeeIds
            : await _payrollRepo.GetActiveEmployeeIdsAsync();

        if (!employeeIds.Any())
            return (new List<PayrollRecordResponseDto>(),
                "No active employees found.");

        var results = new List<PayrollRecordResponseDto>();

        foreach (var empId in employeeIds)
        {
            // Already processed?
            if (await _payrollRepo.ExistsAsync(empId, dto.Month, dto.Year))
                continue;

            // Salary structure
            var structure = await _payrollRepo.GetActiveSalaryStructureAsync(empId);
            if (structure == null) continue;

            // Attendance data
            var totalWorkingDays = GetWorkingDaysInMonth(dto.Month, dto.Year);
            var presentDays = await _payrollRepo.GetPresentDaysAsync(
                empId, dto.Month, dto.Year);
            var leaveDays = await _payrollRepo.GetLeaveDaysAsync(
                empId, dto.Month, dto.Year);
            var lopDays = Math.Max(0, totalWorkingDays - presentDays - leaveDays);

            // Per day salary
            var perDaySalary = structure.GrossSalary / totalWorkingDays;

            // LOP deduction
            var lopDeduction = perDaySalary * lopDays;
            var adjustedGross = structure.GrossSalary - lopDeduction;

            // Deductions calculate karo
            var pfDeduction = Math.Round(structure.BasicSalary * PfRate, 2);
            var taxDeduction = Math.Round(adjustedGross * TaxRate, 2);
            var totalDeductions = pfDeduction + taxDeduction;

            // Net salary
            var netSalary = Math.Round(adjustedGross - totalDeductions, 2);

            var record = new PayrollRecord
            {
                EmployeeId = empId,
                Month = dto.Month,
                Year = dto.Year,
                BasicSalary = structure.BasicSalary,
                HRA = structure.HRA,
                TransportAllowance = structure.TransportAllowance,
                MedicalAllowance = structure.MedicalAllowance,
                OtherAllowances = structure.OtherAllowances,
                GrossEarnings = Math.Round(adjustedGross, 2),
                PfDeduction = pfDeduction,
                TaxDeduction = taxDeduction,
                OtherDeductions = Math.Round(lopDeduction, 2),
                TotalDeductions = Math.Round(totalDeductions + lopDeduction, 2),
                NetSalary = netSalary,
                WorkingDays = totalWorkingDays,
                PresentDays = presentDays,
                LeaveDays = leaveDays,
                LopDays = lopDays,
                Status = PayrollStatus.Generated
            };

            var created = await _payrollRepo.CreateAsync(record);
            results.Add(MapToDto(created));
        }

        return results.Any()
            ? (results, null)
            : (new List<PayrollRecordResponseDto>(),
               "Payroll already processed for all employees this month.");
    }

    public async Task<PaginatedResult<PayrollRecordResponseDto>> GetAllAsync(
        PayrollFilterDto filter)
    {
        var result = await _payrollRepo.GetAllAsync(filter);
        return new PaginatedResult<PayrollRecordResponseDto>
        {
            Data = result.Data.Select(MapToDto),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<PayrollRecordResponseDto?> GetByIdAsync(int id)
    {
        var record = await _payrollRepo.GetByIdAsync(id);
        return record == null ? null : MapToDto(record);
    }

    public async Task<PayrollRecordResponseDto?> GetMyPayslipAsync(
        int employeeId, int month, int year)
    {
        var record = await _payrollRepo.GetByEmployeeMonthAsync(
            employeeId, month, year);
        return record == null ? null : MapToDto(record);
    }

    public async Task<PaginatedResult<PayrollRecordResponseDto>> GetMyPayslipsAsync(
        int employeeId, int? month, int? year)
    {
        var filter = new PayrollFilterDto
        {
            EmployeeId = employeeId,
            Month = month,
            Year = year,
            PageSize = 50
        };
        var result = await _payrollRepo.GetAllAsync(filter);
        return new PaginatedResult<PayrollRecordResponseDto>
        {
            Data = result.Data.Select(MapToDto),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<(PayrollRecordResponseDto? result, string? error)>
        MarkAsPaidAsync(int id)
    {
        var record = await _payrollRepo.GetByIdAsync(id);
        if (record == null) return (null, "Payroll record not found.");
        if (record.Status == PayrollStatus.Paid) return (null, "Already marked as paid.");

        var updated = await _payrollRepo.MarkAsPaidAsync(id);
        return updated == null
            ? (null, "Failed to mark as paid.")
            : (MapToDto(updated), null);
    }

    public async Task<List<PayrollRecordResponseDto>> GetEmployeePayslipsAsync(
        int employeeId)
    {
        var filter = new PayrollFilterDto
        {
            EmployeeId = employeeId,
            PageSize = 24   // Last 2 years
        };
        var result = await _payrollRepo.GetAllAsync(filter);
        return result.Data.Select(MapToDto).ToList();
    }

    // Helpers
    private static int GetWorkingDaysInMonth(int month, int year)
    {
        var days = DateTime.DaysInMonth(year, month);
        var workingDays = 0;
        for (var d = 1; d <= days; d++)
        {
            var date = new DateTime(year, month, d);
            if (date.DayOfWeek != DayOfWeek.Saturday &&
                date.DayOfWeek != DayOfWeek.Sunday)
                workingDays++;
        }
        return workingDays;
    }

    private static SalaryStructureResponseDto MapStructureToDto(
        SalaryStructure s) => new()
    {
        Id = s.Id,
        EmployeeId = s.EmployeeId,
        EmployeeName = s.Employee != null
            ? $"{s.Employee.FirstName} {s.Employee.LastName}"
            : "Unknown",
        EmployeeCode = s.Employee?.EmployeeCode ?? "",
        BasicSalary = s.BasicSalary,
        HRA = s.HRA,
        TransportAllowance = s.TransportAllowance,
        MedicalAllowance = s.MedicalAllowance,
        OtherAllowances = s.OtherAllowances,
        GrossSalary = s.GrossSalary,
        EffectiveFrom = s.EffectiveFrom,
        IsActive = s.IsActive,
        CreatedAt = s.CreatedAt
    };

    private static PayrollRecordResponseDto MapToDto(PayrollRecord p) => new()
    {
        Id = p.Id,
        EmployeeId = p.EmployeeId,
        EmployeeName = p.Employee != null
            ? $"{p.Employee.FirstName} {p.Employee.LastName}"
            : "Unknown",
        EmployeeCode = p.Employee?.EmployeeCode ?? "",
        DepartmentName = p.Employee?.Department?.Name ?? "",
        Month = p.Month,
        Year = p.Year,
        BasicSalary = p.BasicSalary,
        HRA = p.HRA,
        TransportAllowance = p.TransportAllowance,
        MedicalAllowance = p.MedicalAllowance,
        OtherAllowances = p.OtherAllowances,
        GrossEarnings = p.GrossEarnings,
        PfDeduction = p.PfDeduction,
        TaxDeduction = p.TaxDeduction,
        OtherDeductions = p.OtherDeductions,
        TotalDeductions = p.TotalDeductions,
        NetSalary = p.NetSalary,
        WorkingDays = p.WorkingDays,
        PresentDays = p.PresentDays,
        LeaveDays = p.LeaveDays,
        LopDays = p.LopDays,
        Status = p.Status.ToString(),
        PaidOn = p.PaidOn,
        CreatedAt = p.CreatedAt
    };
}
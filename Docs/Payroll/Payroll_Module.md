# Payroll Module

**Status:** Fully Implemented ✅

---

## 1. Module Overview

This module handles:
- Salary structure definition per employee
- Monthly payroll processing (automated calculation)
- LOP (Loss of Pay) calculations based on attendance
- Payslip generation and viewing
- Marking payroll as paid

### Related Files

| Layer | File | Purpose |
|-------|------|---------|
| **Domain** | `src/EMS.Domain/Entities/Payroll/PayrollRecord.cs` | Payroll record entity |
| **Domain** | `src/EMS.Domain/Entities/Payroll/SalaryStructure.cs` | Salary structure entity |
| **Application** | `src/EMS.Application/Modules/Payroll/DTOs/CreateSalaryStructureDto.cs` | Create salary DTO |
| **Application** | `src/EMS.Application/Modules/Payroll/DTOs/PayrollFilterDto.cs` | Filter DTO |
| **Application** | `src/EMS.Application/Modules/Payroll/DTOs/PayrollRecordResponseDto.cs` | Response DTO |
| **Application** | `src/EMS.Application/Modules/Payroll/DTOs/RunPayrollDto.cs` | Run payroll DTO |
| **Application** | `src/EMS.Application/Modules/Payroll/DTOs/SalaryStructureResponseDto.cs` | Structure response DTO |
| **Application** | `src/EMS.Application/Modules/Payroll/Interfaces/IPayrollService.cs` | Service interface |
| **Application** | `src/EMS.Application/Modules/Payroll/Interfaces/IPayrollRepository.cs` | Repository interface |
| **Application** | `src/EMS.Application/Modules/Payroll/Services/PayrollService.cs` | Business logic |
| **Infrastructure** | `src/EMS.Infrastructure/Repositories/PayrollRepository.cs` | Data access |
| **API** | `src/EMS.API/Controllers/v1/PayrollController.cs` | Endpoints |

---

## 2. Entities

### 2.1 SalaryStructure
```csharp
// src/EMS.Domain/Entities/Payroll/SalaryStructure.cs
using EMS.Domain.Common;
using EMS.Domain.Entities.Employee;

public class SalaryStructure : BaseEntity
{
    public int EmployeeId { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal HRA { get; set; }
    public decimal TransportAllowance { get; set; }
    public decimal MedicalAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal GrossSalary { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public EmployeeProfile? Employee { get; set; }
}
```

### 2.2 PayrollRecord
```csharp
// src/EMS.Domain/Entities/Payroll/PayrollRecord.cs
using EMS.Domain.Common;
using EMS.Domain.Entities.Employee;

public class PayrollRecord : BaseEntity
{
    public int EmployeeId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }

    // Earnings
    public decimal BasicSalary { get; set; }
    public decimal HRA { get; set; }
    public decimal TransportAllowance { get; set; }
    public decimal MedicalAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal GrossEarnings { get; set; }

    // Deductions
    public decimal PfDeduction { get; set; }
    public decimal TaxDeduction { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal TotalDeductions { get; set; }

    // Net
    public decimal NetSalary { get; set; }

    // Attendance
    public int WorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int LeaveDays { get; set; }
    public int LopDays { get; set; }

    public string Status { get; set; } = "Generated";  // Generated, Paid
    public DateTime? PaidOn { get; set; }

    // Navigation
    public EmployeeProfile? Employee { get; set; }
}
```

---

## 3. DTOs

### 3.1 CreateSalaryStructureDto
```csharp
// src/EMS.Application/Modules/Payroll/DTOs/CreateSalaryStructureDto.cs
using System.ComponentModel.DataAnnotations;

public class CreateSalaryStructureDto
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    [Range(1, double.MaxValue)]
    public decimal BasicSalary { get; set; }

    [Range(0, double.MaxValue)]
    public decimal HRA { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TransportAllowance { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MedicalAllowance { get; set; }

    [Range(0, double.MaxValue)]
    public decimal OtherAllowances { get; set; }

    [Required]
    public DateTime EffectiveFrom { get; set; }
}
```

### 3.2 RunPayrollDto
```csharp
// src/EMS.Application/Modules/Payroll/DTOs/RunPayrollDto.cs
using System.ComponentModel.DataAnnotations;

// HR/Admin monthly payroll run karne ke liye
public class RunPayrollDto
{
    [Required]
    [Range(1, 12)]
    public int Month { get; set; }

    [Required]
    [Range(2020, 2100)]
    public int Year { get; set; }

    // Specific employees ke liye ya sab ke liye
    public List<int>? EmployeeIds { get; set; }
}
```

### 3.3 PayrollFilterDto
```csharp
// src/EMS.Application/Modules/Payroll/DTOs/PayrollFilterDto.cs
public class PayrollFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? EmployeeId { get; set; }
    public int? DepartmentId { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
    public string? Status { get; set; }  // "Generated","Paid"
}
```

### 3.4 SalaryStructureResponseDto
```csharp
// src/EMS.Application/Modules/Payroll/DTOs/SalaryStructureResponseDto.cs
public class SalaryStructureResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public decimal HRA { get; set; }
    public decimal TransportAllowance { get; set; }
    public decimal MedicalAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal GrossSalary { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### 3.5 PayrollRecordResponseDto
```csharp
// src/EMS.Application/Modules/Payroll/DTOs/PayrollRecordResponseDto.cs
public class PayrollRecordResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public string MonthYear => $"{new DateTime(Year, Month, 1):MMMM yyyy}";

    // Earnings
    public decimal BasicSalary { get; set; }
    public decimal HRA { get; set; }
    public decimal TransportAllowance { get; set; }
    public decimal MedicalAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal GrossEarnings { get; set; }

    // Deductions
    public decimal PfDeduction { get; set; }      // 12% of Basic
    public decimal TaxDeduction { get; set; }      // TDS
    public decimal OtherDeductions { get; set; }
    public decimal TotalDeductions { get; set; }

    // Net Pay
    public decimal NetSalary { get; set; }

    // Attendance based
    public int WorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int LeaveDays { get; set; }
    public int LopDays { get; set; }       // Loss of Pay

    public string Status { get; set; } = string.Empty;
    public DateTime? PaidOn { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

## 4. Interfaces

### 4.1 IPayrollService
```csharp
// src/EMS.Application/Modules/Payroll/Interfaces/IPayrollService.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Payroll.DTOs;

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
    Task<(PayrollRecordResponseDto? result, string? error)> MarkAsPaidAsync(int id);
    Task<List<PayrollRecordResponseDto>> GetEmployeePayslipsAsync(int employeeId);
}
```

### 4.2 IPayrollRepository
```csharp
// src/EMS.Application/Modules/Payroll/Interfaces/IPayrollRepository.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Payroll.DTOs;
using EMS.Domain.Entities.Payroll;

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
```

---

## 5. Service Implementation

### PayrollService
```csharp
// src/EMS.Application/Modules/Payroll/Services/PayrollService.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Payroll.DTOs;
using EMS.Application.Modules.Payroll.Interfaces;
using EMS.Domain.Entities.Payroll;

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
                Status = "Generated"
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

    public async Task<(PayrollRecordResponseDto? result, string? error)>
        MarkAsPaidAsync(int id)
    {
        var record = await _payrollRepo.GetByIdAsync(id);
        if (record == null) return (null, "Payroll record not found.");
        if (record.Status == "Paid") return (null, "Already marked as paid.");

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
        Status = p.Status,
        PaidOn = p.PaidOn,
        CreatedAt = p.CreatedAt
    };
}
```

---

## 6. API Controller

### PayrollController
```csharp
// src/EMS.API/Controllers/v1/PayrollController.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Payroll.DTOs;
using EMS.Application.Modules.Payroll.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PayrollController : ControllerBase
{
    private readonly IPayrollService _service;

    public PayrollController(IPayrollService service)
    {
        _service = service;
    }

    // POST api/v1/payroll/salary-structure
    [HttpPost("salary-structure")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> CreateSalaryStructure(
        [FromBody] CreateSalaryStructureDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.CreateSalaryStructureAsync(dto);
        return Ok(ApiResponse<SalaryStructureResponseDto>.Ok(
            result, "Salary structure created."));
    }

    // GET api/v1/payroll/salary-structure?employeeId=1
    [HttpGet("salary-structure")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> GetSalaryStructures(
        [FromQuery] int? employeeId = null)
    {
        var result = await _service.GetSalaryStructuresAsync(employeeId);
        return Ok(ApiResponse<IEnumerable<SalaryStructureResponseDto>>.Ok(result));
    }

    // POST api/v1/payroll/run
    [HttpPost("run")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> RunPayroll([FromBody] RunPayrollDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (results, error) = await _service.RunPayrollAsync(dto);
        if (error != null && !results.Any())
            return BadRequest(ApiResponse<string>.Fail(error));

        return Ok(ApiResponse<List<PayrollRecordResponseDto>>.Ok(
            results,
            $"Payroll processed for {results.Count} employees."));
    }

    // GET api/v1/payroll?month=3&year=2026
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> GetAll([FromQuery] PayrollFilterDto filter)
    {
        var result = await _service.GetAllAsync(filter);
        return Ok(ApiResponse<PaginatedResult<PayrollRecordResponseDto>>.Ok(result));
    }

    // GET api/v1/payroll/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("Payroll record not found."));
        return Ok(ApiResponse<PayrollRecordResponseDto>.Ok(result));
    }

    // GET api/v1/payroll/payslip/1?month=3&year=2026
    [HttpGet("payslip/{employeeId}")]
    public async Task<IActionResult> GetMyPayslip(
        int employeeId,
        [FromQuery] int month,
        [FromQuery] int year)
    {
        var result = await _service.GetMyPayslipAsync(employeeId, month, year);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("Payslip not found."));
        return Ok(ApiResponse<PayrollRecordResponseDto>.Ok(result));
    }

    // GET api/v1/payroll/payslips/1
    [HttpGet("payslips/{employeeId}")]
    public async Task<IActionResult> GetEmployeePayslips(int employeeId)
    {
        var result = await _service.GetEmployeePayslipsAsync(employeeId);
        return Ok(ApiResponse<List<PayrollRecordResponseDto>>.Ok(result));
    }

    // PATCH api/v1/payroll/5/mark-paid
    [HttpPatch("{id}/mark-paid")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> MarkAsPaid(int id)
    {
        var (result, error) = await _service.MarkAsPaidAsync(id);
        if (error != null)
            return error.Contains("not found")
                ? NotFound(ApiResponse<string>.Fail(error))
                : BadRequest(ApiResponse<string>.Fail(error));

        return Ok(ApiResponse<PayrollRecordResponseDto>.Ok(
            result!, "Payroll marked as paid."));
    }
}
```

---

## 7. API Endpoints Summary

| Method | Endpoint | Authorization | Description |
|--------|----------|---------------|-------------|
| POST | `/api/v1/payroll/salary-structure` | SuperAdmin, HRAdmin | Create salary structure |
| GET | `/api/v1/payroll/salary-structure` | SuperAdmin, HRAdmin | Get salary structures |
| POST | `/api/v1/payroll/run` | SuperAdmin, HRAdmin | Run monthly payroll |
| GET | `/api/v1/payroll` | SuperAdmin, HRAdmin | List payroll records |
| GET | `/api/v1/payroll/{id}` | Authenticated | Get payroll by ID |
| GET | `/api/v1/payroll/payslip/{employeeId}` | Authenticated | Get specific payslip |
| GET | `/api/v1/payroll/payslips/{employeeId}` | Authenticated | Get all payslips |
| PATCH | `/api/v1/payroll/{id}/mark-paid` | SuperAdmin, HRAdmin | Mark as paid |

---

## 8. Features

### 8.1 Salary Structure
- Define basic salary and allowances per employee
- Only one active structure at a time
- Previous structures are automatically deactivated
- Effective from date tracking

### 8.2 Payroll Processing
- Automated monthly payroll run
- Fetches attendance data (present, leave, LOP days)
- Calculates working days excluding weekends
- LOP (Loss of Pay) calculation: `(Gross / Working Days) × LOP Days`
- PF deduction: 12% of Basic
- Tax deduction: 10% of Adjusted Gross
- Skips already processed employees

### 8.3 Payslip Access
- Employees can view their payslips
- Can filter by month/year
- View all past payslips

### 8.4 Payment Status
- Generated → Paid workflow
- Track payment date

---

## 9. API Usage Examples

### Create Salary Structure
```bash
POST /api/v1/payroll/salary-structure
Authorization: Bearer <token>
Content-Type: application/json

{
  "employeeId": 5,
  "basicSalary": 50000,
  "hra": 20000,
  "transportAllowance": 5000,
  "medicalAllowance": 3000,
  "otherAllowances": 2000,
  "effectiveFrom": "2026-04-01"
}

# Response 200:
{
  "success": true,
  "message": "Salary structure created.",
  "data": {
    "id": 1,
    "employeeId": 5,
    "employeeName": "John Doe",
    "basicSalary": 50000,
    "hra": 20000,
    "transportAllowance": 5000,
    "medicalAllowance": 3000,
    "otherAllowances": 2000,
    "grossSalary": 80000,
    "effectiveFrom": "2026-04-01T00:00:00Z",
    "isActive": true
  }
}
```

### Run Monthly Payroll
```bash
POST /api/v1/payroll/run
Authorization: Bearer <token>
Content-Type: application/json

{
  "month": 3,
  "year": 2026
}

# Response 200:
{
  "success": true,
  "message": "Payroll processed for 15 employees.",
  "data": [
    {
      "id": 25,
      "employeeId": 5,
      "employeeName": "John Doe",
      "month": 3,
      "year": 2026,
      "monthYear": "March 2026",
      "basicSalary": 50000,
      "hra": 20000,
      "transportAllowance": 5000,
      "medicalAllowance": 3000,
      "otherAllowances": 2000,
      "grossEarnings": 80000,
      "pfDeduction": 6000,
      "taxDeduction": 7400,
      "otherDeductions": 0,
      "totalDeductions": 13400,
      "netSalary": 66600,
      "workingDays": 22,
      "presentDays": 20,
      "leaveDays": 2,
      "lopDays": 0,
      "status": "Generated"
    }
  ]
}
```

### Get Payslip
```bash
GET /api/v1/payroll/payslip/5?month=3&year=2026
Authorization: Bearer <token>

# Response 200:
{
  "success": true,
  "data": {
    "id": 25,
    "employeeId": 5,
    "employeeName": "John Doe",
    "employeeCode": "EMP005",
    "departmentName": "Engineering",
    "month": 3,
    "year": 2026,
    "monthYear": "March 2026",
    "basicSalary": 50000,
    "hra": 20000,
    "transportAllowance": 5000,
    "medicalAllowance": 3000,
    "otherAllowances": 2000,
    "grossEarnings": 80000,
    "pfDeduction": 6000,
    "taxDeduction": 7400,
    "otherDeductions": 0,
    "totalDeductions": 13400,
    "netSalary": 66600,
    "workingDays": 22,
    "presentDays": 20,
    "leaveDays": 2,
    "lopDays": 0,
    "status": "Generated"
  }
}
```

### Mark as Paid
```bash
PATCH /api/v1/payroll/25/mark-paid
Authorization: Bearer <token>

# Response 200:
{
  "success": true,
  "message": "Payroll marked as paid.",
  "data": {
    "id": 25,
    "status": "Paid",
    "paidOn": "2026-03-31T00:00:00Z",
    ...
  }
}
```

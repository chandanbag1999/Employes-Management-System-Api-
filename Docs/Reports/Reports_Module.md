# Reports Module

**Status:** Fully Implemented ✅

---

## 1. Module Overview

This module provides:
- Attendance reports by month with department filter
- Payroll reports by month with department filter
- Headcount reports by department with demographic breakdown

### Related Files

| Layer | File | Purpose |
|-------|------|---------|
| **Application** | `src/EMS.Application/Modules/Reports/DTOs/AttendanceReportDto.cs` | Attendance report DTO |
| **Application** | `src/EMS.Application/Modules/Reports/DTOs/PayrollReportDto.cs` | Payroll report DTO |
| **Application** | `src/EMS.Application/Modules/Reports/DTOs/HeadcountReportDto.cs` | Headcount report DTO |
| **Application** | `src/EMS.Application/Modules/Reports/Interfaces/IReportService.cs` | Service interface |
| **Infrastructure** | `src/EMS.Infrastructure/Repositories/ReportRepository.cs` | Data access |
| **API** | `src/EMS.API/Controllers/v1/ReportsController.cs` | Endpoints |

---

## 2. DTOs

### 2.1 AttendanceReportDto
```csharp
// src/EMS.Application/Modules/Reports/DTOs/AttendanceReportDto.cs
namespace EMS.Application.Modules.Reports.DTOs;

public class AttendanceReportDto
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public int TotalWorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int HalfDays { get; set; }
    public int LeaveDays { get; set; }
    public double TotalHours { get; set; }
    public double AttendancePercent =>
        TotalWorkingDays > 0
            ? Math.Round((double)PresentDays / TotalWorkingDays * 100, 1)
            : 0;
}
```

### 2.2 PayrollReportDto
```csharp
// src/EMS.Application/Modules/Reports/DTOs/PayrollReportDto.cs
namespace EMS.Application.Modules.Reports.DTOs;

public class PayrollReportDto
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal GrossEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public string Status { get; set; } = string.Empty;
}
```

### 2.3 HeadcountReportDto
```csharp
// src/EMS.Application/Modules/Reports/DTOs/HeadcountReportDto.cs
namespace EMS.Application.Modules.Reports.DTOs;

public class HeadcountReportDto
{
    public string DepartmentName { get; set; } = string.Empty;
    public string? DepartmentCode { get; set; }
    public int TotalEmployees { get; set; }
    public int Active { get; set; }
    public int OnProbation { get; set; }
    public int Resigned { get; set; }
    public int Terminated { get; set; }
    public int MaleCount { get; set; }
    public int FemaleCount { get; set; }
}
```

---

## 3. Interface

### IReportService
```csharp
// src/EMS.Application/Modules/Reports/Interfaces/IReportService.cs
using EMS.Application.Modules.Reports.DTOs;

public interface IReportService
{
    Task<IEnumerable<AttendanceReportDto>> GetAttendanceReportAsync(
        int month, int year, int? departmentId);
    Task<IEnumerable<PayrollReportDto>> GetPayrollReportAsync(
        int month, int year, int? departmentId);
    Task<IEnumerable<HeadcountReportDto>> GetHeadcountReportAsync();
}
```

---

## 4. API Controller

### ReportsController
```csharp
// src/EMS.API/Controllers/v1/ReportsController.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Reports.DTOs;
using EMS.Application.Modules.Reports.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "SuperAdmin,HRAdmin")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;

    public ReportsController(IReportService service)
    {
        _service = service;
    }

    // GET api/v1/reports/attendance?month=3&year=2026&departmentId=1
    [HttpGet("attendance")]
    public async Task<IActionResult> GetAttendanceReport(
        [FromQuery] int month,
        [FromQuery] int year,
        [FromQuery] int? departmentId = null)
    {
        if (month < 1 || month > 12)
            return BadRequest(ApiResponse<string>.Fail("Invalid month."));

        var result = await _service.GetAttendanceReportAsync(
            month, year, departmentId);
        return Ok(ApiResponse<IEnumerable<AttendanceReportDto>>.Ok(result));
    }

    // GET api/v1/reports/payroll?month=3&year=2026
    [HttpGet("payroll")]
    public async Task<IActionResult> GetPayrollReport(
        [FromQuery] int month,
        [FromQuery] int year,
        [FromQuery] int? departmentId = null)
    {
        if (month < 1 || month > 12)
            return BadRequest(ApiResponse<string>.Fail("Invalid month."));

        var result = await _service.GetPayrollReportAsync(
            month, year, departmentId);
        return Ok(ApiResponse<IEnumerable<PayrollReportDto>>.Ok(result));
    }

    // GET api/v1/reports/headcount
    [HttpGet("headcount")]
    public async Task<IActionResult> GetHeadcountReport()
    {
        var result = await _service.GetHeadcountReportAsync();
        return Ok(ApiResponse<IEnumerable<HeadcountReportDto>>.Ok(result));
    }
}
```

---

## 5. API Endpoints Summary

| Method | Endpoint | Authorization | Description |
|--------|----------|---------------|-------------|
| GET | `/api/v1/reports/attendance` | SuperAdmin, HRAdmin | Get attendance report |
| GET | `/api/v1/reports/payroll` | SuperAdmin, HRAdmin | Get payroll report |
| GET | `/api/v1/reports/headcount` | SuperAdmin, HRAdmin | Get headcount report |

---

## 6. Features

### 6.1 Attendance Report
- Monthly attendance data per employee
- Working days, present days, absent days, half days, leave days
- Total hours worked
- Attendance percentage calculation
- Filter by department

### 6.2 Payroll Report
- Monthly payroll data per employee
- Gross earnings, deductions, net salary
- Employee code, name, department, designation
- Payroll status (Generated/Paid)
- Filter by department

### 6.3 Headcount Report
- Department-wise employee breakdown
- Status distribution: Active, OnProbation, Resigned, Terminated
- Gender distribution: Male, Female counts

---

## 7. API Usage Examples

### Get Attendance Report
```bash
GET /api/v1/reports/attendance?month=3&year=2026&departmentId=1
Authorization: Bearer <token>

# Response 200:
{
  "success": true,
  "data": [
    {
      "employeeCode": "EMP001",
      "employeeName": "John Doe",
      "departmentName": "Engineering",
      "month": 3,
      "year": 2026,
      "totalWorkingDays": 22,
      "presentDays": 20,
      "absentDays": 0,
      "halfDays": 2,
      "leaveDays": 2,
      "totalHours": 176.5,
      "attendancePercent": 90.9
    },
    {
      "employeeCode": "EMP002",
      "employeeName": "Jane Smith",
      "departmentName": "Engineering",
      "month": 3,
      "year": 2026,
      "totalWorkingDays": 22,
      "presentDays": 22,
      "absentDays": 0,
      "halfDays": 0,
      "leaveDays": 0,
      "totalHours": 176.0,
      "attendancePercent": 100.0
    }
  ]
}
```

### Get Payroll Report
```bash
GET /api/v1/reports/payroll?month=3&year=2026
Authorization: Bearer <token>

# Response 200:
{
  "success": true,
  "data": [
    {
      "employeeCode": "EMP001",
      "employeeName": "John Doe",
      "departmentName": "Engineering",
      "designation": "Senior Software Engineer",
      "month": 3,
      "year": 2026,
      "grossEarnings": 78000,
      "totalDeductions": 13800,
      "netSalary": 64200,
      "status": "Paid"
    },
    {
      "employeeCode": "EMP002",
      "employeeName": "Jane Smith",
      "departmentName": "Engineering",
      "designation": "Software Engineer",
      "month": 3,
      "year": 2026,
      "grossEarnings": 65000,
      "totalDeductions": 11500,
      "netSalary": 53500,
      "status": "Generated"
    }
  ]
}
```

### Get Headcount Report
```bash
GET /api/v1/reports/headcount
Authorization: Bearer <token>

# Response 200:
{
  "success": true,
  "data": [
    {
      "departmentName": "Engineering",
      "departmentCode": "ENG",
      "totalEmployees": 50,
      "active": 48,
      "onProbation": 5,
      "resigned": 2,
      "terminated": 0,
      "maleCount": 35,
      "femaleCount": 15
    },
    {
      "departmentName": "Human Resources",
      "departmentCode": "HR",
      "totalEmployees": 10,
      "active": 10,
      "onProbation": 1,
      "resigned": 0,
      "terminated": 0,
      "maleCount": 3,
      "femaleCount": 7
    },
    {
      "departmentName": "Sales",
      "departmentCode": "SLS",
      "totalEmployees": 25,
      "active": 23,
      "onProbation": 4,
      "resigned": 1,
      "terminated": 1,
      "maleCount": 15,
      "femaleCount": 10
    }
  ]
}
```

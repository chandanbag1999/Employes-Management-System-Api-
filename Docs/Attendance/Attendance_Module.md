# Attendance Module

**Status:** Fully Implemented ✅

---

## 1. Module Overview

This module handles:
- Employee clock-in and clock-out
- Working hours calculation
- Attendance status tracking
- Monthly attendance summaries
- Manual attendance entry (admin only)

### Related Files

| Layer | File | Purpose |
|-------|------|---------|
| **Domain** | `src/EMS.Domain/Entities/Attendance/AttendanceRecord.cs` | Attendance entity |
| **Domain** | `src/EMS.Domain/Enums/AttendanceStatus.cs` | Attendance status enum |
| **Application** | `src/EMS.Application/Modules/Attendance/DTOs/ClockInDto.cs` | Clock-in request DTO |
| **Application** | `src/EMS.Application/Modules/Attendance/DTOs/ClockOutDto.cs` | Clock-out request DTO |
| **Application** | `src/EMS.Application/Modules/Attendance/DTOs/AttendanceFilterDto.cs` | Filter DTO |
| **Application** | `src/EMS.Application/Modules/Attendance/DTOs/AttendanceResponseDto.cs` | Response DTO |
| **Application** | `src/EMS.Application/Modules/Attendance/DTOs/ManualAttendanceDto.cs` | Manual entry DTO |
| **Application** | `src/EMS.Application/Modules/Attendance/DTOs/MonthlyAttendanceSummaryDto.cs` | Monthly summary DTO |
| **Application** | `src/EMS.Application/Modules/Attendance/Interfaces/IAttendanceService.cs` | Service interface |
| **Application** | `src/EMS.Application/Modules/Attendance/Interfaces/IAttendanceRepository.cs` | Repository interface |
| **Application** | `src/EMS.Application/Modules/Attendance/Services/AttendanceService.cs` | Business logic |
| **Infrastructure** | `src/EMS.Infrastructure/Repositories/AttendanceRepository.cs` | Data access |
| **API** | `src/EMS.API/Controllers/v1/AttendanceController.cs` | Endpoints |

---

## 2. Enum

### AttendanceStatus
```csharp
// src/EMS.Domain/Enums/AttendanceStatus.cs
public enum AttendanceStatus
{
    Present = 1,
    Absent = 2,
    HalfDay = 3,
    Holiday = 4,
    WeekOff = 5,
    OnLeave = 6
}
```

---

## 3. Entity

### AttendanceRecord
```csharp
// src/EMS.Domain/Entities/Attendance/AttendanceRecord.cs
using EMS.Domain.Common;
using EMS.Domain.Enums;
using EMS.Domain.Entities.Employee;

public class AttendanceRecord : BaseEntity
{
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan? ClockIn { get; set; }
    public TimeSpan? ClockOut { get; set; }
    public AttendanceStatus Status { get; set; }
    public double? WorkingHours { get; set; }
    public string? Remarks { get; set; }

    // Navigation
    public EmployeeProfile? Employee { get; set; }
}
```

---

## 4. DTOs

### 4.1 ClockInDto
```csharp
// src/EMS.Application/Modules/Attendance/DTOs/ClockInDto.cs
using System.ComponentModel.DataAnnotations;

public class ClockInDto
{
    [Required]
    public int EmployeeId { get; set; }

    // Optional — agar manually time dena ho (admin ke liye)
    public DateTime? ClockInTime { get; set; }

    [MaxLength(200)]
    public string? Remarks { get; set; }
}
```

### 4.2 ClockOutDto
```csharp
// src/EMS.Application/Modules/Attendance/DTOs/ClockOutDto.cs
using System.ComponentModel.DataAnnotations;

public class ClockOutDto
{
    [Required]
    public int EmployeeId { get; set; }

    public DateTime? ClockOutTime { get; set; }

    [MaxLength(200)]
    public string? Remarks { get; set; }
}
```

### 4.3 AttendanceFilterDto
```csharp
// src/EMS.Application/Modules/Attendance/DTOs/AttendanceFilterDto.cs
public class AttendanceFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 31; // 1 month default
    public int? EmployeeId { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Status { get; set; }  // "Present", "Absent", etc.
}
```

### 4.4 AttendanceResponseDto
```csharp
// src/EMS.Application/Modules/Attendance/DTOs/AttendanceResponseDto.cs
public class AttendanceResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan? ClockIn { get; set; }
    public TimeSpan? ClockOut { get; set; }
    public string Status { get; set; } = string.Empty;
    public double? WorkingHours { get; set; }
    public string? Remarks { get; set; }

    // Computed display fields
    public string ClockInDisplay => ClockIn.HasValue
        ? DateTime.Today.Add(ClockIn.Value).ToString("hh:mm tt")
        : "--";

    public string ClockOutDisplay => ClockOut.HasValue
        ? DateTime.Today.Add(ClockOut.Value).ToString("hh:mm tt")
        : "--";

    public string WorkingHoursDisplay => WorkingHours.HasValue
        ? $"{(int)WorkingHours}h {(int)((WorkingHours.Value % 1) * 60)}m"
        : "--";
}
```

### 4.5 ManualAttendanceDto
```csharp
// src/EMS.Application/Modules/Attendance/DTOs/ManualAttendanceDto.cs
using System.ComponentModel.DataAnnotations;
using EMS.Domain.Enums;

// Admin use kare — manually attendance mark karne ke liye
public class ManualAttendanceDto
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public AttendanceStatus Status { get; set; }

    public TimeSpan? ClockIn { get; set; }
    public TimeSpan? ClockOut { get; set; }

    [MaxLength(200)]
    public string? Remarks { get; set; }
}
```

### 4.6 MonthlyAttendanceSummaryDto
```csharp
// src/EMS.Application/Modules/Attendance/DTOs/MonthlyAttendanceSummaryDto.cs
public class MonthlyAttendanceSummaryDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public int TotalWorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int HalfDays { get; set; }
    public int LeaveDays { get; set; }
    public int HolidayDays { get; set; }
    public double TotalWorkingHours { get; set; }
    public double AverageWorkingHours =>
        PresentDays > 0
            ? Math.Round(TotalWorkingHours / PresentDays, 2)
            : 0;
}
```

---

## 5. Interfaces

### 5.1 IAttendanceService
```csharp
// src/EMS.Application/Modules/Attendance/Interfaces/IAttendanceService.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Attendance.DTOs;

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
```

### 5.2 IAttendanceRepository
```csharp
// src/EMS.Application/Modules/Attendance/Interfaces/IAttendanceRepository.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Attendance.DTOs;
using EMS.Domain.Entities.Attendance;

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
```

---

## 6. Service Implementation

### AttendanceService
```csharp
// src/EMS.Application/Modules/Attendance/Services/AttendanceService.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Attendance.DTOs;
using EMS.Application.Modules.Attendance.Interfaces;
using EMS.Domain.Entities.Attendance;
using EMS.Domain.Enums;

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
```

---

## 7. Repository Implementation

### AttendanceRepository
```csharp
// src/EMS.Infrastructure/Repositories/AttendanceRepository.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Attendance.DTOs;
using EMS.Application.Modules.Attendance.Interfaces;
using EMS.Domain.Entities.Attendance;
using EMS.Domain.Enums;
using EMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly AppDbContext _context;

    public AttendanceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<AttendanceRecord>> GetAllAsync(
        AttendanceFilterDto filter)
    {
        var query = _context.AttendanceRecords
            .Include(a => a.Employee)
            .ThenInclude(e => e!.Department)
            .AsQueryable();

        if (filter.EmployeeId.HasValue)
            query = query.Where(a => a.EmployeeId == filter.EmployeeId);

        if (filter.DepartmentId.HasValue)
            query = query.Where(a => a.Employee!.DepartmentId == filter.DepartmentId);

        if (filter.FromDate.HasValue)
            query = query.Where(a => a.Date >= filter.FromDate.Value.Date);

        if (filter.ToDate.HasValue)
            query = query.Where(a => a.Date <= filter.ToDate.Value.Date);

        if (!string.IsNullOrWhiteSpace(filter.Status) &&
            Enum.TryParse<AttendanceStatus>(filter.Status, true, out var status))
            query = query.Where(a => a.Status == status);

        var total = await query.CountAsync();

        var data = await query
            .OrderByDescending(a => a.Date)
            .ThenBy(a => a.Employee!.EmployeeCode)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PaginatedResult<AttendanceRecord>
        {
            Data = data,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<AttendanceRecord?> GetByIdAsync(int id)
        => await _context.AttendanceRecords
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<AttendanceRecord?> GetTodayRecordAsync(int employeeId)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.AttendanceRecords
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a =>
                a.EmployeeId == employeeId &&
                a.Date == today);
    }

    public async Task<IEnumerable<AttendanceRecord>> GetMonthlyAsync(
        int employeeId, int month, int year)
        => await _context.AttendanceRecords
            .Include(a => a.Employee)
            .Where(a =>
                a.EmployeeId == employeeId &&
                a.Date.Month == month &&
                a.Date.Year == year)
            .OrderBy(a => a.Date)
            .ToListAsync();

    public async Task<AttendanceRecord> CreateAsync(AttendanceRecord record)
    {
        await _context.AttendanceRecords.AddAsync(record);
        await _context.SaveChangesAsync();
        return (await GetByIdAsync(record.Id))!;
    }

    public async Task<AttendanceRecord?> UpdateAsync(int id, AttendanceRecord record)
    {
        var existing = await _context.AttendanceRecords.FindAsync(id);
        if (existing == null) return null;

        existing.ClockOut = record.ClockOut;
        existing.WorkingHours = record.WorkingHours;
        existing.Status = record.Status;
        existing.Remarks = record.Remarks;
        existing.UpdatedAt = record.UpdatedAt;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> HasClockedInTodayAsync(int employeeId)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.AttendanceRecords
            .AnyAsync(a =>
                a.EmployeeId == employeeId &&
                a.Date == today &&
                a.ClockIn.HasValue);
    }

    public async Task<bool> HasClockedOutTodayAsync(int employeeId)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.AttendanceRecords
            .AnyAsync(a =>
                a.EmployeeId == employeeId &&
                a.Date == today &&
                a.ClockOut.HasValue);
    }
}
```

---

## 8. API Controller

### AttendanceController
```csharp
// src/EMS.API/Controllers/v1/AttendanceController.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Attendance.DTOs;
using EMS.Application.Modules.Attendance.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _service;

    public AttendanceController(IAttendanceService service)
    {
        _service = service;
    }

    // POST api/v1/attendance/clock-in
    [HttpPost("clock-in")]
    public async Task<IActionResult> ClockIn([FromBody] ClockInDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (result, error) = await _service.ClockInAsync(dto);
        if (error != null)
            return BadRequest(ApiResponse<string>.Fail(error));

        return Ok(ApiResponse<AttendanceResponseDto>.Ok(result!, "Clocked in successfully."));
    }

    // POST api/v1/attendance/clock-out
    [HttpPost("clock-out")]
    public async Task<IActionResult> ClockOut([FromBody] ClockOutDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (result, error) = await _service.ClockOutAsync(dto);
        if (error != null)
            return BadRequest(ApiResponse<string>.Fail(error));

        return Ok(ApiResponse<AttendanceResponseDto>.Ok(result!, "Clocked out successfully."));
    }

    // GET api/v1/attendance?employeeId=1&fromDate=2026-03-01&toDate=2026-03-31
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,HRAdmin,Manager")]
    public async Task<IActionResult> GetAll([FromQuery] AttendanceFilterDto filter)
    {
        var result = await _service.GetAllAsync(filter);
        return Ok(ApiResponse<PaginatedResult<AttendanceResponseDto>>.Ok(result));
    }

    // GET api/v1/attendance/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("Record not found."));
        return Ok(ApiResponse<AttendanceResponseDto>.Ok(result));
    }

    // GET api/v1/attendance/today/5
    [HttpGet("today/{employeeId}")]
    public async Task<IActionResult> GetToday(int employeeId)
    {
        var result = await _service.GetTodayRecordAsync(employeeId);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("No attendance record for today."));
        return Ok(ApiResponse<AttendanceResponseDto>.Ok(result));
    }

    // GET api/v1/attendance/summary/5?month=3&year=2026
    [HttpGet("summary/{employeeId}")]
    public async Task<IActionResult> GetMonthlySummary(
        int employeeId,
        [FromQuery] int month,
        [FromQuery] int year)
    {
        if (month < 1 || month > 12)
            return BadRequest(ApiResponse<string>.Fail("Invalid month."));

        var result = await _service.GetMonthlySummaryAsync(employeeId, month, year);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("No records found."));

        return Ok(ApiResponse<MonthlyAttendanceSummaryDto>.Ok(result));
    }

    // POST api/v1/attendance/manual  (Admin only)
    [HttpPost("manual")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> MarkManual([FromBody] ManualAttendanceDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (result, error) = await _service.MarkManualAsync(dto);
        if (error != null)
            return BadRequest(ApiResponse<string>.Fail(error));

        return Ok(ApiResponse<AttendanceResponseDto>.Ok(
            result!, "Attendance marked manually."));
    }
}
```

---

## 9. API Endpoints Summary

| Method | Endpoint | Authorization | Description |
|--------|----------|---------------|-------------|
| POST | `/api/v1/attendance/clock-in` | Authenticated | Employee clock-in |
| POST | `/api/v1/attendance/clock-out` | Authenticated | Employee clock-out |
| GET | `/api/v1/attendance` | SuperAdmin, HRAdmin, Manager | List attendance records |
| GET | `/api/v1/attendance/{id}` | Authenticated | Get single record |
| GET | `/api/v1/attendance/today/{employeeId}` | Authenticated | Get today's record |
| GET | `/api/v1/attendance/summary/{employeeId}` | Authenticated | Monthly summary |
| POST | `/api/v1/attendance/manual` | SuperAdmin, HRAdmin | Manual attendance entry |

---

## 10. Features

### 10.1 Clock-In/Clock-Out
- Prevents duplicate clock-in for same day
- Prevents duplicate clock-out for same day
- Working hours calculated automatically

### 10.2 Half-Day Detection
- If working hours < 4, status = HalfDay
- If working hours >= 4, status = Present

### 10.3 Manual Attendance
- Admin can mark attendance manually
- Supports any status (Present, Absent, Holiday, etc.)
- Calculates working hours if both times provided

### 10.4 Monthly Summary
- Counts Present, Absent, HalfDay, Leave, Holiday
- Calculates total and average working hours

---

## 11. API Usage Examples

### Clock In
```bash
POST /api/v1/attendance/clock-in
Authorization: Bearer <token>
Content-Type: application/json

{
  "employeeId": 5
}

# Response 200:
{
  "success": true,
  "message": "Clocked in successfully.",
  "data": {
    "id": 123,
    "employeeId": 5,
    "employeeName": "John Doe",
    "date": "2026-03-30T00:00:00Z",
    "clockIn": "10:30:00",
    "status": "Present",
    ...
  }
}
```

### Clock Out
```bash
POST /api/v1/attendance/clock-out
Authorization: Bearer <token>
Content-Type: application/json

{
  "employeeId": 5
}

# Response 200:
{
  "success": true,
  "message": "Clocked out successfully.",
  "data": {
    "id": 123,
    "employeeId": 5,
    "clockIn": "10:30:00",
    "clockOut": "19:45:00",
    "workingHours": 9.25,
    "workingHoursDisplay": "9h 15m",
    "status": "Present"
  }
}
```

### Get Monthly Summary
```bash
GET /api/v1/attendance/summary/5?month=3&year=2026
Authorization: Bearer <token>

# Response 200:
{
  "success": true,
  "data": {
    "employeeId": 5,
    "employeeName": "John Doe",
    "month": 3,
    "year": 2026,
    "totalWorkingDays": 22,
    "presentDays": 20,
    "absentDays": 1,
    "halfDays": 1,
    "leaveDays": 0,
    "holidayDays": 0,
    "totalWorkingHours": 176.5,
    "averageWorkingHours": 8.83
  }
}
```

### Manual Attendance Entry
```bash
POST /api/v1/attendance/manual
Authorization: Bearer <token>
Content-Type: application/json

{
  "employeeId": 5,
  "date": "2026-03-25",
  "status": 4,  // Holiday
  "remarks": "Company holiday"
}

# Response 200:
{
  "success": true,
  "message": "Attendance marked manually.",
  "data": { ... }
}
```

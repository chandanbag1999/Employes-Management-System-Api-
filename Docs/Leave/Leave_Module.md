# Leave Module

**Status:** Fully Implemented ✅

---

## 1. Module Overview

This module handles:
- Leave application submission
- Leave balance tracking
- Leave approval/rejection workflow
- Leave cancellation
- Leave type management

### Related Files

| Layer | File | Purpose |
|-------|------|---------|
| **Domain** | `src/EMS.Domain/Entities/Leave/LeaveApplication.cs` | Leave application entity |
| **Domain** | `src/EMS.Domain/Entities/Leave/LeaveType.cs` | Leave type entity |
| **Domain** | `src/EMS.Domain/Enums/LeaveStatus.cs` | Leave status enum |
| **Application** | `src/EMS.Application/Modules/Leave/DTOs/ApplyLeaveDto.cs` | Apply leave DTO |
| **Application** | `src/EMS.Application/Modules/Leave/DTOs/LeaveActionDto.cs` | Approve/reject DTO |
| **Application** | `src/EMS.Application/Modules/Leave/DTOs/LeaveFilterDto.cs` | Filter DTO |
| **Application** | `src/EMS.Application/Modules/Leave/DTOs/LeaveResponseDto.cs` | Response DTO |
| **Application** | `src/EMS.Application/Modules/Leave/DTOs/LeaveBalanceDto.cs` | Balance DTO |
| **Application** | `src/EMS.Application/Modules/Leave/DTOs/CreateLeaveTypeDto.cs` | Create leave type DTO |
| **Application** | `src/EMS.Application/Modules/Leave/DTOs/LeaveTypeResponseDto.cs` | Leave type response DTO |
| **Application** | `src/EMS.Application/Modules/Leave/Interfaces/ILeaveService.cs` | Service interface |
| **Application** | `src/EMS.Application/Modules/Leave/Interfaces/ILeaveRepository.cs` | Repository interface |
| **Application** | `src/EMS.Application/Modules/Leave/Services/LeaveService.cs` | Business logic |
| **Infrastructure** | `src/EMS.Infrastructure/Repositories/LeaveRepository.cs` | Data access |
| **API** | `src/EMS.API/Controllers/v1/LeaveController.cs` | Endpoints |

---

## 2. Enums

### LeaveStatus
```csharp
// src/EMS.Domain/Enums/LeaveStatus.cs
public enum LeaveStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4
}
```

---

## 3. Entities

### 3.1 LeaveApplication
```csharp
// src/EMS.Domain/Entities/Leave/LeaveApplication.cs
using EMS.Domain.Common;
using EMS.Domain.Enums;
using EMS.Domain.Entities.Employee;

public class LeaveApplication : BaseEntity
{
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public int? ApprovedById { get; set; }
    public string? RejectionReason { get; set; }

    // Navigation
    public LeaveType LeaveType { get; set; } = null!;
    public EmployeeProfile? Employee { get; set; }
    public EmployeeProfile? ApprovedBy { get; set; }
}
```

### 3.2 LeaveType
```csharp
// src/EMS.Domain/Entities/Leave/LeaveType.cs
using EMS.Domain.Common;

public class LeaveType : BaseEntity
{
    public string Name { get; set; } = string.Empty;   // "Casual", "Sick", "Earned"
    public int MaxDaysPerYear { get; set; }
    public bool IsCarryForwardAllowed { get; set; }
    public bool IsPaid { get; set; } = true;
}
```

---

## 4. DTOs

### 4.1 ApplyLeaveDto
```csharp
// src/EMS.Application/Modules/Leave/DTOs/ApplyLeaveDto.cs
using System.ComponentModel.DataAnnotations;

public class ApplyLeaveDto
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public int LeaveTypeId { get; set; }

    [Required]
    public DateTime FromDate { get; set; }

    [Required]
    public DateTime ToDate { get; set; }

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}
```

### 4.2 LeaveActionDto
```csharp
// src/EMS.Application/Modules/Leave/DTOs/LeaveActionDto.cs
using System.ComponentModel.DataAnnotations;

// Manager/HR approve ya reject karne ke liye
public class LeaveActionDto
{
    [Required]
    public int ActionById { get; set; }  // Manager/HR ka EmployeeId

    [Required]
    public bool IsApproved { get; set; }

    [MaxLength(500)]
    public string? RejectionReason { get; set; }
}
```

### 4.3 LeaveFilterDto
```csharp
// src/EMS.Application/Modules/Leave/DTOs/LeaveFilterDto.cs
public class LeaveFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? EmployeeId { get; set; }
    public int? DepartmentId { get; set; }
    public int? LeaveTypeId { get; set; }
    public string? Status { get; set; }   // "Pending","Approved","Rejected"
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
```

### 4.4 LeaveResponseDto
```csharp
// src/EMS.Application/Modules/Leave/DTOs/LeaveResponseDto.cs
public class LeaveResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? ApprovedById { get; set; }
    public string? ApprovedByName { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### 4.5 LeaveBalanceDto
```csharp
// src/EMS.Application/Modules/Leave/DTOs/LeaveBalanceDto.cs
public class LeaveBalanceDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public List<LeaveBalanceItemDto> Balances { get; set; } = new();
}

public class LeaveBalanceItemDto
{
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public int TotalAllowed { get; set; }
    public int Used { get; set; }
    public int Pending { get; set; }
    public int Remaining => TotalAllowed - Used - Pending;
}
```

### 4.6 CreateLeaveTypeDto
```csharp
// src/EMS.Application/Modules/Leave/DTOs/CreateLeaveTypeDto.cs
using System.ComponentModel.DataAnnotations;

public class CreateLeaveTypeDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(1, 365)]
    public int MaxDaysPerYear { get; set; }

    public bool IsCarryForwardAllowed { get; set; } = false;
    public bool IsPaid { get; set; } = true;
}
```

### 4.7 LeaveTypeResponseDto
```csharp
// src/EMS.Application/Modules/Leave/DTOs/LeaveTypeResponseDto.cs
public class LeaveTypeResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MaxDaysPerYear { get; set; }
    public bool IsCarryForwardAllowed { get; set; }
    public bool IsPaid { get; set; }
}
```

---

## 5. Interfaces

### 5.1 ILeaveService
```csharp
// src/EMS.Application/Modules/Leave/Interfaces/ILeaveService.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Leave.DTOs;

public interface ILeaveService
{
    // Leave Applications
    Task<(LeaveResponseDto? result, string? error)> ApplyAsync(ApplyLeaveDto dto);
    Task<(LeaveResponseDto? result, string? error)> ApproveOrRejectAsync(
        int id, LeaveActionDto dto);
    Task<(bool success, string? error)> CancelAsync(int id, int employeeId);
    Task<PaginatedResult<LeaveResponseDto>> GetAllAsync(LeaveFilterDto filter);
    Task<LeaveResponseDto?> GetByIdAsync(int id);

    // Leave Balance
    Task<LeaveBalanceDto?> GetBalanceAsync(int employeeId, int year);

    // Leave Types
    Task<IEnumerable<LeaveTypeResponseDto>> GetAllLeaveTypesAsync();
    Task<LeaveTypeResponseDto> CreateLeaveTypeAsync(CreateLeaveTypeDto dto);
}
```

### 5.2 ILeaveRepository
```csharp
// src/EMS.Application/Modules/Leave/Interfaces/ILeaveRepository.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Leave.DTOs;
using EMS.Domain.Entities.Leave;
using EMS.Domain.Enums;

public interface ILeaveRepository
{
    Task<PaginatedResult<LeaveApplication>> GetAllAsync(LeaveFilterDto filter);
    Task<LeaveApplication?> GetByIdAsync(int id);
    Task<LeaveApplication> CreateAsync(LeaveApplication application);
    Task<LeaveApplication?> UpdateStatusAsync(int id, LeaveStatus status,
        int? approvedById, string? rejectionReason);
    Task<LeaveApplication?> CancelAsync(int id);
    Task<int> GetUsedDaysAsync(int employeeId, int leaveTypeId, int year);
    Task<int> GetPendingDaysAsync(int employeeId, int leaveTypeId, int year);
    Task<bool> HasOverlappingLeaveAsync(int employeeId, DateTime from, DateTime to,
        int? excludeId = null);

    // Leave Types
    Task<IEnumerable<LeaveType>> GetAllLeaveTypesAsync();
    Task<LeaveType?> GetLeaveTypeByIdAsync(int id);
    Task<LeaveType> CreateLeaveTypeAsync(LeaveType leaveType);
}
```

---

## 6. Service Implementation

### LeaveService
```csharp
// src/EMS.Application/Modules/Leave/Services/LeaveService.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Leave.DTOs;
using EMS.Application.Modules.Leave.Interfaces;
using EMS.Domain.Entities.Leave;
using EMS.Domain.Enums;

public class LeaveService : ILeaveService
{
    private readonly ILeaveRepository _leaveRepo;

    public LeaveService(ILeaveRepository leaveRepo)
    {
        _leaveRepo = leaveRepo;
    }

    public async Task<(LeaveResponseDto? result, string? error)> ApplyAsync(
        ApplyLeaveDto dto)
    {
        // Date validation
        var from = dto.FromDate.Date;
        var to = dto.ToDate.Date;

        if (from > to)
            return (null, "From date cannot be after To date.");

        if (from < DateTime.Today)
            return (null, "Cannot apply leave for past dates.");

        // Total days calculate — weekends count bhi hote hain basic mein
        var totalDays = (int)(to - from).TotalDays + 1;

        // Overlapping leave check
        if (await _leaveRepo.HasOverlappingLeaveAsync(dto.EmployeeId, from, to))
            return (null, "You already have a leave application for these dates.");

        // Leave type exist karta hai?
        var leaveType = await _leaveRepo.GetLeaveTypeByIdAsync(dto.LeaveTypeId);
        if (leaveType == null)
            return (null, "Invalid leave type.");

        // Balance check
        var year = from.Year;
        var usedDays = await _leaveRepo.GetUsedDaysAsync(
            dto.EmployeeId, dto.LeaveTypeId, year);
        var pendingDays = await _leaveRepo.GetPendingDaysAsync(
            dto.EmployeeId, dto.LeaveTypeId, year);

        var remaining = leaveType.MaxDaysPerYear - usedDays - pendingDays;
        if (totalDays > remaining)
            return (null,
                $"Insufficient leave balance. Remaining: {remaining} days.");

        var application = new LeaveApplication
        {
            EmployeeId = dto.EmployeeId,
            LeaveTypeId = dto.LeaveTypeId,
            FromDate = DateTime.SpecifyKind(from, DateTimeKind.Utc),
            ToDate = DateTime.SpecifyKind(to, DateTimeKind.Utc),
            TotalDays = totalDays,
            Reason = dto.Reason.Trim(),
            Status = LeaveStatus.Pending
        };

        var created = await _leaveRepo.CreateAsync(application);
        return (MapToDto(created), null);
    }

    public async Task<(LeaveResponseDto? result, string? error)> ApproveOrRejectAsync(
        int id, LeaveActionDto dto)
    {
        var application = await _leaveRepo.GetByIdAsync(id);
        if (application == null)
            return (null, "Leave application not found.");

        if (application.Status != LeaveStatus.Pending)
            return (null, "Only pending applications can be approved/rejected.");

        if (!dto.IsApproved && string.IsNullOrWhiteSpace(dto.RejectionReason))
            return (null, "Rejection reason is required.");

        var newStatus = dto.IsApproved
            ? LeaveStatus.Approved
            : LeaveStatus.Rejected;

        var updated = await _leaveRepo.UpdateStatusAsync(
            id, newStatus, dto.ActionById, dto.RejectionReason);

        return updated == null
            ? (null, "Action failed.")
            : (MapToDto(updated), null);
    }

    public async Task<(bool success, string? error)> CancelAsync(
        int id, int employeeId)
    {
        var application = await _leaveRepo.GetByIdAsync(id);
        if (application == null)
            return (false, "Leave application not found.");

        if (application.EmployeeId != employeeId)
            return (false, "You can only cancel your own leave.");

        if (application.Status == LeaveStatus.Approved &&
            application.FromDate.Date <= DateTime.Today)
            return (false, "Cannot cancel leave that has already started.");

        if (application.Status == LeaveStatus.Rejected)
            return (false, "Cannot cancel a rejected leave.");

        var cancelled = await _leaveRepo.CancelAsync(id);
        return cancelled != null
            ? (true, null)
            : (false, "Cancel failed.");
    }

    public async Task<PaginatedResult<LeaveResponseDto>> GetAllAsync(
        LeaveFilterDto filter)
    {
        var result = await _leaveRepo.GetAllAsync(filter);
        return new PaginatedResult<LeaveResponseDto>
        {
            Data = result.Data.Select(MapToDto),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<LeaveResponseDto?> GetByIdAsync(int id)
    {
        var app = await _leaveRepo.GetByIdAsync(id);
        return app == null ? null : MapToDto(app);
    }

    public async Task<LeaveBalanceDto?> GetBalanceAsync(int employeeId, int year)
    {
        var leaveTypes = await _leaveRepo.GetAllLeaveTypesAsync();
        var items = new List<LeaveBalanceItemDto>();

        foreach (var lt in leaveTypes)
        {
            var used = await _leaveRepo.GetUsedDaysAsync(employeeId, lt.Id, year);
            var pending = await _leaveRepo.GetPendingDaysAsync(employeeId, lt.Id, year);

            items.Add(new LeaveBalanceItemDto
            {
                LeaveTypeId = lt.Id,
                LeaveTypeName = lt.Name,
                TotalAllowed = lt.MaxDaysPerYear,
                Used = used,
                Pending = pending
            });
        }

        return new LeaveBalanceDto
        {
            EmployeeId = employeeId,
            Year = year,
            Balances = items
        };
    }

    public async Task<IEnumerable<LeaveTypeResponseDto>> GetAllLeaveTypesAsync()
    {
        var types = await _leaveRepo.GetAllLeaveTypesAsync();
        return types.Select(MapLeaveTypeToDto);
    }

    public async Task<LeaveTypeResponseDto> CreateLeaveTypeAsync(CreateLeaveTypeDto dto)
    {
        var leaveType = new LeaveType
        {
            Name = dto.Name.Trim(),
            MaxDaysPerYear = dto.MaxDaysPerYear,
            IsCarryForwardAllowed = dto.IsCarryForwardAllowed,
            IsPaid = dto.IsPaid
        };

        var created = await _leaveRepo.CreateLeaveTypeAsync(leaveType);
        return MapLeaveTypeToDto(created);
    }

    private static LeaveResponseDto MapToDto(LeaveApplication a) => new()
    {
        Id = a.Id,
        EmployeeId = a.EmployeeId,
        EmployeeName = a.Employee != null
            ? $"{a.Employee.FirstName} {a.Employee.LastName}"
            : "Unknown",
        EmployeeCode = a.Employee?.EmployeeCode ?? "",
        DepartmentName = a.Employee?.Department?.Name ?? "",
        LeaveTypeId = a.LeaveTypeId,
        LeaveTypeName = a.LeaveType?.Name ?? "",
        FromDate = a.FromDate,
        ToDate = a.ToDate,
        TotalDays = a.TotalDays,
        Reason = a.Reason,
        Status = a.Status.ToString(),
        ApprovedById = a.ApprovedById,
        ApprovedByName = a.ApprovedBy != null
            ? $"{a.ApprovedBy.FirstName} {a.ApprovedBy.LastName}"
            : null,
        RejectionReason = a.RejectionReason,
        CreatedAt = a.CreatedAt
    };

    private static LeaveTypeResponseDto MapLeaveTypeToDto(LeaveType lt) => new()
    {
        Id = lt.Id,
        Name = lt.Name,
        MaxDaysPerYear = lt.MaxDaysPerYear,
        IsCarryForwardAllowed = lt.IsCarryForwardAllowed,
        IsPaid = lt.IsPaid
    };
}
```

---

## 7. API Controller

### LeaveController
```csharp
// src/EMS.API/Controllers/v1/LeaveController.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Leave.DTOs;
using EMS.Application.Modules.Leave.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class LeaveController : ControllerBase
{
    private readonly ILeaveService _service;

    public LeaveController(ILeaveService service)
    {
        _service = service;
    }

    // GET api/v1/leave?status=Pending&employeeId=1
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] LeaveFilterDto filter)
    {
        var result = await _service.GetAllAsync(filter);
        return Ok(ApiResponse<PaginatedResult<LeaveResponseDto>>.Ok(result));
    }

    // GET api/v1/leave/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("Leave application not found."));
        return Ok(ApiResponse<LeaveResponseDto>.Ok(result));
    }

    // POST api/v1/leave/apply
    [HttpPost("apply")]
    public async Task<IActionResult> Apply([FromBody] ApplyLeaveDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (result, error) = await _service.ApplyAsync(dto);
        if (error != null)
            return BadRequest(ApiResponse<string>.Fail(error));

        return CreatedAtAction(nameof(GetById),
            new { id = result!.Id },
            ApiResponse<LeaveResponseDto>.Ok(result, "Leave applied successfully."));
    }

    // PATCH api/v1/leave/5/action
    [HttpPatch("{id}/action")]
    [Authorize(Roles = "SuperAdmin,HRAdmin,Manager")]
    public async Task<IActionResult> ApproveOrReject(
        int id, [FromBody] LeaveActionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (result, error) = await _service.ApproveOrRejectAsync(id, dto);
        if (error != null)
            return error.Contains("not found")
                ? NotFound(ApiResponse<string>.Fail(error))
                : BadRequest(ApiResponse<string>.Fail(error));

        var msg = result!.Status == "Approved"
            ? "Leave approved." : "Leave rejected.";
        return Ok(ApiResponse<LeaveResponseDto>.Ok(result, msg));
    }

    // PATCH api/v1/leave/5/cancel
    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id, [FromQuery] int employeeId)
    {
        var (success, error) = await _service.CancelAsync(id, employeeId);
        if (!success)
            return error!.Contains("not found")
                ? NotFound(ApiResponse<string>.Fail(error))
                : BadRequest(ApiResponse<string>.Fail(error));

        return Ok(ApiResponse<string>.Ok("Cancelled", "Leave cancelled."));
    }

    // GET api/v1/leave/balance/5?year=2026
    [HttpGet("balance/{employeeId}")]
    public async Task<IActionResult> GetBalance(
        int employeeId, [FromQuery] int year = 0)
    {
        if (year == 0) year = DateTime.Today.Year;

        var result = await _service.GetBalanceAsync(employeeId, year);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("No data found."));

        return Ok(ApiResponse<LeaveBalanceDto>.Ok(result));
    }

    // GET api/v1/leave/types
    [HttpGet("types")]
    public async Task<IActionResult> GetLeaveTypes()
    {
        var result = await _service.GetAllLeaveTypesAsync();
        return Ok(ApiResponse<IEnumerable<LeaveTypeResponseDto>>.Ok(result));
    }

    // POST api/v1/leave/types
    [HttpPost("types")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> CreateLeaveType([FromBody] CreateLeaveTypeDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.CreateLeaveTypeAsync(dto);
        return Ok(ApiResponse<LeaveTypeResponseDto>.Ok(result, "Leave type created."));
    }
}
```

---

## 8. API Endpoints Summary

| Method | Endpoint | Authorization | Description |
|--------|----------|---------------|-------------|
| GET | `/api/v1/leave` | Authenticated | List leave applications |
| GET | `/api/v1/leave/{id}` | Authenticated | Get single application |
| POST | `/api/v1/leave/apply` | Authenticated | Apply for leave |
| PATCH | `/api/v1/leave/{id}/action` | SuperAdmin, HRAdmin, Manager | Approve/Reject |
| PATCH | `/api/v1/leave/{id}/cancel` | Authenticated | Cancel own leave |
| GET | `/api/v1/leave/balance/{employeeId}` | Authenticated | Get leave balance |
| GET | `/api/v1/leave/types` | Authenticated | List leave types |
| POST | `/api/v1/leave/types` | SuperAdmin, HRAdmin | Create leave type |

---

## 9. Features

### 9.1 Leave Balance Tracking
- Tracks used and pending days per leave type
- Calculates remaining balance
- Validates against MaxDaysPerYear

### 9.2 Overlap Prevention
- Prevents duplicate leave applications for same dates
- Checks for pending and approved leaves

### 9.3 Approval Workflow
- Pending → Approved/Rejected by Manager/HR
- Rejection requires reason
- Only pending applications can be actioned

### 9.4 Cancellation Rules
- Only own leaves can be cancelled
- Cannot cancel approved leaves that have started
- Cannot cancel rejected leaves

---

## 10. API Usage Examples

### Apply for Leave
```bash
POST /api/v1/leave/apply
Authorization: Bearer <token>
Content-Type: application/json

{
  "employeeId": 5,
  "leaveTypeId": 1,
  "fromDate": "2026-04-01",
  "toDate": "2026-04-05",
  "reason": "Family vacation"
}

# Response 201:
{
  "success": true,
  "message": "Leave applied successfully.",
  "data": {
    "id": 15,
    "employeeId": 5,
    "employeeName": "John Doe",
    "leaveTypeName": "Casual",
    "fromDate": "2026-04-01T00:00:00Z",
    "toDate": "2026-04-05T00:00:00Z",
    "totalDays": 5,
    "status": "Pending",
    ...
  }
}
```

### Approve Leave
```bash
PATCH /api/v1/leave/15/action
Authorization: Bearer <token>
Content-Type: application/json

{
  "actionById": 1,
  "isApproved": true
}

# Response 200:
{
  "success": true,
  "message": "Leave approved.",
  "data": { ... }
}
```

### Reject Leave
```bash
PATCH /api/v1/leave/15/action
Authorization: Bearer <token>
Content-Type: application/json

{
  "actionById": 1,
  "isApproved": false,
  "rejectionReason": "Project deadline approaching"
}

# Response 200:
{
  "success": true,
  "message": "Leave rejected.",
  "data": { ... }
}
```

### Get Leave Balance
```bash
GET /api/v1/leave/balance/5?year=2026
Authorization: Bearer <token>

# Response 200:
{
  "success": true,
  "data": {
    "employeeId": 5,
    "employeeName": "John Doe",
    "year": 2026,
    "balances": [
      {
        "leaveTypeId": 1,
        "leaveTypeName": "Casual",
        "totalAllowed": 12,
        "used": 5,
        "pending": 2,
        "remaining": 5
      },
      {
        "leaveTypeId": 2,
        "leaveTypeName": "Sick",
        "totalAllowed": 10,
        "used": 2,
        "pending": 0,
        "remaining": 8
      }
    ]
  }
}
```

### Create Leave Type
```bash
POST /api/v1/leave/types
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Earned",
  "maxDaysPerYear": 15,
  "isCarryForwardAllowed": true,
  "isPaid": true
}

# Response 200:
{
  "success": true,
  "message": "Leave type created.",
  "data": {
    "id": 4,
    "name": "Earned",
    "maxDaysPerYear": 15,
    "isCarryForwardAllowed": true,
    "isPaid": true
  }
}
```

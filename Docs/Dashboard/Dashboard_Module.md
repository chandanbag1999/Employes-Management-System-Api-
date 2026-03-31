# Dashboard Module

**Status:** Fully Implemented ✅

---

## 1. Module Overview

This module provides:
- Overall system statistics (employees, attendance, leave, payroll)
- Department-wise headcount breakdown
- Recent activities feed

### Related Files

| Layer | File | Purpose |
|-------|------|---------|
| **Application** | `src/EMS.Application/Modules/Dashboard/DTOs/DashboardStatsDto.cs` | Stats DTO |
| **Application** | `src/EMS.Application/Modules/Dashboard/DTOs/DepartmentHeadcountDto.cs` | Headcount DTO |
| **Application** | `src/EMS.Application/Modules/Dashboard/DTOs/RecentActivityDto.cs` | Activity DTO |
| **Application** | `src/EMS.Application/Modules/Dashboard/Interfaces/IDashboardService.cs` | Service interface |
| **Infrastructure** | `src/EMS.Infrastructure/Repositories/DashboardRepository.cs` | Data access |
| **API** | `src/EMS.API/Controllers/v1/DashboardController.cs` | Endpoints |

---

## 2. DTOs

### 2.1 DashboardStatsDto
```csharp
// src/EMS.Application/Modules/Dashboard/DTOs/DashboardStatsDto.cs
namespace EMS.Application.Modules.Dashboard.DTOs;

public class DashboardStatsDto
{
    // Employee Stats
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int NewJoineesThisMonth { get; set; }
    public int OnProbation { get; set; }

    // Today's Attendance
    public int PresentToday { get; set; }
    public int AbsentToday { get; set; }
    public int OnLeaveToday { get; set; }
    public double AttendancePercentToday =>
        TotalEmployees > 0
            ? Math.Round((double)PresentToday / TotalEmployees * 100, 1)
            : 0;

    // Leave Stats
    public int PendingLeaveRequests { get; set; }
    public int ApprovedLeavesThisMonth { get; set; }

    // Payroll Stats
    public decimal TotalPayrollThisMonth { get; set; }
    public int PayrollProcessedCount { get; set; }

    // Organization
    public int TotalDepartments { get; set; }
    public int TotalDesignations { get; set; }

    // Performance
    public int PendingReviews { get; set; }
    public int GoalsCompletedThisQuarter { get; set; }
}
```

### 2.2 DepartmentHeadcountDto
```csharp
// src/EMS.Application/Modules/Dashboard/DTOs/DepartmentHeadcountDto.cs
namespace EMS.Application.Modules.Dashboard.DTOs;

public class DepartmentHeadcountDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? DepartmentCode { get; set; }
    public int EmployeeCount { get; set; }
    public int ActiveCount { get; set; }
}
```

### 2.3 RecentActivityDto
```csharp
// src/EMS.Application/Modules/Dashboard/DTOs/RecentActivityDto.cs
namespace EMS.Application.Modules.Dashboard.DTOs;

public class RecentActivityDto
{
    public string Type { get; set; } = string.Empty;  // "NewEmployee","Leave","Payroll"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? EmployeeName { get; set; }
}
```

---

## 3. Interface

### IDashboardService
```csharp
// src/EMS.Application/Modules/Dashboard/Interfaces/IDashboardService.cs
using EMS.Application.Modules.Dashboard.DTOs;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync();
    Task<IEnumerable<DepartmentHeadcountDto>> GetDepartmentHeadcountAsync();
    Task<IEnumerable<RecentActivityDto>> GetRecentActivitiesAsync(int count = 10);
}
```

---

## 4. API Controller

### DashboardController
```csharp
// src/EMS.API/Controllers/v1/DashboardController.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Dashboard.DTOs;
using EMS.Application.Modules.Dashboard.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "SuperAdmin,HRAdmin,Manager")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    // GET api/v1/dashboard/stats
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _service.GetStatsAsync();
        return Ok(ApiResponse<DashboardStatsDto>.Ok(result));
    }

    // GET api/v1/dashboard/headcount
    [HttpGet("headcount")]
    public async Task<IActionResult> GetHeadcount()
    {
        var result = await _service.GetDepartmentHeadcountAsync();
        return Ok(ApiResponse<IEnumerable<DepartmentHeadcountDto>>.Ok(result));
    }

    // GET api/v1/dashboard/activities?count=10
    [HttpGet("activities")]
    public async Task<IActionResult> GetActivities([FromQuery] int count = 10)
    {
        var result = await _service.GetRecentActivitiesAsync(count);
        return Ok(ApiResponse<IEnumerable<RecentActivityDto>>.Ok(result));
    }
}
```

---

## 5. API Endpoints Summary

| Method | Endpoint | Authorization | Description |
|--------|----------|---------------|-------------|
| GET | `/api/v1/dashboard/stats` | SuperAdmin, HRAdmin, Manager | Get all stats |
| GET | `/api/v1/dashboard/headcount` | SuperAdmin, HRAdmin, Manager | Get headcount by department |
| GET | `/api/v1/dashboard/activities` | SuperAdmin, HRAdmin, Manager | Get recent activities |

---

## 6. Features

### 6.1 Dashboard Stats
- Employee counts (total, active, new joinees, on probation)
- Today's attendance breakdown
- Leave statistics (pending, approved this month)
- Payroll summary for current month
- Organization counts (departments, designations)
- Performance stats (pending reviews, completed goals)

### 6.2 Department Headcount
- List all departments with employee counts
- Shows active employee count per department
- Department code included

### 6.3 Recent Activities
- Activity feed showing latest events
- Activity types: NewEmployee, Leave, Payroll
- Includes timestamp and employee name

---

## 7. API Usage Examples

### Get Dashboard Stats
```bash
GET /api/v1/dashboard/stats
Authorization: Bearer <token>

# Response 200:
{
  "success": true,
  "data": {
    "totalEmployees": 150,
    "activeEmployees": 145,
    "newJoineesThisMonth": 5,
    "onProbation": 12,
    "presentToday": 140,
    "absentToday": 5,
    "onLeaveToday": 3,
    "attendancePercentToday": 96.6,
    "pendingLeaveRequests": 8,
    "approvedLeavesThisMonth": 45,
    "totalPayrollThisMonth": 7500000,
    "payrollProcessedCount": 145,
    "totalDepartments": 8,
    "totalDesignations": 25,
    "pendingReviews": 15,
    "goalsCompletedThisQuarter": 42
  }
}
```

### Get Department Headcount
```bash
GET /api/v1/dashboard/headcount
Authorization: Bearer <token>

# Response 200:
{
  "success": true,
  "data": [
    {
      "departmentId": 1,
      "departmentName": "Engineering",
      "departmentCode": "ENG",
      "employeeCount": 50,
      "activeCount": 48
    },
    {
      "departmentId": 2,
      "departmentName": "Human Resources",
      "departmentCode": "HR",
      "employeeCount": 10,
      "activeCount": 10
    }
  ]
}
```

### Get Recent Activities
```bash
GET /api/v1/dashboard/activities?count=10
Authorization: Bearer <token>

# Response 200:
{
  "success": true,
  "data": [
    {
      "type": "NewEmployee",
      "title": "New Joiner",
      "description": "Jane Smith joined as Software Engineer",
      "timestamp": "2026-03-30T09:00:00Z",
      "employeeName": "Jane Smith"
    },
    {
      "type": "Leave",
      "title": "Leave Approved",
      "description": "John Doe's leave request approved",
      "timestamp": "2026-03-29T14:30:00Z",
      "employeeName": "John Doe"
    },
    {
      "type": "Payroll",
      "title": "Payroll Processed",
      "description": "March 2026 payroll processed for 145 employees",
      "timestamp": "2026-03-28T10:00:00Z",
      "employeeName": null
    }
  ]
}
```

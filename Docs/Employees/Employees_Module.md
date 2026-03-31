# Employees Module

**Status:** Fully Implemented ✅

---

## 1. Module Overview

This module handles:
- Employee profile CRUD operations
- Employee code auto-generation (EMP001, EMP002, etc.)
- Reporting manager relationship (self-referencing)
- Soft delete functionality
- Filtering and pagination

### Related Files

| Layer | File | Purpose |
|-------|------|---------|
| **Domain** | `src/EMS.Domain/Entities/Employee/EmployeeProfile.cs` | Employee entity |
| **Domain** | `src/EMS.Domain/Enums/EmploymentStatus.cs` | Employment status enum |
| **Domain** | `src/EMS.Domain/Enums/Gender.cs` | Gender enum |
| **Application** | `src/EMS.Application/Modules/Employees/DTOs/CreateEmployeeDto.cs` | Create request DTO |
| **Application** | `src/EMS.Application/Modules/Employees/DTOs/UpdateEmployeeDto.cs` | Update request DTO |
| **Application** | `src/EMS.Application/Modules/Employees/DTOs/EmployeeResponseDto.cs` | Response DTO |
| **Application** | `src/EMS.Application/Modules/Employees/DTOs/EmployeeFilterDto.cs` | Filter/pagination DTO |
| **Application** | `src/EMS.Application/Modules/Employees/Interfaces/IEmployeeService.cs` | Service interface |
| **Application** | `src/EMS.Application/Modules/Employees/Interfaces/IEmployeeRepository.cs` | Repository interface |
| **Application** | `src/EMS.Application/Modules/Employees/Services/EmployeeService.cs` | Business logic |
| **Infrastructure** | `src/EMS.Infrastructure/Repositories/EmployeeRepository.cs` | Data access |
| **API** | `src/EMS.API/Controllers/v1/EmployeesController.cs` | Endpoints |

---

## 2. Enums

### 2.1 EmploymentStatus
```csharp
// src/EMS.Domain/Enums/EmploymentStatus.cs
public enum EmploymentStatus
{
    Active = 1,
    Inactive = 2,
    OnProbation = 3,
    Resigned = 4,
    Terminated = 5
}
```

### 2.2 Gender
```csharp
// src/EMS.Domain/Enums/Gender.cs
public enum Gender
{
    Male = 1,
    Female = 2,
    Other = 3
}
```

---

## 3. Entity

### EmployeeProfile
```csharp
// src/EMS.Domain/Entities/Employee/EmployeeProfile.cs
using EMS.Domain.Common;
using EMS.Domain.Enums;
using EMS.Domain.Entities.Organization;

public class EmployeeProfile : BaseEntity
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public DateTime JoiningDate { get; set; }
    public EmploymentStatus Status { get; set; } = EmploymentStatus.Active;
    public string? ProbationEndDate { get; set; }

    // FKs
    public int DepartmentId { get; set; }
    public int? DesignationId { get; set; }
    public int? ReportingManagerId { get; set; }
    public int? UserId { get; set; }

    // Navigation Properties
    public Department Department { get; set; } = null!;
    public Designation? Designation { get; set; }
    public EmployeeProfile? ReportingManager { get; set; }  // Self-reference
}
```

---

## 4. DTOs

### 4.1 CreateEmployeeDto
```csharp
// src/EMS.Application/Modules/Employees/DTOs/CreateEmployeeDto.cs
using System.ComponentModel.DataAnnotations;
using EMS.Domain.Enums;

public class CreateEmployeeDto
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public Gender Gender { get; set; }

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public DateTime JoiningDate { get; set; }

    [Required]
    public int DepartmentId { get; set; }

    public int? DesignationId { get; set; }

    public int? ReportingManagerId { get; set; }

    // Optional — user account bhi banana hai saath mein
    public string? UserPassword { get; set; }
}
```

### 4.2 UpdateEmployeeDto
```csharp
// src/EMS.Application/Modules/Employees/DTOs/UpdateEmployeeDto.cs
using System.ComponentModel.DataAnnotations;
using EMS.Domain.Enums;

public class UpdateEmployeeDto
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    public Gender Gender { get; set; }

    public DateTime DateOfBirth { get; set; }

    public DateTime JoiningDate { get; set; }

    [Required]
    public int DepartmentId { get; set; }

    public int? DesignationId { get; set; }

    public int? ReportingManagerId { get; set; }

    public EmploymentStatus Status { get; set; }
}
```

### 4.3 EmployeeResponseDto
```csharp
// src/EMS.Application/Modules/Employees/DTOs/EmployeeResponseDto.cs
using EMS.Domain.Enums;

public class EmployeeResponseDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public int Age => (int)((DateTime.Today - DateOfBirth).TotalDays / 365.25);
    public DateTime JoiningDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ProfilePhotoUrl { get; set; }

    // Organization
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int? DesignationId { get; set; }
    public string? DesignationTitle { get; set; }
    public int? ReportingManagerId { get; set; }
    public string? ReportingManagerName { get; set; }

    // Linked account
    public int? UserId { get; set; }

    public DateTime CreatedAt { get; set; }
}
```

### 4.4 EmployeeFilterDto
```csharp
// src/EMS.Application/Modules/Employees/DTOs/EmployeeFilterDto.cs
using EMS.Domain.Enums;

// Query params ke liye — search, filter, pagination sab ek jagah
public class EmployeeFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }           // Name ya Email se
    public int? DepartmentId { get; set; }
    public int? DesignationId { get; set; }
    public EmploymentStatus? Status { get; set; }
    public Gender? Gender { get; set; }
}
```

---

## 5. Interfaces

### 5.1 IEmployeeService
```csharp
// src/EMS.Application/Modules/Employees/Interfaces/IEmployeeService.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Employees.DTOs;

public interface IEmployeeService
{
    Task<PaginatedResult<EmployeeResponseDto>> GetAllAsync(EmployeeFilterDto filter);
    Task<EmployeeResponseDto?> GetByIdAsync(int id);
    Task<(EmployeeResponseDto? result, string? error)> CreateAsync(CreateEmployeeDto dto);
    Task<(EmployeeResponseDto? result, string? error)> UpdateAsync(int id, UpdateEmployeeDto dto);
    Task<(bool success, string? error)> DeleteAsync(int id);
}
```

### 5.2 IEmployeeRepository
```csharp
// src/EMS.Application/Modules/Employees/Interfaces/IEmployeeRepository.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Employees.DTOs;
using EMS.Domain.Entities.Employee;

public interface IEmployeeRepository
{
    Task<PaginatedResult<EmployeeProfile>> GetAllAsync(EmployeeFilterDto filter);
    Task<EmployeeProfile?> GetByIdAsync(int id);
    Task<EmployeeProfile?> GetByEmailAsync(string email);
    Task<EmployeeProfile> CreateAsync(EmployeeProfile employee);
    Task<EmployeeProfile?> UpdateAsync(int id, EmployeeProfile employee);
    Task<bool> DeleteAsync(int id);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
    Task<int> GetTotalCountAsync();   // EmployeeCode generate karne ke liye
}
```

---

## 6. Service Implementation

### EmployeeService
```csharp
// src/EMS.Application/Modules/Employees/Services/EmployeeService.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Employees.DTOs;
using EMS.Application.Modules.Employees.Interfaces;
using EMS.Domain.Entities.Employee;
using EMS.Domain.Enums;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<PaginatedResult<EmployeeResponseDto>> GetAllAsync(EmployeeFilterDto filter)
    {
        var result = await _employeeRepository.GetAllAsync(filter);
        return new PaginatedResult<EmployeeResponseDto>
        {
            Data = result.Data.Select(MapToDto),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<EmployeeResponseDto?> GetByIdAsync(int id)
    {
        var emp = await _employeeRepository.GetByIdAsync(id);
        return emp == null ? null : MapToDto(emp);
    }

    public async Task<(EmployeeResponseDto? result, string? error)> CreateAsync(
        CreateEmployeeDto dto)
    {
        // Email unique check
        if (await _employeeRepository.EmailExistsAsync(dto.Email))
            return (null, "Email already registered.");

        // Auto EmployeeCode generate — EMP001, EMP002...
        var totalCount = await _employeeRepository.GetTotalCountAsync();
        var employeeCode = $"EMP{(totalCount + 1):D3}";  // EMP001 format

        var employee = new EmployeeProfile
        {
            EmployeeCode = employeeCode,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.ToLower().Trim(),
            Phone = dto.Phone.Trim(),
            Gender = dto.Gender,
            DateOfBirth = DateTime.SpecifyKind(dto.DateOfBirth, DateTimeKind.Utc),
            JoiningDate = DateTime.SpecifyKind(dto.JoiningDate, DateTimeKind.Utc),
            DepartmentId = dto.DepartmentId,
            DesignationId = dto.DesignationId,
            ReportingManagerId = dto.ReportingManagerId,
            Status = EmploymentStatus.Active
        };

        var created = await _employeeRepository.CreateAsync(employee);
        return (MapToDto(created), null);
    }

    public async Task<(EmployeeResponseDto? result, string? error)> UpdateAsync(
        int id, UpdateEmployeeDto dto)
    {
        var existing = await _employeeRepository.GetByIdAsync(id);
        if (existing == null)
            return (null, "Employee not found.");

        // Email change ho rahi hai toh check karo
        if (existing.Email != dto.Email.ToLower().Trim() &&
            await _employeeRepository.EmailExistsAsync(dto.Email, excludeId: id))
            return (null, "Email already in use.");

        existing.FirstName = dto.FirstName.Trim();
        existing.LastName = dto.LastName.Trim();
        existing.Email = dto.Email.ToLower().Trim();
        existing.Phone = dto.Phone.Trim();
        existing.Gender = dto.Gender;
        existing.DateOfBirth = DateTime.SpecifyKind(dto.DateOfBirth, DateTimeKind.Utc);
        existing.JoiningDate = DateTime.SpecifyKind(dto.JoiningDate, DateTimeKind.Utc);
        existing.DepartmentId = dto.DepartmentId;
        existing.DesignationId = dto.DesignationId;
        existing.ReportingManagerId = dto.ReportingManagerId;
        existing.Status = dto.Status;
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _employeeRepository.UpdateAsync(id, existing);
        return updated == null ? (null, "Update failed.") : (MapToDto(updated), null);
    }

    public async Task<(bool success, string? error)> DeleteAsync(int id)
    {
        var emp = await _employeeRepository.GetByIdAsync(id);
        if (emp == null) return (false, "Employee not found.");

        var deleted = await _employeeRepository.DeleteAsync(id);
        return deleted ? (true, null) : (false, "Delete failed.");
    }

    private static EmployeeResponseDto MapToDto(EmployeeProfile e) => new()
    {
        Id = e.Id,
        EmployeeCode = e.EmployeeCode,
        FirstName = e.FirstName,
        LastName = e.LastName,
        Email = e.Email,
        Phone = e.Phone,
        Gender = e.Gender.ToString(),
        DateOfBirth = e.DateOfBirth,
        JoiningDate = e.JoiningDate,
        Status = e.Status.ToString(),
        ProfilePhotoUrl = e.ProfilePhotoUrl,
        DepartmentId = e.DepartmentId,
        DepartmentName = e.Department?.Name ?? "Unknown",
        DesignationId = e.DesignationId,
        DesignationTitle = e.Designation?.Title,
        ReportingManagerId = e.ReportingManagerId,
        ReportingManagerName = e.ReportingManager != null
            ? $"{e.ReportingManager.FirstName} {e.ReportingManager.LastName}"
            : null,
        UserId = e.UserId,
        CreatedAt = e.CreatedAt
    };
}
```

---

## 7. Repository Implementation

### EmployeeRepository
```csharp
// src/EMS.Infrastructure/Repositories/EmployeeRepository.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Employees.DTOs;
using EMS.Application.Modules.Employees.Interfaces;
using EMS.Domain.Entities.Employee;
using EMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<EmployeeProfile>> GetAllAsync(EmployeeFilterDto filter)
    {
        var query = _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.ReportingManager)
            .AsQueryable();

        // Search — name ya email
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.ToLower();
            query = query.Where(e =>
                e.FirstName.ToLower().Contains(s) ||
                e.LastName.ToLower().Contains(s) ||
                e.Email.ToLower().Contains(s) ||
                e.EmployeeCode.ToLower().Contains(s));
        }

        // Filters
        if (filter.DepartmentId.HasValue)
            query = query.Where(e => e.DepartmentId == filter.DepartmentId);

        if (filter.DesignationId.HasValue)
            query = query.Where(e => e.DesignationId == filter.DesignationId);

        if (filter.Status.HasValue)
            query = query.Where(e => e.Status == filter.Status);

        if (filter.Gender.HasValue)
            query = query.Where(e => e.Gender == filter.Gender);

        var total = await query.CountAsync();

        var data = await query
            .OrderBy(e => e.EmployeeCode)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PaginatedResult<EmployeeProfile>
        {
            Data = data,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<EmployeeProfile?> GetByIdAsync(int id)
        => await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.ReportingManager)
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<EmployeeProfile?> GetByEmailAsync(string email)
        => await _context.Employees
            .FirstOrDefaultAsync(e => e.Email == email.ToLower());

    public async Task<EmployeeProfile> CreateAsync(EmployeeProfile employee)
    {
        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync();

        // Reload with navigations
        return (await GetByIdAsync(employee.Id))!;
    }

    public async Task<EmployeeProfile?> UpdateAsync(int id, EmployeeProfile employee)
    {
        var existing = await _context.Employees.FindAsync(id);
        if (existing == null) return null;

        // Copy updated fields
        _context.Entry(existing).CurrentValues.SetValues(employee);
        existing.Id = id; // ID preserve karo
        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var emp = await _context.Employees.FindAsync(id);
        if (emp == null) return false;

        emp.IsDeleted = true;
        emp.Status = Domain.Enums.EmploymentStatus.Inactive;
        emp.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        => await _context.Employees.AnyAsync(e =>
            e.Email == email.ToLower() &&
            (excludeId == null || e.Id != excludeId));

    public async Task<int> GetTotalCountAsync()
        => await _context.Employees.IgnoreQueryFilters().CountAsync();
}
```

---

## 8. API Controller

### EmployeesController
```csharp
// src/EMS.API/Controllers/v1/EmployeesController.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Employees.DTOs;
using EMS.Application.Modules.Employees.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _service;

    public EmployeesController(IEmployeeService service)
    {
        _service = service;
    }

    // GET api/v1/employees?page=1&pageSize=10&search=john&departmentId=1
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EmployeeFilterDto filter)
    {
        var result = await _service.GetAllAsync(filter);
        return Ok(ApiResponse<PaginatedResult<EmployeeResponseDto>>.Ok(result));
    }

    // GET api/v1/employees/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("Employee not found."));
        return Ok(ApiResponse<EmployeeResponseDto>.Ok(result));
    }

    // POST api/v1/employees
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (result, error) = await _service.CreateAsync(dto);
        if (error != null)
            return Conflict(ApiResponse<string>.Fail(error));

        return CreatedAtAction(nameof(GetById),
            new { id = result!.Id },
            ApiResponse<EmployeeResponseDto>.Ok(result, "Employee created successfully."));
    }

    // PUT api/v1/employees/5
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (result, error) = await _service.UpdateAsync(id, dto);
        if (error != null)
            return error.Contains("not found")
                ? NotFound(ApiResponse<string>.Fail(error))
                : Conflict(ApiResponse<string>.Fail(error));

        return Ok(ApiResponse<EmployeeResponseDto>.Ok(result!, "Employee updated."));
    }

    // DELETE api/v1/employees/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, error) = await _service.DeleteAsync(id);
        if (!success)
            return NotFound(ApiResponse<string>.Fail(error!));

        return Ok(ApiResponse<string>.Ok("Deleted", "Employee deleted."));
    }
}
```

---

## 9. API Endpoints Summary

| Method | Endpoint | Authorization | Description |
|--------|----------|---------------|-------------|
| GET | `/api/v1/employees` | Authenticated | List employees with pagination & filters |
| GET | `/api/v1/employees/{id}` | Authenticated | Get single employee |
| POST | `/api/v1/employees` | SuperAdmin, HRAdmin | Create new employee |
| PUT | `/api/v1/employees/{id}` | SuperAdmin, HRAdmin | Update employee |
| DELETE | `/api/v1/employees/{id}` | SuperAdmin, HRAdmin | Soft delete employee |

---

## 10. Features

### 10.1 Employee Code Auto-Generation
- Format: `EMP001`, `EMP002`, `EMP003`, etc.
- Generated sequentially based on total employee count
- Unique constraint on EmployeeCode

### 10.2 Soft Delete
- Sets `IsDeleted = true`
- Sets `Status = Inactive`
- Query filters exclude soft-deleted records

### 10.3 Reporting Manager (Self-Reference)
- Employee can have a ReportingManagerId pointing to another Employee
- Used for hierarchy in organization

### 10.4 Filtering & Pagination
- Search by name, email, employee code
- Filter by department, designation, status, gender
- Pagination with configurable page size

---

## 11. API Usage Examples

### Create Employee
```bash
POST /api/v1/employees
Authorization: Bearer <token>
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phone": "+91-9876543210",
  "gender": 1,
  "dateOfBirth": "1995-05-15",
  "joiningDate": "2026-03-30",
  "departmentId": 1,
  "designationId": 2,
  "reportingManagerId": 1
}

# Response 201:
{
  "success": true,
  "message": "Employee created successfully.",
  "data": {
    "id": 5,
    "employeeCode": "EMP005",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "fullName": "John Doe",
    "status": "Active",
    ...
  }
}
```

### List Employees with Filters
```bash
GET /api/v1/employees?page=1&pageSize=10&search=john&departmentId=1&status=1
Authorization: Bearer <token>

# Response 200:
{
  "success": true,
  "data": {
    "data": [...],
    "totalCount": 25,
    "page": 1,
    "pageSize": 10,
    "totalPages": 3,
    "hasNext": true,
    "hasPrevious": false
  }
}
```

### Update Employee
```bash
PUT /api/v1/employees/5
Authorization: Bearer <token>
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Smith",
  "email": "john.smith@example.com",
  "phone": "+91-9876543210",
  "gender": 1,
  "dateOfBirth": "1995-05-15",
  "joiningDate": "2026-03-30",
  "departmentId": 1,
  "designationId": 3,
  "reportingManagerId": 1,
  "status": 1
}

# Response 200:
{
  "success": true,
  "message": "Employee updated.",
  "data": { ... }
}
```

### Delete Employee (Soft Delete)
```bash
DELETE /api/v1/employees/5
Authorization: Bearer <token>

# Response 200:
{
  "success": true,
  "message": "Employee deleted.",
  "data": "Deleted"
}
```

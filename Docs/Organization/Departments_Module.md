# Departments Module

**Status:** Fully Implemented ✅

---

## 1. Module Overview

This module handles:
- Department CRUD operations
- Department listing with search and pagination
- Employee count per department
- Prevention of deletion when employees exist

### Related Files

| Layer | File | Purpose |
|-------|------|---------|
| **Domain** | `src/EMS.Domain/Entities/Organization/Department.cs` | Department entity |
| **Application** | `src/EMS.Application/Modules/Organization/DTOs/CreateDepartmentDto.cs` | Create DTO |
| **Application** | `src/EMS.Application/Modules/Organization/DTOs/UpdateDepartmentDto.cs` | Update DTO |
| **Application** | `src/EMS.Application/Modules/Organization/DTOs/DepartmentResponseDto.cs` | Response DTO |
| **Application** | `src/EMS.Application/Modules/Organization/Interfaces/IDepartmentService.cs` | Service interface |
| **Application** | `src/EMS.Application/Modules/Organization/Interfaces/IDepartmentRepository.cs` | Repository interface |
| **Application** | `src/EMS.Application/Modules/Organization/Services/DepartmentService.cs` | Business logic |
| **Infrastructure** | `src/EMS.Infrastructure/Repositories/DepartmentRepository.cs` | Data access |
| **API** | `src/EMS.API/Controllers/v1/DepartmentsController.cs` | Endpoints |

---

## 2. Entity

### Department
```csharp
// src/EMS.Domain/Entities/Organization/Department.cs
using EMS.Domain.Common;
using EMS.Domain.Entities.Employee;

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Code { get; set; }        // e.g. "ENG", "HR"
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<EmployeeProfile> Employees { get; set; } = new List<EmployeeProfile>();
}
```

---

## 3. DTOs

### 3.1 CreateDepartmentDto
```csharp
// src/EMS.Application/Modules/Organization/DTOs/CreateDepartmentDto.cs
using System.ComponentModel.DataAnnotations;

public class CreateDepartmentDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(10)]
    public string? Code { get; set; }

    public bool IsActive { get; set; } = true;
}
```

### 3.2 UpdateDepartmentDto
```csharp
// src/EMS.Application/Modules/Organization/DTOs/UpdateDepartmentDto.cs
using System.ComponentModel.DataAnnotations;

public class UpdateDepartmentDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(10)]
    public string? Code { get; set; }

    public bool IsActive { get; set; } = true;
}
```

### 3.3 DepartmentResponseDto
```csharp
// src/EMS.Application/Modules/Organization/DTOs/DepartmentResponseDto.cs
public class DepartmentResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Code { get; set; }
    public bool IsActive { get; set; }
    public int EmployeeCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

## 4. Interfaces

### 4.1 IDepartmentService
```csharp
// src/EMS.Application/Modules/Organization/Interfaces/IDepartmentService.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Organization.DTOs;

public interface IDepartmentService
{
    Task<PaginatedResult<DepartmentResponseDto>> GetAllAsync(int page, int pageSize, string? search);
    Task<DepartmentResponseDto?> GetByIdAsync(int id);
    Task<(DepartmentResponseDto? result, string? error)> CreateAsync(CreateDepartmentDto dto);
    Task<(DepartmentResponseDto? result, string? error)> UpdateAsync(int id, UpdateDepartmentDto dto);
    Task<(bool success, string? error)> DeleteAsync(int id);
}
```

### 4.2 IDepartmentRepository
```csharp
// src/EMS.Application/Modules/Organization/Interfaces/IDepartmentRepository.cs
using EMS.Application.Common.DTOs;
using EMS.Domain.Entities.Organization;

public interface IDepartmentRepository
{
    Task<PaginatedResult<Department>> GetAllAsync(int page, int pageSize, string? search);
    Task<Department?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> NameExistsAsync(string name, int? excludeId = null);
    Task<Department> CreateAsync(Department department);
    Task<Department?> UpdateAsync(int id, Department department);
    Task<bool> DeleteAsync(int id);
}
```

---

## 5. Service Implementation

### DepartmentService
```csharp
// src/EMS.Application/Modules/Organization/Services/DepartmentService.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Organization.DTOs;
using EMS.Application.Modules.Organization.Interfaces;
using EMS.Domain.Entities.Organization;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;

    public DepartmentService(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<PaginatedResult<DepartmentResponseDto>> GetAllAsync(
        int page, int pageSize, string? search)
    {
        var result = await _departmentRepository.GetAllAsync(page, pageSize, search);
        return new PaginatedResult<DepartmentResponseDto>
        {
            Data = result.Data.Select(MapToDto),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<DepartmentResponseDto?> GetByIdAsync(int id)
    {
        var dept = await _departmentRepository.GetByIdAsync(id);
        return dept == null ? null : MapToDto(dept);
    }

    public async Task<(DepartmentResponseDto? result, string? error)> CreateAsync(
        CreateDepartmentDto dto)
    {
        // Name unique check
        if (await _departmentRepository.NameExistsAsync(dto.Name))
            return (null, "Department name already exists.");

        var dept = new Department
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Code = dto.Code?.ToUpper().Trim(),
            IsActive = true
        };

        var created = await _departmentRepository.CreateAsync(dept);
        return (MapToDto(created), null);
    }

    public async Task<(DepartmentResponseDto? result, string? error)> UpdateAsync(
        int id, UpdateDepartmentDto dto)
    {
        if (!await _departmentRepository.ExistsAsync(id))
            return (null, "Department not found.");

        if (await _departmentRepository.NameExistsAsync(dto.Name, excludeId: id))
            return (null, "Department name already taken.");

        var dept = new Department
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Code = dto.Code?.ToUpper().Trim(),
            IsActive = dto.IsActive
        };

        var updated = await _departmentRepository.UpdateAsync(id, dept);
        return updated == null ? (null, "Update failed.") : (MapToDto(updated), null);
    }

    public async Task<(bool success, string? error)> DeleteAsync(int id)
    {
        var dept = await _departmentRepository.GetByIdAsync(id);
        if (dept == null) return (false, "Department not found.");

        if (dept.Employees.Any())
            return (false, "Cannot delete department with active employees.");

        var deleted = await _departmentRepository.DeleteAsync(id);
        return deleted ? (true, null) : (false, "Delete failed.");
    }

    private static DepartmentResponseDto MapToDto(Department d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        Description = d.Description,
        Code = d.Code,
        IsActive = d.IsActive,
        EmployeeCount = d.Employees?.Count ?? 0,
        CreatedAt = d.CreatedAt
    };
}
```

---

## 6. API Controller

### DepartmentsController
```csharp
// src/EMS.API/Controllers/v1/DepartmentsController.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Organization.DTOs;
using EMS.Application.Modules.Organization.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _service;

    public DepartmentsController(IDepartmentService service)
    {
        _service = service;
    }

    // GET api/v1/departments?page=1&pageSize=10&search=eng
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await _service.GetAllAsync(page, pageSize, search);
        return Ok(ApiResponse<PaginatedResult<DepartmentResponseDto>>.Ok(result));
    }

    // GET api/v1/departments/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("Department not found."));
        return Ok(ApiResponse<DepartmentResponseDto>.Ok(result));
    }

    // POST api/v1/departments
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (result, error) = await _service.CreateAsync(dto);
        if (error != null)
            return Conflict(ApiResponse<string>.Fail(error));

        return CreatedAtAction(nameof(GetById),
            new { id = result!.Id },
            ApiResponse<DepartmentResponseDto>.Ok(result, "Department created."));
    }

    // PUT api/v1/departments/5
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (result, error) = await _service.UpdateAsync(id, dto);
        if (error != null)
            return error.Contains("not found")
                ? NotFound(ApiResponse<string>.Fail(error))
                : Conflict(ApiResponse<string>.Fail(error));

        return Ok(ApiResponse<DepartmentResponseDto>.Ok(result!, "Department updated."));
    }

    // DELETE api/v1/departments/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, error) = await _service.DeleteAsync(id);
        if (!success)
            return error!.Contains("not found")
                ? NotFound(ApiResponse<string>.Fail(error))
                : BadRequest(ApiResponse<string>.Fail(error));

        return Ok(ApiResponse<string>.Ok("Deleted", "Department deleted."));
    }
}
```

---

## 7. API Endpoints Summary

| Method | Endpoint | Authorization | Description |
|--------|----------|---------------|-------------|
| GET | `/api/v1/departments` | Authenticated | List departments |
| GET | `/api/v1/departments/{id}` | Authenticated | Get department by ID |
| POST | `/api/v1/departments` | SuperAdmin, HRAdmin | Create department |
| PUT | `/api/v1/departments/{id}` | SuperAdmin, HRAdmin | Update department |
| DELETE | `/api/v1/departments/{id}` | SuperAdmin | Delete department |

---

## 8. Features

### 8.1 CRUD Operations
- Create departments with name, description, and code
- Update department details
- List with pagination and search
- Soft delete protection

### 8.2 Business Rules
- Department name must be unique
- Cannot delete department with active employees
- Department code is auto-uppercased

### 8.3 Employee Count
- Each department response includes employee count
- Helps in managing organization structure

---

## 9. API Usage Examples

### Create Department
```bash
POST /api/v1/departments
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Engineering",
  "description": "Software Engineering Department",
  "code": "ENG",
  "isActive": true
}

# Response 201:
{
  "success": true,
  "message": "Department created.",
  "data": {
    "id": 3,
    "name": "Engineering",
    "description": "Software Engineering Department",
    "code": "ENG",
    "isActive": true,
    "employeeCount": 0,
    "createdAt": "2026-03-30T10:00:00Z"
  }
}
```

### List Departments
```bash
GET /api/v1/departments?page=1&pageSize=10&search=eng
Authorization: Bearer <token>

# Response 200:
{
  "success": true,
  "data": {
    "data": [
      {
        "id": 3,
        "name": "Engineering",
        "code": "ENG",
        "isActive": true,
        "employeeCount": 15,
        "createdAt": "2026-03-30T10:00:00Z"
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 10
  }
}
```

### Update Department
```bash
PUT /api/v1/departments/3
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Software Engineering",
  "description": "All software development teams",
  "code": "SWE",
  "isActive": true
}

# Response 200:
{
  "success": true,
  "message": "Department updated.",
  "data": { ... }
}
```

### Delete Department (Fails if employees exist)
```bash
DELETE /api/v1/departments/3
Authorization: Bearer <token>

# Response 400:
{
  "success": false,
  "message": "Cannot delete department with active employees."
}
```

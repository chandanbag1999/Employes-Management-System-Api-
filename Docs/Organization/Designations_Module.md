# Designations Module

**Status:** Fully Implemented ✅

---

## 1. Module Overview

This module handles:
- Designation CRUD operations
- Designation listing with department filter
- Soft delete with restore functionality
- Purge old deleted designations

### Related Files

| Layer | File | Purpose |
|-------|------|---------|
| **Domain** | `src/EMS.Domain/Entities/Organization/Designation.cs` | Designation entity |
| **Application** | `src/EMS.Application/Modules/Organization/DTOs/CreateDesignationDto.cs` | Create DTO |
| **Application** | `src/EMS.Application/Modules/Organization/DTOs/DesignationResponseDto.cs` | Response DTO |
| **Application** | `src/EMS.Application/Modules/Organization/Interfaces/IDesignationService.cs` | Service interface |
| **Application** | `src/EMS.Application/Modules/Organization/Interfaces/IDesignationRepository.cs` | Repository interface |
| **Application** | `src/EMS.Application/Modules/Organization/Services/DesignationService.cs` | Business logic |
| **Infrastructure** | `src/EMS.Infrastructure/Repositories/DesignationRepository.cs` | Data access |
| **API** | `src/EMS.API/Controllers/v1/DesignationsController.cs` | Endpoints |

---

## 2. Entity

### Designation
```csharp
// src/EMS.Domain/Entities/Organization/Designation.cs
using EMS.Domain.Common;

public class Designation : BaseEntity
{
    public string Title { get; set; } = string.Empty;   // e.g. "Software Engineer"
    public string? Description { get; set; }
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
}
```

---

## 3. DTOs

### 3.1 CreateDesignationDto
```csharp
// src/EMS.Application/Modules/Organization/DTOs/CreateDesignationDto.cs
using System.ComponentModel.DataAnnotations;

public class CreateDesignationDto
{
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int? DepartmentId { get; set; }
}
```

### 3.2 DesignationResponseDto
```csharp
// src/EMS.Application/Modules/Organization/DTOs/DesignationResponseDto.cs
public class DesignationResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

## 4. Interfaces

### 4.1 IDesignationService
```csharp
// src/EMS.Application/Modules/Organization/Interfaces/IDesignationService.cs
using EMS.Application.Modules.Organization.DTOs;

public interface IDesignationService
{
    Task<IEnumerable<DesignationResponseDto>> GetAllAsync(int? departmentId);
    Task<IEnumerable<DesignationResponseDto>> GetAllDeletedAsync();
    Task<DesignationResponseDto?> GetByIdAsync(int id);
    Task<DesignationResponseDto> CreateAsync(CreateDesignationDto dto);
    Task<DesignationResponseDto?> UpdateAsync(int id, CreateDesignationDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> RestoreAsync(int id);
    Task<int> PurgeOldDeletedAsync(int olderThanMonths = 12);
}
```

### 4.2 IDesignationRepository
```csharp
// src/EMS.Application/Modules/Organization/Interfaces/IDesignationRepository.cs
using EMS.Domain.Entities.Organization;

public interface IDesignationRepository
{
    Task<IEnumerable<Designation>> GetAllAsync(int? departmentId);
    Task<IEnumerable<Designation>> GetAllDeletedAsync();
    Task<Designation?> GetByIdAsync(int id);
    Task<Designation> CreateAsync(Designation designation);
    Task<Designation?> UpdateAsync(int id, Designation designation);
    Task<bool> DeleteAsync(int id);
    Task<bool> RestoreAsync(int id);
    Task<int> PurgeOldDeletedAsync(int olderThanMonths);
}
```

---

## 5. Service Implementation

### DesignationService
```csharp
// src/EMS.Application/Modules/Organization/Services/DesignationService.cs
using EMS.Application.Modules.Organization.DTOs;
using EMS.Application.Modules.Organization.Interfaces;
using EMS.Domain.Entities.Organization;

public class DesignationService : IDesignationService
{
    private readonly IDesignationRepository _repo;

    public DesignationService(IDesignationRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<DesignationResponseDto>> GetAllAsync(int? departmentId)
    {
        var list = await _repo.GetAllAsync(departmentId);
        return list.Select(MapToDto);
    }

    public async Task<IEnumerable<DesignationResponseDto>> GetAllDeletedAsync()
    {
        var list = await _repo.GetAllDeletedAsync();
        return list.Select(MapToDto);
    }

    public async Task<DesignationResponseDto?> GetByIdAsync(int id)
    {
        var d = await _repo.GetByIdAsync(id);
        return d == null ? null : MapToDto(d);
    }

    public async Task<DesignationResponseDto> CreateAsync(CreateDesignationDto dto)
    {
        var designation = new Designation
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            DepartmentId = dto.DepartmentId
        };
        var created = await _repo.CreateAsync(designation);
        return MapToDto(created);
    }

    public async Task<DesignationResponseDto?> UpdateAsync(int id, CreateDesignationDto dto)
    {
        var designation = new Designation
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            DepartmentId = dto.DepartmentId
        };
        var updated = await _repo.UpdateAsync(id, designation);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
        => await _repo.DeleteAsync(id);

    public async Task<bool> RestoreAsync(int id)
        => await _repo.RestoreAsync(id);

    public async Task<int> PurgeOldDeletedAsync(int olderThanMonths = 12)
        => await _repo.PurgeOldDeletedAsync(olderThanMonths);

    private static DesignationResponseDto MapToDto(Designation d) => new()
    {
        Id = d.Id,
        Title = d.Title,
        Description = d.Description,
        DepartmentId = d.DepartmentId,
        DepartmentName = d.Department?.Name,
        CreatedAt = d.CreatedAt
    };
}
```

---

## 6. API Controller

### DesignationsController
```csharp
// src/EMS.API/Controllers/v1/DesignationsController.cs
using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Organization.DTOs;
using EMS.Application.Modules.Organization.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DesignationsController : ControllerBase
{
    private readonly IDesignationService _service;

    public DesignationsController(IDesignationService service)
    {
        _service = service;
    }

    // GET api/v1/designations
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? departmentId = null)
    {
        var result = await _service.GetAllAsync(departmentId);
        return Ok(ApiResponse<IEnumerable<DesignationResponseDto>>.Ok(result));
    }

    // GET api/v1/designations/deleted
    [HttpGet("deleted")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> GetAllDeleted()
    {
        var result = await _service.GetAllDeletedAsync();
        return Ok(ApiResponse<IEnumerable<DesignationResponseDto>>.Ok(result));
    }

    // GET api/v1/designations/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("Designation not found."));
        return Ok(ApiResponse<DesignationResponseDto>.Ok(result));
    }

    // POST api/v1/designations
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateDesignationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById),
            new { id = result.Id },
            ApiResponse<DesignationResponseDto>.Ok(result, "Designation created."));
    }

    // PUT api/v1/designations/5
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateDesignationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.UpdateAsync(id, dto);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail("Designation not found."));
        return Ok(ApiResponse<DesignationResponseDto>.Ok(result, "Designation updated."));
    }

    // DELETE api/v1/designations/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result)
            return NotFound(ApiResponse<string>.Fail("Designation not found."));
        return Ok(ApiResponse<string>.Ok("Deleted", "Designation deleted."));
    }

    // POST api/v1/designations/5/restore
    [HttpPost("{id}/restore")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Restore(int id)
    {
        var result = await _service.RestoreAsync(id);
        if (!result)
            return NotFound(ApiResponse<string>.Fail("Deleted designation not found."));
        return Ok(ApiResponse<string>.Ok("Restored", "Designation restored successfully."));
    }

    // DELETE api/v1/designations/purge?months=12
    [HttpDelete("purge")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> PurgeOld([FromQuery] int months = 12)
    {
        var count = await _service.PurgeOldDeletedAsync(months);
        return Ok(ApiResponse<string>.Ok($"{count}", $"Purged {count} old deleted designations."));
    }
}
```

---

## 7. API Endpoints Summary

| Method | Endpoint | Authorization | Description |
|--------|----------|---------------|-------------|
| GET | `/api/v1/designations` | Authenticated | List designations |
| GET | `/api/v1/designations/deleted` | SuperAdmin, HRAdmin | List deleted designations |
| GET | `/api/v1/designations/{id}` | Authenticated | Get designation by ID |
| POST | `/api/v1/designations` | SuperAdmin, HRAdmin | Create designation |
| PUT | `/api/v1/designations/{id}` | SuperAdmin, HRAdmin | Update designation |
| DELETE | `/api/v1/designations/{id}` | SuperAdmin, HRAdmin | Soft delete designation |
| POST | `/api/v1/designations/{id}/restore` | SuperAdmin | Restore deleted designation |
| DELETE | `/api/v1/designations/purge` | SuperAdmin | Purge old deleted designations |

---

## 8. Features

### 8.1 CRUD Operations
- Create designations with title and department association
- Update designation details
- List with optional department filter
- Soft delete (marks as deleted)

### 8.2 Restore & Purge
- Restore accidentally deleted designations
- View all deleted designations
- Purge old deleted designations (default 12 months)
- Only SuperAdmin can restore/purge

### 8.3 Department Association
- Designations can be linked to departments
- Filter designations by department

---

## 9. API Usage Examples

### Create Designation
```bash
POST /api/v1/designations
Authorization: Bearer <token>
Content-Type: application/json

{
  "title": "Senior Software Engineer",
  "description": "Experienced engineer with 5+ years",
  "departmentId": 3
}

# Response 201:
{
  "success": true,
  "message": "Designation created.",
  "data": {
    "id": 8,
    "title": "Senior Software Engineer",
    "description": "Experienced engineer with 5+ years",
    "departmentId": 3,
    "departmentName": "Engineering",
    "createdAt": "2026-03-30T10:00:00Z"
  }
}
```

### List Designations by Department
```bash
GET /api/v1/designations?departmentId=3
Authorization: Bearer <token>

# Response 200:
{
  "success": true,
  "data": [
    {
      "id": 8,
      "title": "Senior Software Engineer",
      "departmentName": "Engineering"
    }
  ]
}
```

### Restore Deleted Designation
```bash
POST /api/v1/designations/5/restore
Authorization: Bearer <token>

# Response 200:
{
  "success": true,
  "message": "Designation restored successfully.",
  "data": "Restored"
}
```

### Purge Old Deleted Designations
```bash
DELETE /api/v1/designations/purge?months=12
Authorization: Bearer <token>

# Response 200:
{
  "success": true,
  "message": "Purged 3 old deleted designations.",
  "data": "3"
}
```

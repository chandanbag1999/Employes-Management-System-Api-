# Departments Module

**Status:** Not Started

## Module Overview
Manages department master data and department-level employee grouping.

## Current Files
- `src/EMS.Application/Modules/Organization/DTOs/CreateDepartmentDto.cs`
- `src/EMS.Application/Modules/Organization/DTOs/UpdateDepartmentDto.cs`
- `src/EMS.Application/Modules/Organization/Services/DepartmentService.cs`
- `src/EMS.Infrastructure/Repositories/DepartmentRepository.cs`
- `src/EMS.API/Controllers/v1/DepartmentsController.cs`

## Current State
- pagination and search exist
- delete guard exists when department has employees
- soft delete exists via `IsDeleted`

## Target State
- keep CRUD stable
- align permission model with company/admin scope
- update docs on every API or validation change

## Activity Log
| Date | Activity | Intention | Files Changed | What Changed | Risk | Test Status | Next Step |
|------|----------|-----------|---------------|--------------|------|-------------|----------|
| 2026-03-27 22:52 | Documentation scaffold | Create dedicated department module doc | `Docs/Organization/Departments_Module.md` | Added module documentation shell | Low | N/A | Review module before implementation |

## Validation / Testing
- [ ] list departments
- [ ] create department
- [ ] update department
- [ ] prevent delete when employees exist

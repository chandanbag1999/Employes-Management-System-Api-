# Employees Module

**Status:** Not Started

## Module Overview
Owns employee lifecycle management, filters, reporting manager link, and soft delete behavior.

## Current Files
- `src/EMS.Application/Modules/Employees/*`
- `src/EMS.Infrastructure/Repositories/EmployeeRepository.cs`
- `src/EMS.API/Controllers/v1/EmployeesController.cs`

## Current State
- employee CRUD exists
- employee code auto-generation exists
- reporting manager relation exists

## Target State
- stronger scope enforcement
- better self/team/company access separation

## Activity Log
| Date | Activity | Intention | Files Changed | What Changed | Risk | Test Status | Next Step |
|------|----------|-----------|---------------|--------------|------|-------------|----------|
| 2026-03-27 22:52 | Documentation scaffold | Create dedicated employees module doc | `Docs/Employees/Employees_Module.md` | Added module documentation shell | Low | N/A | Review module before implementation |

## Validation / Testing
- [ ] list employees
- [ ] create employee
- [ ] update employee
- [ ] soft delete employee
- [ ] verify reporting manager behavior

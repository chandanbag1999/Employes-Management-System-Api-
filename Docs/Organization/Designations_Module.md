# Designations Module

**Status:** Not Started

## Module Overview
Manages designation master data and optional linkage to departments.

## Current Files
- `src/EMS.Application/Modules/Organization/DTOs/CreateDesignationDto.cs`
- `src/EMS.Application/Modules/Organization/Services/DesignationService.cs`
- `src/EMS.Infrastructure/Repositories/DesignationRepository.cs`
- `src/EMS.API/Controllers/v1/DesignationsController.cs`

## Current State
- designation CRUD exists
- deleted record listing exists
- restore and purge flows exist

## Target State
- permission-aware operations
- document soft-delete / restore / purge rules clearly

## Activity Log
| Date | Activity | Intention | Files Changed | What Changed | Risk | Test Status | Next Step |
|------|----------|-----------|---------------|--------------|------|-------------|----------|
| 2026-03-27 22:52 | Documentation scaffold | Create dedicated designation module doc | `Docs/Organization/Designations_Module.md` | Added module documentation shell | Low | N/A | Review module before implementation |

## Validation / Testing
- [ ] list designations
- [ ] create designation
- [ ] update designation
- [ ] delete / restore / purge flows

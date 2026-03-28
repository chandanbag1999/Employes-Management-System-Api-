# Dashboard Module

**Status:** Not Started

## Module Overview
Provides high-level operational stats, headcount summaries, and recent activity feeds.

## Current Files
- `src/EMS.Application/Modules/Dashboard/*`
- `src/EMS.Infrastructure/Services/Dashboard/DashboardService.cs`
- `src/EMS.API/Controllers/v1/DashboardController.cs`

## Current State
- stats, headcount, and activity feed endpoints exist
- service currently queries `AppDbContext` directly

## Target State
- stable admin/manager visibility
- documented KPI definitions

## Activity Log
| Date | Activity | Intention | Files Changed | What Changed | Risk | Test Status | Next Step |
|------|----------|-----------|---------------|--------------|------|-------------|----------|
| 2026-03-27 22:52 | Documentation scaffold | Create dedicated dashboard module doc | `Docs/Dashboard/Dashboard_Module.md` | Added module documentation shell | Low | N/A | Review module before implementation |

## Validation / Testing
- [ ] stats endpoint
- [ ] headcount endpoint
- [ ] recent activities endpoint

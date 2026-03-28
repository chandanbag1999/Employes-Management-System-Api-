# Performance Module

**Status:** Not Started

## Module Overview
Handles goals, progress updates, performance reviews, self comments, and employee performance summary.

## Current Files
- `src/EMS.Application/Modules/Performance/*`
- `src/EMS.Infrastructure/Repositories/PerformanceRepository.cs`
- `src/EMS.API/Controllers/v1/PerformanceController.cs`

## Current State
- goals and reviews exist
- rating calculations exist
- self comment flow exists

## Target State
- clean team-based manager restrictions
- documented review workflow and cycle rules

## Activity Log
| Date | Activity | Intention | Files Changed | What Changed | Risk | Test Status | Next Step |
|------|----------|-----------|---------------|--------------|------|-------------|----------|
| 2026-03-27 22:52 | Documentation scaffold | Create dedicated performance module doc | `Docs/Performance/Performance_Module.md` | Added module documentation shell | Low | N/A | Review module before implementation |

## Validation / Testing
- [ ] create goal
- [ ] update goal progress
- [ ] create review
- [ ] submit review
- [ ] employee summary

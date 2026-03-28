# Attendance Module

**Status:** Not Started

## Module Overview
Handles clock-in, clock-out, working hour calculation, monthly summary, and manual attendance marking.

## Current Files
- `src/EMS.Application/Modules/Attendance/*`
- `src/EMS.Infrastructure/Repositories/AttendanceRepository.cs`
- `src/EMS.API/Controllers/v1/AttendanceController.cs`

## Current State
- clock-in / clock-out exist
- half-day logic exists
- manual attendance exists

## Target State
- strong self/team/admin access rules
- duplicate/manual edge case handling documented

## Activity Log
| Date | Activity | Intention | Files Changed | What Changed | Risk | Test Status | Next Step |
|------|----------|-----------|---------------|--------------|------|-------------|----------|
| 2026-03-27 22:52 | Documentation scaffold | Create dedicated attendance module doc | `Docs/Attendance/Attendance_Module.md` | Added module documentation shell | Low | N/A | Review module before implementation |

## Validation / Testing
- [ ] clock in
- [ ] clock out
- [ ] monthly summary
- [ ] manual mark

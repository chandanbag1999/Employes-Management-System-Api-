# Leave Module

**Status:** Not Started

## Module Overview
Handles leave application, approval/rejection, cancellation, balance tracking, and leave type management.

## Current Files
- `src/EMS.Application/Modules/Leave/*`
- `src/EMS.Infrastructure/Repositories/LeaveRepository.cs`
- `src/EMS.API/Controllers/v1/LeaveController.cs`

## Current State
- leave apply flow exists
- overlap checks exist
- approval/rejection and cancel flows exist

## Target State
- self/team/company access enforcement
- clearer manager approval boundary documentation

## Activity Log
| Date | Activity | Intention | Files Changed | What Changed | Risk | Test Status | Next Step |
|------|----------|-----------|---------------|--------------|------|-------------|----------|
| 2026-03-27 22:52 | Documentation scaffold | Create dedicated leave module doc | `Docs/Leave/Leave_Module.md` | Added module documentation shell | Low | N/A | Review module before implementation |

## Validation / Testing
- [ ] apply leave
- [ ] reject overlapping leave
- [ ] approve / reject flow
- [ ] cancel flow
- [ ] leave balance

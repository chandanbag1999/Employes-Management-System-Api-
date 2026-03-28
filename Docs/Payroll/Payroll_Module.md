# Payroll Module

**Status:** Not Started

## Module Overview
Handles salary structures, payroll generation, deductions, payslips, and payment state transitions.

## Current Files
- `src/EMS.Application/Modules/Payroll/*`
- `src/EMS.Infrastructure/Repositories/PayrollRepository.cs`
- `src/EMS.API/Controllers/v1/PayrollController.cs`

## Current State
- salary structure creation exists
- payroll run exists
- payslip retrieval exists

## Target State
- stricter company-level authorization
- better payroll-specific documentation and validation notes

## Activity Log
| Date | Activity | Intention | Files Changed | What Changed | Risk | Test Status | Next Step |
|------|----------|-----------|---------------|--------------|------|-------------|----------|
| 2026-03-27 22:52 | Documentation scaffold | Create dedicated payroll module doc | `Docs/Payroll/Payroll_Module.md` | Added module documentation shell | Low | N/A | Review module before implementation |

## Validation / Testing
- [ ] salary structure create/read
- [ ] payroll run
- [ ] payslip view
- [ ] mark paid

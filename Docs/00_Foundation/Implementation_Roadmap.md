# Implementation Roadmap

## Objective
Upgrade the existing EMS backend from a solid MVP foundation to a **mini-enterprise grade backend** with:
- stronger Identity/Auth,
- scalable authorization,
- hierarchy and scope enforcement,
- documentation-first delivery,
- step-by-step module integration with frontend.

---

## Roadmap Phases

### Phase 0 — Documentation Foundation
**Goal:** Create reusable documentation structure before major code changes.

Deliverables:
- documentation convention
- auth module design doc
- implementation roadmap
- module-wise doc scaffolds

Exit Criteria:
- every module has a dedicated documentation home
- auth module has a structured markdown design document

---

### Phase 1 — Identity/Auth Hardening
**Goal:** Make authentication and authorization safe enough for further module work.

Primary tasks:
- remove privileged public self-registration risk
- standardize auth responses
- add proper authorization registration and policies
- stop trusting arbitrary self-related IDs from request when JWT identity is available
- define permission and scope model

Exit Criteria:
- login flow is stable
- auth risk list reduced
- frontend login can safely connect

---

### Phase 2 — Refresh Token + Session Lifecycle
**Goal:** Add session continuity and safer token lifecycle.

Primary tasks:
- refresh token entity
- refresh endpoint
- logout / revoke flow
- token rotation strategy

Exit Criteria:
- access token is short-lived
- refresh token is stored and revocable

---

### Phase 3 — Role + Permission Infrastructure
**Goal:** Move from enum-only authorization toward scalable authorization.

Primary tasks:
- roles table / entity
- permissions table / entity
- role-permission mapping
- permission resolution service

Exit Criteria:
- action permissions are defined centrally
- policies can evolve without role explosion

---

### Phase 4 — Scope + Hierarchy Enforcement
**Goal:** Enforce real access boundaries.

Scope model:
- `SELF`
- `TEAM`
- `DEPARTMENT`
- `COMPANY`
- `GLOBAL`

Primary tasks:
- self access checks
- team access checks using `ReportingManagerId`
- department/company level rules where needed

Exit Criteria:
- employee cannot read another employee's private data
- manager sees only valid team data

---

### Phase 5 — Module-by-Module Stabilization

Recommended order:
1. Identity/Auth
2. Organization (Departments, Designations)
3. Employees
4. Dashboard
5. Attendance
6. Leave
7. Payroll
8. Performance
9. Reports

For each module:
- backend audit/fix
- frontend integration
- API testing
- UI testing
- documentation update

---

### Phase 6 — Audit & Enterprise Readiness Enhancements
Future additions:
- audit logs
- rate limiting
- invite flow
- password reset
- holiday calendar
- export support
- integration tests

---

## Immediate Next Step
**Start with Identity/Auth implementation using the new documentation-first workflow.**

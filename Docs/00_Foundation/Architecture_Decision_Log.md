# Architecture Decision Log

## Purpose
This file records high-impact technical decisions so future work stays aligned.

---

| ADR | Title | Status | Decision | Why | Consequence |
|-----|-------|--------|----------|-----|-------------|
| ADR-001 | Documentation-first workflow | Accepted | Every meaningful backend module change must update its module markdown file | Improves learning, traceability, handover, and maintenance | Slight extra effort per change, but much better project clarity |
| ADR-002 | Hybrid authorization model | Accepted | Use Role + Permission + Scope + Hierarchy model for the future auth design | Role-only auth will not scale cleanly for real projects | Requires extra entities/services, but gives enterprise-style growth path |
| ADR-003 | No privileged public self-registration | Accepted | Public registration must never assign elevated roles | Prevents critical privilege escalation risk | User onboarding should move toward employee-only self-register or admin/invite flow |
| ADR-004 | Ownership checks belong in service layer | Accepted | Controller may do coarse policy check, but final self/team/company validation will happen in services/access-control layer | Services know actual resource relation and business context | Adds one more layer, but improves correctness and reusability |
| ADR-005 | Refresh token support required | Accepted | JWT access token alone is not enough for mini-enterprise auth | Better user experience and safer session lifecycle | Requires refresh token storage, rotation, revocation support |

---

## How To Use This Log
Add a new ADR whenever a decision changes:
- auth model,
- data model,
- API contract,
- security strategy,
- documentation workflow,
- frontend-backend integration strategy.

---
gsd_state_version: 1.0
milestone: v3.1
milestone_name: K8s & GitOps Hardening
status: executing
stopped_at: Completed 33-01-PLAN.md
last_updated: "2026-03-08T15:12:37.526Z"
last_activity: 2026-03-08 — Completed Phase 32 Plan 01 (Kustomize Hygiene)
progress:
  total_phases: 6
  completed_phases: 3
  total_plans: 6
  completed_plans: 5
  percent: 75
---

# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-03-08)

**Core value:** A user can complete a purchase end-to-end — deployed to Kubernetes via GitOps
**Current focus:** Phase 32 - Kustomize Hygiene

## Current Position

Phase: 32 (Kustomize Hygiene) — third of 6 in v3.1
Plan: 01 (complete)
Status: Executing
Last activity: 2026-03-08 — Completed Phase 32 Plan 01 (Kustomize Hygiene)

Progress: [████████░░] 75%

## Performance Metrics

**Velocity:**
- Total plans completed: 113 (v1.0: 49, v1.1: 23, v2.0: 9, v3.0: 29, v3.1: 3)

**By Milestone:**

| Milestone | Phases | Plans | Timeline |
|-----------|--------|-------|----------|
| v1.0 MVP | 10 | 49 | 16 days |
| v1.1 User Features | 7 | 23 | 2 days |
| v2.0 DDD Foundation | 9 | 9 | 11 days |
| v3.0 K8s & GitOps | 8 | 29 | 5 days |
| v3.1 Hardening | 6 | 3 | in progress |
| Phase 32 P02 | 2min | 2 tasks | 16 files |
| Phase 33 P01 | 1min | 1 tasks | 24 files |

## Accumulated Context

### Decisions

- [30-01] Aspire pinned via SDK package ref in csproj; CI does not install workload
- [30-01] Release workflow uses workflow_call test gate with needs: [tests]
- [30-01] Dockerfile uses ARG (not ENV) for build-time auth placeholders
- [31-01] Top-level permissions: {} on release.yml with job-level overrides for deny-by-default
- [31-01] Inclusive paths filter (not paths-ignore) for explicit allowlist of trigger paths
- [31-01] NuGet cache key uses both csproj and Directory.Packages.props hashes
- [32-01] Namespace set only via Kustomize transformer in kustomization.yaml, never in individual manifests
- [32-01] otel-collector and aspire-dashboard stay in base/ directory but referenced only from dev overlay
- [Phase 32]: Retained existing app: X labels alongside app.kubernetes.io/* for backward compatibility
- [Phase 32]: All containers use imagePullPolicy: IfNotPresent (no latest tags in use)
- [Phase 32]: Service selectors keep only app: X to avoid restrictive pod matching
- [Phase 33]: postgres uses runAsNonRoot: false because official image manages privilege dropping via gosu
- [Phase 33]: postgres/keycloak/rabbitmq use readOnlyRootFilesystem: false due to runtime write requirements
- [Phase 33]: securityContext at container level (not pod level) for granular control

### Pending Todos

None.

### Blockers/Concerns

- ~~CI workflows are currently broken (SDK mismatch, stale paths) — Phase 30 addresses this first~~ RESOLVED by 30-01

## Session Continuity

Last session: 2026-03-08T15:12:37.524Z
Stopped at: Completed 33-01-PLAN.md
Resume file: None
Next step: Next phase in v3.1 or `/gsd:plan-phase 33` for next phase

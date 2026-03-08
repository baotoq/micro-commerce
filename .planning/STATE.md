---
gsd_state_version: 1.0
milestone: v3.1
milestone_name: K8s & GitOps Hardening
status: executing
stopped_at: Completed 35-02-PLAN.md
last_updated: "2026-03-08T16:36:11.477Z"
last_activity: 2026-03-08 — Completed Phase 33 Plan 02 (Web Secrets & Keycloak Production Mode)
progress:
  total_phases: 6
  completed_phases: 6
  total_plans: 11
  completed_plans: 11
  percent: 83
---

# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-03-08)

**Core value:** A user can complete a purchase end-to-end — deployed to Kubernetes via GitOps
**Current focus:** Phase 33 - K8s Security Hardening

## Current Position

Phase: 33 (K8s Security Hardening) — fourth of 6 in v3.1
Plan: 02 (complete)
Status: Executing
Last activity: 2026-03-08 — Completed Phase 33 Plan 02 (Web Secrets & Keycloak Production Mode)

Progress: [█████████░] 83%

## Performance Metrics

**Velocity:**
- Total plans completed: 114 (v1.0: 49, v1.1: 23, v2.0: 9, v3.0: 29, v3.1: 4)

**By Milestone:**

| Milestone | Phases | Plans | Timeline |
|-----------|--------|-------|----------|
| v1.0 MVP | 10 | 49 | 16 days |
| v1.1 User Features | 7 | 23 | 2 days |
| v2.0 DDD Foundation | 9 | 9 | 11 days |
| v3.0 K8s & GitOps | 8 | 29 | 5 days |
| v3.1 Hardening | 6 | 4 | in progress |
| Phase 32 P02 | 2min | 2 tasks | 16 files |
| Phase 33 P01 | 1min | 1 tasks | 24 files |
| Phase 33 P02 | 1min | 2 tasks | 6 files |
| Phase 34 P01 | 1min | 1 tasks | 1 files |
| Phase 34 P03 | 2min | 1 tasks | 1 files |
| Phase 34 P02 | 2min | 2 tasks | 9 files |
| Phase 35 P01 | 2min | 2 tasks | 11 files |
| Phase 35 P02 | 2min | 2 tasks | 18 files |

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
- [33-02] Web secrets (AUTH_SECRET, KEYCLOAK_CLIENT_SECRET) use secretKeyRef to web-secrets SealedSecret
- [33-02] Keycloak base uses 'start' (production); dev overlay patches to 'start-dev' via strategic merge
- [Phase 34]: docker added to prerequisite checks alongside kind/kubectl/kubeseal since needed for image builds
- [Phase 34]: Context guard placed after cluster creation so kind sets context before verification
- [Phase 34]: Identical outbox config across all DbContexts (1s QueryDelay, 5min DuplicateDetectionWindow)
- [Phase 34]: NEXT_PUBLIC_API_URL returns empty string in Aspire (same-origin), http://localhost:38800 in K8s
- [Phase 34]: RabbitMQ uses StatefulSet with headless service and 1Gi PVC for message persistence
- [Phase 34]: Startup probe timeouts tuned per service: RabbitMQ 130s, gateway/web/aspire 65s, otel-collector 33s
- [Phase 35]: Sync wave ordering: postgres=1, rabbitmq/keycloak=2, apiservice=3, gateway=4, web=5, otel/aspire=6
- [Phase 35]: AppProject allows micro-commerce and argocd namespaces; clusterResourceWhitelist limited to Namespace kind
- [Phase 35]: ignoreDifferences on postgres and rabbitmq StatefulSets for volumeClaimTemplates drift prevention
- [Phase 35]: Keycloak ArgoCD app also updated from base/ to overlays/dev/ for consistency
- [Phase 35]: Placeholder sealed secrets use AgAAAA== dummy values, overwritten by bootstrap.sh per-cluster

### Pending Todos

None.

### Blockers/Concerns

- ~~CI workflows are currently broken (SDK mismatch, stale paths) — Phase 30 addresses this first~~ RESOLVED by 30-01

## Session Continuity

Last session: 2026-03-08T16:36:11.474Z
Stopped at: Completed 35-02-PLAN.md
Resume file: None
Next step: Next phase in v3.1 milestone

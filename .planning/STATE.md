---
gsd_state_version: 1.0
milestone: v1.0
milestone_name: Kubernetes & GitOps
status: phase-complete
last_updated: "2026-02-25T18:07:34.072Z"
progress:
  total_phases: 1
  completed_phases: 1
  total_plans: 3
  completed_plans: 3
---

# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-02-25)

**Core value:** A user can complete a purchase end-to-end — now deployed to Kubernetes via GitOps
**Current focus:** v3.0 Kubernetes & GitOps — Phase 23: Dockerfiles and Container Image Pipeline

## Current Position

Phase: 23 of 28 (Dockerfiles and Container Image Pipeline) -- COMPLETE
Plan: 3 of 3
Status: Phase Complete
Last activity: 2026-02-26 — Completed 23-03 (GitHub Actions Container Image Workflow)

Progress: [████████████████████░░░░░░░░░░] 71% (23/28 phases complete across all milestones — 1/6 v3.0 phases complete)

## Performance Metrics

**Velocity:**
- Total plans completed: 80 (v1.0: 49, v1.1: 23, v2.0: 8)
- Average duration: 22 min
- Total execution time: 28.47 hours

**By Milestone:**

| Milestone | Phases | Plans | Duration | Timeline |
|-----------|--------|-------|----------|----------|
| v1.0 MVP | 10 | 49 | 18.8h | 16 days |
| v1.1 User Features | 7 | 23 | 8.8h | 2 days |
| v2.0 DDD Foundation | 7 | 9 | 40 min | 11 days |
| v3.0 K8s & GitOps | 1/6 | 3/TBD | 6 min | In progress |
| Phase 23 P03 | 2min | 2 tasks | 1 file |
| Phase 23 P02 | 2min | 2 tasks | 3 files |
| Phase 23 P01 | 2min | 2 tasks | 3 files |

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table.

Recent decisions affecting v3.0:
- **Aspire + K8s parallel paths**: Aspire stays as local inner-loop dev tool; Kustomize + ArgoCD define cluster state. No conflict.
- **RabbitMQ as K8s transport**: Azure SB emulator cannot run in K8s; `MASSTRANSIT_TRANSPORT` env var switches transport. Same consumer/saga/outbox config.
- **Kustomize over Helm**: Simpler for single-project; no templating language; built into kubectl.
- **ArgoCD v3.3.2 specifically**: v3.3.0/3.3.1 have a client-side apply migration bug.
- **Sealed Secrets from day one**: No plaintext credentials in Git ever. Key backup in cluster bootstrap script.
- **NodePort for kind access**: Ingress NGINX EOL March 2026; NodePort is simplest for local kind cluster.
- **No MinIO**: Azurite not suitable for K8s; use placeholder images in K8s deployment.
- [Phase 23]: Next.js Dockerfile uses AUTH_SECRET=placeholder-for-docker-build to prevent next-auth v5 build failures; real secret injected at K8s runtime
- [Phase 23]: Use noble-chiseled-extra base image (not plain noble-chiseled) for .NET containers because InvariantGlobalization is not set
- [Phase 23]: Separate container-images.yml workflow from release.yml -- GITHUB_TOKEN for ghcr.io auth, .NET multi-arch via per-arch publish + imagetools merge

### Pending Todos

None.

### Blockers/Concerns

- **Keycloak realm import approach**: Operator `KeycloakRealmImport` CR runs once (IGNORE_EXISTING). May need upsert-via-Admin-API script for idempotent realm config. Resolve in Phase 24 planning.
- **MassTransit DLQ for RabbitMQ**: `DeadLetterQueueService` uses Azure SDK `ServiceBusClient`. Needs RabbitMQ-compatible implementation or graceful disable path. Most uncertain code change — research during Phase 25 planning.
- **CORS origins in YARP Gateway**: Currently `*` — must become an explicit allowed-origins list in the Kustomize overlay ConfigMap for the Gateway.

## Session Continuity

Last session: 2026-02-26
Stopped at: Completed 23-03-PLAN.md (GitHub Actions Container Image Workflow) -- Phase 23 complete
Resume file: None
Next step: Plan Phase 24 (Infrastructure Manifests and Secrets)

---
gsd_state_version: 1.0
milestone: v1.0
milestone_name: Kubernetes & GitOps
status: unknown
last_updated: "2026-02-26T10:06:47.204Z"
progress:
  total_phases: 3
  completed_phases: 3
  total_plans: 10
  completed_plans: 10
---

# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-02-25)

**Core value:** A user can complete a purchase end-to-end — now deployed to Kubernetes via GitOps
**Current focus:** v3.0 Kubernetes & GitOps — Phase 26: ArgoCD GitOps

## Current Position

Phase: 26 of 28 (ArgoCD GitOps)
Plan: 0 of TBD
Status: Not Started
Last activity: 2026-02-26 — Completed Phase 25 (Application Manifests and MassTransit Transport)

Progress: [█████████████████████████░░░░░] 83% (25/28 phases complete across all milestones — 3/6 v3.0 phases complete)

## Performance Metrics

**Velocity:**
- Total plans completed: 81 (v1.0: 49, v1.1: 23, v2.0: 8)
- Average duration: 22 min
- Total execution time: 28.47 hours

**By Milestone:**

| Milestone | Phases | Plans | Duration | Timeline |
|-----------|--------|-------|----------|----------|
| v1.0 MVP | 10 | 49 | 18.8h | 16 days |
| v1.1 User Features | 7 | 23 | 8.8h | 2 days |
| v2.0 DDD Foundation | 7 | 9 | 40 min | 11 days |
| v3.0 K8s & GitOps | 3/6 | 11/TBD | 19 min | In progress |
| Phase 23 P03 | 2min | 2 tasks | 1 file |
| Phase 23 P02 | 2min | 2 tasks | 3 files |
| Phase 23 P01 | 2min | 2 tasks | 3 files |
| Phase 24-01 P01 | 1min | 2 tasks | 7 files |
| Phase 24-02 P02 | 1min | 2 tasks | 7 files |
| Phase 24-03 P03 | 1min | 2 tasks | 1 file |
| Phase 24-04 P04 | 1min | 2 tasks | 2 files |
| Phase 25-01 P01 | 3min | 2 tasks | 5 files |
| Phase 25-02 P02 | 2min | 2 tasks | 12 files |
| Phase 25-03 P03 | 2min | 2 tasks | 1 file |

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
- [Phase 24]: [Phase 24-01]: PGDATA subdirectory avoids initdb directory-not-empty error from kind local-path-provisioner
- [Phase 24]: [Phase 24-01]: Offset ports (3xxxx) for kind cluster avoid conflicts with Aspire local dev services
- [Phase 24]: [Phase 24-01]: Separate headless + NodePort services for PostgreSQL -- headless for StatefulSet DNS, NodePort for kind port mapping chain
- [Phase 24]: [Phase 24-02]: Keycloak startup probe on management port 9000 with 15s initial delay and 60s probing window for slow first-boot realm import
- [Phase 24]: [Phase 24-02]: Realm JSON copied from AppHost/Realms to K8s manifests for self-contained deployment; configMapGenerator for hash-suffixed ConfigMap
- [Phase 24]: [Phase 24-02]: Separate NodePort services for kind external access; KC_BOOTSTRAP_ADMIN_USERNAME/PASSWORD (not deprecated KEYCLOAK_ADMIN)
- [Phase 24]: [Phase 24-03]: SealedSecrets v0.27.3 pinned for reproducible controller install; sealed-secret.yaml generated dynamically by bootstrap script
- [Phase 24]: [Phase 24-03]: seal_secret helper function centralizes kubeseal invocation; dev defaults postgres/guest/admin for local-only kind cluster
- [Phase 24]: [Phase 24-03]: Bootstrap script idempotent (skips existing cluster); Keycloak 180s pod wait timeout for realm import
- [Phase 24]: [Phase 24-04]: ROADMAP criterion #5 corrected from ApiService to Keycloak startup probe; ApiService probe deferred to Phase 25 criterion #6
- [Phase 25]: [Phase 25-01]: MASSTRANSIT_TRANSPORT env var controls transport: RabbitMQ for K8s, AzureServiceBus (default) for Aspire
- [Phase 25]: [Phase 25-01]: NoOp DLQ service returns empty results in RabbitMQ mode; health endpoints unconditional for K8s probes
- [Phase 25]: [Phase 25-01]: CORS origins configurable via Cors:Origins config section with localhost fallback
- [Phase 25]: [Phase 25-02]: Env var substitution pattern: define secretKeyRef env vars BEFORE connection strings so K8s $(VAR) substitution works
- [Phase 25]: [Phase 25-02]: Gateway dual Service: ClusterIP for internal, NodePort 30800 for kind host access at 38800
- [Phase 25]: [Phase 25-02]: Web uses plaintext dev secrets matching Phase 23 Dockerfile approach; production would use SealedSecrets
- [Phase 25]: [Phase 25-02]: ApiService startup probe 160s window (10s initial + 30x5s) for EF Core migrations on first boot
- [Phase 25]: [Phase 25-03]: Infrastructure-first bootstrap: Step 7 applies base/ for infra, Step 11 applies overlays/dev/ for full stack (idempotent)
- [Phase 25]: [Phase 25-03]: ContainerRegistry='' overrides ghcr.io default for local-only kind images; dotnet publish /t:PublishContainer for .NET, docker build for Web

### Pending Todos

None.

### Roadmap Evolution

- Phase 25.1 inserted after Phase 25: Update UI based on @design/*.pen design with shadcn (URGENT)

### Blockers/Concerns

- **Keycloak realm import approach**: Operator `KeycloakRealmImport` CR runs once (IGNORE_EXISTING). May need upsert-via-Admin-API script for idempotent realm config. Resolve in Phase 24 planning.
- **MassTransit DLQ for RabbitMQ**: `DeadLetterQueueService` uses Azure SDK `ServiceBusClient`. Needs RabbitMQ-compatible implementation or graceful disable path. Most uncertain code change — research during Phase 25 planning.
- **CORS origins in YARP Gateway**: Currently `*` — must become an explicit allowed-origins list in the Kustomize overlay ConfigMap for the Gateway.

## Session Continuity

Last session: 2026-02-26
Stopped at: Completed Phase 25 (Application Manifests and MassTransit Transport) - all 3 plans done
Resume file: None
Next step: Plan Phase 26 (ArgoCD GitOps)

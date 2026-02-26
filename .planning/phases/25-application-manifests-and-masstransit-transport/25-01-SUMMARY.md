---
phase: 25-application-manifests-and-masstransit-transport
plan: 01
subsystem: infra
tags: [masstransit, rabbitmq, azure-service-bus, health-checks, cors, keycloak, k8s]

# Dependency graph
requires:
  - phase: 24-infrastructure-manifests-and-secrets
    provides: K8s base manifests, sealed secrets, bootstrap script
provides:
  - Conditional MassTransit transport switching (RabbitMQ / Azure Service Bus)
  - NoOpDeadLetterQueueService for non-Azure environments
  - Unconditional health endpoints for K8s liveness/readiness probes
  - Configurable CORS origins in Gateway
  - Unconditional Keycloak HTTP metadata in ApiService and Gateway
affects: [25-02-PLAN, 25-03-PLAN, k8s-manifests, deployment]

# Tech tracking
tech-stack:
  added: [MassTransit.RabbitMQ 9.0.0]
  patterns: [env-var-driven transport switching, conditional service registration, no-op service pattern]

key-files:
  created: []
  modified:
    - src/MicroCommerce.ApiService/MicroCommerce.ApiService.csproj
    - src/MicroCommerce.ApiService/Program.cs
    - src/MicroCommerce.ApiService/Common/Messaging/DeadLetterQueueService.cs
    - src/MicroCommerce.ServiceDefaults/Extensions.cs
    - src/MicroCommerce.Gateway/Program.cs

key-decisions:
  - "MASSTRANSIT_TRANSPORT env var controls transport: RabbitMQ for K8s, AzureServiceBus (default) for Aspire"
  - "NoOp DLQ service returns empty results instead of throwing in RabbitMQ mode"
  - "Health endpoints unconditional for K8s probes (ClusterIP only, not public)"
  - "Keycloak RequireHttpsMetadata=false unconditional for kind cluster HTTP access"
  - "CORS origins configurable via Cors:Origins config section with localhost fallback"

patterns-established:
  - "Env-var transport switching: read MASSTRANSIT_TRANSPORT, branch once, all consumers/sagas/outbox unchanged"
  - "No-op service pattern: IDeadLetterQueueService has NoOp impl for non-Azure environments"
  - "Conditional Azure SDK: blobs/messaging only registered when Azure Service Bus is the transport"

requirements-completed: [TRAN-01, TRAN-02, MFST-02]

# Metrics
duration: 3min
completed: 2026-02-26
---

# Phase 25 Plan 01: Application Manifests and MassTransit Transport Summary

**MassTransit dual-transport switching via MASSTRANSIT_TRANSPORT env var, unconditional K8s health probes, and configurable Gateway CORS**

## Performance

- **Duration:** 3 min
- **Started:** 2026-02-26T09:51:16Z
- **Completed:** 2026-02-26T09:54:02Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments
- MassTransit transport switching: MASSTRANSIT_TRANSPORT=RabbitMQ uses UsingRabbitMq, default uses UsingAzureServiceBus
- Azure SDK registrations (blobs, messaging) are conditional -- skipped in RabbitMQ mode to avoid missing connection string errors
- NoOpDeadLetterQueueService returns empty results gracefully for RabbitMQ environments
- Health endpoints /health and /alive registered unconditionally for K8s liveness/readiness probes
- CORS origins configurable via Cors:Origins configuration section (supports K8s env var injection)
- Keycloak RequireHttpsMetadata=false unconditional in both ApiService and Gateway

## Task Commits

Each task was committed atomically:

1. **Task 1: Add MassTransit.RabbitMQ package and implement conditional transport switching** - `d3f7ee5a` (feat)
2. **Task 2: Fix health endpoints and Gateway Keycloak for K8s environments** - `e8dcfb83` (feat)

## Files Created/Modified
- `src/MicroCommerce.ApiService/MicroCommerce.ApiService.csproj` - Added MassTransit.RabbitMQ 9.0.0 package reference
- `src/MicroCommerce.ApiService/Program.cs` - Conditional transport switching, conditional Azure SDK, conditional DLQ service, unconditional RequireHttpsMetadata
- `src/MicroCommerce.ApiService/Common/Messaging/DeadLetterQueueService.cs` - Added NoOpDeadLetterQueueService class
- `src/MicroCommerce.ServiceDefaults/Extensions.cs` - Removed IsDevelopment guard from MapDefaultEndpoints health checks
- `src/MicroCommerce.Gateway/Program.cs` - Unconditional RequireHttpsMetadata, configurable CORS origins

## Decisions Made
- **MASSTRANSIT_TRANSPORT default is AzureServiceBus**: Preserves existing Aspire local dev workflow without any env var changes
- **NoOp DLQ over throwing**: RabbitMQ has its own DLQ mechanism; returning empty results is safer than disabling the endpoints
- **Health endpoints unconditional**: K8s probes need /health and /alive; these run behind ClusterIP, not publicly exposed
- **RequireHttpsMetadata=false everywhere**: Kind cluster and Aspire both run Keycloak over HTTP; this is dev-only deployment
- **CORS origins from config with fallback**: Allows K8s manifests to inject origins via Cors__Origins__0 env vars

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Application code now supports dual-environment execution (Aspire + K8s)
- Ready for K8s application manifest creation (Plan 02) with proper health probes, configurable CORS, and RabbitMQ transport
- All 182 existing tests pass -- transport switch is transparent to MassTransit TestHarness

## Self-Check: PASSED

All 5 modified files verified on disk. Both task commits (d3f7ee5a, e8dcfb83) verified in git log.

---
*Phase: 25-application-manifests-and-masstransit-transport*
*Completed: 2026-02-26*

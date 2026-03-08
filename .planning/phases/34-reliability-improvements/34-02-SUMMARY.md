---
phase: 34-reliability-improvements
plan: 02
subsystem: infra
tags: [kubernetes, rabbitmq, statefulset, startup-probes, api-url]

requires:
  - phase: 33-k8s-security-hardening
    provides: K8s manifests with security contexts and secrets
provides:
  - Client-side API URL resolution via NEXT_PUBLIC_API_URL
  - RabbitMQ StatefulSet with persistent storage (1Gi PVC)
  - Startup probes on gateway, web, rabbitmq, otel-collector, aspire-dashboard
affects: [kubernetes-deployment, k8s-overlays]

tech-stack:
  added: []
  patterns: [statefulset-for-stateful-services, startup-probes-for-slow-containers, client-vs-server-url-separation]

key-files:
  created:
    - infra/k8s/base/rabbitmq/statefulset.yaml
  modified:
    - src/MicroCommerce.Web/src/lib/config.ts
    - src/MicroCommerce.Web/src/app/api/config/route.ts
    - infra/k8s/base/web/deployment.yaml
    - infra/k8s/base/rabbitmq/service.yaml
    - infra/k8s/base/rabbitmq/kustomization.yaml
    - infra/k8s/base/gateway/deployment.yaml
    - infra/k8s/base/otel-collector/deployment.yaml
    - infra/k8s/base/aspire-dashboard/deployment.yaml

key-decisions:
  - "NEXT_PUBLIC_API_URL returns empty string in Aspire (same-origin), http://localhost:38800 in K8s"
  - "RabbitMQ uses StatefulSet with headless service and 1Gi PVC for message persistence"
  - "Startup probe timeouts tuned per service: RabbitMQ 130s, gateway/web/aspire 65s, otel-collector 33s"

patterns-established:
  - "Client-vs-server URL separation: getApiBaseUrl() for SSR, getClientApiBaseUrl() for browser"
  - "StatefulSet pattern for stateful workloads requiring persistent storage"

requirements-completed: [REL-01, REL-02, REL-03]

duration: 2min
completed: 2026-03-08
---

# Phase 34 Plan 02: Reliability Improvements Summary

**Client-side API URL fix via NEXT_PUBLIC_API_URL, RabbitMQ StatefulSet with 1Gi PVC, and startup probes on 5 K8s workloads**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-08T16:15:40Z
- **Completed:** 2026-03-08T16:17:12Z
- **Tasks:** 2
- **Files modified:** 9

## Accomplishments
- Fixed client-side API calls by separating browser-reachable URL (NEXT_PUBLIC_API_URL) from cluster-internal URL (services__gateway)
- Converted RabbitMQ from Deployment to StatefulSet with 1Gi PersistentVolumeClaim for message durability across restarts
- Added startup probes to gateway, web, rabbitmq, otel-collector, and aspire-dashboard with service-appropriate timeouts

## Task Commits

Each task was committed atomically:

1. **Task 1: Fix client-side API URL and convert RabbitMQ to StatefulSet** - `7fd100ea` (feat)
2. **Task 2: Add startup probes to 5 workloads** - `63004bac` (feat)

## Files Created/Modified
- `src/MicroCommerce.Web/src/lib/config.ts` - Added getClientApiBaseUrl() for browser-reachable URL
- `src/MicroCommerce.Web/src/app/api/config/route.ts` - Uses getClientApiBaseUrl() instead of getApiBaseUrl()
- `infra/k8s/base/web/deployment.yaml` - Added NEXT_PUBLIC_API_URL env var and startup probe
- `infra/k8s/base/rabbitmq/statefulset.yaml` - New StatefulSet with PVC and startup probe (replaces deployment.yaml)
- `infra/k8s/base/rabbitmq/service.yaml` - Added clusterIP: None for headless service
- `infra/k8s/base/rabbitmq/kustomization.yaml` - Updated resource reference to statefulset.yaml
- `infra/k8s/base/gateway/deployment.yaml` - Added startup probe (HTTP /health, 65s)
- `infra/k8s/base/otel-collector/deployment.yaml` - Added startup probe (HTTP /, 33s)
- `infra/k8s/base/aspire-dashboard/deployment.yaml` - Added startup probe (HTTP /, 65s)

## Decisions Made
- NEXT_PUBLIC_API_URL returns empty string in Aspire (same-origin fetch), http://localhost:38800 in K8s (NodePort)
- RabbitMQ uses StatefulSet with headless service (clusterIP: None) and 1Gi PVC at /var/lib/rabbitmq
- Startup probe timeouts tuned per service startup characteristics: RabbitMQ 130s (Erlang/mnesia), gateway/web/aspire 65s, otel-collector 33s

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- K8s deployment is now reliable across pod restarts with persistent RabbitMQ storage
- Client-side API calls work correctly in both Aspire and K8s environments
- All services have appropriate startup probes preventing premature liveness/readiness checks

## Self-Check: PASSED

All 10 files verified present. Both task commits (7fd100ea, 63004bac) verified in git log.

---
*Phase: 34-reliability-improvements*
*Completed: 2026-03-08*

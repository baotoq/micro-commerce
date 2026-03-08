---
phase: 32-kustomize-hygiene
plan: 02
subsystem: infra
tags: [kubernetes, kustomize, labels, imagePullPolicy]

# Dependency graph
requires:
  - phase: 32-kustomize-hygiene-01
    provides: Namespace-scoped base manifests for Kustomize overlays
provides:
  - Standard app.kubernetes.io/* labels on all workloads and services
  - Explicit imagePullPolicy on all container specs
affects: [kustomize-overlays, monitoring, argocd]

# Tech tracking
tech-stack:
  added: []
  patterns: [kubernetes-standard-labels, explicit-image-pull-policy]

key-files:
  created: []
  modified:
    - infra/k8s/base/apiservice/deployment.yaml
    - infra/k8s/base/gateway/deployment.yaml
    - infra/k8s/base/web/deployment.yaml
    - infra/k8s/base/keycloak/deployment.yaml
    - infra/k8s/base/otel-collector/deployment.yaml
    - infra/k8s/base/aspire-dashboard/deployment.yaml
    - infra/k8s/base/rabbitmq/deployment.yaml
    - infra/k8s/base/postgres/statefulset.yaml
    - infra/k8s/base/apiservice/service.yaml
    - infra/k8s/base/gateway/service.yaml
    - infra/k8s/base/web/service.yaml
    - infra/k8s/base/keycloak/service.yaml
    - infra/k8s/base/otel-collector/service.yaml
    - infra/k8s/base/aspire-dashboard/service.yaml
    - infra/k8s/base/rabbitmq/service.yaml
    - infra/k8s/base/postgres/service.yaml

key-decisions:
  - "Retained existing app: X labels alongside new app.kubernetes.io/* labels for backward compatibility"
  - "Used IfNotPresent for all images including local kind builds and pinned registry images"
  - "Kept service selectors using only app: X to avoid unnecessarily restrictive matching"

patterns-established:
  - "Standard labels: all K8s resources carry app, app.kubernetes.io/name, component, part-of"
  - "imagePullPolicy: all containers explicitly declare IfNotPresent"

requirements-completed: [KUST-03, KUST-04]

# Metrics
duration: 2min
completed: 2026-03-08
---

# Phase 32 Plan 02: Labels and ImagePullPolicy Summary

**Standard app.kubernetes.io/* labels and explicit imagePullPolicy: IfNotPresent across all 8 workloads and 13 service resources**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-08T14:55:25Z
- **Completed:** 2026-03-08T14:57:52Z
- **Tasks:** 2
- **Files modified:** 16

## Accomplishments
- Added app.kubernetes.io/name, app.kubernetes.io/component, and app.kubernetes.io/part-of labels to all 8 workloads (deployments + statefulset) in metadata, selector, and template positions
- Added explicit imagePullPolicy: IfNotPresent to all 8 container specs
- Added standard labels to all 13 service resources (8 ClusterIP + 5 NodePort) in metadata.labels
- Maintained backward compatibility by retaining existing app: X labels

## Task Commits

Each task was committed atomically:

1. **Task 1: Add standard Kubernetes labels and imagePullPolicy to all workloads** - `5fe2ae5c` (feat)
2. **Task 2: Update service selectors to match new label scheme** - `17405c06` (feat)

## Files Created/Modified
- `infra/k8s/base/apiservice/deployment.yaml` - Standard labels + imagePullPolicy for API service
- `infra/k8s/base/gateway/deployment.yaml` - Standard labels + imagePullPolicy for YARP gateway
- `infra/k8s/base/web/deployment.yaml` - Standard labels + imagePullPolicy for Next.js frontend
- `infra/k8s/base/keycloak/deployment.yaml` - Standard labels + imagePullPolicy for Keycloak auth
- `infra/k8s/base/otel-collector/deployment.yaml` - Standard labels + imagePullPolicy for OTel collector
- `infra/k8s/base/aspire-dashboard/deployment.yaml` - Standard labels + imagePullPolicy for Aspire dashboard
- `infra/k8s/base/rabbitmq/deployment.yaml` - Standard labels + imagePullPolicy for RabbitMQ
- `infra/k8s/base/postgres/statefulset.yaml` - Standard labels + imagePullPolicy for PostgreSQL
- `infra/k8s/base/*/service.yaml` - Standard metadata labels for all 13 service resources

## Decisions Made
- Retained existing `app: X` labels alongside new `app.kubernetes.io/*` labels for backward compatibility with existing scripts and ArgoCD selectors
- Used `IfNotPresent` for all images (local kind images and pinned registry images) since all use specific tags, not `latest`
- Kept service `spec.selector` using only `app: X` to avoid unnecessarily restrictive pod matching

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All workloads now have consistent standard Kubernetes labels for monitoring and tooling integration
- Explicit imagePullPolicy prevents ambiguity in kind clusters
- Ready for overlay configuration and further Kustomize hygiene

---
*Phase: 32-kustomize-hygiene*
*Completed: 2026-03-08*

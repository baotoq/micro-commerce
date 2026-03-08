---
phase: 32-kustomize-hygiene
plan: 01
subsystem: infra
tags: [kustomize, kubernetes, namespace, overlay, gitops]

# Dependency graph
requires:
  - phase: 29-k8s-manifests
    provides: Base Kubernetes manifests for all workloads
provides:
  - Clean base manifests without hardcoded namespaces
  - Dev overlay with otel-collector and aspire-dashboard resources
  - Production-ready base kustomization (no dev-only tools)
affects: [33-argocd, production-overlays]

# Tech tracking
tech-stack:
  added: []
  patterns: [kustomize-namespace-transformer, overlay-based-environment-separation]

key-files:
  created: []
  modified:
    - infra/k8s/base/kustomization.yaml
    - infra/k8s/base/keycloak/deployment.yaml
    - infra/k8s/base/rabbitmq/deployment.yaml
    - infra/k8s/base/postgres/statefulset.yaml
    - infra/k8s/base/rabbitmq/service.yaml
    - infra/k8s/base/keycloak/service.yaml
    - infra/k8s/base/postgres/service.yaml
    - infra/k8s/overlays/dev/kustomization.yaml
    - infra/k8s/overlays/dev/apiservice/kustomization.yaml
    - infra/k8s/overlays/dev/gateway/kustomization.yaml
    - infra/k8s/overlays/dev/web/kustomization.yaml

key-decisions:
  - "Namespace set only via Kustomize transformer in kustomization.yaml, never in individual manifests"
  - "otel-collector and aspire-dashboard stay in base/ directory but referenced only from dev overlay"

patterns-established:
  - "Kustomize namespace convention: namespace transformer in kustomization.yaml, not hardcoded in resources"
  - "Environment separation: dev-only tools referenced from overlay, not base"

requirements-completed: [KUST-01, KUST-02]

# Metrics
duration: 1min
completed: 2026-03-08
---

# Phase 32 Plan 01: Kustomize Hygiene Summary

**Removed hardcoded namespaces from base manifests and moved otel-collector/aspire-dashboard to dev overlay**

## Performance

- **Duration:** 1 min
- **Started:** 2026-03-08T14:55:32Z
- **Completed:** 2026-03-08T14:56:48Z
- **Tasks:** 2
- **Files modified:** 11

## Accomplishments
- Removed all hardcoded `namespace: micro-commerce` from base deployment, service, and statefulset manifests (9 occurrences across 6 files)
- Removed redundant namespace transformer from 3 overlay sub-kustomizations
- Moved otel-collector and aspire-dashboard resource references from base to dev overlay
- Base kustomization is now production-ready without dev-only observability tools

## Task Commits

Each task was committed atomically:

1. **Task 1: Remove hardcoded namespace fields from all base manifests** - `71085174` (chore)
2. **Task 2: Move otel-collector and aspire-dashboard from base to dev overlay** - `1e5fae5f` (chore)

## Files Created/Modified
- `infra/k8s/base/keycloak/deployment.yaml` - Removed hardcoded namespace
- `infra/k8s/base/rabbitmq/deployment.yaml` - Removed hardcoded namespace
- `infra/k8s/base/postgres/statefulset.yaml` - Removed hardcoded namespace
- `infra/k8s/base/rabbitmq/service.yaml` - Removed 2 hardcoded namespaces
- `infra/k8s/base/keycloak/service.yaml` - Removed 2 hardcoded namespaces
- `infra/k8s/base/postgres/service.yaml` - Removed 2 hardcoded namespaces
- `infra/k8s/base/kustomization.yaml` - Removed otel-collector and aspire-dashboard resources
- `infra/k8s/overlays/dev/kustomization.yaml` - Added otel-collector and aspire-dashboard resources
- `infra/k8s/overlays/dev/apiservice/kustomization.yaml` - Removed redundant namespace
- `infra/k8s/overlays/dev/gateway/kustomization.yaml` - Removed redundant namespace
- `infra/k8s/overlays/dev/web/kustomization.yaml` - Removed redundant namespace

## Decisions Made
- Namespace set only via Kustomize transformer in kustomization.yaml, never in individual manifests
- otel-collector and aspire-dashboard directories remain in base/ but are only referenced from the dev overlay

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Base manifests follow Kustomize best practices for namespace management
- Ready for production overlay creation (base has no dev-only dependencies)
- ArgoCD can target base or overlays cleanly

---
*Phase: 32-kustomize-hygiene*
*Completed: 2026-03-08*

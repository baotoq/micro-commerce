---
phase: 24-infrastructure-manifests-and-secrets
plan: 01
subsystem: infra
tags: [kubernetes, kind, kustomize, postgresql, statefulset, pvc]

# Dependency graph
requires:
  - phase: 23-dockerfiles-and-container-image-pipeline
    provides: Container images for application services
provides:
  - kind cluster configuration with offset port mappings (35432/35672/38080)
  - Kustomize base/overlay directory scaffold
  - PostgreSQL StatefulSet with PVC-backed persistent storage
  - Headless and NodePort Services for PostgreSQL
  - micro-commerce namespace manifest
affects: [24-02-PLAN, 24-03-PLAN, 25-application-k8s-manifests]

# Tech tracking
tech-stack:
  added: [kind, kustomize]
  patterns: [kustomize-base-overlay, statefulset-with-pvc, nodeport-kind-portmapping]

key-files:
  created:
    - infra/k8s/kind-config.yaml
    - infra/k8s/base/kustomization.yaml
    - infra/k8s/base/namespace.yaml
    - infra/k8s/base/postgres/kustomization.yaml
    - infra/k8s/base/postgres/statefulset.yaml
    - infra/k8s/base/postgres/service.yaml
    - infra/k8s/overlays/dev/kustomization.yaml
  modified: []

key-decisions:
  - "Single control-plane node for kind cluster (no workers needed for dev)"
  - "Offset ports (3xxxx) to avoid conflicts with Aspire local dev services"
  - "PGDATA subdirectory (/var/lib/postgresql/data/pgdata) to avoid initdb directory-not-empty error"
  - "Separate headless + NodePort services for PostgreSQL (headless for StatefulSet DNS, NodePort for local access)"
  - "sealed-secret.yaml pre-referenced in postgres kustomization for bootstrap script generation in Plan 03"

patterns-established:
  - "Kustomize base/overlay: base/ contains all infrastructure, overlays/dev/ references base"
  - "Port mapping chain: host (3xxxx) -> kind container (3xxxx) -> K8s NodePort (3xxxx) -> pod (standard port)"
  - "StatefulSet pattern: headless Service + volumeClaimTemplates + PGDATA subdirectory"
  - "Credentials via Secret: never plaintext in manifests, secretKeyRef to named Secret"

requirements-completed: [INFRA-04, INFRA-01]

# Metrics
duration: 1min
completed: 2026-02-26
---

# Phase 24 Plan 01: Kind Cluster Config and PostgreSQL Manifests Summary

**Kind cluster config with 3 offset port mappings, Kustomize base/overlay scaffold, and PostgreSQL StatefulSet with 1Gi PVC and headless + NodePort Services**

## Performance

- **Duration:** 1 min
- **Started:** 2026-02-26T08:40:25Z
- **Completed:** 2026-02-26T08:41:47Z
- **Tasks:** 2
- **Files created:** 7

## Accomplishments
- kind cluster configuration with single control-plane node and 3 port mappings for PostgreSQL (35432), RabbitMQ (35672), and Keycloak (38080)
- Kustomize base/overlay directory structure with root kustomization referencing namespace, postgres, rabbitmq, and keycloak subdirectories
- PostgreSQL StatefulSet with 1Gi PersistentVolumeClaim, PGDATA subdirectory, credentials from Secret, readiness/liveness probes, and resource limits
- Headless Service for StatefulSet stable DNS and NodePort Service (30432) aligned with kind port mapping chain

## Task Commits

Each task was committed atomically:

1. **Task 1: Create kind cluster config and Kustomize directory scaffold** - `f83b8c47` (feat)
2. **Task 2: Create PostgreSQL StatefulSet and Service manifests** - `233f9e39` (feat)

## Files Created/Modified
- `infra/k8s/kind-config.yaml` - kind cluster with control-plane node and 3 extraPortMappings
- `infra/k8s/base/kustomization.yaml` - Root Kustomize base referencing namespace + 3 infrastructure subdirectories
- `infra/k8s/base/namespace.yaml` - micro-commerce namespace definition
- `infra/k8s/base/postgres/kustomization.yaml` - PostgreSQL Kustomize resources (statefulset, service, sealed-secret)
- `infra/k8s/base/postgres/statefulset.yaml` - PostgreSQL StatefulSet with PVC, probes, resource limits
- `infra/k8s/base/postgres/service.yaml` - Headless Service + NodePort Service for PostgreSQL
- `infra/k8s/overlays/dev/kustomization.yaml` - Dev overlay referencing base

## Decisions Made
- Single control-plane node only (no separate workers) -- simplest for local dev kind cluster
- Offset ports (3xxxx range) to avoid conflicts with Aspire local dev services running on standard ports
- PGDATA set to subdirectory `/var/lib/postgresql/data/pgdata` to avoid initdb "directory is not empty" error from kind's local-path-provisioner
- Separate headless + NodePort services rather than combining -- headless required for StatefulSet, NodePort required for kind port mapping chain
- Pre-referenced `sealed-secret.yaml` in postgres kustomization even though it does not exist yet -- bootstrap script (Plan 03) will generate it before `kubectl apply -k`

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Directory structure ready for Plan 02 to add RabbitMQ and Keycloak manifests in `infra/k8s/base/rabbitmq/` and `infra/k8s/base/keycloak/`
- PostgreSQL sealed-secret.yaml will be generated by Plan 03 bootstrap script
- Port mapping chain verified: host 35432 -> kind container 30432 -> K8s NodePort 30432 -> pod 5432

## Self-Check: PASSED

All 7 created files verified on disk. Both task commits (f83b8c47, 233f9e39) found in git log.

---
*Phase: 24-infrastructure-manifests-and-secrets*
*Completed: 2026-02-26*

---
phase: 33-k8s-security-hardening
plan: 01
subsystem: infra
tags: [kubernetes, security, securityContext, serviceaccount, least-privilege]

requires:
  - phase: 32-kustomize-hygiene
    provides: Clean Kustomize manifests with consistent labels and structure
provides:
  - Container-level securityContext on all 8 K8s workloads
  - Dedicated ServiceAccounts with automountServiceAccountToken disabled
  - Least-privilege security posture (non-root, read-only FS, no capabilities)
affects: [34-k8s-network-policies, 35-k8s-rbac]

tech-stack:
  added: []
  patterns: [container-level-securityContext, dedicated-serviceaccounts, least-privilege]

key-files:
  created:
    - infra/k8s/base/apiservice/serviceaccount.yaml
    - infra/k8s/base/gateway/serviceaccount.yaml
    - infra/k8s/base/web/serviceaccount.yaml
    - infra/k8s/base/keycloak/serviceaccount.yaml
    - infra/k8s/base/postgres/serviceaccount.yaml
    - infra/k8s/base/rabbitmq/serviceaccount.yaml
    - infra/k8s/base/otel-collector/serviceaccount.yaml
    - infra/k8s/base/aspire-dashboard/serviceaccount.yaml
  modified:
    - infra/k8s/base/apiservice/deployment.yaml
    - infra/k8s/base/gateway/deployment.yaml
    - infra/k8s/base/web/deployment.yaml
    - infra/k8s/base/keycloak/deployment.yaml
    - infra/k8s/base/postgres/statefulset.yaml
    - infra/k8s/base/rabbitmq/deployment.yaml
    - infra/k8s/base/otel-collector/deployment.yaml
    - infra/k8s/base/aspire-dashboard/deployment.yaml
    - infra/k8s/base/apiservice/kustomization.yaml
    - infra/k8s/base/gateway/kustomization.yaml
    - infra/k8s/base/web/kustomization.yaml
    - infra/k8s/base/keycloak/kustomization.yaml
    - infra/k8s/base/postgres/kustomization.yaml
    - infra/k8s/base/rabbitmq/kustomization.yaml
    - infra/k8s/base/otel-collector/kustomization.yaml
    - infra/k8s/base/aspire-dashboard/kustomization.yaml

key-decisions:
  - "postgres uses runAsNonRoot: false because official postgres image manages privilege dropping internally via gosu"
  - "postgres, keycloak, rabbitmq use readOnlyRootFilesystem: false due to runtime write requirements (data dirs, caches, mnesia)"
  - "securityContext placed at container level (not pod level) for granular per-container control"

patterns-established:
  - "Container securityContext pattern: runAsNonRoot + readOnlyRootFilesystem + allowPrivilegeEscalation: false + drop ALL capabilities"
  - "Dedicated ServiceAccount per workload with automountServiceAccountToken: false"

requirements-completed: [SEC-02, SEC-04]

duration: 1min
completed: 2026-03-08
---

# Phase 33 Plan 01: K8s Security Hardening Summary

**Security contexts and dedicated service accounts added to all 8 K8s workloads with least-privilege defaults and capability dropping**

## Performance

- **Duration:** 1 min
- **Started:** 2026-03-08T15:10:35Z
- **Completed:** 2026-03-08T15:11:48Z
- **Tasks:** 1
- **Files modified:** 24

## Accomplishments
- All 8 workloads hardened with container-level securityContext (runAsNonRoot, readOnlyRootFilesystem, allowPrivilegeEscalation: false, drop ALL capabilities)
- 8 dedicated ServiceAccounts created with automountServiceAccountToken: false
- Appropriate exceptions applied for postgres (runAsNonRoot: false, readOnlyRootFilesystem: false), keycloak (readOnlyRootFilesystem: false), and rabbitmq (readOnlyRootFilesystem: false)
- Kustomize build verified passing with all changes

## Task Commits

Each task was committed atomically:

1. **Task 1: Add securityContext to all 8 workloads and create dedicated ServiceAccounts** - `37f52d5f` (feat)

## Files Created/Modified
- `infra/k8s/base/*/serviceaccount.yaml` - Dedicated ServiceAccount per workload with automountServiceAccountToken: false
- `infra/k8s/base/*/deployment.yaml` - Added serviceAccountName and container-level securityContext
- `infra/k8s/base/postgres/statefulset.yaml` - Added serviceAccountName and securityContext (non-root exception)
- `infra/k8s/base/*/kustomization.yaml` - Added serviceaccount.yaml to resources list

## Decisions Made
- postgres uses runAsNonRoot: false because the official postgres image manages privilege dropping internally via gosu
- postgres, keycloak, and rabbitmq use readOnlyRootFilesystem: false due to runtime write requirements (data directories, theme caches, mnesia database)
- securityContext placed at container level (not pod level) for granular per-container control

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All workloads now have dedicated ServiceAccounts, ready for RBAC policies if needed
- Security contexts established, ready for NetworkPolicy hardening in subsequent phases

---
*Phase: 33-k8s-security-hardening*
*Completed: 2026-03-08*

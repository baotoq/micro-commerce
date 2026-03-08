---
phase: 35-argocd-gitops-best-practices
plan: 02
subsystem: infra
tags: [kustomize, argocd, sealed-secrets, gitops, kubernetes]

requires:
  - phase: 32-k8s-manifest-consistency
    provides: Base Kustomize structure with overlays for apiservice, gateway, keycloak, web
provides:
  - Dev overlay directories for postgres, rabbitmq, otel-collector, aspire-dashboard
  - Placeholder sealed-secret.yaml files committed to Git for declarative ArgoCD deployment
  - All 8 ArgoCD Application manifests pointing to overlays/dev/ paths consistently
affects: [argocd-apps, bootstrap-script, kustomize-overlays]

tech-stack:
  added: []
  patterns: [kustomize-overlay-per-service, sealed-secret-placeholder-pattern]

key-files:
  created:
    - infra/k8s/base/postgres/sealed-secret.yaml
    - infra/k8s/base/rabbitmq/sealed-secret.yaml
    - infra/k8s/base/keycloak/sealed-secret.yaml
    - infra/k8s/base/web/sealed-secret.yaml
    - infra/k8s/overlays/dev/postgres/kustomization.yaml
    - infra/k8s/overlays/dev/rabbitmq/kustomization.yaml
    - infra/k8s/overlays/dev/otel-collector/kustomization.yaml
    - infra/k8s/overlays/dev/aspire-dashboard/kustomization.yaml
  modified:
    - infra/k8s/base/postgres/kustomization.yaml
    - infra/k8s/base/rabbitmq/kustomization.yaml
    - infra/k8s/base/keycloak/kustomization.yaml
    - infra/k8s/base/web/kustomization.yaml
    - infra/k8s/overlays/dev/kustomization.yaml
    - infra/k8s/argocd/apps/postgres.yaml
    - infra/k8s/argocd/apps/rabbitmq.yaml
    - infra/k8s/argocd/apps/otel-collector.yaml
    - infra/k8s/argocd/apps/aspire-dashboard.yaml
    - infra/k8s/argocd/apps/keycloak.yaml

key-decisions:
  - "Keycloak ArgoCD app also updated from base/ to overlays/dev/ for consistency (was missed in plan)"
  - "Placeholder sealed secrets use AgAAAA== dummy encrypted values, overwritten by bootstrap.sh per-cluster"

patterns-established:
  - "Sealed secret placeholder pattern: commit dummy SealedSecret to Git so Kustomize can reference it declaratively"
  - "Every service gets a dev overlay directory even if it only references base with no overrides"

requirements-completed: [ARGO-02, ARGO-05]

duration: 2min
completed: 2026-03-08
---

# Phase 35 Plan 02: Infrastructure Overlays & Sealed Secrets Summary

**Dev overlay directories for all infrastructure services and placeholder sealed-secret.yaml files committed to Git as Kustomize resources for fully declarative ArgoCD deployment**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-08T16:33:24Z
- **Completed:** 2026-03-08T16:35:14Z
- **Tasks:** 2
- **Files modified:** 18

## Accomplishments
- Created placeholder SealedSecret YAML files for postgres, rabbitmq, keycloak, and web in base directories
- Added sealed-secret.yaml as a Kustomize resource in all 4 base kustomization.yaml files
- Created dev overlay directories for postgres, rabbitmq, otel-collector, and aspire-dashboard
- Updated all 8 ArgoCD Application manifests to consistently use overlays/dev/ paths (none reference base/ directly)

## Task Commits

Each task was committed atomically:

1. **Task 1: Create placeholder sealed-secret.yaml files and add to Kustomize resources** - `f09235ae` (feat)
2. **Task 2: Create dev overlays for infrastructure services and update ArgoCD app paths** - `eaa1d083` (feat)

## Files Created/Modified
- `infra/k8s/base/postgres/sealed-secret.yaml` - Placeholder SealedSecret for postgres credentials
- `infra/k8s/base/rabbitmq/sealed-secret.yaml` - Placeholder SealedSecret for rabbitmq credentials
- `infra/k8s/base/keycloak/sealed-secret.yaml` - Placeholder SealedSecret for keycloak credentials
- `infra/k8s/base/web/sealed-secret.yaml` - Placeholder SealedSecret for web secrets
- `infra/k8s/overlays/dev/postgres/kustomization.yaml` - Dev overlay referencing base/postgres
- `infra/k8s/overlays/dev/rabbitmq/kustomization.yaml` - Dev overlay referencing base/rabbitmq
- `infra/k8s/overlays/dev/otel-collector/kustomization.yaml` - Dev overlay referencing base/otel-collector
- `infra/k8s/overlays/dev/aspire-dashboard/kustomization.yaml` - Dev overlay referencing base/aspire-dashboard
- `infra/k8s/overlays/dev/kustomization.yaml` - Root overlay now references all services via subdirectories
- `infra/k8s/argocd/apps/*.yaml` - All 8 apps updated to overlays/dev/ paths

## Decisions Made
- Keycloak ArgoCD app was also pointing to base/keycloak instead of overlays/dev/keycloak -- updated for consistency (auto-fix)
- Placeholder sealed secrets use `AgAAAA==` dummy encrypted values that are overwritten by bootstrap.sh at cluster setup time

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed keycloak ArgoCD app path pointing to base/ instead of overlays/dev/**
- **Found during:** Task 2 (verification step)
- **Issue:** Keycloak app manifest referenced `infra/k8s/base/keycloak` but keycloak already had a dev overlay directory
- **Fix:** Updated path to `infra/k8s/overlays/dev/keycloak` for consistency with all other apps
- **Files modified:** infra/k8s/argocd/apps/keycloak.yaml
- **Verification:** All 8 ArgoCD apps now reference overlays/dev/; zero base/ references remain
- **Committed in:** eaa1d083 (Task 2 commit)

---

**Total deviations:** 1 auto-fixed (1 bug fix)
**Impact on plan:** Essential fix for the plan's stated success criteria that all apps use overlays/dev/ paths consistently. No scope creep.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All ArgoCD applications now use consistent overlay paths
- Sealed secrets are tracked declaratively in Git
- Ready for any further ArgoCD or Kustomize improvements in subsequent plans

---
*Phase: 35-argocd-gitops-best-practices*
*Completed: 2026-03-08*

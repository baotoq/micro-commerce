---
phase: 26-argocd-gitops
plan: 01
subsystem: infra
tags: [argocd, kubernetes, kustomize, gitops, kind]

# Dependency graph
requires:
  - phase: 25-kubernetes-manifests
    provides: Kustomize base and overlay structure for all 6 services
provides:
  - ArgoCD Application manifests (root app-of-apps + 6 child apps)
  - Per-service Kustomize overlays for independent ArgoCD sync
  - ArgoCD NodePort service and TLS-disable ConfigMap
  - kind-config.yaml ArgoCD port mapping
affects: [26-argocd-gitops, 27-ci-cd-pipeline]

# Tech tracking
tech-stack:
  added: [argocd-applications]
  patterns: [app-of-apps, per-service-kustomize-overlays]

key-files:
  created:
    - infra/k8s/argocd/root-app.yaml
    - infra/k8s/argocd/argocd-server-nodeport.yaml
    - infra/k8s/argocd/argocd-cmd-params-cm.yaml
    - infra/k8s/argocd/apps/postgres.yaml
    - infra/k8s/argocd/apps/rabbitmq.yaml
    - infra/k8s/argocd/apps/keycloak.yaml
    - infra/k8s/argocd/apps/apiservice.yaml
    - infra/k8s/argocd/apps/gateway.yaml
    - infra/k8s/argocd/apps/web.yaml
    - infra/k8s/overlays/dev/apiservice/kustomization.yaml
    - infra/k8s/overlays/dev/gateway/kustomization.yaml
    - infra/k8s/overlays/dev/web/kustomization.yaml
  modified:
    - infra/k8s/kind-config.yaml
    - infra/k8s/base/postgres/kustomization.yaml
    - infra/k8s/base/rabbitmq/kustomization.yaml
    - infra/k8s/base/keycloak/kustomization.yaml
    - infra/k8s/overlays/dev/kustomization.yaml

key-decisions:
  - "Removed namespace.yaml from dev overlay resources — namespace creation handled by bootstrap.sh Step 5 and ArgoCD syncOptions CreateNamespace=true"
  - "Used namespace field in dev overlay kustomization.yaml instead of referencing namespace.yaml file — avoids kustomize path security restriction"
  - "Infrastructure child apps point to base/ dirs (no image patching), app services point to per-service overlays/dev/{service}/ dirs (with image tags)"

patterns-established:
  - "App-of-apps pattern: root Application discovers child apps from infra/k8s/argocd/apps/ directory"
  - "Per-service Kustomize overlays: each app service has its own overlay for independent image tag management"
  - "Sealed secrets excluded from Git-tracked kustomization: applied directly by bootstrap script"

requirements-completed: [GOPS-01, GOPS-02]

# Metrics
duration: 8min
completed: 2026-03-02
---

# Plan 26-01: ArgoCD Manifests and Kustomize Restructure Summary

**ArgoCD app-of-apps with 6 child Applications, per-service Kustomize overlays, and sealed-secret exclusion from Git-synced paths**

## Performance

- **Duration:** 8 min
- **Tasks:** 2
- **Files created:** 12
- **Files modified:** 5

## Accomplishments
- Created root app-of-apps Application that auto-discovers 6 child Applications from `infra/k8s/argocd/apps/`
- All child apps configured with auto-sync, self-heal, prune, and retry (5 attempts with exponential backoff)
- Removed sealed-secret.yaml references from infrastructure base kustomization files (postgres, rabbitmq, keycloak) so ArgoCD can sync from Git
- Created per-service overlay directories (apiservice, gateway, web) enabling independent image tag management for CI/CD
- ArgoCD UI exposed via NodePort (30443 -> host 38443) with TLS disabled for local development
- All 7 Kustomize build paths validated successfully

## Task Commits

Each task was committed atomically:

1. **Task 1: Create ArgoCD manifest directory with root app, child apps, and supporting resources** - `239f36b7` (feat)
2. **Task 2: Restructure Kustomize for per-service ArgoCD management** - `41d42a7d` (feat)

## Files Created/Modified
- `infra/k8s/argocd/root-app.yaml` - Root app-of-apps Application pointing to apps/ directory
- `infra/k8s/argocd/argocd-server-nodeport.yaml` - NodePort service for ArgoCD UI access on 30443
- `infra/k8s/argocd/argocd-cmd-params-cm.yaml` - ConfigMap disabling TLS (server.insecure: true)
- `infra/k8s/argocd/apps/postgres.yaml` - Child Application for PostgreSQL (base/postgres path)
- `infra/k8s/argocd/apps/rabbitmq.yaml` - Child Application for RabbitMQ (base/rabbitmq path)
- `infra/k8s/argocd/apps/keycloak.yaml` - Child Application for Keycloak (base/keycloak path)
- `infra/k8s/argocd/apps/apiservice.yaml` - Child Application for ApiService (overlays/dev/apiservice path)
- `infra/k8s/argocd/apps/gateway.yaml` - Child Application for Gateway (overlays/dev/gateway path)
- `infra/k8s/argocd/apps/web.yaml` - Child Application for Web (overlays/dev/web path)
- `infra/k8s/overlays/dev/apiservice/kustomization.yaml` - Per-service overlay with image tag dev
- `infra/k8s/overlays/dev/gateway/kustomization.yaml` - Per-service overlay with image tag dev
- `infra/k8s/overlays/dev/web/kustomization.yaml` - Per-service overlay with image tag dev
- `infra/k8s/kind-config.yaml` - Added ArgoCD port mapping (30443 -> 38443)
- `infra/k8s/base/postgres/kustomization.yaml` - Removed sealed-secret.yaml reference
- `infra/k8s/base/rabbitmq/kustomization.yaml` - Removed sealed-secret.yaml reference
- `infra/k8s/base/keycloak/kustomization.yaml` - Removed sealed-secret.yaml reference
- `infra/k8s/overlays/dev/kustomization.yaml` - Restructured to reference infra bases + per-service overlay subdirs

## Decisions Made
- Removed namespace.yaml resource reference from dev overlay — namespace is created by bootstrap.sh and ArgoCD's CreateNamespace=true syncOption
- Used `namespace: micro-commerce` field in dev overlay kustomization.yaml instead of referencing namespace.yaml file to avoid kustomize path security restriction

## Deviations from Plan

### Auto-fixed Issues

**1. [Kustomize path security] namespace.yaml reference caused build failure**
- **Found during:** Task 2 (Kustomize restructure)
- **Issue:** `../../base/namespace.yaml` reference in dev overlay kustomization.yaml caused Kustomize security error (file not in or below directory)
- **Fix:** Removed namespace.yaml resource, added `namespace: micro-commerce` field to kustomization.yaml instead. Namespace creation handled by bootstrap.sh Step 5 and ArgoCD CreateNamespace syncOption.
- **Files modified:** infra/k8s/overlays/dev/kustomization.yaml
- **Verification:** `kubectl kustomize infra/k8s/overlays/dev/` succeeds
- **Committed in:** 41d42a7d (Task 2 commit)

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Auto-fix necessary for kustomize build correctness. No scope creep.

## Issues Encountered
None beyond the kustomize path security issue documented above.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- ArgoCD manifests ready for Plan 26-02 to integrate into bootstrap.sh
- Per-service overlays ready for Phase 27 CI/CD pipeline to update image tags

---
*Phase: 26-argocd-gitops*
*Completed: 2026-03-02*

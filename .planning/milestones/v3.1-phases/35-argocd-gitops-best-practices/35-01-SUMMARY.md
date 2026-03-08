---
phase: 35-argocd-gitops-best-practices
plan: 01
subsystem: infra
tags: [argocd, gitops, kubernetes, sync-waves, appproject]

requires:
  - phase: 34-reliability-improvements
    provides: StatefulSet-based RabbitMQ and bootstrap script
provides:
  - Dedicated AppProject restricting all apps to single repo and namespaces
  - Sync wave ordering for infrastructure-before-apps deployment
  - Root app retry strategy matching child apps
  - PostgreSQL and RabbitMQ StatefulSet ignoreDifferences
affects: [argocd, kubernetes, bootstrap]

tech-stack:
  added: []
  patterns: [ArgoCD AppProject scoping, sync wave ordering, ignoreDifferences for StatefulSets]

key-files:
  created:
    - infra/k8s/argocd/project.yaml
  modified:
    - infra/k8s/argocd/root-app.yaml
    - infra/k8s/argocd/apps/postgres.yaml
    - infra/k8s/argocd/apps/keycloak.yaml
    - infra/k8s/argocd/apps/rabbitmq.yaml
    - infra/k8s/argocd/apps/gateway.yaml
    - infra/k8s/argocd/apps/apiservice.yaml
    - infra/k8s/argocd/apps/web.yaml
    - infra/k8s/argocd/apps/otel-collector.yaml
    - infra/k8s/argocd/apps/aspire-dashboard.yaml
    - infra/k8s/bootstrap.sh

key-decisions:
  - "Sync wave ordering: postgres=1, rabbitmq/keycloak=2, apiservice=3, gateway=4, web=5, otel/aspire=6"
  - "AppProject allows both micro-commerce and argocd namespaces (argocd needed for child Application resources)"
  - "clusterResourceWhitelist limited to Namespace kind only (required by CreateNamespace=true)"
  - "ignoreDifferences applied to both postgres and rabbitmq StatefulSets for volumeClaimTemplates drift"

patterns-established:
  - "AppProject scoping: all apps use project: micro-commerce instead of default"
  - "Sync wave convention: infrastructure waves 1-2, application waves 3-5, observability wave 6"

requirements-completed: [ARGO-01, ARGO-03, ARGO-04, SEC-05]

duration: 2min
completed: 2026-03-08
---

# Phase 35 Plan 01: ArgoCD GitOps Best Practices Summary

**Dedicated AppProject with sync wave ordering, root app retry strategy, and StatefulSet ignoreDifferences for scoped resilient GitOps deployments**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-08T16:33:29Z
- **Completed:** 2026-03-08T16:35:00Z
- **Tasks:** 2
- **Files modified:** 11

## Accomplishments
- Created dedicated micro-commerce AppProject restricting sourceRepos and destination namespaces
- Added sync-wave annotations to all 9 manifests enforcing infrastructure-before-apps ordering
- Added retry strategy to root app matching child app configuration (limit 5, 5s/factor 2/max 3m)
- Added ignoreDifferences for PostgreSQL and RabbitMQ StatefulSet volumeClaimTemplates with RespectIgnoreDifferences
- Updated bootstrap script to apply AppProject before root app

## Task Commits

Each task was committed atomically:

1. **Task 1: Create AppProject and add sync waves, retry, ignoreDifferences** - `4e8228d3` (feat)
2. **Task 2: Update bootstrap script to apply AppProject before root app** - `46213eea` (feat)

## Files Created/Modified
- `infra/k8s/argocd/project.yaml` - Dedicated AppProject with restricted sourceRepos and destinations
- `infra/k8s/argocd/root-app.yaml` - Added project ref, sync-wave "0", retry strategy
- `infra/k8s/argocd/apps/postgres.yaml` - Sync-wave "1", project ref, ignoreDifferences, RespectIgnoreDifferences
- `infra/k8s/argocd/apps/rabbitmq.yaml` - Sync-wave "2", project ref, ignoreDifferences, RespectIgnoreDifferences
- `infra/k8s/argocd/apps/keycloak.yaml` - Sync-wave "2", project ref
- `infra/k8s/argocd/apps/apiservice.yaml` - Sync-wave "3", project ref
- `infra/k8s/argocd/apps/gateway.yaml` - Sync-wave "4", project ref
- `infra/k8s/argocd/apps/web.yaml` - Sync-wave "5", project ref
- `infra/k8s/argocd/apps/otel-collector.yaml` - Sync-wave "6", project ref
- `infra/k8s/argocd/apps/aspire-dashboard.yaml` - Sync-wave "6", project ref
- `infra/k8s/bootstrap.sh` - Apply project.yaml before root-app.yaml in Step 12

## Decisions Made
- Sync wave ordering: postgres=1, rabbitmq/keycloak=2, apiservice=3, gateway=4, web=5, otel/aspire=6
- AppProject allows both micro-commerce and argocd namespaces (argocd needed for child Application resources)
- clusterResourceWhitelist limited to Namespace kind only (required by CreateNamespace=true)
- ignoreDifferences applied to both postgres and rabbitmq StatefulSets for volumeClaimTemplates drift

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- All ArgoCD applications now scoped to dedicated AppProject
- Sync wave ordering ensures correct deployment sequence
- Ready for further GitOps best practices in Plan 02

---
*Phase: 35-argocd-gitops-best-practices*
*Completed: 2026-03-08*

---
phase: 35-argocd-gitops-best-practices
verified: 2026-03-08T17:00:00Z
status: passed
score: 6/6 must-haves verified
re_verification: false
---

# Phase 35: ArgoCD GitOps Best Practices Verification Report

**Phase Goal:** ArgoCD deployments are ordered, scoped, and fully declarative (sealed secrets in Git)
**Verified:** 2026-03-08T17:00:00Z
**Status:** passed
**Re-verification:** No -- initial verification

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | Sync wave annotations enforce infrastructure-before-apps deployment ordering | VERIFIED | All 9 manifests have sync-wave annotations: root=0, postgres=1, rabbitmq/keycloak=2, apiservice=3, gateway=4, web=5, otel-collector/aspire-dashboard=6. `grep -rn sync-wave` returns 9 matches. |
| 2 | All sealed secret YAML files are committed to Git and listed as Kustomize resources | VERIFIED | 4 sealed-secret.yaml files exist (postgres, rabbitmq, keycloak, web) with valid SealedSecret structure. All 4 base kustomization.yaml files list `sealed-secret.yaml` in resources. |
| 3 | Root app retry strategy matches child app configuration | VERIFIED | root-app.yaml has `retry: limit: 5, backoff: duration: 5s, factor: 2, maxDuration: 3m` -- identical to all child apps. |
| 4 | PostgreSQL StatefulSet diffs are ignored by ArgoCD | VERIFIED | postgres.yaml has `ignoreDifferences` for StatefulSet volumeClaimTemplates (apiVersion, kind) with `RespectIgnoreDifferences=true` sync option. rabbitmq.yaml also has same config. |
| 5 | All applications use consistent overlay paths | VERIFIED | All 8 ArgoCD child apps reference `infra/k8s/overlays/dev/*` paths. Zero references to `base/` in ArgoCD app source paths. Dev overlay kustomization.yaml files exist for all 4 previously missing services (postgres, rabbitmq, otel-collector, aspire-dashboard). |
| 6 | Dedicated ArgoCD AppProject restricts sourceRepos and destination namespaces | VERIFIED | `project.yaml` defines AppProject `micro-commerce` restricting sourceRepos to `github.com/baotoq/micro-commerce.git` and destinations to `micro-commerce` + `argocd` namespaces. All 9 manifests use `project: micro-commerce`. Zero references to `project: default`. |

**Score:** 6/6 truths verified

### Required Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `infra/k8s/argocd/project.yaml` | AppProject for micro-commerce | VERIFIED | Contains `kind: AppProject` with restricted sourceRepos and destinations |
| `infra/k8s/argocd/root-app.yaml` | Root app with retry and project ref | VERIFIED | Has sync-wave "0", project: micro-commerce, retry strategy |
| `infra/k8s/argocd/apps/postgres.yaml` | PostgreSQL app with ignoreDifferences | VERIFIED | Has ignoreDifferences for StatefulSet, sync-wave "1", RespectIgnoreDifferences |
| `infra/k8s/argocd/apps/rabbitmq.yaml` | RabbitMQ app with ignoreDifferences | VERIFIED | Has ignoreDifferences for StatefulSet, sync-wave "2", RespectIgnoreDifferences |
| `infra/k8s/argocd/apps/keycloak.yaml` | Keycloak app | VERIFIED | sync-wave "2", project: micro-commerce, overlays/dev path |
| `infra/k8s/argocd/apps/apiservice.yaml` | ApiService app | VERIFIED | sync-wave "3", project: micro-commerce, overlays/dev path |
| `infra/k8s/argocd/apps/gateway.yaml` | Gateway app | VERIFIED | sync-wave "4", project: micro-commerce, overlays/dev path |
| `infra/k8s/argocd/apps/web.yaml` | Web app | VERIFIED | sync-wave "5", project: micro-commerce, overlays/dev path |
| `infra/k8s/argocd/apps/otel-collector.yaml` | OTEL Collector app | VERIFIED | sync-wave "6", project: micro-commerce, overlays/dev path |
| `infra/k8s/argocd/apps/aspire-dashboard.yaml` | Aspire Dashboard app | VERIFIED | sync-wave "6", project: micro-commerce, overlays/dev path |
| `infra/k8s/base/postgres/sealed-secret.yaml` | Placeholder SealedSecret | VERIFIED | Valid SealedSecret with postgres-credentials name |
| `infra/k8s/base/rabbitmq/sealed-secret.yaml` | Placeholder SealedSecret | VERIFIED | Valid SealedSecret with rabbitmq-credentials name |
| `infra/k8s/base/keycloak/sealed-secret.yaml` | Placeholder SealedSecret | VERIFIED | Valid SealedSecret with keycloak-credentials name |
| `infra/k8s/base/web/sealed-secret.yaml` | Placeholder SealedSecret | VERIFIED | Valid SealedSecret with web-secrets name |
| `infra/k8s/overlays/dev/postgres/kustomization.yaml` | Dev overlay | VERIFIED | References `../../../base/postgres` |
| `infra/k8s/overlays/dev/rabbitmq/kustomization.yaml` | Dev overlay | VERIFIED | References `../../../base/rabbitmq` |
| `infra/k8s/overlays/dev/otel-collector/kustomization.yaml` | Dev overlay | VERIFIED | References `../../../base/otel-collector` |
| `infra/k8s/overlays/dev/aspire-dashboard/kustomization.yaml` | Dev overlay | VERIFIED | References `../../../base/aspire-dashboard` |

### Key Link Verification

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| `project.yaml` | All 9 Application specs | `project: micro-commerce` | WIRED | 9 matches for `project: micro-commerce`, 0 matches for `project: default` |
| `apps/*.yaml` | Deployment ordering | sync-wave annotation | WIRED | 9 sync-wave annotations with correct ordering (1-6) |
| `base/*/kustomization.yaml` | `sealed-secret.yaml` | Kustomize resources list | WIRED | All 4 base kustomizations (postgres, rabbitmq, keycloak, web) include `sealed-secret.yaml` |
| `overlays/dev/*/kustomization.yaml` | `base/*` | Kustomize base reference | WIRED | All 4 new overlays reference `../../../base/` correctly |
| `bootstrap.sh` | `project.yaml` then `root-app.yaml` | Sequential kubectl apply | WIRED | Line 187: `kubectl apply -f project.yaml`, Line 188: `kubectl apply -f root-app.yaml` -- correct order |
| `overlays/dev/kustomization.yaml` | All 8 services | Subdirectory references | WIRED | Lists all 8 services as subdirectory resources (no direct base references) |

### Requirements Coverage

| Requirement | Source Plan | Description | Status | Evidence |
|-------------|------------|-------------|--------|----------|
| ARGO-01 | 35-01 | Sync wave annotations on all Application resources enforcing dependency ordering | SATISFIED | All 9 manifests have sync-wave annotations with correct ordering |
| ARGO-02 | 35-02 | Sealed secret YAML files committed to Git and added as Kustomize resources | SATISFIED | 4 sealed-secret.yaml files exist and are listed in base kustomization resources |
| ARGO-03 | 35-01 | Root app retry strategy matching child app configuration | SATISFIED | Root app has identical retry config (limit 5, 5s/factor 2/max 3m) |
| ARGO-04 | 35-01 | ignoreDifferences configured for PostgreSQL StatefulSet with RespectIgnoreDifferences | SATISFIED | postgres.yaml and rabbitmq.yaml both have ignoreDifferences with RespectIgnoreDifferences |
| ARGO-05 | 35-02 | Consistent overlay paths for all applications | SATISFIED | All 8 apps reference overlays/dev paths; 0 reference base paths |
| SEC-05 | 35-01 | Dedicated ArgoCD AppProject with restricted sourceRepos and destinations | SATISFIED | AppProject micro-commerce restricts to single repo and two namespaces |

No orphaned requirements found. All 6 requirement IDs from ROADMAP.md are covered by plans and verified.

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
|------|------|---------|----------|--------|
| (none) | - | - | - | No anti-patterns found in any phase files |

No TODO, FIXME, PLACEHOLDER, or HACK comments found in any modified files.

### Human Verification Required

### 1. ArgoCD Sync Wave Ordering in Live Cluster

**Test:** Deploy to a kind cluster using `infra/k8s/bootstrap.sh` and observe ArgoCD sync order
**Expected:** PostgreSQL syncs before RabbitMQ/Keycloak, which sync before ApiService, etc.
**Why human:** Sync wave ordering is an ArgoCD runtime behavior that cannot be verified by static file analysis alone

### 2. PostgreSQL StatefulSet No Longer Reports OutOfSync

**Test:** After full sync, check ArgoCD UI for postgres application status
**Expected:** Application shows "Synced" and "Healthy" without requiring manual sync
**Why human:** The ignoreDifferences behavior depends on ArgoCD runtime diff engine

### Gaps Summary

No gaps found. All 6 success criteria from ROADMAP.md are fully satisfied:

1. Sync wave annotations are present on all 9 manifests with correct infrastructure-before-apps ordering
2. All 4 sealed secret placeholder files are committed and listed as Kustomize resources
3. Root app retry strategy is identical to child app configuration
4. PostgreSQL and RabbitMQ StatefulSet diffs are configured to be ignored
5. All 8 ArgoCD Application source paths use overlays/dev consistently
6. Dedicated AppProject restricts sourceRepos and destination namespaces

All 4 feature commits verified: `4e8228d3`, `46213eea`, `f09235ae`, `eaa1d083`

---

_Verified: 2026-03-08T17:00:00Z_
_Verifier: Claude (gsd-verifier)_

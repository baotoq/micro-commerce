---
phase: 26-argocd-gitops
plan: 02
subsystem: infra
tags: [argocd, kubernetes, bootstrap, gitops, sealed-secrets]

# Dependency graph
requires:
  - phase: 26-argocd-gitops
    provides: ArgoCD Application manifests (root app-of-apps + 6 child apps), NodePort service, TLS-disable ConfigMap
provides:
  - Updated bootstrap.sh with ArgoCD installation and GitOps handoff
  - ArgoCD-managed cluster from zero via single script
affects: [27-ci-cd-pipeline]

# Tech tracking
tech-stack:
  added: [argocd-v3.3.2]
  patterns: [gitops-bootstrap, sealed-secrets-direct-apply]

key-files:
  created: []
  modified:
    - infra/k8s/bootstrap.sh

key-decisions:
  - "Server-side apply for ArgoCD CRDs -- exceeds 256KB annotation limit for client-side apply"
  - "Retained pod wait commands after ArgoCD sync -- provides user feedback on deployment readiness"
  - "sleep 10 after root-app apply -- allows ArgoCD time to discover and create child Applications"

patterns-established:
  - "GitOps bootstrap: SealedSecrets + ArgoCD install, then root app-of-apps triggers all sync"
  - "Sealed secrets applied directly via kubectl apply -f, not through Kustomize resources"
  - "ArgoCD initial admin password extracted from argocd-initial-admin-secret for access info"

requirements-completed: [GOPS-01, GOPS-02]

# Metrics
duration: 5min
completed: 2026-03-02
---

# Plan 26-02: Bootstrap Script ArgoCD Integration Summary

**Bootstrap script now installs ArgoCD v3.3.2 and hands off all manifest management to GitOps via root app-of-apps Application**

## Performance

- **Duration:** 5 min
- **Tasks:** 1
- **Files modified:** 1

## Accomplishments
- Integrated ArgoCD v3.3.2 installation into bootstrap script with server-side apply
- Replaced all manual `kubectl apply -k` commands with ArgoCD-managed sync
- Applied sealed secrets directly via `kubectl apply -f` (no longer in kustomization.yaml)
- Added ArgoCD UI access info (http://localhost:38443) with auto-extracted admin password
- Script follows 14-step flow from zero to fully ArgoCD-managed cluster
- Bash syntax check passes (`bash -n`)

## Task Commits

Each task was committed atomically:

1. **Task 1: Update bootstrap.sh to install ArgoCD and replace manual kubectl apply with GitOps** - `9bcff943` (feat)

## Files Created/Modified
- `infra/k8s/bootstrap.sh` - Updated 14-step bootstrap: cluster, SealedSecrets, namespace, sealed secrets (direct apply), ArgoCD install, ArgoCD config (TLS disable + NodePort), image build, image load, root app-of-apps, pod waits, access info

## Decisions Made
- Used `--server-side --force-conflicts` for ArgoCD install manifest because CRDs exceed 256KB client-side apply annotation limit
- Retained pod wait commands after ArgoCD root app apply for user feedback on readiness
- Added `sleep 10` after root app apply to allow ArgoCD time to discover and create child Applications

## Deviations from Plan
None - plan executed exactly as written.

## Issues Encountered
None.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Full ArgoCD GitOps stack ready for Phase 27 CI/CD pipeline integration
- Bootstrap script creates a complete ArgoCD-managed cluster from scratch
- ArgoCD UI accessible for monitoring sync status

---
*Phase: 26-argocd-gitops*
*Completed: 2026-03-02*

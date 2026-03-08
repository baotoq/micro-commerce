---
phase: 33-k8s-security-hardening
plan: 02
subsystem: infra
tags: [kubernetes, kustomize, sealed-secrets, keycloak, security]

requires:
  - phase: 33-k8s-security-hardening-01
    provides: SecurityContext hardening for all workloads, SealedSecrets for infra components
provides:
  - Web frontend secrets managed via SealedSecret (AUTH_SECRET, KEYCLOAK_CLIENT_SECRET)
  - Keycloak production-mode base with dev overlay patch pattern
  - Bootstrap script seals web-secrets alongside infrastructure secrets
affects: [k8s-deployment, keycloak-config, web-config]

tech-stack:
  added: []
  patterns: [sealed-secrets-for-app-secrets, kustomize-overlay-args-patching]

key-files:
  created:
    - infra/k8s/overlays/dev/keycloak/keycloak-start-dev-patch.yaml
    - infra/k8s/overlays/dev/keycloak/kustomization.yaml
  modified:
    - infra/k8s/base/web/deployment.yaml
    - infra/k8s/base/keycloak/deployment.yaml
    - infra/k8s/overlays/dev/kustomization.yaml
    - infra/k8s/bootstrap.sh

key-decisions:
  - "Web secrets (AUTH_SECRET, KEYCLOAK_CLIENT_SECRET) use secretKeyRef to web-secrets SealedSecret"
  - "Keycloak base uses 'start' (production); dev overlay patches to 'start-dev' via strategic merge"

patterns-established:
  - "App-level secrets follow same SealedSecret pattern as infra secrets"
  - "Keycloak mode controlled via kustomize overlay patch, not base manifest"

requirements-completed: [SEC-01, SEC-03]

duration: 1min
completed: 2026-03-08
---

# Phase 33 Plan 02: Web Secrets & Keycloak Production Mode Summary

**Web frontend secrets moved to SealedSecret references, Keycloak base switched to production start with dev overlay patch**

## Performance

- **Duration:** 1 min
- **Started:** 2026-03-08T15:13:29Z
- **Completed:** 2026-03-08T15:15:10Z
- **Tasks:** 2
- **Files modified:** 6

## Accomplishments
- Removed plain-text AUTH_SECRET and KEYCLOAK_CLIENT_SECRET from web deployment manifest
- Web deployment now references web-secrets Secret via secretKeyRef
- Bootstrap script seals and applies web-secrets alongside existing infrastructure secrets
- Keycloak base manifest uses production-mode `start` command
- Dev overlay patches keycloak back to `start-dev` for local development

## Task Commits

Each task was committed atomically:

1. **Task 1: Move web secrets to SealedSecret references and update bootstrap** - `9e333e59` (feat)
2. **Task 2: Switch Keycloak base to production mode with dev overlay patch** - `cc1915d5` (feat)

## Files Created/Modified
- `infra/k8s/base/web/deployment.yaml` - AUTH_SECRET and KEYCLOAK_CLIENT_SECRET now use secretKeyRef to web-secrets
- `infra/k8s/bootstrap.sh` - Added seal_secret and kubectl apply for web-secrets
- `infra/k8s/base/keycloak/deployment.yaml` - Changed args from start-dev to start
- `infra/k8s/overlays/dev/keycloak/keycloak-start-dev-patch.yaml` - Strategic merge patch switching to start-dev
- `infra/k8s/overlays/dev/keycloak/kustomization.yaml` - Dev overlay kustomization referencing base and patch
- `infra/k8s/overlays/dev/kustomization.yaml` - Changed keycloak reference from base to overlay directory

## Decisions Made
- Web secrets (AUTH_SECRET, KEYCLOAK_CLIENT_SECRET) use secretKeyRef to web-secrets SealedSecret, consistent with infra secret pattern
- Keycloak base uses `start` (production mode); dev overlay patches to `start-dev` via strategic merge patch, matching existing overlay patterns for apiservice/gateway/web

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All secrets now managed via SealedSecrets (both infrastructure and application)
- Keycloak production/dev mode separation ready for environment-specific deployments
- Ready for next phase in v3.1 hardening milestone

---
*Phase: 33-k8s-security-hardening*
*Completed: 2026-03-08*

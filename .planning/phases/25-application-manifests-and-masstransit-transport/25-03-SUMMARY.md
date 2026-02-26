---
phase: 25-application-manifests-and-masstransit-transport
plan: 03
subsystem: infra
tags: [kubernetes, kind, bootstrap, kustomize, docker, dotnet-publish, deployment]

# Dependency graph
requires:
  - phase: 24-infrastructure-manifests-and-secrets
    provides: Bootstrap script with kind cluster creation, SealedSecrets, and infrastructure deployment
  - phase: 25-application-manifests-and-masstransit-transport
    plan: 01
    provides: MassTransit dual-transport switching, health endpoints, configurable CORS
  - phase: 25-application-manifests-and-masstransit-transport
    plan: 02
    provides: K8s Deployments/Services for ApiService, Gateway, Web with Kustomize base/overlay
provides:
  - Single-command bootstrap from zero to full running MicroCommerce stack in kind
  - App image build via dotnet publish /t:PublishContainer and docker build
  - Kind image loading for all 3 app services
  - Dev overlay deployment with app manifests
  - Pod readiness verification with appropriate timeouts
affects: [26-argocd-gitops, developer-onboarding]

# Tech tracking
tech-stack:
  added: []
  patterns: [dotnet-publish-container, kind-image-load, kustomize-overlay-deploy, bootstrap-script-lifecycle]

key-files:
  created: []
  modified:
    - infra/k8s/bootstrap.sh

key-decisions:
  - "Keep infrastructure-first deployment: Step 7 applies base/ for infra, Step 11 applies overlays/dev/ for full stack (idempotent for infra, adds apps)"
  - "ApiService gets 180s pod wait timeout for EF Core migrations on first boot; Gateway and Web get 120s"
  - "ContainerRegistry='' and ContainerRepository=apiservice override ghcr.io defaults for local-only kind images"

patterns-established:
  - "Bootstrap lifecycle: cluster create -> sealed secrets -> infra deploy -> infra wait -> image build -> image load -> app deploy -> app wait -> access info"
  - "Local image build pattern: dotnet publish /t:PublishContainer with empty registry for local tags, then kind load docker-image"

requirements-completed: [MFST-01, MFST-02, MFST-03, MFST-04, MFST-05, MFST-06, TRAN-01, TRAN-02]

# Metrics
duration: 2min
completed: 2026-02-26
---

# Phase 25 Plan 03: Bootstrap Script Extension Summary

**Single-command bootstrap with app image build (dotnet publish + docker build), kind image loading, dev overlay deployment, and pod readiness verification for the full 6-service stack**

## Performance

- **Duration:** 2 min
- **Started:** 2026-02-26T09:56:49Z
- **Completed:** 2026-02-26T09:59:48Z
- **Tasks:** 2
- **Files modified:** 1

## Accomplishments
- Extended bootstrap.sh from 9 steps (infra-only) to 13 steps (full stack) with app image build, kind load, dev overlay apply, and app pod readiness
- Added PROJECT_ROOT variable for project-relative dotnet publish and docker build commands
- Step 9: Builds ApiService and Gateway via `dotnet publish /t:PublishContainer`, Web via `docker build`
- Step 10: Loads all 3 images into kind with `kind load docker-image`
- Step 11: Applies dev overlay (`kubectl apply -k overlays/dev/`) which is idempotent for infra and adds app deployments
- Step 12: Waits for all app pods with appropriate timeouts (apiservice 180s, gateway/web 120s)
- Step 13: Prints full access info including Gateway URL at http://localhost:38800
- All 6 validation checks passed: kustomize build, resource limits, health probes, no plaintext credentials, namespace enforcement, bootstrap completeness

## Task Commits

Each task was committed atomically:

1. **Task 1: Extend bootstrap.sh with app image build, kind load, and app deployment** - `0b8f4447` (feat)
2. **Task 2: Validate complete manifest suite and document deployment** - no commit (validation-only, all checks passed, no files modified)

## Files Created/Modified
- `infra/k8s/bootstrap.sh` - Extended from 9 to 13 steps: added PROJECT_ROOT, app image build (dotnet publish + docker build), kind load, dev overlay apply, app pod wait, updated access info

## Decisions Made
- Infrastructure-first deployment preserved: Step 7 applies base/ for infra pods, then Step 11 applies overlays/dev/ (idempotent for infra, adds app Deployments)
- ApiService gets 180s timeout for first-boot EF Core migrations across 7 schemas; Gateway and Web get standard 120s
- `ContainerRegistry=""` and `ContainerRepository=apiservice` override the ghcr.io/baotoq defaults from Directory.Build.props for local-only kind images
- `--nologo -v quiet` for dotnet publish and `--quiet` for docker build to reduce build noise in the bootstrap output

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- Kustomize validation requires sealed-secret.yaml files which are generated at bootstrap time (by design from Phase 24). Used temporary placeholder files for validation, then cleaned up. This is expected behavior, not a bug.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Full stack deployable via single command: `./infra/k8s/bootstrap.sh`
- Ready for Phase 26 (ArgoCD GitOps) which will replace manual `kubectl apply -k` with ArgoCD-managed sync
- Kind cluster needs recreation if upgrading from Phase 24 (new Gateway port mapping in kind-config.yaml from Plan 02)

## Self-Check: PASSED

All modified files verified on disk. Task 1 commit (0b8f4447) verified in git log.

---
*Phase: 25-application-manifests-and-masstransit-transport*
*Completed: 2026-02-26*

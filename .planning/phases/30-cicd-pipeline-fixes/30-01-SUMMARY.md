---
phase: 30-cicd-pipeline-fixes
plan: 01
subsystem: infra
tags: [github-actions, ci-cd, dotnet-10, docker, aspire]

requires: []
provides:
  - "Fixed dotnet-test.yml workflow (.NET 10.0.x, no aspire workload)"
  - "Fixed release.yml workflow (test gate, correct project paths)"
  - "Secure Dockerfile with ARG for build-time secrets"
affects: [31-kubernetes-manifests, 32-gitops-argocd]

tech-stack:
  added: []
  patterns:
    - "Aspire SDK consumed via csproj package ref, not CI workload install"
    - "ARG for build-time placeholder secrets in Dockerfile"

key-files:
  created: []
  modified:
    - ".github/workflows/dotnet-test.yml"
    - ".github/workflows/release.yml"
    - "src/MicroCommerce.Web/Dockerfile"

key-decisions:
  - "Aspire pinned via Aspire.AppHost.Sdk package ref in csproj, CI does not install workload"
  - "Release workflow uses workflow_call to dotnet-test.yml as required gate"
  - "Dockerfile uses ARG (not ENV) for build-time auth placeholders to prevent secret leakage"

patterns-established:
  - "CI SDK version matches project target framework (10.0.x)"
  - "Test gate required before release job via needs: [tests]"

requirements-completed: [CICD-01, CICD-02, CICD-03, CICD-07, CICD-08]

duration: 1min
completed: 2026-03-08
---

# Phase 30 Plan 01: CI/CD Pipeline Fixes Summary

**Fixed CI/CD workflows for .NET 10.0.x SDK, re-enabled test gate, updated stale project paths, and secured Dockerfile build-time secrets with ARG**

## Performance

- **Duration:** 1 min
- **Started:** 2026-03-08T14:28:19Z
- **Completed:** 2026-03-08T14:29:26Z
- **Tasks:** 3
- **Files modified:** 3

## Accomplishments
- Updated both CI workflows from .NET 9.0.x to 10.0.x SDK
- Removed obsolete aspire workload install steps (now handled by SDK package ref)
- Re-enabled test gate in release workflow with needs: [tests] dependency
- Replaced stale CartService and Gateway/Yarp paths with MicroCommerce.ApiService and MicroCommerce.Gateway
- Removed defunct BuildingBlocks.ServiceDefaults NuGet publish step
- Changed Dockerfile ENV to ARG for build-time placeholder secrets

## Task Commits

Each task was committed atomically:

1. **Task 1: Fix dotnet-test.yml** - `14ff81c3` (fix)
2. **Task 2: Fix release.yml** - `2ac43c15` (fix)
3. **Task 3: Fix Dockerfile** - `015a1f86` (fix)

## Files Created/Modified
- `.github/workflows/dotnet-test.yml` - Updated SDK to 10.0.x, removed aspire workload step
- `.github/workflows/release.yml` - Updated SDK, re-enabled test gate, fixed project paths, removed stale steps
- `src/MicroCommerce.Web/Dockerfile` - Changed ENV to ARG for build-time auth placeholders

## Decisions Made
- Aspire pinned via Aspire.AppHost.Sdk package ref in csproj; CI does not install workload
- Release workflow uses workflow_call to dotnet-test.yml as required gate
- Dockerfile uses ARG (not ENV) for build-time auth placeholders to prevent secret leakage into runtime image

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- CI workflows are ready for push to master
- Release workflow correctly references current project structure
- Next phases can rely on passing CI as a quality gate

---
*Phase: 30-cicd-pipeline-fixes*
*Completed: 2026-03-08*

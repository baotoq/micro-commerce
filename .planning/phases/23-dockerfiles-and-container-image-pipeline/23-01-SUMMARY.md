---
phase: 23-dockerfiles-and-container-image-pipeline
plan: 01
subsystem: infra
tags: [dotnet-publish, container, ghcr, chiseled, msbuild, docker]

# Dependency graph
requires: []
provides:
  - "Centralized container MSBuild properties in Directory.Build.props (registry + base image)"
  - "Per-project ContainerRepository overrides for ApiService and Gateway"
  - "dotnet publish /t:PublishContainer ready for both .NET services"
affects: [23-02, 23-03, 24-kubernetes-manifests, 25-kubernetes-deployment]

# Tech tracking
tech-stack:
  added: [dotnet-publish-container, noble-chiseled-extra]
  patterns: [centralized-msbuild-container-config, per-project-repository-override]

key-files:
  modified:
    - src/Directory.Build.props
    - src/MicroCommerce.ApiService/MicroCommerce.ApiService.csproj
    - src/MicroCommerce.Gateway/MicroCommerce.Gateway.csproj

key-decisions:
  - "Use noble-chiseled-extra variant (not plain noble-chiseled) because InvariantGlobalization is not set — ICU libraries needed for EF Core, MassTransit, Keycloak JWT"
  - "ContainerRepository does not include registry prefix — MSBuild inheritance from Directory.Build.props provides ghcr.io"
  - "No ContainerRuntimeIdentifiers in Directory.Build.props — multi-arch handled in CI via per-arch builds + manifest merge"
  - "No ContainerPort set — .NET 8+ defaults to 8080 for non-root containers"

patterns-established:
  - "Centralized container config: shared properties in Directory.Build.props, per-project ContainerRepository only in .csproj"
  - "Chiseled image selection: check InvariantGlobalization setting to choose noble-chiseled vs noble-chiseled-extra"

requirements-completed: [CONT-01, CONT-02]

# Metrics
duration: 2min
completed: 2026-02-25
---

# Phase 23 Plan 01: .NET Container Publishing Summary

**Configured dotnet publish /t:PublishContainer for ApiService and Gateway with ghcr.io registry and noble-chiseled-extra runtime base image**

## Performance

- **Duration:** 2 min
- **Started:** 2026-02-25T17:59:02Z
- **Completed:** 2026-02-25T18:01:18Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments
- Centralized container registry (ghcr.io) and base image (noble-chiseled-extra) in Directory.Build.props
- Added ContainerRepository overrides to ApiService and Gateway csproj files
- Verified both projects build in Release mode and container publishing produces valid images locally

## Task Commits

Each task was committed atomically:

1. **Task 1: Add shared container properties to Directory.Build.props** - `977a919f` (feat)
2. **Task 2: Add ContainerRepository to ApiService and Gateway csproj files** - `0e9bbeda` (feat)

## Files Created/Modified
- `src/Directory.Build.props` - Added ContainerRegistry=ghcr.io and ContainerBaseImage=noble-chiseled-extra in new PropertyGroup
- `src/MicroCommerce.ApiService/MicroCommerce.ApiService.csproj` - Added ContainerRepository=baotoq/micro-commerce/apiservice
- `src/MicroCommerce.Gateway/MicroCommerce.Gateway.csproj` - Added ContainerRepository=baotoq/micro-commerce/gateway

## Decisions Made
- Used `noble-chiseled-extra` variant because `InvariantGlobalization` is not set (defaults to false). The `-extra` variant includes ICU libraries needed for globalization features used by EF Core, MassTransit, and Keycloak JWT validation.
- Did not add `ContainerRuntimeIdentifiers` to Directory.Build.props -- multi-arch is handled in CI via per-arch builds and manifest merge (avoids containerd image store dependency).
- Did not set `ContainerPort` -- .NET 8+ defaults to port 8080 for non-root containers, and chiseled images enforce non-root by default (APP_UID=1654).

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

- Container publish to ghcr.io failed locally due to missing registry credentials (not logged into ghcr.io). This is expected for local development -- verified container build works by publishing to local Docker daemon with `ContainerRegistry=''` override. The image was built and inspected successfully (linux/arm64 on Apple Silicon host).

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- Directory.Build.props container properties are inherited by all .NET projects in src/
- Plan 02 (Next.js Dockerfile) can proceed independently
- Plan 03 (GitHub Actions CI workflow) can reference these MSBuild properties for dotnet publish commands
- Both .NET services are ready for `dotnet publish /t:PublishContainer` in CI with proper ghcr.io authentication

## Self-Check: PASSED

- All 3 modified files exist on disk
- Both task commits verified in git history (977a919f, 0e9bbeda)
- SUMMARY.md created successfully

---
*Phase: 23-dockerfiles-and-container-image-pipeline*
*Completed: 2026-02-25*

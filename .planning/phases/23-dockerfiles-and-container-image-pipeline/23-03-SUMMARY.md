---
phase: 23-dockerfiles-and-container-image-pipeline
plan: 03
subsystem: infra
tags: [github-actions, ci, container-images, ghcr, multi-arch, docker-buildx, dotnet-publish]

# Dependency graph
requires:
  - phase: 23-dockerfiles-and-container-image-pipeline
    plan: 01
    provides: "MSBuild container properties (ContainerRegistry, ContainerBaseImage, ContainerRepository)"
  - phase: 23-dockerfiles-and-container-image-pipeline
    plan: 02
    provides: "Next.js Dockerfile with standalone output for docker build-push-action"
provides:
  - "GitHub Actions workflow building and pushing all 3 container images to ghcr.io"
  - "Multi-arch (linux/amd64 + linux/arm64) support for all images"
  - "Automated tagging: SHA prefix, latest on master, semver on version tags"
affects: [27-cicd-gitops-loop-closure]

# Tech tracking
tech-stack:
  added: [docker/metadata-action, docker/build-push-action, docker/setup-qemu-action, docker-buildx-imagetools]
  patterns: [parallel-ci-jobs, per-arch-dotnet-publish-plus-manifest-merge, gha-docker-layer-caching]

key-files:
  created:
    - .github/workflows/container-images.yml

key-decisions:
  - "Separate workflow from release.yml -- release.yml uses 1Password + NuGet; container-images.yml uses GITHUB_TOKEN + ghcr.io only"
  - ".NET multi-arch via per-arch dotnet publish + docker buildx imagetools create (avoids containerd image store dependency on GitHub runners)"
  - "Next.js multi-arch via QEMU + buildx build-push-action with platforms flag"
  - "GHA cache (type=gha) for Next.js Docker layer caching, scoped per image"
  - "dotnet-version 10.0.x matching project net10.0 target framework"

patterns-established:
  - "Container CI pattern: three independent parallel jobs, each with login -> metadata -> build -> push"
  - ".NET multi-arch CI: publish per-arch with temporary SHA-arch tags, then merge with imagetools create"
  - "Tag strategy: docker/metadata-action generates sha-{short}, latest (master only), semver (tags only)"

requirements-completed: [CONT-04, CICD-01]

# Metrics
duration: 2min
completed: 2026-02-26
---

# Phase 23 Plan 03: GitHub Actions Container Image Workflow Summary

**GitHub Actions CI workflow building and pushing ApiService, Gateway, and Web images to ghcr.io with multi-arch support and SHA/latest/semver tagging**

## Performance

- **Duration:** 2 min
- **Started:** 2026-02-25T18:04:12Z
- **Completed:** 2026-02-25T18:05:53Z
- **Tasks:** 2
- **Files created:** 1

## Accomplishments
- Created container-images.yml workflow with three parallel jobs (build-web, build-apiservice, build-gateway)
- All jobs authenticate to ghcr.io using GITHUB_TOKEN with packages:write permission
- Tag strategy via docker/metadata-action: sha-{short}, latest (master pushes only), semver (version tags)
- Next.js multi-arch via QEMU + docker buildx build-push-action with linux/amd64,linux/arm64
- .NET multi-arch via per-arch dotnet publish /t:PublishContainer + docker buildx imagetools create manifest merge
- GHA cache enabled for Next.js Docker builds
- Validated workflow structure: correct triggers, three jobs, proper permissions, no external secret dependencies

## Task Commits

Each task was committed atomically:

1. **Task 1: Create container-images.yml workflow for all images** - `fe614615` (feat)
2. **Task 2: Validate workflow YAML syntax** - No commit (validation-only task, no file changes)

## Files Created/Modified
- `.github/workflows/container-images.yml` - GitHub Actions workflow with three parallel jobs building and pushing all container images to ghcr.io

## Decisions Made
- Created a new workflow file completely separate from release.yml -- release.yml uses 1Password for secrets and handles NuGet packages; container-images.yml uses only GITHUB_TOKEN for ghcr.io auth
- .NET multi-arch uses per-architecture dotnet publish (x64, arm64) producing single-arch images, then docker buildx imagetools create merges them into a multi-arch OCI manifest -- avoids containerd image store dependency on GitHub runners
- Next.js multi-arch uses standard QEMU + buildx approach with docker/build-push-action platforms flag
- GHA cache (type=gha) scoped per image for Docker layer caching on Next.js builds
- Used dotnet-version 10.0.x matching the project's net10.0 target framework (release.yml still uses 9.0.x for legacy NuGet builds)

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## User Setup Required

None - the workflow uses GITHUB_TOKEN which is automatically available in GitHub Actions. No external service configuration required.

## Next Phase Readiness
- Phase 23 is now complete -- all three container images have Dockerfiles and CI builds
- Phase 24 (Infrastructure Manifests and Secrets) can proceed with image references to ghcr.io/baotoq/micro-commerce/{apiservice,gateway,web}
- Phase 27 (CI/CD GitOps Loop Closure) will extend this workflow to commit SHA image tags back to Kustomize overlays

## Self-Check: PASSED

All artifacts verified:
- .github/workflows/container-images.yml: FOUND
- 23-03-SUMMARY.md: FOUND
- Commit fe614615 (Task 1): FOUND

---
*Phase: 23-dockerfiles-and-container-image-pipeline*
*Completed: 2026-02-26*

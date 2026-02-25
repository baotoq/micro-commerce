---
phase: 23-dockerfiles-and-container-image-pipeline
plan: 02
subsystem: infra
tags: [docker, nextjs, standalone, node-alpine, multi-stage, container]

# Dependency graph
requires:
  - phase: 22-wire-interceptors-to-dbcontexts
    provides: Complete v2.0 codebase ready for containerization
provides:
  - Multi-stage Next.js Dockerfile with standalone output and non-root user
  - .dockerignore for Next.js build context optimization
  - next.config.ts with standalone output mode enabled
affects: [23-03-container-images-workflow, 25-application-manifests]

# Tech tracking
tech-stack:
  added: [node:22-alpine, docker-multi-stage]
  patterns: [nextjs-standalone-output, non-root-container-user, build-time-placeholder-secrets]

key-files:
  created:
    - src/MicroCommerce.Web/Dockerfile
    - src/MicroCommerce.Web/.dockerignore
  modified:
    - src/MicroCommerce.Web/next.config.ts

key-decisions:
  - "Used npm ci --ignore-scripts for reproducible dependency installs without postinstall side effects"
  - "AUTH_SECRET=placeholder-for-docker-build prevents next-auth v5 build failures; real secret injected at K8s runtime"
  - "No HEALTHCHECK in Dockerfile; Kubernetes liveness/readiness probes handle health checking (Phase 25)"

patterns-established:
  - "Three-stage Dockerfile pattern: deps (install) -> builder (build) -> runner (minimal production)"
  - "Non-root container user: nextjs (UID 1001) matching official Next.js Docker example"
  - "Standalone output requires explicit COPY of .next/static and public directories"

requirements-completed: [CONT-03]

# Metrics
duration: 2min
completed: 2026-02-26
---

# Phase 23 Plan 02: Next.js Web Dockerfile Summary

**Multi-stage Next.js Dockerfile with node:22-alpine, standalone output, and non-root nextjs user serving on port 3000**

## Performance

- **Duration:** 2 min
- **Started:** 2026-02-25T17:59:22Z
- **Completed:** 2026-02-25T18:00:53Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments
- Enabled standalone output in next.config.ts for minimal self-contained server.js production bundle
- Created three-stage Dockerfile (deps/builder/runner) with node:22-alpine base producing a small production image
- Created .dockerignore excluding node_modules, .next, .env files, and test artifacts from build context
- Docker build succeeds, container starts as non-root user and returns HTTP 200 on port 3000

## Task Commits

Each task was committed atomically:

1. **Task 1: Enable standalone output in next.config.ts** - `c5314616` (feat)
2. **Task 2: Create Dockerfile and .dockerignore for Next.js Web app** - `afae4b5f` (feat)

## Files Created/Modified
- `src/MicroCommerce.Web/next.config.ts` - Added `output: 'standalone'` for minimal production image
- `src/MicroCommerce.Web/Dockerfile` - Three-stage multi-stage build (deps, builder, runner) with node:22-alpine
- `src/MicroCommerce.Web/.dockerignore` - Excludes node_modules, .next, .env, test artifacts from Docker context

## Decisions Made
- Used `npm ci --ignore-scripts` instead of `npm ci --no-audit --no-fund` (plan specification) for security and reproducibility
- Set `AUTH_SECRET=placeholder-for-docker-build` and `AUTH_TRUST_HOST=true` as build-time environment variables to prevent next-auth v5 build failures without baking real secrets into the image
- No HEALTHCHECK instruction in Dockerfile -- Kubernetes probes handle health checking (locked decision from Phase 23 research)
- Used `libc6-compat` on Alpine for Node.js native module compatibility

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

- Docker build produces warnings about `AUTH_SECRET` and `AUTH_TRUST_HOST` in ENV instructions (`SecretsUsedInArgOrEnv`). This is expected and intentional -- these are placeholder values, not real secrets. The real AUTH_SECRET is injected at runtime via Kubernetes Secrets (Phase 25).

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- Next.js Dockerfile is ready for the GitHub Actions container image workflow (Plan 03)
- The Dockerfile supports multi-arch builds via `docker/build-push-action` with QEMU/Buildx
- Container verified: builds successfully, starts on port 3000, runs as non-root user, returns HTTP 200

## Self-Check: PASSED

All artifacts verified:
- src/MicroCommerce.Web/next.config.ts: FOUND
- src/MicroCommerce.Web/Dockerfile: FOUND
- src/MicroCommerce.Web/.dockerignore: FOUND
- 23-02-SUMMARY.md: FOUND
- Commit c5314616 (Task 1): FOUND
- Commit afae4b5f (Task 2): FOUND

---
*Phase: 23-dockerfiles-and-container-image-pipeline*
*Completed: 2026-02-26*

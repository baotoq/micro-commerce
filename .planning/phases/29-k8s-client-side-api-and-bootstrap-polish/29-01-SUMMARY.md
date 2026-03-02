---
phase: 29-k8s-client-side-api-and-bootstrap-polish
plan: 01
subsystem: api
tags: [next.js, runtime-config, kubernetes, tanstack-query, fetch]

# Dependency graph
requires:
  - phase: 25-k8s-app-deployment
    provides: "/api/config route handler exposing runtime gateway URL"
provides:
  - "Runtime-resolved API base URL in api.ts via getApiBase() singleton"
  - "Client-side API calls work in both Aspire local dev and K8s deployments"
affects: [k8s-deployment, frontend]

# Tech tracking
tech-stack:
  added: []
  patterns: ["promise-based singleton for runtime config resolution"]

key-files:
  created: []
  modified:
    - "src/MicroCommerce.Web/src/lib/api.ts"

key-decisions:
  - "Promise-based singleton caches the Promise itself (not resolved value) to prevent race conditions from concurrent component mounts"
  - "typeof window guard prevents SSR from fetching its own /api/config route handler via HTTP"
  - "Server-side fallback uses NEXT_PUBLIC_API_URL for backward compatibility, catch fallback to localhost:5200 for offline dev"

patterns-established:
  - "getApiBase() singleton: all client-side API functions resolve base URL at runtime via await getApiBase()"

requirements-completed: [K8S-API-01]

# Metrics
duration: 3min
completed: 2026-03-02
---

# Phase 29 Plan 01: Runtime API Base URL Summary

**Replaced build-time NEXT_PUBLIC_API_URL constant with runtime getApiBase() singleton fetching gateway URL from /api/config**

## Performance

- **Duration:** 3 min
- **Started:** 2026-03-02T17:01:29Z
- **Completed:** 2026-03-02T17:04:43Z
- **Tasks:** 1
- **Files modified:** 1

## Accomplishments
- Removed hardcoded `NEXT_PUBLIC_API_URL` build-time constant from api.ts
- Added `getApiBase()` promise-based singleton that fetches `/api/config` once per page load
- Mechanically updated all 50 `${API_BASE}` usages across 40+ async functions to use `${apiBase}` via `await getApiBase()`
- Client-side API calls (cart, checkout, orders, wishlist, reviews, profiles) now resolve gateway URL at runtime

## Task Commits

Each task was committed atomically:

1. **Task 1: Replace build-time API_BASE with runtime getApiBase() singleton** - `aa534334` (feat)

## Files Created/Modified
- `src/MicroCommerce.Web/src/lib/api.ts` - Runtime-resolved API base URL via getApiBase() singleton replacing build-time constant

## Decisions Made
- Promise caches the Promise object itself (not the resolved string) to prevent duplicate /api/config fetches when multiple React components mount simultaneously
- `typeof window !== "undefined"` guard ensures SSR context uses env var fallback instead of attempting self-referential HTTP fetch
- Catch handler falls back to localhost:5200 so offline/local dev without /api/config still works

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Runtime API resolution complete, enabling single Docker image for both Aspire and K8s deployments
- Plan 29-02 (bootstrap script polish) can proceed independently

## Self-Check: PASSED

- FOUND: src/MicroCommerce.Web/src/lib/api.ts
- FOUND: .planning/phases/29-k8s-client-side-api-and-bootstrap-polish/29-01-SUMMARY.md
- FOUND: commit aa534334

---
*Phase: 29-k8s-client-side-api-and-bootstrap-polish*
*Completed: 2026-03-02*

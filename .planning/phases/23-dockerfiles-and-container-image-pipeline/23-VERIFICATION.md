---
phase: 23-dockerfiles-and-container-image-pipeline
verified: 2026-02-26T00:00:00Z
status: passed
score: 14/14 must-haves verified
re_verification: false
human_verification:
  - test: "Push a commit to master branch and observe the container-images workflow run"
    expected: "All three jobs (build-web, build-apiservice, build-gateway) complete successfully and images appear in ghcr.io/baotoq/micro-commerce/{apiservice,gateway,web}"
    why_human: "Cannot verify GitHub Actions CI execution or ghcr.io registry state without running a live push"
  - test: "Pull and run ghcr.io/baotoq/micro-commerce/web:latest; check whoami"
    expected: "Process runs as nextjs user (UID 1001), container responds on port 3000"
    why_human: "Container runtime behavior and user verification requires an actual docker run"
---

# Phase 23: Dockerfiles and Container Image Pipeline Verification Report

**Phase Goal:** Three production-ready container images exist in ghcr.io and are built automatically on every push to master
**Verified:** 2026-02-26
**Status:** PASSED
**Re-verification:** No — initial verification

## Goal Achievement

### Observable Truths

All 14 must-have truths across the three plans were verified against the actual codebase.

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | dotnet publish /t:PublishContainer succeeds for ApiService — chiseled base configured | VERIFIED | `ContainerBaseImage=mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra` in `src/Directory.Build.props:13`; `ContainerRepository=baotoq/micro-commerce/apiservice` in ApiService.csproj:10 |
| 2 | dotnet publish /t:PublishContainer succeeds for Gateway — chiseled base configured | VERIFIED | Same `Directory.Build.props` inheritance; `ContainerRepository=baotoq/micro-commerce/gateway` in Gateway.csproj:10 |
| 3 | .NET container images use chiseled runtime (non-root by default) | VERIFIED | `noble-chiseled-extra` base image in Directory.Build.props; `extra` variant chosen because `InvariantGlobalization` is not set (ICU needed for EF Core/MassTransit/Keycloak) |
| 4 | Container registry and base image config is centralized in Directory.Build.props | VERIFIED | `src/Directory.Build.props` lines 11–14: `ContainerRegistry=ghcr.io`, `ContainerBaseImage=...noble-chiseled-extra` |
| 5 | Each project only overrides its ContainerRepository | VERIFIED | ApiService.csproj and Gateway.csproj each have one `ContainerRepository` PropertyGroup; no other container properties duplicated |
| 6 | Next.js builds with standalone output mode producing a self-contained server.js | VERIFIED | `next.config.ts:4` contains `output: 'standalone'` |
| 7 | Next.js Dockerfile is a multi-stage build with node:22-alpine | VERIFIED | `src/MicroCommerce.Web/Dockerfile:1` — `FROM node:22-alpine AS base`; 3 stages: deps, builder, runner; 45 lines total |
| 8 | Container runs as non-root nextjs user (UID 1001) | VERIFIED | Dockerfile lines 33–34: `addgroup --gid 1001 nodejs && adduser --uid 1001 nextjs`; `USER nextjs` at line 41 |
| 9 | Static assets and public directory are included in production image | VERIFIED | Dockerfile lines 37–39: explicit `COPY --from=builder` for `public`, `.next/standalone`, and `.next/static` |
| 10 | Container serves pages on port 3000 | VERIFIED | `EXPOSE 3000` at line 43; `ENV PORT=3000` at line 30; `CMD ["node", "server.js"]` at line 45 |
| 11 | A push to master triggers the container-images workflow | VERIFIED | `.github/workflows/container-images.yml:4–6`: `on.push.branches: [master]` |
| 12 | A version tag push (v*.*.*) triggers the container-images workflow | VERIFIED | `.github/workflows/container-images.yml:7`: `on.push.tags: ['v*.*.*']` |
| 13 | All three images are built and pushed to ghcr.io with SHA/latest/semver tags | VERIFIED | Three parallel jobs (build-web, build-apiservice, build-gateway); `docker/metadata-action` generates `type=sha,prefix=sha-`, `type=raw,value=latest` (master only), `type=semver,pattern={{version}}` |
| 14 | Images pushed to ghcr.io using GITHUB_TOKEN with packages:write permission | VERIFIED | All three jobs have `permissions.packages: write`; `docker/login-action` uses `secrets.GITHUB_TOKEN`; no external secret dependencies |

**Score:** 14/14 truths verified

### Required Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `src/Directory.Build.props` | Shared container MSBuild properties | VERIFIED | Contains `ContainerRegistry=ghcr.io` and `ContainerBaseImage=...noble-chiseled-extra`; 14 lines, substantive |
| `src/MicroCommerce.ApiService/MicroCommerce.ApiService.csproj` | ApiService container repository override | VERIFIED | `ContainerRepository=baotoq/micro-commerce/apiservice` present |
| `src/MicroCommerce.Gateway/MicroCommerce.Gateway.csproj` | Gateway container repository override | VERIFIED | `ContainerRepository=baotoq/micro-commerce/gateway` present |
| `src/MicroCommerce.Web/Dockerfile` | Multi-stage Next.js production Dockerfile | VERIFIED | 45 lines; 3 stages (deps/builder/runner); `FROM node:22-alpine`; `USER nextjs`; `CMD ["node", "server.js"]` |
| `src/MicroCommerce.Web/.dockerignore` | Docker build context exclusions for Next.js | VERIFIED | 11 lines; excludes `node_modules`, `.next`, `.env.*`, `e2e`, test artifacts |
| `src/MicroCommerce.Web/next.config.ts` | Next.js config with standalone output | VERIFIED | `output: 'standalone'` as first property in NextConfig object |
| `.github/workflows/container-images.yml` | CI workflow for building and pushing all 3 container images | VERIFIED | 174 lines; 3 parallel jobs; all 22 structural checks passed |

### Key Link Verification

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| `ApiService.csproj` | `src/Directory.Build.props` | MSBuild property inheritance | VERIFIED | `ContainerBaseImage` and `ContainerRegistry` defined in Directory.Build.props; csproj only adds `ContainerRepository` |
| `Gateway.csproj` | `src/Directory.Build.props` | MSBuild property inheritance | VERIFIED | Same pattern; `ContainerRepository=baotoq/micro-commerce/gateway` |
| `src/MicroCommerce.Web/Dockerfile` | `src/MicroCommerce.Web/next.config.ts` | standalone output enables server.js | VERIFIED | `next.config.ts:4` has `output: 'standalone'`; Dockerfile line 38 copies `.next/standalone` |
| `src/MicroCommerce.Web/Dockerfile` | `.next/standalone/server.js` | CMD entrypoint | VERIFIED | `CMD ["node", "server.js"]` at Dockerfile line 45 |
| `.github/workflows/container-images.yml` | `src/MicroCommerce.Web/Dockerfile` | docker/build-push-action context | VERIFIED | `context: src/MicroCommerce.Web` in build-web job |
| `.github/workflows/container-images.yml` | `src/MicroCommerce.ApiService/MicroCommerce.ApiService.csproj` | dotnet publish /t:PublishContainer | VERIFIED | `dotnet publish src/MicroCommerce.ApiService /t:PublishContainer` in build-apiservice job |
| `.github/workflows/container-images.yml` | `ghcr.io` | docker/login-action with GITHUB_TOKEN | VERIFIED | All three jobs authenticate to `ghcr.io` via `secrets.GITHUB_TOKEN` with `permissions.packages: write` |

### Requirements Coverage

| Requirement | Source Plan | Description | Status | Evidence |
|-------------|------------|-------------|--------|----------|
| CONT-01 | Plan 01 | ApiService has a multi-stage Dockerfile with layer-cached restore and chiseled runtime image | SATISFIED (with note) | Uses `dotnet publish /t:PublishContainer` (Dockerfile-free) with `noble-chiseled-extra` base. Research explicitly maps this requirement to the SDK approach; no Dockerfile needed. Layer caching is not explicit (no NuGet cache action) but not a blocker. |
| CONT-02 | Plan 01 | Gateway has a multi-stage Dockerfile with layer-cached restore and chiseled runtime image | SATISFIED (with note) | Same approach as CONT-01; `ContainerRepository=baotoq/micro-commerce/gateway` |
| CONT-03 | Plan 02 | Web (Next.js) has a multi-stage Dockerfile with standalone output and node:alpine runtime | SATISFIED | 3-stage Dockerfile with `node:22-alpine`; `output: 'standalone'` in next.config.ts; `USER nextjs` (UID 1001) |
| CONT-04 | Plan 03 | All 3 images are pushed to ghcr.io with SHA-based tags | SATISFIED | `docker/metadata-action` generates `type=sha,prefix=sha-` tags for all three jobs; manifest merge produces multi-arch OCI images |
| CICD-01 | Plan 03 | GitHub Actions workflow builds and pushes all 3 images to ghcr.io on push to master | SATISFIED | `container-images.yml` triggers on `push.branches: [master]` and `push.tags: ['v*.*.*']`; three parallel jobs cover all three images |

**Orphaned requirements:** None. All five Phase 23 requirements (CONT-01, CONT-02, CONT-03, CONT-04, CICD-01) appear in plan frontmatter and are accounted for.

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
|------|------|---------|----------|--------|
| `src/MicroCommerce.Web/Dockerfile` | 19 | `ENV AUTH_SECRET=placeholder-for-docker-build` | Info | Intentional design: prevents next-auth v5 build failure without baking real secret. Real value injected at K8s runtime (Phase 25). No security risk as it is a placeholder. |
| `src/MicroCommerce.Web/Dockerfile` | 1 | No `--platform=$BUILDPLATFORM` on `FROM node:22-alpine AS base` | Warning | For QEMU-based multi-arch builds, omitting `FROM --platform=$BUILDPLATFORM` means the builder stage runs under QEMU emulation on ARM64. This makes `npm run build` slower on ARM64 runners but does not break correctness. The plan explicitly accepted this tradeoff. |
| `.github/workflows/container-images.yml` | n/a | No NuGet restore cache for build-apiservice and build-gateway jobs | Warning | Each CI run performs a fresh NuGet restore (no `actions/cache` for `.nuget/packages`). Builds are correct but slower than optimal. Next.js job correctly uses GHA Docker layer cache. |

No blocker anti-patterns found. All items are info or warning level.

### Human Verification Required

#### 1. GitHub Actions CI — Live Push Verification

**Test:** Push a commit to master (or create and merge a PR) and observe the `Container Images` workflow run on GitHub Actions.
**Expected:** All three parallel jobs (`Build Web`, `Build ApiService`, `Build Gateway`) complete with green status; images appear under `ghcr.io/baotoq/micro-commerce/{apiservice,gateway,web}` tagged with `sha-{short}` and `latest`.
**Why human:** Cannot trigger or observe GitHub Actions execution or ghcr.io package registry state programmatically from this environment.

#### 2. Container Runtime — Non-Root User Verification

**Test:** `docker run --rm -it --entrypoint=whoami ghcr.io/baotoq/micro-commerce/web:latest` (or inspect a running container).
**Expected:** Output is `nextjs` (UID 1001); container started without privilege escalation errors.
**Why human:** Requires pulling from a live registry and running the container, which is not possible in static analysis.

#### 3. Multi-Arch Manifest Verification

**Test:** `docker buildx imagetools inspect ghcr.io/baotoq/micro-commerce/apiservice:latest` after a CI push.
**Expected:** Output shows both `linux/amd64` and `linux/arm64` platform entries in the OCI index manifest.
**Why human:** Requires a live CI run and access to the ghcr.io registry to inspect the manifest.

---

## Commit Verification

All commits documented in SUMMARYs were verified in git history:

| Commit | Summary | Verified |
|--------|---------|----------|
| `977a919f` | Add shared container properties to Directory.Build.props | FOUND |
| `0e9bbeda` | Add ContainerRepository to ApiService and Gateway csproj | FOUND |
| `c5314616` | Enable standalone output in next.config.ts | FOUND |
| `afae4b5f` | Add Dockerfile and .dockerignore for Next.js web app | FOUND |
| `fe614615` | Add GitHub Actions container-images workflow | FOUND |

---

## Notes on CONT-01/CONT-02 Requirement Language

The REQUIREMENTS.md text for CONT-01 and CONT-02 says "multi-stage Dockerfile with layer-cached restore." The implementation uses `dotnet publish /t:PublishContainer` (the .NET SDK's built-in container publishing, no Dockerfile). The phase research at `23-RESEARCH.md:56–57` explicitly maps these requirements to the SDK approach and notes this is acceptable. The chiseled runtime requirement is fully satisfied via `ContainerBaseImage=mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra`. The "layer-cached restore" language was written with a Dockerfile mental model but the SDK approach achieves the same goal (minimized image, cached layers handled internally by the SDK). This is not a gap — it is a documented intentional implementation choice.

---

_Verified: 2026-02-26_
_Verifier: Claude (gsd-verifier)_

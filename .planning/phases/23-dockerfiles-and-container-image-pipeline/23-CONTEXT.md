# Phase 23: Dockerfiles and Container Image Pipeline - Context

**Gathered:** 2026-02-25
**Status:** Ready for planning

<domain>
## Phase Boundary

Three production-ready container images (ApiService, Gateway, Web) built automatically and pushed to ghcr.io. .NET services use `dotnet publish` container support; Next.js uses a traditional Dockerfile. GitHub Actions workflow handles CI builds on push to master and release tags.

</domain>

<decisions>
## Implementation Decisions

### Dockerfile strategy
- .NET services (ApiService, Gateway) use `dotnet publish` with built-in container support — NO Dockerfiles for .NET
- Container configuration (registry, base image, common settings) lives in `Directory.Build.props` — each .csproj only overrides its image name
- Next.js Web app gets a traditional multi-stage Dockerfile with `node:22-alpine` as the base image
- Next.js uses standalone output mode for minimal image size

### Image tagging & registry
- Tag format: commit SHA on every build, `latest` tag updated on master, semver tags (v1.2.3) on release tags
- Image naming: nested under project — `ghcr.io/{owner}/micro-commerce/apiservice`, `.../gateway`, `.../web`
- Multi-arch builds: linux/amd64 + linux/arm64 for Apple Silicon compatibility with local kind clusters
- No image retention/cleanup policy for now — revisit if storage becomes an issue

### CI workflow design
- Trigger on push to master (SHA + latest tags) and release tags (semver tags)
- Parallel matrix strategy — all 3 images built concurrently
- GitHub Actions cache for Docker layer caching (actions/cache or docker/build-push-action cache backend)
- Claude decides whether to create a new workflow file or extend existing release.yml

### Security & runtime
- No HEALTHCHECK in Dockerfiles — Kubernetes liveness/readiness probes handle health checking (Phase 25)
- Non-root user: .NET chiseled images enforce non-root by default; Next.js Dockerfile creates a dedicated `nextjs` user
- .NET services expose port 8080 (default non-root port for .NET 8+)
- Runtime environment variables injected via Kubernetes ConfigMaps and Secrets (Phase 25) — no baked-in defaults for secrets

### Claude's Discretion
- .NET chiseled base image selection (exact tag/variant)
- CI workflow file structure (new file vs extending existing)
- .dockerignore contents
- Next.js Dockerfile stage naming and layer optimization details
- Exact `dotnet publish` container MSBuild properties beyond registry/image name

</decisions>

<specifics>
## Specific Ideas

- .NET's built-in container publishing (`dotnet publish /t:PublishContainer`) is the explicit choice over traditional Dockerfiles for .NET services — this is a key architectural decision
- Directory.Build.props centralizes container config to avoid duplication across ApiService and Gateway
- Multi-arch support is important because the user runs kind clusters on Apple Silicon Macs

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 23-dockerfiles-and-container-image-pipeline*
*Context gathered: 2026-02-25*

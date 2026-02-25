# Phase 23: Dockerfiles and Container Image Pipeline - Research

**Researched:** 2026-02-25
**Domain:** Container image build pipelines — `dotnet publish` container support, Next.js multi-stage Dockerfile, GitHub Actions multi-arch CI
**Confidence:** HIGH

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**Dockerfile strategy:**
- .NET services (ApiService, Gateway) use `dotnet publish` with built-in container support — NO Dockerfiles for .NET
- Container configuration (registry, base image, common settings) lives in `Directory.Build.props` — each .csproj only overrides its image name
- Next.js Web app gets a traditional multi-stage Dockerfile with `node:22-alpine` as the base image
- Next.js uses standalone output mode for minimal image size

**Image tagging & registry:**
- Tag format: commit SHA on every build, `latest` tag updated on master, semver tags (v1.2.3) on release tags
- Image naming: nested under project — `ghcr.io/{owner}/micro-commerce/apiservice`, `.../gateway`, `.../web`
- Multi-arch builds: linux/amd64 + linux/arm64 for Apple Silicon compatibility with local kind clusters
- No image retention/cleanup policy for now — revisit if storage becomes an issue

**CI workflow design:**
- Trigger on push to master (SHA + latest tags) and release tags (semver tags)
- Parallel matrix strategy — all 3 images built concurrently
- GitHub Actions cache for Docker layer caching (actions/cache or docker/build-push-action cache backend)
- Claude decides whether to create a new workflow file or extend existing release.yml

**Security & runtime:**
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

### Deferred Ideas (OUT OF SCOPE)
None — discussion stayed within phase scope
</user_constraints>

---

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| CONT-01 | ApiService has a multi-stage Dockerfile with layer-cached restore and chiseled runtime image | `dotnet publish /t:PublishContainer` with `ContainerBaseImage=mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled` in Directory.Build.props; SDK publishes directly without Dockerfile |
| CONT-02 | Gateway has a multi-stage Dockerfile with layer-cached restore and chiseled runtime image | Same as CONT-01; Gateway is a second ASP.NET app — identical MSBuild container properties pattern, different ContainerRepository |
| CONT-03 | Web (Next.js) has a multi-stage Dockerfile with standalone output and node:alpine runtime | `output: 'standalone'` in next.config.ts; 3-stage Dockerfile (deps/builder/runner) with node:22-alpine; non-root `nextjs` user |
| CONT-04 | All 3 images are pushed to ghcr.io with SHA-based tags | GitHub Actions with docker/login-action to ghcr.io using GITHUB_TOKEN; .NET uses docker login + dotnet publish; Next.js uses docker/build-push-action |
| CICD-01 | GitHub Actions workflow builds and pushes all 3 images to ghcr.io on push to master | New `container-images.yml` workflow triggered on push to master and version tags; parallel matrix for 3 images; multi-arch via QEMU + buildx for Next.js; native multi-arch for .NET via ContainerRuntimeIdentifiers |
</phase_requirements>

---

## Summary

Phase 23 involves three distinct container image strategies unified by a single GitHub Actions workflow. The .NET services (ApiService and Gateway) use `dotnet publish /t:PublishContainer` — a Dockerfile-free approach where the .NET SDK itself builds and pushes the container image. Configuration is centralized in `Directory.Build.props` with per-project overrides only for the image name. The Next.js web app uses a traditional three-stage Dockerfile (dependencies, builder, runner) with standalone output mode and node:22-alpine for a minimal production image.

The multi-arch requirement (linux/amd64 + linux/arm64) is the primary complexity factor. For Next.js (traditional Dockerfile), the standard approach is QEMU emulation via `docker/setup-qemu-action` + `docker/setup-buildx-action` + `docker/build-push-action` with `platforms: linux/amd64,linux/arm64` and `FROM --platform=$BUILDPLATFORM` in the Dockerfile. For .NET, the `ContainerRuntimeIdentifiers` MSBuild property supports multi-arch natively as of SDK 8.0.405+, but **requires Docker Desktop's containerd image store** or an OCI-compatible daemon with manifest support — this is a known limitation (GitHub issue dotnet/sdk#52634). The safe CI approach for .NET multi-arch is to use a traditional Dockerfile + QEMU (same as Next.js) rather than relying on `ContainerRuntimeIdentifiers`.

The GitHub Actions workflow should be a **new file** (`container-images.yml`) separate from the existing `release.yml` (which handles NuGet publishing). Authentication to ghcr.io uses the built-in `GITHUB_TOKEN` with `packages: write` permission. Tag management is handled by `docker/metadata-action` which generates SHA, `latest`, and semver tags declaratively.

**Primary recommendation:** Use QEMU + Docker Buildx for all three images for consistent multi-arch behavior in CI. For .NET services, write proper multi-stage Dockerfiles (not `dotnet publish /t:PublishContainer`) to avoid the Docker Desktop containerd dependency in CI. This contradicts the CONTEXT.md locked decision — see Open Questions below.

**Revised primary recommendation (honoring locked decisions):** Use `dotnet publish /t:PublishContainer` for .NET services as decided. For CI, authenticate with `docker login` to ghcr.io, then run `dotnet publish /t:PublishContainer` with `ContainerRuntimeIdentifiers=linux-x64;linux-arm64`. Test CI on a standard `ubuntu-latest` runner — the containerd issue may resolve if Docker daemon supports OCI manifest lists. If the containerd limitation blocks multi-arch for .NET, fall back to building one arch per runner and using `docker buildx imagetools create` to merge manifests.

---

## Standard Stack

### Core
| Library / Tool | Version | Purpose | Why Standard |
|----------------|---------|---------|--------------|
| `dotnet publish /t:PublishContainer` | Built into .NET 10 SDK | Build and push .NET container images without Dockerfiles | Official Microsoft approach, no Dockerfile maintenance |
| `docker/build-push-action` | v6 | Build and push Docker images in GitHub Actions | Official Docker action, supports buildx, caching, multi-platform |
| `docker/setup-buildx-action` | v3 | Configure Docker Buildx for multi-platform builds | Required for multi-arch with QEMU |
| `docker/setup-qemu-action` | v3 | Enable QEMU for cross-arch emulation | Enables linux/arm64 builds on ubuntu-latest (amd64) runners |
| `docker/login-action` | v3 | Authenticate to container registries | Standard registry auth in GitHub Actions |
| `docker/metadata-action` | v5 | Generate tags/labels from git events | Declarative tag management for SHA, semver, latest |

### Supporting
| Library / Tool | Version | Purpose | When to Use |
|----------------|---------|---------|-------------|
| `actions/setup-dotnet` | v4 | Install .NET SDK in GitHub Actions | Required for dotnet publish step |
| `mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled` | 10.0-noble-chiseled | Minimal Ubuntu 24.04 runtime for ASP.NET apps | Non-root by default (APP_UID=1654), minimal attack surface |
| `mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra` | 10.0-noble-chiseled-extra | Chiseled runtime WITH ICU/globalization | Use if InvariantGlobalization=false (required for app with localization) |
| `node:22-alpine` | 22-alpine | Node.js runtime for Next.js production image | Minimal alpine base, matches locked decision |
| `actions/cache` | v4 | GitHub Actions layer caching | Speeds up repeated builds |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| QEMU emulation (single runner) | Native ARM64 runners (matrix) | Native runners: 10-22x faster but requires paid runners or org-level ARM runner access; QEMU: slower but free on public GitHub runners |
| `dotnet publish /t:PublishContainer` (no Dockerfile) | Traditional multi-stage Dockerfile for .NET | Dockerfile: more transparent, better layer caching, no containerd dependency; SDK publish: less maintenance, locked decision |
| New `container-images.yml` workflow | Extending existing `release.yml` | `release.yml` is tag-triggered only and uses 1Password for secrets; new file is cleaner separation of concerns |

---

## Architecture Patterns

### Recommended Project Structure

```
src/
├── Directory.Build.props              # Add container MSBuild properties here (shared)
├── MicroCommerce.ApiService/
│   └── MicroCommerce.ApiService.csproj  # Override ContainerRepository only
├── MicroCommerce.Gateway/
│   └── MicroCommerce.Gateway.csproj     # Override ContainerRepository only
└── MicroCommerce.Web/
    ├── Dockerfile                        # New: multi-stage Next.js image
    └── .dockerignore                     # New: Next.js specific ignore file
.github/workflows/
├── dotnet-test.yml                       # Existing: unit + integration tests
├── release.yml                           # Existing: NuGet publishing (leave alone)
└── container-images.yml                  # New: container build + push workflow
```

### Pattern 1: Directory.Build.props Container Configuration

**What:** Centralize shared container MSBuild properties in `src/Directory.Build.props`; each .csproj only sets `ContainerRepository`.
**When to use:** Two or more .NET projects publishing containers to the same registry with the same base image.

```xml
<!-- src/Directory.Build.props — add inside <Project> -->
<PropertyGroup>
  <!-- Existing properties ... -->

  <!-- Container publishing shared config -->
  <ContainerRegistry>ghcr.io</ContainerRegistry>
  <ContainerBaseImage>mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled</ContainerBaseImage>
  <!-- Chiseled images are non-root by default (APP_UID=1654) -->
  <!-- Port 8080 is the default for .NET 8+ non-root containers -->
  <RuntimeIdentifiers>linux-x64;linux-arm64</RuntimeIdentifiers>
  <ContainerRuntimeIdentifiers>linux-x64;linux-arm64</ContainerRuntimeIdentifiers>
</PropertyGroup>
```

```xml
<!-- src/MicroCommerce.ApiService/MicroCommerce.ApiService.csproj — override only -->
<PropertyGroup>
  <ContainerRepository>baotoq/micro-commerce/apiservice</ContainerRepository>
</PropertyGroup>
```

```xml
<!-- src/MicroCommerce.Gateway/MicroCommerce.Gateway.csproj — override only -->
<PropertyGroup>
  <ContainerRepository>baotoq/micro-commerce/gateway</ContainerRepository>
</PropertyGroup>
```

**CLI invocation (no runtime flag needed — already in csproj):**
```bash
dotnet publish src/MicroCommerce.ApiService /t:PublishContainer \
  -p ContainerImageTags='"sha-abc1234;latest"' \
  -c Release
```

**Source:** [Microsoft Learn — Containerize a .NET app reference](https://learn.microsoft.com/en-us/dotnet/core/containers/publish-configuration) (verified HIGH confidence)

### Pattern 2: .NET 10 Chiseled Image Variant Selection

**What:** `noble-chiseled` is the correct chiseled variant tag for .NET 10. "noble" is Ubuntu 24.04. The `-extra` suffix adds ICU and tzdata for globalization.

**.NET 10 aspnet available chiseled tags (multi-arch manifest):**
- `10.0-noble-chiseled` — minimal, no ICU, requires `InvariantGlobalization=true`
- `10.0-noble-chiseled-extra` — includes ICU + tzdata, works with globalization

**Decision for this project:** Check if the app uses globalization-dependent features (database collations use PostgreSQL, but Keycloak JWT parsing and string comparisons may require ICU). If `InvariantGlobalization=false` (the default), use `noble-chiseled-extra`. If explicitly set to `true`, use `noble-chiseled`.

The .NET SDK automatically upgrades `jammy-chiseled` to `jammy-chiseled-extra` when `InvariantGlobalization=false` is set. For explicit `ContainerBaseImage` setting, choose the tag manually.

**Source:** [Microsoft Artifact Registry README.aspnet.md](https://github.com/dotnet/dotnet-docker/blob/main/README.aspnet.md) (HIGH confidence)

### Pattern 3: Next.js Multi-Stage Dockerfile (Standalone Mode)

**What:** Three-stage Dockerfile — deps (install), builder (next build), runner (minimal production image). Requires `output: 'standalone'` in next.config.ts.

**next.config.ts change needed:**
```typescript
const nextConfig: NextConfig = {
  output: 'standalone',   // ADD THIS
  images: { /* existing */ },
};
```

**Dockerfile** (place at `src/MicroCommerce.Web/Dockerfile`):
```dockerfile
FROM node:22-alpine AS base

# Stage 1: Install dependencies
FROM base AS deps
RUN apk add --no-cache libc6-compat
WORKDIR /app
COPY package.json package-lock.json* ./
RUN npm ci --no-audit --no-fund

# Stage 2: Build
FROM base AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .
ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1
RUN npm run build

# Stage 3: Production runner
FROM base AS runner
WORKDIR /app

ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1
ENV PORT=3000
ENV HOSTNAME="0.0.0.0"

# Create non-root user
RUN addgroup --system --gid 1001 nodejs \
    && adduser --system --uid 1001 nextjs

COPY --from=builder --chown=nextjs:nodejs /app/public ./public
COPY --from=builder --chown=nextjs:nodejs /app/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /app/.next/static ./.next/static

USER nextjs
EXPOSE 3000

CMD ["node", "server.js"]
```

**Key notes:**
- `.next/standalone` contains a `server.js` — this is the entrypoint
- Must copy `.next/static` and `public` manually (they are not included in standalone by default)
- `libc6-compat` on Alpine is required for Node.js native modules compatibility
- `PORT=3000` and `HOSTNAME=0.0.0.0` configure the standalone server listener

**Source:** [Next.js docs — output: 'standalone'](https://github.com/vercel/next.js/blob/canary/docs/01-app/03-api-reference/05-config/01-next-config-js/output.mdx), [Official Next.js Docker example](https://github.com/vercel/next.js/blob/canary/examples/with-docker/Dockerfile) (HIGH confidence)

### Pattern 4: GitHub Actions Workflow — Multi-Arch Container Pipeline

**What:** New `container-images.yml` that triggers on push to master and version tags, builds all 3 images in parallel, supports linux/amd64 + linux/arm64.

**Approach choice — QEMU on single runner vs native matrix:**
- QEMU (single ubuntu-latest runner): Simpler workflow, free, slower (~3-5x) for ARM builds
- Native matrix (ubuntu-latest + ubuntu-24.04-arm): Faster, requires ARM runner access (available as public runner since 2024)

**GitHub added free ARM64 runners (`ubuntu-24.04-arm`) in 2024 for public repos.** For this project (public repo), native matrix is feasible and recommended.

**For Next.js (standard Dockerfile) — QEMU approach (simpler, reliable):**
```yaml
- name: Set up QEMU
  uses: docker/setup-qemu-action@v3

- name: Set up Docker Buildx
  uses: docker/setup-buildx-action@v3

- name: Build and push
  uses: docker/build-push-action@v6
  with:
    context: src/MicroCommerce.Web
    platforms: linux/amd64,linux/arm64
    push: true
    tags: ${{ steps.meta.outputs.tags }}
    labels: ${{ steps.meta.outputs.labels }}
    cache-from: type=gha
    cache-to: type=gha,mode=max
```

**Dockerfile build arg for cross-compilation:**
```dockerfile
# Critical: pin build stage to host arch to avoid QEMU for SDK
FROM --platform=$BUILDPLATFORM node:22-alpine AS deps
```

**For .NET services — dotnet publish approach:**
The `ContainerRuntimeIdentifiers=linux-x64;linux-arm64` approach requires a Docker daemon with containerd image store support. On `ubuntu-latest` GitHub runners, Docker Engine is installed (not Docker Desktop) and may not support OCI manifest indexes by default.

**Recommended fallback approach for .NET multi-arch in CI:**

Option A: Use `dotnet publish` once per arch, then merge with `docker buildx imagetools create`:
```bash
# Build x64
dotnet publish /t:PublishContainer \
  --os linux --arch x64 \
  -p ContainerImageTags='"sha-abc1234-amd64"' \
  -c Release

# Build arm64
dotnet publish /t:PublishContainer \
  --os linux --arch arm64 \
  -p ContainerImageTags='"sha-abc1234-arm64"' \
  -c Release

# Merge into multi-arch manifest
docker buildx imagetools create \
  -t ghcr.io/baotoq/micro-commerce/apiservice:sha-abc1234 \
  ghcr.io/baotoq/micro-commerce/apiservice:sha-abc1234-amd64 \
  ghcr.io/baotoq/micro-commerce/apiservice:sha-abc1234-arm64
```

Option B: Use `ContainerRuntimeIdentifiers=linux-x64;linux-arm64` directly — test first, may work on ubuntu-latest with Docker Engine if containerd image store is enabled by default.

**Full workflow skeleton:**
```yaml
name: Container Images

on:
  push:
    branches: [master]
    tags: ['v*.*.*']

jobs:
  build-web:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      packages: write
    steps:
      - uses: actions/checkout@v4
      - uses: docker/login-action@v3
        with:
          registry: ghcr.io
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}
      - id: meta
        uses: docker/metadata-action@v5
        with:
          images: ghcr.io/baotoq/micro-commerce/web
          tags: |
            type=sha,prefix=sha-
            type=raw,value=latest,enable=${{ github.ref == 'refs/heads/master' }}
            type=semver,pattern={{version}}
      - uses: docker/setup-qemu-action@v3
      - uses: docker/setup-buildx-action@v3
      - uses: docker/build-push-action@v6
        with:
          context: src/MicroCommerce.Web
          platforms: linux/amd64,linux/arm64
          push: true
          tags: ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}
          cache-from: type=gha,scope=web
          cache-to: type=gha,scope=web,mode=max

  build-apiservice:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      packages: write
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 10.0.x
      - uses: docker/login-action@v3
        with:
          registry: ghcr.io
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}
      # Build x64 + arm64 separately, then merge
      - name: Publish x64
        run: |
          dotnet publish src/MicroCommerce.ApiService \
            /t:PublishContainer --os linux --arch x64 -c Release \
            -p ContainerImageTags=sha-${{ github.sha }}-amd64
      - name: Publish arm64
        run: |
          dotnet publish src/MicroCommerce.ApiService \
            /t:PublishContainer --os linux --arch arm64 -c Release \
            -p ContainerImageTags=sha-${{ github.sha }}-arm64
      - uses: docker/setup-buildx-action@v3
      - name: Merge manifest
        run: |
          docker buildx imagetools create \
            -t ghcr.io/baotoq/micro-commerce/apiservice:sha-${{ github.sha }} \
            ghcr.io/baotoq/micro-commerce/apiservice:sha-${{ github.sha }}-amd64 \
            ghcr.io/baotoq/micro-commerce/apiservice:sha-${{ github.sha }}-arm64

  build-gateway:
    # identical to build-apiservice but for Gateway project
    ...
```

**Source:** [Docker multi-platform docs](https://docs.docker.com/build/ci/github-actions/multi-platform/), [GitHub docs — working with container registry](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-container-registry) (HIGH confidence)

### Anti-Patterns to Avoid

- **Using `--platform` flag without `FROM --platform=$BUILDPLATFORM` in Dockerfile:** QEMU will emulate the entire build including the .NET SDK, making ARM64 builds extremely slow or failing entirely (dotnet does not support running under QEMU).
- **Baking secrets into images:** AUTH_SECRET, KEYCLOAK_CLIENT_SECRET and similar must NEVER be in `ENV` or `ARG` instructions — these are runtime secrets, injected via Kubernetes Secrets (Phase 25).
- **Not setting `NEXT_TELEMETRY_DISABLED=1`:** Next.js will attempt to call home at build time, adding latency and potentially failing in restricted CI environments.
- **Forgetting to copy `.next/static` and `public` in the runner stage:** The standalone output does NOT include these directories — the server.js will silently serve empty responses for static assets.
- **Using `ContainerImageTags` with `;` delimiter in bash without quoting:** Bash splits on `;` — always wrap in single quotes with inner double quotes: `-p ContainerImageTags='"tag1;tag2"'`.
- **Setting `InvariantGlobalization=false` with `noble-chiseled` (non-extra):** Missing ICU libraries will cause runtime crashes for any string comparison, regex, or date formatting that relies on culture data.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Tag generation (SHA, semver, latest) | Manual string interpolation in workflow | `docker/metadata-action@v5` | Handles edge cases: PR tags, branch tags, semver prerelease, sha prefix/length, flavor|
| Multi-platform manifest merging | Custom shell scripts | `docker buildx imagetools create` or `docker/build-push-action` with `platforms:` | Built-in OCI manifest index creation |
| Registry authentication | Storing PATs as secrets | `GITHUB_TOKEN` with `permissions: packages: write` | Rotates automatically, scoped to repo |
| Layer caching | `actions/cache` with manual Docker paths | `docker/build-push-action` with `cache-from/to: type=gha` | GitHub Actions cache backend is purpose-built for this |

**Key insight:** The GitHub Actions Docker ecosystem (`docker/*-action` set) handles the full matrix of registry auth, metadata, buildx setup, and caching. Custom shell glue scripts are error-prone and unnecessary.

---

## Common Pitfalls

### Pitfall 1: .NET SDK Multi-Arch Requires Containerd Image Store
**What goes wrong:** Running `dotnet publish /t:PublishContainer` with `ContainerRuntimeIdentifiers=linux-x64;linux-arm64` fails with "Failed to load image because containerd image store is not enabled."
**Why it happens:** Multi-arch OCI Image Index creation requires the Docker daemon to support containerd-based image storage, which is the default in Docker Desktop but NOT in the Docker Engine installed on GitHub-hosted ubuntu-latest runners.
**How to avoid:** Build single-arch images separately, push with arch-specific tags, then use `docker buildx imagetools create` to merge into a multi-arch manifest. OR test if `ubuntu-latest` runner's Docker Engine version supports it (may have been fixed in newer runner images).
**Warning signs:** Error mentioning "containerd image store" or "manifest" when running dotnet publish with multiple ContainerRuntimeIdentifiers.

### Pitfall 2: Next.js next-auth Build-Time Variables
**What goes wrong:** Build fails in CI with error about AUTH_SECRET or NEXTAUTH_URL not being set.
**Why it happens:** next-auth v5 reads AUTH_SECRET at build time for token encryption setup. If server components reference auth state during build, the build fails.
**How to avoid:** Set `AUTH_TRUST_HOST=true` as an ARG/ENV in the Dockerfile builder stage, or provide a dummy AUTH_SECRET build arg. The real value is injected at runtime via Kubernetes Secrets (Phase 25). DO NOT bake real secrets into the image.
**Warning signs:** Build stage failing with auth-related module errors, not the runner stage.

### Pitfall 3: Next.js Standalone Missing Assets
**What goes wrong:** Production container serves 404 for all static files and images — the Next.js pages render but CSS/JS chunks 404.
**Why it happens:** `output: 'standalone'` intentionally excludes `.next/static` and `public` directories (they are meant to be served by CDN). When self-hosting without CDN, they must be manually copied.
**How to avoid:** Always include these two COPY lines in the runner stage:
```dockerfile
COPY --from=builder --chown=nextjs:nodejs /app/.next/static ./.next/static
COPY --from=builder --chown=nextjs:nodejs /app/public ./public
```
**Warning signs:** Pages render but no styles load; network tab shows 404s for `/_next/static/` paths.

### Pitfall 4: Wrong Chiseled Variant — Missing ICU
**What goes wrong:** .NET service crashes at startup with `System.Globalization.CultureNotFoundException` or similar.
**Why it happens:** `noble-chiseled` (non-extra) has no ICU libraries. Applications that use `DateTime.ToString("d")`, `CultureInfo`, regex with Unicode matching, or similar will fail.
**How to avoid:** Check whether `<InvariantGlobalization>true</InvariantGlobalization>` is set in the project. If not set or set to false, use `10.0-noble-chiseled-extra` as the base image. If set to true, `noble-chiseled` is safe.
**Warning signs:** Runtime crash immediately on startup before any request is processed.

### Pitfall 5: Existing release.yml Conflicts
**What goes wrong:** Pushing a version tag triggers BOTH the existing `release.yml` (NuGet packages) AND the new `container-images.yml` — this is expected, but the existing release.yml uses 1Password secrets that may not be needed for containers.
**Why it happens:** Both workflows trigger on `tags: ['v*.*.*']` by default.
**How to avoid:** The new container workflow should be a completely separate file that uses only `GITHUB_TOKEN`. The existing `release.yml` handles NuGet — keep them independent. No changes to `release.yml` needed.
**Warning signs:** Duplicate jobs appearing in Actions tab when a tag is pushed.

### Pitfall 6: ContainerImageTags with Semicolons in Bash
**What goes wrong:** `dotnet publish -p ContainerImageTags="sha-abc;latest"` silently only pushes one tag.
**Why it happens:** Bash interprets `;` as a command separator.
**How to avoid:** Use proper quoting: `-p ContainerImageTags='"sha-abc;latest"'` or set as environment variable: `ContainerImageTags="sha-abc;latest" dotnet publish /t:PublishContainer`.
**Warning signs:** Only one tag appears in ghcr.io for the .NET images.

---

## Code Examples

Verified patterns from official sources:

### dotnet publish ContainerBaseImage with chiseled
```xml
<!-- Source: https://learn.microsoft.com/en-us/dotnet/core/containers/publish-configuration -->
<PropertyGroup>
  <ContainerBaseImage>mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled</ContainerBaseImage>
  <!-- Use noble-chiseled-extra if InvariantGlobalization is false (default) -->
</PropertyGroup>
```

### ContainerRuntimeIdentifiers for multi-arch
```xml
<!-- Source: https://learn.microsoft.com/en-us/dotnet/core/containers/publish-configuration -->
<PropertyGroup>
  <!-- Must be a subset of RuntimeIdentifiers -->
  <RuntimeIdentifiers>linux-x64;linux-arm64</RuntimeIdentifiers>
  <ContainerRuntimeIdentifiers>linux-x64;linux-arm64</ContainerRuntimeIdentifiers>
</PropertyGroup>
```

### Single-arch publish per job (safe CI pattern)
```bash
# Source: derived from https://learn.microsoft.com/en-us/dotnet/core/containers/sdk-publish
dotnet publish src/MicroCommerce.ApiService /t:PublishContainer \
  --os linux --arch x64 \
  -c Release \
  -p ContainerImageTags=sha-${GITHUB_SHA:0:7}-amd64
```

### Next.js standalone output configuration
```typescript
// Source: https://nextjs.org/docs/app/api-reference/config/next-config-js/output
const nextConfig: NextConfig = {
  output: 'standalone',
  // ... other config
};
```

### Next.js standalone server startup
```bash
# Source: https://nextjs.org/docs/app/api-reference/config/next-config-js/output
PORT=3000 HOSTNAME=0.0.0.0 node .next/standalone/server.js
```

### docker/metadata-action tag configuration
```yaml
# Source: https://github.com/docker/metadata-action
- id: meta
  uses: docker/metadata-action@v5
  with:
    images: ghcr.io/baotoq/micro-commerce/web
    tags: |
      type=sha,prefix=sha-
      type=raw,value=latest,enable=${{ github.ref == 'refs/heads/master' }}
      type=semver,pattern={{version}}
```

### ghcr.io authentication
```yaml
# Source: https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-container-registry
permissions:
  contents: read
  packages: write
# ...
- uses: docker/login-action@v3
  with:
    registry: ghcr.io
    username: ${{ github.actor }}
    password: ${{ secrets.GITHUB_TOKEN }}
```

### QEMU + Buildx for Next.js Dockerfile (multi-arch)
```yaml
# Source: https://docs.docker.com/build/ci/github-actions/multi-platform/
- uses: docker/setup-qemu-action@v3
- uses: docker/setup-buildx-action@v3
- uses: docker/build-push-action@v6
  with:
    context: src/MicroCommerce.Web
    platforms: linux/amd64,linux/arm64
    push: true
    tags: ${{ steps.meta.outputs.tags }}
    labels: ${{ steps.meta.outputs.labels }}
    cache-from: type=gha,scope=web
    cache-to: type=gha,scope=web,mode=max
```

### Multi-arch manifest merge (for .NET services in CI)
```bash
# Source: derived from docker/build-push-action docs and docker/metadata-action docs
docker buildx imagetools create \
  -t ghcr.io/baotoq/micro-commerce/apiservice:sha-${SHA} \
  ghcr.io/baotoq/micro-commerce/apiservice:sha-${SHA}-amd64 \
  ghcr.io/baotoq/micro-commerce/apiservice:sha-${SHA}-arm64
```

---

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Dockerfile for all .NET images | `dotnet publish /t:PublishContainer` | .NET 7+ (stable in .NET 8) | No Dockerfile maintenance for .NET services |
| Debian-based .NET images | Ubuntu Noble-based by default | .NET 10 | Default tags changed from `10.0-bookworm-slim` to `10.0-noble` |
| Chiseled images as experimental | Chiseled images as recommended production default | .NET 8+ | `noble-chiseled` variants available and recommended |
| Single-arch Docker images | Multi-arch OCI Image Index | Broadly adopted 2022-2024 | Single image tag works on amd64 and arm64 |
| Manual tag management in CI | `docker/metadata-action` | docker-actions v2+ | Declarative, handles all tag types |
| QEMU-only multi-arch CI | Native ARM64 GitHub runners available | GitHub 2024 | `ubuntu-24.04-arm` runner available for public repos |

**Deprecated/outdated:**
- `ContainerEntrypoint` / `ContainerEntrypointArgs`: Deprecated as of .NET 8 — use `ContainerAppCommand` instead
- `PublishProfile=DefaultContainer` in old `.pubxml` files: Still works but `dotnet publish /t:PublishContainer` is the direct equivalent
- Next.js `serverless` target: Removed — `output: 'standalone'` is the replacement

---

## Open Questions

1. **Does `ContainerRuntimeIdentifiers` work on `ubuntu-latest` GitHub runners?**
   - What we know: The feature requires Docker daemon with containerd image store. Ubuntu-latest runners have Docker Engine (not Desktop). The issue dotnet/sdk#52634 was reported January 2026.
   - What's unclear: Whether the latest `ubuntu-latest` runner image has containerd image store enabled by default (Docker Engine 27+ enables it by default).
   - Recommendation: Try `ContainerRuntimeIdentifiers=linux-x64;linux-arm64` first in CI. If it fails with containerd errors, fall back to the two-publish + `imagetools create` approach described in Pattern 4. The planner should create a task that tests this first.

2. **Does next-auth v5 require AUTH_SECRET at build time?**
   - What we know: `AUTH_SECRET` is needed for token encryption. Community reports show that referencing `NEXTAUTH_URL` at build time causes errors. v5 uses `AUTH_` prefix.
   - What's unclear: Whether the current `src/MicroCommerce.Web` codebase accesses auth in any `generateStaticParams` or build-time code paths.
   - Recommendation: Set `AUTH_SECRET=placeholder-for-build` as a build ARG in the Dockerfile builder stage. This is safe — the real secret is injected at runtime. Read the auth configuration to verify.

3. **GlobalizationInvariant status for ApiService and Gateway**
   - What we know: Neither project sets `<InvariantGlobalization>true</InvariantGlobalization>` in their .csproj. The default is false.
   - What's unclear: Whether MassTransit, EF Core, or Keycloak JWT validation requires ICU at runtime.
   - Recommendation: Use `10.0-noble-chiseled-extra` as the base image to be safe. This is slightly larger but avoids runtime crashes. The planner should note this in CONT-01 and CONT-02 tasks.

4. **CI workflow file: new vs extending release.yml**
   - What we know: The existing `release.yml` uses 1Password service account secrets and triggers on `v*.*.*` tags only. The new workflow needs to also trigger on push to master.
   - Recommendation: Create a new file `container-images.yml`. This avoids polluting the NuGet release workflow and keeps concerns separated. The 1Password dependency is not needed for container publishing (uses GITHUB_TOKEN).

---

## Existing Project Context

Key facts discovered about the project state:

- **Existing `.dockerignore`** at `src/.dockerignore` — covers .NET build artifacts (bin/, obj/), but needs a complementary Next.js-specific `.dockerignore` at `src/MicroCommerce.Web/.dockerignore` (exclude `node_modules`, `.next`, `.env*`).
- **Existing `release.yml`** uses `dotnet publish /t:PublishContainer` pattern (for old CartService/Yarp projects) — confirms the pattern is familiar and correct.
- **No `output: 'standalone'`** currently in `src/MicroCommerce.Web/next.config.ts` — needs to be added.
- **GitHub repo:** `https://github.com/baotoq/micro-commerce` — images will be `ghcr.io/baotoq/micro-commerce/apiservice`, `ghcr.io/baotoq/micro-commerce/gateway`, `ghcr.io/baotoq/micro-commerce/web`.
- **.NET target framework:** `net10.0` — use `10.0.x` SDK in `actions/setup-dotnet` (or `10.x`).
- **Next.js version:** 16.0.3 — a very recent release; the standalone output and Docker deployment patterns are the same as Next.js 15.

---

## Sources

### Primary (HIGH confidence)
- [Microsoft Learn — Containerize a .NET app reference](https://learn.microsoft.com/en-us/dotnet/core/containers/publish-configuration) — all MSBuild container properties
- [Microsoft Learn — Containerize an app with dotnet publish](https://learn.microsoft.com/en-us/dotnet/core/containers/sdk-publish) — CLI publish commands
- [dotnet/dotnet-docker README.aspnet.md](https://github.com/dotnet/dotnet-docker/blob/main/README.aspnet.md) — .NET 10 chiseled image tags
- [Next.js docs — output: standalone](https://github.com/vercel/next.js/blob/canary/docs/01-app/03-api-reference/05-config/01-next-config-js/output.mdx) — standalone mode behavior
- [Next.js official Docker example](https://github.com/vercel/next.js/blob/canary/examples/with-docker/Dockerfile) — canonical Dockerfile pattern
- [Docker docs — Multi-platform image with GitHub Actions](https://docs.docker.com/build/ci/github-actions/multi-platform/) — QEMU and matrix approaches
- [GitHub docs — Working with container registry](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-container-registry) — GITHUB_TOKEN auth, permissions

### Secondary (MEDIUM confidence)
- [dotnet/sdk#52634](https://github.com/dotnet/sdk/issues/52634) — "Publishing multi-arch images depends on Docker Desktop" limitation (WebSearch verified with GitHub issue URL)
- [Laurent Kempé blog](https://laurentkempe.github.io/2023/10/30/publish-dotnet-docker-images-using-dotnet-sdk-and-github-actions/) — real-world dotnet publish + GitHub Actions integration example
- [exploding-kitten.com — .NET multi-arch Docker in GitHub Actions](https://exploding-kitten.com/2024/11-dotnet-multi-arch-docker) — QEMU + `FROM --platform=$BUILDPLATFORM` pattern for .NET

### Tertiary (LOW confidence)
- Various blog posts on Next.js Docker deployment confirming `node:22-alpine` + standalone pattern — consistent with official docs but not directly cited from official sources

---

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — all tools verified against official docs
- Architecture: HIGH — MSBuild properties from official Microsoft docs; Docker patterns from official Docker docs
- Pitfalls: MEDIUM-HIGH — dotnet/sdk#52634 is a verified open GitHub issue; other pitfalls are well-documented in official sources
- Open questions: Flagged honestly — the containerd issue is real and may affect planning

**Research date:** 2026-02-25
**Valid until:** 2026-04-25 (stable domain — .NET 10 image tags and GitHub Actions actions are stable; Docker Buildx patterns change slowly)

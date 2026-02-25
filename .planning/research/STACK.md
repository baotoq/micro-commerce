# Stack Research: Kubernetes & GitOps Deployment

**Domain:** Kubernetes deployment with GitOps for .NET 10 microservices e-commerce platform
**Researched:** 2026-02-25
**Confidence:** HIGH

## Context: What Already Exists (Do Not Re-Research)

The following are validated and committed — this research covers only new additions:

- .NET 10, ASP.NET Core Minimal APIs, .NET Aspire 13.1.0
- Next.js 16, React 19, TypeScript 5
- PostgreSQL, Keycloak, YARP Gateway
- MassTransit 9.0.0 with Azure Service Bus (will be supplemented with RabbitMQ transport for K8s)
- 182 automated tests, full DDD building block foundation

## New Stack for v3.0 Kubernetes & GitOps

### Container Build

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| Docker multi-stage build | N/A (Dockerfile pattern) | Build minimal production images | Reduces .NET image from ~2GB (SDK) to ~167MB (runtime-only). Standard for production .NET containers. |
| mcr.microsoft.com/dotnet/aspnet | 10.0 | .NET API/Gateway runtime base image | Official Microsoft image, minimal surface area. Use `aspnet` not `sdk` in final stage. |
| mcr.microsoft.com/dotnet/sdk | 10.0 | Build stage only | Full SDK needed for `dotnet publish`, not included in final image. |
| node:22-alpine | 22-alpine | Next.js build stage | Matches Node 22 LTS. Alpine variant for smaller intermediate image. |
| node:22-alpine | 22-alpine | Next.js runtime base | Use `output: "standalone"` in next.config.js to copy only required files (~229MB vs 892MB). |

#### .NET Dockerfile Pattern (ApiService and Gateway)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Copy csproj files first for layer caching — only re-restores when deps change
COPY ["src/MicroCommerce.ApiService/MicroCommerce.ApiService.csproj", "src/MicroCommerce.ApiService/"]
COPY ["src/MicroCommerce.ServiceDefaults/MicroCommerce.ServiceDefaults.csproj", "src/MicroCommerce.ServiceDefaults/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.csproj", "src/BuildingBlocks/BuildingBlocks.Common/"]
RUN dotnet restore "src/MicroCommerce.ApiService/MicroCommerce.ApiService.csproj"
COPY . .
RUN dotnet publish "src/MicroCommerce.ApiService/MicroCommerce.ApiService.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
# Run as non-root user (built into aspnet image)
USER app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MicroCommerce.ApiService.dll"]
```

#### Next.js Dockerfile Pattern

```dockerfile
FROM node:22-alpine AS deps
WORKDIR /app
COPY src/MicroCommerce.Web/package*.json ./
RUN npm ci

FROM node:22-alpine AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY src/MicroCommerce.Web .
# next.config.js must have: output: "standalone"
RUN npm run build

FROM node:22-alpine AS runner
WORKDIR /app
ENV NODE_ENV=production
USER node
COPY --from=builder /app/.next/standalone ./
COPY --from=builder /app/.next/static ./.next/static
COPY --from=builder /app/public ./public
EXPOSE 3000
CMD ["node", "server.js"]
```

### Container Registry

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| ghcr.io (GitHub Container Registry) | N/A | Store and serve Docker images | Free for public repos, integrated with GitHub Actions via `GITHUB_TOKEN`. No separate auth service needed. Images live next to code. |

### Local Kubernetes Development

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| kind (Kubernetes IN Docker) | v0.31.0 | Local K8s cluster via Docker containers | Default K8s 1.35.0, no VM overhead, Mac/Linux/Windows support, fast cluster teardown/recreation, CI-compatible. Chosen over minikube (heavier VM-based) and k3d (less community support). |

#### kind Cluster Config (multi-node for realistic testing)

```yaml
# kind-config.yaml
kind: Cluster
apiVersion: kind.x-k8s.io/v1alpha4
nodes:
  - role: control-plane
  - role: worker
  - role: worker
```

### Kubernetes Configuration Management

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| Kustomize | v5.8.1 (standalone) or built into kubectl | K8s manifest management, environment overlays | Native to kubectl (no separate install needed for basic use), no templating language to learn, plain YAML output = easy to audit, first-class ArgoCD support. Use Kustomize for internal services (ApiService, Gateway, Web). Use Helm only for third-party infra (PostgreSQL, RabbitMQ, Keycloak). |

#### Kustomize Directory Layout

```
k8s/
  base/                           # Common manifests for all environments
    apiservice/
      deployment.yaml
      service.yaml
      kustomization.yaml
    gateway/
      deployment.yaml
      service.yaml
      kustomization.yaml
    web/
      deployment.yaml
      service.yaml
      kustomization.yaml
    kustomization.yaml            # Aggregates all components
  overlays/
    dev/                          # kind local cluster overrides
      kustomization.yaml          # patches: image tags, replicas=1, resources
      patches/
        resource-limits.yaml
    prod/                         # Production overrides
      kustomization.yaml
      patches/
        replicas.yaml
        resource-limits.yaml
  infra/                          # Third-party infra (Helm-rendered or raw manifests)
    rabbitmq/
    postgres/
    keycloak/
    sealed-secrets/
    otel-collector/
    aspire-dashboard/
```

### GitOps Continuous Delivery

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| ArgoCD | v3.3.2 | GitOps CD controller | Latest stable (Feb 22, 2026). App-of-apps pattern lets one ArgoCD Application deploy all others from Git. Declarative, Git-native, rich UI for diff visualization and rollback. v3.3.2 fixes client-side apply migration issue in v3.3.0/3.3.1. |

#### ArgoCD App-of-Apps Structure

```yaml
# argocd/apps/root-app.yaml — the single application you bootstrap with
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: micro-commerce
  namespace: argocd
spec:
  source:
    repoURL: https://github.com/your-org/micro-commerce
    targetRevision: HEAD
    path: argocd/apps          # Points to folder containing child Application manifests
  destination:
    server: https://kubernetes.default.svc
    namespace: argocd
  syncPolicy:
    automated:
      prune: true
      selfHeal: true
```

### Secrets Management

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| Sealed Secrets (controller) | v0.35.0 | Encrypt K8s Secrets for safe Git storage | Feb 12, 2026 release. One-way encryption: encrypt with public key, only controller in cluster can decrypt. Secrets live in Git as SealedSecret CRDs. Works natively with ArgoCD (ArgoCD syncs the SealedSecret, controller decrypts to Secret). Simpler than Vault for a showcase project. |
| kubeseal (CLI) | v0.35.0 | Encrypt secrets locally before committing | Matches controller version. `kubeseal --fetch-cert` gets cluster public key. |

#### Sealed Secrets Workflow

```bash
# Install controller
kubectl apply -f https://github.com/bitnami-labs/sealed-secrets/releases/download/v0.35.0/controller.yaml

# Seal a secret
kubectl create secret generic db-credentials \
  --from-literal=password=mysecret \
  --dry-run=client -o yaml \
  | kubeseal --format=yaml > k8s/base/sealed-db-credentials.yaml

# SealedSecret is safe to commit; controller decrypts it in-cluster
```

### Message Broker (K8s replacement for Azure Service Bus emulator)

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| RabbitMQ Cluster Operator | v2.19.1 | Deploy and manage RabbitMQ in K8s | Feb 6, 2026 release. Official operator from RabbitMQ team. Manages RabbitMQ as first-class K8s resource via `RabbitmqCluster` CRD. Handles restarts, upgrades, clustering. Simpler than self-managed StatefulSet. |
| RabbitMQ | 4.1.3 (default via operator) | Message broker | Deployed by operator. Supports MassTransit's direct-exchange + durable-queue topology natively. |
| MassTransit.RabbitMQ | 9.0.1 | MassTransit RabbitMQ transport | Latest stable NuGet (Feb 7, 2026). Drop-in replacement for `MassTransit.Azure.ServiceBus.Core`. Only `UsingRabbitMq` call changes — consumers, saga, outbox, circuit breaker, retry config remain identical. |

#### MassTransit Transport Switch (ApiService Program.cs)

Replace `UsingAzureServiceBus` with `UsingRabbitMq` based on environment:

```csharp
// In Program.cs — transport selected by environment variable or config
if (builder.Environment.IsKubernetes()) // custom extension or env var check
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq"));
        cfg.ConfigureEndpoints(context);
    });
}
else
{
    x.UsingAzureServiceBus((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("messaging"));
        cfg.ConfigureEndpoints(context);
    });
}
```

**Important:** Remove the Azure Service Bus-specific DLQ routing callback when using RabbitMQ — the `IServiceBusReceiveEndpointConfigurator` cast will silently no-op on RabbitMQ endpoints, which is fine, but the `DeadLetterQueueService` that reads the ASB DLQ API must be stubbed or conditionally excluded in K8s mode.

#### NuGet Change for RabbitMQ

```xml
<!-- Remove -->
<PackageReference Include="MassTransit.Azure.ServiceBus.Core" Version="9.0.0" />

<!-- Add -->
<PackageReference Include="MassTransit.RabbitMQ" Version="9.0.1" />
```

Note: Keep `MassTransit.Azure.ServiceBus.Core` for Aspire local dev if you want both transports. Use conditional compilation or environment-based config switching.

### Observability (K8s)

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| OpenTelemetry Collector (contrib) | v0.143.0 | Receive OTLP from apps, fan-out to backends | `otel/opentelemetry-collector-contrib` image on Docker Hub. Contrib distribution includes Kubernetes metadata enrichment, Prometheus exporter, and OTLP forwarding out of the box. Deploy as K8s Deployment (sidecar not needed for this topology). |
| .NET Aspire Dashboard (standalone) | mcr.microsoft.com/dotnet/aspire-dashboard:latest | Visualize traces, metrics, logs in K8s | Deploy as K8s Deployment. Receives OTLP directly (port 18888 UI, port 4317 OTLP gRPC). In-memory only — clears on pod restart, which is acceptable for a dev/showcase cluster. Set `ASPIRE_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true` in K8s for no-auth access. |

#### Observability Flow in K8s

```
Apps (ApiService, Gateway, Web)
  → OTLP gRPC → OTEL Collector (port 4317)
  → OTEL Collector fans out to:
      → Aspire Dashboard (port 4317) for UI visualization
      → Prometheus (future) if metrics scraping needed
```

**Why OTEL Collector in front of Aspire Dashboard?**
The Collector adds K8s pod/namespace metadata enrichment, provides a stable OTLP endpoint (apps don't need to know about dashboard location), and allows future fan-out without app changes.

#### OTEL Collector Config (K8s ConfigMap)

```yaml
receivers:
  otlp:
    protocols:
      grpc:
        endpoint: 0.0.0.0:4317
      http:
        endpoint: 0.0.0.0:4318

processors:
  batch:
  k8sattributes:  # Adds pod/namespace labels to telemetry

exporters:
  otlp/aspire:
    endpoint: aspire-dashboard:4317
    tls:
      insecure: true

service:
  pipelines:
    traces:
      receivers: [otlp]
      processors: [batch, k8sattributes]
      exporters: [otlp/aspire]
    metrics:
      receivers: [otlp]
      processors: [batch]
      exporters: [otlp/aspire]
    logs:
      receivers: [otlp]
      processors: [batch]
      exporters: [otlp/aspire]
```

### CI/CD Pipeline

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| GitHub Actions | N/A | CI automation (build, test, push images) | Already integrated (dotnet-test.yml exists). Free for public repos. Native ghcr.io auth via `GITHUB_TOKEN`. |
| docker/build-push-action | v6.19.2 | Build and push Docker images in CI | Latest stable. Supports BuildKit, layer caching via `type=gha`, multi-platform builds. |
| docker/metadata-action | v5 | Generate image tags/labels from Git refs | Auto-generates `ghcr.io/org/repo:sha-abc1234` and `ghcr.io/org/repo:latest` tags from Git context. |
| docker/setup-buildx-action | v3 | Enable BuildKit (required for layer caching) | Must run before build-push-action when using `cache-from: type=gha`. |
| actions/checkout | v4 | Checkout repository | Current stable. |

#### GitHub Actions CI Workflow Pattern

```yaml
# .github/workflows/ci.yml
name: CI
on:
  push:
    branches: [master]
  pull_request:
    branches: [master]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.x'
      - run: dotnet test src/MicroCommerce.ApiService.Tests

  build-and-push:
    needs: test
    runs-on: ubuntu-latest
    # Only push on master, not on PRs (PRs only build to validate)
    permissions:
      contents: read
      packages: write
    steps:
      - uses: actions/checkout@v4
      - uses: docker/setup-buildx-action@v3
      - uses: docker/login-action@v3
        with:
          registry: ghcr.io
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}
      - uses: docker/metadata-action@v5
        id: meta
        with:
          images: ghcr.io/${{ github.repository }}/apiservice
          tags: |
            type=sha,prefix=sha-
            type=raw,value=latest,enable=${{ github.ref == 'refs/heads/master' }}
      - uses: docker/build-push-action@v6.19.2
        with:
          context: .
          file: src/MicroCommerce.ApiService/Dockerfile
          push: ${{ github.ref == 'refs/heads/master' }}
          tags: ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}
          cache-from: type=gha
          cache-to: type=gha,mode=max
```

Repeat the build-and-push job for `gateway` and `web` images (can be a matrix strategy).

### Infrastructure Charts (Third-Party, Helm-Managed)

For third-party components, use Helm charts rendered to static YAML (or let ArgoCD render them). This avoids embedding Helm in the GitOps flow for internal services.

| Component | Chart | Version | Notes |
|-----------|-------|---------|-------|
| PostgreSQL | bitnami/postgresql | 18.4.0 | Single instance for dev. K8s `PersistentVolumeClaim` for data. Connection string via `SealedSecret`. |
| RabbitMQ | rabbitmq/cluster-operator + `RabbitmqCluster` CR | Operator v2.19.1 | Preferred over bitnami chart for K8s — operator manages lifecycle, CRD-based config. |
| Keycloak | official Keycloak K8s manifests | 26.x | Import realm JSON via `ConfigMap`. Use bitnami chart only if operator unavailable. |
| Sealed Secrets | bitnami-labs/sealed-secrets | Helm chart v2.18.1 | Controller deployed via Helm. |

## Integration with Existing .NET Aspire Setup

Aspire continues to be the local development entry point (`dotnet run --project src/MicroCommerce.AppHost`). The K8s deployment path is parallel — not a replacement for local Aspire dev.

**Key integration points:**

1. **Connection strings via environment variables** — Aspire injects `ConnectionStrings__appdb`, `ConnectionStrings__messaging` etc. In K8s, these same env var names come from `ConfigMap` + `SealedSecret`. No app code changes needed beyond the MassTransit transport switch.

2. **OTEL endpoint** — Aspire injects `OTEL_EXPORTER_OTLP_ENDPOINT` automatically. In K8s, set this to the OTEL Collector service URL (e.g., `http://otel-collector:4317`). Apps are already instrumented.

3. **Aspire Dashboard in K8s** — The standalone dashboard container (`mcr.microsoft.com/dotnet/aspire-dashboard`) accepts the same OTLP traffic the apps already emit. No app changes needed.

4. **MassTransit outbox** — The EF Core outbox configuration (`AddEntityFrameworkOutbox<CatalogDbContext>`) is transport-agnostic. The same outbox works with RabbitMQ as it does with Azure Service Bus.

5. **Health endpoints** — `/health` and `/alive` already exist via `MapDefaultEndpoints()`. K8s `livenessProbe` and `readinessProbe` use these as-is.

## Alternatives Considered

| Category | Recommended | Alternative | When to Use Alternative |
|----------|-------------|-------------|-------------------------|
| Local K8s | kind | minikube | Use minikube if you need LoadBalancer support natively or GPU passthrough. kind is lighter for CI. |
| Local K8s | kind | k3d (k3s in Docker) | k3d has built-in registry support. Use if registry mirroring is needed locally. |
| Config Management | Kustomize (internal) + Helm (infra) | Pure Helm | Use pure Helm if you need complex parameterization across many environments. Overkill for a showcase. |
| Config Management | Kustomize | Jsonnet/Cue | Only if you need programmatic config generation at scale. Not justified here. |
| GitOps | ArgoCD | FluxCD | FluxCD is also valid. ArgoCD chosen for richer UI and easier bootstrapping for a showcase. PROJECT.md notes FluxCD as constraint but v3.0 requirements specify ArgoCD. |
| Secrets | Sealed Secrets | HashiCorp Vault | Use Vault for enterprise multi-team secret management, dynamic secrets, audit trails. Overkill for showcase. |
| Secrets | Sealed Secrets | External Secrets Operator + AWS/Azure secrets | Use External Secrets if secrets already live in cloud provider secret store. |
| Broker (K8s) | RabbitMQ | NATS | NATS is lighter but has less MassTransit maturity. RabbitMQ is MassTransit's most tested transport. |
| Broker (K8s) | RabbitMQ | Keep Azure Service Bus emulator in K8s | The ASB emulator is not production-grade and not officially supported in K8s. RabbitMQ is the correct K8s-native choice. |
| CI | GitHub Actions | Jenkins/GitLab CI | No reason to switch — GitHub Actions already integrated. |
| Image Registry | ghcr.io | Docker Hub | Docker Hub rate-limits unauthenticated pulls. ghcr.io is free, co-located with code, and uses `GITHUB_TOKEN`. |

## What NOT to Use

| Avoid | Why | Use Instead |
|-------|-----|-------------|
| Helm for internal service manifests | Introduces templating complexity, harder to audit plain YAML, unnecessary for services your team owns | Kustomize overlays for ApiService, Gateway, Web |
| Istio service mesh | Massive operational overhead, complex mTLS config, overkill for single-cluster showcase | YARP Gateway already handles auth, rate limiting, routing at application layer |
| Kubernetes Operators for your own apps | Operators are for complex stateful workloads (databases, message brokers). For stateless .NET services, Deployment + Service is correct. | Standard K8s Deployment + Service + ConfigMap |
| `latest` tag for application images | Non-deterministic deployments, can't roll back to a specific commit | SHA-based tags (e.g., `sha-abc1234`) for ArgoCD image tracking |
| Pushing to main branch from PRs | Race conditions, deploys unreviewed code | Only build on PR, push images only on master merge |
| Storing secrets in plain K8s Secrets in Git | Secrets visible in plaintext in Git history | Sealed Secrets for all sensitive values |
| Running .NET apps as root in containers | Security violation, unnecessary privilege | `USER app` (built into aspnet base image) |
| Azure Service Bus emulator in K8s | Not production-grade, not officially supported for K8s deployment | RabbitMQ with Cluster Operator |
| Aspire AppHost for K8s production | AppHost is a local dev orchestrator, not a production deployment tool | Kustomize + ArgoCD for K8s |

## Version Compatibility

| Component | Version | Compatible With | Notes |
|-----------|---------|-----------------|-------|
| kind v0.31.0 | K8s 1.35.0 default | Docker 25+, Mac/Linux/Windows | Breaking: cgroup v1 removed in K8s 1.35+, use digest-pinned images |
| ArgoCD v3.3.2 | K8s 1.26+ | Kustomize 5.x built-in | v3.3.2 fixes apply migration issue from v3.3.0/3.3.1 |
| Kustomize v5.8.1 | K8s 1.20+ | ArgoCD v3.x | v5.8.1 fixes namespace propagation regression from v5.8.0 |
| Sealed Secrets v0.35.0 | K8s 1.16+ | kubeseal v0.35.0 | Controller and CLI must match major.minor version |
| MassTransit.RabbitMQ 9.0.1 | .NET 8+ / .NET Standard 2.0 | RabbitMQ 3.x/4.x | Compatible with existing MassTransit 9.0.0 outbox/saga config |
| RabbitMQ Cluster Operator v2.19.1 | K8s 1.19+ | RabbitMQ 4.1.3 | Operator and RabbitmqCluster CR are versioned together |
| OTEL Collector v0.143.0 | K8s 1.19+ | OTLP 1.9.0 | v0.143.0 note: OCB/OpAMPSupervisor artifacts broken in this release — use v0.142.0 if you need those tools |
| Aspire Dashboard (latest tag) | K8s 1.19+ | OTLP (gRPC/HTTP) | In-memory only — clears on pod restart. Acceptable for dev/showcase. |
| docker/build-push-action v6.19.2 | GitHub Actions ubuntu-latest | Docker BuildKit | Requires docker/setup-buildx-action@v3 for GHA cache backend |

## Sources

- [kind v0.31.0 Release](https://github.com/kubernetes-sigs/kind/releases) — version verified, defaults to K8s 1.35.0 (HIGH confidence)
- [ArgoCD v3.3.2 Release](https://github.com/argoproj/argo-cd/releases) — latest stable Feb 22, 2026 (HIGH confidence)
- [Kustomize v5.8.1 Release](https://github.com/kubernetes-sigs/kustomize/releases) — latest stable Feb 9, 2026, fixes namespace propagation (HIGH confidence)
- [Sealed Secrets v0.35.0 Release](https://github.com/bitnami-labs/sealed-secrets/releases) — latest stable Feb 12, 2026 (HIGH confidence)
- [Sealed Secrets Helm chart v2.18.1](https://artifacthub.io/packages/helm/bitnami-labs/sealed-secrets) — latest Helm chart (HIGH confidence)
- [MassTransit.RabbitMQ 9.0.1 on NuGet](https://www.nuget.org/packages/MassTransit.RabbitMQ/) — latest stable Feb 7, 2026 (HIGH confidence)
- [RabbitMQ Cluster Operator v2.19.1](https://github.com/rabbitmq/cluster-operator/releases) — latest stable Feb 6, 2026 (HIGH confidence)
- [OTEL Collector v0.143.0/v1.49.0](https://github.com/open-telemetry/opentelemetry-collector-releases/releases) — Jan 2026 release, note OCB artifact issue (MEDIUM confidence — use v0.142.0 for OCB)
- [Aspire Dashboard standalone docs](https://aspire.dev/dashboard/standalone/) — `mcr.microsoft.com/dotnet/aspire-dashboard:latest`, ports 18888/4317 (HIGH confidence)
- [docker/build-push-action v6.19.2](https://github.com/docker/build-push-action/releases) — Feb 12, 2025 release (HIGH confidence)
- [Kustomize vs Helm 2026 analysis](https://tasrieit.com/blog/helm-vs-kustomize-kubernetes-comparison-2026) — hybrid Kustomize+Helm recommendation for microservices (MEDIUM confidence)
- [ArgoCD app-of-apps pattern](https://argo-cd.readthedocs.io/en/stable/operator-manual/cluster-bootstrapping/) — official docs (HIGH confidence)
- [MassTransit RabbitMQ Configuration](https://masstransit.io/documentation/configuration/transports/rabbitmq) — official docs (HIGH confidence)
- [.NET multi-stage Dockerfile](https://learn.microsoft.com/en-us/dotnet/core/docker/build-container) — official Microsoft docs (HIGH confidence)
- [Next.js standalone output](https://nextjs.org/docs/app/getting-started/deploying) — official Next.js docs (HIGH confidence)

---
*Stack research for: Kubernetes & GitOps Deployment (v3.0 milestone)*
*Researched: 2026-02-25*
*Replaces: Previous STACK.md (DDD Building Blocks focus — that file covered v2.0 milestone)*

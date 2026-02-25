# Architecture Research: Kubernetes & GitOps Deployment

**Domain:** K8s deployment for .NET modular monolith with GitOps
**Researched:** 2026-02-25
**Confidence:** HIGH

## Standard Architecture

### System Overview

```
┌──────────────────────────────────────────────────────────────────┐
│                    GitHub Actions (CI)                            │
│   Test → Build Images → Push ghcr.io → Update image tags        │
└────────────────────────────┬─────────────────────────────────────┘
                             │ git push (image tag update)
┌────────────────────────────▼─────────────────────────────────────┐
│                 Git Repo (GitOps source of truth)                 │
│   deploy/base/         deploy/overlays/dev/      argocd/         │
└────────────────────────────┬─────────────────────────────────────┘
                             │ ArgoCD polls / syncs
┌────────────────────────────▼─────────────────────────────────────┐
│              Kubernetes Cluster (kind for dev)                    │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │   Ingress (NGINX) — routes *.micro-commerce.local           │ │
│  └──────┬─────────────────────────────────────────────┬────────┘ │
│         │ /                                           │ /api/*   │
│  ┌──────▼──────────┐                        ┌────────▼────────┐  │
│  │ Web (Next.js)   │                        │ Gateway (YARP)  │  │
│  │ port 3000       │                        │ port 8080       │  │
│  └─────────────────┘                        └────────┬────────┘  │
│                                                      │           │
│                                             ┌────────▼────────┐  │
│                                             │ ApiService      │  │
│                                             │ (.NET 10)       │  │
│                                             └────────┬────────┘  │
│                                                      │           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────▼────────┐  │
│  │  PostgreSQL  │  │  RabbitMQ    │  │  Keycloak             │  │
│  │  (StatefulSet│  │  (StatefulSet│  │  (StatefulSet)        │  │
│  └──────────────┘  └──────────────┘  └───────────────────────┘  │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────────┐ │
│  │  Monitoring: OTEL Collector + Aspire Dashboard               │ │
│  └──────────────────────────────────────────────────────────────┘ │
│                                                                   │
│  ┌──────────────┐  ┌──────────────┐                             │
│  │  ArgoCD      │  │  Sealed      │                             │
│  │  (GitOps)    │  │  Secrets     │                             │
│  └──────────────┘  └──────────────┘                             │
└──────────────────────────────────────────────────────────────────┘
```

### Component Responsibilities

| Component | Responsibility | K8s Resource |
|-----------|---------------|--------------|
| Web (Next.js) | Customer-facing SSR storefront + admin UI | Deployment + ClusterIP Service |
| Gateway (YARP) | Centralized auth, rate limiting, routing to ApiService | Deployment + ClusterIP Service |
| ApiService (.NET 10) | Modular monolith backend — all feature modules | Deployment + ClusterIP Service |
| PostgreSQL | Shared database, schema-per-feature (8 DbContexts) | StatefulSet + PVC |
| RabbitMQ | Domain events transport (replaces Azure SB emulator) | StatefulSet + PVC |
| Keycloak | JWT issuer and identity provider | StatefulSet + PVC |
| NGINX Ingress | External traffic routing, hostname-based dispatch | IngressClass + Ingress |
| ArgoCD | GitOps controller pulling from Git, applying manifests | Deployment (system namespace) |
| Sealed Secrets | In-cluster decryption of encrypted secrets committed to Git | CRD controller |
| OTEL Collector | Receives OTLP telemetry, fans out to Aspire Dashboard | Deployment |
| Aspire Dashboard | Developer observability (traces, metrics, logs) | Deployment |

## Recommended Project Structure

```
deploy/
├── base/                           # Shared K8s manifests (all environments)
│   ├── kustomization.yaml          # Lists all resources
│   ├── namespace.yaml              # micro-commerce namespace
│   │
│   ├── apiservice/
│   │   ├── deployment.yaml
│   │   ├── service.yaml
│   │   └── configmap.yaml         # Non-secret config (OTEL endpoint, etc.)
│   │
│   ├── gateway/
│   │   ├── deployment.yaml
│   │   ├── service.yaml
│   │   └── configmap.yaml         # YARP route config (appsettings.json content)
│   │
│   ├── web/
│   │   ├── deployment.yaml
│   │   └── service.yaml
│   │
│   ├── postgres/
│   │   ├── statefulset.yaml
│   │   ├── service.yaml
│   │   └── pvc.yaml
│   │
│   ├── rabbitmq/
│   │   ├── statefulset.yaml
│   │   ├── service.yaml
│   │   └── pvc.yaml
│   │
│   ├── keycloak/
│   │   ├── statefulset.yaml
│   │   ├── service.yaml
│   │   ├── pvc.yaml
│   │   └── configmap.yaml         # Realm JSON mounted as volume
│   │
│   ├── monitoring/
│   │   ├── otel-collector.yaml    # Deployment + Service + ConfigMap
│   │   └── aspire-dashboard.yaml  # Deployment + Service
│   │
│   └── ingress/
│       └── ingress.yaml           # Routes to gateway and web
│
├── overlays/
│   └── dev/                        # kind cluster overlay
│       ├── kustomization.yaml      # References base, applies patches
│       ├── patches/
│       │   ├── apiservice-image.yaml    # Image tag patch
│       │   ├── gateway-image.yaml
│       │   ├── web-image.yaml
│       │   └── resource-limits.yaml    # Relaxed limits for dev
│       └── sealed-secrets/
│           ├── postgres-credentials.yaml      # SealedSecret
│           ├── rabbitmq-credentials.yaml      # SealedSecret
│           ├── keycloak-credentials.yaml      # SealedSecret
│           └── app-secrets.yaml              # SealedSecret (AUTH_SECRET, etc.)
│
└── argocd/
    ├── apps/                       # App-of-apps child applications
    │   ├── apiservice-app.yaml
    │   ├── gateway-app.yaml
    │   ├── web-app.yaml
    │   ├── postgres-app.yaml
    │   ├── rabbitmq-app.yaml
    │   ├── keycloak-app.yaml
    │   └── monitoring-app.yaml
    └── root-app.yaml               # Parent app pointing to apps/

src/
├── MicroCommerce.AppHost/          # Aspire orchestrator (local dev only)
├── MicroCommerce.ApiService/
│   └── Dockerfile                  # Multi-stage .NET 10 build
├── MicroCommerce.Gateway/
│   └── Dockerfile                  # Multi-stage .NET 10 build
└── MicroCommerce.Web/
    └── Dockerfile                  # Multi-stage Next.js 16 build

.github/
└── workflows/
    ├── dotnet-test.yml             # Existing: unit + integration tests
    ├── docker-build.yml            # New: build + push images to ghcr.io
    └── update-manifests.yml        # New: update image tags in deploy/ overlays
```

### Structure Rationale

- **deploy/base/:** Common manifests shared across all environments. No image tags here — they are patched in overlays. This is the DRY foundation.
- **deploy/overlays/dev/:** Kind cluster-specific configuration. Contains image tag patches and sealed secrets encrypted for this cluster's key.
- **deploy/argocd/:** GitOps operator configuration. App-of-apps pattern keeps each service independently syncable while a single root app bootstraps everything.
- **Dockerfiles co-located with source:** Each service owns its build definition, following standard practice and enabling `docker build -f src/MicroCommerce.ApiService/Dockerfile .` from repo root.

## Architectural Patterns

### Pattern 1: Aspire for Dev, K8s Manifests for Cluster

**What:** .NET Aspire remains the local development orchestrator. Kubernetes manifests are the production/cluster deployment target. These are parallel paths — not one replacing the other.

**When to use:** Always. Aspire provides the fastest inner loop (hot reload, dashboard, dependency injection), while Kustomize manifests provide the declarative cluster state.

**Trade-offs:**
- Pro: Developers keep fast Aspire workflow unchanged.
- Pro: Cluster deployment is fully declarative and GitOps-driven.
- Con: Two deployment models to maintain (Aspire AppHost + K8s manifests).
- Mitigation: The Aspire Kubernetes integration (`Aspire.Hosting.Kubernetes`) can generate a starter manifest — use as a starting point, not final output.

**Example — Aspire (local):**
```csharp
// AppHost.cs — stays unchanged for local dev
var apiService = builder.AddProject<Projects.MicroCommerce_ApiService>("apiservice")
    .WithReference(keycloak)
    .WithReference(appDb)
    .WithReference(messaging);  // still Azure SB emulator locally
```

**Example — K8s Deployment:**
```yaml
# deploy/base/apiservice/deployment.yaml
spec:
  containers:
    - name: apiservice
      env:
        - name: ConnectionStrings__appdb
          valueFrom:
            secretKeyRef:
              name: postgres-credentials
              key: connection-string
        - name: ConnectionStrings__messaging
          valueFrom:
            secretKeyRef:
              name: rabbitmq-credentials
              key: amqp-url
```

### Pattern 2: Service Discovery — Aspire Env Vars vs K8s DNS

**What:** Aspire injects `services__<name>__https__0` environment variables for service-to-service discovery. Kubernetes uses DNS-based discovery via ClusterIP services.

**Aspire approach (local):**
```
services__gateway__https__0 = https://localhost:5210
services__apiservice__https__0 = https://localhost:7180
```

**K8s approach (cluster):**
```
# YARP appsettings.json in Gateway's ConfigMap
"Clusters": {
  "apiservice": {
    "Destinations": {
      "default": {
        "Address": "http://apiservice.micro-commerce.svc.cluster.local:8080"
      }
    }
  }
}
```

**Key implication:** The Gateway's YARP `appsettings.json` destination address changes from `https+http://apiservice` (Aspire service discovery notation) to `http://apiservice:8080` (K8s DNS). This is the primary configuration delta between environments. The simplest approach is a ConfigMap containing the full YARP config, mounted over the image's embedded `appsettings.json`.

**Next.js gateway URL:** In Aspire, the frontend reads `process.env.services__gateway__https__0`. In K8s, set `API_URL=http://gateway.micro-commerce.svc.cluster.local:8080` as an environment variable in the Web deployment. The `/api/config` route already falls back to `process.env.API_URL`.

**Health check endpoints:** Currently gated behind `IsDevelopment()` check in `ServiceDefaults/Extensions.cs`. In K8s, `ASPNETCORE_ENVIRONMENT=Development` must be set, OR the health checks must be made unconditional. Liveness/readiness probes depend on `/health` and `/alive`.

### Pattern 3: RabbitMQ Replaces Azure Service Bus in K8s

**What:** MassTransit's transport is swapped from `UsingAzureServiceBus` to `UsingRabbitMq` for the K8s environment. The consumer/saga/outbox configuration is transport-agnostic and does not change.

**Why RabbitMQ:** Azure Service Bus emulator requires Docker/Azurite and is not available as a Kubernetes-native resource. RabbitMQ runs as a StatefulSet, has a Helm chart, and integrates well with MassTransit v8/v9.

**MassTransit v8 vs v9 note:** MassTransit v9 (current in project) transitioned to commercial licensing. v8 remains Apache 2.0 with support through end of 2026. Since the project already uses v9, the commercial license applies. For a showcase project, evaluate whether to stay on v9 or pin to v8. `MassTransit.RabbitMQ` package is part of the same release.

**Code change — ApiService Program.cs:**
```csharp
// Replace this (Azure SB):
x.UsingAzureServiceBus((context, cfg) =>
{
    cfg.Host(builder.Configuration.GetConnectionString("messaging"));
    cfg.ConfigureEndpoints(context);
});

// With this (RabbitMQ):
x.UsingRabbitMq((context, cfg) =>
{
    cfg.Host(builder.Configuration["RabbitMQ__Host"] ?? "localhost", "/", h =>
    {
        h.Username(builder.Configuration["RabbitMQ__Username"] ?? "guest");
        h.Password(builder.Configuration["RabbitMQ__Password"] ?? "guest");
    });
    cfg.ConfigureEndpoints(context);
});
```

**Conditional transport (dev vs K8s):** Use environment variable `TRANSPORT=RabbitMQ` in K8s deployments and let Aspire set `TRANSPORT=AzureServiceBus` for local. This avoids Aspire client integration packages loading in K8s where there is no Azure emulator.

**DLQ handling:** The `DeadLetterQueueService` is Azure SB specific. In K8s/RabbitMQ, MassTransit routes failed messages to `_error` queues. The `MessagingEndpoints` need a RabbitMQ-aware implementation or the feature is disabled in K8s for now.

### Pattern 4: GitOps with ArgoCD App-of-Apps

**What:** A root ArgoCD `Application` (`root-app.yaml`) points to `deploy/argocd/apps/`. Each file in that directory is an ArgoCD `Application` manifest pointing to a subdirectory in `deploy/overlays/dev/`. ArgoCD automatically reconciles cluster state to match Git.

**Build order (CI/CD pipeline):**
```
1. GitHub Actions: dotnet test (unit + integration)
2. GitHub Actions: docker build + push (ApiService, Gateway, Web) → ghcr.io
3. GitHub Actions: git commit image tag updates to deploy/overlays/dev/patches/
4. ArgoCD: detects Git change, syncs affected Application(s)
5. K8s: rolls out new pods with updated image tags
```

**Example root app:**
```yaml
# deploy/argocd/root-app.yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: micro-commerce-root
  namespace: argocd
spec:
  project: default
  source:
    repoURL: https://github.com/your-org/micro-commerce
    targetRevision: HEAD
    path: deploy/argocd/apps
  destination:
    server: https://kubernetes.default.svc
    namespace: argocd
  syncPolicy:
    automated:
      prune: true
      selfHeal: true
```

**Example child app:**
```yaml
# deploy/argocd/apps/apiservice-app.yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: apiservice
  namespace: argocd
spec:
  project: default
  source:
    repoURL: https://github.com/your-org/micro-commerce
    targetRevision: HEAD
    path: deploy/overlays/dev
  destination:
    server: https://kubernetes.default.svc
    namespace: micro-commerce
  syncPolicy:
    automated:
      prune: true
      selfHeal: true
```

### Pattern 5: Sealed Secrets for GitOps-Safe Secret Management

**What:** `kubeseal` CLI encrypts a plain K8s Secret using the cluster's public key. The encrypted `SealedSecret` YAML is committed to Git. The Sealed Secrets controller in the cluster decrypts it back to a real K8s Secret.

**Workflow:**
```bash
# 1. Create plain secret (never committed)
kubectl create secret generic postgres-credentials \
  --from-literal=connection-string="Host=postgres;Database=appdb;..." \
  --dry-run=client -o yaml | \
  kubeseal --format yaml > deploy/overlays/dev/sealed-secrets/postgres-credentials.yaml

# 2. Commit the SealedSecret to Git — safe for public repos
# 3. Sealed Secrets controller decrypts it when applied to cluster
```

**Scope:** Secrets are namespace-scoped by default — a SealedSecret for `micro-commerce` namespace can only be decrypted by the controller when applied in that namespace.

### Pattern 6: Dockerfile Multi-Stage Build

**ApiService and Gateway — .NET 10:**
```dockerfile
# Build stage — uses full SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY --link *.slnx .
COPY --link src/ src/
RUN dotnet restore
RUN dotnet publish src/MicroCommerce.ApiService/MicroCommerce.ApiService.csproj \
    -c Release -o /app --no-restore

# Runtime stage — chiseled Ubuntu (~100MB smaller, fewer CVEs)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS runtime
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "MicroCommerce.ApiService.dll"]
```

**Web — Next.js 16 (standalone output):**
```dockerfile
FROM node:22-slim AS deps
WORKDIR /app
COPY package*.json ./
RUN npm ci --only=production

FROM node:22-slim AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .
ENV NEXT_TELEMETRY_DISABLED=1
RUN npm run build

FROM node:22-slim AS runner
WORKDIR /app
ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1
COPY --from=builder /app/.next/standalone ./
COPY --from=builder /app/.next/static ./.next/static
COPY --from=builder /app/public ./public
USER node
EXPOSE 3000
CMD ["node", "server.js"]
```

**next.config.ts requirement:** Must set `output: 'standalone'` for the server.js entrypoint to exist.

**Build context:** All Dockerfiles reference `../..` (repo root) as context to access the `src/` directory. Build from repo root:
```bash
docker build -f src/MicroCommerce.ApiService/Dockerfile -t apiservice:latest .
docker build -f src/MicroCommerce.Gateway/Dockerfile -t gateway:latest .
docker build -f src/MicroCommerce.Web/Dockerfile -t web:latest src/MicroCommerce.Web/
```

## Data Flow

### Request Flow (K8s)

```
Browser (external)
    |
    ▼
NGINX Ingress (hostname routing)
    |── micro-commerce.local/api/* ──► Gateway (YARP) :8080
    |                                      |
    |                                      ▼ HTTP (ClusterIP DNS)
    |                               ApiService :8080
    |                                      |
    |                                      ├──► PostgreSQL :5432
    |                                      ├──► RabbitMQ :5672
    |                                      └──► Keycloak :8080
    |
    └── micro-commerce.local/* ──────► Web (Next.js) :3000
                                           |
                                           ▼ server-side fetch (ClusterIP)
                                       Gateway :8080 ──► ApiService :8080
```

### Service Discovery Transition (Aspire → K8s)

| Connection | Aspire (local) | K8s (cluster) |
|------------|---------------|---------------|
| Gateway → ApiService | `https+http://apiservice` (Aspire SD) | `http://apiservice.micro-commerce.svc.cluster.local:8080` |
| Web → Gateway | `services__gateway__https__0` env var | `API_URL=http://gateway.micro-commerce.svc.cluster.local:8080` |
| ApiService → PostgreSQL | Aspire ConnectionString injection | K8s Secret → `ConnectionStrings__appdb` |
| ApiService → RabbitMQ | `ConnectionStrings__messaging` (Azure SB) | K8s Secret → `RabbitMQ__Host`, `RabbitMQ__Username`, `RabbitMQ__Password` |
| ApiService → Keycloak | `services__keycloak__*` (Aspire SD) | `Keycloak__Authority=http://keycloak:8080/realms/micro-commerce` |
| All services → OTEL | Aspire Dashboard (auto-configured) | `OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317` |

### Event Flow (RabbitMQ in K8s)

```
ApiService Handler
    ├── Saves to DB + CatalogDbContext Outbox (transactional)
    └── DomainEventInterceptor publishes via MassTransit
            |
            ▼
        RabbitMQ exchange (auto-created by MassTransit)
            |
            ├──► Inventory Consumer (stock reservation)
            ├──► Ordering Consumer (saga state machine)
            └──► _error queue (failed messages, replaces Azure DLQ)
```

## Build Order

The build order below respects dependencies between artifacts:

```
Phase 1 — Foundation (no inter-dependencies)
├── 1a. kind cluster setup + local registry (localhost:5001)
├── 1b. NGINX Ingress controller installed
├── 1c. Sealed Secrets controller installed
└── 1d. ArgoCD installed in argocd namespace

Phase 2 — Infrastructure Manifests + Secrets
├── 2a. PostgreSQL StatefulSet + PVC + Service
├── 2b. RabbitMQ StatefulSet + PVC + Service
├── 2c. Keycloak StatefulSet + PVC + Service + realm ConfigMap
└── 2d. SealedSecrets encrypted and committed to Git

Phase 3 — Dockerfiles
├── 3a. ApiService Dockerfile (depends on: src structure understood)
├── 3b. Gateway Dockerfile (depends on: src structure)
└── 3c. Web Dockerfile (depends on: next.config.ts output:standalone)

Phase 4 — Application Manifests (depend on: Docker images exist)
├── 4a. ApiService Deployment + Service + ConfigMap
├── 4b. Gateway Deployment + Service + ConfigMap (YARP routes)
└── 4c. Web Deployment + Service

Phase 5 — CI/CD Pipeline (depends on: Dockerfiles + manifests complete)
├── 5a. GitHub Actions: docker-build.yml (build + push to ghcr.io)
└── 5b. GitHub Actions: update-manifests.yml (commit image tag patches)

Phase 6 — GitOps (depends on: manifests + CI complete)
├── 6a. ArgoCD root-app.yaml committed to Git
├── 6b. ArgoCD child app manifests committed
└── 6c. ArgoCD syncs cluster (end-to-end GitOps loop)

Phase 7 — Monitoring (depends on: apps running)
├── 7a. OTEL Collector Deployment + ConfigMap (exporters)
└── 7b. Aspire Dashboard Deployment + Service
```

## Scaling Considerations

| Scale | Architecture Adjustments |
|-------|--------------------------|
| 1 dev | kind cluster, all StatefulSets with 1 replica, shared resources |
| Small team | Replace kind with cloud K8s (EKS/GKE/AKS), add HPA on ApiService |
| Production | Separate PostgreSQL to managed service (RDS/CloudSQL), external RabbitMQ, Keycloak HA |

### Scaling Priorities

1. **First bottleneck:** ApiService CPU under load — add HPA with `targetCPUUtilizationPercentage: 70`.
2. **Second bottleneck:** PostgreSQL write throughput — connection pooling via PgBouncer sidecar or managed service.

## Anti-Patterns

### Anti-Pattern 1: Aspire Service Discovery Notation in K8s

**What people do:** Leave `https+http://apiservice` (Aspire service discovery shorthand) in YARP config when deploying to K8s.

**Why it's wrong:** The `https+http://` scheme is resolved by `Microsoft.Extensions.ServiceDiscovery` which reads Aspire-injected environment variables. In K8s, those variables don't exist — the YARP proxy silently fails to route requests.

**Do this instead:** Replace with a ConfigMap mounting `appsettings.json` that uses plain `http://apiservice.micro-commerce.svc.cluster.local:8080` in the YARP destinations. Use `ASPNETCORE_ENVIRONMENT=Production` (no Aspire SD) or inject environment-specific config via Kustomize ConfigMap patch.

### Anti-Pattern 2: Baking Secrets into ConfigMaps

**What people do:** Put connection strings, passwords, or API keys in ConfigMap data fields (they're plain text).

**Why it's wrong:** ConfigMaps are not encrypted. Anyone with kubectl access to the namespace can read them. They also appear in Git history if committed.

**Do this instead:** Use `secretKeyRef` in env references and SealedSecrets for the values. Separate configuration (ConfigMap) from credentials (Secret/SealedSecret).

### Anti-Pattern 3: Health Checks Gated Behind IsDevelopment

**What people do:** Leave `MapDefaultEndpoints()` in ServiceDefaults as-is, which only exposes `/health` and `/alive` in the `Development` environment.

**Why it's wrong:** Kubernetes liveness and readiness probes require those endpoints unconditionally. Pods fail to start or appear unhealthy because the health endpoint returns 404 in non-Development environments.

**Do this instead:** Either set `ASPNETCORE_ENVIRONMENT=Development` in the K8s deployment (acceptable for a showcase), or modify `ServiceDefaults/Extensions.cs` to expose health endpoints unconditionally and guard only sensitive diagnostics behind the environment check.

### Anti-Pattern 4: One Giant ArgoCD Application for All Services

**What people do:** Point a single ArgoCD app at `deploy/overlays/dev/` and sync everything together.

**Why it's wrong:** A failing deployment for one service blocks all others. You also lose per-service sync status visibility in the ArgoCD UI.

**Do this instead:** App-of-apps pattern — one ArgoCD Application per service, managed by a root app. Each service syncs independently and shows its own health status.

### Anti-Pattern 5: Image Tag "latest" in Manifests

**What people do:** Use `image: ghcr.io/org/apiservice:latest` in the base deployment manifest.

**Why it's wrong:** ArgoCD cannot detect changes when the tag doesn't change. Kubernetes also caches `latest` pulls unpredictably. Rolling back becomes impossible.

**Do this instead:** CI pipeline generates a unique tag (git SHA or `YYYY.MMDD.HHmm`) and commits it to `deploy/overlays/dev/patches/apiservice-image.yaml` as a strategic merge patch. ArgoCD detects the Git change and rolls out the new image.

### Anti-Pattern 6: Skipping the MassTransit Transport Abstraction

**What people do:** Conditionally compile or ifdef RabbitMQ vs Azure SB configuration.

**Why it's wrong:** Creates divergent code paths that are hard to test and maintain.

**Do this instead:** Use environment variable `MASSTRANSIT_TRANSPORT=RabbitMQ` (default: `AzureServiceBus` for Aspire). In Program.cs, read the variable and call either `UsingRabbitMq` or `UsingAzureServiceBus`. This keeps Aspire local dev working without changes while K8s deployments use RabbitMQ.

## Integration Points

### Modified Components (Existing → K8s Aware)

| Component | Modification | Reason |
|-----------|-------------|--------|
| `ApiService/Program.cs` | Conditional transport: RabbitMQ vs Azure SB | K8s uses RabbitMQ; Aspire uses Azure SB emulator |
| `ServiceDefaults/Extensions.cs` | Expose health endpoints unconditionally | K8s probes need `/health` and `/alive` always |
| `Gateway/appsettings.json` | YARP destination uses K8s DNS | Aspire SD notation not valid in K8s |
| `Web/src/lib/config.ts` | Falls back to `API_URL` env var | Already coded; K8s sets `API_URL` |
| `Web/next.config.ts` | Add `output: 'standalone'` | Required for standalone Docker build |

### New Components (K8s Specific)

| Component | Type | Purpose |
|-----------|------|---------|
| `src/MicroCommerce.ApiService/Dockerfile` | New file | Multi-stage .NET 10 build |
| `src/MicroCommerce.Gateway/Dockerfile` | New file | Multi-stage .NET 10 build |
| `src/MicroCommerce.Web/Dockerfile` | New file | Multi-stage Next.js 16 standalone build |
| `deploy/base/**` | New directory | Kustomize base manifests |
| `deploy/overlays/dev/**` | New directory | Kind cluster overlay + patches |
| `deploy/argocd/**` | New directory | App-of-apps GitOps config |
| `deploy/overlays/dev/sealed-secrets/**` | New directory | Encrypted secrets safe for Git |
| `.github/workflows/docker-build.yml` | New file | CI: build + push to ghcr.io |
| `.github/workflows/update-manifests.yml` | New file | CI: commit image tag updates |

### Internal Boundaries in K8s

| Boundary | Communication | Notes |
|----------|---------------|-------|
| Browser ↔ Ingress | External HTTP/HTTPS | NGINX routes by path prefix |
| Ingress ↔ Gateway | ClusterIP HTTP | Internal only, no TLS needed |
| Ingress ↔ Web | ClusterIP HTTP | Internal only |
| Web ↔ Gateway | ClusterIP HTTP (server-side) | Next.js SSR fetches via `API_URL` |
| Gateway ↔ ApiService | ClusterIP HTTP | YARP upstream address |
| ApiService ↔ PostgreSQL | ClusterIP TCP 5432 | Connection string from Secret |
| ApiService ↔ RabbitMQ | ClusterIP AMQP 5672 | MassTransit connection from Secret |
| ApiService ↔ Keycloak | ClusterIP HTTP 8080 | JWT validation OIDC discovery |
| All apps ↔ OTEL Collector | ClusterIP gRPC 4317 | `OTEL_EXPORTER_OTLP_ENDPOINT` |

## Sources

- [Kubernetes Kustomize documentation](https://kubernetes.io/docs/tasks/manage-kubernetes-objects/kustomization/) — HIGH confidence (official)
- [ArgoCD cluster bootstrapping / app-of-apps](https://argo-cd.readthedocs.io/en/stable/operator-manual/cluster-bootstrapping/) — HIGH confidence (official)
- [MassTransit RabbitMQ quick start](https://masstransit.io/quick-starts/rabbitmq) — HIGH confidence (official)
- [MassTransit v9 announcement](https://masstransit.io/introduction/v9-announcement) — HIGH confidence (official)
- [Sealed Secrets GitHub](https://github.com/bitnami-labs/sealed-secrets) — HIGH confidence (official)
- [kind local registry](https://kind.sigs.k8s.io/docs/user/local-registry/) — HIGH confidence (official)
- [Next.js with-docker Dockerfile](https://github.com/vercel/next.js/blob/canary/examples/with-docker/Dockerfile) — HIGH confidence (official)
- [.NET 10 container images](https://github.com/dotnet/dotnet-docker/discussions/6801) — HIGH confidence (official GitHub)
- [YARP Kubernetes ingress](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/kubernetes-ingress) — HIGH confidence (official)
- [Aspire Kubernetes integration](https://aspire.dev/integrations/compute/kubernetes/) — MEDIUM confidence (official but Aspire K8s integration is still evolving)
- [OpenTelemetry Kubernetes collector](https://opentelemetry.io/docs/platforms/kubernetes/collector/) — HIGH confidence (official)

---
*Architecture research for: Kubernetes & GitOps deployment of MicroCommerce*
*Researched: 2026-02-25*

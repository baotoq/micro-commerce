# Pitfalls Research

**Domain:** Kubernetes & GitOps deployment for .NET Aspire-based microservices platform
**Researched:** 2026-02-25
**Confidence:** HIGH (critical pitfalls verified against official docs and community sources)

---

## Critical Pitfalls

### Pitfall 1: .NET Dockerfile — Merging Restore and Build into One Layer

**What goes wrong:**
The Dockerfile copies all source files then runs `dotnet restore` and `dotnet build` together. Any change to any `.cs` file invalidates the restore layer, causing full NuGet package downloads on every build. For a solution with 6 projects and their transitive dependency graph, this adds 2-4 minutes to every CI run.

**Why it happens:**
Developers write `COPY . .` first because it seems natural, then run `dotnet restore && dotnet build`. The caching benefit of separating restore from build is non-obvious until CI times become painful.

**How to avoid:**
Use two-phase COPY in every service Dockerfile. Copy project files first, restore, then copy the full source:
```dockerfile
# Stage 1: restore — only invalidated when *.csproj or *.slnx changes
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/MicroCommerce.ApiService/MicroCommerce.ApiService.csproj", "src/MicroCommerce.ApiService/"]
COPY ["src/MicroCommerce.ServiceDefaults/MicroCommerce.ServiceDefaults.csproj", "src/MicroCommerce.ServiceDefaults/"]
COPY ["BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.csproj", "BuildingBlocks/BuildingBlocks.Common/"]
RUN dotnet restore "src/MicroCommerce.ApiService/MicroCommerce.ApiService.csproj"

# Stage 2: build — invalidated by any source change
COPY . .
RUN dotnet publish "src/MicroCommerce.ApiService/MicroCommerce.ApiService.csproj" \
    -c Release --no-restore -o /app/publish
```
Pass `--no-restore` to `dotnet publish`. The restore layer is cached as long as `.csproj` files do not change.

In GitHub Actions, add `cache-from: type=gha` and `cache-to: type=gha,mode=max` to `docker/build-push-action` to persist layer cache across workflow runs.

**Warning signs:**
- `dotnet restore` appears in CI build logs after changing only a `.cs` file
- CI build time does not improve between commits that only modify source code
- NuGet package download lines visible in CI logs on every run
- Second image build is as slow as the first (no layer reuse)

**Phase to address:** Phase 1 — Dockerfiles

---

### Pitfall 2: Missing `output: 'standalone'` in Next.js Config Before Dockerizing

**What goes wrong:**
Without `output: 'standalone'` in `next.config.ts`, the Docker image must include the full `node_modules` tree (500MB+ image). The standard `npm start` does not forward `SIGTERM` from Kubernetes, causing pods to miss graceful shutdown signals and dropping in-flight requests during rolling updates.

The current `next.config.ts` does not have `output: 'standalone'` — it only configures `images.remotePatterns`. This is a local-dev config that was never hardened for container deployment.

**Why it happens:**
Next.js standalone output is an opt-in build mode. The default `next build` produces a `node_modules`-heavy output tree. Developers assume the output directory is self-contained — it is not. Additionally, the standalone server does NOT automatically include `public/` or `.next/static/` directories; they must be copied explicitly in the Dockerfile.

**How to avoid:**
1. Add to `next.config.ts`:
```typescript
const nextConfig: NextConfig = {
  output: 'standalone',
  images: { remotePatterns: [...] }
};
```
2. After `next build`, manually copy static assets in the Dockerfile:
```dockerfile
COPY --from=builder /app/.next/standalone ./
COPY --from=builder /app/.next/static ./.next/static
COPY --from=builder /app/public ./public
```
3. Start the container with `node server.js`, not `npm start`, so `SIGTERM` is forwarded to Node directly.
4. The `next.config.ts` `images.remotePatterns` currently hardcodes `127.0.0.1:10000` (Azurite). This must be parameterized for K8s where blob storage will have a different hostname.

**Warning signs:**
- Docker image for Next.js exceeds 800MB
- `standalone/` directory not present after `next build`
- Pod logs show immediate exit when Kubernetes sends `SIGTERM` during rolling updates
- Static assets return 404 in the running container despite build succeeding

**Phase to address:** Phase 1 — Dockerfiles

---

### Pitfall 3: Aspire Service Discovery Environment Variables Conflicting with Kubernetes DNS

**What goes wrong:**
Aspire injects service URLs via environment variables like `services__gateway__https__0` and `services__apiservice__http__0`. In Kubernetes, these variables are absent, so services fall back to `appsettings.json` values that still point to `localhost` or Aspire-managed ports. The result is YARP Gateway calling `http://localhost:5000` for the ApiService — its own loopback — instead of the K8s service DNS name. There is a known Aspire issue (GitHub #3698, #5096) where service discovery endpoint env vars do not override appsettings values consistently in all scenarios.

**Why it happens:**
Aspire's `AddServiceDefaults()` registers an `IConfiguration`-backed service discovery provider. When running outside Aspire (in K8s), the `appsettings.json` `services` section still has Aspire-era values. Developers assume "it works in Aspire, it will resolve differently in K8s" without verifying the fallback chain. The MicroCommerce Gateway uses `WithReference(apiService)` in `AppHost.cs` — this Aspire-managed reference does not translate to K8s manifests automatically.

**How to avoid:**
In Kubernetes manifests, explicitly set environment variables for every inter-service URL using K8s DNS via Kustomize overlays:
```yaml
env:
  - name: services__apiservice__http__0
    value: "http://apiservice-svc:8080"
  - name: services__keycloak__http__0
    value: "http://keycloak-svc:8080"
  - name: services__keycloak__https__0
    value: "http://keycloak-svc:8080"
```
Audit `appsettings.json` and `appsettings.Development.json` for any localhost or Aspire-specific URLs. Use `kubectl exec` to verify env vars inside pods after deployment.

**Warning signs:**
- `HttpRequestException: Connection refused` in Gateway pod logs pointing to `localhost` addresses
- `ECONNREFUSED 127.0.0.1` in Next.js pod logs for API calls
- Services work in local Aspire but fail immediately after K8s deployment
- `kubectl exec` into pod shows `services__gateway__*` env vars absent or pointing to localhost

**Phase to address:** Phase 2 — Kustomize manifests

---

### Pitfall 4: MassTransit Transport Swap Breaking the Outbox, DLQ, and Saga

**What goes wrong:**
The existing `Program.cs` uses `x.UsingAzureServiceBus(...)` with `builder.AddAzureServiceBusClient("messaging")` (Aspire integration), `IServiceBusReceiveEndpointConfigurator` for DLQ routing, and `AddEntityFrameworkOutbox<CatalogDbContext>` for the transactional outbox. Swapping to `UsingRabbitMq(...)` while leaving the `IServiceBusReceiveEndpointConfigurator` cast causes a silent runtime no-op — the DLQ config block is silently skipped rather than throwing. Meanwhile, `builder.AddAzureServiceBusClient("messaging")` still tries to connect to Azure Service Bus and throws at startup.

The `IDeadLetterQueueService` used by `MessagingEndpoints` depends on `ServiceBusClient` (Azure SDK) which will have no valid connection string in the K8s environment using RabbitMQ.

**Why it happens:**
The endpoint configuration callback in `Program.cs` has a transport-specific cast:
```csharp
if (cfg is IServiceBusReceiveEndpointConfigurator sb)
{
    sb.ConfigureDeadLetterQueueErrorTransport();
}
```
This silently becomes a no-op with RabbitMQ. Developers trust MassTransit's transport-agnostic promise and assume zero code changes are needed. The Aspire `AddAzureServiceBusClient` is a separate DI registration from MassTransit's transport configuration.

**How to avoid:**
1. Remove `builder.AddAzureServiceBusClient("messaging")` from `Program.cs` for K8s mode. The `IDeadLetterQueueService` that uses `ServiceBusClient` must be disabled or re-implemented for RabbitMQ management API.
2. Replace the transport registration:
```csharp
x.UsingRabbitMq((context, cfg) =>
{
    cfg.Host(builder.Configuration["RabbitMq:Host"], "/", h =>
    {
        h.Username(builder.Configuration["RabbitMq:Username"]);
        h.Password(builder.Configuration["RabbitMq:Password"]);
    });
    cfg.ConfigureEndpoints(context);
});
```
3. Remove the `IServiceBusReceiveEndpointConfigurator` DLQ block from `AddConfigureEndpointsCallback`. RabbitMQ dead-lettering uses exchange arguments configured at queue declaration time.
4. The EF Core outbox (`AddEntityFrameworkOutbox<CatalogDbContext>`) is transport-agnostic and does not require changes.
5. Use an environment variable (e.g., `MASSTRANSIT_TRANSPORT=rabbitmq`) to select transport at startup, allowing local Aspire (Azure SB) and K8s (RabbitMQ) to use different transports from the same image.
6. The `CheckoutStateMachine` correlation by `OrderId` works identically with RabbitMQ, but verify saga queue names match between environments — Azure Service Bus uses topics/subscriptions; RabbitMQ uses exchanges/queues with different naming conventions.

**Warning signs:**
- `InvalidOperationException` about missing Service Bus connection string at startup
- DLQ not populated with failed messages after transport swap
- Checkout saga `CheckoutState` rows in database with stuck `Submitted` state (stock reservation response never received)
- `CheckoutStateMachine` duplicate message processing (inbox deduplication silently non-functional)
- RabbitMQ management UI shows queues with unexpected naming vs Azure Service Bus queue names

**Phase to address:** Phase 3 — Transport configuration (dedicated phase for transport swap)

---

### Pitfall 5: EF Core Migrations Race Condition in Init Containers on Cold Start

**What goes wrong:**
With 8 separate DbContexts (catalog, cart, ordering, inventory, profiles, reviews, wishlists, outbox schemas), running `dotnet ef database update` as an init container causes race conditions when multiple pods start simultaneously or when the PostgreSQL pod is not yet fully ready. The migration init container exits non-zero because PostgreSQL refuses connections during its own initialization sequence, triggering a pod `CrashLoopBackOff`. Multiple replicas each running migrations can also deadlock on the `__EFMigrationsHistory` table.

**Why it happens:**
Kubernetes starts init containers as soon as the pod is scheduled. If PostgreSQL is still initializing (cluster cold-start, pod restart), connections are refused and the migration runner crashes immediately. Kubernetes sees the non-zero exit code, restarts the pod, and the cycle repeats. With 8 schemas in one database, concurrent migrations across multiple pods can deadlock on the history table even though schemas are separate.

**How to avoid:**
1. Use a single init container that runs all migrations sequentially, preceded by a `pg_isready` wait loop:
```bash
#!/bin/bash
until pg_isready -h $POSTGRES_HOST -p 5432 -U $POSTGRES_USER; do
  echo "Waiting for PostgreSQL..."
  sleep 2
done
dotnet ef database update --context CatalogDbContext
dotnet ef database update --context CartDbContext
dotnet ef database update --context OrderingDbContext
# ... all 8 contexts in dependency order
```
2. Alternatively, call `db.Database.MigrateAsync()` at application startup with a PostgreSQL advisory lock to prevent concurrent migrations from multiple replicas:
```csharp
await using var advisoryLock = await db.Database.ExecuteSqlRawAsync(
    "SELECT pg_advisory_lock(1234567890)");
await db.Database.MigrateAsync();
```
3. Use the same base image for both the migration init container and the ApiService to ensure `dotnet ef` binary matches the application runtime.
4. Set `restartPolicy: Always` on the init container so failed attempts retry automatically.

**Warning signs:**
- Pod stuck in `Init:CrashLoopBackOff` on cluster cold start
- `connection refused` errors in init container logs referencing PostgreSQL host
- Some schemas created, others missing after pod restart (partial migration state)
- Integration tests in CI work against Testcontainers but K8s deployment fails cold-start

**Phase to address:** Phase 2 — Kustomize manifests (init container strategy)

---

### Pitfall 6: ArgoCD Sync Loop from Non-Idempotent Kustomize Output

**What goes wrong:**
ArgoCD compares live cluster state against the rendered `kustomize build` output on every reconciliation cycle. If any Kustomize component generates output that differs between renders — a `commonAnnotations` value containing a dynamic timestamp, a `newTag` that ArgoCD has already applied but the overlay still specifies an old value, or a resource with `helm.sh/chart` annotation added by Helm — the application never reaches `Synced` status and continuously triggers sync operations.

**Why it happens:**
Developers use `commonAnnotations` with build-time values. Kustomize `images:` block with a `newTag` pointing to `latest` causes ArgoCD to re-evaluate on every reconciliation because the digest changes. Any managed field ownership conflict from client-side apply `kubectl` commands versus ArgoCD's apply creates perpetual drift.

**How to avoid:**
1. Never use dynamic values in Kustomize `commonAnnotations`. Use static values. Image tags go only in the `images:` block with immutable digests or specific version tags (never `latest`).
2. Enable `ServerSideApply` in ArgoCD Application spec to eliminate field ownership conflicts:
```yaml
spec:
  syncPolicy:
    syncOptions:
      - ServerSideApply=true
```
3. Add `ignoreDifferences` for any Kubernetes-managed fields that legitimately change without Git commits:
```yaml
spec:
  ignoreDifferences:
    - group: apps
      kind: Deployment
      jsonPointers:
        - /spec/template/metadata/annotations/kubectl.kubernetes.io~1last-applied-configuration
```
4. Run `kustomize build k8s/overlays/dev | kubectl diff -` before committing to verify output is stable across multiple renders.
5. For the app-of-apps pattern: the root Application must NOT point at the directory that also contains child Application manifests unless ArgoCD sync waves are configured.

**Warning signs:**
- ArgoCD application perpetually shows `OutOfSync` despite no Git changes
- `argocd app diff` shows the same fields changing every sync cycle
- `application-controller` pod CPU pegged above 80%
- Sync operations complete but immediately re-trigger (< 3 minute cycle)

**Phase to address:** Phase 4 — ArgoCD GitOps

---

### Pitfall 7: Kustomize Strategic Merge Patch Targeting Wrong Container Name

**What goes wrong:**
The overlay patch updates `resources.requests.memory` for the `apiservice` container but the base Deployment names the container `api-service` (with a hyphen). The strategic merge patch silently adds a new container entry instead of updating the existing one. Kubernetes accepts the Deployment with two containers — the original plus a new one with only the resource patch applied and no image. The pod fails to schedule or starts with wrong configuration.

**Why it happens:**
Kustomize strategic merge patch on `containers[]` uses the container `name` field as the merge key. A naming mismatch between base and overlay silently adds rather than merges. There is no validation error from Kustomize or Kubernetes — the Deployment is accepted and the misconfiguration is only detectable by inspecting the running pod spec.

**How to avoid:**
1. Establish a naming standard immediately: container names use `lowercase` matching the service identifier exactly (e.g., `apiservice`, `gateway`, `frontend`) — no hyphens, no abbreviations.
2. Prefer JSON 6902 patches for surgical container-level changes, as they fail explicitly if the target path does not exist:
```yaml
patches:
  - patch: |-
      - op: replace
        path: /spec/template/spec/containers/0/resources/requests/memory
        value: "512Mi"
    target:
      kind: Deployment
      name: apiservice
```
3. After writing any patch, run `kustomize build k8s/overlays/dev | grep -A5 "name: apiservice"` and count container entries — exactly one should appear per Deployment.
4. Use `kubectl diff -k k8s/overlays/dev` against a running cluster to catch accidental container additions before committing.

**Warning signs:**
- Pod stuck in `Pending` with scheduler event "container X has no image specified"
- `kubectl describe deployment apiservice` shows two containers where one is expected
- Resource limits appear not applied despite overlay patch being committed
- `kustomize build` output has duplicate container name entries under the same Deployment

**Phase to address:** Phase 2 — Kustomize manifests

---

### Pitfall 8: Sealed Secrets Master Key Not Backed Up Before Cluster Teardown

**What goes wrong:**
The kind dev cluster is destroyed and recreated — a routine local development operation. The Sealed Secrets controller generates a new master key on first start. All previously committed `SealedSecret` YAML files in Git are now encrypted with the old key and cannot be decrypted by the new controller. Every secret must be manually re-sealed with `kubeseal`. In CI/CD pipelines, this silently breaks automated deployments until every secret is re-created.

**Why it happens:**
The Sealed Secrets controller stores its private key as a Kubernetes Secret in the `kube-system` namespace. When the kind cluster is deleted, this Secret is gone. Developers assume Git stores everything needed to restore the cluster — but the sealing key is cluster-local and intentionally not committed to Git (it is the private decryption key). There is no warning when the key changes.

**How to avoid:**
1. Immediately after bootstrapping Sealed Secrets on a new cluster, export and back up the sealing key:
```bash
kubectl get secret -n kube-system \
  -l sealedsecrets.bitnami.com/sealed-secrets-key \
  -o yaml > sealed-secrets-master-key.yaml
```
Store this in a password manager or encrypted file store — never in Git.
2. Script cluster recreation to restore the sealing key before the Sealed Secrets controller initializes:
```bash
kind create cluster --config kind-config.yaml
kubectl apply -f sealed-secrets-master-key.yaml  # restore BEFORE controller starts
helm install sealed-secrets sealed-secrets/sealed-secrets -n kube-system
```
3. Prefer bring-your-own-key (BYOK) with `kubeseal --cert` pointing to a custom certificate stored outside the cluster, so the same sealing certificate works across cluster recreations.
4. Automate key backup in the cluster bootstrap script — never rely on a manual step that is skipped under time pressure.

**Warning signs:**
- `error: no key could decrypt secret` in Sealed Secrets controller logs after cluster recreation
- All pods crash with `Secret not found` after cluster teardown and recreate
- ArgoCD shows all applications as `Healthy` (SealedSecrets synced) but pods fail with missing env vars
- `kubectl get sealedsecret` shows resources but `kubectl get secret` shows no corresponding decrypted secrets

**Phase to address:** Phase 5 — Sealed Secrets

---

### Pitfall 9: OTEL Collector Pipeline Misconfigured — Silent Data Loss

**What goes wrong:**
The OTEL Collector is deployed and services send telemetry via OTLP (the project already configures OTLP via `AddServiceDefaults()`). The Collector receives data but one of three failure modes occurs silently: (1) the exporter endpoint is misconfigured and data is dropped at the exporter stage, (2) the `metrics` or `logs` pipeline is omitted so only traces flow, or (3) the `sending_queue` is undersized and the Collector OOMs under burst load — the checkout saga generates telemetry across 5 features simultaneously — causing data loss with no alerting.

**Why it happens:**
OTEL Collector config requires all three sections (`receivers`, `processors`, `exporters`) explicitly wired in `service.pipelines`. Omitting a signal type causes that signal to be silently discarded. The Collector starts successfully with partial pipelines. Under burst load (e.g., seed data loading triggering 50 product events), an unbounded queue grows until the pod is OOM-killed.

**How to avoid:**
1. Always configure all three signal pipelines explicitly:
```yaml
service:
  pipelines:
    traces:
      receivers: [otlp]
      processors: [memory_limiter, batch]
      exporters: [otlp/dashboard]
    metrics:
      receivers: [otlp]
      processors: [memory_limiter, batch]
      exporters: [otlp/dashboard]
    logs:
      receivers: [otlp]
      processors: [memory_limiter, batch]
      exporters: [otlp/dashboard]
```
2. Always include `memory_limiter` as the first processor with a hard cap:
```yaml
processors:
  memory_limiter:
    check_interval: 1s
    limit_mib: 400
    spike_limit_mib: 100
```
3. Validate config before deploying: `otelcol validate --config collector-config.yaml`
4. Include a `debug` exporter in development to verify data flows:
```yaml
exporters:
  debug:
    verbosity: detailed
```
5. The `.NET` services use `OTEL_EXPORTER_OTLP_ENDPOINT` from `AddServiceDefaults()`. In K8s, set this to the Collector's ClusterIP service DNS (e.g., `http://otel-collector-svc:4317`), not `localhost:4317`. Verify via `kubectl exec` that the env var points to the Collector service, not the Aspire Dashboard directly.
6. The Aspire Dashboard can receive OTLP directly on port `18889` (gRPC). Either route through the Collector or send directly — but not both, or data will be duplicated.

**Warning signs:**
- Aspire Dashboard deployed but no traces or metrics visible despite pods running and accepting requests
- OTEL Collector pod logs show `exporting_items` count but Dashboard shows nothing
- Collector pod shows `OOMKilled` status under burst load
- Only traces appear but metrics and logs are absent (partial pipeline)
- `OTEL_EXPORTER_OTLP_ENDPOINT` env var in service pods points to `localhost`

**Phase to address:** Phase 6 — OTEL Collector + Aspire Dashboard

---

### Pitfall 10: Keycloak Realm Import — One-Time Job That Does Not Update on Re-sync

**What goes wrong:**
The project has Keycloak realm JSON files in `src/MicroCommerce.AppHost/Realms/`. In K8s, the Keycloak operator's `KeycloakRealmImport` CR runs as a Kubernetes Job exactly once (on creation) using `IGNORE_EXISTING` strategy. If the realm already exists — which it will after the first deployment — subsequent ArgoCD syncs re-apply the `KeycloakRealmImport` CR but the Job does not re-run. Changes to the realm JSON (new client, updated redirect URI, new role) are silently ignored. The Job shows `Completed` status; ArgoCD treats the application as healthy.

**Why it happens:**
Keycloak operator's `RealmImport` CR is designed for declarative realm creation, not updates. This is documented behavior as of Keycloak v24+ but surprises developers who expect GitOps to mean "Git is truth." The operator provides no mechanism to detect that the source JSON changed and re-run the import with `OVERWRITE_EXISTING`.

**How to avoid:**
1. For the kind dev environment: use a Kubernetes Job or Helm hook that calls the Keycloak Admin REST API to upsert realm configuration on every deploy:
```bash
ACCESS_TOKEN=$(curl -s -X POST http://keycloak/realms/master/protocol/openid-connect/token \
  -d "client_id=admin-cli&grant_type=password&username=$ADMIN_USER&password=$ADMIN_PASS" \
  | jq -r .access_token)

curl -X PUT http://keycloak/admin/realms/micro-commerce \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d @/realm/realm.json
```
2. Alternatively, delete the `KeycloakRealmImport` CR before each deploy to force the Job to re-run:
```bash
kubectl delete keycloakrealmimport micro-commerce-realm -n keycloak
argocd app sync micro-commerce
```
3. Keep the realm JSON export minimal — only clients, roles, and identity providers. Avoid exporting users, sessions, or event logs that change continuously.
4. Store the realm admin credentials in a Sealed Secret, not as a plain ConfigMap value.

**Warning signs:**
- Realm JSON changes committed to Git and ArgoCD sync succeeds, but changes do not appear in Keycloak UI
- `KeycloakRealmImport` Job shows `Completed` status after sync (this is the expected but misleading behavior)
- New OIDC clients added to realm JSON return `invalid_client` from Keycloak
- NextAuth.js cannot authenticate after realm JSON updates because client redirect URIs were changed in Git but not applied

**Phase to address:** Phase 2 — Kustomize manifests (Keycloak bootstrap strategy)

---

## Technical Debt Patterns

| Shortcut | Immediate Benefit | Long-term Cost | When Acceptable |
|----------|-------------------|----------------|-----------------|
| Single `COPY . .` in Dockerfile | Simpler Dockerfile, fewer lines | Full NuGet restore on every `.cs` change; CI 3-4 minutes slower per build | Never — restore/build separation is a one-time setup cost |
| Hardcode service URLs in `appsettings.json` for K8s | Faster initial manifest authoring | Cannot promote same image across environments; breaks immutable image principle | Never — use environment variables in Kustomize overlays |
| `npm start` instead of `node server.js` in Next.js container | Familiar invocation | Kubernetes SIGTERM not forwarded; graceful shutdown broken; requests dropped on rolling update | Never |
| Skip resource `requests`/`limits` on pods | Faster manifest authoring | OOM kills evict adjacent pods; no HPA baseline; kind cluster instability on cold start | Never in K8s manifests |
| Skip `memory_limiter` in OTEL Collector | Simpler Collector config | OOM under burst traffic; data loss for entire observability pipeline with no alerting | Never |
| Sealed Secrets without key backup procedure | One less bootstrap step | Cluster recreation requires re-sealing every secret; CI/CD breaks until manually resolved | Never — automate backup in the bootstrap script |
| ArgoCD auto-sync without health checks | Changes deploy immediately | Broken rollouts keep syncing, replacing healthy pods with broken ones in a loop | Only in `dev` overlay with `selfHeal: false` as a safety valve |
| One init container per DbContext | One-concern-per-container principle | Race conditions on cold start; 8 init container failures restart the pod repeatedly | Never — use single sequential migration runner |
| Use `latest` image tag in Kustomize overlays | Simpler overlay management | ArgoCD detects perpetual drift; no rollback target; no audit trail of what deployed when | Never — always use specific version tags or digest references |

---

## Integration Gotchas

| Integration | Common Mistake | Correct Approach |
|-------------|----------------|------------------|
| Aspire + Kubernetes | Leaving `AddAzureServiceBusClient("messaging")` in `Program.cs` when running with RabbitMQ | Conditionalize transport registration via env var or remove Azure SB client entirely for K8s mode |
| MassTransit + RabbitMQ | Keeping `IServiceBusReceiveEndpointConfigurator` DLQ cast in `AddConfigureEndpointsCallback` | Remove the transport-specific cast block; configure RabbitMQ dead-letter exchanges at queue declaration time |
| YARP Gateway + Kubernetes | YARP resolving ApiService via Aspire env vars (`services__apiservice__*`) that are absent in K8s | Set explicit `services__apiservice__http__0` env var in Gateway K8s Deployment pointing to `http://apiservice-svc:8080` |
| Keycloak + Next.js in K8s | NextAuth.js `KEYCLOAK_ISSUER` set to internal cluster DNS — browser cannot reach it for login redirects | Use internal issuer URL for server-side token validation; use external hostname for browser redirect flows; configure separately |
| PostgreSQL + EF Core + K8s | Migration init container starts before PostgreSQL pod accepts connections | Add `pg_isready` wait loop in init container before any `dotnet ef database update` commands |
| Next.js + Azure Blob Storage | `next.config.ts` `remotePatterns` hardcodes `127.0.0.1:10000` (Azurite); K8s images from different host return 400 | Parameterize blob storage hostname via environment variable; update `remotePatterns` in Kustomize overlay |
| OTEL Collector + Aspire Dashboard | Collector `otlp` exporter targeting `localhost:18889`; Dashboard pod has different ClusterIP | Set Collector exporter endpoint to Dashboard's K8s service DNS (`http://aspire-dashboard-svc:18889`) |
| ArgoCD + Sealed Secrets | ArgoCD syncs `SealedSecret` before `sealed-secrets-controller` is ready; secrets never decrypt | Install Sealed Secrets controller before ArgoCD syncs app manifests; use `sync-wave: "-1"` annotation |
| kind + GHCR | kind cluster does not inherit Docker Desktop login credentials; image pulls from `ghcr.io` fail with `ErrImagePull` | Create an `imagePullSecret` from GHCR PAT and reference it in every Deployment or configure it as the default service account secret |

---

## Performance Traps

| Trap | Symptoms | Prevention | When It Breaks |
|------|----------|------------|----------------|
| No resource requests set on pods | kind node OOMs on cold start; pod eviction cascade during seed data loading | Set `requests.cpu: 100m`, `requests.memory: 256Mi` minimum per service | When all 6 services initialize simultaneously on cluster cold start |
| Next.js image optimization writing to local filesystem only | Optimized images cached per pod replica; each pod re-optimizes same images on first request | Mount a shared PVC for image cache or disable on-disk cache; use CDN for static assets | When more than one Next.js replica runs — invisible in single-replica kind dev |
| OTEL Collector default batch settings under burst load | Telemetry backpressure; tail traces dropped | Configure `batch` processor with `send_batch_size: 512` and `timeout: 5s` | When all 8 features emit events simultaneously during checkout saga execution |
| PostgreSQL without PVC persistence in kind | All data lost on pod restart; seed data re-runs on next startup; migration history inconsistent | Use `PersistentVolumeClaim` for PostgreSQL data; kind supports `hostPath` provisioner | Every time the postgres pod is deleted or kind node restarts |
| No `livenessProbe` on ApiService | Deadlocked pod continues receiving requests; never auto-restarts | Configure liveness probe on `/alive` endpoint (already implemented) with `initialDelaySeconds: 30` | Under heavy saga load when MassTransit consumer threads deadlock |
| Docker build without BuildKit layer cache in CI | Full image rebuild on every CI run even with identical source | Use `docker/build-push-action` with `cache-from: type=gha` in GitHub Actions | From first CI run — invisible locally where Docker daemon caches layers |

---

## Security Mistakes

| Mistake | Risk | Prevention |
|---------|------|------------|
| Committing raw Kubernetes Secrets to Git | Credential exposure in Git history — rotating secrets does not remove history | Use Sealed Secrets exclusively; run `gitleaks` pre-commit hook to block base64-encoded secrets |
| `requireHttpsMetadata: false` leaking into K8s | JWT validation accepts tokens from HTTP issuers; man-in-the-middle possible | The `Program.cs` already gates this on `IsDevelopment()`; verify K8s env name is `Production` not `Development` |
| Keycloak admin credentials in ConfigMap as plain text | Admin password readable by any pod in namespace with ConfigMap access | Store Keycloak admin credentials in Sealed Secrets; reference via `secretKeyRef` in Deployment env |
| CORS origins hardcoded to `*` in YARP Gateway | Any origin can make cross-origin requests including authenticated ones | Set explicit allowed origins in Kustomize overlay — this is a known tech debt item that must be addressed before K8s deployment |
| ArgoCD admin password never rotated after bootstrap | Default ArgoCD admin credential controls entire cluster deployment | Change ArgoCD admin password immediately post-bootstrap; configure OIDC via Keycloak for ArgoCD admin login |
| `imagePullPolicy: Always` with mutable `latest` tag | Different pod replicas may run different image versions; unpredictable rollouts | Use immutable tags (digest or semantic version); set `imagePullPolicy: IfNotPresent` for specific version tags |

---

## "Looks Done But Isn't" Checklist

- [ ] **Next.js Docker image:** Verify `standalone/server.js` exists inside the built container. Run `docker run --rm <image> ls /app/server.js`. Image size should be under 300MB, not 800MB+.
- [ ] **Next.js static assets:** Hit the running container's `/` route and verify `/_next/static/` assets load. Static files are not included in standalone output by default and require an explicit `COPY` step.
- [ ] **MassTransit transport swap:** Verify `AddAzureServiceBusClient` is removed from `Program.cs` for K8s mode. Run a full checkout flow end-to-end; confirm saga completes and stock is deducted.
- [ ] **Keycloak realm import applied:** After ArgoCD sync, verify the `micro-commerce` realm clients exist via Keycloak Admin API: `GET /admin/realms/micro-commerce/clients`. Do not trust Job `Completed` status alone.
- [ ] **Sealed Secrets key backed up:** Run `kubectl get secret -n kube-system -l sealedsecrets.bitnami.com/sealed-secrets-key` immediately after cluster creation. Verify the backup file exists outside the cluster before proceeding with any secret sealing.
- [ ] **OTEL pipeline completeness:** Run a checkout flow (triggers traces + metrics + logs across Catalog, Cart, Ordering, Inventory, Profiles). Verify all three signal types appear in the Aspire Dashboard.
- [ ] **No localhost in K8s manifests:** `grep -r "localhost" k8s/` must return zero results in any env var value or service URL.
- [ ] **PostgreSQL data survives pod restart:** `kubectl delete pod postgres-0`; wait for restart; confirm catalog products still return from `/api/catalog/products`.
- [ ] **ArgoCD sync stability:** Wait 5 minutes after initial sync; application must remain `Synced` without manual intervention. A sync loop becomes apparent within 2-3 reconciliation cycles (default 3 minutes).
- [ ] **kind image pull from GHCR:** Verify `imagePullSecrets` is configured on each Deployment. Run `kubectl describe pod <name>` and check for `ErrImagePull` events before declaring deployment successful.
- [ ] **Resource limits present:** Run `kubectl describe nodes` and verify no pods show `BestEffort` QoS class — all pods should have `Burstable` or `Guaranteed` QoS from defined requests.

---

## Recovery Strategies

| Pitfall | Recovery Cost | Recovery Steps |
|---------|---------------|----------------|
| Sealed Secrets key lost (cluster destroyed) | HIGH | Export all secret values from running pods before teardown; re-seal with new key; commit updated SealedSecrets; ArgoCD auto-syncs |
| MassTransit transport misconfiguration (startup crash) | MEDIUM | Roll back Deployment to previous image: `kubectl rollout undo deployment/apiservice`; fix transport config; rebuild and push new image |
| Saga state stuck (transport swap mid-flight) | HIGH | Query `OrderingDbContext.CheckoutStates` for stuck sagas; manually compensate (release stock via API); clear saga rows; redeploy |
| ArgoCD sync loop (perpetual OutOfSync) | LOW | Identify non-idempotent field via `argocd app diff`; add `ignoreDifferences` rule or remove dynamic annotation from Kustomize output |
| Keycloak realm config lost (operator one-time import) | MEDIUM | Call Keycloak Admin REST API with realm JSON to upsert; or delete `KeycloakRealmImport` CR and trigger ArgoCD sync |
| OTEL Collector OOM crash | LOW | Collector is stateless; redeploy pod; data during crash window is unrecoverable but service continues; add `memory_limiter` processor immediately |
| Migration init container deadlock | MEDIUM | `kubectl delete pod <app-pod>` to trigger restart; add `pg_isready` wait loop to init container script; EF migrations are idempotent — safe to re-run |
| Next.js SIGTERM not forwarded | LOW | Update Dockerfile `CMD` from `["npm", "start"]` to `["node", "server.js"]`; rebuild image; trigger rolling restart |
| kind cluster cold-start PostgreSQL race | LOW | Delete crashed pod; pod restarts and init container retries; add `pg_isready` loop to prevent recurrence |

---

## Pitfall-to-Phase Mapping

| Pitfall | Prevention Phase | Verification |
|---------|------------------|--------------|
| Dockerfile layer caching (restore/build separation) | Phase 1 — Dockerfiles | Build image twice; second build must skip `dotnet restore` layer (cache hit in build logs) |
| Next.js `output: 'standalone'` missing | Phase 1 — Dockerfiles | `docker run` image; confirm `/app/server.js` exists; image size under 300MB |
| Next.js `npm start` vs `node server.js` | Phase 1 — Dockerfiles | Send `kill -SIGTERM <pid>` to container process; verify graceful shutdown within 30 seconds |
| Aspire env vars vs K8s DNS conflict | Phase 2 — Kustomize manifests | `kubectl exec` into pod; `printenv \| grep services__` shows K8s DNS values not localhost |
| EF Core migration race condition | Phase 2 — Kustomize manifests | Delete all pods; restart cluster cold; verify all 8 schemas present after pod stabilizes |
| Kustomize patch targeting wrong container name | Phase 2 — Kustomize manifests | `kustomize build` output; count container entries per Deployment — exactly one per service |
| Keycloak realm import one-time-only | Phase 2 — Kustomize manifests | Modify realm JSON; ArgoCD sync; verify change appears in Keycloak Admin UI |
| MassTransit transport swap (DLQ, saga, outbox) | Phase 3 — Transport configuration | Checkout flow end-to-end; intentionally fail payment; verify stock reservation released |
| ArgoCD sync loop | Phase 4 — ArgoCD GitOps | Wait 5 minutes post-sync; application remains `Synced` without manual intervention |
| ArgoCD app-of-apps bootstrap ordering | Phase 4 — ArgoCD GitOps | Tear down and re-bootstrap cluster; verify all child apps sync in correct dependency order |
| Sealed Secrets key not backed up | Phase 5 — Sealed Secrets | Delete and recreate kind cluster using backed-up key; verify all SealedSecrets decrypt correctly |
| OTEL Collector silent data loss | Phase 6 — OTEL Collector | Run `otelcol validate`; trigger checkout flow; confirm all three signals appear in Dashboard |
| OTEL Collector OOM under burst load | Phase 6 — OTEL Collector | `memory_limiter` configured; 10 concurrent checkout flows must not OOM-kill the Collector pod |

---

## Sources

- [Andrew Lock — Caching Docker layers with multi-stage builds](https://andrewlock.net/caching-docker-layers-on-serverless-build-hosts-with-multi-stage-builds---target,-and---cache-from/)
- [dotnet/dotnet-docker GitHub Discussion #6123 — NuGet caching in Docker](https://github.com/dotnet/dotnet-docker/discussions/6123)
- [Next.js Docs — output: standalone](https://nextjs.org/docs/pages/api-reference/config/next-config-js/output)
- [Next.js Docs — Deploying](https://nextjs.org/docs/app/getting-started/deploying)
- [Next.js GitHub Discussion #75930 — standalone performance](https://github.com/vercel/next.js/discussions/75930)
- [Self-hosting Next.js at Scale in 2025 — Sherpa](https://www.sherpa.sh/blog/secrets-of-self-hosting-nextjs-at-scale-in-2025)
- [dotnet/aspire GitHub Issue #3698 — Ignore URL for service discovery in K8s](https://github.com/dotnet/aspire/issues/3698)
- [dotnet/aspire GitHub Issue #5096 — Service discovery endpoint env vars priority](https://github.com/dotnet/aspire/issues/5096)
- [Milan Jovanovic — Using MassTransit with RabbitMQ and Azure Service Bus](https://www.milanjovanovic.tech/blog/using-masstransit-with-rabbitmq-and-azure-service-bus)
- [MassTransit — Saga Persistence](https://masstransit.io/documentation/patterns/saga/persistence)
- [MassTransit GitHub Discussion #4953 — EF Core Inbox/Outbox with Saga](https://github.com/MassTransit/MassTransit/discussions/4953)
- [Scaling a State Machine Saga with Kubernetes](https://medium.com/@czinege.roland/scaling-a-state-machine-saga-with-kubernetes-43fb8e02689a)
- [Atlas Guides — Schema migrations in K8s with init containers](https://atlasgo.io/guides/deploying/k8s-init-container)
- [ArgoCD FAQ — Sync options](https://argo-cd.readthedocs.io/en/stable/faq/)
- [ArgoCD — Cluster Bootstrapping](https://argo-cd.readthedocs.io/en/stable/operator-manual/cluster-bootstrapping/)
- [ArgoCD GitHub Discussion #11892 — ApplicationSets vs App-of-apps vs Kustomize](https://github.com/argoproj/argo-cd/discussions/11892)
- [Argo Blog — CRDs and Kustomize patching lists](https://blog.argoproj.io/argo-crds-and-kustomize-the-problem-of-patching-lists-5cfc43da288c)
- [Kustomize GitHub Issue #5997 — Strategic merge failures unclear](https://github.com/kubernetes-sigs/kustomize/issues/5997)
- [Kustomize GitHub Issue #5874 — Unexpected strategic merge patch behavior](https://github.com/kubernetes-sigs/kustomize/issues/5874)
- [Sealed Secrets GitHub — bitnami-labs/sealed-secrets](https://github.com/bitnami-labs/sealed-secrets)
- [Medium — Take backup of all sealed-secrets keys](https://ismailyenigul.medium.com/take-backup-of-all-sealed-secrets-keys-or-re-encrypt-regularly-297367b3443)
- [OpenTelemetry — Collector configuration](https://opentelemetry.io/docs/collector/configuration/)
- [OpenTelemetry — Collector resiliency](https://opentelemetry.io/docs/collector/resiliency/)
- [Keycloak — Automating realm import](https://www.keycloak.org/operator/realm-import)
- [Medium — Keycloak realm import in Kubernetes](https://rahulroyz.medium.com/update-keycloak-realm-configurations-using-import-feature-on-kubernetes-platform-b1b0ed85f7f7)
- [Kubernetes Blog — 7 Common Kubernetes Pitfalls (2025)](https://kubernetes.io/blog/2025/10/20/seven-kubernetes-pitfalls-and-how-to-avoid/)
- [Milan Jovanovic — Standalone Aspire Dashboard Setup](https://www.milanjovanovic.tech/blog/standalone-aspire-dashboard-setup-for-distributed-dotnet-applications)
- [Microsoft Learn — Standalone Aspire dashboard](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard/standalone)
- [Microsoft Learn — Aspire Kubernetes integration](https://learn.microsoft.com/en-us/dotnet/aspire/deployment/kubernetes-integration)

---
*Pitfalls research for: Kubernetes & GitOps deployment — MicroCommerce v3.0*
*Researched: 2026-02-25*

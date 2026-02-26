# Phase 25: Application Manifests and MassTransit Transport - Research

**Researched:** 2026-02-26
**Domain:** Kubernetes application manifests (Kustomize) + MassTransit multi-transport configuration
**Confidence:** HIGH

## Summary

Phase 25 deploys the three application services (ApiService, Gateway, Web) into the existing kind cluster from Phase 24, and adds RabbitMQ as an alternative MassTransit transport for K8s environments. The infrastructure (PostgreSQL, RabbitMQ, Keycloak, SealedSecrets) is already running from Phase 24. The work divides cleanly into two tracks: (1) K8s manifests with Kustomize base/overlay structure, and (2) conditional transport switching in the .NET ApiService.

Several critical issues were identified during research that must be addressed for the application to function outside the Aspire development environment:

1. **Health check endpoints are gated behind `IsDevelopment()`** in `ServiceDefaults/Extensions.cs`. K8s probes will fail on `/health` and `/alive` because the K8s environment is not "Development". This must be changed to always expose health endpoints.
2. **Gateway YARP cluster uses Aspire service discovery address** (`https+http://apiservice`). In K8s, this needs to be overridden to the K8s DNS name via environment variable or config override.
3. **Aspire-only registrations** (`AddAzureServiceBusClient`, `AddAzureBlobServiceClient`, `AddKeycloakJwtBearer` with Aspire service discovery) will fail without connection strings. The code must be conditionally configured.
4. **`DeadLetterQueueService` uses Azure Service Bus SDK directly** (not MassTransit). It needs a no-op implementation or conditional registration for RabbitMQ environments.
5. **.NET services use `dotnet publish /t:PublishContainer`** (SDK container builds) not Dockerfiles. For kind, images must be built locally and loaded via `kind load docker-image`.

**Primary recommendation:** Implement transport switching first (code changes to ApiService/Gateway/ServiceDefaults), then create K8s manifests, then extend bootstrap.sh to build+load images and deploy applications.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- Environment variable `MASSTRANSIT_TRANSPORT` controls which transport is active: `RabbitMQ` or `AzureServiceBus`
- Default when not set: Azure Service Bus (preserves existing Aspire dev workflow)
- RabbitMQ transport uses the same transactional outbox pattern as Azure Service Bus -- consistent reliability across environments, checkout saga behavior is identical
- RabbitMQ host is injected via environment variable (e.g., `RabbitMQ__Host`), not hardcoded -- K8s manifest sets this to the K8s DNS name (`rabbitmq.micro-commerce.svc.cluster.local`)
- RabbitMQ credentials injected via environment variables referencing the existing `rabbitmq-credentials` SealedSecret
- All configuration via environment variables in Deployment manifests (12-factor app style)
- Kustomize overlays can patch env vars per environment
- Database connection strings composed from SealedSecret refs: host from K8s DNS (`postgres.micro-commerce.svc.cluster.local`), username/password from `postgres-credentials` secretKeyRef, database name `appdb`
- Keycloak URLs injected as env vars pointing to K8s internal DNS (`http://keycloak.micro-commerce.svc.cluster.local:8080`)
- Gateway discovers ApiService and Web via env vars: `SERVICE_URL_APISERVICE=http://apiservice:8080`, `SERVICE_URL_WEB=http://web:3000` -- Gateway reads these to configure YARP routes
- Web (Next.js) uses standard `next start` with standalone output mode -- health checks via HTTP probe on `/`, no custom server needed

### Claude's Discretion
- Dockerfile multi-stage build structure for each service
- Container image tagging strategy for kind load
- Exact resource limits (CPU/memory) per app service
- Startup probe timing for ApiService EF Core migration
- How to integrate image build + kind load into bootstrap.sh or a separate script
- Init container vs startup probe for database migration readiness

### Deferred Ideas (OUT OF SCOPE)
None -- discussion stayed within phase scope
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| MFST-01 | ApiService, Gateway, and Web have Deployment + Service + ConfigMap manifests | Existing infra manifests (Phase 24) provide exact pattern; K8s DNS names for service discovery documented |
| MFST-02 | All app services have liveness (`/alive`) and readiness (`/health`) probes configured | **CRITICAL**: `MapDefaultEndpoints` is gated behind `IsDevelopment()` -- must fix ServiceDefaults/Extensions.cs to expose health endpoints in all environments |
| MFST-03 | All containers have CPU and memory resource requests and limits | Research provides recommended values based on service characteristics |
| MFST-04 | Kustomize base directory contains all environment-neutral manifests | Existing `infra/k8s/base/` structure provides pattern; app services added as subdirectories |
| MFST-05 | Kustomize dev overlay patches image tags and resource limits for kind | Existing `infra/k8s/overlays/dev/kustomization.yaml` needs `images:` section for local kind tags |
| MFST-06 | All resources are namespaced under `micro-commerce` | Already enforced by base `kustomization.yaml` `namespace: micro-commerce` setting |
| TRAN-01 | MassTransit supports RabbitMQ transport in K8s deployments | MassTransit.RabbitMQ 9.0.x NuGet package; `UsingRabbitMq` with EF Core outbox confirmed compatible |
| TRAN-02 | Transport selection is configurable via `MASSTRANSIT_TRANSPORT` env var | Conditional `if/else` on config value in Program.cs; both transports share same consumer/saga/outbox registrations |
</phase_requirements>

## Standard Stack

### Core
| Library/Tool | Version | Purpose | Why Standard |
|-------------|---------|---------|--------------|
| MassTransit.RabbitMQ | 9.0.x | RabbitMQ transport for MassTransit | Official MassTransit transport package; same outbox/saga compatibility as Azure SB |
| Kustomize | Built into kubectl | K8s manifest management (base/overlay) | Already used in Phase 24; no extra tooling needed |
| kind | Existing | Local K8s cluster | Already provisioned from Phase 24 |
| dotnet publish /t:PublishContainer | .NET SDK built-in | Container image builds for .NET services | Already used in CI pipeline (container-images.yml); no Dockerfile needed for .NET |

### Supporting
| Library/Tool | Version | Purpose | When to Use |
|-------------|---------|---------|-------------|
| kubeseal | v0.27.3 | Seal new app secrets (if needed) | Already installed from Phase 24 bootstrap |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| SDK container builds for .NET | Multi-stage Dockerfiles | SDK builds are already configured in CI; using them for kind keeps consistency. Dockerfiles give more control but add maintenance burden for two build methods |
| Environment variable config | ConfigMaps for JSON config files | Env vars are simpler for 12-factor, work with secretKeyRef, and match the user decision |

**Installation (new NuGet package):**
```bash
dotnet add src/MicroCommerce.ApiService package MassTransit.RabbitMQ --version 9.0.0
```

## Architecture Patterns

### Recommended Project Structure
```
infra/k8s/
  base/
    namespace.yaml          # (existing)
    kustomization.yaml      # (existing, add new app subdirectories)
    postgres/               # (existing)
    rabbitmq/               # (existing)
    keycloak/               # (existing)
    apiservice/             # NEW
      deployment.yaml
      service.yaml
      kustomization.yaml
    gateway/                # NEW
      deployment.yaml
      service.yaml
      kustomization.yaml
    web/                    # NEW
      deployment.yaml
      service.yaml
      kustomization.yaml
  overlays/
    dev/
      kustomization.yaml    # (existing, extend with images: and patches:)
```

### Pattern 1: Conditional MassTransit Transport Switching
**What:** Read `MASSTRANSIT_TRANSPORT` env var at startup to choose between `UsingAzureServiceBus` and `UsingRabbitMq`. All consumer, saga, outbox, and endpoint configuration is shared -- only the terminal `UsingXxx` call differs.
**When to use:** When the same application must run on different message transports across environments.
**Example:**
```csharp
// In Program.cs -- consumer/saga/outbox registration stays the same
builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(typeof(Program).Assembly);

    x.AddSagaStateMachine<CheckoutStateMachine, CheckoutState>()
        .EntityFrameworkRepository(r => { /* same config */ });

    x.AddEntityFrameworkOutbox<CatalogDbContext>(o => { /* same config */ });

    x.AddConfigureEndpointsCallback((context, name, cfg) =>
    {
        // DLQ routing only applies to Azure Service Bus
        if (cfg is IServiceBusReceiveEndpointConfigurator sb)
        {
            sb.ConfigureDeadLetterQueueErrorTransport();
        }

        cfg.UseCircuitBreaker(/* same config */);
        cfg.UseMessageRetry(/* same config */);
        cfg.UseEntityFrameworkOutbox<CatalogDbContext>(context);
    });

    // Transport selection
    string transport = builder.Configuration["MASSTRANSIT_TRANSPORT"] ?? "AzureServiceBus";
    if (transport.Equals("RabbitMQ", StringComparison.OrdinalIgnoreCase))
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
            {
                h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
                h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
            });
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
});
```
Source: MassTransit docs - RabbitMQ transport configuration (Context7 /masstransit/masstransit)

### Pattern 2: YARP Route Override via Environment Variables
**What:** The Gateway reads YARP cluster destinations from `appsettings.json`, but .NET configuration system allows environment variable overrides using `__` (double-underscore) separator for nested keys.
**When to use:** Overriding YARP route addresses in K8s without changing code.
**Example:**
```yaml
# K8s Deployment env var to override YARP cluster destination
- name: ReverseProxy__Clusters__apiservice__Destinations__default__Address
  value: "http://apiservice:8080"
```
This leverages the standard .NET configuration provider hierarchy where environment variables override appsettings.json. The existing `appsettings.json` sets `"Address": "https+http://apiservice"` (Aspire service discovery), and the K8s env var overrides it to a plain HTTP K8s DNS address.

### Pattern 3: Health Endpoint Availability Fix
**What:** The current `MapDefaultEndpoints` only registers `/health` and `/alive` in Development environment. K8s probes require these endpoints to be available in all environments.
**When to use:** Making the application K8s-ready.
**Example:**
```csharp
// ServiceDefaults/Extensions.cs -- change from:
public static WebApplication MapDefaultEndpoints(this WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/alive", new HealthCheckOptions { ... });
    }
    return app;
}
// To: always map health endpoints (remove IsDevelopment guard)
public static WebApplication MapDefaultEndpoints(this WebApplication app)
{
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/alive", new HealthCheckOptions
    {
        Predicate = r => r.Tags.Contains("live")
    });
    return app;
}
```

### Pattern 4: Conditional Aspire Integration Registration
**What:** `AddAzureServiceBusClient("messaging")` and `AddAzureBlobServiceClient("blobs")` will throw if no connection string exists. In K8s with RabbitMQ, there is no Azure SB. These must be conditional.
**When to use:** Running the same application binary in Aspire (Azure) and K8s (non-Azure) environments.
**Example:**
```csharp
string transport = builder.Configuration["MASSTRANSIT_TRANSPORT"] ?? "AzureServiceBus";
if (!transport.Equals("RabbitMQ", StringComparison.OrdinalIgnoreCase))
{
    builder.AddAzureServiceBusClient("messaging");
    builder.AddAzureBlobServiceClient("blobs");
}
else
{
    // Register no-op or stub services for features that depend on Azure SDK
    // BlobServiceClient is needed for image upload -- use placeholder image URL in K8s
    // ServiceBusClient is needed for DLQ management -- register no-op IDeadLetterQueueService
}
```

### Pattern 5: Keycloak JWT Configuration for K8s
**What:** `AddKeycloakJwtBearer(serviceName: "keycloak", realm: "micro-commerce")` uses Aspire service discovery to find Keycloak. In K8s, the Keycloak URL must come from configuration.
**When to use:** Running services outside Aspire orchestration.
**Key insight:** The `AddKeycloakJwtBearer` method ultimately sets the JWT `Authority` URL. The Aspire integration resolves `keycloak` via service discovery to construct `http://keycloak:port/realms/micro-commerce`. In K8s, we can override this through the standard .NET `ConnectionStrings:keycloak` configuration key, which the Aspire integration reads. Setting `ConnectionStrings__keycloak=http://keycloak.micro-commerce.svc.cluster.local:8080` as an env var should satisfy the service discovery lookup.

Additionally, `RequireHttpsMetadata = false` is currently gated behind `IsDevelopment()` in both ApiService and Gateway. In K8s (non-Development env), Keycloak runs on HTTP, so HTTPS metadata validation will fail. This must be disabled for the K8s environment as well (or always, since it's a dev-only cluster).

### Anti-Patterns to Avoid
- **Hardcoding URLs in manifests:** Always use K8s DNS names (`service.namespace.svc.cluster.local`) or short names (`service`) when pods are in the same namespace.
- **Putting secrets in ConfigMaps:** Use secretKeyRef for credentials, not ConfigMap data.
- **Omitting resource limits:** Without limits, a single pod can starve others in the kind cluster.
- **Using `latest` tag for kind images:** Use explicit tags (e.g., `dev`) so Kustomize `images:` overrides work predictably.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| MassTransit RabbitMQ transport | Custom RabbitMQ client | `MassTransit.RabbitMQ` NuGet | Handles connection management, serialization, error queues, consumer pipeline |
| Configuration override in K8s | Custom config loading | .NET configuration provider hierarchy (env vars override appsettings.json via `__` separator) | Built into .NET; works identically for YARP routes, connection strings, and all other config |
| Image loading into kind | Manual docker save/load | `kind load docker-image` | Handles the image transfer into the kind node's containerd runtime |
| Kustomize image tag patching | sed/envsubst on YAML | Kustomize `images:` field in overlay kustomization.yaml | Declarative, works with `kubectl apply -k` |

**Key insight:** The .NET configuration system is the most powerful tool here. Almost all the "wiring" differences between Aspire and K8s reduce to setting environment variables that override specific config paths.

## Common Pitfalls

### Pitfall 1: Health Check Endpoints Not Registered in Non-Development
**What goes wrong:** K8s liveness/readiness probes return 404, pods get killed in restart loops.
**Why it happens:** `MapDefaultEndpoints()` in `ServiceDefaults/Extensions.cs` wraps health endpoint registration in `if (app.Environment.IsDevelopment())`.
**How to avoid:** Remove the `IsDevelopment()` guard. Always register `/health` and `/alive` endpoints.
**Warning signs:** Pods stuck in CrashLoopBackOff with readiness probe failure messages in `kubectl describe pod`.

### Pitfall 2: Aspire Service Discovery Addresses in K8s
**What goes wrong:** Gateway can't reach ApiService; JWT validation fails because Keycloak URL can't be resolved.
**Why it happens:** `https+http://apiservice` is Aspire service discovery syntax, not a real DNS address. Similarly, `keycloak` service name works via Aspire but needs a real connection string in K8s.
**How to avoid:** Override via environment variables in K8s manifests: `ConnectionStrings__keycloak`, `ReverseProxy__Clusters__apiservice__Destinations__default__Address`.
**Warning signs:** "No such host is known" errors in pod logs, "unable to obtain configuration" JWT errors.

### Pitfall 3: Azure SDK Registration Fails Without Connection Strings
**What goes wrong:** `AddAzureServiceBusClient("messaging")` throws at startup because `ConnectionStrings:messaging` is missing.
**Why it happens:** Aspire client integrations require connection strings to be present.
**How to avoid:** Make Azure SDK registrations conditional on transport mode. When `MASSTRANSIT_TRANSPORT=RabbitMQ`, skip `AddAzureServiceBusClient` and `AddAzureBlobServiceClient`. Register stubs for dependent services (DLQ, image upload).
**Warning signs:** Pod crashes immediately at startup with `InvalidOperationException: ConnectionString is missing`.

### Pitfall 4: EF Core Migration Slow First Boot Kills Pod
**What goes wrong:** ApiService runs 7 schema migrations on first boot. This can take 30-60+ seconds. Default liveness probe kills the pod before migrations complete.
**Why it happens:** Liveness probe fires before application is ready; no startup probe configured.
**How to avoid:** Configure a startup probe with generous timeout (e.g., `initialDelaySeconds: 10`, `periodSeconds: 5`, `failureThreshold: 30` = 160s window). Liveness probe only starts after startup probe succeeds.
**Warning signs:** Pod restarts once or twice then stabilizes (intermittent on fresh deployments).

### Pitfall 5: kind Image Loading Requires Explicit Architecture
**What goes wrong:** `kind load docker-image` fails or loads wrong architecture.
**Why it happens:** Multi-arch images in CI use `linux/amd64,linux/arm64`; local build must match host architecture.
**How to avoid:** Use `dotnet publish /t:PublishContainer` with explicit `--os linux --arch <host-arch>` flag, or use Docker Buildx with `--load` (single-platform). Tag images with `dev` or `local` tag for clarity.
**Warning signs:** `exec format error` in pod logs; image pull failures.

### Pitfall 6: CORS Origins in Gateway
**What goes wrong:** Browser requests from the frontend are blocked by CORS in K8s.
**Why it happens:** Gateway CORS policy hardcodes `http://localhost:3000` and `http://localhost:3001`. In K8s accessed via NodePort, the origin may differ.
**How to avoid:** Make CORS allowed origins configurable via environment variables. For kind local access, `http://localhost:<nodeport>` should be in the allowed list.
**Warning signs:** CORS preflight failures in browser console; API calls fail from frontend but work from curl.

### Pitfall 7: Next.js Auth Requires NEXTAUTH_URL
**What goes wrong:** NextAuth.js v5 redirect URIs point to wrong host in K8s.
**Why it happens:** NextAuth uses `NEXTAUTH_URL` (or `AUTH_URL`) to construct callback URLs. Without it, it infers from the request, which may not match the Keycloak client's valid redirect URIs.
**How to avoid:** Set `AUTH_URL` or `NEXTAUTH_URL` env var in the Web Deployment to the externally accessible URL. Also set `AUTH_TRUST_HOST=true` since there's a reverse proxy (Gateway) in front.
**Warning signs:** OAuth redirect loops, "invalid redirect_uri" errors from Keycloak.

## Code Examples

### MassTransit RabbitMQ Configuration
```csharp
// Source: Context7 /masstransit/masstransit - RabbitMQ transport docs
x.UsingRabbitMq((context, cfg) =>
{
    cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
    {
        h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
        h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
    });
    cfg.ConfigureEndpoints(context);
});
```

### K8s Deployment with Health Probes (ApiService pattern)
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: apiservice
spec:
  replicas: 1
  selector:
    matchLabels:
      app: apiservice
  template:
    metadata:
      labels:
        app: apiservice
    spec:
      containers:
      - name: apiservice
        image: apiservice:dev
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: Production
        - name: ConnectionStrings__appdb
          value: "Host=postgres.micro-commerce.svc.cluster.local;Database=appdb;Username=$(POSTGRES_USERNAME);Password=$(POSTGRES_PASSWORD)"
        - name: MASSTRANSIT_TRANSPORT
          value: RabbitMQ
        - name: RabbitMQ__Host
          value: rabbitmq.micro-commerce.svc.cluster.local
        startupProbe:
          httpGet:
            path: /alive
            port: 8080
          initialDelaySeconds: 10
          periodSeconds: 5
          failureThreshold: 30
        livenessProbe:
          httpGet:
            path: /alive
            port: 8080
          periodSeconds: 15
        readinessProbe:
          httpGet:
            path: /health
            port: 8080
          periodSeconds: 10
        resources:
          requests:
            memory: 256Mi
            cpu: 250m
          limits:
            memory: 512Mi
            cpu: 500m
```

### Kustomize Overlay with Image Override
```yaml
# overlays/dev/kustomization.yaml
apiVersion: kustomize.config.k8s.io/v1beta1
kind: Kustomization

resources:
- ../../base

images:
- name: apiservice
  newName: apiservice
  newTag: dev
- name: gateway
  newName: gateway
  newTag: dev
- name: web
  newName: web
  newTag: dev
```

### Connection String Composition with secretKeyRef
```yaml
# PostgreSQL connection string using env var substitution
env:
- name: POSTGRES_USERNAME
  valueFrom:
    secretKeyRef:
      name: postgres-credentials
      key: username
- name: POSTGRES_PASSWORD
  valueFrom:
    secretKeyRef:
      name: postgres-credentials
      key: password
- name: ConnectionStrings__appdb
  value: "Host=postgres.micro-commerce.svc.cluster.local;Database=appdb;Username=$(POSTGRES_USERNAME);Password=$(POSTGRES_PASSWORD)"
```
Note: K8s `$(VAR_NAME)` syntax performs env var substitution within the same container's env block. The dependent vars must be defined before they are referenced.

### No-op DLQ Service for RabbitMQ
```csharp
// When running with RabbitMQ transport, Azure ServiceBusClient is not available.
// Register a no-op DLQ service that returns empty results.
public sealed class NoOpDeadLetterQueueService : IDeadLetterQueueService
{
    public Task<IReadOnlyList<DeadLetterMessageDto>> PeekDeadLettersAsync(
        string queueName, int maxMessages = 20, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<DeadLetterMessageDto>>([]);

    public Task RetryDeadLetterAsync(
        string queueName, long sequenceNumber, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<int> PurgeDeadLettersAsync(
        string queueName, CancellationToken ct = default)
        => Task.FromResult(0);

    public Task<IReadOnlyList<string>> GetQueueNamesAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<string>>([]);
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Dockerfiles for .NET | `dotnet publish /t:PublishContainer` (SDK container builds) | .NET 8+ | No Dockerfile needed for .NET; configured via MSBuild properties in csproj |
| `ContainerBaseImage` in csproj | `mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra` | Already set in Directory.Build.props | Smaller, more secure runtime images |
| Aspire-only deployment | Aspire + K8s parallel paths | This phase | Environment variable switches between Aspire and K8s configurations |

**Deprecated/outdated:**
- KEYCLOAK_ADMIN env var: replaced by KC_BOOTSTRAP_ADMIN_USERNAME (already handled in Phase 24)
- Ingress NGINX: EOL March 2026; using NodePort for kind access (already decided)

## Open Questions

1. **ASPNETCORE_ENVIRONMENT in K8s**
   - What we know: Health endpoints require removal of `IsDevelopment()` guard. JWT HTTPS requirement also needs to work without Development environment.
   - What's unclear: Should K8s use `Production`, `Staging`, or a custom environment name? `Production` is most standard but the Aspire comment warns about security implications of health endpoints in non-dev.
   - Recommendation: Use `Production` environment. The health endpoints are fine to expose -- they are on internal K8s ClusterIP services, not publicly reachable. Remove the `IsDevelopment()` guard entirely from `MapDefaultEndpoints`. For RequireHttpsMetadata, always set to false since this is a dev-only kind cluster with HTTP Keycloak.

2. **Image build strategy for kind**
   - What we know: .NET services use `dotnet publish /t:PublishContainer` in CI. Web uses Dockerfile. For kind, images need to be loaded via `kind load docker-image`.
   - What's unclear: Should we use `dotnet publish /t:PublishContainer` locally (pushes to local Docker daemon) then `kind load`, or use `docker build` for all three?
   - Recommendation: Use `dotnet publish /t:PublishContainer` for ApiService and Gateway (consistent with CI, already configured), and `docker build` for Web (has a Dockerfile). Tag all with `dev`. Load via `kind load docker-image`. Add this to bootstrap.sh or a separate `deploy-apps.sh` script.

3. **Gateway proxying frontend in K8s**
   - What we know: In Aspire, the frontend is accessed directly (port 3000). The Gateway only proxies API routes to ApiService. The CONTEXT.md mentions `SERVICE_URL_WEB=http://web:3000`.
   - What's unclear: Is the Gateway supposed to proxy the frontend in K8s, or is the frontend accessed separately? The user decision says Gateway discovers Web via env var.
   - Recommendation: Add a YARP catch-all route in the Gateway that forwards non-API traffic to the Web service. This way, the Gateway NodePort is the single entry point. Alternatively, use a separate NodePort for the Web service. The user decision to inject `SERVICE_URL_WEB` suggests the Gateway should proxy the frontend.

4. **kind port mapping for Gateway**
   - What we know: Current kind-config.yaml has 3 port mappings (30432, 30672, 30080). Gateway needs a NodePort to be accessible from the host.
   - Recommendation: Add a NodePort mapping for Gateway (e.g., 30443 -> host 38443 or 30800 -> host 38800). Update kind-config.yaml. This requires recreating the kind cluster if it already exists (port mappings are set at cluster creation time).

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit + Testcontainers (integration), xUnit (unit), Playwright (E2E) |
| Config file | `src/MicroCommerce.ApiService.Tests/MicroCommerce.ApiService.Tests.csproj` |
| Quick run command | `dotnet test src/MicroCommerce.ApiService.Tests --filter "Category!=Integration"` |
| Full suite command | `dotnet test src/MicroCommerce.ApiService.Tests` |

### Phase Requirements -> Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| MFST-01 | App deployments exist in K8s | manual/smoke | `kubectl get deployments -n micro-commerce` | N/A - K8s smoke test |
| MFST-02 | Health probes respond | manual/smoke | `kubectl exec -n micro-commerce deploy/apiservice -- curl -s http://localhost:8080/health` | N/A - K8s smoke test |
| MFST-03 | Resource limits are set | manual/smoke | `kubectl describe deployment apiservice -n micro-commerce \| grep -A5 Limits` | N/A - manifest inspection |
| MFST-04 | Kustomize base directory valid | smoke | `kubectl kustomize infra/k8s/base` | N/A - Kustomize build validation |
| MFST-05 | Kustomize dev overlay valid | smoke | `kubectl kustomize infra/k8s/overlays/dev` | N/A - Kustomize build validation |
| MFST-06 | All resources namespaced | manual/smoke | `kubectl get all -n micro-commerce` | N/A - K8s smoke test |
| TRAN-01 | RabbitMQ transport works | integration | `dotnet test src/MicroCommerce.ApiService.Tests` (existing MassTransit harness tests) | Existing tests cover consumer behavior |
| TRAN-02 | Transport switching works | unit | New test or manual validation | Wave 0 gap |

### Sampling Rate
- **Per task commit:** `kubectl kustomize infra/k8s/overlays/dev` (validates YAML)
- **Per wave merge:** `dotnet test src/MicroCommerce.ApiService.Tests` (ensure no regressions from code changes)
- **Phase gate:** Full E2E: `kubectl get pods -n micro-commerce` all Running/Ready + browser test

### Wave 0 Gaps
- [ ] Ensure `dotnet test` still passes after MassTransit transport refactoring (existing test harness uses in-memory transport, should not be affected)
- [ ] Manual E2E validation script or checklist for K8s deployment smoke test

## Sources

### Primary (HIGH confidence)
- Context7 /masstransit/masstransit - RabbitMQ transport configuration, EF Core outbox with RabbitMQ
- Context7 /kubernetes-sigs/kustomize - overlay patches, image overrides, configMapGenerator
- Codebase analysis: Program.cs, ServiceDefaults/Extensions.cs, Gateway appsettings.json, AppHost.cs, bootstrap.sh, container-images.yml

### Secondary (MEDIUM confidence)
- [Aspire Keycloak integration docs](https://aspire.dev/integrations/security/keycloak/) - AddKeycloakJwtBearer service discovery behavior
- [MassTransit.RabbitMQ 9.0.0 on NuGet](https://www.nuget.org/packages/MassTransit.RabbitMQ/) - Package availability confirmed
- [Aspire EF Core PostgreSQL integration](https://aspire.dev/integrations/databases/efcore/postgres/postgresql-client/) - ConnectionStrings__appdb env var pattern

### Tertiary (LOW confidence)
- None -- all findings verified against codebase or official sources

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - MassTransit.RabbitMQ and Kustomize are well-documented; codebase patterns are clear
- Architecture: HIGH - Transport switching pattern is standard MassTransit; K8s manifests follow Phase 24 patterns
- Pitfalls: HIGH - All pitfalls identified from direct codebase analysis (health endpoint gating, Aspire service discovery, Azure SDK dependencies)

**Research date:** 2026-02-26
**Valid until:** 2026-03-28 (stable domain, patterns unlikely to change)

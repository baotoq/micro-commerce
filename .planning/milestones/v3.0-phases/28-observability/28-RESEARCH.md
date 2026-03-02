# Phase 28: Observability - Research

**Researched:** 2026-03-02
**Domain:** Kubernetes observability — OpenTelemetry Collector + Aspire Dashboard standalone
**Confidence:** HIGH

## Summary

Phase 28 adds observability to the kind cluster by deploying two new services: an OpenTelemetry (OTEL) Collector and the .NET Aspire Dashboard in standalone mode. The existing .NET services (ApiService, Gateway) already have full OpenTelemetry instrumentation via `MicroCommerce.ServiceDefaults` — they configure logging, tracing (including MassTransit spans), and metrics, and conditionally enable the OTLP exporter when `OTEL_EXPORTER_OTLP_ENDPOINT` is set. Today in the K8s manifests, that env var is absent, so telemetry is generated but discarded. This phase wires it up.

The architecture is: **Apps -> OTEL Collector -> Aspire Dashboard**. The Collector provides a stable internal OTLP endpoint, adds batch processing and memory limiting, and forwards to the Dashboard. The Dashboard runs as an in-memory dev tool (data clears on pod restart) with anonymous access enabled for the dev cluster. Both services get Kustomize base manifests, ArgoCD Application YAMLs, and a NodePort for Dashboard UI access.

**Primary recommendation:** Deploy OTEL Collector as a Deployment (not DaemonSet) with a minimal config (OTLP receiver, memory_limiter, batch processor, OTLP exporter to Aspire Dashboard). Deploy Aspire Dashboard with `DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true`. Set `OTEL_EXPORTER_OTLP_ENDPOINT` and `OTEL_SERVICE_NAME` env vars on ApiService and Gateway deployments. Skip `k8sattributes` processor to avoid RBAC complexity — unnecessary for a dev showcase cluster.

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| OBSV-01 | OTEL Collector receives OTLP from all app services and forwards to Aspire Dashboard | OTEL Collector deployed as K8s Deployment with OTLP gRPC receiver (port 4317), batch processor, memory_limiter, and OTLP exporter to aspire-dashboard:18889. ApiService and Gateway set `OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317`. |
| OBSV-02 | Aspire Dashboard runs as a standalone container in K8s accessible for dev monitoring | `mcr.microsoft.com/dotnet/aspire-dashboard:9.3` deployed as K8s Deployment. UI on port 18888, OTLP ingestion on port 18889. NodePort 30888 -> host 38888 for local browser access. Anonymous auth via `DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true`. |
</phase_requirements>

## Standard Stack

### Core

| Component | Image/Version | Purpose | Why Standard |
|-----------|---------------|---------|--------------|
| OTEL Collector (core) | `otel/opentelemetry-collector:0.146.0` | Receive, process, and export telemetry | Core distribution is sufficient — we only need OTLP receiver, batch processor, memory_limiter, and OTLP exporter. No need for contrib distribution (no k8sattributes, no Prometheus). Pinned version avoids `latest` tag drift. |
| Aspire Dashboard (standalone) | `mcr.microsoft.com/dotnet/aspire-dashboard:9.3` | Visualize traces, metrics, logs | Official Microsoft image. Use `9.3` tag for stability (latest stable standalone release). The project uses Aspire 13.1.0 SDK but the standalone dashboard image follows its own versioning. `latest` tag also works. |

### Supporting

| Component | Purpose | When to Use |
|-----------|---------|-------------|
| `OTEL_EXPORTER_OTLP_ENDPOINT` env var | Enables OTLP export in .NET apps | Set on ApiService and Gateway K8s Deployments to point to `http://otel-collector:4317` |
| `OTEL_SERVICE_NAME` env var | Identifies service in dashboard traces | Set to `apiservice` and `gateway` respectively — matches Aspire AppHost naming |
| `DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS` | Disables Dashboard login | Set to `true` for dev cluster — no auth needed in kind |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| OTEL Collector core | OTEL Collector contrib (`otel/opentelemetry-collector-contrib`) | Contrib adds k8sattributes, Prometheus exporter, etc. — unnecessary for this phase. Core is smaller image, fewer dependencies. |
| OTEL Collector Deployment | OTEL Collector DaemonSet | DaemonSet runs on every node — overkill for single-node kind cluster. Deployment with 1 replica is simpler. |
| Skip OTEL Collector (apps -> Dashboard directly) | No Collector | Works technically, but violates OBSV-01 requirement and loses batch processing, memory limiting, and future extensibility. |
| k8sattributes processor | No k8s metadata enrichment | k8sattributes requires RBAC (ServiceAccount, ClusterRole, ClusterRoleBinding) and contrib image. Not worth complexity for dev showcase where service names already identify workloads. |
| Aspire Dashboard `9.3` tag | `latest` tag | `latest` may introduce breaking changes. Pinned tag is reproducible. However, `latest` is acceptable for dev. |

## Architecture Patterns

### Telemetry Flow

```
ApiService (port 8080)
  OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
  OTEL_SERVICE_NAME=apiservice
  |
  |--- OTLP gRPC --->  OTEL Collector (port 4317)
                          |
Gateway (port 8080)       |--- memory_limiter
  OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
  OTEL_SERVICE_NAME=gateway    |--- batch processor
  |                            |
  |--- OTLP gRPC --->         |--- OTLP exporter ---> Aspire Dashboard (port 18889)
                                                          |
                                                     Dashboard UI (port 18888)
                                                          |
                                                     NodePort 30888 -> host 38888
                                                          |
                                                     Browser: http://localhost:38888
```

### Kustomize Structure

```
infra/k8s/base/
├── otel-collector/
│   ├── kustomization.yaml        # resources: configmap.yaml, deployment.yaml, service.yaml
│   ├── configmap.yaml            # OTEL Collector config (receivers, processors, exporters, pipelines)
│   ├── deployment.yaml           # Deployment with 1 replica, configmap volume mount
│   └── service.yaml              # ClusterIP service on port 4317 (gRPC) + port 4318 (HTTP)
├── aspire-dashboard/
│   ├── kustomization.yaml        # resources: deployment.yaml, service.yaml
│   ├── deployment.yaml           # Deployment with env vars for anonymous auth
│   └── service.yaml              # ClusterIP + NodePort (30888 for UI access)
└── kustomization.yaml            # Add otel-collector/ and aspire-dashboard/ to resources list
```

### ArgoCD Integration

```
infra/k8s/argocd/apps/
├── otel-collector.yaml           # ArgoCD Application pointing to base/otel-collector
└── aspire-dashboard.yaml         # ArgoCD Application pointing to base/aspire-dashboard
```

Both use the same pattern as existing apps (source: `infra/k8s/base/{service}`, destination: `micro-commerce` namespace, automated sync with selfHeal and prune).

Note: These services do NOT need overlay directories since they use fixed public images (not built by CI). The base manifests are sufficient.

### Pattern 1: OTEL Collector ConfigMap

**What:** OTEL Collector config in a Kubernetes ConfigMap, mounted as a file.
**When to use:** Always — Collector config is YAML, not container args.
**Example:**

```yaml
# Source: OpenTelemetry Collector official docs
apiVersion: v1
kind: ConfigMap
metadata:
  name: otel-collector-config
data:
  otel-collector-config.yaml: |
    receivers:
      otlp:
        protocols:
          grpc:
            endpoint: 0.0.0.0:4317
          http:
            endpoint: 0.0.0.0:4318

    processors:
      memory_limiter:
        check_interval: 5s
        limit_percentage: 80
        spike_limit_percentage: 25
      batch:
        timeout: 5s
        send_batch_size: 1024

    exporters:
      otlp/aspire:
        endpoint: aspire-dashboard:18889
        tls:
          insecure: true

    service:
      pipelines:
        traces:
          receivers: [otlp]
          processors: [memory_limiter, batch]
          exporters: [otlp/aspire]
        metrics:
          receivers: [otlp]
          processors: [memory_limiter, batch]
          exporters: [otlp/aspire]
        logs:
          receivers: [otlp]
          processors: [memory_limiter, batch]
          exporters: [otlp/aspire]
```

### Pattern 2: OTEL Collector Deployment

**What:** K8s Deployment mounting the ConfigMap.
**Example:**

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: otel-collector
  labels:
    app: otel-collector
spec:
  replicas: 1
  selector:
    matchLabels:
      app: otel-collector
  template:
    metadata:
      labels:
        app: otel-collector
    spec:
      containers:
      - name: otel-collector
        image: otel/opentelemetry-collector:0.146.0
        args: ["--config=/etc/otelcol/otel-collector-config.yaml"]
        ports:
        - containerPort: 4317
          name: otlp-grpc
        - containerPort: 4318
          name: otlp-http
        - containerPort: 13133
          name: health
        volumeMounts:
        - name: config
          mountPath: /etc/otelcol
        livenessProbe:
          httpGet:
            path: /
            port: 13133
          periodSeconds: 15
        readinessProbe:
          httpGet:
            path: /
            port: 13133
          periodSeconds: 10
        resources:
          requests:
            memory: 64Mi
            cpu: 50m
          limits:
            memory: 256Mi
            cpu: 200m
      volumes:
      - name: config
        configMap:
          name: otel-collector-config
```

### Pattern 3: Aspire Dashboard Standalone Deployment

**What:** Dashboard as a K8s Deployment with anonymous auth.
**Example:**

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: aspire-dashboard
  labels:
    app: aspire-dashboard
spec:
  replicas: 1
  selector:
    matchLabels:
      app: aspire-dashboard
  template:
    metadata:
      labels:
        app: aspire-dashboard
    spec:
      containers:
      - name: aspire-dashboard
        image: mcr.microsoft.com/dotnet/aspire-dashboard:9.3
        ports:
        - containerPort: 18888
          name: ui
        - containerPort: 18889
          name: otlp-grpc
        env:
        - name: DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS
          value: "true"
        - name: DASHBOARD__OTLP__AUTHMODE
          value: Unsecured
        livenessProbe:
          httpGet:
            path: /
            port: 18888
          initialDelaySeconds: 5
          periodSeconds: 15
        readinessProbe:
          httpGet:
            path: /
            port: 18888
          periodSeconds: 10
        resources:
          requests:
            memory: 128Mi
            cpu: 100m
          limits:
            memory: 512Mi
            cpu: 250m
```

### Pattern 4: App Deployment Env Var Patch

**What:** Add OTEL env vars to existing ApiService and Gateway deployments.
**Example (ApiService additions):**

```yaml
# Added to infra/k8s/base/apiservice/deployment.yaml env section
- name: OTEL_EXPORTER_OTLP_ENDPOINT
  value: "http://otel-collector:4317"
- name: OTEL_SERVICE_NAME
  value: "apiservice"
```

The ServiceDefaults code already checks `OTEL_EXPORTER_OTLP_ENDPOINT` and enables the OTLP exporter:
```csharp
// From src/MicroCommerce.ServiceDefaults/Extensions.cs
var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
if (useOtlpExporter)
{
    builder.Services.AddOpenTelemetry().UseOtlpExporter();
}
```

No code changes needed — only K8s manifest changes.

### Anti-Patterns to Avoid

- **Apps exporting directly to Aspire Dashboard (skipping Collector):** Violates OBSV-01 requirement. Loses batch processing, memory limiting, and the ability to add backends later without changing app config.
- **Using `latest` image tag for OTEL Collector:** Tag drift causes silent breakage. Always pin a specific version.
- **DaemonSet for OTEL Collector on single-node kind:** Wasteful. Deployment with 1 replica is correct for this topology.
- **Adding k8sattributes without RBAC:** Collector crashes on startup with permission denied errors. Either add full RBAC chain or skip k8sattributes entirely.
- **Forgetting `tls: insecure: true` on OTLP exporter to Dashboard:** Dashboard runs HTTP internally. Without this, Collector fails to connect with TLS handshake error.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Telemetry collection | Custom sidecar or log scraper | OTEL Collector | Standard, well-tested, community-maintained |
| Telemetry visualization | Custom dashboard | Aspire Dashboard standalone | Microsoft-maintained, OTLP-native, zero config |
| OTLP export from .NET | Custom exporter middleware | `OpenTelemetry.Exporter.OpenTelemetryProtocol` via ServiceDefaults | Already wired up — just set env var |
| Memory protection for Collector | Hope for the best | `memory_limiter` processor | Prevents OOM kills that take down entire observability pipeline |

**Key insight:** The .NET apps are already fully instrumented. This phase is pure K8s manifest work — no application code changes needed.

## Common Pitfalls

### Pitfall 1: Missing `OTEL_SERVICE_NAME` Env Var

**What goes wrong:** All traces appear as "unknown_service" in the Dashboard, making it impossible to distinguish ApiService from Gateway.
**Why it happens:** .NET OTEL SDK defaults `service.name` to "unknown_service:processname" when `OTEL_SERVICE_NAME` is not set. In Aspire mode, the AppHost sets this automatically, but in K8s there is no AppHost.
**How to avoid:** Set `OTEL_SERVICE_NAME` env var on every instrumented Deployment.
**Warning signs:** Traces in Dashboard all show the same generic service name.

### Pitfall 2: Skipping `memory_limiter` Processor

**What goes wrong:** Under burst traffic, OTEL Collector consumes unbounded memory and gets OOM-killed by Kubernetes.
**Why it happens:** Without `memory_limiter`, the Collector buffers all incoming telemetry in memory. The `batch` processor alone does not limit memory.
**How to avoid:** Always include `memory_limiter` as the FIRST processor in every pipeline. Use `limit_percentage: 80` and `spike_limit_percentage: 25` for containerized deployments.
**Warning signs:** OOM kills on the otel-collector pod, gaps in telemetry data.

### Pitfall 3: Wrong Aspire Dashboard OTLP Port

**What goes wrong:** OTEL Collector cannot connect to Dashboard. Traces never appear.
**Why it happens:** Dashboard UI is port 18888, but OTLP ingestion is port **18889** (gRPC) or 18890 (HTTP). Easy to confuse the two.
**How to avoid:** OTEL Collector exporter must target port 18889, not 18888. The Service should expose 18889 for OTLP.
**Warning signs:** Connection refused errors in Collector logs.

### Pitfall 4: kind Port Mapping Not Updated

**What goes wrong:** Dashboard is running in cluster but unreachable from host browser.
**Why it happens:** The kind-config.yaml must map a containerPort for the Dashboard's NodePort. If the kind cluster was already created without this mapping, it must be recreated.
**How to avoid:** Add the Aspire Dashboard NodePort mapping to kind-config.yaml. Document that existing clusters must be recreated (`kind delete cluster && bootstrap.sh`).
**Warning signs:** `curl localhost:38888` refuses connection even though `kubectl port-forward` works.

### Pitfall 5: OTEL Collector Using `tls: insecure: false` (default)

**What goes wrong:** Collector cannot export to Dashboard because it tries TLS against an HTTP endpoint.
**Why it happens:** The OTLP exporter defaults to TLS. Dashboard's internal OTLP endpoint is plain HTTP.
**How to avoid:** Always set `tls: insecure: true` on the `otlp/aspire` exporter.
**Warning signs:** "TLS handshake" or "certificate" errors in Collector logs.

### Pitfall 6: ArgoCD Application Missing for New Services

**What goes wrong:** OTEL Collector and Dashboard are in base manifests but ArgoCD never deploys them.
**Why it happens:** ArgoCD app-of-apps pattern requires an Application YAML per service in `infra/k8s/argocd/apps/`.
**How to avoid:** Create `otel-collector.yaml` and `aspire-dashboard.yaml` in the ArgoCD apps directory, following the existing pattern.
**Warning signs:** `kubectl get applications -n argocd` does not show the new services.

## Code Examples

### OTEL Collector Health Check

The Collector exposes health on port 13133 by default:

```yaml
# Source: https://github.com/open-telemetry/opentelemetry-collector/tree/main/extension/healthcheckextension
extensions:
  health_check:
    endpoint: 0.0.0.0:13133

service:
  extensions: [health_check]
  pipelines:
    # ...
```

Note: The health_check extension is included by default in the core distribution and is enabled automatically. You only need to explicitly configure it if you want to change the port.

### Complete OTEL Collector Service YAML

```yaml
apiVersion: v1
kind: Service
metadata:
  name: otel-collector
spec:
  selector:
    app: otel-collector
  ports:
  - port: 4317
    targetPort: 4317
    name: otlp-grpc
  - port: 4318
    targetPort: 4318
    name: otlp-http
```

ClusterIP only — no NodePort needed. Only accessed by ApiService/Gateway within the cluster.

### Complete Aspire Dashboard Service YAML

```yaml
apiVersion: v1
kind: Service
metadata:
  name: aspire-dashboard
spec:
  selector:
    app: aspire-dashboard
  ports:
  - port: 18888
    targetPort: 18888
    name: ui
  - port: 18889
    targetPort: 18889
    name: otlp-grpc
---
apiVersion: v1
kind: Service
metadata:
  name: aspire-dashboard-nodeport
spec:
  type: NodePort
  selector:
    app: aspire-dashboard
  ports:
  - port: 18888
    targetPort: 18888
    nodePort: 30888
    name: ui
```

Separate ClusterIP (for internal OTLP) and NodePort (for host browser access).

### ArgoCD Application YAML (Example for otel-collector)

```yaml
# Source: follows existing pattern from infra/k8s/argocd/apps/apiservice.yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: otel-collector
  namespace: argocd
  finalizers:
  - resources-finalizer.argocd.argoproj.io
spec:
  project: default
  source:
    repoURL: https://github.com/baotoq/micro-commerce.git
    targetRevision: master
    path: infra/k8s/base/otel-collector
  destination:
    server: https://kubernetes.default.svc
    namespace: micro-commerce
  syncPolicy:
    automated:
      selfHeal: true
      prune: true
    syncOptions:
    - CreateNamespace=true
    retry:
      limit: 5
      backoff:
        duration: 5s
        factor: 2
        maxDuration: 3m
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Apps export directly to visualization tool | OTEL Collector as intermediary | 2022+ (OTEL Collector GA) | Standard practice — decouples apps from backend |
| Aspire Dashboard requires AppHost | Standalone Docker image available | Aspire 8.0+ (2024) | Dashboard usable in any K8s cluster |
| `ASPIRE_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS` | `DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS` | Aspire 9.x | Env var name changed — use the `DOTNET_` prefix |
| OTEL Collector `memory_limiter` uses `limit_mib` | `limit_percentage` preferred for containers | 2023+ | Scales with container resource limits automatically |
| Dashboard OTLP on port 4317 | Dashboard OTLP on port 18889 (internal) | Aspire 9.x | Docker maps 4317->18889 but K8s should target 18889 directly |

**Deprecated/outdated:**
- `ASPIRE_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS` — replaced by `DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS` in newer versions (both may work for backwards compatibility)
- OTEL Collector contrib example at `0.51.0` in GitHub — very outdated; current is `0.146.0`

## Open Questions

1. **Exact Aspire Dashboard image tag alignment with project SDK**
   - What we know: Project uses Aspire SDK 13.1.0. Dashboard standalone image uses its own version scheme (currently 9.x).
   - What's unclear: Whether `9.3` or `latest` is the best tag for the standalone dashboard image.
   - Recommendation: Use `9.3` for reproducibility. Can switch to `latest` if specific features are needed. The dashboard image tag does NOT need to match the SDK version.

2. **Kind cluster recreation required**
   - What we know: Adding a new NodePort mapping (30888 for Dashboard) requires updating kind-config.yaml. Kind does not support hot-adding port mappings.
   - What's unclear: Whether the bootstrap script documentation should emphasize cluster recreation.
   - Recommendation: Update kind-config.yaml and document that existing clusters must be recreated. The bootstrap script already handles idempotent cluster creation.

3. **OTEL Collector core vs contrib image**
   - What we know: Core image has OTLP receiver, batch processor, memory_limiter, OTLP exporter. Contrib adds k8sattributes, Prometheus, etc.
   - What's unclear: Whether the core image includes the health_check extension.
   - Recommendation: Use core image (`otel/opentelemetry-collector:0.146.0`). Health check extension is included in core by default. If issues arise, switch to contrib.

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | Manual validation via kubectl + curl (infrastructure-only phase) |
| Config file | N/A — no application code changes |
| Quick run command | `kubectl get pods -n micro-commerce -l app=otel-collector && kubectl get pods -n micro-commerce -l app=aspire-dashboard` |
| Full suite command | `kubectl logs deployment/otel-collector -n micro-commerce --tail=20 && curl -s -o /dev/null -w "%{http_code}" http://localhost:38888` |

### Phase Requirements -> Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| OBSV-01 | OTEL Collector receives OTLP and forwards to Dashboard | smoke | `kubectl logs deployment/otel-collector -n micro-commerce \| grep -i "exporting"` | N/A — infrastructure validation |
| OBSV-01 | ApiService/Gateway have OTLP endpoint configured | smoke | `kubectl get deployment apiservice -n micro-commerce -o yaml \| grep OTEL_EXPORTER_OTLP_ENDPOINT` | N/A — manifest check |
| OBSV-02 | Aspire Dashboard is accessible at localhost:38888 | smoke | `curl -s -o /dev/null -w "%{http_code}" http://localhost:38888` (expect 200) | N/A — curl check |
| OBSV-02 | Dashboard shows live traces | manual-only | Open `http://localhost:38888` in browser, trigger checkout flow, verify traces visible | N/A — requires human visual verification |
| Success-3 | OTEL Collector pod stays healthy | smoke | `kubectl get pods -n micro-commerce -l app=otel-collector -o jsonpath='{.items[0].status.containerStatuses[0].restartCount}'` (expect 0) | N/A — kubectl check |

### Sampling Rate

- **Per task commit:** `kubectl get pods -n micro-commerce` (verify pod status)
- **Per wave merge:** Full smoke commands above
- **Phase gate:** All smoke checks pass + manual Dashboard trace verification

### Wave 0 Gaps

None — this phase is purely Kubernetes manifest work. No test framework setup needed. Validation is smoke tests via kubectl and curl against a running kind cluster.

## Sources

### Primary (HIGH confidence)
- [Aspire Dashboard Standalone Mode](https://aspire.dev/dashboard/standalone/) - Docker run commands, port mappings, env vars
- [Aspire Dashboard Configuration](https://aspire.dev/dashboard/configuration/) - All environment variables, OTLP endpoints, auth modes, telemetry limits
- [Aspire Dashboard Security](https://aspire.dev/dashboard/security-considerations/) - OTLP auth, unsecured mode, API key configuration
- [OpenTelemetry Collector Configuration](https://opentelemetry.io/docs/collector/configuration/) - Receivers, processors, exporters, pipelines
- [OTEL Collector OTLP Receiver](https://github.com/open-telemetry/opentelemetry-collector/blob/main/receiver/otlpreceiver/README.md) - gRPC/HTTP protocol config
- [OTEL Collector memory_limiter](https://github.com/open-telemetry/opentelemetry-collector/blob/main/processor/memorylimiterprocessor/README.md) - Processor config, limit_percentage for containers
- [OTEL Collector K8s example](https://github.com/open-telemetry/opentelemetry-collector-contrib/blob/main/examples/kubernetes/otel-collector.yaml) - DaemonSet/Deployment patterns
- Project codebase: `src/MicroCommerce.ServiceDefaults/Extensions.cs` - Existing OTEL instrumentation and OTLP conditional export

### Secondary (MEDIUM confidence)
- [k8sattributes processor RBAC](https://github.com/open-telemetry/opentelemetry-collector-contrib/blob/main/processor/k8sattributesprocessor/README.md) - Required ServiceAccount, ClusterRole, ClusterRoleBinding
- [Aspire Dashboard Helm chart](https://artifacthub.io/packages/helm/aspire-dashboard/aspire-dashboard) - Community Helm chart (not used but confirms deployment patterns)
- [OTEL Collector Docker Hub](https://hub.docker.com/r/otel/opentelemetry-collector) - Image versions
- [Aspire 13.1 Release](https://github.com/dotnet/aspire/releases/tag/v13.1.0) - SDK version alignment

### Tertiary (LOW confidence)
- [OTEL Collector contrib latest version](https://github.com/open-telemetry/opentelemetry-collector-contrib/releases) - Version 0.146.0 per search results, needs verification at implementation time

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - Official OTEL Collector and Aspire Dashboard docs are comprehensive and current
- Architecture: HIGH - Follows well-established OTEL Collector -> backend pattern; project's existing K8s manifest patterns are clear
- Pitfalls: HIGH - Memory limiter, port confusion, and TLS insecure are well-documented in official sources and project's own pitfalls research

**Research date:** 2026-03-02
**Valid until:** 2026-04-02 (30 days — stable technologies, pinned versions)

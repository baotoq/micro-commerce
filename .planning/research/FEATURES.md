# Feature Research

**Domain:** Kubernetes & GitOps deployment for .NET microservices e-commerce platform
**Researched:** 2026-02-25
**Confidence:** HIGH (verified against official Kubernetes docs, ArgoCD docs, OTEL docs, and current community sources)

## Context

This research covers v3.0 milestone: adding K8s deployment, ArgoCD GitOps, CI/CD image pipelines, Kustomize overlays, secrets management, and OTEL monitoring to the existing MicroCommerce platform. The existing platform already has:

- .NET Aspire orchestration (used for local dev, NOT deployed to K8s)
- OpenTelemetry instrumentation wired via `MicroCommerce.ServiceDefaults`
- Health endpoints at `/health` (readiness) and `/alive` (liveness)
- YARP Gateway centralizing CORS, auth, rate limiting
- 3 deployable services: ApiService, Gateway, Web (Next.js)
- Infrastructure dependencies: PostgreSQL, Keycloak, RabbitMQ (replacing Azure SB emulator), Azure Blob Storage

---

## Feature Landscape

### Table Stakes (Must-Have for Production-Like K8s)

Features reviewers and engineers expect in any K8s deployment. Missing these = the setup is not credible.

| Feature | Why Expected | Complexity | Aspire Dependency | Notes |
|---------|--------------|------------|-------------------|-------|
| **Dockerfiles for all 3 services** | K8s requires container images; Aspire builds images internally but does not expose Dockerfiles for external registry push | MEDIUM | Replaces Aspire's implicit build | Multi-stage: `sdk` for build, `aspnet` or chiseled runtime for prod. Next.js uses `node` build + `node:alpine` runtime. Non-root user required. |
| **Kubernetes Deployment manifests** | Core K8s resource type; without it nothing runs | LOW | None (K8s replaces Aspire) | One Deployment per service (ApiService, Gateway, Web). `replicas: 1` for showcase. |
| **Kubernetes Service manifests** | Pods need stable DNS for inter-service communication | LOW | Replaces Aspire's service discovery | ClusterIP for internal services (ApiService, Gateway). LoadBalancer or NodePort for kind local access. |
| **Liveness and Readiness probes** | Without probes, K8s cannot detect unhealthy pods; bad pods receive traffic | LOW | `/alive` and `/health` already implemented | Map `/alive` to liveness, `/health` to readiness. Already wired in CLAUDE.md. No code change required. |
| **Resource requests and limits** | Without limits, a runaway pod starves other pods; without requests, scheduler cannot place pods | LOW | None | CPU/memory requests+limits per container. ApiService: `cpu: 250m/1000m`, `memory: 256Mi/512Mi`. Gateway: `cpu: 100m/500m`. Web: `cpu: 100m/500m`. |
| **ConfigMaps for non-secret config** | Connection strings, feature flags, OTEL endpoint, URLs must be externalized; hardcoded config fails in K8s | LOW | None | Aspire injected connection strings via service references; K8s needs explicit ConfigMaps or env vars from ConfigMap. |
| **Secrets for sensitive values** | Passwords, client secrets, auth tokens must NOT live in ConfigMaps or manifests | LOW | None | PostgreSQL password, Keycloak client secret, RabbitMQ credentials. Base Kubernetes Secret is minimum; Sealed Secrets is the GitOps-safe upgrade. |
| **Kustomize base manifests** | Standard tool for K8s templating without external dependencies; built into kubectl | MEDIUM | None | `k8s/base/` with all Deployment/Service/ConfigMap YAMLs. Kustomize bundles them without duplication. |
| **Kustomize dev overlay** | Dev cluster (kind) differs from base: image tags, replica counts, resource limits may vary | LOW | None | `k8s/overlays/dev/` patches image repo/tag, sets `imagePullPolicy: Always`, adjusts resources. |
| **GitHub Actions image build pipeline** | CI/CD without image automation means manual `docker push` before every deploy; defeats GitOps | MEDIUM | Existing `dotnet-test.yml` for tests | New `build-images.yml`: checkout, build 3 images with Docker buildx, push to `ghcr.io/${{ github.repository }}/` tagged with `${{ github.sha }}`. Triggers on push to `main`. |
| **ghcr.io image registry** | Public free registry tied to the GitHub repo; no external account needed for a showcase | LOW | None | `GITHUB_TOKEN` automatically available in Actions. No extra secret setup. `ghcr.io/owner/micro-commerce/apiservice:sha` pattern. |
| **ArgoCD Application manifests** | GitOps engine; without it deployment is manual kubectl apply | MEDIUM | None | ArgoCD installed in cluster. One Application per service or app-of-apps root. Watches git repo, syncs on commit. |
| **RabbitMQ in K8s** | Azure Service Bus emulator does not run in K8s; RabbitMQ is the standard self-hosted replacement | MEDIUM | Replaces Azure SB emulator | RabbitMQ Deployment + Service manifest. MassTransit already supports RabbitMQ transport; switch `UsingAzureServiceBus` to `UsingRabbitMq`. |
| **PostgreSQL in K8s** | Database must be deployed in dev cluster; StatefulSet for persistence | MEDIUM | Replaces Aspire's Postgres container | StatefulSet + PersistentVolumeClaim + Service. kind requires local-path provisioner for PVC. |
| **Keycloak in K8s** | Auth must run in cluster; Realm import must be automated | MEDIUM | Replaces Aspire's Keycloak container | Deployment + ConfigMap for realm JSON mount + Service. Realm file already lives in `AppHost/Realms/`. |
| **kind cluster setup** | Local dev environment must mirror production K8s; kind is standard for this | LOW | Replaces `dotnet run AppHost` for K8s testing | `kind create cluster --config kind-config.yaml`. Needs port mapping for NodePort access. Single-node sufficient for showcase. |
| **OTEL Collector in K8s** | Aspire Dashboard is a developer tool, not production monitoring; a real OTEL Collector is the production equivalent | MEDIUM | Replaces Aspire's built-in dashboard | OTEL Collector Deployment + Service. Services point `OTEL_EXPORTER_OTLP_ENDPOINT` to collector. Collector can forward to Aspire Dashboard standalone container. |
| **Aspire Dashboard as standalone container** | Aspire Dashboard runs as a standalone container (not AppHost dependency); bridges local dev observability to K8s | LOW | Complements, does not require Aspire | `mcr.microsoft.com/dotnet/aspire-dashboard:latest` image deployed as K8s Deployment. OTEL Collector forwards to it. |
| **imagePullSecret for ghcr.io** | Private ghcr.io packages require a pull secret in the cluster | LOW | None | `kubectl create secret docker-registry ghcr-secret --docker-server=ghcr.io`. Referenced in pod spec `imagePullSecrets`. |
| **Ingress or Gateway routing** | External traffic needs a single entry point into the cluster; raw NodePort is not viable beyond dev | MEDIUM | Replaces YARP's external-facing role | For kind: use NodePort on Gateway service for local access. For more polish: Traefik or Envoy Gateway (not Ingress NGINX — EOL March 2026). |

### Differentiators (Nice-to-Have, Showcase Value)

Features that demonstrate GitOps and K8s maturity beyond bare minimum, but are not required for the cluster to function.

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| **Sealed Secrets for GitOps-safe secrets** | Encrypts K8s secrets so they can be committed to Git; demonstrates GitOps security discipline | MEDIUM | `kubeseal` CLI + controller in cluster. Encrypted SealedSecret YAML checked into repo. Decryption key stays in cluster only. Good showcase story. |
| **ArgoCD app-of-apps pattern** | Hierarchical GitOps: one root App that manages all child Apps declaratively | MEDIUM | Root `Application` manifest points to `k8s/argocd/apps/` directory containing child Application YAMLs. Demonstrates production GitOps patterns. |
| **Image tag update automation (GitOps loop)** | CI builds image, updates Kustomize overlay with new sha tag, ArgoCD detects git change and syncs | HIGH | Requires CI to commit back to repo (or use ArgoCD Image Updater). Closes the full GitOps loop. Strong demo value but complex to implement correctly. |
| **OTEL Collector pipeline configuration** | Multi-stage telemetry pipeline (receivers, processors, exporters) demonstrates real observability engineering | MEDIUM | `otel-collector-config.yaml` with OTLP receiver, batch processor, debug exporter, and OTLP exporter to Aspire Dashboard. |
| **Startup probes for slow-starting services** | Keycloak and PostgreSQL have long startup times; startup probes prevent premature liveness failures during init | LOW | `startupProbe` with higher `failureThreshold` on Keycloak and ApiService. Demonstrates K8s probe awareness. |
| **PodDisruptionBudget** | Ensures at least 1 pod survives voluntary disruptions (rolling updates, node drain) | LOW | Meaningful only with `replicas >= 2`; for showcase with `replicas: 1` it is documentation-only value. |
| **Namespace isolation** | Isolates all MicroCommerce resources in a dedicated K8s namespace | LOW | `namespace: micro-commerce` on all manifests. Keeps cluster clean. Low effort, high polish. |
| **Health-based ArgoCD sync** | ArgoCD waits for health before marking sync complete; requires health checks to be correct | LOW | Default ArgoCD behavior if probes are correct; no extra work if table stakes are done right. |
| **MassTransit transport abstraction** | Single codebase, configurable transport: Azure SB emulator for Aspire dev, RabbitMQ for K8s | MEDIUM | `IConfiguration`-driven transport selection. `MASSTRANSIT_TRANSPORT=rabbitmq` env var in K8s ConfigMap, unset for local Aspire (defaults to Azure SB). |

### Anti-Features (Over-Engineering for Showcase)

Features that seem relevant but add complexity without proportional value for a showcase project.

| Feature | Why Requested | Why Avoid | Alternative |
|---------|---------------|-----------|-------------|
| **Service Mesh (Istio, Linkerd)** | mTLS between services, traffic policies, advanced observability | Adds 4+ CRDs, sidecar injection, significant operational complexity; YARP Gateway already centralizes auth and routing | Keep YARP for auth/routing; OTEL Collector for observability |
| **Horizontal Pod Autoscaler (HPA)** | Demonstrates scaling capability | Requires Metrics Server, meaningful only under load; showcase traffic is near-zero; HPA at `replicas: 1` does nothing | Document HPA as "next step", add a single HPA manifest with commented-out thresholds |
| **Multi-cluster deployment** | Demonstrates GitOps at scale | Massively increases infrastructure cost and complexity; a showcase has one cluster | Single kind cluster is sufficient to demonstrate all K8s and GitOps concepts |
| **Helm charts instead of Kustomize** | Helm is widely used for packaging | Helm adds templating language, requires chart packaging; Kustomize is built into kubectl and simpler for single-project configs | Use Kustomize; note Helm as alternative in docs |
| **External Secrets Operator (ESO)** | Enterprise-grade secrets from Vault/AWS/Azure | Requires an external secret store (Vault or cloud provider); adds infra dependency that does not run locally | Sealed Secrets is sufficient: no external dependency, works fully offline with kind |
| **FluxCD instead of ArgoCD** | FluxCD is Kubernetes-native, no UI overhead | PROJECT.md already specifies ArgoCD; switching tools mid-milestone is waste | Stay with ArgoCD as specified |
| **Network Policies** | Controls pod-to-pod communication | kind's default CNI (kindnet) has limited NetworkPolicy support; requires Calico or Cilium in kind; adds setup complexity | Document network policies as production hardening step; skip for showcase |
| **Certificate management (cert-manager)** | TLS for ingress | TLS termination at ingress is production concern; kind local cluster with self-signed or no TLS is standard for dev | Skip; note cert-manager as production step |
| **Multi-environment overlays (staging, prod)** | Demonstrates environment promotion | Only dev overlay has a running cluster (kind); staging/prod overlays would be empty placeholders | One dev overlay is sufficient; structure the repo so adding prod overlay is obvious |
| **PostgreSQL Operator (CloudNativePG, Zalando)** | Production-grade PostgreSQL HA | Adds operator CRDs, complex HA setup; a showcase needs a running PostgreSQL, not HA PostgreSQL | Simple StatefulSet with single replica + PVC |
| **Azure Blob Storage in K8s** | Production image storage | Azurite emulator does not run well as a K8s StatefulSet; avatar/image upload is not core to K8s demo | Either disable Blob features in K8s (use placeholder images) or use a MinIO StatefulSet as replacement |

---

## Feature Dependencies

```
[Dockerfiles]
    └──required-by──> [GitHub Actions image pipeline]
                           └──required-by──> [imagePullSecret in cluster]
                                                └──required-by──> [K8s Deployments pulling from ghcr.io]

[K8s Deployments]
    └──requires──> [Kustomize base manifests]
                       └──enhanced-by──> [Kustomize dev overlay]
                                             └──enhanced-by──> [ArgoCD Application manifests]

[RabbitMQ in K8s]
    └──required-by──> [MassTransit transport switch]
                           └──requires──> [Secrets for RabbitMQ credentials]

[PostgreSQL StatefulSet]
    └──required-by──> [ApiService can start]
                           └──requires──> [ConfigMap with connection string]
                                             └──requires──> [Secret with PostgreSQL password]

[Keycloak in K8s]
    └──required-by──> [ApiService JWT validation]
    └──required-by──> [Gateway JWT validation]
    └──required-by──> [Web NextAuth.js login]
    └──requires──> [Realm ConfigMap with realm JSON]

[OTEL Collector]
    └──enhanced-by──> [Aspire Dashboard standalone container]
    └──required-by──> [Services OTEL_EXPORTER_OTLP_ENDPOINT env var]

[Sealed Secrets controller]
    └──required-by──> [SealedSecret manifests in Git]
    └──conflicts-with──> [Plain base64 Secrets in Git (never commit these)]

[ArgoCD app-of-apps]
    └──requires──> [ArgoCD installed in cluster]
    └──requires──> [All K8s manifests committed to Git]
    └──enhances──> [GitHub Actions image pipeline] (GitOps loop: CI build -> git commit -> ArgoCD sync)

[kind cluster]
    └──required-by──> [Local testing of all above]
    └──requires──> [local-path-provisioner for PVC support]
    └──requires──> [Port-forward or NodePort for external access]
```

### Dependency Notes

- **Dockerfiles required before everything else:** Without container images, no K8s resource can start. Dockerfiles are the root dependency for the entire milestone.
- **RabbitMQ replaces Azure Service Bus emulator:** The existing Aspire stack uses Azure Service Bus emulator. This emulator is not suitable for K8s deployment. MassTransit supports both transports; switching is a config-level change, not an architecture change.
- **PostgreSQL PVC requires kind local-path provisioner:** kind does not provision PVCs by default in all versions. The `local-path-provisioner` must be confirmed available or explicitly installed.
- **Keycloak realm import:** The realm JSON in `AppHost/Realms/` must be mounted as a ConfigMap volume in the Keycloak Deployment. This replaces Aspire's `.WithRealmImport()` call.
- **OTEL without Aspire AppHost:** In production (K8s), `OTEL_EXPORTER_OTLP_ENDPOINT` must be explicitly set in ConfigMaps or Deployment env vars. Aspire auto-injects this in local dev; K8s does not.
- **MassTransit transport switch must be backward compatible:** Local Aspire dev still uses Azure SB emulator. The transport selection must be driven by configuration, not compilation.
- **Ingress NGINX is EOL March 2026:** Do not use Ingress NGINX. Use NodePort for kind local access, or Traefik/Envoy Gateway for more polished routing.

---

## MVP Definition

### Launch With (v3.0 Core)

Minimum required for the cluster to run the full application end-to-end.

- [ ] Dockerfiles for ApiService, Gateway, Web — images must exist to deploy anything
- [ ] Kustomize base manifests for all 3 services (Deployment + Service + ConfigMap) — core K8s resources
- [ ] Kustomize dev overlay for kind cluster — image repo/tag overrides
- [ ] PostgreSQL StatefulSet + PVC + Service — database must run in cluster
- [ ] RabbitMQ Deployment + Service + ConfigMap — messaging must run in cluster
- [ ] Keycloak Deployment + Service + Realm ConfigMap — auth must run in cluster
- [ ] Secrets for PostgreSQL password, RabbitMQ credentials, Keycloak client secret
- [ ] Liveness and readiness probes configured on all 3 app services — table stake for K8s health
- [ ] Resource requests and limits on all containers — without this, kind cluster scheduler misbehaves
- [ ] GitHub Actions workflow: build + push 3 images to ghcr.io on push to main
- [ ] imagePullSecret in cluster for ghcr.io
- [ ] MassTransit RabbitMQ transport support (configurable via env var)
- [ ] OTEL Collector + Aspire Dashboard standalone in cluster — observability must work
- [ ] ArgoCD Application manifests with app-of-apps root — GitOps deployment working

### Add After Core Works (v3.0 Polish)

Features to add once the cluster runs end-to-end.

- [ ] Sealed Secrets replacing plain K8s Secrets — GitOps-safe secret management
- [ ] Namespace isolation (`micro-commerce` namespace on all resources)
- [ ] Startup probes for Keycloak and ApiService (slow startup protection)
- [ ] Image tag update automation (CI commits sha back to overlay, ArgoCD auto-syncs)
- [ ] MassTransit transport abstraction (`MASSTRANSIT_TRANSPORT` env var switches between Azure SB and RabbitMQ)

### Future Consideration (Post v3.0)

Features to defer until core is validated and working.

- [ ] HPA — meaningful only with real load; document as next step
- [ ] Network Policies — requires non-default CNI in kind; production hardening
- [ ] cert-manager + TLS — production hardening, not needed for showcase
- [ ] PostgreSQL Operator (CloudNativePG) — production HA, not needed for showcase
- [ ] Multi-environment overlays (staging, prod) — only dev cluster exists
- [ ] MinIO for Blob Storage replacement — low demo value; disable or use placeholder images

---

## Feature Prioritization Matrix

| Feature | Showcase Value | Implementation Cost | Priority |
|---------|---------------|---------------------|----------|
| Dockerfiles (3 services) | HIGH | MEDIUM | P1 |
| Kustomize base manifests | HIGH | MEDIUM | P1 |
| GitHub Actions image pipeline | HIGH | MEDIUM | P1 |
| PostgreSQL StatefulSet | HIGH | MEDIUM | P1 |
| RabbitMQ in K8s + MassTransit switch | HIGH | MEDIUM | P1 |
| Keycloak in K8s + realm import | HIGH | MEDIUM | P1 |
| ArgoCD app-of-apps | HIGH | MEDIUM | P1 |
| Liveness/readiness probes | HIGH | LOW | P1 |
| Resource limits | MEDIUM | LOW | P1 |
| OTEL Collector + Aspire Dashboard | HIGH | MEDIUM | P1 |
| Sealed Secrets | HIGH | MEDIUM | P2 |
| Namespace isolation | MEDIUM | LOW | P2 |
| Startup probes | MEDIUM | LOW | P2 |
| Image tag update automation | HIGH | HIGH | P2 |
| MassTransit transport abstraction | MEDIUM | MEDIUM | P2 |
| Kustomize dev overlay | MEDIUM | LOW | P2 |
| HPA | LOW | MEDIUM | P3 |
| Network Policies | LOW | HIGH | P3 |
| cert-manager TLS | LOW | MEDIUM | P3 |
| Service Mesh | LOW | HIGH | P3 |
| PostgreSQL Operator | LOW | HIGH | P3 |

**Priority key:**
- P1: Must have for a working K8s demo
- P2: Should have for a credible GitOps demo
- P3: Defer; mention as "production next steps" in docs

---

## Complexity Notes Per Area

### Dockerfiles (MEDIUM complexity)

Multi-stage builds are non-trivial for first-time .NET K8s deployments:
- `mcr.microsoft.com/dotnet/sdk:10.0` for build stage
- `mcr.microsoft.com/dotnet/aspnet:10.0` (or chiseled variant) for runtime
- Must handle `COPY *.csproj` + `dotnet restore` before `COPY .` to maximize layer cache
- Must set `ASPNETCORE_URLS=http://+:8080` and run as non-root user
- Next.js Dockerfile is separate: node build + standalone output + `node:alpine` runtime

### Kustomize (LOW-MEDIUM complexity)

Kustomize is built into kubectl; no separate install for basic use. Structure:
```
k8s/
  base/
    kustomization.yaml
    apiservice/deployment.yaml, service.yaml, configmap.yaml
    gateway/deployment.yaml, service.yaml
    web/deployment.yaml, service.yaml
    postgres/statefulset.yaml, pvc.yaml, service.yaml
    rabbitmq/deployment.yaml, service.yaml
    keycloak/deployment.yaml, service.yaml, configmap.yaml (realm)
    otel/collector-deployment.yaml, aspire-dashboard-deployment.yaml
  overlays/
    dev/
      kustomization.yaml  (patches image tags, sets imagePullPolicy: Always)
```

### RabbitMQ Transport Switch (MEDIUM complexity)

MassTransit already supports RabbitMQ. The only change is in `Program.cs`:
```csharp
// Instead of: .UsingAzureServiceBus(...)
// Use: .UsingRabbitMq((ctx, cfg) => { cfg.Host("rabbitmq://rabbitmq-service/"); })
// Driven by: IConfiguration["MassTransit:Transport"] == "rabbitmq"
```
This is a configuration change, not an architecture change. The challenge is ensuring the Azure SB emulator path still works in Aspire local dev.

### ArgoCD App-of-Apps (MEDIUM complexity)

Two layers of ArgoCD YAML:
1. Root `Application` pointing to `k8s/argocd/apps/` in Git
2. Child `Application` YAMLs in that directory, each pointing to a Kustomize overlay

The complexity is in bootstrapping: ArgoCD itself must be installed in the cluster before it can manage anything, including itself (optional: make ArgoCD manage itself).

### Sealed Secrets (MEDIUM complexity)

Three steps:
1. Install `sealed-secrets-controller` in cluster
2. Install `kubeseal` CLI
3. For each K8s Secret: `kubeseal < secret.yaml > sealed-secret.yaml` and commit the sealed version

The challenge is key management: if the cluster is destroyed, the sealing key is lost and secrets must be re-sealed. For kind dev clusters, document this limitation explicitly.

### OTEL Without Aspire AppHost (MEDIUM complexity)

Aspire auto-injects `OTEL_EXPORTER_OTLP_ENDPOINT` into processes it manages. In K8s, this must be explicit:
```yaml
env:
  - name: OTEL_EXPORTER_OTLP_ENDPOINT
    value: "http://otel-collector-service:4317"
  - name: OTEL_SERVICE_NAME
    value: "apiservice"
```
The `MicroCommerce.ServiceDefaults` package already configures OTLP exporting; it just needs the endpoint env var set.

---

## Sources

- [Kubernetes Configuration Good Practices](https://kubernetes.io/blog/2025/11/25/configuration-good-practices/)
- [Kubernetes Horizontal Pod Autoscaler](https://kubernetes.io/docs/concepts/workloads/autoscaling/horizontal-pod-autoscale/)
- [Kustomize with ArgoCD](https://argo-cd.readthedocs.io/en/stable/user-guide/kustomize/)
- [ArgoCD Secret Management](https://argo-cd.readthedocs.io/en/stable/operator-manual/secret-management/)
- [ArgoCD App-of-Apps Pattern (CNCF Blog 2025)](https://www.cncf.io/blog/2025/10/07/managing-kubernetes-workloads-using-the-app-of-apps-pattern-in-argocd-2/)
- [ArgoCD App-of-Apps vs ApplicationSet](https://bytegoblin.io/blog/argocd-deployment-patterns-app-of-apps-vs-applicationsets.mdx)
- [Sealed Secrets for GitOps (RedHat)](https://www.redhat.com/en/blog/a-guide-to-secrets-management-with-gitops-and-kubernetes)
- [Kubernetes Secrets Management 2025 (Atmosly)](https://atmosly.com/blog/kubernetes-secrets-management-vault-vs-sealed-secrets-vs-external-secrets-2025)
- [OpenTelemetry Collector on Kubernetes](https://opentelemetry.io/docs/platforms/kubernetes/collector/)
- [OTEL with .NET Aspire (Microsoft Learn)](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-otlp-example)
- [Aspire Dashboard as Standalone Container](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/telemetry)
- [GitHub Actions + ArgoCD K8s Deployment](https://sheraziqbal.medium.com/practical-guide-deploying-to-kubernetes-using-github-actions-argocd-cd5c47c92e63)
- [Build and Push Docker to GHCR (GitHub Actions)](https://github.com/marketplace/actions/build-docker-image-and-push-to-ghcr-docker-hub-or-aws-ecr)
- [Ingress NGINX Retirement (Kubernetes.io)](https://kubernetes.io/blog/2025/11/11/ingress-nginx-retirement/)
- [Ingress NGINX EOL March 2026](https://www.chkk.io/blog/ingress-nginx-deprecation)
- [MassTransit RabbitMQ Configuration](https://masstransit.io/documentation/transports/rabbitmq)
- [RabbitMQ Cluster Kubernetes Operator](https://github.com/rabbitmq/cluster-operator)
- [kind - Local Kubernetes](https://kind.sigs.k8s.io/)
- [.NET Container Best Practices 2025](https://developersvoice.com/blog/cloud/dotnet-containers-aot-sbom/)
- [Kustomize Tutorial (DevOpsCube)](https://devopscube.com/kustomize-tutorial/)

---
*Feature research for: Kubernetes & GitOps deployment — MicroCommerce v3.0*
*Researched: 2026-02-25*

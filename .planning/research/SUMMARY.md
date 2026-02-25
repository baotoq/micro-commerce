# Project Research Summary

**Project:** MicroCommerce v3.0 — Kubernetes & GitOps Deployment
**Domain:** K8s deployment with GitOps for .NET 10 microservices e-commerce platform
**Researched:** 2026-02-25
**Confidence:** HIGH

## Executive Summary

MicroCommerce v3.0 is a Kubernetes and GitOps deployment milestone layered on top of a fully implemented .NET 10 modular monolith with 182 automated tests, complete DDD building blocks, and .NET Aspire local dev orchestration. The goal is not to change the application but to containerize and deploy it to a kind-based Kubernetes cluster using ArgoCD for GitOps continuous delivery, Kustomize for manifest management, Sealed Secrets for secret safety, and RabbitMQ as the K8s-native messaging transport (replacing the Azure Service Bus emulator which does not run in K8s). The recommended approach is to build in strict dependency order: Dockerfiles first (images are the root dependency for all K8s resources), then infrastructure manifests (PostgreSQL, RabbitMQ, Keycloak), then application manifests, then the CI/CD pipeline, then ArgoCD GitOps wiring, and finally observability.

The key architectural decision is that Aspire and Kubernetes are parallel deployment paths — not competing. Aspire remains the local inner-loop dev tool unchanged; Kustomize overlays and ArgoCD define the cluster state. The primary application code change is a configurable MassTransit transport switch (`MASSTRANSIT_TRANSPORT` env var selecting between Azure Service Bus for Aspire dev and RabbitMQ for K8s). Minor changes are also required in ServiceDefaults (health endpoints must not be gated on `IsDevelopment()`), Gateway appsettings (YARP destinations must use K8s DNS not Aspire service discovery notation), and `next.config.ts` (must add `output: 'standalone'` before the Next.js Dockerfile will produce a correct image).

The principal risks are operational rather than architectural. The three highest-impact pitfalls are: (1) Aspire service discovery env vars being absent in K8s causing YARP to route to localhost, (2) the MassTransit transport swap silently breaking the DLQ feature and potentially the checkout saga if the Azure Service Bus-specific code paths are not removed, and (3) EF Core migration init containers racing with PostgreSQL startup causing `CrashLoopBackOff` on cold cluster start. All three are preventable with well-known mitigations documented in PITFALLS.md. Sealed Secrets key management is the only long-term operational risk — a lost master key requires re-sealing every secret — and must be addressed with a key backup procedure in the cluster bootstrap script.

## Key Findings

### Recommended Stack

The new stack additions for v3.0 are all well-established, version-pinned, and verified against official sources as of 2026-02-25. For container images: `.NET 10 aspnet` runtime (mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled) and `node:22-alpine` for Next.js. Container registry: `ghcr.io` (free, GitHub-token auth, no rate limits). Local Kubernetes: `kind v0.31.0` (K8s 1.35.0, Docker-based, CI-compatible). Manifest management: `Kustomize v5.8.1` for internal services, Helm only for third-party infra. GitOps: `ArgoCD v3.3.2` (v3.3.2 specifically — v3.3.0 and v3.3.1 have a client-side apply migration bug). Secrets: `Sealed Secrets v0.35.0`. Message broker: `RabbitMQ Cluster Operator v2.19.1` with `MassTransit.RabbitMQ 9.0.1`. Observability: `OTEL Collector contrib v0.143.0` feeding the standalone Aspire Dashboard container.

**Core technologies:**
- kind v0.31.0: local K8s cluster — Docker-based, no VM overhead, CI-compatible
- ArgoCD v3.3.2: GitOps CD controller — app-of-apps pattern, declarative, Git-native; use v3.3.2 specifically (fixes apply migration issue in v3.3.0/3.3.1)
- Kustomize v5.8.1: K8s manifest management — built into kubectl, plain YAML output, no templating language required
- Sealed Secrets v0.35.0: GitOps-safe secrets — one-way encryption, safe to commit to Git
- MassTransit.RabbitMQ 9.0.1: K8s messaging transport — drop-in replacement for Azure SB, same consumer/saga/outbox config unchanged
- OTEL Collector contrib v0.143.0: telemetry pipeline — receives OTLP from apps, fans out to Aspire Dashboard; use v0.142.0 if OCB/OpAMPSupervisor tooling is needed (broken in v0.143.0)
- ghcr.io: image registry — free for public repos, `GITHUB_TOKEN` auth, no Docker Hub rate limits
- docker/build-push-action v6.19.2: CI image builds — BuildKit, GHA layer cache support

### Expected Features

**Must have (P1 — required for a working K8s demo):**
- Dockerfiles for ApiService, Gateway, and Web (Next.js) — images are the root dependency for all K8s resources
- Kustomize base manifests for all 3 services (Deployment + Service + ConfigMap)
- PostgreSQL StatefulSet + PVC + Service — database must persist in cluster
- RabbitMQ Deployment + Service + ConfigMap — messaging must run in cluster
- Keycloak Deployment + Service + Realm ConfigMap — auth must run in cluster
- Liveness and readiness probes on all 3 app services — K8s table stake
- Resource requests and limits on all containers — without these, kind cluster scheduler misbehaves
- GitHub Actions workflow: build and push 3 images to ghcr.io on push to master
- imagePullSecret in cluster for ghcr.io
- MassTransit RabbitMQ transport support configurable via env var
- OTEL Collector + standalone Aspire Dashboard — observability must work in cluster
- ArgoCD Application manifests with app-of-apps root — GitOps deployment working end-to-end

**Should have (P2 — required for a credible GitOps demo):**
- Sealed Secrets replacing plain K8s Secrets — GitOps-safe secret management
- Namespace isolation (`micro-commerce` namespace on all resources)
- Startup probes for Keycloak and ApiService (slow startup protection)
- Image tag update automation (CI commits SHA back to overlay, ArgoCD auto-syncs)
- Kustomize dev overlay for kind cluster (image repo/tag overrides)

**Defer (P3 — post-v3.0):**
- Horizontal Pod Autoscaler — meaningful only under real load; document as next step
- Network Policies — requires non-default CNI in kind; production hardening
- cert-manager + TLS — production hardening only
- Service Mesh (Istio/Linkerd) — YARP already handles auth/routing; massive overhead for no showcase gain
- PostgreSQL Operator (CloudNativePG) — HA overkill for showcase
- Multi-environment overlays (staging, prod) — only dev cluster exists
- MinIO for Blob Storage — low demo value; disable or use placeholder images in K8s

### Architecture Approach

The architecture separates concerns cleanly across two layers: CI (GitHub Actions builds and pushes images, then commits updated image tags to the Kustomize overlay) and CD (ArgoCD detects the Git change and syncs the cluster). Application services (ApiService, Gateway, Web) are standard K8s Deployments with ClusterIP Services; stateful dependencies (PostgreSQL, RabbitMQ, Keycloak) are StatefulSets with PersistentVolumeClaims. The YARP Gateway remains the single external entry point (via NodePort or Ingress), and the internal service-to-service communication switches from Aspire's env-var-based service discovery to standard K8s DNS (`http://service-name.namespace.svc.cluster.local:port`). The key configuration delta between local Aspire and K8s is captured entirely in Kustomize overlays — no application source changes beyond the transport switch and health endpoint gate.

**Major components:**
1. GitHub Actions CI — tests, builds 3 container images, pushes to ghcr.io, commits SHA tag back to deploy/overlays/dev/
2. Git repository (deploy/ directory) — single source of truth for all K8s manifests; ArgoCD syncs from this
3. ArgoCD (app-of-apps) — root Application watches deploy/argocd/apps/, each child Application independently syncs one service
4. kind cluster — local K8s environment; 1 control-plane + 2 worker nodes for realistic scheduling
5. Kustomize base/overlays — base/ has environment-neutral manifests; overlays/dev/ patches image tags, resource limits, and contains SealedSecrets
6. Sealed Secrets controller — decrypts SealedSecret CRDs committed to Git into live K8s Secrets
7. OTEL Collector — receives OTLP gRPC from all apps, enriches with K8s metadata, fans out to Aspire Dashboard
8. Aspire Dashboard (standalone) — developer observability UI for traces, metrics, and logs in K8s (in-memory; clears on pod restart)

**Recommended project structure:**
```
deploy/
  base/                     # Environment-neutral manifests
    apiservice/, gateway/, web/, postgres/, rabbitmq/, keycloak/, monitoring/
  overlays/
    dev/                    # kind cluster overrides: image tags, resource limits
      patches/
      sealed-secrets/       # SealedSecret YAMLs safe to commit
  argocd/
    apps/                   # Child Application manifests (one per service)
    root-app.yaml           # Bootstrap: points ArgoCD at apps/ directory
src/
  MicroCommerce.ApiService/Dockerfile
  MicroCommerce.Gateway/Dockerfile
  MicroCommerce.Web/Dockerfile
.github/workflows/
  dotnet-test.yml           # Existing
  docker-build.yml          # New: build + push to ghcr.io
  update-manifests.yml      # New: commit SHA tags to overlay
```

### Critical Pitfalls

1. **Aspire service discovery env vars absent in K8s** — YARP routes to localhost causing immediate connection failures. Explicitly set `services__apiservice__http__0=http://apiservice-svc:8080` in Gateway's K8s Deployment env vars via Kustomize overlay. Verify with `kubectl exec` after deployment.

2. **MassTransit transport swap breaking DLQ and saga** — `AddAzureServiceBusClient("messaging")` throws at startup; `IServiceBusReceiveEndpointConfigurator` DLQ cast silently no-ops on RabbitMQ. Remove the Azure SB client registration and transport-specific DLQ block for K8s mode; use `MASSTRANSIT_TRANSPORT` env var to switch transports. Run a full checkout flow including intentional payment failure to verify saga compensates correctly with RabbitMQ.

3. **EF Core migration init container race condition** — 8 DbContexts run migrations at startup; if PostgreSQL is still initializing, init containers crash-loop. Use a single init container with a `pg_isready` wait loop running all 8 migrations sequentially, or call `MigrateAsync()` at startup with a PostgreSQL advisory lock.

4. **Next.js missing `output: 'standalone'`** — Without it, the Docker image is 800MB+ and `SIGTERM` is not forwarded, breaking rolling updates. Add `output: 'standalone'` to `next.config.ts` and use `node server.js` (not `npm start`) as the container entrypoint. Explicitly copy `.next/static/` and `public/` which are not included in standalone output by default.

5. **Sealed Secrets master key lost on cluster teardown** — kind clusters are routinely destroyed. Export the sealing key immediately after bootstrapping and store outside Git. Automate key restoration in the cluster bootstrap script; never rely on a manual step under time pressure.

## Implications for Roadmap

Based on the dependency graph in FEATURES.md and the build order in ARCHITECTURE.md, 6 phases are suggested:

### Phase 1: Dockerfiles and Container Image Pipeline
**Rationale:** Container images are the root dependency for all K8s resources. Nothing else can proceed without them. This phase also captures the two highest-impact code changes to the existing application (`output: 'standalone'` in next.config.ts, `ASPNETCORE_URLS` and non-root user in .NET Dockerfiles).
**Delivers:** Three production-ready multi-stage container images (ApiService, Gateway, Web). GitHub Actions workflow builds and pushes to ghcr.io on every master commit using BuildKit layer caching.
**Addresses:** Dockerfiles for all 3 services, GitHub Actions image pipeline, ghcr.io registry, imagePullSecret.
**Avoids:** Dockerfile layer caching pitfall (copy .csproj first, restore, then copy source); Next.js standalone output pitfall; `npm start` vs `node server.js` pitfall.

### Phase 2: Infrastructure Manifests and Secrets Strategy
**Rationale:** Application services cannot start without their dependencies (PostgreSQL, RabbitMQ, Keycloak). Secrets strategy must be established before any credentials are written to files. The EF Core migration init container approach is decided here.
**Delivers:** PostgreSQL StatefulSet + PVC, RabbitMQ StatefulSet + PVC, Keycloak StatefulSet + realm ConfigMap. Sealed Secrets controller bootstrapped; all credentials sealed and committed to Git.
**Addresses:** PostgreSQL in K8s, RabbitMQ in K8s, Keycloak in K8s + realm import, secrets management.
**Avoids:** EF Core migration race condition (init container with pg_isready loop); Keycloak realm one-time-import pitfall (upsert via Admin API instead of operator Job); plain secrets in Git (Sealed Secrets from day one); Sealed Secrets key loss (automated backup in bootstrap script).

### Phase 3: Application Manifests and MassTransit Transport Switch
**Rationale:** With infrastructure running and images available, application service manifests can be written. The MassTransit transport switch is the most complex application code change and belongs in its own phase to allow complete verification before GitOps is wired.
**Delivers:** ApiService, Gateway, and Web Deployments + Services + ConfigMaps with correct K8s DNS service discovery. Configurable MassTransit transport (`MASSTRANSIT_TRANSPORT` env var). Health endpoints exposed unconditionally. YARP routing via K8s DNS.
**Addresses:** Kustomize base manifests, liveness/readiness probes, resource limits, service discovery transition, MassTransit transport abstraction.
**Avoids:** Aspire service discovery conflict (explicit K8s DNS in YARP config); MassTransit transport swap breaking DLQ/saga (full checkout flow verification before proceeding); health checks gated on IsDevelopment.

### Phase 4: Kustomize Overlays and ArgoCD GitOps
**Rationale:** Once base manifests work with infrastructure running, the Kustomize dev overlay patches them for the kind cluster, and ArgoCD is configured to manage the cluster state from Git. This phase closes the GitOps management loop.
**Delivers:** Kustomize dev overlay with image tag patches and resource limit adjustments. ArgoCD app-of-apps root application and per-service child applications. ArgoCD syncs cluster state from Git.
**Addresses:** Kustomize dev overlay, ArgoCD app-of-apps, namespace isolation, ArgoCD Application manifests.
**Avoids:** Kustomize strategic merge patch targeting wrong container name (establish naming convention, prefer JSON 6902 patches); ArgoCD sync loop from non-idempotent output (ServerSideApply enabled, no dynamic annotations, immutable image tags); single monolithic ArgoCD application anti-pattern.

### Phase 5: CI/CD GitOps Loop Closure
**Rationale:** Once ArgoCD manages the cluster from Git, the CI pipeline is extended to commit image SHA tags back to the overlay, closing the full GitOps loop. This is a pure automation improvement with no impact on cluster functionality.
**Delivers:** GitHub Actions update-manifests.yml workflow commits new image SHA tags to deploy/overlays/dev/ after successful image push. ArgoCD detects the Git change and automatically rolls out new pods.
**Addresses:** Image tag update automation, full GitOps loop (CI build → Git commit → ArgoCD sync → K8s rollout).
**Avoids:** `latest` tag anti-pattern (SHA-based tags prevent perpetual ArgoCD OutOfSync); non-auditable deployments (every deploy is a traceable Git commit).

### Phase 6: Observability — OTEL Collector and Aspire Dashboard
**Rationale:** Observability is deferred until services are running and the GitOps loop is closed. Adding it last makes debugging OTEL configuration much easier — there is a running application to send telemetry from.
**Delivers:** OTEL Collector Deployment + Service + ConfigMap with all three signal pipelines (traces, metrics, logs). Standalone Aspire Dashboard Deployment + Service. All app Deployments updated with `OTEL_EXPORTER_OTLP_ENDPOINT` env var pointing to Collector ClusterIP service.
**Addresses:** OTEL Collector in K8s, standalone Aspire Dashboard, observability without Aspire AppHost.
**Avoids:** Silent OTEL data loss (explicit pipeline config for all three signals, `memory_limiter` processor mandatory at 400MiB); OTEL Collector OOM under burst checkout saga load; Collector pointing to localhost instead of Dashboard K8s service DNS.

### Phase Ordering Rationale

- Dockerfiles first because images are the absolute root dependency — no K8s resource can reference a non-existent image, and the Next.js standalone config change must land before any Dockerfile is written.
- Infrastructure before applications because ApiService cannot start without PostgreSQL and RabbitMQ; Keycloak must be running for JWT validation; Sealed Secrets strategy must exist before any credential is encoded.
- Application manifests before GitOps wiring because Kustomize overlays patch base manifests that must exist and have been verified working first.
- ArgoCD before the full CI/CD loop because ArgoCD must be installed and the app-of-apps structure must work before CI can commit back to Git and expect a sync.
- Observability last because it has no blocking dependencies and a running, stable cluster is the best environment for debugging OTEL pipelines.

### Research Flags

Phases likely needing deeper research during planning:
- **Phase 2 (Keycloak realm import):** The operator's `KeycloakRealmImport` CR runs exactly once (IGNORE_EXISTING strategy). The upsert-via-Admin-API approach needs a tested script for this project's specific realm structure and ArgoCD sync lifecycle. Research the Keycloak Admin REST API realm PUT behavior for existing realms with active sessions.
- **Phase 3 (MassTransit DLQ for RabbitMQ):** The existing `DeadLetterQueueService` uses `ServiceBusClient` (Azure SDK). A RabbitMQ-compatible implementation using the RabbitMQ Management API or a graceful disable path needs design. This is the most uncertain code change in the milestone.
- **Phase 5 (Image tag update strategy):** Two viable approaches — CI commits back to repo (needs write token, creates noise commits) vs. ArgoCD Image Updater (additional controller). For a showcase the commit-back approach is simpler but the implementation has several edge cases (race conditions when multiple services build simultaneously, GPG signing requirements on the bot commit).

Phases with standard, well-documented patterns (safe to skip research-phase):
- **Phase 1 (Dockerfiles):** Multi-stage .NET and Next.js Dockerfiles are thoroughly documented; exact patterns with layer caching strategy are provided in STACK.md and ARCHITECTURE.md.
- **Phase 4 (Kustomize + ArgoCD):** App-of-apps is an official ArgoCD pattern with comprehensive documentation and the exact YAML structure is provided in ARCHITECTURE.md.
- **Phase 6 (OTEL Collector):** OTEL Collector configuration for K8s is well-documented; the exact ConfigMap with all three signal pipelines and memory_limiter is provided in STACK.md and PITFALLS.md.

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | All versions verified against official GitHub releases and NuGet as of 2026-02-25. One caveat: OTEL Collector v0.143.0 has a known OCB/OpAMPSupervisor artifact issue — use v0.142.0 if those tools are needed. ArgoCD v3.3.2 specifically required (v3.3.0/v3.3.1 have a client-side apply bug). |
| Features | HIGH | Feature set is grounded in K8s deployment fundamentals. P1/P2/P3 prioritization is well-reasoned and matches community consensus for a showcase-grade deployment. Anti-feature decisions (no service mesh, no HPA, no PostgreSQL operator) are clearly justified. |
| Architecture | HIGH | All patterns (app-of-apps, Sealed Secrets workflow, multi-stage Dockerfiles, Kustomize base/overlay structure, service discovery transition table) are from official documentation. The Aspire + K8s parallel deployment model is confirmed by Microsoft docs. |
| Pitfalls | HIGH | All 10 critical pitfalls are sourced from official Aspire GitHub issues, MassTransit GitHub discussions, Kubernetes Blog, and community production experience. The Keycloak realm import limitation is documented operator behavior. Each pitfall has concrete warning signs and recovery steps. |

**Overall confidence:** HIGH

### Gaps to Address

- **Azurite / Azure Blob Storage in K8s:** `next.config.ts` `remotePatterns` hardcodes `127.0.0.1:10000` (Azurite). The decision to disable blob features in K8s (placeholder images) vs. add a MinIO StatefulSet has not been made. Resolve during Phase 3 planning — if placeholder images are used, the image optimization config must still be updated to avoid Next.js returning 400 on image requests from a different hostname.
- **Ingress vs NodePort for external access:** FEATURES.md notes Ingress NGINX is EOL March 2026. ARCHITECTURE.md shows NGINX Ingress in the diagram but for kind local access NodePort on the Gateway service is the simplest path. For more polished routing Traefik or Envoy Gateway are the alternatives. Resolve at the start of Phase 3 — the choice affects the Gateway Service manifest type.
- **MassTransit v9 commercial license scope:** MassTransit v9 transitioned to commercial licensing; v8 remains Apache 2.0 through end of 2026. The project already uses v9. For a public showcase project, confirm whether the license terms permit open-source use before Phase 3. If not, evaluate pinning `MassTransit.RabbitMQ` at 8.x.
- **CORS allowed origins in YARP Gateway:** PITFALLS.md flags that CORS is currently `*` in the YARP Gateway — acknowledged tech debt that must be addressed before K8s deployment. An explicit allowed-origins list is needed in the Kustomize overlay ConfigMap for the Gateway. The correct origin value (the K8s-exposed hostname for the Web service) depends on the Ingress/NodePort decision above.

## Sources

### Primary (HIGH confidence)
- [kind v0.31.0 Release](https://github.com/kubernetes-sigs/kind/releases) — version verification, K8s default, compatibility matrix
- [ArgoCD v3.3.2 Release](https://github.com/argoproj/argo-cd/releases) — latest stable, apply migration bug fix
- [Kustomize v5.8.1 Release](https://github.com/kubernetes-sigs/kustomize/releases) — namespace propagation regression fix
- [Sealed Secrets v0.35.0](https://github.com/bitnami-labs/sealed-secrets/releases) — version, workflow documentation
- [MassTransit.RabbitMQ 9.0.1](https://www.nuget.org/packages/MassTransit.RabbitMQ/) — NuGet latest stable, Feb 7, 2026
- [RabbitMQ Cluster Operator v2.19.1](https://github.com/rabbitmq/cluster-operator/releases) — operator version, Feb 6, 2026
- [ArgoCD cluster bootstrapping / app-of-apps](https://argo-cd.readthedocs.io/en/stable/operator-manual/cluster-bootstrapping/) — official pattern
- [Kustomize documentation](https://kubernetes.io/docs/tasks/manage-kubernetes-objects/kustomization/) — overlay structure
- [MassTransit RabbitMQ quick start](https://masstransit.io/quick-starts/rabbitmq) — transport configuration
- [Next.js standalone output docs](https://nextjs.org/docs/app/getting-started/deploying) — output mode, static asset handling
- [.NET container docs](https://learn.microsoft.com/en-us/dotnet/core/docker/build-container) — multi-stage Dockerfile patterns
- [Aspire Dashboard standalone](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard/standalone) — ports, environment config
- [OpenTelemetry Collector on Kubernetes](https://opentelemetry.io/docs/platforms/kubernetes/collector/) — pipeline configuration

### Secondary (MEDIUM confidence)
- [dotnet/aspire GitHub Issue #3698](https://github.com/dotnet/aspire/issues/3698) — service discovery env var conflict with K8s
- [dotnet/aspire GitHub Issue #5096](https://github.com/dotnet/aspire/issues/5096) — env var priority in service discovery
- [Kustomize vs Helm 2026 analysis](https://tasrieit.com/blog/helm-vs-kustomize-kubernetes-comparison-2026) — hybrid Kustomize+Helm recommendation
- [CNCF Blog — ArgoCD app-of-apps 2025](https://www.cncf.io/blog/2025/10/07/managing-kubernetes-workloads-using-the-app-of-apps-pattern-in-argocd-2/)
- [Sealed Secrets key backup](https://ismailyenigul.medium.com/take-backup-of-all-sealed-secrets-keys-or-re-encrypt-regularly-297367b3443)
- [Keycloak realm import in K8s](https://rahulroyz.medium.com/update-keycloak-realm-configurations-using-import-feature-on-kubernetes-platform-b1b0ed85f7f7)
- [Kubernetes Blog — Common Pitfalls 2025](https://kubernetes.io/blog/2025/10/20/seven-kubernetes-pitfalls-and-how-to-avoid/)
- [Atlas — Schema migrations in K8s with init containers](https://atlasgo.io/guides/deploying/k8s-init-container)
- [Milan Jovanovic — Using MassTransit with RabbitMQ and Azure Service Bus](https://www.milanjovanovic.tech/blog/using-masstransit-with-rabbitmq-and-azure-service-bus)

### Tertiary (LOW confidence, validate at implementation time)
- [Ingress NGINX EOL March 2026](https://www.chkk.io/blog/ingress-nginx-deprecation) — replacement guidance; confirm current status before Phase 3
- [MassTransit v9 announcement](https://masstransit.io/introduction/v9-announcement) — commercial license terms; confirm applicability for public showcase use case

---
*Research completed: 2026-02-25*
*Ready for roadmap: yes*

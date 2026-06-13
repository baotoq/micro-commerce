# ArgoCD GitOps Deployment — Design Spec

**Date:** 2026-06-13
**Status:** Approved design, pending implementation plan
**Research backing:** [`docs/superpowers/research/2026-06-13-argocd-gitops-research.md`](../research/2026-06-13-argocd-gitops-research.md) (7-agent doc/web verification; cited inline below)

## 1. Goal

Stand up **full GitOps** for the Micro Commerce stack: containerize the services, author Kubernetes manifests for the whole topology, and have **ArgoCD** continuously reconcile a local **OrbStack** Kubernetes cluster from this git repo. A push to `master` builds images, pushes them to GHCR, and writes the new tags back to git; ArgoCD detects the commit and syncs the cluster.

## 2. Locked decisions

| Decision | Choice |
|---|---|
| Target cluster | Local **OrbStack** Kubernetes (single node) |
| Dapr | **Included fully** — control plane via Helm + sidecar injection + Redis pubsub Component |
| Images | **CI builds + pushes to GHCR**; manifests reference immutable SHA tags; CI writes tag bumps back to git |
| Manifest tooling | **Kustomize** for our workloads + supporting infra; **upstream Helm** (via ArgoCD Applications) for Dapr & ArgoCD itself |
| ArgoCD structure | **App-of-Apps** with child-Application sync waves |

## 3. Corrections the research forced into this design

These overturned naive assumptions; they are load-bearing:

1. **`/health` + `/alive` are gated behind `IsDevelopment()`** in `Api/Extensions.cs` (`MapDefaultEndpoints`). They 404 in Production → k8s probes fail → pods never Ready. **Fix:** run the Catalog API with `ASPNETCORE_ENVIRONMENT=Development` on the local cluster. This single setting also enables seeding (`SEED_PRODUCTS`), the Azurite CORS bootstrap (`Program.cs` 52–66), and OpenAPI — all desirable locally.
2. **OrbStack `*.k8s.orb.local` resolves inside pods but UNRELIABLY** (open issue #2306, `EAI_AGAIN`). Do not rely on it for pod→pod calls (API→Keycloak JWKS). **Fix:** a CoreDNS rewrite maps the external hostnames to in-cluster Service FQDNs.
3. **Keycloak `start-dev` defaults `hostname-strict=false`** → dynamic issuer per request → `iss` mismatch between browser and pods. **Fix:** explicitly pin `KC_HOSTNAME` to the one external URL.
4. **Keycloak `--import-realm` is import-once** (skipped if the realm already exists in the DB). **Fix:** run Keycloak with an **ephemeral DB** (no PVC) so every boot re-imports — keeps git the source of truth, drift-free.
5. **The realm's `redirectUris` is `http://localhost:3000/*`** — won't match the k8s web host → "Invalid redirect_uri". **Fix:** add the k8s web origin/redirect URI to the realm JSON.
6. **`next.config.ts` lacks `output: 'standalone'`** and its `images.remotePatterns` doesn't include the Azurite k8s host. Both are **required code changes**.
7. **Catalog API output assembly is `MicroCommerce.Catalog.dll`** (csproj is `MicroCommerce.Catalog.csproj`), *not* `...Catalog.Api.dll`.
8. **Azurite needs `--disableProductStyleUrl`** (path-style host) + `--blobHost 0.0.0.0` or uploads 400.
9. **Sync-wave annotations don't cross Application boundaries** — they order child Application CRs in the root app and rely on health-gating, not resources inside sibling apps.
10. **Dapr is not 1.15** — pin a current GA (~1.16+); verify with `helm search repo dapr --versions` at apply time.

## 4. Architecture

```
bootstrap (imperative, ONE-TIME — ArgoCD cannot deploy itself from nothing)
  orb start k8s
  helm install argocd argo/argo-cd -n argocd --create-namespace --version 9.5.x
  (optional) create GHCR pull secret   [skip if packages made public]
  kubectl apply -f deploy/bootstrap/root-app.yaml
        │
        ▼
root Application  ──watches──▶  deploy/argocd/apps/*.yaml   (child Application CRs)
        ├─ wave -2:  dapr      (Helm dapr/dapr → dapr-system; ServerSideApply; ha off; prune only)
        ├─ wave  0:  infra     (Kustomize: postgres, redis, keycloak, azurite,
        │                        coredns-custom rewrite, dev secrets)
        ├─ wave  1:  components (Kustomize: Dapr pubsub.redis Component — needs Dapr CRDs + Redis)
        └─ wave  2:  app        (Kustomize: catalog-api [+Dapr sidecar], web)
```

**Waves order *when each child Application is created*; the next wave waits until the current wave's apps report Healthy.** Waves give ordering, **not readiness** — readiness probes + a Postgres-wait init handle DB readiness for the auto-migrating API.

## 5. Repo layout (all new, in-repo)

```
deploy/
  README.md                      # bootstrap + operate
  bootstrap/
    install-argocd.sh            # orb start k8s; helm install argocd; [pull secret]; apply root-app
    root-app.yaml                # App-of-Apps root Application (finalizer for cascade delete)
  argocd/apps/                   # child Application CRs (one per wave-group)
    dapr.yaml                    # Helm dapr/dapr      (sync-wave -2, prune only, no selfHeal)
    infra.yaml                   # Kustomize deploy/k8s/infra      (sync-wave 0)
    dapr-components.yaml         # Kustomize deploy/k8s/dapr-components (sync-wave 1)
    app.yaml                     # Kustomize deploy/k8s/app/overlays/local (sync-wave 2)
  k8s/
    infra/                       # namespace, postgres (StatefulSet+PVC), redis,
                                 #   keycloak (Deployment + realm ConfigMap, ephemeral),
                                 #   azurite (Deployment + PVC), coredns-custom, dev Secrets
    dapr-components/             # pubsub.redis Component
    app/
      base/                      # catalog-api (+Dapr annotations), web, Services, config
      overlays/local/            # image tags pinned here; CI writes bumps back

src/Services/Catalog.API/src/Dockerfile   (+ .dockerignore)   # context = .../Catalog.API/src
src/web/Dockerfile                        (+ .dockerignore)
```

## 6. Containerization

### Catalog API — `src/Services/Catalog.API/src/Dockerfile`
- Build context = `src/Services/Catalog.API/src` (so all four csproj — Api/Application/Infrastructure/Domain — are reachable).
- Multi-stage: `mcr.microsoft.com/dotnet/sdk:10.0` → copy the 4 csproj first, `dotnet restore`, copy rest, `dotnet publish -c Release -o /app --no-restore /p:UseAppHost=false`.
- Runtime: `mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled` (non-root `app`/1654 by default, no shell, port 8080 baked in).
- `ENTRYPOINT ["dotnet", "MicroCommerce.Catalog.dll"]`. `LABEL org.opencontainers.image.source=...` to auto-link the GHCR package.
- No `curl` HEALTHCHECK (chiseled has no shell) — k8s `httpGet` probes only.
- No `latest` tag exists for .NET images; pin `10.0`. OTLP exporter is unregistered when `OTEL_EXPORTER_OTLP_ENDPOINT` is unset → no crash.

### Web — `src/web/Dockerfile`
- Based on the current `vercel/next.js` `with-docker` example: 3 stages (`deps`/`builder`/`runner`), built-in `node` user, `node:24-slim`.
- COPY `.next/standalone`, then `.next/static`, then `public` separately. `ENV PORT=3000 HOSTNAME=0.0.0.0`, `CMD ["node","server.js"]`.
- **No** `AUTH_*` / `API_URL` build args — they're server-only runtime env (zero `NEXT_PUBLIC_` vars → build-once-run-anywhere).

## 7. Required application code changes

| File | Change | Why |
|---|---|---|
| `src/web/next.config.ts` | add `output: 'standalone'` | no standalone output → Docker build has nothing to copy |
| `src/web/next.config.ts` | add Azurite k8s host to `images.remotePatterns` | else `next/image` 400s on photo URLs from `azurite.…k8s.orb.local` |
| `src/AppHost/Realms/microcommerce-realm.json` | add k8s web origin to `redirectUris` + `webOrigins` (keep `localhost:3000` for Aspire dev) | OAuth callback fails on the k8s host otherwise |

No wiring code changes: the API/web read the same `ConnectionStrings__*` / `AUTH_*` / `Keycloak__Authority` env var names Aspire uses; k8s supplies them directly.

## 8. In-cluster components

All in namespace `micro-commerce` (Dapr control plane in `dapr-system`). PVCs omit `storageClassName` → bind to OrbStack's default `local-path`.

| Component | Shape | Key config |
|---|---|---|
| **Catalog API** | Deployment + Service (ClusterIP) | Dapr annotations (§9); `ASPNETCORE_ENVIRONMENT=Development`, `SEED_PRODUCTS=true`; `httpGet /health` (readiness) + `/alive` (liveness); initContainer waits for Postgres; **single replica** (boot-time `MigrateAsync` races at >1) |
| **web** | Deployment + Service type **LoadBalancer** | `API_URL=http://catalog-api.micro-commerce.svc.cluster.local:8080` (in-cluster DNS); `AUTH_*` from Secret; external host `web.micro-commerce.k8s.orb.local` |
| **Postgres** | StatefulSet + PVC | `catalogdb`; password from dev Secret |
| **Redis** | Deployment + Service | DB 0 = output cache; DB 1 = Dapr pubsub (node mode) |
| **Keycloak** | Deployment + Service type LoadBalancer + realm ConfigMap | **ephemeral DB** (re-import each boot); `KC_HOSTNAME`, `KC_HTTP_ENABLED=true`, `KC_BOOTSTRAP_ADMIN_USERNAME/PASSWORD`; readiness on `/realms/microcommerce/.well-known/openid-configuration` |
| **Azurite** | Deployment + PVC + Service type LoadBalancer | args `--blobHost 0.0.0.0 --disableProductStyleUrl --location /data` |
| **Dapr** | Helm control plane + `pubsub.redis` Component | `global.ha.enabled=false`; Component `redisDB:"1"`, `consumerID:"catalog-api"`, `type:node` |

## 9. Networking — single-URL alignment

**One hostname per externally-reachable service, identical inside and outside the cluster.** Services are type `LoadBalancer` → OrbStack auto-assigns `<svc>.<ns>.k8s.orb.local` (exact format verified on the live cluster during impl). For **pod-side** resolution of those same names, an ArgoCD-managed **`coredns-custom` ConfigMap** (K3s-honored) rewrites them to in-cluster Service FQDNs — robust, no hardcoded ClusterIP, survives redeploys. Re-verify after `orb restart k8s`.

- **Issuer (one string everywhere):** `http://keycloak.micro-commerce.k8s.orb.local/realms/microcommerce` — set as Keycloak `KC_HOSTNAME`, API `Keycloak__Authority`, and web `AUTH_KEYCLOAK_ISSUER`. Browser resolves via OrbStack; pods via the CoreDNS rewrite. `RequireHttpsMetadata=false` already holds in Development.
- **Azurite (one connection string drives SAS host + pod reachability):** `ConnectionStrings__photos` `BlobEndpoint=http://azurite.micro-commerce.k8s.orb.local/devstoreaccount1`. SAS is minted client-side over the canonical resource (host-independent), so the browser-reachable host is signature-valid; the pod reaches the same host for `CreateIfNotExists`.
- **Pod→pod (web→API, Dapr):** use `*.svc.cluster.local` directly — never `*.k8s.orb.local`.

## 10. Secrets

Two tiers, dev-only:
- **Committed, fake** (clearly DEV-ONLY): Postgres password, Keycloak admin, `AUTH_SECRET`, `AUTH_KEYCLOAK_SECRET` — these already live as hardcoded dev values in `AppHost.cs`, so committing them changes nothing. Documented upgrade path: Sealed Secrets / External Secrets for any real cluster.
- **Not committed:** GHCR pull secret. **Default for local: make the GHCR packages public** (no secret needed). If kept private, `install-argocd.sh` creates a per-namespace `dockerconfigjson` from `$GHCR_PAT`.

## 11. CI → GHCR → write-back loop

New workflow (push to `master`, `paths-ignore: ['deploy/k8s/app/overlays/local/**']`):

- **`build` job** (`permissions: contents: read, packages: write`): matrix over catalog-api + web; `docker/login-action` (GHCR, `github.actor` + `GITHUB_TOKEN`) → `metadata-action` (`type=sha,format=long`) → `build-push-action push:true`. Actions pinned by SHA.
- **`bump-gitops` job** (`needs: build`, `permissions: contents: write`, `concurrency` group to serialize): `kustomize edit set image` in `deploy/k8s/app/overlays/local` → commit `"chore(gitops): bump images to sha-<sha> [skip ci]"`. The `GITHUB_TOKEN` push **does not retrigger** workflows (primary loop guard); `[skip ci]` + `paths-ignore` are belt-and-suspenders.
- **`manifests` job** (validation gate — our "tests" for YAML): `kustomize build` every overlay piped to `kubeconform`, plus `helm template` lint for the Dapr app.
- Deployments use immutable SHA tags with `imagePullPolicy: IfNotPresent`. Extend `dependabot.yml` to cover the new workflow's actions.
- *(Alternative noted but not chosen: Argo CD Image Updater — adds a controller + in-cluster git creds; reserve for multi-env fleets.)*

## 12. Bootstrap & sync flow

`deploy/bootstrap/install-argocd.sh` (one-time): `orb start k8s` → `helm install argocd argo/argo-cd` (non-HA) → optional GHCR pull secret → `kubectl apply root-app.yaml` → print admin password + UI port-forward. Thereafter ArgoCD reconciles everything from git via the §4 waves. ArgoCD self-management is **out of scope** (drift-fight risk for little local benefit). `selfHeal` is **off** for the Dapr control-plane app, **on** for infra/components/app.

## 13. Verification (evidence before "done")

**Static (validation-first):** `kustomize build | kubeconform` + `helm template` lint, run locally and as the CI `manifests` gate. Build gate for web: confirm `.next/standalone/server.js` exists after `npm run build`.

**Live smoke after sync:**
- `argocd app list` → all Apps `Synced` + `Healthy`; `kubectl get pods` all `Ready`.
- `curl` Catalog `/health` → 200; products endpoint returns seeded data; `kubectl get components.dapr.io` present; `daprd` logs show pubsub.
- **agent-browser** the web app at its OrbStack URL: load storefront, complete a **Keycloak login round-trip** (proves single-issuer plumbing), exercise a **photo SAS upload** (proves Azurite host + CORS), confirm zero console errors, screenshot to `qa/evidence/`.

## 14. Phased build sequence

1. **Containerize** — both Dockerfiles + `.dockerignore`s; `output: 'standalone'`; `remotePatterns` + realm `redirectUris` edits; build images into OrbStack and run each container by hand to prove boot.
2. **Infra manifests** — namespace, Postgres, Redis, Keycloak (+realm ConfigMap, ephemeral), Azurite, `coredns-custom`, dev Secrets; `kustomize build` validates; `kubectl apply` and confirm healthy + DNS rewrite works in-pod.
3. **App manifests + Dapr** — Dapr Helm install, pubsub Component, Catalog API (+annotations, initContainer, probes) & web; apply end-to-end; verify login + upload + events manually.
4. **ArgoCD** — install, root App-of-Apps, child apps + waves; tear down the manual apply and let ArgoCD reconcile from git.
5. **CI loop** — `build` + `bump-gitops` + `manifests` jobs; trigger a commit; watch ArgoCD auto-sync the new SHA.
6. **Verify & document** — full live smoke + browser round-trip; `deploy/README.md`.

## 15. Open risks (carry into the plan)

- **Dapr version drift** (1.16 vs 1.18-rc across sources) — pin via `helm search repo dapr --versions` at apply time.
- **CoreDNS rewrite durability** — managed Corefile may reconcile on `orb restart k8s`; use `coredns-custom` import and re-verify after restart.
- **Realm import-once vs persistence** — ephemeral Keycloak chosen; restarts re-seed (acceptable locally). Realm JSON must keep the audience + roles mappers and the added k8s redirect URI.
- **Azurite CORS flakiness** (historical #731) — verify a real browser PUT preflight succeeds, don't trust the 202 alone. CORS is set by the app's Development boot path (`Program.cs`).
- **`next/image` allow-list is build-time** — the Azurite host must be in `remotePatterns` *before* the web image is built.
- **EF migrate at boot** — single replica only; Postgres must be Ready first (initContainer + probes, not wave timing).
- **OrbStack PV reclaim quirk** (#1858) — deleted PVs free disk only after `orb restart`; budget periodic cleanup during iteration.

## 16. Out of scope

Multi-environment overlays beyond `local`; cloud cluster targets; ArgoCD self-management; production secret management (documented upgrade path only); HA replicas; Argo CD Image Updater.

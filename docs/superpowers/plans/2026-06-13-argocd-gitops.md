# ArgoCD GitOps Deployment — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Deploy the full Micro Commerce stack to a local OrbStack Kubernetes cluster, continuously reconciled from this git repo by ArgoCD, with CI building images to GHCR and writing tag bumps back to git.

**Architecture:** Kustomize for our workloads + supporting infra; upstream Helm (via ArgoCD Applications) for Dapr and ArgoCD itself. App-of-Apps root Application fans out to child Applications ordered by sync-wave: Dapr control plane → infra → Dapr components → app. GitHub Actions builds/pushes images on push to `master` and commits the new SHA tags into the `local` overlay, which ArgoCD detects and syncs.

**Tech Stack:** Kubernetes (OrbStack/K3s), ArgoCD, Kustomize, Helm, Dapr, Docker (.NET 10 chiseled, Next.js 16 standalone), Postgres, Redis, Keycloak 26, Azurite, GitHub Actions, kubeconform.

**Spec:** `docs/superpowers/specs/2026-06-13-argocd-gitops-design.md`
**Research (cited facts):** `docs/superpowers/research/2026-06-13-argocd-gitops-research.md`

---

## Conventions & constants (used by every task)

| Name | Value |
|---|---|
| App namespace | `micro-commerce` |
| Dapr namespace | `dapr-system` |
| ArgoCD namespace | `argocd` |
| Issuer URL (one string everywhere) | `http://keycloak.micro-commerce.k8s.orb.local/realms/microcommerce` |
| Web external URL | `http://web.micro-commerce.k8s.orb.local` |
| Azurite external URL | `http://azurite.micro-commerce.k8s.orb.local/devstoreaccount1` |
| In-cluster API URL (pod→pod) | `http://catalog-api.micro-commerce.svc.cluster.local:8080` |
| Image names (base, bare) | `catalog-api`, `web` |
| Image refs (overlay + CI) | `ghcr.io/baotoq/micro-commerce/catalog-api`, `.../web` |

**GitHub owner note:** the plan uses `baotoq` as the GHCR owner and `https://github.com/baotoq/micro-commerce.git` as the repo URL. If your GitHub owner/repo differs, replace `baotoq/micro-commerce` consistently in: the overlay `images:` (Task 16), the ArgoCD `repoURL` fields (Tasks 17–19), and the CI `IMAGE_PREFIX` (Task 22). It must equal `${{ github.repository }}`.

**OrbStack domain format — verify once (Task 6, Step 0):** the spec assumes LoadBalancer services resolve as `<svc>.<ns>.k8s.orb.local`. If the live cluster uses a different format (e.g. `<svc>.k8s.orb.local`), update the three host constants above (issuer/web/azurite) everywhere they appear. This is the only environment-derived unknown.

**TDD adaptation for declarative infra:** there is no business logic to unit-test here (CLAUDE.md: don't add maintenance/tax tests). The enforced gates are: `kustomize build | kubeconform` (schema-valid manifests), `docker build` + container boot (images), and live `kubectl rollout status` + HTTP smoke (behavior). Each task writes the gate, watches it fail, makes it pass.

---

## File structure

**App code changes (3 files):**
- Modify `src/web/next.config.ts` — add `output: 'standalone'` + Azurite host in `images.remotePatterns`
- Modify `src/AppHost/Realms/microcommerce-realm.json` — add k8s web origin to `redirectUris` + `webOrigins`
- (copy) `deploy/k8s/infra/keycloak/microcommerce-realm.json` — k8s ConfigMap source, kept in sync with the AppHost realm

**Dockerfiles (4 files):**
- `src/Services/Catalog.API/src/Dockerfile`, `src/Services/Catalog.API/src/.dockerignore`
- `src/web/Dockerfile`, `src/web/.dockerignore`

**Infra manifests — `deploy/k8s/infra/`:** `kustomization.yaml`, `namespace.yaml`, `coredns-custom.yaml`, `postgres/{statefulset,service,secret}.yaml`, `redis/{deployment,service}.yaml`, `keycloak/{deployment,service,admin-secret}.yaml` + realm json, `azurite/{deployment,service,pvc}.yaml`

**Dapr components — `deploy/k8s/dapr-components/`:** `kustomization.yaml`, `pubsub.yaml`

**App manifests — `deploy/k8s/app/`:** `base/kustomization.yaml`, `base/catalog-api/{deployment,service,config,secret}.yaml`, `base/web/{deployment,service,config,secret}.yaml`, `overlays/local/kustomization.yaml`

**ArgoCD — `deploy/argocd/apps/`:** `dapr.yaml`, `infra.yaml`, `dapr-components.yaml`, `app.yaml`; **`deploy/bootstrap/`:** `root-app.yaml`, `install-argocd.sh`; **`deploy/README.md`**

**CI — `.github/workflows/gitops.yml`** (new; `ci.yml` unchanged)

---

# Phase 1 — Containerization

### Task 1: App code changes for containerization

**Files:**
- Modify: `src/web/next.config.ts`
- Modify: `src/AppHost/Realms/microcommerce-realm.json`

- [ ] **Step 1: Add `output: 'standalone'` and the Azurite host to `next.config.ts`**

In `src/web/next.config.ts`, add `output: "standalone",` as the first property of `nextConfig`, and add the Azurite host to `images.remotePatterns`:

```ts
const nextConfig: NextConfig = {
  output: "standalone",
  // ...existing reactCompiler, cacheComponents, allowedDevOrigins...
  images: {
    remotePatterns: [
      { protocol: "https", hostname: "placehold.co" },
      { protocol: "https", hostname: "**.blob.core.windows.net" },
      { protocol: "http", hostname: "127.0.0.1", port: "10000" },
      { protocol: "http", hostname: "azurite.micro-commerce.k8s.orb.local" },
    ],
  },
};
```

- [ ] **Step 2: Verify the web build still passes and produces standalone output**

Run: `cd src/web && npm run build && ls .next/standalone/server.js`
Expected: build succeeds; `.next/standalone/server.js` exists.

- [ ] **Step 3: Add the k8s web origin to the realm `redirectUris` and `webOrigins`**

In `src/AppHost/Realms/microcommerce-realm.json`, for the `microcommerce-web` client, extend the arrays (keep the existing `localhost:3000` entry so Aspire dev still works):

```jsonc
"redirectUris": ["http://localhost:3000/*", "http://web.micro-commerce.k8s.orb.local/*"],
"webOrigins": ["http://localhost:3000", "http://web.micro-commerce.k8s.orb.local"]
```

(If `webOrigins` is absent, add it next to `redirectUris`.)

- [ ] **Step 4: Validate the realm JSON is still well-formed**

Run: `cd src/Services/.. ; python3 -m json.tool src/AppHost/Realms/microcommerce-realm.json > /dev/null && echo OK`
Expected: `OK` (no JSON error).

- [ ] **Step 5: Commit**

```bash
git add src/web/next.config.ts src/AppHost/Realms/microcommerce-realm.json
git commit -m "feat(deploy): make web container-ready + allow k8s OAuth redirect/origin"
```

---

### Task 2: Catalog API Dockerfile

**Files:**
- Create: `src/Services/Catalog.API/src/Dockerfile`
- Create: `src/Services/Catalog.API/src/.dockerignore`

- [ ] **Step 1: Write the `.dockerignore`** (`src/Services/Catalog.API/src/.dockerignore`)

```gitignore
**/bin/
**/obj/
**/.vs/
**/.vscode/
**/*.user
**/node_modules/
Dockerfile*
.dockerignore
**/*.md
```

- [ ] **Step 2: Write the Dockerfile** (`src/Services/Catalog.API/src/Dockerfile`)

Build context is `src/Services/Catalog.API/src` (so all four projects are reachable). Output assembly is `MicroCommerce.Catalog.dll`.

```dockerfile
# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# csproj-first for layer caching
COPY Api/MicroCommerce.Catalog.csproj                           Api/
COPY Application/MicroCommerce.Catalog.Application.csproj        Application/
COPY Infrastructure/MicroCommerce.Catalog.Infrastructure.csproj Infrastructure/
COPY Domain/MicroCommerce.Catalog.Domain.csproj                 Domain/
RUN dotnet restore Api/MicroCommerce.Catalog.csproj
COPY . .
RUN dotnet publish Api/MicroCommerce.Catalog.csproj -c Release -o /app --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
WORKDIR /app
LABEL org.opencontainers.image.source=https://github.com/baotoq/micro-commerce
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "MicroCommerce.Catalog.dll"]
```

- [ ] **Step 3: Build the image (the gate)**

Run: `docker build -t catalog-api:dev -f src/Services/Catalog.API/src/Dockerfile src/Services/Catalog.API/src`
Expected: build succeeds; final image based on `aspnet:10.0-noble-chiseled`.

- [ ] **Step 4: Smoke-run the container (expect it to start and fail only on missing DB)**

Run:
```bash
docker run --rm -e ASPNETCORE_ENVIRONMENT=Development catalog-api:dev &
sleep 5; docker ps --filter ancestor=catalog-api:dev --format '{{.Status}}'; \
docker stop $(docker ps -q --filter ancestor=catalog-api:dev) 2>/dev/null || true
```
Expected: the process starts (logs show it attempting `MigrateAsync` and failing to reach Postgres — that proves the entrypoint/assembly name is correct). A clean `dotnet MicroCommerce.Catalog.dll` start with no "file not found" is the pass signal.

- [ ] **Step 5: Commit**

```bash
git add src/Services/Catalog.API/src/Dockerfile src/Services/Catalog.API/src/.dockerignore
git commit -m "feat(deploy): chiseled .NET 10 Dockerfile for Catalog API"
```

---

### Task 3: Web Dockerfile

**Files:**
- Create: `src/web/Dockerfile`
- Create: `src/web/.dockerignore`

- [ ] **Step 1: Write the `.dockerignore`** (`src/web/.dockerignore`)

```gitignore
node_modules
.next
.git
Dockerfile*
.dockerignore
npm-debug.log*
**/*.md
e2e
playwright-report
test-results
```

- [ ] **Step 2: Write the Dockerfile** (`src/web/Dockerfile`) — based on the current `vercel/next.js` `with-docker` example

```dockerfile
# syntax=docker/dockerfile:1
ARG NODE_VERSION=24-slim
FROM node:${NODE_VERSION} AS deps
WORKDIR /app
COPY package.json package-lock.json* ./
RUN --mount=type=cache,target=/root/.npm npm ci --no-audit --no-fund

FROM node:${NODE_VERSION} AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .
ENV NODE_ENV=production
RUN npm run build

FROM node:${NODE_VERSION} AS runner
WORKDIR /app
LABEL org.opencontainers.image.source=https://github.com/baotoq/micro-commerce
ENV NODE_ENV=production
ENV PORT=3000
ENV HOSTNAME="0.0.0.0"
COPY --from=builder --chown=node:node /app/public ./public
RUN mkdir .next && chown node:node .next
COPY --from=builder --chown=node:node /app/.next/standalone ./
COPY --from=builder --chown=node:node /app/.next/static ./.next/static
USER node
EXPOSE 3000
CMD ["node", "server.js"]
```

- [ ] **Step 3: Build the image (the gate)**

Run: `docker build -t web:dev src/web`
Expected: build succeeds (requires Task 1's `output: 'standalone'`).

- [ ] **Step 4: Smoke-run the container**

Run:
```bash
docker run --rm -d -p 3001:3000 --name web-smoke web:dev
sleep 4; curl -fsS -o /dev/null -w "%{http_code}\n" http://localhost:3001 || true
docker stop web-smoke
```
Expected: HTTP `200` (or `307` redirect to login) — server boots and serves. Not a connection-refused.

- [ ] **Step 5: Commit**

```bash
git add src/web/Dockerfile src/web/.dockerignore
git commit -m "feat(deploy): Next.js 16 standalone Dockerfile for web"
```

---

# Phase 2 — Infrastructure manifests

### Task 4: Infra kustomization scaffold + namespace

**Files:**
- Create: `deploy/k8s/infra/namespace.yaml`
- Create: `deploy/k8s/infra/kustomization.yaml`

- [ ] **Step 1: Write `namespace.yaml`**

```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: micro-commerce
```

- [ ] **Step 2: Write `kustomization.yaml` (starts with just the namespace; resources added per task)**

```yaml
apiVersion: kustomize.config.k8s.io/v1beta1
kind: Kustomization
namespace: micro-commerce
resources:
  - namespace.yaml
  - postgres/secret.yaml
  - postgres/statefulset.yaml
  - postgres/service.yaml
  - redis/deployment.yaml
  - redis/service.yaml
  - keycloak/admin-secret.yaml
  - keycloak/deployment.yaml
  - keycloak/service.yaml
  - azurite/pvc.yaml
  - azurite/deployment.yaml
  - azurite/service.yaml
  - coredns-custom.yaml
configMapGenerator:
  - name: keycloak-realm
    files:
      - keycloak/microcommerce-realm.json
generatorOptions:
  disableNameSuffixHash: true
```

- [ ] **Step 3: Install kubeconform (validation tool) if missing**

Run: `which kubeconform || (brew install kubeconform)`
Expected: a kubeconform path.

- [ ] **Step 4: Verify the build fails (resources not authored yet) — the failing gate**

Run: `kustomize build deploy/k8s/infra >/dev/null`
Expected: FAIL — missing files (e.g. `postgres/secret.yaml`). This confirms the kustomization references the files the next tasks create.

- [ ] **Step 5: Commit**

```bash
git add deploy/k8s/infra/namespace.yaml deploy/k8s/infra/kustomization.yaml
git commit -m "feat(deploy): infra kustomization scaffold + namespace"
```

---

### Task 5: Postgres manifests

**Files:**
- Create: `deploy/k8s/infra/postgres/secret.yaml`, `statefulset.yaml`, `service.yaml`

- [ ] **Step 1: Write `postgres/secret.yaml`** (DEV-ONLY — mirrors the hardcoded dev value in AppHost)

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: postgres-secret
type: Opaque
stringData:
  POSTGRES_PASSWORD: "dev-only-postgres-pw"  # DEV ONLY — never reuse in prod
```

- [ ] **Step 2: Write `postgres/statefulset.yaml`** (PVC omits storageClassName → default `local-path`)

```yaml
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: postgres
spec:
  serviceName: postgres
  replicas: 1
  selector: { matchLabels: { app: postgres } }
  template:
    metadata: { labels: { app: postgres } }
    spec:
      containers:
        - name: postgres
          image: postgres:17
          ports: [ { containerPort: 5432 } ]
          env:
            - name: POSTGRES_DB
              value: catalogdb
            - name: POSTGRES_PASSWORD
              valueFrom: { secretKeyRef: { name: postgres-secret, key: POSTGRES_PASSWORD } }
            - name: PGDATA
              value: /var/lib/postgresql/data/pgdata
          readinessProbe:
            exec: { command: ["pg_isready", "-U", "postgres", "-d", "catalogdb"] }
            initialDelaySeconds: 5
            periodSeconds: 5
          volumeMounts:
            - { name: data, mountPath: /var/lib/postgresql/data }
  volumeClaimTemplates:
    - metadata: { name: data }
      spec:
        accessModes: ["ReadWriteOnce"]
        resources: { requests: { storage: 2Gi } }
```

- [ ] **Step 3: Write `postgres/service.yaml`**

```yaml
apiVersion: v1
kind: Service
metadata:
  name: postgres
spec:
  selector: { app: postgres }
  ports: [ { port: 5432, targetPort: 5432 } ]
```

- [ ] **Step 4: Validate**

Run: `kustomize build deploy/k8s/infra 2>&1 | head -5` then once redis/keycloak/azurite exist the full validation runs in Task 9. For now: `kubeconform -summary -skip Secret <(kustomize build deploy/k8s/infra 2>/dev/null || true)` may still error on not-yet-authored files — acceptable until Task 9.
Expected: no schema error attributable to the Postgres resources.

- [ ] **Step 5: Commit**

```bash
git add deploy/k8s/infra/postgres
git commit -m "feat(deploy): postgres statefulset + service + dev secret"
```

---

### Task 6: Redis manifests + verify OrbStack cluster

**Files:**
- Create: `deploy/k8s/infra/redis/deployment.yaml`, `service.yaml`

- [ ] **Step 0: Start OrbStack k8s and verify the LoadBalancer domain format**

Run:
```bash
orb start k8s
kubectl config use-context orbstack
kubectl get storageclass
```
Expected: cluster reachable; `local-path (default) rancher.io/local-path` present. Note the domain format OrbStack uses for LoadBalancer services (test later in Task 8); if it is not `<svc>.<ns>.k8s.orb.local`, update the host constants.

- [ ] **Step 1: Write `redis/deployment.yaml`**

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: redis
spec:
  replicas: 1
  selector: { matchLabels: { app: redis } }
  template:
    metadata: { labels: { app: redis } }
    spec:
      containers:
        - name: redis
          image: redis:7
          ports: [ { containerPort: 6379 } ]
          readinessProbe:
            exec: { command: ["redis-cli", "ping"] }
            initialDelaySeconds: 3
            periodSeconds: 5
```

- [ ] **Step 2: Write `redis/service.yaml`**

```yaml
apiVersion: v1
kind: Service
metadata:
  name: redis
spec:
  selector: { app: redis }
  ports: [ { port: 6379, targetPort: 6379 } ]
```

- [ ] **Step 3: Commit**

```bash
git add deploy/k8s/infra/redis
git commit -m "feat(deploy): redis deployment + service (cache + dapr pubsub backing)"
```

---

### Task 7: Keycloak manifests (ephemeral, hostname-pinned)

**Files:**
- Create: `deploy/k8s/infra/keycloak/admin-secret.yaml`, `deployment.yaml`, `service.yaml`
- Create: `deploy/k8s/infra/keycloak/microcommerce-realm.json` (copy of the AppHost realm with k8s redirect URI)

- [ ] **Step 1: Copy the realm JSON into the deploy tree (kept in sync with AppHost)**

Run: `cp src/AppHost/Realms/microcommerce-realm.json deploy/k8s/infra/keycloak/microcommerce-realm.json`
(This file already includes the k8s redirect URI from Task 1. Add a top comment to both? JSON has no comments — instead note the sync requirement in `deploy/README.md`, Task 25.)

- [ ] **Step 2: Write `keycloak/admin-secret.yaml`** (DEV-ONLY)

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: keycloak-admin
type: Opaque
stringData:
  password: "admin"  # DEV ONLY
```

- [ ] **Step 3: Write `keycloak/deployment.yaml`** (ephemeral DB → re-imports realm every boot; `KC_HOSTNAME` pinned)

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: keycloak
spec:
  replicas: 1
  selector: { matchLabels: { app: keycloak } }
  template:
    metadata: { labels: { app: keycloak } }
    spec:
      containers:
        - name: keycloak
          image: quay.io/keycloak/keycloak:26.3.3
          args: ["start-dev", "--import-realm"]
          env:
            - { name: KC_HOSTNAME, value: "http://keycloak.micro-commerce.k8s.orb.local" }
            - { name: KC_HTTP_ENABLED, value: "true" }
            - { name: KC_BOOTSTRAP_ADMIN_USERNAME, value: "admin" }
            - name: KC_BOOTSTRAP_ADMIN_PASSWORD
              valueFrom: { secretKeyRef: { name: keycloak-admin, key: password } }
          ports: [ { containerPort: 8080, name: http } ]
          readinessProbe:
            httpGet: { path: /realms/microcommerce/.well-known/openid-configuration, port: 8080 }
            initialDelaySeconds: 25
            periodSeconds: 10
            failureThreshold: 12
          volumeMounts:
            - { name: realm-import, mountPath: /opt/keycloak/data/import, readOnly: true }
      volumes:
        - name: realm-import
          configMap: { name: keycloak-realm }
```

- [ ] **Step 4: Write `keycloak/service.yaml`** (LoadBalancer → external OrbStack domain)

```yaml
apiVersion: v1
kind: Service
metadata:
  name: keycloak
spec:
  type: LoadBalancer
  selector: { app: keycloak }
  ports: [ { port: 80, targetPort: 8080 } ]
```

- [ ] **Step 5: Commit**

```bash
git add deploy/k8s/infra/keycloak
git commit -m "feat(deploy): keycloak (ephemeral, hostname-pinned) + realm configmap"
```

---

### Task 8: Azurite manifests

**Files:**
- Create: `deploy/k8s/infra/azurite/pvc.yaml`, `deployment.yaml`, `service.yaml`

- [ ] **Step 1: Write `azurite/pvc.yaml`**

```yaml
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: azurite-data
spec:
  accessModes: ["ReadWriteOnce"]
  resources: { requests: { storage: 1Gi } }
```

- [ ] **Step 2: Write `azurite/deployment.yaml`** (`--disableProductStyleUrl` + `--blobHost 0.0.0.0` are mandatory)

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: azurite
spec:
  replicas: 1
  selector: { matchLabels: { app: azurite } }
  template:
    metadata: { labels: { app: azurite } }
    spec:
      containers:
        - name: azurite
          image: mcr.microsoft.com/azure-storage/azurite:latest
          args: ["azurite-blob", "--blobHost", "0.0.0.0", "--blobPort", "10000",
                 "--disableProductStyleUrl", "--location", "/data", "--skipApiVersionCheck"]
          ports: [ { containerPort: 10000 } ]
          volumeMounts:
            - { name: data, mountPath: /data }
      volumes:
        - name: data
          persistentVolumeClaim: { claimName: azurite-data }
```

- [ ] **Step 3: Write `azurite/service.yaml`** (LoadBalancer → external domain, port 80 so the URL needs no port)

```yaml
apiVersion: v1
kind: Service
metadata:
  name: azurite
spec:
  type: LoadBalancer
  selector: { app: azurite }
  ports: [ { port: 80, targetPort: 10000 } ]
```

- [ ] **Step 4: Commit**

```bash
git add deploy/k8s/infra/azurite
git commit -m "feat(deploy): azurite (path-style, blobHost 0.0.0.0) + pvc + service"
```

---

### Task 9: CoreDNS rewrite + full infra validation + apply

**Files:**
- Create: `deploy/k8s/infra/coredns-custom.yaml`

- [ ] **Step 1: Write `coredns-custom.yaml`** (K3s-honored custom server block; maps external hosts → in-cluster Service FQDNs for pods)

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: coredns-custom
  namespace: kube-system
data:
  micro-commerce.server: |
    keycloak.micro-commerce.k8s.orb.local:53 {
      rewrite stop {
        name exact keycloak.micro-commerce.k8s.orb.local keycloak.micro-commerce.svc.cluster.local
        answer auto
      }
      forward . 127.0.0.1
    }
    azurite.micro-commerce.k8s.orb.local:53 {
      rewrite stop {
        name exact azurite.micro-commerce.k8s.orb.local azurite.micro-commerce.svc.cluster.local
        answer auto
      }
      forward . 127.0.0.1
    }
```

> Note: this ConfigMap is in `kube-system`, not `micro-commerce`. Because the infra kustomization sets `namespace: micro-commerce`, pin it back with the explicit `namespace: kube-system` in metadata AND exclude it from the namespace transformer — add it via a separate `resources` entry is fine since Kustomize honors an explicit `metadata.namespace` only if the transformer allows. To be safe, the app.yaml ArgoCD Application for infra sets the destination namespace to `micro-commerce`, but cluster-scoped/other-namespace resources keep their own `metadata.namespace`. Validate in Step 2; if Kustomize rewrites it, move `coredns-custom.yaml` into its own tiny kustomize dir referenced by the infra ArgoCD app via multiple sources, or apply it in the bootstrap script. (Decision recorded in README.)

- [ ] **Step 2: Validate the full infra build against schemas**

Run:
```bash
kustomize build deploy/k8s/infra | kubeconform -summary -ignore-missing-schemas -skip Secret
```
Expected: `Valid` for all resources, `0 errors`. Confirm the `coredns-custom` ConfigMap still has `namespace: kube-system` in the output (`kustomize build deploy/k8s/infra | grep -A2 coredns-custom`). If it was rewritten to `micro-commerce`, apply the fallback from Step 1's note.

- [ ] **Step 3: Apply infra to the live cluster**

Run:
```bash
kubectl apply -k deploy/k8s/infra
kubectl -n kube-system rollout restart deployment coredns
kubectl -n micro-commerce rollout status deploy/keycloak --timeout=180s
kubectl -n micro-commerce rollout status deploy/azurite --timeout=120s
kubectl -n micro-commerce rollout status statefulset/postgres --timeout=120s
kubectl -n micro-commerce get svc
```
Expected: all roll out Ready; LoadBalancer services show external domains.

- [ ] **Step 4: Verify the external domain format + in-pod CoreDNS rewrite**

Run:
```bash
# external (host) resolution + format check:
kubectl -n micro-commerce get svc keycloak -o wide
curl -fsS http://keycloak.micro-commerce.k8s.orb.local/realms/microcommerce/.well-known/openid-configuration | head -c 200
# in-pod resolution via the rewrite:
kubectl -n micro-commerce run dnstest --rm -it --restart=Never --image=busybox -- \
  nslookup keycloak.micro-commerce.k8s.orb.local
```
Expected: discovery doc returns JSON with `"issuer":"http://keycloak.micro-commerce.k8s.orb.local/realms/microcommerce"`; nslookup resolves to a cluster ClusterIP. **If the external domain format differs**, update the three host constants (Task conventions) across all files now and re-apply.

- [ ] **Step 5: Commit**

```bash
git add deploy/k8s/infra/coredns-custom.yaml
git commit -m "feat(deploy): coredns rewrite for single-URL keycloak/azurite resolution"
```

---

# Phase 3 — App workloads + Dapr

### Task 10: Install Dapr control plane (manual, pre-ArgoCD)

- [ ] **Step 1: Add the Helm repo and pin the current GA version**

Run:
```bash
helm repo add dapr https://dapr.github.io/helm-charts/ && helm repo update
helm search repo dapr/dapr --versions | head -3
```
Expected: a list; note the latest GA (e.g. `1.16.x`). Record the exact version — it is reused in Task 18.

- [ ] **Step 2: Install Dapr (non-HA, ServerSideApply via helm)**

Run:
```bash
helm upgrade --install dapr dapr/dapr --version <GA_FROM_STEP_1> \
  --namespace dapr-system --create-namespace \
  --set global.ha.enabled=false --wait
kubectl get pods -n dapr-system
kubectl get crd | grep dapr.io
```
Expected: `dapr-operator`, `dapr-placement`, `dapr-sidecar-injector`, `dapr-sentry` Running; `components.dapr.io` CRD present.

(No commit — this is an imperative install reproduced declaratively in Task 18.)

---

### Task 11: Dapr pubsub Component

**Files:**
- Create: `deploy/k8s/dapr-components/pubsub.yaml`, `kustomization.yaml`

- [ ] **Step 1: Write `pubsub.yaml`** (name `pubsub` matches `dapr.PublishEventAsync("pubsub", ...)`; DB 1 isolates from cache DB 0)

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: pubsub
  namespace: micro-commerce
spec:
  type: pubsub.redis
  version: v1
  metadata:
    - { name: redisHost, value: "redis.micro-commerce.svc.cluster.local:6379" }
    - { name: redisPassword, value: "" }
    - { name: redisDB, value: "1" }
    - { name: redisType, value: "node" }
    - { name: consumerID, value: "catalog-api" }
    - { name: enableTLS, value: "false" }
```

- [ ] **Step 2: Write `kustomization.yaml`**

```yaml
apiVersion: kustomize.config.k8s.io/v1beta1
kind: Kustomization
namespace: micro-commerce
resources:
  - pubsub.yaml
```

- [ ] **Step 3: Validate + apply (Dapr CRDs now exist from Task 10)**

Run:
```bash
kustomize build deploy/k8s/dapr-components | kubeconform -summary -ignore-missing-schemas
kubectl apply -k deploy/k8s/dapr-components
kubectl -n micro-commerce get components.dapr.io
```
Expected: kubeconform skips the CRD (missing schema, ignored); `pubsub` component listed.

- [ ] **Step 4: Commit**

```bash
git add deploy/k8s/dapr-components
git commit -m "feat(deploy): dapr redis pubsub component (db1, consumerID=catalog-api)"
```

---

### Task 12: Catalog API manifests

**Files:**
- Create: `deploy/k8s/app/base/catalog-api/{deployment,service,config,secret}.yaml`

- [ ] **Step 1: Write `catalog-api/secret.yaml`** (DEV-ONLY connection strings)

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: catalog-secrets
type: Opaque
stringData:
  ConnectionStrings__catalogdb: "Host=postgres;Port=5432;Database=catalogdb;Username=postgres;Password=dev-only-postgres-pw"
  ConnectionStrings__photos: "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://azurite.micro-commerce.k8s.orb.local/devstoreaccount1;"
```

- [ ] **Step 2: Write `catalog-api/config.yaml`**

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: catalog-config
data:
  ASPNETCORE_ENVIRONMENT: "Development"
  SEED_PRODUCTS: "true"
  ConnectionStrings__cache: "redis:6379"
  Keycloak__Authority: "http://keycloak.micro-commerce.k8s.orb.local/realms/microcommerce"
```

- [ ] **Step 3: Write `catalog-api/deployment.yaml`** (Dapr annotations on pod template; initContainer waits for Postgres; httpGet probes; single replica)

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: catalog-api
spec:
  replicas: 1
  selector: { matchLabels: { app: catalog-api } }
  template:
    metadata:
      labels: { app: catalog-api }
      annotations:
        dapr.io/enabled: "true"
        dapr.io/app-id: "catalog-api"
        dapr.io/app-port: "8080"
        dapr.io/app-protocol: "http"
    spec:
      initContainers:
        - name: wait-postgres
          image: busybox:1.36
          command: ["sh", "-c", "until nc -z postgres 5432; do echo waiting-postgres; sleep 2; done"]
      containers:
        - name: catalog-api
          image: catalog-api
          ports: [ { containerPort: 8080 } ]
          envFrom:
            - configMapRef: { name: catalog-config }
            - secretRef: { name: catalog-secrets }
          readinessProbe:
            httpGet: { path: /health, port: 8080 }
            initialDelaySeconds: 10
            periodSeconds: 10
            failureThreshold: 12
          livenessProbe:
            httpGet: { path: /alive, port: 8080 }
            initialDelaySeconds: 20
            periodSeconds: 15
```

- [ ] **Step 4: Write `catalog-api/service.yaml`** (ClusterIP — only the web pod and Dapr reach it)

```yaml
apiVersion: v1
kind: Service
metadata:
  name: catalog-api
spec:
  selector: { app: catalog-api }
  ports: [ { port: 8080, targetPort: 8080 } ]
```

- [ ] **Step 5: Commit**

```bash
git add deploy/k8s/app/base/catalog-api
git commit -m "feat(deploy): catalog-api deployment (+dapr sidecar, pg-wait, probes)"
```

---

### Task 13: Web manifests

**Files:**
- Create: `deploy/k8s/app/base/web/{deployment,service,config,secret}.yaml`

- [ ] **Step 1: Write `web/secret.yaml`** (DEV-ONLY auth secrets — mirror AppHost values)

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: web-secrets
type: Opaque
stringData:
  AUTH_KEYCLOAK_SECRET: "dev-only-web-secret"
  AUTH_SECRET: "dev-only-auth-secret-not-for-prod-0123456789abcdef"
```

- [ ] **Step 2: Write `web/config.yaml`** (API via in-cluster DNS; issuer is the one external URL)

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: web-config
data:
  API_URL: "http://catalog-api.micro-commerce.svc.cluster.local:8080"
  AUTH_KEYCLOAK_ISSUER: "http://keycloak.micro-commerce.k8s.orb.local/realms/microcommerce"
  AUTH_KEYCLOAK_ID: "microcommerce-web"
  AUTH_URL: "http://web.micro-commerce.k8s.orb.local"
```

- [ ] **Step 3: Write `web/deployment.yaml`**

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: web
spec:
  replicas: 1
  selector: { matchLabels: { app: web } }
  template:
    metadata: { labels: { app: web } }
    spec:
      containers:
        - name: web
          image: web
          ports: [ { containerPort: 3000 } ]
          envFrom:
            - configMapRef: { name: web-config }
            - secretRef: { name: web-secrets }
          readinessProbe:
            httpGet: { path: /, port: 3000 }
            initialDelaySeconds: 8
            periodSeconds: 10
```

- [ ] **Step 4: Write `web/service.yaml`** (LoadBalancer → external domain)

```yaml
apiVersion: v1
kind: Service
metadata:
  name: web
spec:
  type: LoadBalancer
  selector: { app: web }
  ports: [ { port: 80, targetPort: 3000 } ]
```

- [ ] **Step 5: Commit**

```bash
git add deploy/k8s/app/base/web
git commit -m "feat(deploy): web deployment + LoadBalancer service + config/secret"
```

---

### Task 14: App base + local overlay, build images into OrbStack, apply end-to-end

**Files:**
- Create: `deploy/k8s/app/base/kustomization.yaml`
- Create: `deploy/k8s/app/overlays/local/kustomization.yaml`

- [ ] **Step 1: Write `base/kustomization.yaml`**

```yaml
apiVersion: kustomize.config.k8s.io/v1beta1
kind: Kustomization
namespace: micro-commerce
resources:
  - catalog-api/secret.yaml
  - catalog-api/config.yaml
  - catalog-api/deployment.yaml
  - catalog-api/service.yaml
  - web/secret.yaml
  - web/config.yaml
  - web/deployment.yaml
  - web/service.yaml
```

- [ ] **Step 2: Write `overlays/local/kustomization.yaml`** (pins image tags; CI rewrites these). For the first manual bring-up, use the locally-built `:dev` tags loaded into OrbStack.

```yaml
apiVersion: kustomize.config.k8s.io/v1beta1
kind: Kustomization
namespace: micro-commerce
resources:
  - ../../base
images:
  - name: catalog-api
    newName: ghcr.io/baotoq/micro-commerce/catalog-api
    newTag: dev
  - name: web
    newName: ghcr.io/baotoq/micro-commerce/web
    newTag: dev
```

- [ ] **Step 3: Build images into OrbStack's shared store under the GHCR names**

Run:
```bash
docker build -t ghcr.io/baotoq/micro-commerce/catalog-api:dev -f src/Services/Catalog.API/src/Dockerfile src/Services/Catalog.API/src
docker build -t ghcr.io/baotoq/micro-commerce/web:dev src/web
```
Expected: both build. (OrbStack shares its image store with the cluster; `imagePullPolicy: IfNotPresent` is not set on base, so add it OR rely on the image existing locally. Add `imagePullPolicy: IfNotPresent` to both container specs now if pulls are attempted.)

- [ ] **Step 4: Validate + apply the overlay**

Run:
```bash
kustomize build deploy/k8s/app/overlays/local | kubeconform -summary -ignore-missing-schemas -skip Secret
kubectl apply -k deploy/k8s/app/overlays/local
kubectl -n micro-commerce rollout status deploy/catalog-api --timeout=240s
kubectl -n micro-commerce rollout status deploy/web --timeout=180s
```
Expected: both Ready. The catalog-api pod has 2 containers (`catalog-api` + injected `daprd`).

- [ ] **Step 5: Manual end-to-end verification**

Run:
```bash
curl -fsS http://catalog-api.micro-commerce.k8s.orb.local/health 2>/dev/null || \
  kubectl -n micro-commerce exec deploy/web -- wget -qO- http://catalog-api.micro-commerce.svc.cluster.local:8080/health
kubectl -n micro-commerce logs deploy/catalog-api -c daprd | grep -i "pubsub" | head
curl -fsS -o /dev/null -w "%{http_code}\n" http://web.micro-commerce.k8s.orb.local
```
Expected: `/health` → `Healthy`; daprd logs show the pubsub component loaded; web returns 200/redirect. **Then verify a Keycloak login + photo upload via browser in Task 24** (deferred to the full smoke).

- [ ] **Step 6: Commit**

```bash
git add deploy/k8s/app
git commit -m "feat(deploy): app base + local overlay; full stack runs on OrbStack"
```

---

# Phase 4 — ArgoCD

### Task 15: ArgoCD child Applications

**Files:**
- Create: `deploy/argocd/apps/{dapr,infra,dapr-components,app}.yaml`

- [ ] **Step 1: Write `infra.yaml`** (Kustomize, wave 0)

```yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: infra
  namespace: argocd
  annotations: { argocd.argoproj.io/sync-wave: "0" }
  finalizers: [ resources-finalizer.argocd.argoproj.io ]
spec:
  project: default
  source:
    repoURL: https://github.com/baotoq/micro-commerce.git
    path: deploy/k8s/infra
    targetRevision: master
  destination: { server: https://kubernetes.default.svc, namespace: micro-commerce }
  syncPolicy:
    automated: { prune: true, selfHeal: true }
    syncOptions: [ CreateNamespace=true ]
```

- [ ] **Step 2: Write `dapr-components.yaml`** (Kustomize, wave 1)

```yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: dapr-components
  namespace: argocd
  annotations: { argocd.argoproj.io/sync-wave: "1" }
  finalizers: [ resources-finalizer.argocd.argoproj.io ]
spec:
  project: default
  source:
    repoURL: https://github.com/baotoq/micro-commerce.git
    path: deploy/k8s/dapr-components
    targetRevision: master
  destination: { server: https://kubernetes.default.svc, namespace: micro-commerce }
  syncPolicy:
    automated: { prune: true, selfHeal: true }
    syncOptions: [ CreateNamespace=true ]
```

- [ ] **Step 3: Write `app.yaml`** (Kustomize overlay, wave 2)

```yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: app
  namespace: argocd
  annotations: { argocd.argoproj.io/sync-wave: "2" }
  finalizers: [ resources-finalizer.argocd.argoproj.io ]
spec:
  project: default
  source:
    repoURL: https://github.com/baotoq/micro-commerce.git
    path: deploy/k8s/app/overlays/local
    targetRevision: master
  destination: { server: https://kubernetes.default.svc, namespace: micro-commerce }
  syncPolicy:
    automated: { prune: true, selfHeal: true }
    syncOptions: [ CreateNamespace=true ]
```

- [ ] **Step 4: Write `dapr.yaml`** (Helm, wave -2, prune only — no selfHeal on the control plane)

```yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: dapr
  namespace: argocd
  annotations: { argocd.argoproj.io/sync-wave: "-2" }
  finalizers: [ resources-finalizer.argocd.argoproj.io ]
spec:
  project: default
  source:
    repoURL: https://dapr.github.io/helm-charts/
    chart: dapr
    targetRevision: <GA_FROM_TASK_10>   # e.g. 1.16.5
    helm:
      releaseName: dapr
      valuesObject:
        global: { ha: { enabled: false } }
  destination: { server: https://kubernetes.default.svc, namespace: dapr-system }
  syncPolicy:
    automated: { prune: true }
    syncOptions: [ CreateNamespace=true, ServerSideApply=true ]
```

- [ ] **Step 5: Validate all four are schema-valid Applications**

Run: `kubeconform -summary -ignore-missing-schemas deploy/argocd/apps/*.yaml`
Expected: `Valid` (Application CRD schema is ignored as missing — confirm 0 hard errors).

- [ ] **Step 6: Commit**

```bash
git add deploy/argocd/apps
git commit -m "feat(deploy): argocd child applications with sync waves"
```

---

### Task 16: Root App-of-Apps + bootstrap script

**Files:**
- Create: `deploy/bootstrap/root-app.yaml`, `deploy/bootstrap/install-argocd.sh`

- [ ] **Step 1: Write `root-app.yaml`**

```yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: root
  namespace: argocd
  finalizers: [ resources-finalizer.argocd.argoproj.io ]
spec:
  project: default
  source:
    repoURL: https://github.com/baotoq/micro-commerce.git
    path: deploy/argocd/apps
    targetRevision: master
    directory: { recurse: true }
  destination: { server: https://kubernetes.default.svc, namespace: argocd }
  syncPolicy:
    automated: { prune: true, selfHeal: true }
    syncOptions: [ CreateNamespace=true ]
```

- [ ] **Step 2: Write `install-argocd.sh`**

```bash
#!/usr/bin/env bash
set -euo pipefail

orb start k8s
kubectl config use-context orbstack

helm repo add argo https://argoproj.github.io/argo-helm >/dev/null 2>&1 || true
helm repo update >/dev/null
helm upgrade --install argocd argo/argo-cd \
  --namespace argocd --create-namespace --wait

# Optional private-GHCR pull secret (skip if packages are public):
if [[ -n "${GHCR_PAT:-}" && -n "${GHCR_USER:-}" ]]; then
  kubectl create namespace micro-commerce --dry-run=client -o yaml | kubectl apply -f -
  kubectl -n micro-commerce create secret docker-registry ghcr-pull \
    --docker-server=ghcr.io --docker-username="$GHCR_USER" \
    --docker-password="$GHCR_PAT" --docker-email="dev@local" \
    --dry-run=client -o yaml | kubectl apply -f -
fi

kubectl apply -f "$(dirname "$0")/root-app.yaml"

echo "ArgoCD admin password:"
kubectl -n argocd get secret argocd-initial-admin-secret \
  -o jsonpath='{.data.password}' | base64 -d; echo
echo "UI: kubectl -n argocd port-forward svc/argocd-server 8090:443  → https://localhost:8090"
```

- [ ] **Step 3: Make it executable + lint**

Run: `chmod +x deploy/bootstrap/install-argocd.sh && bash -n deploy/bootstrap/install-argocd.sh && echo OK`
Expected: `OK` (no syntax errors).

- [ ] **Step 4: Commit**

```bash
git add deploy/bootstrap
git commit -m "feat(deploy): app-of-apps root + bootstrap script"
```

---

### Task 17: Hand the running stack over to ArgoCD

> Goal: tear down the manual `kubectl apply`s and prove ArgoCD reconciles the same stack from git. Requires the commits so far to be pushed to `master` (the branch `targetRevision` tracks).

- [ ] **Step 1: Push the branch ArgoCD tracks**

Run: `git push origin HEAD:master` (or merge `dev`→`master` per your flow)
Expected: the `deploy/**` files are on `master`.

- [ ] **Step 2: Remove the manual applies (let ArgoCD own them)**

Run:
```bash
kubectl delete -k deploy/k8s/app/overlays/local --ignore-not-found
kubectl delete -k deploy/k8s/dapr-components --ignore-not-found
kubectl delete -k deploy/k8s/infra --ignore-not-found
# leave Dapr (helm) — ArgoCD will adopt it via the dapr Application
```

- [ ] **Step 3: Bootstrap ArgoCD**

Run: `./deploy/bootstrap/install-argocd.sh`
Expected: ArgoCD installed; root app applied; admin password printed.

- [ ] **Step 4: Watch reconciliation**

Run:
```bash
kubectl -n argocd get applications -w   # Ctrl-C when all Synced+Healthy
kubectl -n micro-commerce get pods
```
Expected: `dapr` → `infra` → `dapr-components` → `app` progress through waves to `Synced`/`Healthy`; all pods Ready.

- [ ] **Step 5: Commit (no file changes; record nothing or skip)** — no commit needed.

---

# Phase 5 — CI image loop

### Task 18: CI — manifest validation + image build + GitOps write-back

**Files:**
- Create: `.github/workflows/gitops.yml`

- [ ] **Step 1: Write `.github/workflows/gitops.yml`** (validate on PR/push; build + bump only on push to `master`)

```yaml
name: GitOps
on:
  push:
    branches: [master]
    paths-ignore: ['deploy/k8s/app/overlays/local/**']
  pull_request:
    branches: [master]

env:
  REGISTRY: ghcr.io
  IMAGE_PREFIX: ${{ github.repository }}

jobs:
  validate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: yokawasa/action-setup-kube-tools@v0.11.2
        with: { kustomize: '5.4.3', kubeconform: '0.6.7' }
      - name: Validate manifests
        run: |
          for d in deploy/k8s/infra deploy/k8s/dapr-components deploy/k8s/app/overlays/local; do
            echo "== $d =="
            kustomize build "$d" | kubeconform -summary -ignore-missing-schemas -skip Secret
          done

  build:
    if: github.event_name == 'push'
    needs: validate
    runs-on: ubuntu-latest
    permissions: { contents: read, packages: write }
    strategy:
      matrix:
        include:
          - name: catalog-api
            context: src/Services/Catalog.API/src
            file: src/Services/Catalog.API/src/Dockerfile
          - name: web
            context: src/web
            file: src/web/Dockerfile
    steps:
      - uses: actions/checkout@v4
      - uses: docker/login-action@9780b0c442fbb1117ed29e0efdff1e18412f7567   # v3.3.0
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}
      - id: meta
        uses: docker/metadata-action@8e5442c4ef9f78752691e2d8f8d19755c6f78e81   # v5.5.1
        with:
          images: ${{ env.REGISTRY }}/${{ env.IMAGE_PREFIX }}/${{ matrix.name }}
          tags: type=sha,format=long
      - uses: docker/build-push-action@4f58ea79222b3b9dc2c8bbdd6debcd730fa81162  # v6.9.0
        with:
          context: ${{ matrix.context }}
          file: ${{ matrix.file }}
          push: true
          tags: ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}

  bump-gitops:
    if: github.event_name == 'push'
    needs: build
    runs-on: ubuntu-latest
    permissions: { contents: write }
    concurrency: { group: gitops-write-back, cancel-in-progress: false }
    steps:
      - uses: actions/checkout@v4
      - uses: yokawasa/action-setup-kube-tools@v0.11.2
        with: { kustomize: '5.4.3' }
      - name: Bump image tags in the local overlay
        working-directory: deploy/k8s/app/overlays/local
        run: |
          TAG=sha-${{ github.sha }}
          kustomize edit set image \
            catalog-api=${{ env.REGISTRY }}/${{ env.IMAGE_PREFIX }}/catalog-api:$TAG \
            web=${{ env.REGISTRY }}/${{ env.IMAGE_PREFIX }}/web:$TAG
      - uses: stefanzweifel/git-auto-commit-action@8621497c8c39c72f3e2a999a26b4ca1b5058a842  # v5.0.1
        with:
          commit_message: "chore(gitops): bump images to sha-${{ github.sha }} [skip ci]"
          file_pattern: deploy/k8s/app/overlays/local/kustomization.yaml
```

> Note: the overlay `images:` `name:` keys must be the **bare** base names (`catalog-api`, `web`) so `kustomize edit set image catalog-api=...` matches. Task 14 set `newName` already; `kustomize edit set image` rewrites both newName and newTag.

- [ ] **Step 2: Validate the workflow YAML locally**

Run: `python3 -c "import yaml,sys; yaml.safe_load(open('.github/workflows/gitops.yml')); print('OK')"`
Expected: `OK`.

- [ ] **Step 3: Make GHCR packages public (avoids pull secret) OR set GHCR_PAT for bootstrap**

Action (manual, after first push): GitHub → Packages → `catalog-api` & `web` → Package settings → Change visibility → Public. (Documented in README.)

- [ ] **Step 4: Commit + push to trigger the loop**

```bash
git add .github/workflows/gitops.yml
git commit -m "ci(deploy): build images to GHCR + GitOps tag write-back"
git push origin HEAD:master
```

- [ ] **Step 5: Verify the loop closes**

Watch the Actions run: `validate` → `build` (pushes `sha-<sha>` images) → `bump-gitops` (commits the overlay). Then:
Run: `kubectl -n argocd get app app -o jsonpath='{.status.sync.status} {.status.health.status}'; echo`
Expected: ArgoCD picks up the bump commit and the `app` Application re-syncs to the new SHA images, ending `Synced Healthy`.

---

# Phase 6 — Verify & document

### Task 19: Full live smoke + browser round-trip

- [ ] **Step 1: ArgoCD + pod health**

Run:
```bash
kubectl -n argocd get applications
kubectl -n micro-commerce get pods
```
Expected: all Applications `Synced`/`Healthy`; all pods `Ready`.

- [ ] **Step 2: API + Dapr smoke**

Run:
```bash
curl -fsS http://catalog-api.micro-commerce.k8s.orb.local/health; echo
kubectl -n micro-commerce logs deploy/catalog-api -c daprd | grep -i pubsub | head
```
Expected: `Healthy`; pubsub component loaded. (If `/health` isn't externally exposed — catalog-api is ClusterIP — exec from the web pod instead.)

- [ ] **Step 3: Browser round-trip with agent-browser** (per CLAUDE.md QA + memory: always verify UI in a real browser)

Run:
```bash
AB=/usr/local/bin/agent-browser
$AB --session gitops open http://web.micro-commerce.k8s.orb.local
$AB --session gitops wait --load networkidle
$AB --session gitops errors
$AB --session gitops screenshot qa/evidence/screenshots/gitops-storefront.png
```
Then drive a **Keycloak login** (proves single-issuer plumbing) and a **seller photo upload** (proves Azurite host + CORS): navigate to the login flow, authenticate with the seeded dev seller (`seller@microcommerce.dev` / `Passw0rd!`), and on a listing page upload an image; confirm the SAS PUT preflight returns `Access-Control-Allow-Origin` (check `$AB ... network` or browser console).
Expected: storefront loads with zero console errors; login completes (no "invalid issuer"/"invalid redirect_uri"); upload succeeds (no CORS error); screenshot saved.

- [ ] **Step 4: Commit evidence**

```bash
git add qa/evidence/screenshots/gitops-storefront.png
git commit -m "test(deploy): live smoke evidence for GitOps stack"
```

---

### Task 20: Operator documentation

**Files:**
- Create: `deploy/README.md`

- [ ] **Step 1: Write `deploy/README.md`**

Cover: prerequisites (OrbStack k8s, helm, kustomize, kubeconform); one-time bootstrap (`GHCR_PAT`/public-packages note, `./deploy/bootstrap/install-argocd.sh`); the sync-wave topology; how to access the ArgoCD UI + web/keycloak URLs; the **realm-sync requirement** (`deploy/k8s/infra/keycloak/microcommerce-realm.json` must mirror `src/AppHost/Realms/microcommerce-realm.json`); the **CoreDNS rewrite re-verify after `orb restart k8s`**; the GitOps image loop (push to master → CI → write-back → ArgoCD); the DEV-ONLY-secrets caveat + Sealed Secrets upgrade path; and the `local-path` PV reclaim quirk (`orb restart` to free disk).

- [ ] **Step 2: Commit**

```bash
git add deploy/README.md
git commit -m "docs(deploy): operator README for the GitOps stack"
```

---

## Self-review

**Spec coverage** (every spec section maps to a task):
- §6 Containerization → Tasks 2, 3 · §7 code changes → Task 1 · §8 components → Tasks 5–8, 12, 13 · §9 networking/CoreDNS → Task 9 + host constants · §10 secrets → dev Secrets in Tasks 5/7/12/13 + bootstrap pull secret (16) · §11 CI loop → Task 18 · §12 bootstrap/waves → Tasks 15, 16, 17 · §13 verification → Tasks 9/11/14 (incremental) + 19 (full) · §14 phased sequence → Phases 1–6 · §15 open risks → addressed inline (Dapr version Task 10/15; CoreDNS durability Task 9 note + README; realm sync README; Azurite CORS Task 19; remotePatterns Task 1; EF migrate single-replica Task 12; PV reclaim README).
- §3 corrections all implemented: health via `ASPNETCORE_ENVIRONMENT=Development` (Task 12), CoreDNS rewrite (9), `KC_HOSTNAME` (7), ephemeral Keycloak (7), realm redirectUris (1), standalone+remotePatterns (1), `MicroCommerce.Catalog.dll` (2), Azurite flags (8), child-app sync-waves (15), Dapr pin (10).

**Placeholder scan:** the only deferred values are environment-derived and explicitly instructed: GHCR owner (`baotoq` + replace-note), `<GA_FROM_TASK_10>` Dapr version (resolved by `helm search` in Task 10, reused in 15), and the OrbStack domain-format verification (Task 6/9). No "TBD/handle errors/similar-to" placeholders.

**Type/name consistency:** image base names `catalog-api`/`web` are bare in base (Tasks 12/14) and used as `kustomize edit set image` keys in CI (18); `pubsub` component name matches `PublishEventAsync("pubsub", …)`; namespace `micro-commerce`, issuer/host strings, and Service DNS names are identical across Tasks 7/9/12/13; `targetRevision: master` is consistent across all ArgoCD Applications (15/16) and the push target (17/18).

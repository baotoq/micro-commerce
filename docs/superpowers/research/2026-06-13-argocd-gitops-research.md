# AREA: GHCR image build/push + GitOps tag write-back (local OrbStack k8s + ArgoCD)

## Corrections to design
- Wrong/risky assumption: 'permissions: { packages: write, contents: write }' on the build/push job. For PUSHING images you only need `packages: write` (+ `contents: read`). `contents: write` is only needed by the SEPARATE write-back job/step that commits the tag bump. Keep them split: the image-build job runs with least privilege (packages: write, contents: read); a downstream write-back job gets contents: write. Lumping contents:write onto the build job over-privileges it. (Optionally add attestations:write + id-token:write only if you generate attestations.)
- For a LOCAL single-cluster OrbStack setup, do NOT reach for Argo CD Image Updater first. The simplest correct loop is option (a): CI runs `kustomize edit set image` in the overlay and commits the bump back. Image Updater adds a second controller, requires Git write credentials living in-cluster, and its default (argocd API) method is pseudo-persistent (lost on app delete or Git sync). Recommend (a) for local; reserve Image Updater for multi-env/many-service fleets where hand-wiring CI write-back per app is the real pain.
- '[skip ci] to avoid loops' is only half the mechanism and is the weaker half. The PRIMARY loop-breaker is that a push authenticated with the default GITHUB_TOKEN does not trigger another workflow run at all. Rely on that; treat [skip ci] as a belt-and-suspenders marker. Conversely, if you push the bump using a PAT or GitHub App token (e.g. to deliberately trigger downstream CI), the GITHUB_TOKEN guard no longer applies and you MUST use [skip ci] + a path filter to avoid an infinite loop.
- Don't pull images by mutable tag (e.g. :latest or :dev) on the cluster and expect ArgoCD to redeploy — ArgoCD reconciles Git state, not registry state. The Deployment manifest must reference an immutable tag (git SHA / semver / digest) that the write-back step changes in Git. That Git change is what ArgoCD detects and syncs. With OrbStack also set imagePullPolicy appropriately: with immutable SHA tags, IfNotPresent is fine and avoids needless re-pulls.
- OrbStack-specific: an imagePullSecret in the manifests is still required for PRIVATE ghcr.io even on a local cluster — OrbStack's k8s does not share your host docker/podman credentials with kubelet. The pragmatic local shortcut is to make the GHCR package PUBLIC, which eliminates the secret entirely. If you keep it private, also remember the secret is per-namespace, so create it in every namespace that runs your workloads (or attach it to the namespace's default ServiceAccount).
- Pin third-party actions by commit SHA, not floating major tags, for the build/push and login steps (the official reference workflow pins docker/login-action, docker/metadata-action, docker/build-push-action by SHA). Floating @v6/@v5 tags on actions that handle registry credentials is a supply-chain risk.

## Verified facts
- [high] GitHub's canonical 'publishing Docker images' workflow pushes to ghcr.io using docker/login-action (registry: ghcr.io, username: ${{ github.actor }}, password: ${{ secrets.GITHUB_TOKEN }}), docker/metadata-action for tags/labels, and docker/build-push-action with push: true. The job-level permissions block is `contents: read`, `packages: write`, and (for attestations) `attestations: write` + `id-token: write`.  (src: https://docs.github.com/en/actions/use-cases-and-examples/publishing-packages/publishing-docker-images)
- [high] The repo-scoped GITHUB_TOKEN can push images to ghcr.io as long as the job has `permissions: packages: write`. No PAT is required for PUSH from CI. For the package to be auto-linked to the repo (and inherit Actions access), add `LABEL org.opencontainers.image.source=https://github.com/<owner>/<repo>` to the Dockerfile.  (src: https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-container-registry)
- [high] To PULL a private ghcr.io package into Kubernetes you create a dockerconfigjson secret of type kubernetes.io/dockerconfigjson via `kubectl create secret docker-registry`, with --docker-server, --docker-username, --docker-password (a PAT with read:packages), --docker-email; the Pod/Deployment references it via imagePullSecrets. The secret is namespace-scoped (must exist in the same namespace as the Pod).  (src: https://kubernetes.io/docs/tasks/configure-pod-container/pull-image-private-registry/)
- [high] A private ghcr.io package needs a PAT with at least the read:packages scope to pull. Making the package PUBLIC (Package settings > Danger Zone > Change visibility > Public) removes the auth requirement entirely — public images are pulled without any credentials, so no imagePullSecret is needed.  (src: https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-container-registry)
- [high] Argo CD Image Updater supports two write-back methods. The 'argocd' (default) method patches the Application via the API and is pseudo-persistent (lost on Application delete/recreate or a Git sync). The 'git' method (requires Argo CD v2.0+) commits back to the repo; with annotation write-back-target: kustomization it edits the Kustomization file 'as though you ran kustomize edit set image'. It needs write credentials configured via `git:secret:<namespace>/<secret>` (username+PAT for HTTPS, or sshPrivateKey, or GitHub App), and the Application should track a branch (or set git-branch).  (src: https://argocd-image-updater.readthedocs.io/en/stable/basics/update-methods/)
- [high] Commits/pushes made with the default GITHUB_TOKEN do NOT trigger new workflow runs (GitHub built-in recursion guard), so the CI write-back loop is broken even without [skip ci]. [skip ci] in the commit message is an additional/portable guard. stefanzweifel/git-auto-commit-action documents this same behavior and requires `permissions: contents: write` to push.  (src: https://github.com/stefanzweifel/git-auto-commit-action)
- [high] ghcr.io login uses docker/login-action; the pinned reference workflow uses actions/checkout@v6, docker/login-action, docker/metadata-action, docker/build-push-action (id: push, exposing outputs.digest), and actions/attest@v4 for provenance/attestation pushed to the registry.  (src: https://docs.github.com/en/actions/use-cases-and-examples/publishing-packages/publishing-docker-images)

## Recommended config
## 1) CI: build + push to GHCR (least-privilege job)

```yaml
# .github/workflows/release-images.yml
name: Build & Push Images
on:
  push:
    branches: [master]            # build on merges to main line
    paths-ignore: ['gitops/**']   # don't rebuild on tag write-back commits

env:
  REGISTRY: ghcr.io
  IMAGE_PREFIX: ${{ github.repository }}   # owner/micro-commerce

jobs:
  build:
    runs-on: ubuntu-latest
    permissions:
      contents: read            # NOT write — this job only pushes images
      packages: write
      # attestations: write     # only if you keep the attest step
      # id-token: write
    strategy:
      matrix:
        include:
          - name: catalog-api
            context: src
            dockerfile: src/Services/Catalog.API/src/Api/Dockerfile
          - name: web
            context: src/web
            dockerfile: src/web/Dockerfile
    outputs:
      sha: ${{ github.sha }}
    steps:
      - uses: actions/checkout@v4
      - uses: docker/login-action@65b78e6e13532edd9afa3aa52ac7964289d1a9c1   # pin by SHA
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}
      - id: meta
        uses: docker/metadata-action@9ec57ed1fcdbf14dcef7dfbe97b2010124a938b7
        with:
          images: ${{ env.REGISTRY }}/${{ env.IMAGE_PREFIX }}/${{ matrix.name }}
          tags: |
            type=sha,format=long          # immutable: sha-<gitsha>
            type=raw,value=latest,enable={{is_default_branch}}
      - uses: docker/build-push-action@f2a1d5e99d037542a71f64918e516c093c6f3fc4
        with:
          context: ${{ matrix.context }}
          file: ${{ matrix.dockerfile }}
          push: true
          tags: ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}
```
Add to each Dockerfile so packages auto-link to the repo:
`LABEL org.opencontainers.image.source=https://github.com/<owner>/micro-commerce`

## 2) GitOps tag write-back job (RECOMMENDED for local: option (a))

```yaml
  bump-gitops:
    needs: build
    runs-on: ubuntu-latest
    permissions:
      contents: write           # write-back lives HERE, isolated
    concurrency:                 # serialize bumps to avoid push races
      group: gitops-write-back
      cancel-in-progress: false
    steps:
      - uses: actions/checkout@v4
      - name: Install kustomize
        run: |
          curl -s "https://raw.githubusercontent.com/kubernetes-sigs/kustomize/master/hack/install_kustomize.sh" | bash
          sudo mv kustomize /usr/local/bin/
      - name: Bump image tags in overlay
        working-directory: gitops/overlays/local
        run: |
          TAG=sha-${{ needs.build.outputs.sha }}
          kustomize edit set image \
            ghcr.io/<owner>/micro-commerce/catalog-api=ghcr.io/<owner>/micro-commerce/catalog-api:$TAG \
            ghcr.io/<owner>/micro-commerce/web=ghcr.io/<owner>/micro-commerce/web:$TAG
      - name: Commit & push bump
        # GITHUB_TOKEN push does NOT retrigger workflows (recursion guard);
        # [skip ci] is the belt-and-suspenders marker.
        uses: stefanzweifel/git-auto-commit-action@v5
        with:
          commit_message: "chore(gitops): bump images to sha-${{ needs.build.outputs.sha }} [skip ci]"
          file_pattern: gitops/overlays/local/kustomization.yaml
```

## 3) Pull from GHCR on OrbStack k8s

EASIEST (local): make each package Public (Package settings > Danger Zone). No secret, no imagePullSecrets needed.

If kept private:
```bash
kubectl create secret docker-registry ghcr-pull \
  --docker-server=ghcr.io \
  --docker-username=<github-username> \
  --docker-password=<PAT-with-read:packages> \
  --docker-email=<any@email> \
  -n micro-commerce          # repeat per namespace, or attach to the SA
```
Deployment:
```yaml
spec:
  template:
    spec:
      imagePullSecrets:
        - name: ghcr-pull
      containers:
        - name: catalog-api
          image: ghcr.io/<owner>/micro-commerce/catalog-api:sha-<gitsha>
          imagePullPolicy: IfNotPresent   # immutable SHA tags -> safe
```
Provision the secret declaratively under ArgoCD as a Sealed Secret / SOPS, or (local-only) a plain Secret in an early sync wave; never commit a raw PAT.

## Open risks
- Push race: if multiple branches/runs push to master close together, two bump commits can race on the write-back. Mitigated by the `concurrency: { group, cancel-in-progress: false }` block above so bumps serialize; without it git-auto-commit can fail on non-fast-forward.
- Secret distribution: a private-GHCR imagePullSecret is per-namespace. If Catalog API, web, Keycloak, etc. land in different namespaces you must replicate the secret (or use a reflector). Making packages public for the local cluster sidesteps this entirely.
- ArgoCD only reconciles Git, not the registry. If anyone mutates a :latest/:dev tag in GHCR, the cluster will not change until a Git bump lands. Always deploy immutable SHA/digest tags so Git is the single source of truth.
- If you later switch the write-back to push with a PAT/GitHub App (to trigger downstream CI), the GITHUB_TOKEN recursion guard no longer protects you — you MUST keep [skip ci] AND the paths-ignore filter or you get an infinite build->bump->build loop.
- Argo CD Image Updater (option b), if adopted later, needs in-cluster Git write credentials and its git method requires the Application to track a branch; its default argocd-API method is pseudo-persistent and will silently drift back on app recreate/sync — a real footgun if mixed with Git-sourced overlays.
- Pinning actions by SHA (recommended) means you must periodically refresh the pins (Dependabot for github-actions ecosystem already exists in this repo's .github/dependabot.yml — extend it to the new workflow).


================================================================================

# AREA: Dapr on Kubernetes — control-plane Helm install via ArgoCD, sidecar injection annotations, Redis pubsub Component, and CRD/sync-wave ordering

## Corrections to design
- STALE VERSION: The locked design assumes Dapr 1.15. Current 'latest' in Dapr docs is v1.18 — pin `targetRevision: 1.18.x` (chart version == Dapr release version), not 1.15. Verify the exact patch with `helm search repo dapr --versions` after `helm repo add dapr https://dapr.github.io/helm-charts/`.
- CRDs are NOT a separate concern you can ignore: the dapr/dapr chart bundles the Component/Configuration (and Subscription/Resiliency/HTTPEndpoint) CRDs. Because ArgoCD will not sequence Helm-bundled CRDs against your own Component CRs via sync-waves alone, you MUST put the Dapr control-plane Application (which carries the CRDs) in an earlier sync-wave than the Application(s) that contain the `kind: Component` manifests. Do not apply the Redis pubsub Component in the same wave as, or before, the control-plane chart — the CR kind won't exist yet and the sync dry-run will fail.
- consumerID should be set explicitly (not left to auto-generation) for the Catalog API. With app-id already used as the pub/sub consumer ID, an unset consumerID is fine for a single replica, but if you scale replicas you want one shared consumer group — pin `consumerID` to the app-id (or `{appID}`) so all replicas share offsets rather than each pod forming its own group.
- Sharing one Redis between Dapr pubsub AND the .NET output cache is safe via `redisDB` ONLY in node mode. Your locked design uses a single in-cluster Redis (node mode), so set pubsub `redisDB: "1"` and keep the output-cache/state on `redisDB: "0"` (or vice-versa). This isolation silently breaks if Redis is ever switched to cluster mode (redisDB ignored) — note that as a constraint.
- app-protocol for the Catalog API should be `http` (the default) since it's an ASP.NET minimal API over HTTP. Only set `dapr.io/app-protocol: "h2c"` or `grpc` if the app actually serves gRPC/HTTP-2 cleartext to daprd; do not set it speculatively.
- Aspire ServiceDefaults expose `/health` and `/alive`. These are app (Kubernetes) probes, not Dapr's. If you also want Dapr's app-health-check feature, that's separate annotations (`dapr.io/enable-app-health-check`, `dapr.io/app-health-check-path`); don't conflate the two. The locked design doesn't require Dapr app-health-check — leave it off unless needed.

## Verified facts
- [high] The Dapr control plane is installed from the OCI/HTTP Helm repo `https://dapr.github.io/helm-charts/`, chart name `dapr/dapr`, into namespace `dapr-system`. Canonical command: `helm upgrade --install dapr dapr/dapr --version=1.18 --namespace dapr-system --create-namespace --wait`. v1.18 is the version marked 'latest' in current docs (the prompt's assumption of 1.15 is stale).  (src: https://docs.dapr.io/operations/hosting/kubernetes/kubernetes-deploy/)
- [high] A successful control-plane install yields four pods in `dapr-system`: dapr-operator, dapr-placement, dapr-sidecar-injector, dapr-sentry.  (src: https://docs.dapr.io/operations/hosting/kubernetes/kubernetes-deploy/)
- [high] The dapr/dapr chart installs the Dapr CRDs (the README states it installs the 'Dapr Component and Configuration Kubernetes CRDs'). CRDs ship inside the chart, not only via Helm's bare crds/ dir, which matters for ArgoCD ordering.  (src: https://github.com/dapr/dapr/blob/master/charts/dapr/README.md)
- [high] Sidecar injection is driven by pod-template annotations watched by dapr-sidecar-injector: `dapr.io/enabled: "true"` injects the daprd container; `dapr.io/app-id` is the unique app ID (used for service discovery, state encapsulation, and the pub/sub consumer ID); `dapr.io/app-port` tells Dapr which port the app listens on.  (src: https://docs.dapr.io/reference/arguments-annotations-overview/)
- [high] `dapr.io/app-protocol` configures Dapr->app communication and accepts: http, grpc, https (HTTP+TLS), grpcs (gRPC+TLS), h2c (HTTP/2 cleartext). Default is `http`.  (src: https://docs.dapr.io/reference/arguments-annotations-overview/)
- [high] Redis pubsub Component spec: apiVersion `dapr.io/v1alpha1`, kind `Component`, spec.type `pubsub.redis`, version `v1`. Required metadata: `redisHost`. Optional: `redisPassword`, `redisDB`, `consumerID`, `enableTLS` (default "false"), `redisType` ("node"|"cluster", default "node").  (src: https://docs.dapr.io/reference/components-reference/supported-pubsub/setup-redis-pubsub/)
- [high] `redisDB` selects the Redis logical database after connecting; default is "0"; it is ONLY honored when redisType is "node" (ignored in cluster mode). This is the mechanism to isolate pubsub from the output-cache state store on a shared Redis instance.  (src: https://docs.dapr.io/reference/components-reference/supported-pubsub/setup-redis-pubsub/)
- [medium] consumerID supports template tags like `{podName}`, `{appID}`, `{namespace}`, `{uuid}` and is auto-generated when unset; for a stable consumer group across pod restarts, set it explicitly (e.g. to the app-id).  (src: https://docs.dapr.io/reference/components-reference/supported-pubsub/setup-redis-pubsub/)
- [high] ArgoCD installs a remote Helm chart via spec.source with repoURL (the Helm repo URL), chart, targetRevision (chart version), and helm.parameters / helm.valuesObject. Values precedence: parameters > valuesObject > values > valueFiles > chart values.yaml.  (src: https://argo-cd.readthedocs.io/en/latest/user-guide/helm/)
- [high] ArgoCD respects Helm's `crds/` folder by default (installs CRDs if not present); `spec.source.helm.skipCrds: true` disables that. ArgoCD does NOT order Helm-bundled CRDs via sync-waves — sync-wave annotations apply to discrete resources ArgoCD manages, so cross-Application ordering (CRDs+control-plane before Components) is the reliable control.  (src: https://argo-cd.readthedocs.io/en/latest/user-guide/helm/)

## Recommended config
### 1. Dapr control-plane ArgoCD Application (early sync-wave — owns the CRDs)
```yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: dapr
  namespace: argocd
  annotations:
    argocd.argoproj.io/sync-wave: "-1"   # before any Dapr Component CRs
spec:
  project: default
  source:
    repoURL: https://dapr.github.io/helm-charts/
    chart: dapr
    targetRevision: 1.18.0     # confirm exact patch via `helm search repo dapr --versions`
    helm:
      releaseName: dapr
      # skipCrds: false  (default) — let the chart install Component/Configuration CRDs
      valuesObject:
        global:
          ha:
            enabled: false      # single-node OrbStack: HA off
  destination:
    server: https://kubernetes.default.svc
    namespace: dapr-system
  syncPolicy:
    automated: { prune: true, selfHeal: true }
    syncOptions:
      - CreateNamespace=true
      - ServerSideApply=true        # avoids "metadata.annotations too long" on big CRDs
```

### 2. Catalog API Deployment — sidecar injection annotations (on the POD TEMPLATE, not the Deployment)
```yaml
spec:
  template:
    metadata:
      annotations:
        dapr.io/enabled: "true"
        dapr.io/app-id: "catalog-api"
        dapr.io/app-port: "8080"        # the HTTP port the minimal API listens on
        dapr.io/app-protocol: "http"    # default; ASP.NET minimal API over HTTP
        # optional ergonomics:
        # dapr.io/log-as-json: "true"
        # dapr.io/enable-metrics: "true"
```

### 3. Redis pubsub Component (later sync-wave — CRD must already exist)
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: pubsub                 # this becomes the Dapr pubsub component name your app references
  namespace: default           # same namespace as catalog-api
  annotations:
    argocd.argoproj.io/sync-wave: "1"
spec:
  type: pubsub.redis
  version: v1
  metadata:
    - name: redisHost
      value: "redis-master.default.svc.cluster.local:6379"  # in-cluster Redis Service:port
    - name: redisPassword
      value: ""                # or secretKeyRef to a Secret
    - name: redisDB
      value: "1"               # DB 1 for pubsub; output cache/state uses DB 0 — isolates them
    - name: redisType
      value: "node"            # node mode REQUIRED for redisDB to be honored
    - name: consumerID
      value: "catalog-api"     # pin to app-id so scaled replicas share one consumer group
    - name: enableTLS
      value: "false"
# scopes: ["catalog-api"]      # optional: restrict which app-ids may use this component
```

Verification commands:
```bash
helm repo add dapr https://dapr.github.io/helm-charts/ && helm repo update
helm search repo dapr --versions | head        # confirm 1.18.x patch
kubectl get crd | grep dapr.io                  # components/configurations/subscriptions/resiliencies/httpendpoints
kubectl get pods -n dapr-system                 # operator, placement, sidecar-injector, sentry
kubectl get components.dapr.io -A               # pubsub Component present
kubectl logs <catalog-pod> -c daprd | grep -i pubsub
```

## Open risks
- Version drift: design says 1.15, current latest is 1.18. Pin and re-check the exact patch at apply time; don't hard-code an unverified patch like 1.18.0 without `helm search repo dapr --versions`.
- ArgoCD + Helm-bundled CRD ordering is fragile. If the control-plane Application and the Component Application are in the SAME ArgoCD Application or same wave, the first sync can fail dry-run because `kind: Component` (components.dapr.io) doesn't exist yet. Mitigate with App-of-Apps + sync-waves (control plane wave < component wave) AND consider `syncOptions: SkipDryRunOnMissingResource=true` on the workload app for the very first bootstrap.
- redisDB isolation silently breaks if Redis is reconfigured to cluster mode — redisDB is ignored in cluster mode and pubsub keys would collide with the output-cache keyspace. Document that the single-Redis-with-DB-split scheme assumes node mode. A cleaner long-term isolation is two Redis instances/Services.
- Dapr pub/sub on Redis uses Redis Streams (consumer groups), which is distinct from Redis's SUBSCRIBE/PUBLISH. Confirm the in-cluster Redis is >=5.x (Streams support); the cache use-case has no such requirement, so verify the chosen Redis image/version satisfies both.
- Sidecar mTLS/Sentry: with the control plane installed, daprd establishes mTLS via Sentry by default. On a throwaway local OrbStack cluster this is fine, but if you disable HA/Sentry or mismatch trust anchors the sidecar will fail to start — keep Sentry enabled (default).
- consumerID auto-generation default behavior (whether it falls back to app-id vs a per-pod value) is documented only via the templated-metadata reference, rated medium confidence — explicitly setting consumerID removes this ambiguity, which the recommended config does.
- Aspire-to-Kubernetes translation: Aspire normally wires Dapr via `WithDaprSidecar()` and generates the resource graph. Hand-authoring Kustomize means you are NOT using Aspire's Dapr integration — ensure the env vars the Catalog API expects (ConnectionStrings__catalogdb/cache/photos, Keycloak__Authority, and the Dapr pubsub component name it publishes to) are provided independently; Aspire won't inject them in this topology.
- ServerSideApply is recommended for the Dapr chart because Dapr CRDs can exceed the client-side-apply 256KB annotation limit; if you omit it, CRD apply may fail with 'metadata.annotations: Too long'.


================================================================================

# AREA: OrbStack Kubernetes networking & storage (verification for GitOps/ArgoCD/Dapr micro-commerce deploy)

## Corrections to design
- MISCONCEPTION 'host-side orb.local domains only resolve on the host': WRONG. *.k8s.orb.local resolves from inside pods too (verified Keycloak case). The real problem is RELIABILITY, not reachability — in-pod resolution intermittently fails with EAI_AGAIN. So do NOT architect pod->pod traffic (Catalog API -> Keycloak token/JWKS, Next.js server-side fetch -> Catalog) around *.k8s.orb.local.
- RISKY ASSUMPTION 'use one *.k8s.orb.local hostname for both browser and pods and call it done': WRONG/fragile. The browser hitting keycloak.k8s.orb.local needs that name to validate against Keycloak's issuer, AND the Catalog API pod validates the JWT issuer against Keycloak__Authority. If the pod resolves keycloak.k8s.orb.local via the flaky path you get sporadic auth failures. CORRECT approach: keep the SAME hostname string everywhere (so issuer matches) but make CoreDNS resolve keycloak.k8s.orb.local to the in-cluster ClusterIP for pods — via a CoreDNS rewrite to the Service FQDN. This is the canonical fix and is more robust than hostAliases.
- CORRECTION on hostAliases vs CoreDNS: hostAliases is per-pod, requires a hardcoded ClusterIP (which is not stable across redeploys unless you pin it), and must be repeated on every Deployment — a poor fit for GitOps where you want one declarative source of truth. PREFER a CoreDNS rewrite (cluster-wide, no hardcoded IP because it targets the Service FQDN which Kubernetes resolves). Use hostAliases only as a last-resort per-pod patch.
- CAUTION on editing OrbStack/K3s CoreDNS: the CoreDNS Corefile is delivered via a managed ConfigMap (coredns in kube-system) and OrbStack/K3s may reconcile it on cluster restart. For GitOps durability, prefer the K3s-style 'NodeHosts'/custom Corefile import OR have ArgoCD own a CoreDNS custom ConfigMap (coredns-custom) rather than hand-editing 'kubectl edit cm coredns -n kube-system', which can be overwritten. Verify after 'orb restart k8s' that the rewrite survived.
- CORRECTION on storage assumption: do NOT assume you must author a StorageClass — 'local-path' already exists and is default. Postgres/Keycloak/Azurite PVCs should simply omit storageClassName (or set it to local-path). Authoring a duplicate default StorageClass in Kustomize would create two defaults and a binding ambiguity.
- NOTE on the EAI_AGAIN bug interacting with Dapr: Dapr service invocation and pubsub use cluster.local Service DNS internally, NOT *.k8s.orb.local, so Dapr itself is unaffected. The *.k8s.orb.local fragility only bites app code that hardcodes the external hostname for in-cluster calls (e.g. Keycloak__Authority, server-side Next.js fetch of the API). Route those over cluster.local Service names or the CoreDNS rewrite.

## Verified facts
- [high] OrbStack exposes Services to the host with NO port-forwarding. LoadBalancer and Ingress services are reachable at the wildcard domain *.k8s.orb.local (e.g. keycloak.k8s.orb.local, web.k8s.orb.local), and the cluster API server is at k8s.orb.local. ClusterIP services are reachable from the Mac by their cluster IP directly, and NodePort services are reachable at localhost:<nodePort>. cluster.local DNS names (svc.namespace.svc.cluster.local) also resolve from the Mac.  (src: https://docs.orbstack.dev/kubernetes/)
- [high] By default NodePort and LoadBalancer ports are only accessible to localhost (the Mac). To reach them from other LAN devices you must enable 'Expose services to local network devices' in OrbStack Settings > Kubernetes.  (src: https://docs.orbstack.dev/kubernetes/)
- [high] *.k8s.orb.local DOES resolve from INSIDE pods/containers in the OrbStack cluster (not host-only). Confirmed by a real Keycloak-in-cluster report where an app pod resolved keycloak.k8s.orb.local to an OrbStack-managed address (IPv6 fd07:... / IPv4 198.19.248.3 in the 198.19.x.x range). HOWEVER it is unreliable: a separate, recent confirmed bug (OrbStack 2.0.5, macOS 26.1, Jan 2026) shows in-pod requests to *.k8s.orb.local failing intermittently with getaddrinfo EAI_AGAIN while the same request from the Mac terminal succeeds.  (src: https://github.com/orbstack/orbstack/issues/1790)
- [high] The in-pod EAI_AGAIN failure against *.k8s.orb.local is a known, still-open issue. This makes *.k8s.orb.local unsafe to rely on for pod->pod (e.g. Catalog API pod -> Keycloak) traffic.  (src: https://github.com/orbstack/orbstack/issues/2306)
- [high] Dynamic PVC provisioning works out of the box. OrbStack's K8s ships Rancher's Local Path Provisioner as the default StorageClass, named 'local-path', with provisioner 'rancher.io/local-path' and volumeBindingMode WaitForFirstConsumer (a PVC with no storageClassName binds to it automatically). This is the standard K3s default-storage behavior.  (src: https://github.com/orgs/orbstack/discussions/1448)
- [medium] PersistentVolumes with reclaimPolicy Delete are NOT actually freed from disk on PVC deletion until OrbStack itself is restarted/closed — a reported quirk to be aware of when iterating on DB volumes.  (src: https://github.com/orbstack/orbstack/issues/1858)
- [high] Kubernetes is started/managed via the CLI: 'orb start k8s', 'orb stop k8s', 'orb restart k8s', 'orb delete k8s'. The cluster can also be enabled via the GUI under OrbStack Settings > Kubernetes. kubectl is included and its context is wired up automatically.  (src: https://docs.orbstack.dev/kubernetes/)
- [high] CoreDNS hosts plugin inline syntax: a 'hosts { <IP> <name>\n fallthrough }' block inside a server block adds entries treated as additional /etc/hosts content; 'fallthrough' passes unmatched queries to the next plugin (e.g. kubernetes).  (src: https://coredns.io/plugins/hosts/)
- [high] CoreDNS rewrite plugin syntax: 'rewrite [continue|stop] name [exact|prefix|suffix|substring|regex] FROM TO [answer auto]'. The 'answer auto' option rewrites the answer section back to the original name so resolvers don't reject the mismatch. This lets one hostname be rewritten to an in-cluster Service FQDN.  (src: https://coredns.io/plugins/rewrite/)

## Recommended config
RECOMMENDED: one hostname, resolves correctly both in-pod and from the host browser.

Keep external hostnames in *.k8s.orb.local for the browser (web.k8s.orb.local, keycloak.k8s.orb.local). For pods, override resolution via a CoreDNS rewrite so the SAME name maps to the in-cluster ClusterIP — robust, no hardcoded IP, GitOps-friendly.

Option A (preferred) — ArgoCD-managed coredns-custom ConfigMap (K3s honors this; OrbStack k8s is K3s-based). Place rewrites BEFORE kubernetes plugin:

  apiVersion: v1
  kind: ConfigMap
  metadata:
    name: coredns-custom
    namespace: kube-system
  data:
    orbstack.server: |
      rewrite stop {
        name exact keycloak.k8s.orb.local keycloak.platform.svc.cluster.local
        answer auto
      }
      rewrite stop {
        name exact catalog-api.k8s.orb.local catalog-api.platform.svc.cluster.local
        answer auto
      }

(Replace 'platform' with the namespace each Service lives in. The Service name + .svc.cluster.local is stable across redeploys, unlike a ClusterIP.)

Option B — direct Corefile edit (use if coredns-custom import is not wired). Edit the coredns ConfigMap and add a rewrite line in the main server block above 'kubernetes':

  kubectl -n kube-system edit configmap coredns
  # inside the Corefile '.:53 { ... }' block, ABOVE the 'kubernetes cluster.local ...' line:
  #   rewrite stop name exact keycloak.k8s.orb.local keycloak.platform.svc.cluster.local answer auto
  kubectl -n kube-system rollout restart deployment coredns
  # WARNING: re-verify after 'orb restart k8s' — managed Corefile may be reconciled.

Option C (last resort, per-pod) — hostAliases on the Deployment (needs a pinned ClusterIP; brittle):
  spec:
    template:
      spec:
        hostAliases:
          - ip: "10.43.x.x"   # pin Keycloak Service via spec.clusterIP in the Service manifest
            hostnames: ["keycloak.k8s.orb.local"]

ENV WIRING for Catalog API (issuer must match what the browser sees):
  Keycloak__Authority = http://keycloak.k8s.orb.local/realms/<realm>
  # with Option A/B, the pod resolves this to the ClusterIP automatically.

STORAGE — just rely on the default; do not author a StorageClass:
  apiVersion: v1
  kind: PersistentVolumeClaim
  metadata: { name: catalogdb-data, namespace: platform }
  spec:
    accessModes: ["ReadWriteOnce"]
    resources: { requests: { storage: 5Gi } }
    # storageClassName omitted -> binds to default 'local-path' (rancher.io/local-path)

ENABLE / START CLUSTER:
  orb start k8s        # start (or enable via Settings > Kubernetes the first time)
  orb status           # check
  orb restart k8s      # restart
  kubectl config use-context orbstack
  kubectl get storageclass    # expect: local-path (default)  rancher.io/local-path

INGRESS NOTE: *.k8s.orb.local points at LoadBalancer/Ingress. For a single browser entrypoint, deploy an ingress controller (or use Service type LoadBalancer) and host-route web.k8s.orb.local / keycloak.k8s.orb.local. ArgoCD UI itself can be exposed the same way (argocd.k8s.orb.local).

## Open risks
- In-pod resolution of *.k8s.orb.local is intermittently broken (EAI_AGAIN, open issue #2306 on OrbStack 2.0.x). The CoreDNS rewrite mitigates this for named services, but any code path that constructs a *.k8s.orb.local URL NOT covered by an explicit rewrite rule (e.g. dynamic SAS photo URLs to Azurite, or a forgotten service) can still fail from inside pods. Enumerate every external-hostname-used-from-a-pod and add a rewrite for each.
- The coredns ConfigMap in OrbStack's K3s is managed; a hand-edited Corefile (Option B) may be reverted on 'orb restart k8s' or OrbStack upgrade. Must verify survival after restart, and prefer the coredns-custom import (Option A) which K3s is designed to merge.
- PV reclaim quirk (#1858): PVs with Delete policy aren't reclaimed from disk until OrbStack restarts. Repeated PVC create/delete cycles during GitOps experimentation can leak disk; budget for periodic 'orb restart' or manual cleanup.
- IPv6 quirk (#1790): *.k8s.orb.local may return an AAAA (fd07:...) record first; if pods/runtime have IPv6 disabled, connections can fail before falling back to IPv4. The CoreDNS rewrite to a cluster.local Service sidesteps this (Service ClusterIP is IPv4 in a SingleStack cluster).
- Could not retrieve an authoritative OrbStack doc statement that the cluster is literally K3s; the local-path default StorageClass and coredns-custom support strongly indicate K3s, but treat 'coredns-custom is honored' as an assumption to validate on the actual cluster before relying on it in ArgoCD.
- Azurite SAS upload flow: the browser receives a pre-signed blob URL from the API. If the API mints that URL with an in-cluster hostname (azurite...svc.cluster.local) the browser can't reach it; if it mints *.k8s.orb.local the upload must traverse host-side resolution. Decide the Azurite blob endpoint hostname deliberately and ensure it resolves both from the browser (host) and is signature-consistent — this is the most likely networking footgun in the stack.
- ArgoCD sync waves: ensure CoreDNS rewrite (kube-system, infra wave) and Keycloak realm import land BEFORE the Catalog API pod starts validating issuers, or first-boot auth will flap until reconciliation settles.


================================================================================

# AREA: ArgoCD App-of-Apps + bootstrap on a fresh local OrbStack Kubernetes cluster

## Corrections to design
- The locked decision says 'ArgoCD pulled as an upstream Helm chart referenced by an ArgoCD Application.' That is fine for ongoing management but it is NOT the bootstrap method and must not be the first step. ArgoCD cannot install itself from nothing. Correct order: (1) imperatively install ArgoCD ONCE (kubectl apply install.yaml, or `helm install argocd argo/argo-cd -n argocd --create-namespace`); (2) apply the root App-of-Apps Application; (3) OPTIONALLY include a child Application that points back at the argo/argo-cd Helm chart so ArgoCD self-manages future upgrades. If you do self-manage, you must reconcile the bootstrap install's settings with the chart's values or ArgoCD will detect drift and selfHeal-fight itself on the first sync. For a single local OrbStack cluster, self-management adds real risk for little benefit. Recommendation: bootstrap with `helm install` (so the install is already chart-shaped and value-controlled), and only optionally add self-management later.
- Do NOT enable `selfHeal: true` on the Application that manages ArgoCD itself, and ideally not on the Dapr control-plane Application, before the cluster is stable. A self-healing ArgoCD-manages-ArgoCD loop can interrupt the very controller doing the sync. Bring these up with manual or prune-only sync first, confirm healthy, then turn on selfHeal.
- `kubectl apply -f https://github.com/argoproj/argo-cd/blob/stable/manifests/install.yaml` (a github.com /blob/ URL) returns an HTML page, not YAML, and will fail. Use the raw URL: `https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml`. The earlier doc-extracted command had the wrong host.
- Sync waves do NOT cross Application boundaries for the resources INSIDE child apps. A `sync-wave` annotation only orders resources within the SAME sync operation of a SINGLE Application. To order Dapr vs infra vs workloads you must annotate the child Application CRs themselves (which all live in the root app's one sync operation), OR rely on each child app's health-gating. Putting sync-wave on a Deployment in app A and expecting it to wait for a Deployment in app B will not work. Use child-Application-level waves plus ArgoCD's health checks (a child app does not report Healthy until its workloads are ready, and the next wave's child app won't be created until the current wave is healthy).
- EF Core auto-migrates on boot for Catalog API. With App-of-Apps + automated sync this means Catalog must NOT start before Postgres is accepting connections. sync-wave alone does not guarantee Postgres is READY (wave delay is only 2s) - it guarantees ordering of creation, not readiness. Put Postgres in an earlier wave AND give the Catalog Deployment a proper readiness/startup probe + the Postgres a Service that the app waits on (initContainer or app-level retry). Relying on wave ordering for DB readiness is a classic bug.
- Keycloak `start-dev --import-realm` and Azurite are dev-only single-replica stateful workloads. Do not set `allowEmpty: true` casually and avoid `prune: true` racing with PVC deletion; an accidental prune can wipe the realm import config / blob data. Keep prune on for app workloads but be deliberate about stateful infra.
- Dapr Components (pubsub on Redis, the SAS/blob bindings) are Dapr CRDs that only exist AFTER the Dapr control plane CRDs are installed. They must be in a strictly later wave than the Dapr Helm Application, and Redis must exist before the pubsub Component is usable. Authoring Components as your own Kustomize and Dapr control plane as the upstream chart (as decided) is correct - just sequence them: Dapr chart wave < Components wave.

## Verified facts
- [high] ArgoCD itself cannot be GitOps-deployed from nothing; the canonical bootstrap is to install ArgoCD imperatively once (kubectl apply or helm install), then have ArgoCD manage everything else (including, optionally, itself) declaratively. The official non-HA manifest install is: `kubectl create namespace argocd` then `kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml`. The `stable` branch tracks the latest stable release tag.  (src: https://argo-cd.readthedocs.io/en/stable/getting_started/)
- [high] The official manifest ships in several variants: `install.yaml` (standard, cluster-admin via ClusterRole/ClusterRoleBinding), `ha/install.yaml` (same but multi-replica components for HA), and `namespace-install.yaml` (namespace-level privileges only, no cluster roles). The CRDs are NOT included in namespace-install.yaml and must be applied separately via `kubectl apply --server-side --force-conflicts -k https://github.com/argoproj/argo-cd/manifests/crds?ref=stable`.  (src: https://argo-cd.readthedocs.io/en/stable/operator-manual/installation/)
- [high] The community Helm chart is `argo/argo-cd` from repo `https://argoproj.github.io/argo-helm`. It exposes `crds.install` (default true). Since chart v5.2.0 the CRDs were moved from `<chart>/crds` to `<chart>/templates` so Helm can upgrade them (Helm by design will not upgrade CRDs in the crds/ folder). Chart version is decoupled from app version (e.g. chart 9.5.x bundles ArgoCD v3.x).  (src: https://github.com/argoproj/argo-helm/tree/main/charts/argo-cd)
- [high] Latest stable ArgoCD as of mid-2026 is the 3.x line (v3.3/v3.4 stable; v3.5 in development targeting Aug 2026 GA). The argo-cd Helm chart latest is ~9.5.x (e.g. 9.5.21 on ArtifactHub).  (src: https://artifacthub.io/packages/helm/argo/argo-cd)
- [high] App-of-Apps: a single root Application points its source at a Git path/directory containing child Application manifests. ArgoCD applies those Application CRs, each of which in turn syncs its own target. Official root example uses `source.path: apps`, `repoURL`, `targetRevision: HEAD`, `destination.server: https://kubernetes.default.svc`, `destination.namespace: argocd`, `project: default`, and `syncPolicy.automated.prune: true`.  (src: https://argo-cd.readthedocs.io/en/stable/operator-manual/cluster-bootstrapping/)
- [high] To cascade-delete child apps and all their resources when a parent is deleted, add the finalizer `resources-finalizer.argocd.argoproj.io` to the child (and/or root) Application metadata. Without it, deleting an Application orphans its managed resources.  (src: https://argo-cd.readthedocs.io/en/stable/operator-manual/cluster-bootstrapping/)
- [high] Sync waves are set via the `argocd.argoproj.io/sync-wave` annotation; value is a string integer. Default wave is 0; negative waves run before wave 0. ArgoCD orders by (1) phase, (2) wave ascending, (3) kind (namespaces first), (4) name. It applies the lowest out-of-sync wave, waits, then proceeds. Default inter-wave delay is 2 seconds, configurable via env var `ARGOCD_SYNC_WAVE_DELAY` on the application-controller.  (src: https://argo-cd.readthedocs.io/en/stable/user-guide/sync-waves/)
- [medium] Sync waves order child Applications too: because each child is itself a Kubernetes resource (Application CR) inside the root app, putting `argocd.argoproj.io/sync-wave` on the child Application manifest's metadata orders WHEN that Application object is created by the root. This is the standard mechanism to stage Dapr -> infra -> components -> workloads.  (src: https://argo-cd.readthedocs.io/en/stable/operator-manual/cluster-bootstrapping/)
- [high] An Application whose source is an upstream Helm chart uses `source.chart` (NOT `source.path`), `source.repoURL` pointing at the Helm repo, `source.targetRevision` as the chart version, and `source.helm.valuesObject`/`values`/`valueFiles`/`releaseName`. Value precedence: parameters > valuesObject > values > valueFiles > chart values.yaml.  (src: https://argo-cd.readthedocs.io/en/stable/user-guide/helm/)
- [high] An Application whose source is a Kustomize dir in the same git repo uses `source.repoURL` (the git URL), `source.path` (dir containing kustomization.yaml), and `source.targetRevision` (branch/tag). ArgoCD auto-detects kustomize when a kustomization.yaml is present.  (src: https://argo-cd.readthedocs.io/en/stable/user-guide/kustomize/)
- [high] Automated sync policy fields live under `spec.syncPolicy.automated`: `prune` (delete resources removed from git), `selfHeal` (revert manual cluster drift back to git), `allowEmpty` (permit pruning to zero resources). Retry under `spec.syncPolicy.retry`. selfHeal does NOT prune by default; prune is independent.  (src: https://argo-cd.readthedocs.io/en/stable/user-guide/auto_sync/)
- [high] `syncOptions` is a list under `spec.syncPolicy`. `CreateNamespace=true` makes ArgoCD create the destination namespace if missing. `ServerSideApply=true` switches to server-side apply (needed for large CRDs that exceed the kubectl client-side-apply annotation size limit, e.g. some Helm-rendered CRDs).  (src: https://argo-cd.readthedocs.io/en/stable/user-guide/sync-options/)
- [high] Dapr control plane installs from Helm repo `https://dapr.github.io/helm-charts/`, chart `dapr/dapr`, into namespace `dapr-system`, with `--create-namespace`. Chart version is pinned to the runtime version (chart 1.16 = Dapr runtime 1.16). Latest GA line is 1.16 (released Sep 2025); 1.18-rc exists on ArtifactHub.  (src: https://github.com/dapr/dapr/blob/master/charts/dapr/README.md)
- [high] ApplicationSet (cluster/git generators) is the modern alternative ArgoCD officially recommends over hand-written App-of-Apps for scaling to many clusters/apps, but plain App-of-Apps remains fully supported and is simpler for a single local cluster.  (src: https://argo-cd.readthedocs.io/en/stable/operator-manual/cluster-bootstrapping/)

## Recommended config
## 0. Bootstrap order (run once, imperatively) — ArgoCD can't deploy itself from nothing

Option A (recommended — install is already chart-shaped/value-controlled):
```bash
helm repo add argo https://argoproj.github.io/argo-helm
helm repo update
helm install argocd argo/argo-cd \
  --namespace argocd --create-namespace \
  --version 9.5.x            # bundles ArgoCD v3.x; pin exact version
# then apply the single root app:
kubectl apply -n argocd -f bootstrap/root-app.yaml
```

Option B (plain manifest — note RAW url, not /blob/):
```bash
kubectl create namespace argocd
kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml
kubectl apply -n argocd -f bootstrap/root-app.yaml
```

## 1. Root App-of-Apps (bootstrap/root-app.yaml) — points at a dir of child Application manifests
```yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: root
  namespace: argocd
  finalizers:
    - resources-finalizer.argocd.argoproj.io   # cascade-delete children
spec:
  project: default
  source:
    repoURL: https://github.com/<you>/micro-commerce.git
    path: deploy/argocd/apps        # dir holding the child Application YAMLs
    targetRevision: main
    directory:
      recurse: true                 # pick up nested child app files
  destination:
    server: https://kubernetes.default.svc
    namespace: argocd               # children are Application CRs -> live in argocd ns
  syncPolicy:
    automated:
      prune: true
      selfHeal: true
    syncOptions:
      - CreateNamespace=true
```

## 2. Sane sync-wave numbers (annotate the CHILD Application manifests)
- wave "-2": Dapr control plane (upstream Helm chart child app) + ArgoCD self-mgmt (optional, no selfHeal)
- wave "-1": cluster-scoped infra CRDs/operators if any (skip if none)
- wave "0":  stateful infra — Postgres, Redis, Keycloak, Azurite (your Kustomize)
- wave "1":  Dapr Components (pubsub/Redis, blob bindings) — needs Dapr CRDs (wave -2) + Redis (wave 0)
- wave "2":  app workloads — Catalog API, Next.js web
```yaml
metadata:
  annotations:
    argocd.argoproj.io/sync-wave: "0"   # on each child Application
```
Note: waves order WHEN each child Application object is created. ArgoCD waits for the current wave's child apps to be Healthy before creating the next wave. This is your cross-app sequencing — sync-wave on resources INSIDE a single app does not reach across apps. (Inter-wave delay default 2s via ARGOCD_SYNC_WAVE_DELAY; that is delay, not readiness — still use readiness probes for Postgres/Catalog.)

## 3. Child app: upstream Helm chart (Dapr) — deploy/argocd/apps/dapr.yaml
```yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: dapr
  namespace: argocd
  annotations:
    argocd.argoproj.io/sync-wave: "-2"
  finalizers:
    - resources-finalizer.argocd.argoproj.io
spec:
  project: default
  source:
    repoURL: https://dapr.github.io/helm-charts/   # Helm repo
    chart: dapr                                     # chart, NOT path
    targetRevision: 1.16.x                          # = Dapr runtime version
    helm:
      releaseName: dapr
      valuesObject:
        global:
          ha:
            enabled: false       # single local node
  destination:
    server: https://kubernetes.default.svc
    namespace: dapr-system
  syncPolicy:
    automated:
      prune: true
      # selfHeal: leave OFF initially for control plane
    syncOptions:
      - CreateNamespace=true
      - ServerSideApply=true     # Dapr CRDs are large -> avoid client-side-apply size limit
```

## 4. Child app: Kustomize dir in THIS repo (e.g. Catalog API) — deploy/argocd/apps/catalog.yaml
```yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: catalog-api
  namespace: argocd
  annotations:
    argocd.argoproj.io/sync-wave: "2"
  finalizers:
    - resources-finalizer.argocd.argoproj.io
spec:
  project: default
  source:
    repoURL: https://github.com/<you>/micro-commerce.git
    path: deploy/k8s/catalog        # dir with kustomization.yaml (auto-detected)
    targetRevision: main
  destination:
    server: https://kubernetes.default.svc
    namespace: micro-commerce
  syncPolicy:
    automated:
      prune: true
      selfHeal: true
    syncOptions:
      - CreateNamespace=true
```

## 5. Automated sync policy summary
- App workloads + Components: `automated: { prune: true, selfHeal: true }` — full GitOps reconciliation.
- Dapr control plane + (optional) self-managed ArgoCD: bring up with `prune: true` only, add `selfHeal` after verified healthy.
- Stateful infra (Postgres/Keycloak/Azurite): `prune: true` but be deliberate; do NOT set `allowEmpty: true`.
- Add `CreateNamespace=true` on every app that targets a namespace ArgoCD must create.

## Open risks
- EF Core boot-time auto-migration + automated sync is a readiness hazard. sync-wave guarantees Postgres is CREATED before Catalog, not READY (2s delay only). Catalog will crash-loop on first boot if Postgres isn't accepting connections yet. Mitigate with a startup/readiness probe on Catalog and DB-connection retry in ServiceDefaults, or an initContainer that waits for Postgres. Do not treat wave ordering as a readiness barrier.
- Self-managing ArgoCD via a child Helm Application can deadlock or drift-fight if the bootstrap install (whether kubectl manifest or helm) used different values than the chart child app declares. If you self-manage, bootstrap with helm using the SAME values file the child app references, and disable selfHeal on that app until reconciled.
- Dapr Components depend on Dapr CRDs existing first. If ArgoCD tries to apply a Component (Component/Subscription CRD) before the Dapr control-plane chart's CRDs land, the child app errors. Waves help, but also ensure the Dapr app reports Healthy (control plane pods ready) before the Components wave — verify Dapr's health-check coverage in ArgoCD, as custom CRDs sometimes need a custom health.lua to report Healthy and not block the next wave forever.
- Local OrbStack k8s is single-node: set Dapr global.ha.enabled=false and avoid HA replica counts, or pods will stay Pending on anti-affinity / insufficient resources. The HA ArgoCD manifest (ha/install.yaml) is wrong for this environment — use non-HA.
- GHCR images are private by default. ArgoCD only deploys manifests; the cluster's kubelet pulls images. You need an imagePullSecret (GHCR PAT) referenced by the workload ServiceAccounts, or the Catalog/web pods will ImagePullBackOff. This is outside ArgoCD's sync and easy to forget.
- Keycloak realm import (start-dev --import-realm) reads a realm JSON, typically mounted from a ConfigMap. If that ConfigMap is large it can exceed limits or churn; and start-dev is non-persistent — restarts re-import. prune racing with the Keycloak PVC/ConfigMap during a sync could wipe realm state. Keep Keycloak in an earlier, stable wave and avoid aggressive prune timing.
- ServerSideApply=true is recommended for the Dapr chart CRDs but can surface field-manager conflicts on subsequent syncs; if you see conflicts, add Validate=false only where required, and prefer applying CRDs as a separate earlier step rather than mixing with chart templates.
- Children of the root app are Application CRs in the argocd namespace by default. If you ever move to 'Applications in any namespace', RBAC/AppProject sourceNamespaces config is required or the children silently won't reconcile.


================================================================================

# AREA: Keycloak 26 on OrbStack Kubernetes: single-issuer-URL OIDC alignment for browser + Next.js pod + .NET API pod (hostname-v2, realm import, proxy, JWKS)

## Corrections to design
- WRONG ASSUMPTION (implied by 'start-dev so we don't set hostname'): start-dev does NOT solve the single-issuer problem - it makes it WORSE. With start-dev, --hostname-strict false is the default, so Keycloak dynamically derives the issuer from each request's Host/X-Forwarded headers. The browser (going through ingress as keycloak.micro-commerce.k8s.orb.local) and the API pod (if it talked to the Service DNS keycloak.svc:8080) would receive tokens/metadata with DIFFERENT iss values -> ValidateIssuer fails with IDX10205. FIX: even though you run start-dev, you MUST explicitly set KC_HOSTNAME=http://keycloak.micro-commerce.k8s.orb.local to pin ONE issuer. start-dev only governs DB/HTTPS/caching, not the requirement to pin hostname.
- WRONG ASSUMPTION ('the API pod can just use the external issuer URL like the browser does'): It can only do so if that hostname RESOLVES and is REACHABLE from inside the pod. On OrbStack, *.k8s.orb.local resolution from inside pods is unreliable (EAI_AGAIN, see orbstack issue #2306, with a reported Keycloak case). FIX (pick one): (a) Add a CoreDNS rewrite/hosts entry so keycloak.micro-commerce.k8s.orb.local resolves to the ingress-controller ClusterIP from inside the cluster - cleanest, keeps a single issuer string everywhere; or (b) add hostAliases on the catalog-api pod mapping the issuer host to the ingress LB IP; or (c) keep KC_HOSTNAME as the external URL but set KC_HOSTNAME_BACKCHANNEL_DYNAMIC=true and point the API's MetadataAddress at an in-cluster URL - but note the API still validates iss against the EXTERNAL string, so you'd then also need ValidIssuer override. Option (a) is recommended because it requires zero app changes.
- WRONG ASSUMPTION ('http issuer is fine, no proxy settings needed'): If Keycloak sits behind an ingress (NGINX/Traefik) that terminates and Keycloak is reached as keycloak.micro-commerce.k8s.orb.local while the pod only knows its own Service host, you need KC_PROXY_HEADERS=xforwarded so Keycloak trusts the X-Forwarded-* headers, AND - because you set KC_HOSTNAME to a full http:// URL and KC_HTTP_ENABLED=true - Keycloak will happily mint http issuers. That's acceptable for LOCAL only. The repo's API sets RequireHttpsMetadata=false only in Development/Testing, so the http issuer + http metadata fetch is consistent. In any non-local environment this must become https and RequireHttpsMetadata stays true.
- WRONG ASSUMPTION ('realm.json in a ConfigMap, mount and go'): two gotchas. (1) ConfigMap is capped at ~1 MiB - a realm export with users can exceed it; keep the JSON to realm + clients + roles (and seed users via a tiny set or skip). (2) --import-realm is import-ONCE: if Keycloak's DB already has the realm (e.g. you used a PersistentVolume for Postgres/H2), the mount is silently ignored on restart. For repeatable GitOps, either run Keycloak ephemerally (no persisted realm, re-import every boot) or use the Keycloak Operator's KeycloakRealmImport CR. The audience matters too: the API expects Audience='catalog-api', so the realm JSON must define a catalog-api client/audience mapper or every token fails audience validation.
- CLARIFICATION on Audience: the repo sets options.Audience = "catalog-api". Keycloak does NOT put an aud claim for a client automatically unless you add an Audience protocol mapper (or the client is treated as a resource). Ensure the realm JSON includes an audience mapper that injects 'catalog-api' into aud, or the API returns 401 (IDX10214 audience invalid) even with a correct issuer.

## Verified facts
- [high] Keycloak 26 uses the hostname-v2 provider. KC_HOSTNAME accepts a full URL (scheme+host[+port][+path]) or just a hostname. When set to a full URL like http://keycloak.micro-commerce.k8s.orb.local, that URL becomes the authoritative base for tokens (the iss claim), redirect URLs, and the OIDC Discovery Document at realms/{realm}/.well-known/openid-configuration (which in turn sets jwks_uri). This is exactly what forces a single issuer string.  (src: https://www.keycloak.org/server/hostname)
- [high] In the dev profile (start-dev), --hostname-strict false is the DEFAULT. So start-dev does NOT require KC_HOSTNAME at all - it will dynamically resolve the issuer from request headers (Host / X-Forwarded-*). That dynamic resolution is the ROOT CAUSE of the single-issuer problem: the issuer becomes whatever host each caller used, so browser vs in-cluster callers get different iss values. Setting KC_HOSTNAME explicitly (a full URL) pins one issuer for everyone.  (src: https://www.keycloak.org/server/hostname)
- [high] KC_HOSTNAME_STRICT default is true (production). KC_HOSTNAME_BACKCHANNEL_DYNAMIC default is false; when true Keycloak resolves backchannel URLs (issuer/JWKS for internal callers) dynamically from request headers while keeping the fixed public frontchannel hostname - this requires KC_HOSTNAME to be a full URL. KC_HOSTNAME_ADMIN exists for serving the admin console on a different address.  (src: https://www.keycloak.org/server/hostname)
- [high] Behind a TLS-terminating reverse proxy / ingress, Keycloak needs KC_PROXY_HEADERS set to either 'xforwarded' (parses X-Forwarded-For/Proto/Host/Port/Prefix) or 'forwarded' (RFC 7239 Forwarded header), PLUS KC_HTTP_ENABLED=true so Keycloak accepts plain HTTP from the proxy after edge TLS termination. KC_PROXY=edge is deprecated; its replacement is KC_PROXY_HEADERS + KC_HTTP_ENABLED=true (both required).  (src: https://www.keycloak.org/server/reverseproxy)
- [high] --import-realm imports any realm config file from the data/import directory (container path /opt/keycloak/data/import). If the realm already exists, the import is SKIPPED to avoid losing state across restarts - so a ConfigMap-mounted realm.json is import-once per persistent volume; to force re-import you must run the explicit kc.sh import command or start from a fresh DB.  (src: https://www.keycloak.org/server/importExport)
- [high] A Kubernetes ConfigMap (and Secret) is hard-capped at ~1 MiB because objects are stored in etcd, which enforces a ~1MB per-value limit. A typical realm export with users/clients can exceed this. Keep the mounted realm.json lean (clients + roles + realm settings, minimal/no users) to stay under 1 MiB; otherwise use an initContainer that fetches the file, an emptyDir, or the Keycloak Operator's KeycloakRealmImport CR.  (src: https://able8.medium.com/why-k8s-secret-and-configmap-are-limited-to-1mib-in-size-ba79d86b0372)
- [high] ASP.NET Core Microsoft.AspNetCore.Authentication.JwtBearer auto-discovers signing keys: on first request it fetches {Authority}/.well-known/openid-configuration, reads jwks_uri from it, then loads the JWKS. By default ValidateIssuer=true and the token's iss must equal the Authority/the discovery doc's issuer. Therefore the API pod must (a) be able to RESOLVE and reach the issuer host to fetch metadata, and (b) the fetched issuer string must equal the iss in browser-minted tokens. This is the core single-issuer constraint on the API side.  (src: https://github.com/dotnet/aspnetcore/issues/24309)
- [high] OrbStack exposes LoadBalancer/Ingress services at the wildcard *.k8s.orb.local from the macOS host, and supports standard service.namespace.svc.cluster.local in-cluster DNS. The cluster API server is at k8s.orb.local.  (src: https://docs.orbstack.dev/kubernetes/)
- [high] Resolving *.k8s.orb.local from INSIDE pods is NOT reliably guaranteed - there are open OrbStack issues reporting EAI_AGAIN / resolution failures for *.k8s.orb.local from within containers, including a specific Keycloak (keycloak.k8s.orb.local) case. The documented workarounds are pod hostAliases or a CoreDNS hosts/rewrite entry mapping the issuer host to the ingress/LB IP. This directly threatens the .NET API's ability to fetch JWKS at the external issuer host.  (src: https://github.com/orbstack/orbstack/issues/2306)

## Recommended config
## Single source of truth: ONE issuer string everywhere
ISSUER = http://keycloak.micro-commerce.k8s.orb.local/realms/microcommerce

### 1) Keycloak 26.3.3 Deployment (start-dev, but hostname pinned)
```yaml
apiVersion: apps/v1
kind: Deployment
metadata: { name: keycloak, namespace: micro-commerce }
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
            # --- THE single-issuer pin (full URL => sets iss + discovery + jwks_uri) ---
            - { name: KC_HOSTNAME, value: "http://keycloak.micro-commerce.k8s.orb.local" }
            # start-dev defaults hostname-strict=false; pinning KC_HOSTNAME overrides dynamic iss.
            # --- behind ingress (edge), trust forwarded headers + allow plain HTTP ---
            - { name: KC_PROXY_HEADERS, value: "xforwarded" }
            - { name: KC_HTTP_ENABLED, value: "true" }
            # --- bootstrap admin (KEYCLOAK_ADMIN is deprecated in 26) ---
            - { name: KC_BOOTSTRAP_ADMIN_USERNAME, value: "admin" }
            - valueFrom: { secretKeyRef: { name: keycloak-admin, key: password } }
              name: KC_BOOTSTRAP_ADMIN_PASSWORD
          ports: [ { containerPort: 8080, name: http } ]
          volumeMounts:
            - { name: realm-import, mountPath: /opt/keycloak/data/import, readOnly: true }
          readinessProbe: { httpGet: { path: /realms/microcommerce/.well-known/openid-configuration, port: 8080 }, initialDelaySeconds: 20, periodSeconds: 10 }
      volumes:
        - name: realm-import
          configMap: { name: keycloak-realm }   # data key: microcommerce-realm.json (<1MiB)
---
apiVersion: v1
kind: Service
metadata: { name: keycloak, namespace: micro-commerce }
spec:
  selector: { app: keycloak }
  ports: [ { port: 8080, targetPort: 8080 } ]
```
Ingress host MUST equal the KC_HOSTNAME host:
```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: keycloak
  namespace: micro-commerce
  annotations: { nginx.ingress.kubernetes.io/proxy-buffer-size: "16k" }  # KC headers can be large
spec:
  rules:
    - host: keycloak.micro-commerce.k8s.orb.local
      http: { paths: [ { path: /, pathType: Prefix, backend: { service: { name: keycloak, port: { number: 8080 } } } } ] }
```

### 2) In-cluster resolution fix (so the API can fetch JWKS at the SAME issuer host)
Add a CoreDNS rewrite so the issuer host resolves to the ingress controller from inside the cluster (replace IP with `kubectl -n <ingress-ns> get svc <ingress-controller> -o jsonpath='{.spec.clusterIP}'`):
```yaml
# kubectl -n kube-system edit configmap coredns  (add inside the server block)
hosts {
    10.96.x.y keycloak.micro-commerce.k8s.orb.local
    fallthrough
}
```
Or, per-pod alternative on the catalog-api Deployment:
```yaml
hostAliases:
  - ip: "10.96.x.y"          # ingress controller ClusterIP / LB IP
    hostnames: [ "keycloak.micro-commerce.k8s.orb.local" ]
```

### 3) Catalog API Deployment env (matches AuthenticationExtensions.cs)
```yaml
env:
  # Authority == the ONE issuer; handler fetches {Authority}/.well-known/openid-configuration -> jwks_uri
  - { name: Keycloak__Authority, value: "http://keycloak.micro-commerce.k8s.orb.local/realms/microcommerce" }
  - { name: ConnectionStrings__catalogdb, valueFrom: { secretKeyRef: { name: catalog-secrets, key: catalogdb } } }
  - { name: ConnectionStrings__cache,      value: "redis:6379" }
  - { name: ConnectionStrings__photos,     valueFrom: { secretKeyRef: { name: catalog-secrets, key: photos } } }
  # Development env keeps RequireHttpsMetadata=false (per the code) so http issuer + http JWKS work locally.
  - { name: ASPNETCORE_ENVIRONMENT, value: "Development" }
```
No code change needed: ValidIssuer is inferred from Authority, JWKS auto-discovered, MapInboundClaims=false already set.

### 4) Next.js / browser
- next-auth (or oidc client) issuer/authority = http://keycloak.micro-commerce.k8s.orb.local/realms/microcommerce (SAME string). Browser hits it via ingress from the host; that resolution works out of the box on OrbStack.
- If the Next.js *server* (RSC/route handler) also calls Keycloak token endpoints, it needs the same in-cluster resolution fix from step 2.

### 5) realm JSON ConfigMap must define the audience
The realm must include a `catalog-api` client AND an audience mapper injecting aud=catalog-api, plus the flattened `roles` mapper (the API uses RoleClaimType="roles"). Without the aud mapper, tokens fail audience validation despite a correct issuer.

## Open risks
- OrbStack *.k8s.orb.local resolution from inside pods is unreliable (open issue #2306, EAI_AGAIN, with a reported Keycloak case). The CoreDNS hosts entry in step 2 is the mitigation but pins a ClusterIP that can change if you recreate the ingress controller - parameterize it or use the ingress LB IP and re-check after cluster recreation. This is the single most likely thing to break the API->JWKS fetch.
- Mixed http issuer is acceptable ONLY locally. Because KC_HOSTNAME is http://, every minted token and the discovery doc carry an http issuer, and the API must keep RequireHttpsMetadata=false (it does, in Development). Any promotion beyond local requires switching the whole chain to https + RequireHttpsMetadata=true, or token validation silently regresses.
- --import-realm is import-once: if you give Keycloak a persistent DB volume, realm.json edits in Git won't apply on restart (import skipped). For true GitOps drift-free behavior, either run Keycloak with an ephemeral DB (re-import every boot) or adopt the Keycloak Operator KeycloakRealmImport CR. Decide this explicitly - it surprises people during ArgoCD syncs.
- ConfigMap ~1 MiB cap: a realm export with seeded users can exceed it and the Deployment will fail to mount/apply. Keep the JSON to realm+clients+roles+mappers; seed users out-of-band or keep them minimal.
- Audience mapper omission: the API requires aud=catalog-api. If the realm JSON lacks the audience protocol mapper, every request 401s (IDX10214) even though the issuer is perfectly aligned - easy to misdiagnose as an issuer problem.
- Browser PKCE/redirect: redirect_uri and post-logout URLs registered in the realm must use the external host (keycloak... and the web app host on *.k8s.orb.local). A mismatch surfaces as 'Invalid parameter: redirect_uri', unrelated to issuer but commonly conflated with it.
- Sync-wave ordering: Keycloak (with realm import + readiness on the discovery endpoint) must be Healthy BEFORE catalog-api starts, otherwise the API's first metadata fetch fails. Place Keycloak in an earlier sync wave than catalog-api and rely on the readinessProbe hitting /.well-known/openid-configuration.
- KC 26 deprecation: use KC_BOOTSTRAP_ADMIN_USERNAME/PASSWORD (not KEYCLOAK_ADMIN/KEYCLOAK_ADMIN_PASSWORD) and KC_PROXY_HEADERS (not KC_PROXY=edge). Stale manifests copied from KC <26 will appear to start but ignore those vars.


================================================================================

# AREA: .NET 10 multi-stage Dockerfile for the Catalog API (chiseled, non-root, port 8080) + Aspire ServiceDefaults / OTLP behavior

## Corrections to design
- ENTRYPOINT assembly name: the natural guess 'MicroCommerce.Catalog.Api.dll' (from the project folder/namespace) is WRONG. The csproj file is named MicroCommerce.Catalog.csproj, so the build output is MicroCommerce.Catalog.dll. Use ENTRYPOINT ["dotnet","MicroCommerce.Catalog.dll"]. Verified against the actual bin output.
- /health and /alive are NOT exposed in non-Development environments. The locked design implicitly assumes they exist for K8s liveness/readiness probes, but MapDefaultEndpoints() guards them behind IsDevelopment(). Fix one of: (a) remove the IsDevelopment() guard in MapDefaultEndpoints (recommended — this is the documented Aspire 'has security implications, review aka.ms/aspire/healthchecks' note, acceptable on a local cluster), or (b) set ASPNETCORE_ENVIRONMENT=Development in the Deployment env. Without this, probes 404 and pods never become Ready. NOTE: this also gates MapOpenApi() and the Azurite CORS bootstrap — both only run in Development, so if you go route (a) keep that in mind; route (b) is the lower-risk change for a local OrbStack cluster and also enables the SAS-upload CORS setup against Azurite.
- Do NOT use a 'latest' tag — it does not exist for mcr.microsoft.com/dotnet. Pin '10.0' (or a digest). A Dockerfile written with FROM mcr.microsoft.com/dotnet/aspnet:latest will fail to pull.
- Do NOT assume Debian. .NET 10 base images are Ubuntu Noble by default; '-bookworm-slim'/Debian tags from .NET 8/9 muscle memory are not published for 10. If a Debian-style tag is in the proposed Dockerfile, it is wrong.
- The restore step must reference ALL project files in the dependency graph (Api -> Application + Infrastructure -> Domain), not just the Api csproj, or 'dotnet restore'/'dotnet publish --no-restore' on the Api project will fail to find the referenced projects. Copy all four csproj into their correct relative paths before restore. Restoring via the project (or the .slnx) is required; a single 'COPY *.csproj' at root will not work because each project lives in its own folder.
- Chiseled runtime image has NO shell, curl, or wget. A Dockerfile HEALTHCHECK using 'curl localhost:8080/health' will NOT work on -noble-chiseled. Rely on Kubernetes httpGet probes instead (they hit the port directly, no in-container binary needed), or use the chiseled-extra variant if you truly need a shell. Also EF migrate-on-boot still works (no shell needed), but if you ever want to run 'dotnet ef' you'd need the SDK image, not chiseled.
- An explicit 'USER $APP_UID' line is harmless but redundant on the -noble-chiseled image (already runs as app/1654). For NON-chiseled aspnet:10.0 you DO need to add 'USER $APP_UID' to run non-root. Decide based on which final base you pick.
- Build context matters: the csproj is at src/Services/Catalog.API/src/Api/ but it references ../Application, ../Infrastructure, ../Domain (siblings under .../src/). The Docker build context root must be at least .../Catalog.API/src/ (or the repo root) so all four projects are reachable by COPY. If you place the Dockerfile in the Api folder and build with context = Api folder, the COPY of sibling projects will fail.

## Verified facts
- [high] .NET 10.0 is GA. Images are published at mcr.microsoft.com/dotnet/sdk and mcr.microsoft.com/dotnet/aspnet. The version-only tags '10.0', '10.0-noble', and chiseled '10.0-noble-chiseled' all exist for aspnet; sdk publishes '10.0' / '10.0-noble' (plus -aot, -alpine, -azurelinux variants). The default OS for unqualified tags changed from Debian to Ubuntu 24.04 'Noble' — Debian images are NOT published for .NET 10.  (src: https://github.com/dotnet/dotnet-docker/discussions/6801)
- [high] Microsoft does NOT publish a 'latest' tag for the dotnet/sdk or dotnet/aspnet repos. You must pin a version tag (e.g. '10.0' or, more precisely, a digest). There is no mcr.microsoft.com/dotnet/aspnet:latest. The version-floating tag for .NET 10 is '10.0' (rolls forward across 10.0.x servicing/OS rebuilds).  (src: https://learn.microsoft.com/en-us/dotnet/architecture/microservices/net-core-net-framework-containers/official-net-docker-images)
- [high] Since .NET 8 every Linux image ships a non-root user 'app' with UID 1654, exposed via the $APP_UID env var. For chiseled images the non-root 'app' user is the DEFAULT (USER is already set in the image), which is why the official dotnet-docker chiseled sample (FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled) does NOT add an explicit USER line. For non-chiseled images you opt in with 'USER $APP_UID'.  (src: https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8/containers)
- [high] ASP.NET Core container images default to port 8080 (not 80) since .NET 8, set via ASPNETCORE_HTTP_PORTS=8080 baked into the base image. The official .NET 10 dotnet-docker chiseled sample contains 'EXPOSE 8080'. Running as non-root requires the port stay >=1024 (8080 is fine); reverting to port 80 breaks non-root.  (src: https://github.com/dotnet/dotnet-docker/blob/main/samples/aspnetapp/Dockerfile.chiseled)
- [high] The official .NET 10 multi-stage pattern (MS Learn, aspnetcore-10.0 moniker): FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build; copy csproj files first; RUN dotnet restore; copy the rest; RUN dotnet publish -c Release -o /app --no-restore; then FROM mcr.microsoft.com/dotnet/aspnet:10.0; COPY --from=build /app ./; ENTRYPOINT ["dotnet","<App>.dll"]. The csproj-first / restore-as-distinct-layer ordering is explicitly recommended for build-cache reuse.  (src: https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/building-net-docker-images?view=aspnetcore-10.0)
- [high] MS Learn recommends pinning the base image by SHA256 digest (e.g. mcr.microsoft.com/dotnet/aspnet:10.0@sha256:...) as a best practice for immutability/supply-chain integrity.  (src: https://learn.microsoft.com/en-us/dotnet/core/docker/build-container)
- [high] The Catalog API output assembly is MicroCommerce.Catalog.dll (verified in bin/Debug/net10.0). No AssemblyName override and no Directory.Build.props/Directory.Packages.props exist in the repo, so the assembly name follows the csproj filename. Therefore ENTRYPOINT must be ["dotnet","MicroCommerce.Catalog.dll"], NOT MicroCommerce.Catalog.Api.dll.  (src: /Users/baotoq/Work/micro-commerce/src/Services/Catalog.API/src/Api/MicroCommerce.Catalog.csproj)
- [high] The Aspire ServiceDefaults code (Api/Extensions.cs, inlined into the Api project — there is NO separate ServiceDefaults project) only registers the OTLP exporter when builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] is non-empty (UseOtlpExporter() is inside an if-guard). With no collector / no env var, the exporter is never registered: no connection attempt, no crash, no warning spam. Telemetry is simply collected in-process and not exported.  (src: /Users/baotoq/Work/micro-commerce/src/Services/Catalog.API/src/Api/Extensions.cs)
- [high] Even if OTEL_EXPORTER_OTLP_ENDPOINT were set but the collector unreachable, the OTLP exporter does NOT crash the app: export runs on a background BatchExportProcessor (traces/logs) / PeriodicExportingMetricReader (metrics, 60s). Failures are logged via the EventSource 'OpenTelemetry-Exporter-OpenTelemetryProtocol', not thrown. Default endpoint is localhost:4317 (gRPC) / 4318 (HTTP).  (src: https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.OpenTelemetryProtocol/README.md)
- [high] MapDefaultEndpoints() in Extensions.cs maps /health and /alive ONLY when app.Environment.IsDevelopment(). In a Kubernetes/Production environment those endpoints return 404. The task description's assumption that the API 'has /health and /alive endpoints from Aspire ServiceDefaults' is only true in Development — K8s probes against them will fail unless this guard is removed or ASPNETCORE_ENVIRONMENT=Development is set.  (src: /Users/baotoq/Work/micro-commerce/src/Services/Catalog.API/src/Api/Extensions.cs)
- [high] The Api project references Microsoft.EntityFrameworkCore.Design (PrivateAssets=all) and runs db.Database.MigrateAsync() at boot — migrations are compiled into the app assembly and run at runtime; the design package is build-time only and does NOT need the SDK/dotnet-ef in the runtime image. EF auto-migrate on boot works in the aspnet runtime image.  (src: /Users/baotoq/Work/micro-commerce/src/Services/Catalog.API/src/Api/MicroCommerce.Catalog.csproj)

## Recommended config
# ---- Dockerfile (place at repo root OR at src/Services/Catalog.API/src/ ; below assumes
#      build context = src/Services/Catalog.API/src  so the four projects are reachable) ----
# Pin a digest in CI for supply-chain integrity (MS Learn best practice); shown by tag here.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1) Copy ONLY csproj files first, preserving relative paths, then restore (cached layer).
COPY Api/MicroCommerce.Catalog.csproj                 Api/
COPY Application/MicroCommerce.Catalog.Application.csproj  Application/
COPY Infrastructure/MicroCommerce.Catalog.Infrastructure.csproj Infrastructure/
COPY Domain/MicroCommerce.Catalog.Domain.csproj       Domain/
RUN dotnet restore Api/MicroCommerce.Catalog.csproj

# 2) Copy the rest of the source and publish (framework-dependent, Release).
COPY . .
RUN dotnet publish Api/MicroCommerce.Catalog.csproj \
    -c Release -o /app --no-restore /p:UseAppHost=false

# 3) Runtime: chiseled, distroless, already non-root (app / UID 1654), ASPNETCORE_HTTP_PORTS=8080 baked in.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
WORKDIR /app
COPY --from=build /app .
# ASPNETCORE_HTTP_PORTS=8080 is already set by the base image; set explicitly only if you want to be defensive:
# ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
# Chiseled base already runs as 'app'; the next line is OPTIONAL/redundant on chiseled, REQUIRED on non-chiseled aspnet:10.0:
# USER $APP_UID
ENTRYPOINT ["dotnet", "MicroCommerce.Catalog.dll"]

# ============================================================
# .dockerignore  (place at the SAME directory as the build context root)
# ============================================================
# **/bin/
# **/obj/
# **/.vs/
# **/.vscode/
# **/.idea/
# **/*.user
# **/node_modules/
# Dockerfile*
# .dockerignore
# .git/
# .gitignore
# **/appsettings.*.json   # optional: keep out of image, inject via K8s ConfigMap/Secret/env
# README.md
# **/*.md

# ============================================================
# K8s probes (do NOT use a chiseled-incompatible curl HEALTHCHECK; use httpGet)
# ============================================================
# livenessProbe:  { httpGet: { path: /alive,  port: 8080 } }
# readinessProbe: { httpGet: { path: /health, port: 8080 } }
# PREREQUISITE: either remove the IsDevelopment() guard around MapHealthChecks in
# Api/Extensions.cs (MapDefaultEndpoints), OR set env ASPNETCORE_ENVIRONMENT=Development
# on the Deployment, otherwise /health and /alive return 404 and pods never go Ready.

# OTLP: with no collector, leave OTEL_EXPORTER_OTLP_ENDPOINT UNSET -> exporter is never
# registered (Extensions.cs guard) -> no crash, no warnings. To wire telemetry later, set
# OTEL_EXPORTER_OTLP_ENDPOINT=http://<collector>:4317 in the Deployment env.

## Open risks
- /health and /alive 404 outside Development is the single biggest deployment risk — K8s readiness probe will fail and the pod will never receive traffic. Must be fixed (remove guard or set ASPNETCORE_ENVIRONMENT=Development) before this deploys. This is a code/config change, not a Dockerfile change, so it is easy to miss in a 'Dockerfile-only' task.
- Setting ASPNETCORE_ENVIRONMENT=Development to expose probes also turns on MapOpenApi() and the Azurite-CORS bootstrap (GetPropertiesAsync/SetPropertiesAsync on the blob client at startup). On the local OrbStack cluster that is probably desirable (SAS uploads to Azurite need CORS), but it means the app will fail fast at boot if the 'photos' Azure blob connection string is missing/unreachable — sequence Azurite before Catalog API via sync waves.
- EF MigrateAsync() runs on every pod start before the app is ready. With multiple replicas this races; for a single-replica local cluster it is fine, but if you scale the Deployment >1 you need a migration init-container or leader-election, otherwise concurrent MigrateAsync calls can deadlock/duplicate. The app also hard-depends on Postgres being reachable at boot (no retry around MigrateAsync) — order Postgres before Catalog API in sync waves.
- Chiseled image has no shell — exec-debugging (kubectl exec ... sh) won't work, and any tooling/sidecar expecting a shell in this container will fail. Use ephemeral debug containers or the -noble-chiseled-extra variant if shell access is needed.
- Dapr sidecar: the app calls AddDaprClient() and publishes via Dapr pubsub. The Dockerfile itself is Dapr-agnostic (sidecar is injected by the Dapr K8s operator via pod annotations), but verify the app's Dapr HTTP/gRPC ports and that DAPR_HTTP_PORT/DAPR_GRPC_PORT are available — Dapr injection is a Deployment-annotation concern, not a Dockerfile concern. Nothing in the Dockerfile needs to change for Dapr.
- Digest pinning vs the floating '10.0' tag is a trade-off: pin a digest for reproducible/secure builds in GitHub Actions, but you then must bump it to pick up servicing CVEs. For a local cluster the floating '10.0' tag is acceptable.
- The publish uses /p:UseAppHost=false to skip the native apphost (smaller image, and the ENTRYPOINT uses 'dotnet X.dll' anyway). If you instead want ENTRYPOINT ["./MicroCommerce.Catalog"] you must drop that flag and ensure the apphost is chmod +x — the 'dotnet X.dll' form avoids that entirely and is the safer default.
- There is a stray MicroCommerce.Server.dll in the current bin output, suggesting a leftover/renamed reference somewhere in build history. It does not affect the Dockerfile (publish output is driven by the csproj graph), but worth a sanity check that the solution graph is clean before building the image.


================================================================================

# AREA: GitOps deployment to local OrbStack k8s — PART A: Next.js 16 standalone Dockerfile (build vs runtime env); PART B: Azurite on k8s (external SAS host + CORS for browser PUT)

## Corrections to design
- PART A — `output:'standalone'` is NOT currently set in src/web/next.config.ts. The proposed plan assumes it's there; it is not. You must add `output: 'standalone'` to next.config.ts or the Docker build will have no `.next/standalone` to COPY. This is the single most likely thing to break the build silently.
- PART A — Do NOT plan to bake AUTH_*/API_URL as build ARGs. They are server-only (no NEXT_PUBLIC_ prefix) and are read at runtime — inject them as pod env/Secrets, not at build time. Treating them as build-time would force a rebuild per environment for no reason and risk leaking the Keycloak client secret into image layers.
- PART A — The 'create a non-root nextjs:nodejs user' step is optional/over-specified. The current official with-docker example simply uses the image's built-in `node` user with `USER node` plus `--chown=node:node` on the COPYs and a `mkdir .next && chown node:node .next`. Either approach is fine; the built-in user is simpler and is what Vercel ships today.
- PART A — `public/` may not exist or may need the directory pre-created; the official Dockerfile does `RUN mkdir .next` before copying static so chown works. If `public/` is empty the COPY can fail in some BuildKit configs — guard it. This repo DOES have a populated public/ dir, so a plain `COPY public ./public` is fine here.
- PART B — The proposed external host `http://azurite.micro-commerce.k8s.orb.local` is a PATH-STYLE host (account name lives in the path: /devstoreaccount1/photos/...), NOT a production-style `<account>.blob.<host>` FQDN. Azurite defaults to parsing the account from the HOST. You MUST run Azurite with `--disableProductStyleUrl` (and `--blobHost 0.0.0.0`) or every request 400s with an account-name parse error. The proposed plan as written would not work without this flag.
- PART B — 'pods resolve the same host as the browser' is achievable but think about it explicitly: the catalog-api pod must reach `azurite.micro-commerce.k8s.orb.local` for CreateIfNotExistsAsync AND the SAS host must equal that name. The cleanest model on OrbStack is to make the BlobEndpoint host = the external Ingress hostname and route it through the Ingress to the azurite Service, so browser and pod use the identical URL. Using two different hosts (internal Service DNS for the pod, external for the SAS) will produce SAS signatures whose host the browser can reach but means the pod's CreateIfNotExists call goes to a different endpoint — workable but the connection string drives BOTH, so you cannot trivially split them with a single ConnectionStrings__photos. If you must split, the pod should call Azurite via the same external hostname too (resolvable in-cluster via k8s.orb.local), keeping one connection string.
- PART B — CORS is NOT automatic. The plan must include an explicit one-time step to set blob-service CORS rules (allow the web origin, PUT+OPTIONS, the x-ms-* and content-type headers) via SetProperties — either an init Job/container calling the Azure SDK, or seeding on app boot. The current BlobPhotoUploadUrlIssuer.cs does NOT set CORS. Browser PUT uploads will fail preflight until this exists.
- PART B — Do NOT rely on `host.docker.internal` here. That hostname trick is for Docker-host scenarios and is not how OrbStack k8s pod networking resolves the azurite Service; use `--disableProductStyleUrl` + a real Ingress/Service hostname instead.
- PART B — Azurite is HTTP-only here (DefaultEndpointsProtocol=http). The .NET BlobClient needs `CanGenerateSasUri==true`, which requires the client be built with SHARED-KEY credentials (the connection string carries AccountKey). If you ever switch the connection string to a token/OAuth form, GenerateSasUri throws (the code already guards this at line 41-44). Keep the AccountKey in the connection string.

## Verified facts
- [high] This repo already sets `output:'standalone'` requirement implicitly but the actual next.config.ts at src/web/next.config.ts does NOT currently contain `output:'standalone'`. It has reactCompiler, cacheComponents, allowedDevOrigins, images.remotePatterns only. You MUST add `output:'standalone'` for the Docker image to work — without it `.next/standalone` is never produced.  (src: /Users/baotoq/Work/micro-commerce/src/web/next.config.ts)
- [high] Next.js standalone produces `.next/standalone` with a minimal `server.js`. server.js does NOT bundle the `public/` or `.next/static/` folders by default — you must copy them manually so the runtime serves them: `cp -r public .next/standalone/ && cp -r .next/static .next/standalone/.next/`. In Docker the equivalent COPYs are: `COPY --from=builder /app/.next/standalone ./`, `COPY --from=builder /app/.next/static ./.next/static`, `COPY --from=builder /app/public ./public`.  (src: src/web/node_modules/next/dist/docs/01-app/03-api-reference/05-config/01-next-config-js/output.md (lines 34-48))
- [high] Correct runtime start command is `node server.js` (run from the standalone root, which becomes WORKDIR /app). Port/host are controlled by `PORT` and `HOSTNAME` env vars read at process start; for containers set `ENV PORT=3000` and `ENV HOSTNAME="0.0.0.0"` (0.0.0.0 is required so the server binds all interfaces and is reachable from the k8s Service/pod network).  (src: src/web/node_modules/next/dist/docs/.../output.md (lines 54,63) + Next deploying doc 17-deploying.md)
- [high] The CURRENT official Next.js with-docker example Dockerfile uses the image's BUILT-IN `node` user (uid/gid 1000) rather than creating a custom `nextjs:nodejs` user. It uses `COPY --from=builder --chown=node:node`, `RUN mkdir .next && chown node:node .next`, then `USER node`. NODE_VERSION arg is 24.13.0-slim, three stages (dependencies/builder/runner), EXPOSE 3000, CMD ["node","server.js"]. Either a custom non-root user OR the built-in `node` user satisfies the non-root requirement.  (src: https://raw.githubusercontent.com/vercel/next.js/canary/examples/with-docker/Dockerfile)
- [high] App Router runtime-vs-build env: server-only vars (no NEXT_PUBLIC_ prefix) are read from process.env at RUNTIME during dynamic rendering — this includes ALL of this app's auth/API vars: API_URL, AUTH_KEYCLOAK_ISSUER, AUTH_KEYCLOAK_ID, AUTH_KEYCLOAK_SECRET, AUTH_SECRET, AUTH_URL. None of these are referenced client-side, so none need to be present at `next build` time; inject them at pod runtime via env/Secret. Grep confirms every consumer is a server module (route handlers, lib/auth/config.ts, lib/catalog/*.ts).  (src: src/web/node_modules/next/dist/docs/01-app/02-guides/environment-variables.md (lines 194-238) + grep of src/web/src)
- [high] Any var prefixed NEXT_PUBLIC_ is INLINED into the client JS bundle at `next build` and frozen — it canNOT be changed at runtime by setting an env var on the pod. This app currently has ZERO NEXT_PUBLIC_ vars, so the 'build once, run anywhere' single-image model works cleanly. If a public browser-visible value is added later (e.g. a browser-facing API base or Keycloak public URL for client-side redirects), it must be a build ARG or use a runtime-injection shim (window.__ENV pattern).  (src: src/web/node_modules/next/dist/docs/01-app/02-guides/environment-variables.md (lines 154-198))
- [high] Auth.js v5 (next-auth 5.0.0-beta.31) reads AUTH_KEYCLOAK_ID/SECRET/ISSUER and AUTH_SECRET at runtime on the server; AUTH_URL pins the canonical callback origin. These are all server-side — safe to supply at runtime. The token-refresh fetch hits `${AUTH_KEYCLOAK_ISSUER}/protocol/openid-connect/token` server-side, so the issuer must be a hostname the WEB POD can resolve in-cluster (it does not need to be browser-reachable for refresh, but the browser DOES hit the issuer during the OAuth redirect/login — so the issuer URL must be reachable by BOTH browser and pod, identical value, e.g. http://keycloak.micro-commerce.k8s.orb.local/realms/microcommerce).  (src: /Users/baotoq/Work/micro-commerce/src/web/src/lib/auth/config.ts (lines 63-76) + package.json)
- [high] Azurite default well-known connection string: `DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;`. Blob port 10000, queue 10001, table 10002. Account key must be base64.  (src: https://learn.microsoft.com/en-us/azure/storage/common/storage-connect-azurite)
- [high] The .NET Azure.Storage.Blobs SAS URL is generated ENTIRELY CLIENT-SIDE: BlobClient.GenerateSasUri() builds the URI from the BlobServiceClient's endpoint (taken from the connection string's BlobEndpoint) plus the shared key — Azurite is never contacted to mint the SAS. Therefore the SAS host == whatever BlobEndpoint host is in `ConnectionStrings__photos`. To make server-generated SAS URLs point at an external browser-reachable host, override BlobEndpoint in that connection string. The signature is over the canonicalized resource (account+container+blob), NOT the host, so changing the host does not by itself invalidate the signature.  (src: /Users/baotoq/Work/micro-commerce/src/Services/Catalog.API/src/Infrastructure/Photos/BlobPhotoUploadUrlIssuer.cs (lines 41-49) + storage-connect-azurite doc)
- [high] Aspire injects the blob connection string as env var `ConnectionStrings__photos`, and AddAzureBlobServiceClient("photos") in Program.cs resolves the BlobServiceClient from it. In k8s (no Aspire orchestrator), you set `ConnectionStrings__photos` directly on the catalog-api pod. This is the single override point for both the SAS host AND the pod's own reachability to Azurite.  (src: /Users/baotoq/Work/micro-commerce/src/Services/Catalog.API/src/Api/Program.cs (line 19) + dotnet/aspire connection-string naming)
- [high] Azurite host parsing: by DEFAULT Azurite expects production-style URLs where the account name is the FIRST LABEL of the HOST (e.g. devstoreaccount1.blob.localhost). For a path-style host like `azurite.micro-commerce.k8s.orb.local/devstoreaccount1/...` you MUST start Azurite with `--disableProductStyleUrl` so it parses the account name from the URL PATH instead of the host. Exception: a host of exactly `host.docker.internal` is always treated as path-style.  (src: https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite (Production-style URL section))
- [high] Azurite binds to 127.0.0.1 by default (--blobHost default 127.0.0.1, --blobPort default 10000); to accept connections from other pods/the browser you MUST start it with `--blobHost 0.0.0.0`.  (src: https://github.com/Azure/Azurite (CLI options))
- [medium] Azurite V3 lists 'CORS and Preflight' as a SUPPORTED blob feature, but CORS rules are EMPTY by default and must be set explicitly via the SetServiceProperties / SetProperties API (Azure.Storage.Blobs BlobServiceClient.SetProperties with BlobCorsRule, or @azure/storage-blob setProperties). Up to 5 CORS rules per service. There is a historical Azurite bug (#731/#55) where set CORS rules returned 202 but headers weren't emitted to the browser; modern V3 implements it but it remains a known fragility — verify against your pinned Azurite version.  (src: https://github.com/Azure/Azurite + https://github.com/Azure/Azurite/issues/731 + https://learn.microsoft.com/en-us/rest/api/storageservices/cross-origin-resource-sharing--cors--support-for-the-azure-storage-services)
- [high] Browser PUT upload to a SAS URL is a NON-simple cross-origin request: the browser sends a preflight OPTIONS first. The CORS rule on the blob service must allow the web app origin, the PUT and OPTIONS methods, and the headers the SAS PUT sends (x-ms-blob-type, x-ms-version, content-type, etc.). Without a matching CORS rule the upload fails with 'No Access-Control-Allow-Origin header'.  (src: https://learn.microsoft.com/en-us/rest/api/storageservices/cross-origin-resource-sharing--cors--support-for-the-azure-storage-services)

## Recommended config
PART A — next.config.ts: add `output: 'standalone'` (it is currently MISSING):
```ts
const nextConfig: NextConfig = {
  output: 'standalone',
  reactCompiler: true,
  cacheComponents: true,
  // ...existing allowedDevOrigins, images...
};
```
Dockerfile (src/web/Dockerfile), based on the current official vercel/next.js with-docker example:
```dockerfile
ARG NODE_VERSION=24.13.0-slim
FROM node:${NODE_VERSION} AS deps
WORKDIR /app
COPY package.json package-lock.json* ./
RUN --mount=type=cache,target=/root/.npm npm ci --no-audit --no-fund

FROM node:${NODE_VERSION} AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .
ENV NODE_ENV=production
# No AUTH_*/API_URL needed at build time (server-only, read at runtime).
RUN npm run build

FROM node:${NODE_VERSION} AS runner
WORKDIR /app
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
Web pod env (runtime, via Secret/ConfigMap — NOT build args):
```
API_URL=http://catalog-api.micro-commerce.svc.cluster.local:8080   # in-cluster service DNS
AUTH_KEYCLOAK_ISSUER=http://keycloak.micro-commerce.k8s.orb.local/realms/microcommerce  # MUST be reachable by browser AND pod, identical value
AUTH_KEYCLOAK_ID=microcommerce-web
AUTH_KEYCLOAK_SECRET=<secret>
AUTH_SECRET=<secret>
AUTH_URL=http://web.micro-commerce.k8s.orb.local
```

PART B — Azurite container args (Deployment):
```
azurite --blobHost 0.0.0.0 --blobPort 10000 --disableProductStyleUrl --location /data --skipApiVersionCheck
```
catalog-api pod env (single source of truth for SAS host + pod reachability):
```
ConnectionStrings__photos=DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://azurite.micro-commerce.k8s.orb.local/devstoreaccount1;
```
(Ingress route: `azurite.micro-commerce.k8s.orb.local` -> azurite Service :10000. The pod resolves the same `*.k8s.orb.local` name in-cluster, so one connection string serves both CreateIfNotExists and the browser-facing SAS host.)

CORS — one-time init (an init Job or app-boot seed) using Azure.Storage.Blobs:
```csharp
var props = await blobServiceClient.GetPropertiesAsync();
props.Value.Cors = new List<BlobCorsRule> {
  new BlobCorsRule {
    AllowedOrigins  = "http://web.micro-commerce.k8s.orb.local",
    AllowedMethods  = "PUT,OPTIONS",
    AllowedHeaders  = "x-ms-blob-type,x-ms-version,content-type",
    ExposedHeaders  = "*",
    MaxAgeInSeconds = 3600,
  }
};
await blobServiceClient.SetPropertiesAsync(props.Value);
```
(Up to 5 rules. Set this BEFORE the first browser upload; it is not configured today.)

## Open risks
- next.config.ts currently lacks `output:'standalone'` — must be added before any Docker build; verify `.next/standalone/server.js` exists after `npm run build` as the build gate.
- Azurite CORS enforcement has a history of flakiness (#731): rules accepted (202) but headers occasionally not emitted to the browser. Pin a recent Azurite image and VERIFY a real browser PUT succeeds (preflight OPTIONS returns Access-Control-Allow-Origin) before declaring done — do not trust the 202 from SetProperties alone.
- next.config.ts images.remotePatterns currently allows only `http 127.0.0.1:10000` and `**.blob.core.windows.net` for next/image. If product photoUrls (BlobUrl returned by the issuer) now use `azurite.micro-commerce.k8s.orb.local`, the storefront ProductImage via next/image will REJECT them (image optimizer 400). You must add that host to remotePatterns, and because remotePatterns are read at build time the image config is effectively build-time-fixed for the optimizer allow-list.
- Account-name-in-host vs path is the single biggest Azurite footgun: forgetting `--disableProductStyleUrl` yields cryptic 400 'unable to extract account name' errors only at upload time, not at SAS-generation time (SAS is minted client-side and looks valid).
- SAS PUT does not enforce Content-Length (noted TODO in BlobPhotoUploadUrlIssuer.cs line 27) — unbounded upload size remains an open issue independent of k8s.
- If browser and pod ever end up with different hostnames for Azurite, the SAS signature stays valid (signature excludes host) but operational confusion and split-DNS bugs are likely; keep ONE hostname in ConnectionStrings__photos.
- Keycloak issuer URL must be byte-identical between the browser redirect and the pod's token-refresh fetch, AND must match the realm's registered redirect URIs and the OIDC `iss` claim — a mismatch between an in-cluster Service DNS issuer and an external Ingress issuer is a classic cause of 'invalid issuer'/redirect_uri errors. Use the external k8s.orb.local hostname everywhere.
- Azurite blob data needs a PVC (`--location /data`) or uploaded photos vanish on pod restart; the AppHost uses a persistent data volume today — replicate that with a PersistentVolumeClaim.


================================================================================


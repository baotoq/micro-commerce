# Phase 26: ArgoCD GitOps - Research

**Researched:** 2026-03-02
**Domain:** ArgoCD GitOps, app-of-apps pattern, Kubernetes declarative management
**Confidence:** HIGH

## Summary

Phase 26 installs ArgoCD v3.3.2 in the existing kind cluster and configures it to manage all MicroCommerce cluster resources from Git using the app-of-apps pattern. Currently, the bootstrap script (`infra/k8s/bootstrap.sh`) deploys all resources via manual `kubectl apply -k`. After this phase, ArgoCD watches the Git repository and automatically syncs the cluster state to match. Manual `kubectl apply` is replaced by ArgoCD's continuous reconciliation loop.

The existing Kustomize structure (`infra/k8s/base/` + `infra/k8s/overlays/dev/`) is already well-suited for ArgoCD. ArgoCD natively detects and renders Kustomize directories when a `kustomization.yaml` is present. The app-of-apps pattern creates a root Application that points to a directory of child Application manifests, where each child Application points to a specific Kustomize path (e.g., `infra/k8s/overlays/dev/` or individual service directories). Self-heal mode ensures that if someone manually deletes a Deployment, ArgoCD automatically restores it within its 5-second default reconciliation cycle.

The critical decision -- already locked in STATE.md -- is to use ArgoCD v3.3.2 specifically. Versions 3.3.0 and 3.3.1 had a client-side apply migration bug (GitHub issue #26279) that caused `failed to perform client-side apply migration` errors. This was fixed in v3.3.2 (released 2026-02-22).

**Primary recommendation:** Install ArgoCD v3.3.2 in the `argocd` namespace with the non-HA `install.yaml` manifest, expose the UI via NodePort on port 30443 (mapped to host 38443), create a root app-of-apps Application that references a new `infra/k8s/argocd/apps/` directory containing one child Application YAML per service, each pointing to the appropriate Kustomize path.

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| GOPS-01 | ArgoCD is installed in the kind cluster and manages all services | ArgoCD v3.3.2 non-HA `install.yaml` installed via `kubectl apply --server-side --force-conflicts` in the `argocd` namespace. Self-heal + auto-sync enabled on all Applications. NodePort service exposes UI on port 30443 (host 38443). |
| GOPS-02 | App-of-apps root Application manages per-service child Applications | Root Application in `infra/k8s/argocd/root-app.yaml` points to `infra/k8s/argocd/apps/` directory. Each child Application YAML defines one service (postgres, rabbitmq, keycloak, apiservice, gateway, web) pointing to its Kustomize path with auto-sync and self-heal enabled. |
</phase_requirements>

## Standard Stack

### Core
| Tool | Version | Purpose | Why Standard |
|------|---------|---------|--------------|
| ArgoCD | v3.3.2 | GitOps continuous delivery for Kubernetes | De facto standard K8s GitOps tool; CNCF graduated project; native Kustomize support; v3.3.2 specifically chosen per project decision to avoid client-side apply migration bug in v3.3.0/3.3.1 |
| Kustomize | built-in (kubectl) | Manifest customization | Already used in Phases 24-25; ArgoCD auto-detects kustomization.yaml files |

### Supporting
| Tool | Version | Purpose | When to Use |
|------|---------|---------|-------------|
| kubectl | existing | Apply ArgoCD install manifest, port-forward, debugging | Initial ArgoCD installation before ArgoCD manages itself |
| argocd CLI | v3.3.2 | Retrieve initial admin password, manual sync/debugging | Optional -- all operations can be done via `kubectl` and the UI |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| App-of-apps | ApplicationSets | ApplicationSets are more dynamic (generators), but app-of-apps is simpler for a fixed set of 6 services; the project REQUIREMENTS.md explicitly requires "App-of-apps root Application" |
| ArgoCD non-HA install | ArgoCD HA install | HA install runs 3+ replicas of each component -- overkill for a single-node kind cluster |
| Plain YAML child apps | Helm-templated child apps | Plain YAML is simpler and sufficient; no templating needed for 6 static child apps |

**Installation:**
```bash
kubectl create namespace argocd
kubectl apply -n argocd --server-side --force-conflicts \
  -f https://raw.githubusercontent.com/argoproj/argo-cd/v3.3.2/manifests/install.yaml
```

## Architecture Patterns

### Recommended Directory Structure
```
infra/k8s/
  argocd/
    root-app.yaml             # Root app-of-apps Application (applied manually once)
    apps/                     # Directory of child Application manifests
      postgres.yaml           # Points to infra/k8s/base/postgres/
      rabbitmq.yaml           # Points to infra/k8s/base/rabbitmq/
      keycloak.yaml           # Points to infra/k8s/base/keycloak/
      apiservice.yaml         # Points to infra/k8s/overlays/dev/ (or base/apiservice/)
      gateway.yaml            # Points to infra/k8s/overlays/dev/ (or base/gateway/)
      web.yaml                # Points to infra/k8s/overlays/dev/ (or base/web/)
  base/                       # (existing) Environment-neutral manifests
    kustomization.yaml        # Root kustomization referencing all sub-dirs
    namespace.yaml
    postgres/
    rabbitmq/
    keycloak/
    apiservice/
    gateway/
    web/
  overlays/
    dev/
      kustomization.yaml      # (existing) Patches image tags for dev
```

### Pattern 1: App-of-Apps with Plain YAML Children
**What:** A root Application whose source path contains plain YAML Application manifests. ArgoCD recursively creates child Applications from this directory.
**When to use:** Fixed set of services that rarely change in number. Simpler than ApplicationSets for small, static service lists.
**Key design decision:** Each child Application should point to the **Kustomize overlay path** (`infra/k8s/overlays/dev/`) rather than individual base directories, because the overlay is where image tags are patched. However, since the current overlay aggregates ALL services via `resources: [../../base]`, each child cannot independently point to the overlay without syncing everything. The solution is to restructure: either (a) have one Application point to the full overlay, or (b) split the overlay into per-service overlays.

**Recommended approach (a) -- single dev overlay Application + per-infra base Applications:**
- Infrastructure services (postgres, rabbitmq, keycloak) are managed as individual child Applications pointing to their base directories (they have no overlay patches)
- Application services (apiservice, gateway, web) are managed by a single child Application pointing to `infra/k8s/overlays/dev/` which includes the full stack

**Alternative approach -- more granular:** Restructure overlays to per-service directories. This is cleaner for Phase 27 (CI image tag updates per service) but requires refactoring the existing overlay. Can be done now or deferred.

**Example root Application:**
```yaml
# Source: https://argo-cd.readthedocs.io/en/stable/operator-manual/cluster-bootstrapping/
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: micro-commerce-root
  namespace: argocd
  finalizers:
  - resources-finalizer.argocd.argoproj.io
spec:
  project: default
  source:
    repoURL: https://github.com/baotoq/micro-commerce.git
    targetRevision: master
    path: infra/k8s/argocd/apps
  destination:
    server: https://kubernetes.default.svc
    namespace: argocd
  syncPolicy:
    automated:
      selfHeal: true
      prune: true
```

**Example child Application (infrastructure service):**
```yaml
# Source: https://argo-cd.readthedocs.io/en/stable/operator-manual/declarative-setup/
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: postgres
  namespace: argocd
  finalizers:
  - resources-finalizer.argocd.argoproj.io
spec:
  project: default
  source:
    repoURL: https://github.com/baotoq/micro-commerce.git
    targetRevision: master
    path: infra/k8s/base/postgres
  destination:
    server: https://kubernetes.default.svc
    namespace: micro-commerce
  syncPolicy:
    automated:
      selfHeal: true
      prune: true
    syncOptions:
    - CreateNamespace=true
```

**Example child Application (full dev overlay):**
```yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: micro-commerce-apps
  namespace: argocd
  finalizers:
  - resources-finalizer.argocd.argoproj.io
spec:
  project: default
  source:
    repoURL: https://github.com/baotoq/micro-commerce.git
    targetRevision: master
    path: infra/k8s/overlays/dev
  destination:
    server: https://kubernetes.default.svc
    namespace: micro-commerce
  syncPolicy:
    automated:
      selfHeal: true
      prune: true
    syncOptions:
    - CreateNamespace=true
```

### Pattern 2: ArgoCD NodePort Access for kind
**What:** Patch the `argocd-server` Service to NodePort and add a kind extraPortMapping so the ArgoCD UI is reachable from the host.
**When to use:** kind clusters where LoadBalancer is not available.
**Example:**
```yaml
# NodePort service for argocd-server
apiVersion: v1
kind: Service
metadata:
  name: argocd-server-nodeport
  namespace: argocd
spec:
  type: NodePort
  selector:
    app.kubernetes.io/name: argocd-server
  ports:
  - name: https
    port: 443
    targetPort: 8080
    nodePort: 30443
```

And in `kind-config.yaml`, add the port mapping:
```yaml
- containerPort: 30443
  hostPort: 38443
  protocol: TCP
```

### Pattern 3: Disable TLS for Local Dev
**What:** ArgoCD server runs HTTPS by default. For kind local dev, disable TLS so NodePort works without certificate issues.
**When to use:** Local development clusters only.
**Example:**
```yaml
# Patch argocd-cmd-params-cm ConfigMap
apiVersion: v1
kind: ConfigMap
metadata:
  name: argocd-cmd-params-cm
  namespace: argocd
data:
  server.insecure: "true"
```
After patching, restart the argocd-server deployment:
```bash
kubectl rollout restart deployment argocd-server -n argocd
```

### Anti-Patterns to Avoid
- **Deeply nested app-of-apps:** Do not create apps that create apps that create apps. One level of nesting (root -> children) is sufficient.
- **Using HEAD targetRevision in production:** Use a specific branch name (`master`) or commit SHA. HEAD can cause unexpected syncs.
- **Mixing directory and Kustomize sources:** If a path contains `kustomization.yaml`, ArgoCD uses Kustomize mode. If you set `directory.recurse: true` on a path that also has `kustomization.yaml`, it will fail. Do not mix modes.
- **Omitting finalizers:** Without `resources-finalizer.argocd.argoproj.io`, deleting an Application does not delete its managed resources, leading to orphaned resources.
- **SealedSecrets conflict:** ArgoCD will try to sync SealedSecret resources. Since SealedSecrets are dynamically generated by the bootstrap script and may not exist in Git (they are `.gitignore`d or generated per-cluster), ArgoCD may report them as OutOfSync. Plan for this.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| GitOps sync controller | Custom kubectl-apply-on-commit script | ArgoCD automated sync + self-heal | Handles drift detection, health checks, retry logic, UI, RBAC, multi-app orchestration |
| App dependency ordering | Custom init containers or scripts for service startup order | ArgoCD sync waves + health checks | ArgoCD waits for resources to be Healthy before proceeding to next wave |
| Git webhook reconciliation | Custom GitHub webhook -> kubectl pipeline | ArgoCD's built-in Git polling (3 min default) or webhook receiver | ArgoCD has a built-in webhook server that triggers immediate sync on push |
| Resource health dashboard | Custom monitoring of pod status | ArgoCD UI resource tree view | Shows real-time health for every resource in every application |

**Key insight:** ArgoCD replaces the manual `kubectl apply -k` commands in the bootstrap script for application deployment. The bootstrap script still handles one-time infrastructure setup (kind cluster creation, SealedSecrets controller installation, secret sealing), but ArgoCD takes over the ongoing management of all Kubernetes resources from Git.

## Common Pitfalls

### Pitfall 1: SealedSecrets Not in Git
**What goes wrong:** ArgoCD reports applications as OutOfSync because `sealed-secret.yaml` files are generated dynamically by the bootstrap script and may not be committed to Git (they contain cluster-specific encrypted data).
**Why it happens:** SealedSecrets are encrypted with the controller's public key, which is unique per cluster. The sealed YAML files in the repo are valid only for the cluster that sealed them.
**How to avoid:** Commit the `sealed-secret.yaml` files to Git after the bootstrap script generates them. They are safe to commit (encrypted). The bootstrap script already generates them to `base/{service}/sealed-secret.yaml` -- ensure they are committed and referenced in the service's `kustomization.yaml`.
**Warning signs:** Applications stuck in OutOfSync; pods failing with `secret not found` errors.

### Pitfall 2: Namespace Ownership Conflict
**What goes wrong:** Multiple ArgoCD Applications try to manage the same `micro-commerce` namespace, causing sync conflicts.
**Why it happens:** The `namespace.yaml` is referenced in `base/kustomization.yaml`, and if both the root kustomization and individual service applications try to create the namespace, ArgoCD reports conflicts.
**How to avoid:** Have exactly one Application own the namespace resource, or use `syncOptions: [CreateNamespace=true]` on child apps and remove the explicit `namespace.yaml` from Kustomize resources. Alternatively, manage the namespace outside ArgoCD (in the bootstrap script).
**Warning signs:** `ComparisonError` or `already exists` errors in ArgoCD sync status.

### Pitfall 3: Client-Side Apply Migration Bug (v3.3.0/v3.3.1)
**What goes wrong:** ArgoCD fails to sync with `failed to perform client-side apply migration` errors.
**Why it happens:** A regression in ArgoCD v3.3.0 and v3.3.1 broke the client-side apply migration path (GitHub #26279).
**How to avoid:** Use v3.3.2 specifically, which contains the fix.
**Warning signs:** Sync errors mentioning "client-side apply migration" immediately after ArgoCD installation.

### Pitfall 4: ArgoCD CRD Size Requires Server-Side Apply
**What goes wrong:** `kubectl apply` fails with "metadata.annotations too long" or similar error.
**Why it happens:** ArgoCD CRDs exceed the 256KB annotation size limit for client-side apply.
**How to avoid:** Always use `--server-side --force-conflicts` flags when installing ArgoCD manifests.
**Warning signs:** kubectl errors during initial ArgoCD installation mentioning annotation size.

### Pitfall 5: Image Pull Failures for Local kind Images
**What goes wrong:** ArgoCD syncs the manifests but pods fail with `ImagePullBackOff` because the locally-built images (e.g., `apiservice:dev`) are not available in kind.
**Why it happens:** ArgoCD syncing manifests does not load Docker images into kind. Images must be pre-loaded via `kind load docker-image`.
**How to avoid:** The bootstrap script must continue to handle image building and loading into kind before ArgoCD takes over sync. ArgoCD manages the manifest state, not the image availability.
**Warning signs:** Pods stuck in `ImagePullBackOff` or `ErrImagePull` after ArgoCD sync.

### Pitfall 6: Kustomize Image Transformer Not Applied by Individual Base Apps
**What goes wrong:** Child Applications pointing to individual `base/{service}/` directories get the base image tags (e.g., `apiservice:dev` hardcoded in deployment.yaml) rather than the overlay-patched tags.
**Why it happens:** The Kustomize `images:` transformer only applies when rendering through the overlay path that contains it.
**How to avoid:** Either point app services to the overlay (not individual base dirs), or ensure the base deployment.yaml already has the correct image tags.
**Warning signs:** Pods running with wrong image tags; overlay image patches not taking effect.

## Code Examples

Verified patterns from official ArgoCD documentation:

### ArgoCD Installation in Bootstrap Script
```bash
# Source: https://argo-cd.readthedocs.io/en/stable/getting_started/
# Install ArgoCD v3.3.2 (non-HA, single cluster)
kubectl create namespace argocd
kubectl apply -n argocd --server-side --force-conflicts \
  -f https://raw.githubusercontent.com/argoproj/argo-cd/v3.3.2/manifests/install.yaml

# Wait for ArgoCD to be ready
kubectl rollout status deployment argocd-server -n argocd --timeout=120s
kubectl rollout status deployment argocd-repo-server -n argocd --timeout=120s
kubectl rollout status deployment argocd-applicationset-controller -n argocd --timeout=120s

# Disable TLS for local kind access (HTTP on port 8080)
kubectl patch configmap argocd-cmd-params-cm -n argocd \
  --type merge -p '{"data":{"server.insecure":"true"}}'
kubectl rollout restart deployment argocd-server -n argocd
kubectl rollout status deployment argocd-server -n argocd --timeout=120s

# Expose via NodePort
kubectl apply -f "$SCRIPT_DIR/argocd/argocd-server-nodeport.yaml"

# Retrieve initial admin password
ARGOCD_PASSWORD=$(kubectl -n argocd get secret argocd-initial-admin-secret \
  -o jsonpath='{.data.password}' | base64 --decode)
echo "ArgoCD UI: http://localhost:38443 (user: admin, pass: $ARGOCD_PASSWORD)"
```

### Root App-of-Apps Application
```yaml
# Source: https://argo-cd.readthedocs.io/en/stable/operator-manual/cluster-bootstrapping/
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: micro-commerce-root
  namespace: argocd
  finalizers:
  - resources-finalizer.argocd.argoproj.io
spec:
  project: default
  source:
    repoURL: https://github.com/baotoq/micro-commerce.git
    targetRevision: master
    path: infra/k8s/argocd/apps
  destination:
    server: https://kubernetes.default.svc
    namespace: argocd
  syncPolicy:
    automated:
      selfHeal: true
      prune: true
```

### Child Application (Kustomize-based)
```yaml
# Source: https://argo-cd.readthedocs.io/en/stable/user-guide/kustomize/
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: micro-commerce-dev
  namespace: argocd
  finalizers:
  - resources-finalizer.argocd.argoproj.io
spec:
  project: default
  source:
    repoURL: https://github.com/baotoq/micro-commerce.git
    targetRevision: master
    path: infra/k8s/overlays/dev
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

### Retrieving ArgoCD Admin Password
```bash
# Source: https://argo-cd.readthedocs.io/en/stable/getting_started/
kubectl -n argocd get secret argocd-initial-admin-secret \
  -o jsonpath='{.data.password}' | base64 --decode
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| `kubectl apply -k` in scripts | ArgoCD automated sync from Git | Ongoing (this phase) | Drift detection, auto-heal, audit trail, UI visibility |
| ArgoCD v2.x with separate CRD install | ArgoCD v3.x with `--server-side --force-conflicts` | ArgoCD 3.0 (2025) | CRDs bundled in install.yaml; must use server-side apply due to size |
| Client-side apply as default | Server-side apply migration in v3.3.x | ArgoCD 3.3.0 | v3.3.0/3.3.1 had a migration bug; v3.3.2 fixes it |
| App-of-apps as primary pattern | ApplicationSets as recommended for dynamic workloads | ArgoCD 2.6+ | For static service lists like this project, app-of-apps remains simpler and more explicit |

**Deprecated/outdated:**
- ArgoCD v2.x: Still supported but v3.x is current stable line
- `argocd-util` CLI: Replaced by `argocd admin` subcommands
- Client-side apply default: v3.3+ migrates to server-side apply

## Open Questions

1. **App granularity: one child per service vs one child for the full overlay?**
   - What we know: The current Kustomize overlay (`overlays/dev/`) aggregates all services in one kustomization. Individual base directories exist per service. ArgoCD can point to either.
   - What's unclear: Whether to create 6 individual child Applications (one per service) each pointing to its base directory, or fewer Applications pointing to the overlay. Per-service apps are more granular but the image tag overlay only applies when rendered through `overlays/dev/`.
   - Recommendation: Use a hybrid approach -- one child Application for the full `overlays/dev/` Kustomize path (covers all 6 services with correct image tags), plus the namespace managed by `CreateNamespace=true` sync option. This gives the app-of-apps structure while respecting the existing Kustomize overlay architecture. If true per-service granularity is needed for Phase 27 CI, refactor overlays then.

2. **SealedSecret YAML files in Git**
   - What we know: The bootstrap script generates `sealed-secret.yaml` files dynamically. They are cluster-specific (encrypted with that cluster's controller key).
   - What's unclear: Whether these files are currently committed to Git or `.gitignore`d. ArgoCD needs them in Git to sync.
   - Recommendation: Verify during implementation. If not committed, the bootstrap script should commit them after generation, or ArgoCD should be configured to ignore missing SealedSecret resources.

3. **Bootstrap script modification scope**
   - What we know: The bootstrap script currently does everything: cluster creation, SealedSecrets, manifests, image building.
   - What's unclear: How much of the script should change. ArgoCD replaces `kubectl apply -k` but not cluster/image/secret operations.
   - Recommendation: Keep the bootstrap script for: kind cluster creation, SealedSecrets controller install, secret sealing, image building/loading, and ArgoCD installation + root app creation. Remove the manual `kubectl apply -k` steps that ArgoCD now handles.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | Manual validation via kubectl + ArgoCD UI (infrastructure phase, no unit test framework) |
| Config file | none -- validation is via kubectl commands |
| Quick run command | `kubectl get applications -n argocd` |
| Full suite command | `kubectl get applications -n argocd -o json \| jq '.items[] \| {name: .metadata.name, health: .status.health.status, sync: .status.sync.status}'` |

### Phase Requirements to Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| GOPS-01 | ArgoCD installed, all services Synced and Healthy | smoke | `kubectl get applications -n argocd -o jsonpath='{range .items[*]}{.metadata.name}{"\t"}{.status.sync.status}{"\t"}{.status.health.status}{"\n"}{end}'` | N/A -- kubectl |
| GOPS-01 | ArgoCD UI reachable | manual-only | Open `http://localhost:38443` in browser | N/A -- manual |
| GOPS-02 | App-of-apps root manages children | smoke | `kubectl get applications -n argocd -l app.kubernetes.io/instance=micro-commerce-root` | N/A -- kubectl |
| GOPS-02 | Self-heal restores deleted deployment | smoke | `kubectl delete deployment apiservice -n micro-commerce && sleep 10 && kubectl get deployment apiservice -n micro-commerce` | N/A -- kubectl |

### Sampling Rate
- **Per task commit:** `kubectl get applications -n argocd` (verify ArgoCD sees apps)
- **Per wave merge:** Full validation: all apps Synced+Healthy, delete+restore test
- **Phase gate:** All three success criteria verified before `/gsd:verify-work`

### Wave 0 Gaps
None -- this phase creates Kubernetes manifests and shell scripts, not application code with test frameworks.

## Sources

### Primary (HIGH confidence)
- [ArgoCD v3.3.2 Release](https://github.com/argoproj/argo-cd/releases/tag/v3.3.2) - Confirmed client-side apply migration bug fix, release date 2026-02-22
- [ArgoCD Getting Started](https://argo-cd.readthedocs.io/en/stable/getting_started/) - Installation commands, admin password retrieval, server-side apply requirements
- [ArgoCD Cluster Bootstrapping / App-of-Apps](https://argo-cd.readthedocs.io/en/stable/operator-manual/cluster-bootstrapping/) - Root Application pattern, directory structure, child Application examples, finalizer requirement
- [ArgoCD Declarative Setup](https://argo-cd.readthedocs.io/en/stable/operator-manual/declarative-setup/) - Application CRD spec, AppProject config, repository secrets
- [ArgoCD Kustomize Support](https://argo-cd.readthedocs.io/en/stable/user-guide/kustomize/) - Auto-detection of kustomization.yaml, overlay path configuration
- [ArgoCD Auto Sync](https://argo-cd.readthedocs.io/en/stable/user-guide/auto_sync/) - selfHeal, prune, automated sync YAML spec
- [ArgoCD Directory Source](https://argo-cd.readthedocs.io/en/latest/user-guide/directory/) - Plain YAML directory handling, recurse option, include/exclude patterns
- [ArgoCD Installation](https://argo-cd.readthedocs.io/en/stable/operator-manual/installation/) - install.yaml vs namespace-install.yaml, HA vs non-HA
- [ArgoCD TLS Configuration](https://argo-cd.readthedocs.io/en/stable/operator-manual/tls/) - Disabling TLS via argocd-cmd-params-cm ConfigMap

### Secondary (MEDIUM confidence)
- [ArgoCD Resource Health](https://argo-cd.readthedocs.io/en/latest/operator-manual/health/) - Built-in health checks for Deployments, StatefulSets, Services
- [CNCF Blog: App-of-Apps Pattern](https://www.cncf.io/blog/2025/10/07/managing-kubernetes-workloads-using-the-app-of-apps-pattern-in-argocd-2/) - Best practices, security considerations

### Tertiary (LOW confidence)
- None -- all findings verified with official documentation

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - ArgoCD v3.3.2 confirmed via official GitHub releases; installation method from official docs
- Architecture: HIGH - App-of-apps pattern documented in official ArgoCD cluster bootstrapping guide; Kustomize integration confirmed via official docs
- Pitfalls: HIGH - Client-side apply bug confirmed in v3.3.2 release notes; SealedSecrets interaction based on understanding of both tools; namespace conflict well-documented in ArgoCD community

**Research date:** 2026-03-02
**Valid until:** 2026-04-02 (30 days -- ArgoCD is mature, stable release cadence)

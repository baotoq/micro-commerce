# Phase 24: Infrastructure Manifests and Secrets - Research

**Researched:** 2026-02-26
**Domain:** Kubernetes infrastructure (kind, Kustomize, StatefulSets, SealedSecrets)
**Confidence:** HIGH

## Summary

Phase 24 provisions the infrastructure layer of the MicroCommerce Kubernetes deployment: PostgreSQL, RabbitMQ, and Keycloak running stably in a kind cluster with all credentials encrypted via SealedSecrets. This phase creates the `infra/k8s/` directory structure with Kustomize base/overlay manifests, a kind cluster config, and a bootstrap script that goes from zero to running cluster.

The key technologies are all mature and well-documented: kind (local K8s clusters), Kustomize (built into kubectl), Bitnami SealedSecrets (encrypt-at-rest for secrets), and standard Kubernetes resource types (StatefulSet, Deployment, Service, ConfigMap). The main complexity is in the bootstrap script sequencing (cluster creation -> SealedSecrets controller -> seal secrets -> apply manifests) and correctly configuring kind port mappings so infrastructure services are accessible from localhost at offset ports (35432, 35672, 38080).

**Primary recommendation:** Use a single `infra/k8s/bootstrap.sh` script that orchestrates the full flow: create kind cluster, install SealedSecrets controller, wait for it to be ready, seal dev secrets, apply Kustomize base manifests, and wait for pods to reach Running state. Keep manifests simple -- PostgreSQL as a StatefulSet with PVC, RabbitMQ as a Deployment (single-node dev, no need for StatefulSet), Keycloak as a Deployment with realm JSON mounted via ConfigMap.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- All K8s manifests live under `infra/k8s/`
- Kustomize structure: `base/` for environment-neutral resources, `overlays/` for environment patches
- Inside `base/`, subdirectories per service: `base/postgres/`, `base/rabbitmq/`, `base/keycloak/`
- Kind cluster config at `infra/k8s/kind-config.yaml`
- Full bootstrap script that creates kind cluster + installs SealedSecrets controller + seals dev secrets in one command
- SealedSecret YAML files live alongside their service manifests (e.g., `base/postgres/sealed-secret.yaml`)
- Dev credential values use well-known defaults: postgres/postgres, guest/guest for RabbitMQ, admin/admin for Keycloak
- Single node (control-plane only, no separate worker)
- Fixed cluster name: `micro-commerce`
- Standard offset port mappings to avoid conflicts with Aspire local dev: PostgreSQL 35432, RabbitMQ management 35672, Keycloak 38080
- Use kind's default rancher local-path-provisioner for PVC storage
- ConfigMap created from realm JSON, volume-mounted into Keycloak container with `--import-realm` flag
- Realm JSON (`micro-commerce-realm.json`) copied from `src/MicroCommerce.AppHost/Realms/` into `infra/k8s/base/keycloak/`
- Keycloak runs in dev mode (`start-dev`)
- Startup probes: 15s initial delay, 60s failure threshold timeout for Keycloak and ApiService

### Claude's Discretion
- Exact bootstrap script structure and error handling
- RabbitMQ Deployment resource limits
- PostgreSQL StatefulSet replica count (1 for dev)
- Namespace creation approach (in bootstrap script vs manifest)
- SealedSecrets controller version selection

### Deferred Ideas (OUT OF SCOPE)
None -- discussion stayed within phase scope
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| INFRA-01 | PostgreSQL runs as a StatefulSet with PersistentVolumeClaim in the kind cluster | PostgreSQL StatefulSet pattern with `volumeClaimTemplates` using kind's default `standard` StorageClass (local-path-provisioner). Single replica, headless Service for stable DNS. |
| INFRA-02 | RabbitMQ runs as a Deployment with Service in the kind cluster | Simple Deployment with `rabbitmq:4-management` image, ClusterIP Service exposing AMQP 5672 and management 15672. NodePort for management UI access via offset port. |
| INFRA-03 | Keycloak runs as a Deployment with realm JSON imported via ConfigMap | Keycloak `start-dev --import-realm` with ConfigMap volume-mounted to `/opt/keycloak/data/import/`. Health checks on management port 9000. `KC_BOOTSTRAP_ADMIN_USERNAME`/`KC_BOOTSTRAP_ADMIN_PASSWORD` env vars. |
| INFRA-04 | kind cluster can be created from a config file with port mappings for local access | `kind-config.yaml` with `extraPortMappings` mapping NodePort containerPorts to offset hostPorts. `kind create cluster --name micro-commerce --config kind-config.yaml`. |
| INFRA-05 | Startup probes protect Keycloak and ApiService from premature liveness failures | `startupProbe` with `httpGet` on health endpoint, `initialDelaySeconds: 15`, `failureThreshold: 20`, `periodSeconds: 3` (totaling 75s window). While startup probe runs, liveness/readiness are disabled. |
| GOPS-03 | Sealed Secrets controller encrypts all credentials; SealedSecret YAMLs committed to Git | SealedSecrets controller v0.27.x installed via `kubectl apply -f controller.yaml`. `kubeseal` CLI seals plain Secrets into SealedSecret resources. Public key can be exported for offline sealing. |
</phase_requirements>

## Standard Stack

### Core
| Tool | Version | Purpose | Why Standard |
|------|---------|---------|--------------|
| kind | 0.27.x | Local K8s cluster in Docker | De facto standard for local K8s development; lighter than minikube, used by K8s CI itself |
| Kustomize | built-in (kubectl) | Manifest customization without templating | Built into kubectl; simpler than Helm for single-project; no template language to learn |
| SealedSecrets | v0.27.x controller / v0.27.x kubeseal CLI | Encrypt K8s Secrets for safe Git storage | Industry standard for GitOps secret management; asymmetric encryption, controller decrypts at deploy |
| PostgreSQL | 17 (docker image `postgres:17`) | Database | Same major version as Aspire dev environment |
| RabbitMQ | 4.x (docker image `rabbitmq:4-management`) | Message broker for K8s deployment | MassTransit RabbitMQ transport needed in K8s (Azure SB emulator cannot run in K8s) |
| Keycloak | 26.x (docker image `quay.io/keycloak/keycloak:26.0`) | Identity provider | Same version as Aspire Keycloak integration |

### Supporting
| Tool | Version | Purpose | When to Use |
|------|---------|---------|-------------|
| kubectl | 1.32.x | K8s CLI | Applying manifests, port-forwarding, debugging |
| kubeseal | 0.27.x | SealedSecrets CLI | Encrypting secrets before committing to Git |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| SealedSecrets | SOPS + age | SOPS is more flexible but requires external key management; SealedSecrets is simpler for cluster-scoped secrets |
| Kustomize | Helm | Helm is more powerful (templating, repositories) but overkill for single-project; Kustomize is built into kubectl |
| RabbitMQ Deployment | RabbitMQ Cluster Operator | Operator is production-grade but heavy for single-node dev; plain Deployment is simpler |
| kind | minikube / k3d | minikube is heavier; k3d uses k3s (not vanilla K8s); kind uses official K8s images |

**Installation:**
```bash
# kind (macOS)
brew install kind

# kubeseal
brew install kubeseal

# Or download kubeseal binary directly
curl -OL "https://github.com/bitnami-labs/sealed-secrets/releases/download/v0.27.3/kubeseal-0.27.3-darwin-amd64.tar.gz"
```

## Architecture Patterns

### Recommended Directory Structure
```
infra/k8s/
├── kind-config.yaml                    # kind cluster configuration
├── bootstrap.sh                        # One-command cluster setup
├── base/
│   ├── kustomization.yaml              # Base kustomization (references all subdirs)
│   ├── namespace.yaml                  # micro-commerce namespace
│   ├── postgres/
│   │   ├── kustomization.yaml
│   │   ├── statefulset.yaml
│   │   ├── service.yaml                # Headless + NodePort services
│   │   ├── sealed-secret.yaml          # Encrypted credentials
│   │   └── configmap.yaml              # init scripts (optional)
│   ├── rabbitmq/
│   │   ├── kustomization.yaml
│   │   ├── deployment.yaml
│   │   ├── service.yaml                # ClusterIP + NodePort for management
│   │   └── sealed-secret.yaml          # Encrypted credentials
│   └── keycloak/
│       ├── kustomization.yaml
│       ├── deployment.yaml
│       ├── service.yaml                # ClusterIP + NodePort for admin UI
│       ├── sealed-secret.yaml          # Encrypted admin credentials
│       ├── configmap.yaml              # Realm JSON via configMapGenerator
│       └── micro-commerce-realm.json   # Copied from AppHost/Realms/
└── overlays/
    └── dev/
        └── kustomization.yaml          # Dev overlay (currently just references base)
```

### Pattern 1: kind NodePort Port Mapping
**What:** Map kind container ports to host ports so K8s NodePort services are accessible from localhost.
**When to use:** Always, for local access to infrastructure services.
**Example:**
```yaml
# infra/k8s/kind-config.yaml
# Source: https://kind.sigs.k8s.io/docs/user/configuration/
kind: Cluster
apiVersion: kind.x-k8s.io/v1alpha4
name: micro-commerce
nodes:
- role: control-plane
  extraPortMappings:
  # PostgreSQL
  - containerPort: 30432
    hostPort: 35432
    protocol: TCP
  # RabbitMQ Management UI
  - containerPort: 30672
    hostPort: 35672
    protocol: TCP
  # Keycloak
  - containerPort: 30080
    hostPort: 38080
    protocol: TCP
```
**Key insight:** The `containerPort` in kind config must match the `nodePort` in the K8s Service spec. The `hostPort` is what you access from `localhost`. All three must be aligned.

### Pattern 2: PostgreSQL StatefulSet with PVC
**What:** StatefulSet ensures stable pod identity and persistent storage via `volumeClaimTemplates`.
**When to use:** For any stateful workload that needs data persistence across pod restarts.
**Example:**
```yaml
# Source: Kubernetes docs + PostgreSQL best practices
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: postgres
  namespace: micro-commerce
spec:
  serviceName: postgres
  replicas: 1
  selector:
    matchLabels:
      app: postgres
  template:
    metadata:
      labels:
        app: postgres
    spec:
      containers:
      - name: postgres
        image: postgres:17
        ports:
        - containerPort: 5432
        env:
        - name: POSTGRES_USER
          valueFrom:
            secretKeyRef:
              name: postgres-credentials
              key: username
        - name: POSTGRES_PASSWORD
          valueFrom:
            secretKeyRef:
              name: postgres-credentials
              key: password
        - name: POSTGRES_DB
          value: appdb
        volumeMounts:
        - name: postgres-data
          mountPath: /var/lib/postgresql/data
        readinessProbe:
          exec:
            command: ["pg_isready", "-U", "postgres"]
          initialDelaySeconds: 5
          periodSeconds: 10
        livenessProbe:
          exec:
            command: ["pg_isready", "-U", "postgres"]
          initialDelaySeconds: 15
          periodSeconds: 20
  volumeClaimTemplates:
  - metadata:
      name: postgres-data
    spec:
      accessModes: ["ReadWriteOnce"]
      resources:
        requests:
          storage: 1Gi
```
**Key insight:** kind's default `standard` StorageClass (local-path-provisioner) automatically provisions local volumes. No extra StorageClass configuration needed.

### Pattern 3: Keycloak Realm Import via ConfigMap
**What:** Mount realm JSON into Keycloak's import directory and start with `--import-realm`.
**When to use:** To pre-load the MicroCommerce realm configuration on first boot.
**Example:**
```yaml
# Source: https://www.keycloak.org/server/importExport
apiVersion: apps/v1
kind: Deployment
metadata:
  name: keycloak
  namespace: micro-commerce
spec:
  replicas: 1
  selector:
    matchLabels:
      app: keycloak
  template:
    spec:
      containers:
      - name: keycloak
        image: quay.io/keycloak/keycloak:26.0
        args: ["start-dev", "--import-realm"]
        env:
        - name: KC_BOOTSTRAP_ADMIN_USERNAME
          valueFrom:
            secretKeyRef:
              name: keycloak-credentials
              key: admin-username
        - name: KC_BOOTSTRAP_ADMIN_PASSWORD
          valueFrom:
            secretKeyRef:
              name: keycloak-credentials
              key: admin-password
        - name: KC_HEALTH_ENABLED
          value: "true"
        ports:
        - containerPort: 8080
          name: http
        - containerPort: 9000
          name: management
        volumeMounts:
        - name: realm-config
          mountPath: /opt/keycloak/data/import
        startupProbe:
          httpGet:
            path: /health/started
            port: 9000
          initialDelaySeconds: 15
          periodSeconds: 3
          failureThreshold: 20
        livenessProbe:
          httpGet:
            path: /health/live
            port: 9000
          periodSeconds: 15
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 9000
          periodSeconds: 10
      volumes:
      - name: realm-config
        configMap:
          name: keycloak-realm
```

### Pattern 4: SealedSecrets Workflow
**What:** Create plain K8s Secret -> seal with kubeseal -> commit SealedSecret YAML -> controller decrypts at deploy.
**When to use:** For any credential that must be in Git.
**Example:**
```bash
# Source: https://github.com/bitnami-labs/sealed-secrets
# Step 1: Create a plain secret (dry-run, never applied to cluster)
kubectl create secret generic postgres-credentials \
  --namespace micro-commerce \
  --from-literal=username=postgres \
  --from-literal=password=postgres \
  --dry-run=client -o yaml > /tmp/postgres-secret.yaml

# Step 2: Seal it with the controller's public key
kubeseal \
  --controller-name=sealed-secrets-controller \
  --controller-namespace=kube-system \
  --format yaml < /tmp/postgres-secret.yaml > infra/k8s/base/postgres/sealed-secret.yaml

# Step 3: Clean up plain secret
rm /tmp/postgres-secret.yaml
```

### Pattern 5: Bootstrap Script Structure
**What:** Single entry point that creates cluster and provisions all infrastructure.
**When to use:** Always -- this is the developer experience entry point.
**Example:**
```bash
#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CLUSTER_NAME="micro-commerce"

# 1. Create kind cluster (idempotent -- skip if exists)
if ! kind get clusters 2>/dev/null | grep -q "^${CLUSTER_NAME}$"; then
  echo "Creating kind cluster..."
  kind create cluster --name "$CLUSTER_NAME" --config "$SCRIPT_DIR/kind-config.yaml"
else
  echo "Cluster '$CLUSTER_NAME' already exists, skipping creation."
fi

# 2. Set kubectl context
kubectl cluster-info --context "kind-${CLUSTER_NAME}"

# 3. Install SealedSecrets controller
echo "Installing SealedSecrets controller..."
kubectl apply -f https://github.com/bitnami-labs/sealed-secrets/releases/download/v0.27.3/controller.yaml

# 4. Wait for controller to be ready
echo "Waiting for SealedSecrets controller..."
kubectl rollout status deployment/sealed-secrets-controller -n kube-system --timeout=120s

# 5. Create namespace
kubectl create namespace micro-commerce --dry-run=client -o yaml | kubectl apply -f -

# 6. Seal secrets (requires controller running)
echo "Sealing dev secrets..."
# [seal postgres, rabbitmq, keycloak secrets]

# 7. Apply infrastructure manifests
echo "Applying infrastructure manifests..."
kubectl apply -k "$SCRIPT_DIR/base/"

# 8. Wait for pods
echo "Waiting for infrastructure pods..."
kubectl wait --for=condition=ready pod -l app=postgres -n micro-commerce --timeout=120s
kubectl wait --for=condition=ready pod -l app=rabbitmq -n micro-commerce --timeout=120s
kubectl wait --for=condition=ready pod -l app=keycloak -n micro-commerce --timeout=180s

echo "Infrastructure ready!"
```

### Anti-Patterns to Avoid
- **Plaintext secrets in YAML:** Never commit K8s Secret resources directly; always use SealedSecret.
- **Using `latest` image tag:** Pin image versions (e.g., `postgres:17`, `rabbitmq:4-management`, `quay.io/keycloak/keycloak:26.0`).
- **Skipping headless Service for StatefulSet:** PostgreSQL StatefulSet requires a headless Service (`clusterIP: None`) for stable DNS names.
- **Mounting PVC at `/var/lib/postgresql/data` directly:** PostgreSQL requires a subdirectory; use `subPath: pgdata` or set `PGDATA=/var/lib/postgresql/data/pgdata` to avoid "initdb: directory is not empty" errors from kind's local-path-provisioner.
- **Hardcoding NodePort values without matching kind containerPort:** The kind `extraPortMappings.containerPort` MUST equal the K8s Service `spec.ports[].nodePort`.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Secret encryption for Git | Custom encryption scripts | SealedSecrets (kubeseal + controller) | Asymmetric encryption, automatic key rotation, controller-managed decryption |
| PVC provisioning in kind | Manual PV creation | kind's built-in local-path-provisioner | Automatically provisions PVs from StorageClass `standard`; zero config |
| Keycloak realm setup | Admin API scripts | `--import-realm` with ConfigMap | Built-in Keycloak feature; idempotent (uses IGNORE_EXISTING strategy by default) |
| Health check endpoints | Custom health scripts | Built-in probe endpoints (`pg_isready`, `/health/started`, `rabbitmq-diagnostics`) | Standard, well-tested, production-proven health checks |

**Key insight:** Every infrastructure component in this phase has well-established Kubernetes patterns. The risk is in the integration (port mapping alignment, secret sealing workflow, probe timing) rather than in novel technology.

## Common Pitfalls

### Pitfall 1: kind Port Mapping Misalignment
**What goes wrong:** Services are not accessible from localhost because the kind `containerPort`, the K8s Service `nodePort`, and the `hostPort` are not properly aligned.
**Why it happens:** Three different config files must agree on port numbers. Missing any one breaks the chain.
**How to avoid:** Define port constants once and use consistently:
- kind `containerPort: 30432` = K8s Service `nodePort: 30432` = host access at `hostPort: 35432`
- Document the mapping table in the bootstrap script or README.
**Warning signs:** `curl localhost:35432` times out or connection refused.

### Pitfall 2: PostgreSQL "initdb: directory is not empty"
**What goes wrong:** PostgreSQL container fails to start because the PVC mount point `/var/lib/postgresql/data` already contains files from the local-path-provisioner (like a `lost+found` directory).
**Why it happens:** kind's local-path-provisioner creates a host directory that may contain filesystem artifacts.
**How to avoid:** Set `PGDATA` environment variable to a subdirectory: `PGDATA=/var/lib/postgresql/data/pgdata`. This tells PostgreSQL to use a subdirectory within the mount, avoiding the "not empty" check on the mount root.
**Warning signs:** Pod in CrashLoopBackOff with initdb errors in logs.

### Pitfall 3: SealedSecrets Controller Not Ready During Sealing
**What goes wrong:** `kubeseal` fails with connection errors because the controller hasn't finished starting.
**Why it happens:** Bootstrap script runs `kubeseal` immediately after `kubectl apply -f controller.yaml` without waiting.
**How to avoid:** Add `kubectl rollout status deployment/sealed-secrets-controller -n kube-system --timeout=120s` before any `kubeseal` calls.
**Warning signs:** `kubeseal` errors like "cannot fetch certificate" or "connection refused".

### Pitfall 4: Keycloak Admin Credentials Environment Variables
**What goes wrong:** Keycloak starts without an admin user because the wrong environment variable names are used.
**Why it happens:** Keycloak 26.x uses `KC_BOOTSTRAP_ADMIN_USERNAME` / `KC_BOOTSTRAP_ADMIN_PASSWORD`. Older docs reference `KEYCLOAK_ADMIN` / `KEYCLOAK_ADMIN_PASSWORD` which are deprecated.
**How to avoid:** Use `KC_BOOTSTRAP_ADMIN_USERNAME` and `KC_BOOTSTRAP_ADMIN_PASSWORD` for Keycloak 26.x.
**Warning signs:** Keycloak starts but admin console login fails.

### Pitfall 5: Keycloak Health Checks on Wrong Port
**What goes wrong:** Startup/liveness/readiness probes fail because they target port 8080 instead of the management port.
**Why it happens:** Keycloak 26.x exposes health endpoints on management port 9000 by default, not on the main HTTP port 8080.
**How to avoid:** Configure probes to use port 9000 (management port) and enable health with `KC_HEALTH_ENABLED=true`.
**Warning signs:** Probes fail with 404 errors; pod keeps restarting.

### Pitfall 6: SealedSecret Namespace/Name Mismatch
**What goes wrong:** SealedSecret is created but the controller doesn't produce the expected Secret.
**Why it happens:** SealedSecrets are scoped to a specific namespace and name by default (`strict` scope). If the original Secret had a different namespace than the SealedSecret, the controller rejects it.
**How to avoid:** Always specify `--namespace micro-commerce` when creating the dry-run Secret AND when applying the SealedSecret. Verify the embedded namespace matches.
**Warning signs:** SealedSecret exists but corresponding Secret is missing; controller logs show "unseal error".

### Pitfall 7: Startup Probe Timing for Slow EF Core Migrations
**What goes wrong:** ApiService pod gets killed during first boot because liveness probe fails while EF Core migrations are running.
**Why it happens:** Without a startup probe, the liveness probe starts immediately. EF Core migrations can take 30-60s on first boot, exceeding liveness probe thresholds.
**How to avoid:** Use a `startupProbe` with generous timeout (e.g., `failureThreshold: 20 * periodSeconds: 3 = 60s`). While startup probe is active, liveness/readiness probes are disabled. Once startup probe succeeds, normal probes take over.
**Warning signs:** Pod repeatedly CrashLoopBackOff during first deployment; works after manual retry.

## Code Examples

### Kustomize Base kustomization.yaml
```yaml
# infra/k8s/base/kustomization.yaml
# Source: https://kubectl.docs.kubernetes.io/references/kustomize/kustomization/
apiVersion: kustomize.config.k8s.io/v1beta1
kind: Kustomization

namespace: micro-commerce

resources:
- namespace.yaml
- postgres/
- rabbitmq/
- keycloak/
```

### Namespace Manifest
```yaml
# infra/k8s/base/namespace.yaml
apiVersion: v1
kind: Namespace
metadata:
  name: micro-commerce
```

### PostgreSQL Headless Service
```yaml
# infra/k8s/base/postgres/service.yaml
apiVersion: v1
kind: Service
metadata:
  name: postgres
  namespace: micro-commerce
spec:
  # Headless service for StatefulSet (required)
  clusterIP: None
  selector:
    app: postgres
  ports:
  - port: 5432
    targetPort: 5432
---
# NodePort for local access via kind port mapping
apiVersion: v1
kind: Service
metadata:
  name: postgres-nodeport
  namespace: micro-commerce
spec:
  type: NodePort
  selector:
    app: postgres
  ports:
  - port: 5432
    targetPort: 5432
    nodePort: 30432
```

### RabbitMQ Deployment
```yaml
# infra/k8s/base/rabbitmq/deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: rabbitmq
  namespace: micro-commerce
spec:
  replicas: 1
  selector:
    matchLabels:
      app: rabbitmq
  template:
    metadata:
      labels:
        app: rabbitmq
    spec:
      containers:
      - name: rabbitmq
        image: rabbitmq:4-management
        ports:
        - containerPort: 5672
          name: amqp
        - containerPort: 15672
          name: management
        env:
        - name: RABBITMQ_DEFAULT_USER
          valueFrom:
            secretKeyRef:
              name: rabbitmq-credentials
              key: username
        - name: RABBITMQ_DEFAULT_PASS
          valueFrom:
            secretKeyRef:
              name: rabbitmq-credentials
              key: password
        readinessProbe:
          exec:
            command: ["rabbitmq-diagnostics", "ping"]
          initialDelaySeconds: 10
          periodSeconds: 10
        livenessProbe:
          exec:
            command: ["rabbitmq-diagnostics", "status"]
          initialDelaySeconds: 30
          periodSeconds: 30
        resources:
          requests:
            memory: 256Mi
            cpu: 250m
          limits:
            memory: 512Mi
            cpu: 500m
```

### Sealing Multiple Secrets in Bootstrap
```bash
seal_secret() {
  local name="$1"
  local namespace="$2"
  local output="$3"
  shift 3
  # Remaining args are --from-literal pairs

  kubectl create secret generic "$name" \
    --namespace "$namespace" \
    "$@" \
    --dry-run=client -o yaml | \
  kubeseal \
    --controller-name=sealed-secrets-controller \
    --controller-namespace=kube-system \
    --format yaml > "$output"
}

seal_secret "postgres-credentials" "micro-commerce" \
  "$SCRIPT_DIR/base/postgres/sealed-secret.yaml" \
  --from-literal=username=postgres \
  --from-literal=password=postgres

seal_secret "rabbitmq-credentials" "micro-commerce" \
  "$SCRIPT_DIR/base/rabbitmq/sealed-secret.yaml" \
  --from-literal=username=guest \
  --from-literal=password=guest

seal_secret "keycloak-credentials" "micro-commerce" \
  "$SCRIPT_DIR/base/keycloak/sealed-secret.yaml" \
  --from-literal=admin-username=admin \
  --from-literal=admin-password=admin
```

### Keycloak ConfigMap from Realm JSON (Kustomize)
```yaml
# infra/k8s/base/keycloak/kustomization.yaml
apiVersion: kustomize.config.k8s.io/v1beta1
kind: Kustomization

resources:
- deployment.yaml
- service.yaml
- sealed-secret.yaml

configMapGenerator:
- name: keycloak-realm
  files:
  - micro-commerce-realm.json
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| `KEYCLOAK_ADMIN` env var | `KC_BOOTSTRAP_ADMIN_USERNAME` env var | Keycloak 25+ (2024) | Old env vars deprecated; new names required |
| Keycloak health on main port 8080 | Health on management port 9000 | Keycloak 25+ (2024) | Probes must target port 9000, not 8080 |
| `kubeseal` v1 CLI syntax | `kubeseal` v2 with create/fetch-cert subcommands | SealedSecrets 0.24+ (2024) | Both old and new syntax still work; prefer new |
| SealedSecrets Helm-only install | `kubectl apply -f controller.yaml` from releases | Always available | YAML manifest install is simplest for bootstrap scripts |

**Deprecated/outdated:**
- `KEYCLOAK_ADMIN` / `KEYCLOAK_ADMIN_PASSWORD`: Replaced by `KC_BOOTSTRAP_ADMIN_USERNAME` / `KC_BOOTSTRAP_ADMIN_PASSWORD`
- Keycloak Operator `KeycloakRealmImport` CR: Overkill for dev; `--import-realm` flag is simpler and sufficient
- RabbitMQ Cluster Operator: Production-grade but heavy for single-node dev cluster

## Open Questions

1. **Exact SealedSecrets controller version**
   - What we know: v0.27.x is latest stable line; v0.27.3 is a recent release
   - What's unclear: Whether a newer point release exists at time of implementation
   - Recommendation: Use `v0.27.3` and pin the URL in the bootstrap script. Easy to bump later.

2. **Keycloak image version alignment with Aspire**
   - What we know: Aspire.Hosting.Keycloak 13.1.0-preview pulls a Keycloak image (likely 26.x); the exact version is abstracted by the Aspire integration
   - What's unclear: Exact pinned version Aspire uses internally
   - Recommendation: Use `quay.io/keycloak/keycloak:26.0` which is the current stable. The realm JSON format is stable across 26.x minor versions. If issues arise, check Aspire source for the exact tag.

3. **RabbitMQ image version**
   - What we know: RabbitMQ 4.x is the current major line; `rabbitmq:4-management` includes the management plugin
   - What's unclear: Whether MassTransit RabbitMQ transport has any version-specific requirements
   - Recommendation: Use `rabbitmq:4-management`. MassTransit 9.x supports RabbitMQ 3.x and 4.x.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | Shell-based validation (bash assertions + kubectl commands) |
| Config file | `infra/k8s/bootstrap.sh` (contains validation steps) |
| Quick run command | `kubectl get pods -n micro-commerce` |
| Full suite command | `./infra/k8s/bootstrap.sh && kubectl get pods -n micro-commerce -o wide` |

### Phase Requirements -> Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| INFRA-01 | PostgreSQL runs as StatefulSet with PVC | smoke | `kubectl get statefulset postgres -n micro-commerce -o jsonpath='{.status.readyReplicas}' \| grep -q 1` | No -- Wave 0 |
| INFRA-02 | RabbitMQ runs as Deployment with Service | smoke | `kubectl get deployment rabbitmq -n micro-commerce -o jsonpath='{.status.readyReplicas}' \| grep -q 1` | No -- Wave 0 |
| INFRA-03 | Keycloak runs with realm imported | smoke | `curl -sf http://localhost:38080/realms/micro-commerce \| jq -e '.realm == "micro-commerce"'` | No -- Wave 0 |
| INFRA-04 | kind cluster from config with port mappings | smoke | `kind get clusters \| grep -q micro-commerce && curl -sf http://localhost:38080/health/ready` | No -- Wave 0 |
| INFRA-05 | Startup probes protect from premature liveness | manual-only | Verify `startupProbe` in `kubectl get deployment keycloak -n micro-commerce -o yaml`; manual: restart pod and observe no premature CrashLoopBackOff | No -- Wave 0 |
| GOPS-03 | SealedSecrets encrypt credentials; no plaintext in Git | smoke | `grep -rL 'kind: SealedSecret' infra/k8s/base/*/sealed-secret.yaml \| wc -l \| grep -q 0 && ! grep -r 'kind: Secret' infra/k8s/base/` | No -- Wave 0 |

### Sampling Rate
- **Per task commit:** `kubectl get pods -n micro-commerce` (verify pods still running)
- **Per wave merge:** Full `bootstrap.sh` from clean slate (delete cluster + recreate)
- **Phase gate:** All pods Running + all smoke tests pass + `grep` for no plaintext secrets

### Wave 0 Gaps
- [ ] `infra/k8s/bootstrap.sh` -- full bootstrap script (creates cluster, installs controller, seals secrets, applies manifests)
- [ ] `infra/k8s/kind-config.yaml` -- kind cluster configuration with port mappings
- [ ] `infra/k8s/base/` -- all Kustomize base manifests (postgres, rabbitmq, keycloak)
- [ ] Validation: smoke test commands embedded in bootstrap script output or separate verify script

## Sources

### Primary (HIGH confidence)
- [/kubernetes-sigs/kind](https://github.com/kubernetes-sigs/kind) via Context7 -- cluster configuration, port mappings, local-path-provisioner behavior
- [/bitnami-labs/sealed-secrets](https://github.com/bitnami-labs/sealed-secrets) via Context7 -- kubeseal CLI usage, controller installation, offline sealing workflow
- [/kubernetes-sigs/kustomize](https://github.com/kubernetes-sigs/kustomize) via Context7 -- kustomization.yaml structure, configMapGenerator, overlay patterns
- [Keycloak health checks](https://www.keycloak.org/observability/health) -- health endpoints /health/started, /health/live, /health/ready on management port 9000
- [Keycloak import/export](https://www.keycloak.org/server/importExport) -- `--import-realm` flag, `/opt/keycloak/data/import/` directory
- [Kubernetes probes](https://kubernetes.io/docs/concepts/configuration/liveness-readiness-startup-probes/) -- startup probe disables liveness/readiness during init

### Secondary (MEDIUM confidence)
- [PostgreSQL StatefulSet patterns](https://devopscube.com/deploy-postgresql-statefulset/) -- StatefulSet YAML structure, headless Service, volumeClaimTemplates
- [kind port mapping docs](https://kind.sigs.k8s.io/docs/user/configuration/) -- extraPortMappings, containerPort/hostPort alignment
- [SealedSecrets releases](https://github.com/bitnami-labs/sealed-secrets/releases) -- v0.27.x version information, controller.yaml download
- [RabbitMQ DIY Kubernetes](https://github.com/rabbitmq/diy-kubernetes-examples) -- example manifests for kind deployment

### Tertiary (LOW confidence)
- Keycloak container image version in Aspire.Hosting.Keycloak 13.1.0 -- exact version abstracted; assumed 26.x based on release timeline

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH -- all technologies are mature, well-documented, and standard for local K8s development
- Architecture: HIGH -- directory structure and manifest patterns are well-established Kustomize conventions; port mapping pattern verified via kind docs
- Pitfalls: HIGH -- all pitfalls are well-known and documented in official docs and community resources (PGDATA subdirectory, Keycloak port 9000, SealedSecrets namespace scoping)

**Research date:** 2026-02-26
**Valid until:** 2026-03-28 (30 days -- stable technologies, unlikely to change significantly)

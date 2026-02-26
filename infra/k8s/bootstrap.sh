#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
CLUSTER_NAME="micro-commerce"
NAMESPACE="micro-commerce"
SEALED_SECRETS_VERSION="v0.27.3"

# Color output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

info() { echo -e "${GREEN}[INFO]${NC} $*"; }
warn() { echo -e "${YELLOW}[WARN]${NC} $*"; }

# --- Step 1: Create kind cluster ---
if kind get clusters 2>/dev/null | grep -q "^${CLUSTER_NAME}$"; then
  info "Cluster '${CLUSTER_NAME}' already exists, skipping creation."
else
  info "Creating kind cluster '${CLUSTER_NAME}'..."
  kind create cluster --name "$CLUSTER_NAME" --config "$SCRIPT_DIR/kind-config.yaml"
fi

# --- Step 2: Set kubectl context ---
info "Setting kubectl context..."
kubectl cluster-info --context "kind-${CLUSTER_NAME}" >/dev/null 2>&1
kubectl config use-context "kind-${CLUSTER_NAME}"

# --- Step 3: Install SealedSecrets controller ---
info "Installing SealedSecrets controller ${SEALED_SECRETS_VERSION}..."
kubectl apply -f "https://github.com/bitnami-labs/sealed-secrets/releases/download/${SEALED_SECRETS_VERSION}/controller.yaml"

# --- Step 4: Wait for SealedSecrets controller ---
info "Waiting for SealedSecrets controller to be ready..."
kubectl rollout status deployment/sealed-secrets-controller -n kube-system --timeout=120s

# --- Step 5: Create namespace ---
info "Creating namespace '${NAMESPACE}'..."
kubectl create namespace "$NAMESPACE" --dry-run=client -o yaml | kubectl apply -f -

# --- Step 6: Seal dev secrets ---
info "Sealing dev secrets..."

seal_secret() {
  local name="$1"
  local output="$2"
  shift 2
  # Remaining args are --from-literal pairs

  kubectl create secret generic "$name" \
    --namespace "$NAMESPACE" \
    "$@" \
    --dry-run=client -o yaml | \
  kubeseal \
    --controller-name=sealed-secrets-controller \
    --controller-namespace=kube-system \
    --format yaml > "$output"

  info "  Sealed: $name -> $output"
}

seal_secret "postgres-credentials" \
  "$SCRIPT_DIR/base/postgres/sealed-secret.yaml" \
  --from-literal=username=postgres \
  --from-literal=password=postgres

seal_secret "rabbitmq-credentials" \
  "$SCRIPT_DIR/base/rabbitmq/sealed-secret.yaml" \
  --from-literal=username=guest \
  --from-literal=password=guest

seal_secret "keycloak-credentials" \
  "$SCRIPT_DIR/base/keycloak/sealed-secret.yaml" \
  --from-literal=admin-username=admin \
  --from-literal=admin-password=admin

# --- Step 7: Apply infrastructure manifests ---
info "Applying infrastructure manifests..."
kubectl apply -k "$SCRIPT_DIR/base/"

# --- Step 8: Wait for pods ---
info "Waiting for PostgreSQL..."
kubectl wait --for=condition=ready pod -l app=postgres -n "$NAMESPACE" --timeout=120s

info "Waiting for RabbitMQ..."
kubectl wait --for=condition=ready pod -l app=rabbitmq -n "$NAMESPACE" --timeout=120s

info "Waiting for Keycloak (may take up to 3 minutes for realm import)..."
kubectl wait --for=condition=ready pod -l app=keycloak -n "$NAMESPACE" --timeout=180s

# --- Step 9: Build application container images ---
info "Building application container images..."

info "  Building ApiService..."
dotnet publish "$PROJECT_ROOT/src/MicroCommerce.ApiService/MicroCommerce.ApiService.csproj" \
  /t:PublishContainer \
  -p:ContainerRegistry="" \
  -p:ContainerRepository=apiservice \
  -p:ContainerImageTag=dev \
  --nologo -v quiet

info "  Building Gateway..."
dotnet publish "$PROJECT_ROOT/src/MicroCommerce.Gateway/MicroCommerce.Gateway.csproj" \
  /t:PublishContainer \
  -p:ContainerRegistry="" \
  -p:ContainerRepository=gateway \
  -p:ContainerImageTag=dev \
  --nologo -v quiet

info "  Building Web..."
docker build \
  -f "$PROJECT_ROOT/src/MicroCommerce.Web/Dockerfile" \
  -t web:dev \
  "$PROJECT_ROOT/src/MicroCommerce.Web" \
  --quiet

# --- Step 10: Load images into kind ---
info "Loading images into kind cluster..."
kind load docker-image apiservice:dev --name "$CLUSTER_NAME"
kind load docker-image gateway:dev --name "$CLUSTER_NAME"
kind load docker-image web:dev --name "$CLUSTER_NAME"

# --- Step 11: Apply application manifests ---
info "Applying application manifests..."
kubectl apply -k "$SCRIPT_DIR/overlays/dev/"

# --- Step 12: Wait for application pods ---
info "Waiting for ApiService (may take up to 3 minutes for first-boot migrations)..."
kubectl wait --for=condition=ready pod -l app=apiservice -n "$NAMESPACE" --timeout=180s

info "Waiting for Gateway..."
kubectl wait --for=condition=ready pod -l app=gateway -n "$NAMESPACE" --timeout=120s

info "Waiting for Web..."
kubectl wait --for=condition=ready pod -l app=web -n "$NAMESPACE" --timeout=120s

# --- Step 13: Print access info ---
echo ""
info "Full stack ready!"
echo ""
echo "Access points:"
echo "  PostgreSQL:          localhost:35432 (user: postgres, pass: postgres, db: appdb)"
echo "  RabbitMQ Management: http://localhost:35672 (user: guest, pass: guest)"
echo "  Keycloak Admin:      http://localhost:38080 (user: admin, pass: admin)"
echo "  Gateway (API):       http://localhost:38800"
echo ""
echo "Storefront: http://localhost:38800 (via Gateway)"
echo ""
echo "Useful commands:"
echo "  kubectl get pods -n ${NAMESPACE}"
echo "  kubectl logs -f deployment/apiservice -n ${NAMESPACE}"
echo "  kubectl logs -f deployment/gateway -n ${NAMESPACE}"
echo "  kubectl logs -f deployment/web -n ${NAMESPACE}"
echo "  kind delete cluster --name ${CLUSTER_NAME}"

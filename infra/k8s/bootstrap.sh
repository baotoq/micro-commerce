#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
CLUSTER_NAME="micro-commerce"
NAMESPACE="micro-commerce"
SEALED_SECRETS_VERSION="v0.27.3"
ARGOCD_VERSION="v3.3.2"

# Color output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

info() { echo -e "${GREEN}[INFO]${NC} $*"; }
warn() { echo -e "${YELLOW}[WARN]${NC} $*"; }
error() { echo -e "${RED}[ERROR]${NC} $*"; }

# --- Pre-flight: Check required CLI tools ---
check_prerequisites() {
  local missing=false
  for cmd in kind kubectl kubeseal docker; do
    if ! command -v "$cmd" &>/dev/null; then
      error "Required tool '$cmd' not found. Please install it before running this script."
      missing=true
    fi
  done
  if [ "$missing" = true ]; then
    error "Install missing tools:"
    error "  kind:      https://kind.sigs.k8s.io/docs/user/quick-start/#installation"
    error "  kubectl:   https://kubernetes.io/docs/tasks/tools/"
    error "  kubeseal:  https://github.com/bitnami-labs/sealed-secrets#kubeseal"
    error "  docker:    https://docs.docker.com/get-docker/"
    exit 1
  fi
  info "All required tools found."
}

check_prerequisites

# --- Trap handler: Print cleanup guidance on failure ---
cleanup_on_error() {
  echo ""
  error "Bootstrap failed! To clean up:"
  error "  kind delete cluster --name ${CLUSTER_NAME}"
  error "  kubectl config unset contexts.kind-${CLUSTER_NAME}"
  echo ""
  error "To retry, run this script again."
}

trap cleanup_on_error ERR

# --- Step 1: Create kind cluster ---
if kind get clusters 2>/dev/null | grep -q "^${CLUSTER_NAME}$"; then
  info "Cluster '${CLUSTER_NAME}' already exists, skipping creation."
else
  info "Creating kind cluster '${CLUSTER_NAME}'..."
  kind create cluster --name "$CLUSTER_NAME" --config "$SCRIPT_DIR/kind-config.yaml"
fi

# --- Step 2: Verify kubectl context targets the kind cluster ---
CURRENT_CONTEXT=$(kubectl config current-context 2>/dev/null || echo "")
EXPECTED_CONTEXT="kind-${CLUSTER_NAME}"
if [ "$CURRENT_CONTEXT" != "$EXPECTED_CONTEXT" ]; then
  error "kubectl context is '${CURRENT_CONTEXT}' but expected '${EXPECTED_CONTEXT}'"
  error "This script only runs against the local kind cluster."
  error "Switch context: kubectl config use-context ${EXPECTED_CONTEXT}"
  exit 1
fi
info "kubectl context verified: ${EXPECTED_CONTEXT}"
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

# --- Step 6: Seal dev secrets and apply directly ---
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

seal_secret "web-secrets" \
  "$SCRIPT_DIR/base/web/sealed-secret.yaml" \
  --from-literal=auth-secret=k8s-dev-auth-secret-change-in-production \
  --from-literal=keycloak-client-secret=nextjs-app-secret

info "Applying sealed secrets..."
kubectl apply -f "$SCRIPT_DIR/base/postgres/sealed-secret.yaml"
kubectl apply -f "$SCRIPT_DIR/base/rabbitmq/sealed-secret.yaml"
kubectl apply -f "$SCRIPT_DIR/base/keycloak/sealed-secret.yaml"
kubectl apply -f "$SCRIPT_DIR/base/web/sealed-secret.yaml"

# --- Step 7: Install ArgoCD ---
info "Installing ArgoCD ${ARGOCD_VERSION}..."
kubectl create namespace argocd --dry-run=client -o yaml | kubectl apply -f -
kubectl apply -n argocd --server-side --force-conflicts \
  -f "https://raw.githubusercontent.com/argoproj/argo-cd/${ARGOCD_VERSION}/manifests/install.yaml"

# --- Step 8: Wait for ArgoCD to be ready ---
info "Waiting for ArgoCD to be ready..."
kubectl rollout status deployment argocd-server -n argocd --timeout=120s
kubectl rollout status deployment argocd-repo-server -n argocd --timeout=120s
kubectl rollout status deployment argocd-applicationset-controller -n argocd --timeout=120s

# --- Step 9: Configure ArgoCD for local access ---
info "Configuring ArgoCD for local access..."
kubectl apply -f "$SCRIPT_DIR/argocd/argocd-cmd-params-cm.yaml"
kubectl rollout restart deployment argocd-server -n argocd
kubectl rollout status deployment argocd-server -n argocd --timeout=120s
kubectl apply -f "$SCRIPT_DIR/argocd/argocd-server-nodeport.yaml"

# --- Step 10: Build application container images ---
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

# --- Step 11: Load images into kind ---
info "Loading images into kind cluster..."
kind load docker-image apiservice:dev --name "$CLUSTER_NAME"
kind load docker-image gateway:dev --name "$CLUSTER_NAME"
kind load docker-image web:dev --name "$CLUSTER_NAME"

# --- Step 12: Apply ArgoCD root app-of-apps ---
info "Applying ArgoCD root app-of-apps..."
kubectl apply -f "$SCRIPT_DIR/argocd/root-app.yaml"

# --- Step 13: Wait for ArgoCD to sync all applications ---
info "Waiting for ArgoCD to sync all applications..."
sleep 10

info "Waiting for PostgreSQL..."
kubectl wait --for=condition=ready pod -l app=postgres -n "$NAMESPACE" --timeout=120s

info "Waiting for RabbitMQ..."
kubectl wait --for=condition=ready pod -l app=rabbitmq -n "$NAMESPACE" --timeout=120s

info "Waiting for Keycloak (may take up to 3 minutes for realm import)..."
kubectl wait --for=condition=ready pod -l app=keycloak -n "$NAMESPACE" --timeout=180s

info "Waiting for ApiService (may take up to 3 minutes for first-boot migrations)..."
kubectl wait --for=condition=ready pod -l app=apiservice -n "$NAMESPACE" --timeout=180s

info "Waiting for Gateway..."
kubectl wait --for=condition=ready pod -l app=gateway -n "$NAMESPACE" --timeout=120s

info "Waiting for Web..."
kubectl wait --for=condition=ready pod -l app=web -n "$NAMESPACE" --timeout=120s

info "Waiting for OTEL Collector..."
kubectl wait --for=condition=ready pod -l app=otel-collector -n "$NAMESPACE" --timeout=120s

info "Waiting for Aspire Dashboard..."
kubectl wait --for=condition=ready pod -l app=aspire-dashboard -n "$NAMESPACE" --timeout=120s

# --- Step 14: Print access info ---
ARGOCD_PASSWORD=$(kubectl -n argocd get secret argocd-initial-admin-secret \
  -o jsonpath='{.data.password}' | base64 --decode)

echo ""
info "Full stack ready!"
echo ""
echo "Access points:"
echo "  PostgreSQL:          localhost:35432 (user: postgres, pass: postgres, db: appdb)"
echo "  RabbitMQ Management: http://localhost:35672 (user: guest, pass: guest)"
echo "  Keycloak Admin:      http://localhost:38080 (user: admin, pass: admin)"
echo "  Gateway (API):       http://localhost:38800"
echo "  ArgoCD UI:           http://localhost:38443 (user: admin, pass: ${ARGOCD_PASSWORD})"
echo "  Aspire Dashboard:    http://localhost:38888"
echo ""
echo "Storefront: http://localhost:38800 (via Gateway)"
echo ""
echo "Useful commands:"
echo "  kubectl get pods -n ${NAMESPACE}"
echo "  kubectl get applications -n argocd"
echo "  kubectl logs -f deployment/apiservice -n ${NAMESPACE}"
echo "  kubectl logs -f deployment/gateway -n ${NAMESPACE}"
echo "  kubectl logs -f deployment/web -n ${NAMESPACE}"
echo "  kind delete cluster --name ${CLUSTER_NAME}"

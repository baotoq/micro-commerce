#!/bin/bash
# =============================================================================
# ArgoCD Cluster Bootstrap Script
# =============================================================================
#
# This script automates the complete cluster bootstrap process including:
# - Cluster registration with ArgoCD
# - Cluster secret generation
# - ArgoCD project creation
# - Values directory scaffolding
#
# Usage:
#   ./bootstrap-script.sh --name <cluster-name> --environment <env> [options]
#
# Options:
#   --name          Cluster name (required)
#   --environment   Environment: dev|hlg|prd (required)
#   --region        Azure region (default: brazilsouth)
#   --node-type     Node type: spot|standard|mixed (default: standard)
#   --infra-repo    Path to infra-team repo (default: ./infra-team)
#   --values-repo   Path to values repo (default: ./argo-cd-helm-values)
#   --dry-run       Generate files without committing
#   --help          Show this help message
#
# Example:
#   ./bootstrap-script.sh \
#     --name cafehyna-dev-02 \
#     --environment dev \
#     --region brazilsouth \
#     --node-type spot
#
# =============================================================================

set -euo pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Default values
REGION="brazilsouth"
NODE_TYPE="standard"
INFRA_REPO="./infra-team"
VALUES_REPO="./argo-cd-helm-values"
DRY_RUN="false"
CLUSTER_NAME=""
ENVIRONMENT=""

# Components to scaffold
COMPONENTS=(
    "cert-manager"
    "external-secrets"
    "ingress-nginx"
    "prometheus-stack"
    "loki"
    "external-dns"
)

# -----------------------------------------------------------------------------
# Helper Functions
# -----------------------------------------------------------------------------

log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

show_help() {
    head -40 "$0" | tail -35
    exit 0
}

# -----------------------------------------------------------------------------
# Argument Parsing
# -----------------------------------------------------------------------------

while [[ $# -gt 0 ]]; do
    case $1 in
        --name)
            CLUSTER_NAME="$2"
            shift 2
            ;;
        --environment)
            ENVIRONMENT="$2"
            shift 2
            ;;
        --region)
            REGION="$2"
            shift 2
            ;;
        --node-type)
            NODE_TYPE="$2"
            shift 2
            ;;
        --infra-repo)
            INFRA_REPO="$2"
            shift 2
            ;;
        --values-repo)
            VALUES_REPO="$2"
            shift 2
            ;;
        --dry-run)
            DRY_RUN="true"
            shift
            ;;
        --help|-h)
            show_help
            ;;
        *)
            log_error "Unknown option: $1"
            exit 1
            ;;
    esac
done

# -----------------------------------------------------------------------------
# Validation
# -----------------------------------------------------------------------------

validate_inputs() {
    log_info "Validating inputs..."

    # Required arguments
    if [[ -z "$CLUSTER_NAME" ]]; then
        log_error "--name is required"
        exit 1
    fi

    if [[ -z "$ENVIRONMENT" ]]; then
        log_error "--environment is required"
        exit 1
    fi

    # Validate environment
    if [[ ! "$ENVIRONMENT" =~ ^(dev|hlg|prd|hub)$ ]]; then
        log_error "Invalid environment: $ENVIRONMENT (must be dev|hlg|prd|hub)"
        exit 1
    fi

    # Validate node type
    if [[ ! "$NODE_TYPE" =~ ^(spot|standard|mixed)$ ]]; then
        log_error "Invalid node-type: $NODE_TYPE (must be spot|standard|mixed)"
        exit 1
    fi

    # Validate repositories exist
    if [[ ! -d "$INFRA_REPO" ]]; then
        log_error "Infra repository not found: $INFRA_REPO"
        exit 1
    fi

    if [[ ! -d "$VALUES_REPO" ]]; then
        log_error "Values repository not found: $VALUES_REPO"
        exit 1
    fi

    log_success "Input validation passed"
}

# -----------------------------------------------------------------------------
# Prerequisites Check
# -----------------------------------------------------------------------------

check_prerequisites() {
    log_info "Checking prerequisites..."

    # Check required commands
    local required_commands=("argocd" "kubectl" "yq" "git")
    for cmd in "${required_commands[@]}"; do
        if ! command -v "$cmd" &> /dev/null; then
            log_error "Required command not found: $cmd"
            exit 1
        fi
    done

    # Check ArgoCD login
    if ! argocd cluster list &> /dev/null; then
        log_error "Not logged into ArgoCD. Run: argocd login <server>"
        exit 1
    fi

    log_success "Prerequisites check passed"
}

# -----------------------------------------------------------------------------
# Cluster Registration
# -----------------------------------------------------------------------------

register_cluster() {
    log_info "Registering cluster with ArgoCD..."

    # Check if cluster already exists
    if argocd cluster get "$CLUSTER_NAME" &> /dev/null; then
        log_warning "Cluster already registered: $CLUSTER_NAME"
        return 0
    fi

    # Get kubectl context
    local context="aks-${CLUSTER_NAME}"
    if ! kubectl config get-contexts "$context" &> /dev/null; then
        log_error "kubectl context not found: $context"
        log_info "Run: az aks get-credentials --resource-group <rg> --name $context"
        exit 1
    fi

    if [[ "$DRY_RUN" == "true" ]]; then
        log_info "[DRY-RUN] Would run: argocd cluster add $context --name $CLUSTER_NAME"
    else
        argocd cluster add "$context" --name "$CLUSTER_NAME" --yes
    fi

    log_success "Cluster registered: $CLUSTER_NAME"
}

# -----------------------------------------------------------------------------
# Generate Cluster Secret
# -----------------------------------------------------------------------------

generate_cluster_secret() {
    log_info "Generating cluster secret..."

    local output_file="${INFRA_REPO}/argocd-clusters/${CLUSTER_NAME}.yaml"

    # Get server URL
    local server_url
    if [[ "$DRY_RUN" == "true" ]]; then
        server_url="https://aks-${CLUSTER_NAME}-placeholder.hcp.${REGION}.azmk8s.io:443"
    else
        server_url=$(argocd cluster get "$CLUSTER_NAME" -o json | jq -r '.server')
    fi

    # Determine connection type
    local connection_type="internal"

    cat > "$output_file" << EOF
apiVersion: v1
kind: Secret
metadata:
  name: ${CLUSTER_NAME}
  namespace: argocd
  labels:
    argocd.argoproj.io/secret-type: cluster
    environment: ${ENVIRONMENT}
    region: ${REGION}
    cluster-name: ${CLUSTER_NAME}
    node-type: ${NODE_TYPE}
    connection-type: ${connection_type}
    tier: application
type: Opaque
stringData:
  name: ${CLUSTER_NAME}
  server: ${server_url}
  config: |
    {
      "execProviderConfig": {
        "command": "argocd-k8s-auth",
        "args": ["azure", "--cluster-name", "aks-${CLUSTER_NAME}"],
        "apiVersion": "client.authentication.k8s.io/v1beta1"
      },
      "tlsClientConfig": {
        "insecure": false
      }
    }
EOF

    log_success "Generated: $output_file"
}

# -----------------------------------------------------------------------------
# Generate ArgoCD Project
# -----------------------------------------------------------------------------

generate_argocd_project() {
    log_info "Generating ArgoCD project..."

    local output_file="${INFRA_REPO}/argocd-projects/${CLUSTER_NAME}.yaml"

    # Get server URL
    local server_url
    if [[ "$DRY_RUN" == "true" ]]; then
        server_url="https://aks-${CLUSTER_NAME}-placeholder.hcp.${REGION}.azmk8s.io:443"
    else
        server_url=$(argocd cluster get "$CLUSTER_NAME" -o json | jq -r '.server')
    fi

    cat > "$output_file" << EOF
apiVersion: argoproj.io/v1alpha1
kind: AppProject
metadata:
  name: ${CLUSTER_NAME}
  namespace: argocd
  finalizers:
    - resources-finalizer.argocd.argoproj.io
spec:
  description: "${CLUSTER_NAME} Cluster Project (${ENVIRONMENT})"
  sourceRepos:
    - '*'
  destinations:
    - namespace: '*'
      server: ${server_url}
      name: ${CLUSTER_NAME}
  clusterResourceWhitelist:
    - group: '*'
      kind: '*'
  namespaceResourceWhitelist:
    - group: '*'
      kind: '*'
  orphanedResources:
    warn: true
EOF

    log_success "Generated: $output_file"
}

# -----------------------------------------------------------------------------
# Generate Values Files
# -----------------------------------------------------------------------------

generate_values_files() {
    log_info "Generating values files for components..."

    for component in "${COMPONENTS[@]}"; do
        local values_dir="${VALUES_REPO}/kube-addons/${component}/${CLUSTER_NAME}"
        local values_file="${values_dir}/values.yaml"

        mkdir -p "$values_dir"

        # Generate environment-specific values
        if [[ "$ENVIRONMENT" == "dev" ]]; then
            generate_dev_values "$component" "$values_file"
        elif [[ "$ENVIRONMENT" == "prd" ]]; then
            generate_prd_values "$component" "$values_file"
        else
            generate_default_values "$component" "$values_file"
        fi

        log_success "Generated: $values_file"
    done
}

generate_dev_values() {
    local component=$1
    local output_file=$2

    cat > "$output_file" << EOF
# ${component} values for ${CLUSTER_NAME}
# Environment: ${ENVIRONMENT}
# Region: ${REGION}
# Generated: $(date -u +"%Y-%m-%dT%H:%M:%SZ")

# Development configuration
replicaCount: 1

# Spot instance tolerations
tolerations:
  - key: "kubernetes.azure.com/scalesetpriority"
    operator: "Equal"
    value: "spot"
    effect: "NoSchedule"

nodeSelector:
  kubernetes.azure.com/scalesetpriority: spot

# Reduced resources for dev
resources:
  requests:
    cpu: 100m
    memory: 128Mi
  limits:
    cpu: 500m
    memory: 512Mi
EOF
}

generate_prd_values() {
    local component=$1
    local output_file=$2

    cat > "$output_file" << EOF
# ${component} values for ${CLUSTER_NAME}
# Environment: ${ENVIRONMENT}
# Region: ${REGION}
# Generated: $(date -u +"%Y-%m-%dT%H:%M:%SZ")

# Production configuration
replicaCount: 3

# Standard nodes only
nodeSelector:
  kubernetes.azure.com/mode: system

tolerations: []

# Production resources
resources:
  requests:
    cpu: 500m
    memory: 512Mi
  limits:
    cpu: 2000m
    memory: 2Gi

# High availability
podDisruptionBudget:
  enabled: true
  minAvailable: 2

affinity:
  podAntiAffinity:
    requiredDuringSchedulingIgnoredDuringExecution:
      - labelSelector:
          matchLabels:
            app.kubernetes.io/name: ${component}
        topologyKey: kubernetes.io/hostname
EOF
}

generate_default_values() {
    local component=$1
    local output_file=$2

    cat > "$output_file" << EOF
# ${component} values for ${CLUSTER_NAME}
# Environment: ${ENVIRONMENT}
# Region: ${REGION}
# Generated: $(date -u +"%Y-%m-%dT%H:%M:%SZ")

# Default configuration
replicaCount: 2

resources:
  requests:
    cpu: 250m
    memory: 256Mi
  limits:
    cpu: 1000m
    memory: 1Gi
EOF
}

# -----------------------------------------------------------------------------
# Summary
# -----------------------------------------------------------------------------

show_summary() {
    echo ""
    echo "============================================================================="
    echo "                    BOOTSTRAP COMPLETE: ${CLUSTER_NAME}"
    echo "============================================================================="
    echo ""
    echo "Cluster Details:"
    echo "  Name:        ${CLUSTER_NAME}"
    echo "  Environment: ${ENVIRONMENT}"
    echo "  Region:      ${REGION}"
    echo "  Node Type:   ${NODE_TYPE}"
    echo ""
    echo "Generated Files:"
    echo "  - ${INFRA_REPO}/argocd-clusters/${CLUSTER_NAME}.yaml"
    echo "  - ${INFRA_REPO}/argocd-projects/${CLUSTER_NAME}.yaml"
    for component in "${COMPONENTS[@]}"; do
        echo "  - ${VALUES_REPO}/kube-addons/${component}/${CLUSTER_NAME}/values.yaml"
    done
    echo ""
    echo "Next Steps:"
    echo "  1. Review generated files"
    echo "  2. Run pre-commit validation:"
    echo "     pre-commit run --all-files"
    echo "  3. Commit and push changes:"
    echo "     cd ${INFRA_REPO} && git add . && git commit -m 'feat(cluster): add ${CLUSTER_NAME}'"
    echo "     cd ${VALUES_REPO} && git add . && git commit -m 'feat(values): add ${CLUSTER_NAME}'"
    echo "  4. Monitor ArgoCD for sync status"
    echo ""
    if [[ "$DRY_RUN" == "true" ]]; then
        echo "NOTE: This was a DRY RUN. Cluster was NOT registered with ArgoCD."
        echo ""
    fi
}

# -----------------------------------------------------------------------------
# Main
# -----------------------------------------------------------------------------

main() {
    echo ""
    echo "============================================================================="
    echo "                ArgoCD Cluster Bootstrap Script"
    echo "============================================================================="
    echo ""

    validate_inputs
    check_prerequisites

    if [[ "$DRY_RUN" != "true" ]]; then
        register_cluster
    else
        log_info "[DRY-RUN] Skipping cluster registration"
    fi

    generate_cluster_secret
    generate_argocd_project
    generate_values_files

    show_summary
}

main

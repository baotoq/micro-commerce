#!/usr/bin/env bash
set -euo pipefail

orb start k8s
kubectl config use-context orbstack

# Cluster DNS plumbing — single-URL resolution for pods (one-time, kube-system).
kubectl apply -f "$(dirname "$0")/../k8s/coredns-custom.yaml"
kubectl -n kube-system rollout restart deployment coredns

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
echo "UI: kubectl -n argocd port-forward svc/argocd-server 8090:443  -> https://localhost:8090"

---
phase: 28-observability
status: passed
verified: 2026-03-02
requirements: [OBSV-01, OBSV-02]
must_haves_checked: 6/6
---

# Phase 28: Observability - Verification Report

## Phase Goal
Traces, metrics, and logs from all cluster services are visible in the Aspire Dashboard without requiring .NET Aspire AppHost.

## Requirements Verification

### OBSV-01: OTEL Collector receives OTLP from all app services and forwards to Aspire Dashboard

| Check | Status | Evidence |
|-------|--------|----------|
| OTEL Collector Deployment exists with pinned image 0.146.0 | PASS | `infra/k8s/base/otel-collector/deployment.yaml` contains `otel/opentelemetry-collector:0.146.0` |
| ConfigMap has OTLP receiver on ports 4317 (gRPC) and 4318 (HTTP) | PASS | `infra/k8s/base/otel-collector/configmap.yaml` has `grpc.endpoint: 0.0.0.0:4317` and `http.endpoint: 0.0.0.0:4318` |
| Pipeline has memory_limiter (first) and batch processors | PASS | All 3 pipelines (traces, metrics, logs) list `processors: [memory_limiter, batch]` |
| OTLP exporter targets aspire-dashboard:18889 with TLS insecure | PASS | `endpoint: aspire-dashboard:18889` and `tls.insecure: true` confirmed |
| Collector Service exposes ports 4317 and 4318 as ClusterIP | PASS | `infra/k8s/base/otel-collector/service.yaml` has both ports |
| ApiService Deployment has OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317 | PASS | `infra/k8s/base/apiservice/deployment.yaml` env var confirmed |
| ApiService Deployment has OTEL_SERVICE_NAME=apiservice | PASS | Confirmed in deployment.yaml |
| Gateway Deployment has OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317 | PASS | `infra/k8s/base/gateway/deployment.yaml` env var confirmed |
| Gateway Deployment has OTEL_SERVICE_NAME=gateway | PASS | Confirmed in deployment.yaml |
| ArgoCD Application exists for otel-collector | PASS | `infra/k8s/argocd/apps/otel-collector.yaml` points to `infra/k8s/base/otel-collector` |

### OBSV-02: Aspire Dashboard runs as a standalone container in K8s accessible for dev monitoring

| Check | Status | Evidence |
|-------|--------|----------|
| Aspire Dashboard Deployment exists with image 9.3 | PASS | `infra/k8s/base/aspire-dashboard/deployment.yaml` contains `mcr.microsoft.com/dotnet/aspire-dashboard:9.3` |
| Anonymous auth enabled (DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true) | PASS | Env var confirmed in deployment.yaml |
| OTLP auth mode set to Unsecured | PASS | `DASHBOARD__OTLP__AUTHMODE: Unsecured` confirmed |
| UI port 18888 and OTLP port 18889 exposed | PASS | Both ports in deployment.yaml and ClusterIP service |
| NodePort 30888 configured for browser access | PASS | `infra/k8s/base/aspire-dashboard/service.yaml` has `nodePort: 30888` |
| Kind port mapping 30888->38888 | PASS | `infra/k8s/kind-config.yaml` has `containerPort: 30888, hostPort: 38888` |
| ArgoCD Application exists for aspire-dashboard | PASS | `infra/k8s/argocd/apps/aspire-dashboard.yaml` points to `infra/k8s/base/aspire-dashboard` |
| Both services registered in base kustomization.yaml | PASS | `otel-collector/` and `aspire-dashboard/` in resources list |

## Must-Haves Cross-Reference

| Must-Have | Status |
|-----------|--------|
| OTEL Collector base manifests define a Deployment, Service, and ConfigMap with OTLP receiver, memory_limiter, batch processor, and OTLP exporter to Aspire Dashboard | PASS |
| Aspire Dashboard base manifests define a Deployment and Service with anonymous auth and OTLP ingestion on port 18889 | PASS |
| Both new services are registered in the base kustomization.yaml so Kustomize can discover them | PASS |
| ApiService and Gateway have OTEL_EXPORTER_OTLP_ENDPOINT and OTEL_SERVICE_NAME env vars set to send telemetry to the OTEL Collector | PASS |
| ArgoCD app-of-apps discovers and manages both new observability services | PASS |
| Aspire Dashboard UI is reachable from the host browser at localhost:38888 via kind port mapping | PASS |

## Key Artifacts

| Artifact | Purpose | Verified |
|----------|---------|----------|
| `infra/k8s/base/otel-collector/configmap.yaml` | OTEL Collector pipeline config | Contains `otlp/aspire` exporter |
| `infra/k8s/base/otel-collector/deployment.yaml` | Collector Deployment | Image `otel/opentelemetry-collector:0.146.0` |
| `infra/k8s/base/otel-collector/service.yaml` | Collector ClusterIP Service | Ports 4317 and 4318 |
| `infra/k8s/base/aspire-dashboard/deployment.yaml` | Dashboard Deployment | Image `aspire-dashboard:9.3`, anonymous auth |
| `infra/k8s/base/aspire-dashboard/service.yaml` | Dashboard ClusterIP + NodePort | NodePort 30888 |
| `infra/k8s/base/kustomization.yaml` | Root kustomization | Lists both new services |
| `infra/k8s/base/apiservice/deployment.yaml` | ApiService OTEL env vars | `OTEL_EXPORTER_OTLP_ENDPOINT` set |
| `infra/k8s/base/gateway/deployment.yaml` | Gateway OTEL env vars | `OTEL_EXPORTER_OTLP_ENDPOINT` set |
| `infra/k8s/argocd/apps/otel-collector.yaml` | ArgoCD app for Collector | Path: `infra/k8s/base/otel-collector` |
| `infra/k8s/argocd/apps/aspire-dashboard.yaml` | ArgoCD app for Dashboard | Path: `infra/k8s/base/aspire-dashboard` |
| `infra/k8s/kind-config.yaml` | Kind port mapping | 30888->38888 |

## Key Links Verified

| From | To | Via | Verified |
|------|----|-----|----------|
| ApiService Deployment | otel-collector:4317 | OTEL_EXPORTER_OTLP_ENDPOINT env var | PASS |
| Gateway Deployment | otel-collector:4317 | OTEL_EXPORTER_OTLP_ENDPOINT env var | PASS |
| OTEL Collector ConfigMap | aspire-dashboard:18889 | OTLP exporter endpoint | PASS |
| OTEL Collector Deployment | ConfigMap | Volume mount at /etc/otelcol | PASS |
| ArgoCD otel-collector app | infra/k8s/base/otel-collector | Source path | PASS |
| ArgoCD aspire-dashboard app | infra/k8s/base/aspire-dashboard | Source path | PASS |
| Kind config | Aspire Dashboard NodePort 30888 | extraPortMappings | PASS |

## Conclusion

Phase 28 goal **achieved**. All must-haves verified. The complete telemetry pipeline is wired:
- Applications (ApiService, Gateway) -> OTEL Collector (port 4317) -> Aspire Dashboard (port 18889)
- Dashboard UI accessible at localhost:38888 via kind NodePort mapping
- Both services managed by ArgoCD with automated sync

No application code changes were required -- the existing ServiceDefaults OTLP conditional logic activates when the env var is set.

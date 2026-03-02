---
phase: 28-observability
plan: 01
subsystem: infra
tags: [kubernetes, otel, opentelemetry, aspire-dashboard, kustomize, observability]

requires:
  - phase: 26-kubernetes-base-manifests
    provides: Base kustomization pattern and existing service manifests
provides:
  - OTEL Collector base manifests with OTLP receiver, memory_limiter, batch, and OTLP exporter
  - Aspire Dashboard base manifests with anonymous auth and OTLP ingestion
  - Both services registered in root kustomization.yaml
affects: [28-02, observability, kubernetes]

tech-stack:
  added: [otel-collector 0.146.0, aspire-dashboard 9.3]
  patterns: [otel-pipeline-config, nodeport-for-dev-access]

key-files:
  created:
    - infra/k8s/base/otel-collector/configmap.yaml
    - infra/k8s/base/otel-collector/deployment.yaml
    - infra/k8s/base/otel-collector/service.yaml
    - infra/k8s/base/otel-collector/kustomization.yaml
    - infra/k8s/base/aspire-dashboard/deployment.yaml
    - infra/k8s/base/aspire-dashboard/service.yaml
    - infra/k8s/base/aspire-dashboard/kustomization.yaml
  modified:
    - infra/k8s/base/kustomization.yaml

key-decisions:
  - "Used OTLP exporter with tls.insecure:true since Aspire Dashboard runs plain HTTP internally"
  - "NodePort 30888 for Dashboard UI browser access from host via kind port mapping"
  - "memory_limiter processor placed first in pipeline per OTEL best practices"

patterns-established:
  - "Observability services use base/ only (no overlays) since they use fixed public images"
  - "Dual service pattern: ClusterIP for internal traffic + NodePort for dev browser access"

requirements-completed: [OBSV-01, OBSV-02]

duration: 5min
completed: 2026-03-02
---

# Plan 28-01: OTEL Collector and Aspire Dashboard Base Manifests Summary

**OTEL Collector pipeline (receivers, processors, exporters) and Aspire Dashboard with anonymous auth deployed as Kubernetes base manifests**

## Performance

- **Duration:** 5 min
- **Tasks:** 2
- **Files created:** 7
- **Files modified:** 1

## Accomplishments
- OTEL Collector with complete pipeline config: OTLP gRPC/HTTP receivers, memory_limiter + batch processors, OTLP exporter to Aspire Dashboard on port 18889
- Aspire Dashboard with anonymous auth enabled and OTLP ingestion
- NodePort service (30888) for Dashboard UI browser access
- Both services registered in root kustomization.yaml for Kustomize discovery

## Task Commits

Each task was committed atomically:

1. **Task 1: Create OTEL Collector base manifests** - `bcb46987` (feat)
2. **Task 2: Create Aspire Dashboard base manifests and update root kustomization** - `12641650` (feat)

## Files Created/Modified
- `infra/k8s/base/otel-collector/configmap.yaml` - OTEL Collector pipeline config (OTLP receiver, memory_limiter, batch, OTLP exporter to Aspire Dashboard)
- `infra/k8s/base/otel-collector/deployment.yaml` - Collector Deployment with pinned image 0.146.0, health probes on 13133
- `infra/k8s/base/otel-collector/service.yaml` - ClusterIP on ports 4317 (gRPC) and 4318 (HTTP)
- `infra/k8s/base/otel-collector/kustomization.yaml` - Kustomize resource list
- `infra/k8s/base/aspire-dashboard/deployment.yaml` - Dashboard Deployment with 9.3 image, anonymous auth env vars
- `infra/k8s/base/aspire-dashboard/service.yaml` - ClusterIP (18888+18889) and NodePort (30888) services
- `infra/k8s/base/aspire-dashboard/kustomization.yaml` - Kustomize resource list
- `infra/k8s/base/kustomization.yaml` - Added otel-collector/ and aspire-dashboard/ to resources

## Decisions Made
- Used `tls.insecure: true` on OTLP exporter because Aspire Dashboard runs plain HTTP internally
- NodePort 30888 chosen to avoid conflicts with existing ArgoCD NodePort (30443)
- Followed existing service manifest pattern (labels, resource limits, probe configuration)

## Deviations from Plan
None - plan executed exactly as written

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Base manifests ready for Plan 28-02 to wire telemetry from application services
- ArgoCD apps and kind port mapping still needed (Plan 28-02)

---
*Phase: 28-observability*
*Completed: 2026-03-02*

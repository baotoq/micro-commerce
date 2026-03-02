---
phase: 28-observability
plan: 02
subsystem: infra
tags: [kubernetes, otel, opentelemetry, aspire-dashboard, argocd, kind, observability]

requires:
  - phase: 28-observability
    provides: OTEL Collector and Aspire Dashboard base manifests (Plan 01)
  - phase: 26-kubernetes-base-manifests
    provides: ApiService and Gateway deployment manifests, ArgoCD app-of-apps pattern
provides:
  - End-to-end telemetry pipeline from ApiService/Gateway through OTEL Collector to Aspire Dashboard
  - ArgoCD management for both observability services
  - Kind port mapping for Dashboard browser access at localhost:38888
affects: [observability, kubernetes, argocd]

tech-stack:
  added: []
  patterns: [otel-env-var-injection, argocd-base-path-apps]

key-files:
  created:
    - infra/k8s/argocd/apps/otel-collector.yaml
    - infra/k8s/argocd/apps/aspire-dashboard.yaml
  modified:
    - infra/k8s/base/apiservice/deployment.yaml
    - infra/k8s/base/gateway/deployment.yaml
    - infra/k8s/kind-config.yaml

key-decisions:
  - "ArgoCD apps point to base/ paths (not overlays) since observability services use fixed public images"
  - "OTEL_SERVICE_NAME set to lowercase names matching Aspire AppHost convention"

patterns-established:
  - "Infrastructure services use base/ ArgoCD paths; application services use overlay/ paths"
  - "OTEL env var injection pattern for any .NET service with ServiceDefaults"

requirements-completed: [OBSV-01, OBSV-02]

duration: 4min
completed: 2026-03-02
---

# Plan 28-02: Wire Telemetry, ArgoCD Apps, Kind Port Mapping Summary

**End-to-end observability pipeline wired: ApiService/Gateway -> OTEL Collector -> Aspire Dashboard, managed by ArgoCD, accessible at localhost:38888**

## Performance

- **Duration:** 4 min
- **Tasks:** 2
- **Files created:** 2
- **Files modified:** 3

## Accomplishments
- ApiService and Gateway deployments now emit OTLP telemetry to the OTEL Collector via OTEL_EXPORTER_OTLP_ENDPOINT
- OTEL_SERVICE_NAME set distinctly ("apiservice", "gateway") so traces are identifiable in the Dashboard
- ArgoCD Application manifests created for both observability services with automated sync
- Kind port mapping 30888->38888 enables Aspire Dashboard UI access from host browser

## Task Commits

Each task was committed atomically:

1. **Task 1: Add OTEL env vars to ApiService and Gateway** - `f48c5ae5` (feat)
2. **Task 2: Create ArgoCD Applications and update kind-config** - `9388b71d` (feat)

## Files Created/Modified
- `infra/k8s/base/apiservice/deployment.yaml` - Added OTEL_EXPORTER_OTLP_ENDPOINT and OTEL_SERVICE_NAME env vars
- `infra/k8s/base/gateway/deployment.yaml` - Added OTEL_EXPORTER_OTLP_ENDPOINT and OTEL_SERVICE_NAME env vars
- `infra/k8s/argocd/apps/otel-collector.yaml` - ArgoCD Application for OTEL Collector (base/ path)
- `infra/k8s/argocd/apps/aspire-dashboard.yaml` - ArgoCD Application for Aspire Dashboard (base/ path)
- `infra/k8s/kind-config.yaml` - Added port mapping containerPort 30888 -> hostPort 38888

## Decisions Made
- No application code changes needed -- ServiceDefaults already conditionally enables OTLP when env var is set
- ArgoCD apps use base/ paths since these services use fixed public images (no CI/CD image patching)

## Deviations from Plan
None - plan executed exactly as written

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Full observability stack deployed and wired
- Kind cluster must be recreated for new port mapping to take effect
- All telemetry flows: Apps -> Collector (4317) -> Dashboard (18889) -> UI (18888/38888)

---
*Phase: 28-observability*
*Completed: 2026-03-02*

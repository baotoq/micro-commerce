---
phase: 24-infrastructure-manifests-and-secrets
plan: 02
subsystem: infra
tags: [kubernetes, rabbitmq, keycloak, kustomize, configmap, probes, nodeport]

requires:
  - phase: 23-dockerfiles-and-container-image-pipeline
    provides: Container images for application services
provides:
  - RabbitMQ Kubernetes Deployment with management plugin and health probes
  - RabbitMQ ClusterIP and NodePort services (AMQP 5672, management 15672/30672)
  - Keycloak Kubernetes Deployment with realm import and startup probe
  - Keycloak ClusterIP and NodePort services (HTTP 8080/30080)
  - Keycloak realm ConfigMap via configMapGenerator
  - Self-contained micro-commerce-realm.json in K8s manifests
affects: [24-03-bootstrap-secrets, 25-application-manifests, 26-argocd]

tech-stack:
  added: [rabbitmq:4-management, keycloak:26.0]
  patterns: [configMapGenerator-for-file-mounting, startup-probe-for-slow-boot, nodeport-for-kind-access, secret-ref-credentials]

key-files:
  created:
    - infra/k8s/base/rabbitmq/kustomization.yaml
    - infra/k8s/base/rabbitmq/deployment.yaml
    - infra/k8s/base/rabbitmq/service.yaml
    - infra/k8s/base/keycloak/kustomization.yaml
    - infra/k8s/base/keycloak/deployment.yaml
    - infra/k8s/base/keycloak/service.yaml
    - infra/k8s/base/keycloak/micro-commerce-realm.json
  modified: []

key-decisions:
  - "Keycloak startup probe on management port 9000 with 15s initial delay and 60s probing window protects against slow first-boot realm import"
  - "Realm JSON copied from AppHost/Realms to K8s manifests for self-contained deployment"
  - "configMapGenerator creates hash-suffixed ConfigMap for automatic Deployment rollout on realm changes"
  - "Separate NodePort services for external access instead of mixing NodePort into ClusterIP"

patterns-established:
  - "Infrastructure service pattern: Deployment + ClusterIP (internal) + NodePort (kind access) + sealed-secret ref"
  - "ConfigMap mounting via configMapGenerator for file-based configuration"
  - "Startup probe pattern for slow-booting services (Keycloak)"

requirements-completed: [INFRA-02, INFRA-03, INFRA-05]

duration: 1min
completed: 2026-02-26
---

# Phase 24 Plan 02: RabbitMQ and Keycloak Manifests Summary

**RabbitMQ and Keycloak K8s Deployments with probes, realm ConfigMap import, and NodePort access for kind cluster**

## Performance

- **Duration:** 1 min
- **Started:** 2026-02-26T08:40:28Z
- **Completed:** 2026-02-26T08:41:54Z
- **Tasks:** 2
- **Files created:** 7

## Accomplishments
- RabbitMQ Deployment with management plugin, readiness/liveness probes, and Secret-based credentials
- Keycloak Deployment with dev-mode realm import, startup probe on management port 9000, and KC_BOOTSTRAP_ADMIN env vars
- ClusterIP + NodePort service pairs for both services, aligned with kind port mappings (30672, 30080)
- Self-contained realm JSON copied from AppHost/Realms with configMapGenerator for automatic hash-suffixed ConfigMap

## Task Commits

Each task was committed atomically:

1. **Task 1: Create RabbitMQ Deployment and Service manifests** - `4244a8b6` (feat)
2. **Task 2: Create Keycloak Deployment, Service, and realm ConfigMap manifests** - `32577d30` (feat)

## Files Created/Modified
- `infra/k8s/base/rabbitmq/kustomization.yaml` - Kustomize base referencing deployment, service, sealed-secret
- `infra/k8s/base/rabbitmq/deployment.yaml` - RabbitMQ 4-management with probes and Secret credentials
- `infra/k8s/base/rabbitmq/service.yaml` - ClusterIP (5672+15672) and NodePort (30672) services
- `infra/k8s/base/keycloak/kustomization.yaml` - Kustomize base with configMapGenerator for realm JSON
- `infra/k8s/base/keycloak/deployment.yaml` - Keycloak 26.0 with start-dev, import-realm, startup probe
- `infra/k8s/base/keycloak/service.yaml` - ClusterIP (8080) and NodePort (30080) services
- `infra/k8s/base/keycloak/micro-commerce-realm.json` - Realm config copied from AppHost/Realms

## Decisions Made
- Keycloak startup probe uses management port 9000 (not main 8080) per Keycloak 26.x architecture
- 15s initial delay + 20 retries at 3s intervals = 75s total window for slow first-boot with realm import
- Realm JSON is duplicated (not symlinked) from AppHost/Realms to make K8s manifests self-contained
- configMapGenerator creates hash-suffixed ConfigMap names, ensuring Deployment rollout on realm changes
- Separate NodePort services (not mixed into ClusterIP) for cleaner separation of concerns
- KC_BOOTSTRAP_ADMIN_USERNAME/PASSWORD used instead of deprecated KEYCLOAK_ADMIN vars
- RabbitMQ uses Deployment (not StatefulSet) since dev environment doesn't need message persistence

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- RabbitMQ and Keycloak manifests ready for inclusion in base kustomization.yaml (Plan 01)
- Sealed secrets for rabbitmq-credentials and keycloak-credentials needed from Plan 03 bootstrap script
- NodePorts 30672 and 30080 align with kind-config.yaml port mappings from Plan 01

## Self-Check: PASSED

All 7 created files verified on disk. Both task commits (4244a8b6, 32577d30) found in git log.

---
*Phase: 24-infrastructure-manifests-and-secrets*
*Completed: 2026-02-26*

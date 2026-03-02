---
phase: 25-application-manifests-and-masstransit-transport
plan: 02
subsystem: infra
tags: [kubernetes, kustomize, deployment, service, nodeport, kind, health-probes]

# Dependency graph
requires:
  - phase: 24-infrastructure-manifests-and-secrets
    provides: Kustomize base/overlay scaffold, kind cluster config, infrastructure manifests (PostgreSQL, RabbitMQ, Keycloak), SealedSecret-backed credentials
provides:
  - ApiService K8s Deployment with startup/liveness/readiness probes, DB/RabbitMQ/Keycloak env vars, and resource limits
  - Gateway K8s Deployment with YARP cluster override, CORS origin, and ClusterIP + NodePort services
  - Web (Next.js) K8s Deployment with auth env vars, Keycloak OIDC, and gateway service discovery
  - Kustomize base extended with all 6 service groups (3 infra + 3 app)
  - Dev overlay with image tag overrides for kind local images
  - Kind cluster config with Gateway port mapping (30800 -> 38800)
affects: [25-03-PLAN, 26-argocd-gitops]

# Tech tracking
tech-stack:
  added: []
  patterns: [k8s-deployment-with-probes, nodeport-kind-chain, env-var-secret-substitution, kustomize-image-override]

key-files:
  created:
    - infra/k8s/base/apiservice/deployment.yaml
    - infra/k8s/base/apiservice/service.yaml
    - infra/k8s/base/apiservice/kustomization.yaml
    - infra/k8s/base/gateway/deployment.yaml
    - infra/k8s/base/gateway/service.yaml
    - infra/k8s/base/gateway/kustomization.yaml
    - infra/k8s/base/web/deployment.yaml
    - infra/k8s/base/web/service.yaml
    - infra/k8s/base/web/kustomization.yaml
  modified:
    - infra/k8s/base/kustomization.yaml
    - infra/k8s/overlays/dev/kustomization.yaml
    - infra/k8s/kind-config.yaml

key-decisions:
  - "Env var substitution pattern for credentials: define secret env vars BEFORE connection strings so $(VAR) substitution works"
  - "Gateway dual Service pattern: ClusterIP for internal pod-to-pod, NodePort 30800 for kind host access via port 38800"
  - "Web dev placeholder secrets: AUTH_SECRET and KEYCLOAK_CLIENT_SECRET use plaintext dev values (matching Phase 23 Dockerfile approach); production would use SealedSecrets"
  - "CORS origin set to http://localhost:38800 (Gateway NodePort host URL) for kind dev access"
  - "ApiService startup probe: 10s initial delay + 30x5s = 160s window for EF Core migrations on first boot"

patterns-established:
  - "App service manifest pattern: Deployment + ClusterIP Service + kustomization.yaml per service"
  - "Env var secret substitution: define secretKeyRef env vars first, then reference via $(VAR_NAME) in connection strings"
  - "NodePort chain for kind: host port -> kind container port -> K8s NodePort -> pod port"
  - "Kustomize image override: dev overlay uses images section with newName/newTag for local kind images"

requirements-completed: [MFST-01, MFST-03, MFST-04, MFST-05, MFST-06]

# Metrics
duration: 2min
completed: 2026-02-26
---

# Phase 25 Plan 02: Application K8s Manifests Summary

**K8s Deployments and Services for ApiService, Gateway (NodePort), and Web with health probes, secret-backed credentials, and Kustomize image overrides for kind**

## Performance

- **Duration:** 2 min
- **Started:** 2026-02-26T09:51:20Z
- **Completed:** 2026-02-26T09:53:15Z
- **Tasks:** 2
- **Files created:** 9
- **Files modified:** 3

## Accomplishments
- ApiService Deployment with startup probe (160s window for EF migrations), liveness/readiness probes, PostgreSQL/RabbitMQ/Keycloak env vars via secretKeyRef substitution, and 256Mi/512Mi resource limits
- Gateway Deployment with YARP cluster override pointing to `http://apiservice:8080`, CORS origin for kind host access, dual Service (ClusterIP + NodePort 30800), and 128Mi/256Mi resource limits
- Web Deployment with Next.js auth env vars (AUTH_SECRET, Keycloak OIDC, gateway service discovery), liveness/readiness probes on `/`, and 128Mi/256Mi resource limits
- Base kustomization extended to 7 resources (namespace + 3 infra + 3 app services)
- Dev overlay with `images:` section for apiservice/gateway/web dev tags
- Kind cluster config extended with Gateway port mapping (30800 -> 38800)

## Task Commits

Each task was committed atomically:

1. **Task 1: Create ApiService and Gateway K8s manifests** - `08f800c7` (feat)
2. **Task 2: Create Web manifests, update Kustomize base/overlay, and add kind Gateway port mapping** - `4a35826d` (feat)

## Files Created/Modified
- `infra/k8s/base/apiservice/deployment.yaml` - ApiService Deployment with health probes, env vars for DB/RabbitMQ/Keycloak, startup probe, resource limits
- `infra/k8s/base/apiservice/service.yaml` - ClusterIP Service on port 8080 (internal only, Gateway reaches ApiService)
- `infra/k8s/base/apiservice/kustomization.yaml` - Kustomize resources for apiservice
- `infra/k8s/base/gateway/deployment.yaml` - Gateway Deployment with YARP override, CORS origin, Keycloak connection, resource limits
- `infra/k8s/base/gateway/service.yaml` - ClusterIP Service + NodePort Service (30800) for kind host access
- `infra/k8s/base/gateway/kustomization.yaml` - Kustomize resources for gateway
- `infra/k8s/base/web/deployment.yaml` - Web Deployment with Next.js auth env vars, Keycloak OIDC, gateway service discovery
- `infra/k8s/base/web/service.yaml` - ClusterIP Service on port 3000
- `infra/k8s/base/web/kustomization.yaml` - Kustomize resources for web
- `infra/k8s/base/kustomization.yaml` - Extended with apiservice/, gateway/, web/ resources
- `infra/k8s/overlays/dev/kustomization.yaml` - Added images section with dev tags for all 3 app services
- `infra/k8s/kind-config.yaml` - Added Gateway port mapping (30800 -> 38800)

## Decisions Made
- Env var substitution pattern: `POSTGRES_USERNAME`/`POSTGRES_PASSWORD` defined BEFORE `ConnectionStrings__appdb` so K8s `$(...)` substitution works at container startup
- Gateway dual Service: ClusterIP for internal pod-to-pod communication, separate NodePort (30800) for kind host access at port 38800
- Web uses plaintext dev secrets (`AUTH_SECRET`, `KEYCLOAK_CLIENT_SECRET`) matching Phase 23 Dockerfile approach; production deployments would use SealedSecrets
- CORS origin `http://localhost:38800` matches the Gateway NodePort host URL for local kind development
- ApiService startup probe with 160s total window (10s initial + 30 failures x 5s) accommodates EF Core migrations on first boot
- No startup probe for Gateway (fast startup, no DB migrations)
- Web health probes on `/` (Next.js homepage) rather than a custom health endpoint

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- `kubectl kustomize infra/k8s/overlays/dev` fails due to pre-existing missing `sealed-secret.yaml` files in postgres/, rabbitmq/, and keycloak/ directories (Phase 24 design: generated at bootstrap time by `infra/k8s/bootstrap.sh`, not committed to Git). Individual app service kustomizations (`apiservice/`, `gateway/`, `web/`) all validate successfully.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All 9 application manifest files in place for Plan 03 (MassTransit transport abstraction)
- Kind cluster needs recreation (`kind delete cluster --name micro-commerce` + re-bootstrap) to pick up the new Gateway port mapping
- Full `kubectl kustomize` validation requires running the bootstrap script first to generate sealed-secret.yaml files

## Self-Check: PASSED

All 12 files (9 created, 3 modified) verified on disk. Both task commits (08f800c7, 4a35826d) found in git log.

---
*Phase: 25-application-manifests-and-masstransit-transport*
*Completed: 2026-02-26*

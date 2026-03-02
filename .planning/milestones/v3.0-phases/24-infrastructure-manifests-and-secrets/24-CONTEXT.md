# Phase 24: Infrastructure Manifests and Secrets - Context

**Gathered:** 2026-02-26
**Status:** Ready for planning

<domain>
## Phase Boundary

PostgreSQL, RabbitMQ, and Keycloak run stably in a kind cluster with all credentials sealed via SealedSecrets. Includes kind cluster configuration, infrastructure StatefulSets/Deployments, ConfigMaps, SealedSecrets, startup probes, and a bootstrap script. Application manifests (ApiService, Gateway, Web) are Phase 25. ArgoCD and GitOps are Phase 26.

</domain>

<decisions>
## Implementation Decisions

### Manifest directory layout
- All K8s manifests live under `infra/k8s/`
- Kustomize structure: `base/` for environment-neutral resources, `overlays/` for environment patches
- Inside `base/`, subdirectories per service: `base/postgres/`, `base/rabbitmq/`, `base/keycloak/`
- Kind cluster config at `infra/k8s/kind-config.yaml`

### Secrets workflow
- Full bootstrap script that creates kind cluster + installs SealedSecrets controller + seals dev secrets in one command
- SealedSecret YAML files live alongside their service manifests (e.g., `base/postgres/sealed-secret.yaml`)
- Dev credential values use well-known defaults: postgres/postgres, guest/guest for RabbitMQ, admin/admin for Keycloak — easy to debug locally

### Kind cluster setup
- Single node (control-plane only, no separate worker)
- Fixed cluster name: `micro-commerce`
- Standard offset port mappings to avoid conflicts with Aspire local dev: PostgreSQL 35432, RabbitMQ management 35672, Keycloak 38080
- Use kind's default rancher local-path-provisioner for PVC storage — zero extra config

### Keycloak realm import
- ConfigMap created from realm JSON, volume-mounted into Keycloak container with `--import-realm` flag
- Realm JSON (`micro-commerce-realm.json`) copied from `src/MicroCommerce.AppHost/Realms/` into `infra/k8s/base/keycloak/` — K8s manifests are self-contained
- Keycloak runs in dev mode (`start-dev`) — no TLS, fast startup, appropriate for local kind cluster

### Startup probes
- Moderate settings: 15s initial delay, 60s failure threshold timeout
- Applies to both Keycloak and ApiService (EF Core migrations can be slow on first boot)

### Claude's Discretion
- Exact bootstrap script structure and error handling
- RabbitMQ Deployment resource limits
- PostgreSQL StatefulSet replica count (1 for dev)
- Namespace creation approach (in bootstrap script vs manifest)
- SealedSecrets controller version selection

</decisions>

<specifics>
## Specific Ideas

- Bootstrap script should be a single entry point: `./infra/k8s/bootstrap.sh` that goes from zero to running cluster
- Port offset pattern (3xxxx) keeps kind services clearly distinct from Aspire-managed local services
- Existing realm at `src/MicroCommerce.AppHost/Realms/micro-commerce-realm.json` is the source of truth — copy it into K8s manifests directory

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 24-infrastructure-manifests-and-secrets*
*Context gathered: 2026-02-26*

---
phase: 24-infrastructure-manifests-and-secrets
plan: 03
subsystem: infra
tags: [kubernetes, kind, sealed-secrets, kubeseal, bootstrap, shell-script]

# Dependency graph
requires:
  - phase: 24-infrastructure-manifests-and-secrets
    plan: 01
    provides: Kind cluster config, Kustomize scaffold, PostgreSQL manifests
  - phase: 24-infrastructure-manifests-and-secrets
    plan: 02
    provides: RabbitMQ and Keycloak manifests
provides:
  - One-command bootstrap script for full kind cluster setup
  - SealedSecrets controller installation and secret sealing workflow
  - Three sealed-secret.yaml files (postgres, rabbitmq, keycloak) generated at runtime
  - Pod readiness verification for all infrastructure services
affects: [25-application-k8s-manifests, 26-argocd-gitops]

# Tech tracking
tech-stack:
  added: [sealed-secrets-v0.27.3, kubeseal]
  patterns: [sealed-secrets-for-gitops-credentials, idempotent-bootstrap-script, seal-then-apply-workflow]

key-files:
  created:
    - infra/k8s/bootstrap.sh
  modified: []

key-decisions:
  - "SealedSecrets v0.27.3 pinned for reproducible controller installation"
  - "seal_secret helper function centralizes kubeseal invocation for 3 secrets"
  - "Dev defaults: postgres/postgres, guest/guest, admin/admin for local-only kind cluster"
  - "Keycloak gets 180s pod wait timeout vs 120s for others due to realm import startup time"
  - "sealed-secret.yaml files generated dynamically by bootstrap script, not pre-committed"

patterns-established:
  - "Bootstrap pattern: idempotent cluster creation -> controller install -> seal secrets -> apply manifests -> wait for ready"
  - "SealedSecrets workflow: kubectl create secret --dry-run=client | kubeseal --format yaml > sealed-secret.yaml"
  - "No plaintext secrets in Git: only SealedSecret YAML files committed after first bootstrap run"

requirements-completed: [GOPS-03, INFRA-04, INFRA-05]

# Metrics
duration: 1min
completed: 2026-02-26
---

# Phase 24 Plan 03: Bootstrap Script and Sealed Secrets Summary

**Idempotent bootstrap.sh that creates kind cluster, installs SealedSecrets v0.27.3, seals 3 dev credential sets via kubeseal, applies Kustomize manifests, and waits for all infrastructure pods**

## Performance

- **Duration:** 1 min
- **Started:** 2026-02-26T08:44:42Z
- **Completed:** 2026-02-26T08:46:07Z
- **Tasks:** 2
- **Files created:** 1

## Accomplishments
- Executable bootstrap.sh that goes from zero to running infrastructure in one command
- SealedSecrets integration: controller install, readiness wait, kubeseal sealing of postgres/rabbitmq/keycloak credentials
- Comprehensive validation confirming no plaintext secrets, aligned port mappings, correct startup probes, and 15 total infrastructure files
- Access info output showing PostgreSQL (35432), RabbitMQ (35672), and Keycloak (38080) endpoints

## Task Commits

Each task was committed atomically:

1. **Task 1: Create the bootstrap script with SealedSecrets integration** - `95493951` (feat)
2. **Task 2: Validate no plaintext secrets and verify manifest consistency** - validation only, no file changes

## Files Created/Modified
- `infra/k8s/bootstrap.sh` - One-command cluster bootstrap: kind create -> SealedSecrets install -> seal 3 secrets -> kubectl apply -k -> wait for pods -> print access info

## Decisions Made
- SealedSecrets v0.27.3 pinned (per research recommendation) for reproducible installs
- `set -euo pipefail` for strict error handling -- any failure stops the script immediately
- `kubectl rollout status` gates secret sealing -- controller must be ready before kubeseal can fetch the public key
- Namespace created idempotently via `--dry-run=client -o yaml | kubectl apply -f -`
- `seal_secret` helper function avoids code duplication for 3 nearly-identical sealing operations
- Keycloak pod wait timeout at 180s (vs 120s for PostgreSQL/RabbitMQ) to accommodate slow first-boot realm import
- sealed-secret.yaml files are generated dynamically at bootstrap time, not pre-committed -- they change per SealedSecrets key pair

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required. Prerequisites are kind, kubectl, and kubeseal CLI tools.

## Next Phase Readiness
- Full infrastructure bootstrap complete: kind cluster + PostgreSQL + RabbitMQ + Keycloak all ready via single command
- Phase 24 fully complete: all 3 plans executed (kind config + Kustomize scaffold, service manifests, bootstrap + sealed secrets)
- Ready for Phase 25 (application K8s manifests) to add ApiService, Gateway, and Web deployments referencing these infrastructure services
- sealed-secret.yaml files should be committed to Git after first bootstrap run (they are encrypted and safe to commit)

## Self-Check: PASSED

All 1 created file verified on disk (bootstrap.sh exists and is executable). Task commit (95493951) found in git log.

---
*Phase: 24-infrastructure-manifests-and-secrets*
*Completed: 2026-02-26*

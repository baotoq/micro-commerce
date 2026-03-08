---
phase: 33-k8s-security-hardening
verified: 2026-03-08T16:00:00Z
status: passed
score: 4/4 must-haves verified
gaps: []
---

# Phase 33: K8s Security Hardening Verification Report

**Phase Goal:** All workloads run with least-privilege security posture
**Verified:** 2026-03-08T16:00:00Z
**Status:** passed
**Re-verification:** No -- initial verification

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | Web frontend secrets (AUTH_SECRET, KEYCLOAK_CLIENT_SECRET) are SealedSecrets, not plain Secrets | VERIFIED | `infra/k8s/base/web/deployment.yaml` uses `secretKeyRef` to `web-secrets` for both values; no plain-text secret values remain; `bootstrap.sh` contains `seal_secret "web-secrets"` |
| 2 | All 8 workloads have securityContext: runAsNonRoot, readOnlyRootFilesystem, drop ALL capabilities | VERIFIED | All 8 deployment/statefulset files contain container-level `securityContext` with `allowPrivilegeEscalation: false` and `capabilities.drop: [ALL]`. Standard workloads (apiservice, gateway, web, otel-collector, aspire-dashboard) have `runAsNonRoot: true` and `readOnlyRootFilesystem: true`. Exceptions applied correctly: postgres (`runAsNonRoot: false`, `readOnlyRootFilesystem: false`), keycloak and rabbitmq (`readOnlyRootFilesystem: false`) |
| 3 | Keycloak base manifest runs `start` (production mode); dev overlay patches to `start-dev` | VERIFIED | Base `deployment.yaml` has `args: ["start", "--import-realm"]`; overlay patch `keycloak-start-dev-patch.yaml` switches to `["start-dev", "--import-realm"]`; dev `kustomization.yaml` references base + patch |
| 4 | Each workload has its own ServiceAccount with automountServiceAccountToken: false | VERIFIED | All 8 `serviceaccount.yaml` files exist with `automountServiceAccountToken: false`; all 8 deployments/statefulsets reference their dedicated ServiceAccount via `serviceAccountName`; all 8 `kustomization.yaml` files include `serviceaccount.yaml` in resources |

**Score:** 4/4 truths verified

### Required Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `infra/k8s/base/apiservice/serviceaccount.yaml` | Dedicated SA | VERIFIED | Exists, automountServiceAccountToken: false, referenced in deployment |
| `infra/k8s/base/gateway/serviceaccount.yaml` | Dedicated SA | VERIFIED | Exists, automountServiceAccountToken: false, referenced in deployment |
| `infra/k8s/base/web/serviceaccount.yaml` | Dedicated SA | VERIFIED | Exists, automountServiceAccountToken: false, referenced in deployment |
| `infra/k8s/base/postgres/serviceaccount.yaml` | Dedicated SA | VERIFIED | Exists, automountServiceAccountToken: false, referenced in statefulset |
| `infra/k8s/base/rabbitmq/serviceaccount.yaml` | Dedicated SA | VERIFIED | Exists, automountServiceAccountToken: false, referenced in deployment |
| `infra/k8s/base/keycloak/serviceaccount.yaml` | Dedicated SA | VERIFIED | Exists, automountServiceAccountToken: false, referenced in deployment |
| `infra/k8s/base/otel-collector/serviceaccount.yaml` | Dedicated SA | VERIFIED | Exists, automountServiceAccountToken: false, referenced in deployment |
| `infra/k8s/base/aspire-dashboard/serviceaccount.yaml` | Dedicated SA | VERIFIED | Exists, automountServiceAccountToken: false, referenced in deployment |
| `infra/k8s/base/web/deployment.yaml` | secretKeyRef for secrets | VERIFIED | AUTH_SECRET and KEYCLOAK_CLIENT_SECRET use secretKeyRef to web-secrets |
| `infra/k8s/base/keycloak/deployment.yaml` | Production start command | VERIFIED | `args: ["start", "--import-realm"]` |
| `infra/k8s/overlays/dev/keycloak/keycloak-start-dev-patch.yaml` | Dev mode patch | VERIFIED | Strategic merge patch with `args: ["start-dev", "--import-realm"]` |
| `infra/k8s/overlays/dev/keycloak/kustomization.yaml` | Overlay kustomization | VERIFIED | References base keycloak + patch file |
| `infra/k8s/bootstrap.sh` | Seals web-secrets | VERIFIED | Contains `seal_secret "web-secrets"` call |

### Key Link Verification

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| All 8 deployments/statefulsets | serviceaccount.yaml | `serviceAccountName` | WIRED | All 8 workloads reference their dedicated SA |
| All 8 kustomization.yaml | serviceaccount.yaml | `resources` list | WIRED | All 8 include serviceaccount.yaml |
| Web deployment | web-secrets Secret | `secretKeyRef` | WIRED | Both AUTH_SECRET and KEYCLOAK_CLIENT_SECRET reference web-secrets |
| Dev overlay kustomization | keycloak patch | `patches` | WIRED | References keycloak-start-dev-patch.yaml |
| Dev root kustomization | keycloak overlay dir | `resources` | WIRED | References `keycloak/` (overlay directory, not base) |
| Kustomize build | All manifests | `kubectl kustomize` | WIRED | `kubectl kustomize infra/k8s/overlays/dev/` succeeds |

### Requirements Coverage

| Requirement | Source Plan | Description | Status | Evidence |
|-------------|------------|-------------|--------|----------|
| SEC-01 | 33-02 | Web frontend secrets moved to SealedSecret | SATISFIED | deployment.yaml uses secretKeyRef; bootstrap.sh seals web-secrets |
| SEC-02 | 33-01 | securityContext on all 8 workloads | SATISFIED | All 8 have runAsNonRoot, readOnlyRootFilesystem (where feasible), drop ALL capabilities |
| SEC-03 | 33-02 | Keycloak production mode with dev overlay | SATISFIED | Base uses `start`; dev overlay patches to `start-dev` |
| SEC-04 | 33-01 | Dedicated ServiceAccounts per workload | SATISFIED | 8 ServiceAccounts with automountServiceAccountToken: false |

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
|------|------|---------|----------|--------|
| None | - | - | - | No anti-patterns detected |

No TODO, FIXME, PLACEHOLDER, or HACK comments found in any modified files.

### Human Verification Required

None required. All verification can be performed programmatically via manifest inspection and kustomize build validation.

### Gaps Summary

No gaps found. All 4 success criteria verified, all 4 requirements satisfied, all artifacts exist and are substantive and wired, kustomize build succeeds. Phase goal "All workloads run with least-privilege security posture" is achieved.

---

_Verified: 2026-03-08T16:00:00Z_
_Verifier: Claude (gsd-verifier)_

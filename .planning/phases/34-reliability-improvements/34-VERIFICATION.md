---
phase: 34-reliability-improvements
verified: 2026-03-08T16:30:00Z
status: passed
score: 7/7 must-haves verified
re_verification: false
---

# Phase 34: Reliability Improvements Verification Report

**Phase Goal:** Services survive restarts, start reliably, and bootstrap is safe to run
**Verified:** 2026-03-08T16:30:00Z
**Status:** passed
**Re-verification:** No -- initial verification

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | Client-side API calls in K8s reach gateway via browser-reachable URL (not cluster-internal) | VERIFIED | `route.ts` uses `getClientApiBaseUrl()` which returns `NEXT_PUBLIC_API_URL`; web deployment sets it to `http://localhost:38800` |
| 2 | RabbitMQ runs as StatefulSet with PVC -- messages survive pod restarts | VERIFIED | `statefulset.yaml` has `kind: StatefulSet`, `volumeClaimTemplates` with 1Gi PVC at `/var/lib/rabbitmq`; old `deployment.yaml` deleted |
| 3 | Gateway, web, rabbitmq, otel-collector, and aspire-dashboard have startup probes | VERIFIED | All 5 workload manifests contain `startupProbe` with service-appropriate timeouts |
| 4 | Bootstrap script exits early with clear error if kind/kubectl/kubeseal CLI tools are missing | VERIFIED | `check_prerequisites()` loops over `kind kubectl kubeseal docker` with `command -v` checks, prints install links, exits 1 |
| 5 | Bootstrap script has trap handler that prints cleanup guidance on failure | VERIFIED | `cleanup_on_error()` function with `trap cleanup_on_error ERR` at line 53 |
| 6 | Bootstrap script refuses to run if kubectl context is not the expected kind cluster | VERIFIED | Context guard at lines 64-71 compares `CURRENT_CONTEXT` to `kind-${CLUSTER_NAME}`, exits 1 on mismatch |
| 7 | MassTransit outbox is registered on all DbContexts that publish domain events | VERIFIED | 5 `AddEntityFrameworkOutbox` calls in Program.cs: Catalog, Ordering, Inventory, Reviews, Profiles -- all with identical config |

**Score:** 7/7 truths verified

### Required Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `infra/k8s/bootstrap.sh` | Pre-flight checks, trap, context guard | VERIFIED | All 3 safety mechanisms present, `bash -n` syntax valid |
| `infra/k8s/base/rabbitmq/statefulset.yaml` | StatefulSet with PVC | VERIFIED | `kind: StatefulSet`, `volumeClaimTemplates` 1Gi, mount at `/var/lib/rabbitmq` |
| `infra/k8s/base/rabbitmq/deployment.yaml` | Deleted (replaced by statefulset) | VERIFIED | File does not exist |
| `infra/k8s/base/rabbitmq/service.yaml` | Headless service for StatefulSet | VERIFIED | `clusterIP: None` on primary service |
| `infra/k8s/base/rabbitmq/kustomization.yaml` | References statefulset.yaml | VERIFIED | `statefulset.yaml` in resources list |
| `src/MicroCommerce.Web/src/app/api/config/route.ts` | Browser-reachable URL via NEXT_PUBLIC_API_URL | VERIFIED | Imports and calls `getClientApiBaseUrl()` |
| `src/MicroCommerce.Web/src/lib/config.ts` | getClientApiBaseUrl() function | VERIFIED | Returns `process.env.NEXT_PUBLIC_API_URL \|\| ""` |
| `infra/k8s/base/web/deployment.yaml` | NEXT_PUBLIC_API_URL env var + startup probe | VERIFIED | Env var set to `http://localhost:38800`, startupProbe on port 3000 |
| `infra/k8s/base/gateway/deployment.yaml` | Startup probe | VERIFIED | HTTP /health on port 8080, 65s max |
| `infra/k8s/base/otel-collector/deployment.yaml` | Startup probe | VERIFIED | HTTP / on port 13133, 33s max |
| `infra/k8s/base/aspire-dashboard/deployment.yaml` | Startup probe | VERIFIED | HTTP / on port 18888, 65s max |
| `src/MicroCommerce.ApiService/Program.cs` | 5 outbox registrations | VERIFIED | Catalog, Ordering, Inventory, Reviews, Profiles -- all with UsePostgres/UseBusOutbox/1s delay/5min dedup |

### Key Link Verification

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| `route.ts` | Browser fetch calls | `getClientApiBaseUrl()` | WIRED | Import from `@/lib/config`, returns env var value |
| `config.ts` | NEXT_PUBLIC_API_URL | `process.env` | WIRED | `getClientApiBaseUrl()` reads env var, web deployment sets it |
| `statefulset.yaml` | `service.yaml` | Headless service | WIRED | `clusterIP: None` on service, `serviceName: rabbitmq` on StatefulSet |
| `bootstrap.sh` | CLI tools | `command -v` checks | WIRED | Loops over kind/kubectl/kubeseal/docker, exits on missing |
| `bootstrap.sh` | kubectl context | Context name comparison | WIRED | Compares to `kind-${CLUSTER_NAME}` before any operations |
| `Program.cs` | MassTransit outbox | `AddEntityFrameworkOutbox` calls | WIRED | All 5 DbContexts registered with identical config |

### Requirements Coverage

| Requirement | Source Plan | Description | Status | Evidence |
|-------------|------------|-------------|--------|----------|
| REL-01 | 34-02 | Client-side API URL resolution returns browser-reachable URL in K8s | SATISFIED | `getClientApiBaseUrl()` + NEXT_PUBLIC_API_URL env var |
| REL-02 | 34-02 | RabbitMQ converted to StatefulSet with persistent volume | SATISFIED | StatefulSet with 1Gi PVC, headless service, old deployment deleted |
| REL-03 | 34-02 | Startup probes added to gateway, web, rabbitmq, otel-collector, aspire-dashboard | SATISFIED | All 5 manifests have startupProbe with tuned timeouts |
| REL-04 | 34-01 | Bootstrap script checks for required CLI tools before execution | SATISFIED | `check_prerequisites()` with `command -v` loop |
| REL-05 | 34-01 | Bootstrap script adds trap handler for failure cleanup guidance | SATISFIED | `cleanup_on_error()` + `trap cleanup_on_error ERR` |
| REL-06 | 34-01 | Bootstrap script context guard prevents applying to wrong cluster | SATISFIED | Context comparison to `kind-${CLUSTER_NAME}` with exit 1 |
| REL-07 | 34-03 | MassTransit outbox extended to all domain-event-publishing DbContexts | SATISFIED | 5 registrations: Catalog, Ordering, Inventory, Reviews, Profiles |

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
|------|------|---------|----------|--------|
| (none) | - | - | - | No anti-patterns detected |

### Human Verification Required

### 1. K8s RabbitMQ persistence test

**Test:** Deploy to kind cluster, publish messages to RabbitMQ, delete the rabbitmq pod, wait for StatefulSet to recreate it, verify messages are still available.
**Expected:** Messages survive pod restart because PVC retains data at /var/lib/rabbitmq.
**Why human:** Requires running K8s cluster and actual message publish/consume cycle.

### 2. Client-side API calls in K8s

**Test:** Deploy to kind cluster, open the storefront in browser, check Network tab for API calls.
**Expected:** Browser fetch calls go to `http://localhost:38800` (gateway NodePort), not `http://gateway:8080` (cluster-internal).
**Why human:** Requires browser inspection of actual network requests in K8s environment.

### 3. Bootstrap script missing tool behavior

**Test:** Rename `kind` binary temporarily, run `bootstrap.sh`.
**Expected:** Script exits immediately with clear error listing missing tool and install link, no cluster operations attempted.
**Why human:** Requires modifying PATH/tools on the machine to simulate missing prerequisites.

---

_Verified: 2026-03-08T16:30:00Z_
_Verifier: Claude (gsd-verifier)_

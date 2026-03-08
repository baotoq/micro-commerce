# Roadmap: MicroCommerce

## Milestones

- ✅ **v1.0 MVP** — Phases 1-10 (shipped 2026-02-13) — [archive](milestones/v1.0-ROADMAP.md)
- ✅ **v1.1 User Features** — Phases 11-14.3 (shipped 2026-02-14) — [archive](milestones/v1.1-ROADMAP.md)
- ✅ **v2.0 DDD Foundation** — Phases 15-22 (shipped 2026-02-25) — [archive](milestones/v2.0-ROADMAP.md)
- ✅ **v3.0 Kubernetes & GitOps** — Phases 23-29 (shipped 2026-03-03) — [archive](milestones/v3.0-ROADMAP.md)
- 🚧 **v3.1 K8s & GitOps Hardening** — Phases 30-35 (in progress)

## Phases

<details>
<summary>✅ v1.0 MVP (Phases 1-10) — SHIPPED 2026-02-13</summary>

- [x] Phase 1: Foundation & Project Structure (6/6 plans) — completed 2026-01-30
- [x] Phase 2: Catalog Domain & Admin CRUD (7/7 plans) — completed 2026-01-30
- [x] Phase 3: Catalog Storefront & Seed Data (6/6 plans) — completed 2026-02-07
- [x] Phase 4: Inventory Domain (5/5 plans) — completed 2026-02-08
- [x] Phase 5: Event Bus Infrastructure (3/3 plans) — completed 2026-02-09
- [x] Phase 6: Cart Domain (4/4 plans) — completed 2026-02-09
- [x] Phase 7: Ordering Domain & Checkout (4/4 plans) — completed 2026-02-10
- [x] Phase 8: Order History & Management (5/5 plans) — completed 2026-02-12
- [x] Phase 9: API Gateway (3/3 plans) — completed 2026-02-12
- [x] Phase 10: Testing & Polish (6/6 plans) — completed 2026-02-13

</details>

<details>
<summary>✅ v1.1 User Features (Phases 11-14.3) — SHIPPED 2026-02-14</summary>

- [x] Phase 11: User Profiles & Authentication Flow (5/5 plans) — completed 2026-02-13
- [x] Phase 12: Product Reviews & Ratings (3/3 plans) — completed 2026-02-13
- [x] Phase 13: Wishlists & Saved Items (3/3 plans) — completed 2026-02-13
- [x] Phase 14: Integration & Polish (3/3 plans) — completed 2026-02-14
- [x] Phase 14.1: Check DDD Approach Correctness (2/2 plans) — completed 2026-02-14
- [x] Phase 14.2: ValueObject Record Struct Migration (3/3 plans) — completed 2026-02-14
- [x] Phase 14.3: DDD Audit Issue Fixes (4/4 plans) — completed 2026-02-14

</details>

<details>
<summary>✅ v2.0 DDD Foundation (Phases 15-22) — SHIPPED 2026-02-25</summary>

- [x] Phase 15: Foundation - Entity Base & Audit Infrastructure (2/2 plans) — completed 2026-02-14
- [x] Phase 16: Conventions - DRY Configuration (2/2 plans) — completed 2026-02-24
- [x] Phase 16.1: Adopt Vogen for Value Object (2/2 plans) — completed 2026-02-24
- [x] Phase 17: Result Pattern - Explicit Error Handling (2/2 plans) — completed 2026-02-24
- [x] Phase 18: Enumeration - Enums with Behavior (2/2 plans) — completed 2026-02-24
- [x] Phase 19: Specification Pattern - Complex Query Logic (2/2 plans) — completed 2026-02-24
- [x] Phase 20: Integration Testing Infrastructure (2/2 plans) — completed 2026-02-25
- [x] Phase 21: Adoption - Full Building Block Integration (3/3 plans) — completed 2026-02-25
- [x] Phase 22: Wire Interceptors to DbContexts (1/1 plan) — completed 2026-02-25

</details>

<details>
<summary>✅ v3.0 Kubernetes & GitOps (Phases 23-29) — SHIPPED 2026-03-03</summary>

- [x] Phase 23: Dockerfiles and Container Image Pipeline (3/3 plans) — completed 2026-02-25
- [x] Phase 24: Infrastructure Manifests and Secrets (4/4 plans) — completed 2026-02-26
- [x] Phase 25: Application Manifests and MassTransit Transport (3/3 plans) — completed 2026-02-26
- [x] Phase 25.1: Update UI with shadcn (12/12 plans) — completed 2026-03-02
- [x] Phase 26: ArgoCD GitOps (2/2 plans) — completed 2026-03-02
- [x] Phase 27: CI/CD GitOps Loop Closure (1/1 plan) — completed 2026-03-02
- [x] Phase 28: Observability (2/2 plans) — completed 2026-03-02
- [x] Phase 29: K8s Client-Side API & Bootstrap Polish (2/2 plans) — completed 2026-03-03

</details>

### v3.1 K8s & GitOps Hardening

**Milestone Goal:** Fix critical security, reliability, and CI/CD issues identified in the v3.0 implementation audit.

- [x] **Phase 30: CI/CD Pipeline Fixes** - Restore broken workflows with correct SDK, paths, and test gates (completed 2026-03-08)
- [x] **Phase 31: CI/CD Hardening** - Secure and optimize CI with permissions, caching, and path filters (completed 2026-03-08)
- [x] **Phase 32: Kustomize Hygiene** - Clean up K8s manifests with standard labels, image policies, and overlay structure (completed 2026-03-08)
- [ ] **Phase 33: K8s Security Hardening** - Lock down workloads with security contexts, service accounts, and sealed secrets
- [ ] **Phase 34: Reliability Improvements** - Harden runtime with persistent messaging, startup probes, and bootstrap safety
- [ ] **Phase 35: ArgoCD GitOps Best Practices** - Enforce deployment ordering, seal secrets in Git, and scope ArgoCD access

## Phase Details

### Phase 30: CI/CD Pipeline Fixes
**Goal**: CI workflows pass on push to master — tests run, images build, releases work
**Depends on**: Nothing (first phase — CI is broken, fix it first)
**Requirements**: CICD-01, CICD-02, CICD-03, CICD-07, CICD-08
**Success Criteria** (what must be TRUE):
  1. `dotnet-test.yml` workflow passes with .NET 10 SDK and current project paths
  2. `release.yml` builds and publishes images using correct project references
  3. Tests run as a required gate before release publishing
  4. Dockerfile uses ARG for build-time placeholder secrets (no ENV leaking to runtime)
  5. Aspire workload version is pinned (not floating latest)
**Plans:** 1/1 plans complete
Plans:
- [ ] 30-01-PLAN.md — Fix all CI/CD workflows (SDK, paths, test gate, Aspire, Dockerfile secrets)

### Phase 31: CI/CD Hardening
**Goal**: CI pipelines follow security and performance best practices
**Depends on**: Phase 30
**Requirements**: CICD-04, CICD-05, CICD-06
**Success Criteria** (what must be TRUE):
  1. All workflow files declare explicit least-privilege `permissions:` blocks
  2. NuGet packages are cached across workflow runs (faster builds)
  3. Container image builds skip when only non-source files change (docs, manifests)
**Plans:** 1/1 plans complete
Plans:
- [ ] 31-01-PLAN.md — Add permissions, NuGet caching, and path filters to all workflows

### Phase 32: Kustomize Hygiene
**Goal**: K8s manifests follow Kustomize conventions and Kubernetes labeling standards
**Depends on**: Nothing (independent of CI/CD fixes)
**Requirements**: KUST-01, KUST-02, KUST-03, KUST-04
**Success Criteria** (what must be TRUE):
  1. No hardcoded `namespace:` fields in base manifests — Kustomize transformer sets namespace
  2. otel-collector and aspire-dashboard appear in dev overlay (not base)
  3. All application deployments declare explicit `imagePullPolicy`
  4. All workloads carry standard `app.kubernetes.io/*` labels (name, component, part-of)
**Plans:** 2/2 plans complete
Plans:
- [ ] 32-01-PLAN.md — Remove hardcoded namespaces and move dev-only resources to overlay
- [ ] 32-02-PLAN.md — Add standard Kubernetes labels and imagePullPolicy to all workloads

### Phase 33: K8s Security Hardening
**Goal**: All workloads run with least-privilege security posture
**Depends on**: Phase 32 (clean manifests before adding security)
**Requirements**: SEC-01, SEC-02, SEC-03, SEC-04
**Success Criteria** (what must be TRUE):
  1. Web frontend secrets (AUTH_SECRET, KEYCLOAK_CLIENT_SECRET) are SealedSecrets, not plain Secrets
  2. All 8 workloads have securityContext: runAsNonRoot, readOnlyRootFilesystem, drop ALL capabilities
  3. Keycloak base manifest runs `start` (production mode); dev overlay patches to `start-dev`
  4. Each workload has its own ServiceAccount with automountServiceAccountToken: false
**Plans:** 1/2 plans executed
Plans:
- [ ] 33-01-PLAN.md — Security contexts and dedicated ServiceAccounts for all 8 workloads
- [ ] 33-02-PLAN.md — Web SealedSecrets and Keycloak production mode with dev overlay

### Phase 34: Reliability Improvements
**Goal**: Services survive restarts, start reliably, and bootstrap is safe to run
**Depends on**: Phase 32 (manifests cleaned up before adding probes/volumes)
**Requirements**: REL-01, REL-02, REL-03, REL-04, REL-05, REL-06, REL-07
**Success Criteria** (what must be TRUE):
  1. Client-side API calls in K8s reach the gateway via browser-reachable URL (not cluster-internal)
  2. RabbitMQ runs as StatefulSet with PersistentVolumeClaim — messages survive pod restarts
  3. Gateway, web, rabbitmq, otel-collector, and aspire-dashboard have startup probes
  4. Bootstrap script exits early with clear error if kind/kubectl/kubeseal CLI tools are missing
  5. Bootstrap script has trap handler that prints cleanup guidance on failure
  6. Bootstrap script refuses to run if kubectl context is not the expected kind cluster
  7. MassTransit outbox is registered on all DbContexts that publish domain events
**Plans**: TBD

### Phase 35: ArgoCD GitOps Best Practices
**Goal**: ArgoCD deployments are ordered, scoped, and fully declarative (sealed secrets in Git)
**Depends on**: Phase 33 (sealed secrets must exist before committing to Git)
**Requirements**: ARGO-01, ARGO-02, ARGO-03, ARGO-04, ARGO-05, SEC-05
**Success Criteria** (what must be TRUE):
  1. Sync wave annotations enforce infrastructure-before-apps deployment ordering
  2. All sealed secret YAML files are committed to Git and listed as Kustomize resources
  3. Root app retry strategy matches child app configuration (consistent retry behavior)
  4. PostgreSQL StatefulSet diffs are ignored by ArgoCD (RespectIgnoreDifferences enabled)
  5. All applications use consistent overlay paths (infrastructure services moved to overlays)
  6. Dedicated ArgoCD AppProject restricts sourceRepos and destination namespaces
**Plans**: TBD

## Progress

| Phase | Milestone | Plans | Status | Completed |
|-------|-----------|-------|--------|-----------|
| 1-10 | v1.0 | 49/49 | Complete | 2026-02-13 |
| 11-14.3 | v1.1 | 23/23 | Complete | 2026-02-14 |
| 15-22 | v2.0 | 9/9 | Complete | 2026-02-25 |
| 23-29 | v3.0 | 29/29 | Complete | 2026-03-03 |
| 30. CI/CD Pipeline Fixes | 1/1 | Complete    | 2026-03-08 | - |
| 31. CI/CD Hardening | 1/1 | Complete    | 2026-03-08 | - |
| 32. Kustomize Hygiene | 2/2 | Complete    | 2026-03-08 | - |
| 33. K8s Security Hardening | 1/2 | In Progress|  | - |
| 34. Reliability Improvements | v3.1 | 0/TBD | Not started | - |
| 35. ArgoCD GitOps Best Practices | v3.1 | 0/TBD | Not started | - |

---
*Roadmap created: 2026-01-29*
*v1.0 shipped: 2026-02-13*
*v1.1 shipped: 2026-02-14*
*v2.0 shipped: 2026-02-25*
*v3.0 shipped: 2026-03-03*
*v3.1 roadmap created: 2026-03-08*

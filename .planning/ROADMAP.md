# Roadmap: MicroCommerce

## Milestones

- ✅ **v1.0 MVP** — Phases 1-10 (shipped 2026-02-13) — [archive](milestones/v1.0-ROADMAP.md)
- ✅ **v1.1 User Features** — Phases 11-14.3 (shipped 2026-02-14) — [archive](milestones/v1.1-ROADMAP.md)
- ✅ **v2.0 DDD Foundation** — Phases 15-22 (shipped 2026-02-25) — [archive](milestones/v2.0-ROADMAP.md)
- 🚧 **v3.0 Kubernetes & GitOps** — Phases 23-28 (in progress)

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

---

### 🚧 v3.0 Kubernetes & GitOps (In Progress)

**Milestone Goal:** Deploy the full MicroCommerce stack to a kind-based Kubernetes cluster using GitOps via ArgoCD, with GitHub Actions CI building and pushing images to ghcr.io, Kustomize managing manifests, and Sealed Secrets handling credentials safely in Git.

- [x] **Phase 23: Dockerfiles and Container Image Pipeline** - Build production-ready images for all three services and push them to ghcr.io via GitHub Actions (completed 2026-02-25)
- [ ] **Phase 24: Infrastructure Manifests and Secrets** - Deploy PostgreSQL, RabbitMQ, and Keycloak in the kind cluster with Sealed Secrets for all credentials
- [ ] **Phase 25: Application Manifests and MassTransit Transport** - Deploy ApiService, Gateway, and Web with Kustomize base/overlay structure and RabbitMQ transport support
- [ ] **Phase 26: ArgoCD GitOps** - Install ArgoCD and wire app-of-apps to manage all cluster resources from Git
- [ ] **Phase 27: CI/CD GitOps Loop Closure** - Extend CI to commit SHA image tags back to the overlay, completing the full GitOps loop
- [ ] **Phase 28: Observability** - Deploy OTEL Collector and standalone Aspire Dashboard for in-cluster monitoring

## Phase Details

### Phase 23: Dockerfiles and Container Image Pipeline
**Goal**: Three production-ready container images exist in ghcr.io and are built automatically on every push to master
**Depends on**: Phase 22 (v2.0 complete)
**Requirements**: CONT-01, CONT-02, CONT-03, CONT-04, CICD-01
**Success Criteria** (what must be TRUE):
  1. `docker build` succeeds for ApiService, Gateway, and Web with no warnings or root-user violations
  2. `docker run` starts each service and the process responds to health checks within 30 seconds
  3. A push to master triggers the GitHub Actions workflow and all three images appear in ghcr.io tagged with the commit SHA
  4. Image sizes are reasonable: .NET images use chiseled runtime and Next.js uses standalone output with node:alpine
**Plans**: TBD

### Phase 24: Infrastructure Manifests and Secrets
**Goal**: PostgreSQL, RabbitMQ, and Keycloak run stably in the kind cluster with all credentials sealed in Git
**Depends on**: Phase 23
**Requirements**: INFRA-01, INFRA-02, INFRA-03, INFRA-04, INFRA-05, GOPS-03
**Success Criteria** (what must be TRUE):
  1. `kind create cluster --config kind-config.yaml` creates a cluster and all infrastructure pods reach Running state
  2. PostgreSQL data persists across pod restarts (PVC-backed StatefulSet)
  3. Keycloak admin UI is reachable and the MicroCommerce realm is pre-loaded from a ConfigMap
  4. No plaintext credentials exist in any committed YAML file; all secrets are SealedSecret objects
  5. ApiService startup probe prevents liveness failure during the slow first-boot EF Core migration
**Plans**: 3 plans
- [ ] 24-01-PLAN.md -- Kind cluster config, Kustomize scaffold, PostgreSQL StatefulSet
- [ ] 24-02-PLAN.md -- RabbitMQ Deployment, Keycloak Deployment with realm import
- [ ] 24-03-PLAN.md -- Bootstrap script with SealedSecrets integration

### Phase 25: Application Manifests and MassTransit Transport
**Goal**: ApiService, Gateway, and Web are deployed via Kustomize and communicate correctly using K8s DNS and RabbitMQ messaging
**Depends on**: Phase 24
**Requirements**: MFST-01, MFST-02, MFST-03, MFST-04, MFST-05, MFST-06, TRAN-01, TRAN-02
**Success Criteria** (what must be TRUE):
  1. The storefront loads in a browser via the Gateway NodePort — browsing products and adding to cart works end-to-end
  2. Checkout completes successfully using the RabbitMQ transport (saga orchestrates stock reservation and mock payment)
  3. All pods report liveness and readiness as healthy (`kubectl get pods -n micro-commerce` shows all Running/Ready)
  4. All resources are in the `micro-commerce` namespace and have CPU/memory resource limits set
  5. Setting `MASSTRANSIT_TRANSPORT=AzureServiceBus` (Aspire dev path) and `RabbitMQ` (K8s path) both produce working configurations
**Plans**: TBD

### Phase 26: ArgoCD GitOps
**Goal**: ArgoCD manages all cluster resources from Git using app-of-apps, replacing manual kubectl apply
**Depends on**: Phase 25
**Requirements**: GOPS-01, GOPS-02
**Success Criteria** (what must be TRUE):
  1. ArgoCD UI is reachable in the kind cluster and shows all services as Synced and Healthy
  2. Deleting a deployment manually causes ArgoCD to automatically restore it within one sync cycle
  3. The app-of-apps root Application manages each service as an independent child Application
**Plans**: TBD

### Phase 27: CI/CD GitOps Loop Closure
**Goal**: A push to master automatically flows through CI image build, Git tag commit, and ArgoCD cluster rollout without manual intervention
**Depends on**: Phase 26
**Requirements**: CICD-02
**Success Criteria** (what must be TRUE):
  1. After a push to master, a new pod with the updated SHA-tagged image is running in the cluster within 5 minutes of the CI workflow completing
  2. Every deployment is traceable to a Git commit — the image tag in the overlay matches the commit SHA that triggered the build
**Plans**: TBD

### Phase 28: Observability
**Goal**: Traces, metrics, and logs from all cluster services are visible in the Aspire Dashboard without requiring .NET Aspire AppHost
**Depends on**: Phase 27
**Requirements**: OBSV-01, OBSV-02
**Success Criteria** (what must be TRUE):
  1. The Aspire Dashboard UI is reachable in the kind cluster and shows live traces from ApiService and Gateway
  2. A completed checkout flow produces a distributed trace visible end-to-end in the Dashboard spanning ApiService, the saga, and RabbitMQ consumers
  3. OTEL Collector pod stays healthy under normal load (no OOM kills, no dropped telemetry in logs)
**Plans**: TBD

## Progress

**Execution Order:** 23 → 24 → 25 → 26 → 27 → 28

| Phase | Milestone | Plans Complete | Status | Completed |
|-------|-----------|----------------|--------|-----------|
| 1. Foundation & Project Structure | v1.0 | 6/6 | Complete | 2026-01-30 |
| 2. Catalog Domain & Admin CRUD | v1.0 | 7/7 | Complete | 2026-01-30 |
| 3. Catalog Storefront & Seed Data | v1.0 | 6/6 | Complete | 2026-02-07 |
| 4. Inventory Domain | v1.0 | 5/5 | Complete | 2026-02-08 |
| 5. Event Bus Infrastructure | v1.0 | 3/3 | Complete | 2026-02-09 |
| 6. Cart Domain | v1.0 | 4/4 | Complete | 2026-02-09 |
| 7. Ordering Domain & Checkout | v1.0 | 4/4 | Complete | 2026-02-10 |
| 8. Order History & Management | v1.0 | 5/5 | Complete | 2026-02-12 |
| 9. API Gateway | v1.0 | 3/3 | Complete | 2026-02-12 |
| 10. Testing & Polish | v1.0 | 6/6 | Complete | 2026-02-13 |
| 11. User Profiles & Authentication Flow | v1.1 | 5/5 | Complete | 2026-02-13 |
| 12. Product Reviews & Ratings | v1.1 | 3/3 | Complete | 2026-02-13 |
| 13. Wishlists & Saved Items | v1.1 | 3/3 | Complete | 2026-02-13 |
| 14. Integration & Polish | v1.1 | 3/3 | Complete | 2026-02-14 |
| 14.1. Check DDD Approach Correctness | v1.1 | 2/2 | Complete | 2026-02-14 |
| 14.2. ValueObject Record Struct Migration | v1.1 | 3/3 | Complete | 2026-02-14 |
| 14.3. DDD Audit Issue Fixes | v1.1 | 4/4 | Complete | 2026-02-14 |
| 15. Foundation - Entity Base & Audit Infrastructure | v2.0 | 2/2 | Complete | 2026-02-14 |
| 16. Conventions - DRY Configuration | v2.0 | 2/2 | Complete | 2026-02-24 |
| 16.1. Adopt Vogen for Value Object | v2.0 | 2/2 | Complete | 2026-02-24 |
| 17. Result Pattern - Explicit Error Handling | v2.0 | 2/2 | Complete | 2026-02-24 |
| 18. Enumeration - Enums with Behavior | v2.0 | 2/2 | Complete | 2026-02-24 |
| 19. Specification Pattern - Complex Query Logic | v2.0 | 2/2 | Complete | 2026-02-24 |
| 20. Integration Testing Infrastructure | v2.0 | 2/2 | Complete | 2026-02-25 |
| 21. Adoption - Full Building Block Integration | v2.0 | 3/3 | Complete | 2026-02-25 |
| 22. Wire Interceptors to DbContexts | v2.0 | 1/1 | Complete | 2026-02-25 |
| 23. Dockerfiles and Container Image Pipeline | 3/3 | Complete    | 2026-02-25 | - |
| 24. Infrastructure Manifests and Secrets | 2/3 | In Progress|  | - |
| 25. Application Manifests and MassTransit Transport | v3.0 | 0/TBD | Not started | - |
| 26. ArgoCD GitOps | v3.0 | 0/TBD | Not started | - |
| 27. CI/CD GitOps Loop Closure | v3.0 | 0/TBD | Not started | - |
| 28. Observability | v3.0 | 0/TBD | Not started | - |

---
*Roadmap created: 2026-01-29*
*v1.0 shipped: 2026-02-13*
*v1.1 shipped: 2026-02-14*
*v2.0 shipped: 2026-02-25*
*v3.0 started: 2026-02-25*

# Requirements: MicroCommerce

**Defined:** 2026-03-08
**Core Value:** A user can complete a purchase end-to-end — deployed to Kubernetes via GitOps

## v3.1 Requirements

Requirements for K8s & GitOps Hardening. Each maps to roadmap phases.

### CI/CD

- [x] **CICD-01**: .NET SDK version updated to 10.0.x in dotnet-test.yml and release.yml
- [x] **CICD-02**: Stale project paths in release.yml updated to current structure (MicroCommerce.ApiService, MicroCommerce.Gateway)
- [x] **CICD-03**: Aspire workload install pinned to specific version
- [ ] **CICD-04**: Explicit least-privilege `permissions:` block on all workflows
- [ ] **CICD-05**: NuGet package caching added to test and image build workflows
- [ ] **CICD-06**: Path filtering on container-images.yml to skip non-source changes
- [x] **CICD-07**: Tests re-enabled as gate in release.yml
- [x] **CICD-08**: Dockerfile uses ARG instead of ENV for build-time placeholder secrets

### Security

- [ ] **SEC-01**: Web frontend secrets (AUTH_SECRET, KEYCLOAK_CLIENT_SECRET) moved to SealedSecret
- [ ] **SEC-02**: securityContext added to all 8 workloads (runAsNonRoot, readOnlyRootFilesystem, drop ALL capabilities)
- [ ] **SEC-03**: Keycloak base manifest uses `start` (production mode); dev overlay patches to `start-dev`
- [ ] **SEC-04**: Dedicated ServiceAccounts per workload with automountServiceAccountToken: false
- [ ] **SEC-05**: Dedicated ArgoCD AppProject with restricted sourceRepos and destinations

### ArgoCD

- [ ] **ARGO-01**: Sync wave annotations on all Application resources enforcing dependency ordering
- [ ] **ARGO-02**: Sealed secret YAML files committed to Git and added as Kustomize resources
- [ ] **ARGO-03**: Root app retry strategy matching child app configuration
- [ ] **ARGO-04**: ignoreDifferences configured for PostgreSQL StatefulSet with RespectIgnoreDifferences
- [ ] **ARGO-05**: Consistent overlay paths for all applications (infrastructure services moved to overlays)

### Reliability

- [ ] **REL-01**: Client-side API URL resolution returns browser-reachable URL in K8s
- [ ] **REL-02**: RabbitMQ converted to StatefulSet with persistent volume
- [ ] **REL-03**: Startup probes added to gateway, web, rabbitmq, otel-collector, aspire-dashboard
- [ ] **REL-04**: Bootstrap script checks for required CLI tools before execution
- [ ] **REL-05**: Bootstrap script adds trap handler for failure cleanup guidance
- [ ] **REL-06**: Bootstrap script context guard prevents applying to wrong cluster
- [ ] **REL-07**: MassTransit outbox extended to cover all DbContexts that publish domain events

### Kustomize Hygiene

- [ ] **KUST-01**: Hardcoded namespaces removed from base manifests (Kustomize transformer handles it)
- [ ] **KUST-02**: otel-collector and aspire-dashboard added to dev overlay
- [ ] **KUST-03**: Explicit imagePullPolicy on all application deployments
- [ ] **KUST-04**: Standard Kubernetes labels (app.kubernetes.io/*) on all workloads

## Future Requirements

None deferred — all audit findings scoped to v3.1.

## Out of Scope

| Feature | Reason |
|---------|--------|
| Network policies | Low priority for local dev kind cluster |
| PodDisruptionBudgets | Single-replica dev; add when scaling |
| Pod anti-affinity/topology spread | Irrelevant for single-node kind cluster |
| ResourceQuota/LimitRange | Resource limits already on all containers |
| Prometheus scrape annotations | OTEL Collector handles observability |
| Next.js frontend OTEL instrumentation | Separate concern, not v3.0 hardening |
| Random generated credentials in bootstrap | Dev-only kind cluster; document the pattern |
| Pin image digests (node, postgres, etc.) | Dependabot handles version updates |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| CICD-01 | Phase 30 | Complete |
| CICD-02 | Phase 30 | Complete |
| CICD-03 | Phase 30 | Complete |
| CICD-04 | Phase 31 | Pending |
| CICD-05 | Phase 31 | Pending |
| CICD-06 | Phase 31 | Pending |
| CICD-07 | Phase 30 | Complete |
| CICD-08 | Phase 30 | Complete |
| SEC-01 | Phase 33 | Pending |
| SEC-02 | Phase 33 | Pending |
| SEC-03 | Phase 33 | Pending |
| SEC-04 | Phase 33 | Pending |
| SEC-05 | Phase 35 | Pending |
| ARGO-01 | Phase 35 | Pending |
| ARGO-02 | Phase 35 | Pending |
| ARGO-03 | Phase 35 | Pending |
| ARGO-04 | Phase 35 | Pending |
| ARGO-05 | Phase 35 | Pending |
| REL-01 | Phase 34 | Pending |
| REL-02 | Phase 34 | Pending |
| REL-03 | Phase 34 | Pending |
| REL-04 | Phase 34 | Pending |
| REL-05 | Phase 34 | Pending |
| REL-06 | Phase 34 | Pending |
| REL-07 | Phase 34 | Pending |
| KUST-01 | Phase 32 | Pending |
| KUST-02 | Phase 32 | Pending |
| KUST-03 | Phase 32 | Pending |
| KUST-04 | Phase 32 | Pending |

**Coverage:**
- v3.1 requirements: 29 total
- Mapped to phases: 29
- Unmapped: 0

---
*Requirements defined: 2026-03-08*
*Last updated: 2026-03-08 — traceability updated with phase assignments*

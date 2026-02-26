# Requirements: MicroCommerce v3.0

**Defined:** 2026-02-25
**Core Value:** A user can complete a purchase end-to-end — now deployed to Kubernetes via GitOps

## v3.0 Requirements

Requirements for Kubernetes & GitOps deployment milestone. Each maps to roadmap phases.

### Containerization

- [x] **CONT-01**: ApiService has a multi-stage Dockerfile with layer-cached restore and chiseled runtime image
- [x] **CONT-02**: Gateway has a multi-stage Dockerfile with layer-cached restore and chiseled runtime image
- [x] **CONT-03**: Web (Next.js) has a multi-stage Dockerfile with standalone output and node:alpine runtime
- [x] **CONT-04**: All 3 images are pushed to ghcr.io with SHA-based tags

### K8s Infrastructure

- [x] **INFRA-01**: PostgreSQL runs as a StatefulSet with PersistentVolumeClaim in the kind cluster
- [ ] **INFRA-02**: RabbitMQ runs as a Deployment with Service in the kind cluster
- [ ] **INFRA-03**: Keycloak runs as a Deployment with realm JSON imported via ConfigMap
- [x] **INFRA-04**: kind cluster can be created from a config file with port mappings for local access
- [ ] **INFRA-05**: Startup probes protect Keycloak and ApiService from premature liveness failures

### Application Manifests

- [ ] **MFST-01**: ApiService, Gateway, and Web have Deployment + Service + ConfigMap manifests
- [ ] **MFST-02**: All app services have liveness (`/alive`) and readiness (`/health`) probes configured
- [ ] **MFST-03**: All containers have CPU and memory resource requests and limits
- [ ] **MFST-04**: Kustomize base directory contains all environment-neutral manifests
- [ ] **MFST-05**: Kustomize dev overlay patches image tags and resource limits for kind
- [ ] **MFST-06**: All resources are namespaced under `micro-commerce`

### MassTransit Transport

- [ ] **TRAN-01**: MassTransit supports RabbitMQ transport in K8s deployments
- [ ] **TRAN-02**: Transport selection is configurable via `MASSTRANSIT_TRANSPORT` env var (Azure SB default, RabbitMQ for K8s)

### CI/CD Pipeline

- [x] **CICD-01**: GitHub Actions workflow builds and pushes all 3 images to ghcr.io on push to master
- [ ] **CICD-02**: CI commits updated SHA image tags to Kustomize dev overlay, triggering ArgoCD sync

### GitOps

- [ ] **GOPS-01**: ArgoCD is installed in the kind cluster and manages all services
- [ ] **GOPS-02**: App-of-apps root Application manages per-service child Applications
- [ ] **GOPS-03**: Sealed Secrets controller encrypts all credentials; SealedSecret YAMLs committed to Git

### Observability

- [ ] **OBSV-01**: OTEL Collector receives OTLP from all app services and forwards to Aspire Dashboard
- [ ] **OBSV-02**: Aspire Dashboard runs as a standalone container in K8s accessible for dev monitoring

## Future Requirements

Deferred to post-v3.0. Tracked but not in current roadmap.

### Scaling & Hardening

- **SCALE-01**: Horizontal Pod Autoscaler with metrics-based scaling
- **SCALE-02**: Network Policies for pod-to-pod traffic control
- **SCALE-03**: cert-manager with TLS termination at ingress
- **SCALE-04**: PostgreSQL Operator (CloudNativePG) for HA database

### Multi-Environment

- **ENV-01**: Staging overlay for pre-production validation
- **ENV-02**: Production overlay with production-grade resource limits
- **ENV-03**: Multi-cluster deployment with ArgoCD

### Storage

- **STOR-01**: MinIO for Blob Storage replacement in K8s (avatar/product images)

## Out of Scope

| Feature | Reason |
|---------|--------|
| Service Mesh (Istio/Linkerd) | YARP Gateway already centralizes auth/routing; massive operational complexity |
| Helm charts | Kustomize is simpler for single-project, built into kubectl |
| External Secrets Operator | Requires external vault; Sealed Secrets works offline with kind |
| FluxCD | ArgoCD already chosen; switching tools is waste |
| Multi-environment overlays | Only dev cluster exists; structure repo for easy addition later |
| Ingress NGINX | EOL March 2026; use NodePort for kind local access |
| Azure Blob Storage in K8s | Azurite emulator not suitable for K8s; use placeholder images |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| CONT-01 | Phase 23 | Complete |
| CONT-02 | Phase 23 | Complete |
| CONT-03 | Phase 23 | Complete |
| CONT-04 | Phase 23 | Complete |
| CICD-01 | Phase 23 | Complete |
| INFRA-01 | Phase 24 | Complete |
| INFRA-02 | Phase 24 | Pending |
| INFRA-03 | Phase 24 | Pending |
| INFRA-04 | Phase 24 | Complete |
| INFRA-05 | Phase 24 | Pending |
| GOPS-03 | Phase 24 | Pending |
| MFST-01 | Phase 25 | Pending |
| MFST-02 | Phase 25 | Pending |
| MFST-03 | Phase 25 | Pending |
| MFST-04 | Phase 25 | Pending |
| MFST-05 | Phase 25 | Pending |
| MFST-06 | Phase 25 | Pending |
| TRAN-01 | Phase 25 | Pending |
| TRAN-02 | Phase 25 | Pending |
| GOPS-01 | Phase 26 | Pending |
| GOPS-02 | Phase 26 | Pending |
| CICD-02 | Phase 27 | Pending |
| OBSV-01 | Phase 28 | Pending |
| OBSV-02 | Phase 28 | Pending |

**Coverage:**
- v3.0 requirements: 24 total
- Mapped to phases: 24
- Unmapped: 0 ✓

---
*Requirements defined: 2026-02-25*
*Last updated: 2026-02-25 — traceability filled after roadmap creation (Phases 23-28)*

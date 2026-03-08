# Milestones

## v3.1 K8s & GitOps Hardening (Shipped: 2026-03-08)

**Phases completed:** 6 phases, 11 plans, 20 tasks
**Timeline:** 1 day (2026-03-08)
**Stats:** 20 feat commits, 58 files, +694 / -58 lines
**Requirements:** 29/29 satisfied (100%)

**Key accomplishments:**
- Fixed all 3 CI workflows (.NET 10.0.x SDK, test gate, correct project paths) and hardened with least-privilege permissions, NuGet caching, and path filters
- Kustomize hygiene: removed hardcoded namespaces, added standard `app.kubernetes.io/*` labels, explicit `imagePullPolicy`, and dev overlay structure for all 8 services
- Security hardened all 8 K8s workloads with securityContext (runAsNonRoot, readOnlyRootFilesystem, drop ALL), dedicated ServiceAccounts, and SealedSecrets for web frontend secrets
- Keycloak production-mode base manifest with dev overlay patch pattern for start-dev
- Reliability improvements: RabbitMQ StatefulSet with PVC, startup probes on 7 workloads, MassTransit outbox on all 5 domain-event DbContexts, bootstrap script safety (pre-flight, context guard, trap handler)
- ArgoCD best practices: dedicated AppProject scoping, sync wave ordering (infrastructure-before-apps), consistent overlay paths, sealed secret placeholders committed to Git, retry strategy alignment

**Archives:**
- [v3.1 Roadmap](milestones/v3.1-ROADMAP.md)
- [v3.1 Requirements](milestones/v3.1-REQUIREMENTS.md)
- [v3.1 Audit](milestones/v3.1-MILESTONE-AUDIT.md)

---

## v3.0 Kubernetes & GitOps (Shipped: 2026-03-03)

**Phases completed:** 8 phases, 29 plans
**Timeline:** 5 days (2026-02-26 → 2026-03-03)
**Stats:** 45 feat commits, 226 files, +41,530 / -4,331 lines
**Requirements:** 38/38 satisfied (100%)

**Key accomplishments:**
- Three production-ready Docker images (ApiService chiseled .NET, Gateway chiseled .NET, Web standalone Node.js) built and pushed to ghcr.io via GitHub Actions CI
- K8s infrastructure: PostgreSQL StatefulSet, RabbitMQ, Keycloak running in kind cluster with Sealed Secrets for all credentials
- All services deployed via Kustomize with health probes, resource limits, and MassTransit RabbitMQ transport switching
- Complete UI refresh: 11 storefront and admin pages restyled with shadcn/ui, oklch color tokens, and DM Sans typography
- GitOps pipeline: ArgoCD app-of-apps manages all cluster resources; CI auto-commits SHA image tags triggering zero-touch deployments
- Observability: OTEL Collector + Aspire Dashboard provide distributed tracing and metrics in K8s without .NET Aspire AppHost
- Runtime API URL resolution for client-side features in K8s deployment via getApiBase() singleton

**Archives:**
- [v3.0 Roadmap](milestones/v3.0-ROADMAP.md)
- [v3.0 Requirements](milestones/v3.0-REQUIREMENTS.md)
- [v3.0 Audit](milestones/v3.0-MILESTONE-AUDIT.md)

---

## v1.0 MVP (Shipped: 2026-02-13)

**Phases completed:** 10 phases, 49 plans
**Timeline:** 16 days (2026-01-29 → 2026-02-13)
**Stats:** 187 commits, 647 files, 94,355 lines added
**Requirements:** 24/24 satisfied (100%)

**Key accomplishments:**
- Modular monolith foundation with CQRS (MediatR), FluentValidation, and transactional outbox
- Product catalog with admin CRUD, storefront browsing, search, category filters, and infinite scroll
- Inventory tracking with stock reservations, TTL expiration, and optimistic concurrency
- Event-driven architecture with idempotent consumers, DLQ management, and circuit breakers
- Shopping cart with guest support, optimistic UI mutations, and database persistence
- Checkout saga orchestrating stock reservation, mock payments, and compensation handlers
- Order history, admin dashboard with Kanban board, stat cards, and status management
- YARP API Gateway with JWT auth, rate limiting, CORS, and Aspire service discovery
- 180 automated tests (144 unit + 29 integration + 7 E2E) with Testcontainers and Playwright

**Archives:**
- [v1.0 Roadmap](.planning/milestones/v1.0-ROADMAP.md)
- [v1.0 Requirements](.planning/milestones/v1.0-REQUIREMENTS.md)
- [v1.0 Audit](.planning/milestones/v1.0-MILESTONE-AUDIT.md)

---


## v1.1 User Features (Shipped: 2026-02-14)

**Phases completed:** 7 phases, 23 plans
**Timeline:** 2 days (2026-02-13 → 2026-02-14)
**Stats:** 98 commits, 349 files, +46,500 / -8,523 lines
**Requirements:** 21/21 satisfied (100%)

**Key accomplishments:**
- User profiles with display name, avatar upload (ImageSharp), address book, and guest-to-auth cart merge
- Product reviews with verified purchase enforcement, star ratings, and denormalized aggregate ratings
- Wishlists with add/remove, move-to-cart, and optimistic heart icon indicators
- Integration polish with consolidated review UX, content-matching skeletons, and E2E Playwright tests
- Full DDD audit (71 findings across 7 modules) with severity-tagged report
- Value objects migrated to readonly record structs (20x faster equality), CQRS compliance fixes, obsolete infrastructure removal

**Archives:**
- [v1.1 Roadmap](.planning/milestones/v1.1-ROADMAP.md)
- [v1.1 Requirements](.planning/milestones/v1.1-REQUIREMENTS.md)

---


## v2.0 DDD Foundation (Shipped: 2026-02-25)

**Phases completed:** 26 phases, 90 plans, 49 tasks

**Key accomplishments:**
- (none recorded)

---


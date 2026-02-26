# Phase 25: Application Manifests and MassTransit Transport - Context

**Gathered:** 2026-02-26
**Status:** Ready for planning

<domain>
## Phase Boundary

Deploy ApiService, Gateway, and Web to the kind cluster via Kustomize manifests, and configure MassTransit to support both Azure Service Bus (Aspire dev) and RabbitMQ (K8s) transports. The full e-commerce flow — browsing products, adding to cart, and checkout — must work end-to-end in K8s.

Infrastructure services (PostgreSQL, RabbitMQ, Keycloak) are already deployed from Phase 24. This phase adds application service manifests and the transport abstraction layer.

</domain>

<decisions>
## Implementation Decisions

### MassTransit transport switching
- Environment variable `MASSTRANSIT_TRANSPORT` controls which transport is active: `RabbitMQ` or `AzureServiceBus`
- Default when not set: Azure Service Bus (preserves existing Aspire dev workflow)
- RabbitMQ transport uses the same transactional outbox pattern as Azure Service Bus — consistent reliability across environments, checkout saga behavior is identical
- RabbitMQ host is injected via environment variable (e.g., `RabbitMQ__Host`), not hardcoded — K8s manifest sets this to the K8s DNS name (`rabbitmq.micro-commerce.svc.cluster.local`)
- RabbitMQ credentials injected via environment variables referencing the existing `rabbitmq-credentials` SealedSecret

### App configuration in K8s
- All configuration via environment variables in Deployment manifests (12-factor app style)
- Kustomize overlays can patch env vars per environment
- Database connection strings composed from SealedSecret refs: host from K8s DNS (`postgres.micro-commerce.svc.cluster.local`), username/password from `postgres-credentials` secretKeyRef, database name `appdb`
- Keycloak URLs injected as env vars pointing to K8s internal DNS (`http://keycloak.micro-commerce.svc.cluster.local:8080`)
- Gateway discovers ApiService and Web via env vars: `SERVICE_URL_APISERVICE=http://apiservice:8080`, `SERVICE_URL_WEB=http://web:3000` — Gateway reads these to configure YARP routes
- Web (Next.js) uses standard `next start` with standalone output mode — health checks via HTTP probe on `/`, no custom server needed

### Claude's Discretion
- Dockerfile multi-stage build structure for each service
- Container image tagging strategy for kind load
- Exact resource limits (CPU/memory) per app service
- Startup probe timing for ApiService EF Core migration
- How to integrate image build + kind load into bootstrap.sh or a separate script
- Init container vs startup probe for database migration readiness

</decisions>

<specifics>
## Specific Ideas

- The transport switch should be transparent to business logic — consumers, sagas, and publishers should not know which transport is active
- Aspire dev path (Azure Service Bus emulator) and K8s path (real RabbitMQ) should produce identical observable behavior from the application's perspective
- Gateway YARP route configuration should work both ways: Aspire service discovery (existing) and K8s env var injection (new)

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 25-application-manifests-and-masstransit-transport*
*Context gathered: 2026-02-26*

# MicroCommerce

## What This Is

A showcase e-commerce platform demonstrating modern .NET microservices architecture with DDD tactical patterns. Users can browse products, manage profiles, write verified reviews, save wishlists, add items to cart, and complete purchases through a polished Next.js storefront backed by a modular monolith with event-driven architecture, CQRS, and a comprehensive DDD building block foundation (strongly-typed IDs, SmartEnums, Result pattern, Specifications, interceptor-driven audit/concurrency/soft-delete).

## Core Value

**A user can complete a purchase end-to-end** — browse products, add to cart, checkout as a guest, and see their order confirmation. Everything else supports this flow.

## Requirements

### Validated

- ✓ Product catalog with categories, search, and infinite scroll — v1.0
- ✓ Product detail pages with stock display and add-to-cart — v1.0
- ✓ Shopping cart (persistent, guest-friendly, optimistic UI) — v1.0
- ✓ Checkout flow with mock payment and saga orchestration — v1.0
- ✓ Order confirmation and order history with real-time status — v1.0
- ✓ Inventory tracking with stock reservations and TTL — v1.0
- ✓ Admin UI for product management, inventory, and orders — v1.0
- ✓ Admin dashboard with stat cards, charts, and Kanban board — v1.0
- ✓ Seed data with 50 products across 8 categories — v1.0
- ✓ API Gateway (YARP) with JWT auth and rate limiting — v1.0
- ✓ Event-driven communication (MassTransit + Azure Service Bus) — v1.0
- ✓ CQRS pattern with MediatR and validation pipeline — v1.0
- ✓ Database-per-feature isolation with separate DbContexts — v1.0
- ✓ 180 automated tests (unit, integration, E2E) — v1.0
- ✓ Keycloak authentication (backend JWT + NextAuth.js) — existing
- ✓ .NET Aspire orchestration — existing
- ✓ DDD building blocks (aggregates, domain events, value objects) — existing
- ✓ OpenTelemetry observability — existing
- ✓ User profiles with display name, avatar, address book — v1.1
- ✓ Guest-to-authenticated cart and order merge on login — v1.1
- ✓ Product reviews with verified purchase enforcement (star ratings + text) — v1.1
- ✓ Aggregate star ratings and review counts on product pages — v1.1
- ✓ Wishlists with add/remove, move-to-cart, heart icon indicators — v1.1
- ✓ E2E test coverage for user features (guest, authenticated, navigation) — v1.1
- ✓ DDD audit (71 findings) with value object migration to record structs — v1.1
- ✓ CQRS compliance fixes and aggregate boundary enforcement — v1.1
- ✓ Entity hierarchy (Entity<TId>, AuditableAggregateRoot<TId>) with typed ID child entities — v2.0
- ✓ Vogen source-generated strongly-typed IDs (14 types) with auto EF Core + JSON converters — v2.0
- ✓ EF Core conventions for StronglyTypedId auto-conversion and ConcurrencyToken auto-config — v2.0
- ✓ SmartEnum state machines for OrderStatus (8 states) and ProductStatus (3 states) — v2.0
- ✓ Result pattern (FluentResults) with dual MediatR validation pipeline and HTTP mapping — v2.0
- ✓ Specification pattern (Ardalis.Specification) with 8 composable query specs — v2.0
- ✓ AuditInterceptor auto-setting CreatedAt/UpdatedAt on IAuditable entities — v2.0
- ✓ ConcurrencyInterceptor with explicit Version column replacing xmin — v2.0
- ✓ SoftDeleteInterceptor infrastructure wired (ready for future entity adoption) — v2.0
- ✓ Integration testing with Testcontainers + WebApplicationFactory (182 tests) — v2.0
- ✓ Full building block adoption across all 7 feature modules — v2.0
- ✓ OpenAPI schema transformers for Vogen IDs and SmartEnums — v2.0
- ✓ Obsolete ValueObject base class and StronglyTypedIdConvention removed — v2.0

### Active

**Current Milestone: v3.0 Kubernetes & GitOps**

- [ ] Dockerfiles for ApiService, Gateway, and Web
- [ ] Kustomize manifests (base + dev overlay) for all services and infrastructure
- [ ] Full stack deployment in K8s: apps + PostgreSQL + Keycloak + RabbitMQ
- [ ] RabbitMQ transport support for MassTransit (replacing Azure SB emulator in K8s)
- [ ] GitHub Actions CI pipeline building and pushing images to ghcr.io
- [ ] ArgoCD with app-of-apps pattern for GitOps deployment
- [ ] Sealed Secrets for K8s secret management
- [ ] OTEL Collector + Aspire Dashboard deployed in K8s for monitoring
- [ ] Local kind cluster as dev environment

### Out of Scope

- Real payment processing (Stripe, etc.) — mock payments sufficient for demo
- Event sourcing — adds complexity without proportional demo value
- Separate admin application — integrated admin routes simpler
- Mobile app — web-first, responsive design covers mobile
- Multi-tenancy — single store demonstration
- Internationalization — English only for now
- Anonymous reviews — trust collapse risk; verified purchase enforcement is core
- Public user profiles — privacy concern; profiles are private to account owner
- AI review summaries — requires LLM integration, cost analysis
- Review image moderation — requires moderation service
- Generic Repository wrapper — EF Core DbContext already is unit-of-work + repository
- Event sourcing primitives — adds complexity without proportional demo value
- CreatedBy/ModifiedBy user tracking — requires IHttpContextAccessor integration, defer to future

## Context

**Shipped v2.0 DDD Foundation** with 27K LOC C# backend and Next.js 16 frontend. 182 tests (144 unit + 38 integration).

**Tech stack:**
- Backend: .NET 10, ASP.NET Core Minimal APIs, .NET Aspire 13.1.0
- Frontend: Next.js 16, React 19, TypeScript 5, shadcn/ui
- Auth: Keycloak (backend JWT + NextAuth.js frontend)
- Database: PostgreSQL (per-feature isolation, 8 DbContexts)
- Messaging: Azure Service Bus (MassTransit) with emulator
- Gateway: YARP reverse proxy with JWT, rate limiting, CORS
- Patterns: CQRS (MediatR), DDD, Event-Driven, Vertical Slice Architecture
- DDD Building Blocks: Vogen StronglyTypedId, SmartEnum, FluentResults, Ardalis.Specification, EF Core Interceptors
- Testing: xUnit, FluentAssertions, Testcontainers, WebApplicationFactory

**Architecture:** Modular monolith with clear bounded contexts (Catalog, Cart, Ordering, Inventory, Profiles, Reviews, Wishlists, Messaging). Full DDD building block foundation adopted across all modules. Ready for gradual service extraction.

**Known tech debt:**
- Hardcoded configuration (stock thresholds, shipping rates, CORS origins)
- Placeholder images (placehold.co URLs)
- Cross-context DbContext injection in Reviews and Wishlists modules (bounded context isolation violation)
- ProductStatus missing `[JsonConverter]` attribute (server-side works, latent client-side gap)
- DomainEventInterceptor uses scoped DI (not wired via AddInterceptors — needs IPublishEndpoint)
- ISoftDeletable has zero entity adopters (infrastructure ready, no entities implement it yet)

## Constraints

- **Tech Stack**: .NET 10 + Aspire for backend, Next.js 16 for frontend — committed
- **Identity**: Keycloak — integrated, don't duplicate auth
- **Message Broker**: Azure Service Bus emulator — Aspire ecosystem alignment
- **Database**: PostgreSQL — in deployment manifests
- **Deployment**: Kubernetes with FluxCD

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Gradual service extraction | Real-world pattern, demonstrates monolith-to-microservices journey | ✓ Good — clean module boundaries established |
| Database-per-feature | True microservices isolation, showcases data ownership | ✓ Good — 8 separate DbContexts working well |
| Guest checkout | Lower friction, simpler v1 scope | ✓ Good — cookie-based buyer identity works seamlessly |
| Mock payments | Avoid payment processor complexity while demonstrating flow | ✓ Good — toggleable success/failure |
| Cart in Postgres (not Redis) | Durability over speed, consistent with database-per-service | ✓ Good — reliable with TTL cleanup |
| Admin in main app | Simpler deployment, shared auth context | ✓ Good — route-based separation clean |
| shadcn/ui | Modern, Tailwind-based, great DX | ✓ Good — consistent design across storefront and admin |
| MassTransit + transactional outbox | Reliable event delivery with at-least-once guarantees | ✓ Good — checkout saga compensation works |
| Checkout saga (MassTransit state machine) | Orchestrate multi-step checkout with compensation | ✓ Good — handles stock reservation, payment, cart clearing |
| YARP API Gateway | Unified entry point, security centralization | ✓ Good — JWT + rate limiting at edge |
| Testcontainers for integration tests | Real database testing without Docker Compose | ✓ Good — catches real SQL/EF issues |
| ImageSharp for avatar processing | Crop-to-square and resize to 400x400 | ✓ Good — no external service needed |
| Auto-create profile on first GET | Profile always exists for authenticated users | ✓ Good — eliminates null profile edge cases |
| Silent cart merge on login | Guest cart transfers to authenticated user without intervention | ✓ Good — seamless UX |
| Verified purchase reviews | Reviews require order with Paid/Confirmed/Shipped/Delivered status | ✓ Good — trust through enforcement |
| Denormalized review stats | AverageRating and ReviewCount on Product entity | ✓ Good — fast queries, synchronous recalculation |
| Optimistic wishlist UI | Heart icon toggles immediately with cache rollback on error | ✓ Good — instant feedback |
| Readonly record structs for value objects | Compiler-generated equality, 20x faster than reflection-based ValueObject | ✓ Good — modern C# semantics |
| ComplexProperty for struct value objects | Avoids EF Core shadow primary key generation from OwnsOne | ✓ Good — cleaner mapping |
| Vernon's aggregate rules as audit standard | 4 rules for aggregate design validation | ✓ Good — caught 21 critical violations |
| Reference by identity only (Vernon Rule 3) | Product references Category by CategoryId, not navigation property | ✓ Good — proper aggregate isolation |
| Vogen for StronglyTypedId | Source-generated record structs vs hand-rolled base class | ✓ Good — zero runtime overhead, auto converters |
| SmartEnum for status types | Behavior-rich enumerations vs plain enum + switch | ✓ Good — state transitions encapsulated, no magic strings |
| FluentResults for explicit errors | Result<T> vs exceptions for domain errors | ✓ Good — 4 handlers adopted, HTTP mapping clean |
| Ardalis.Specification for queries | Composable specs vs inline LINQ | ✓ Good — 8 specs, chained composition works |
| Stateless interceptors as singletons | new() instances vs DI-resolved scoped | ✓ Good — no constructor deps, simpler wiring |
| Interceptor order: SoftDelete → Concurrency → Audit | SoftDelete before Audit so UpdatedAt set on soft-deleted entities | ✓ Good — correct behavioral ordering |
| Testcontainers over in-memory DB | Real PostgreSQL vs EF InMemory provider | ✓ Good — catches real SQL/EF issues, 38 integration tests |
| Order skips AuditableAggregateRoot | Domain-specific CreatedAt/PaidAt semantics | ✓ Good — intentional, documented design choice |

---
*Last updated: 2026-02-25 after v3.0 milestone started*

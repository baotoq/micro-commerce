# MicroCommerce

## What This Is

A showcase e-commerce platform demonstrating modern .NET microservices architecture with best practices. Users can browse products, manage profiles, write verified reviews, save wishlists, add items to cart, and complete purchases through a polished Next.js storefront backed by a modular monolith with event-driven architecture, CQRS, and DDD tactical patterns.

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

### Active

## Current Milestone: v2.0 DDD Foundation

**Goal:** Strengthen and modernize DDD building blocks with full adoption across all features

**Target features:**
- Extract common patterns (Entity base, audit fields, concurrency) into BuildingBlocks
- Add new DDD building blocks (Result type, Enumeration class, Specification pattern)
- Clean up and modernize (remove obsolete ValueObject, improve StronglyTypedId with converters, add source generators)
- Adopt all new building blocks across every existing feature

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

## Context

**Shipped v1.1 User Features** with ~585K LOC across .NET 10 backend and Next.js 16 frontend.

**Tech stack:**
- Backend: .NET 10, ASP.NET Core Minimal APIs, .NET Aspire 13.1.0
- Frontend: Next.js 16, React 19, TypeScript 5, shadcn/ui
- Auth: Keycloak (backend JWT + NextAuth.js frontend)
- Database: PostgreSQL (per-feature isolation, 8 DbContexts)
- Messaging: Azure Service Bus (MassTransit) with emulator
- Gateway: YARP reverse proxy with JWT, rate limiting, CORS
- Patterns: CQRS (MediatR), DDD, Event-Driven, Vertical Slice Architecture
- Testing: xUnit, FluentAssertions, Testcontainers, Playwright

**Architecture:** Modular monolith with clear bounded contexts (Catalog, Cart, Ordering, Inventory, Profiles, Reviews, Wishlists, Messaging). Ready for gradual service extraction.

**Known tech debt:**
- Hardcoded configuration (stock thresholds, shipping rates, CORS origins)
- Placeholder images (placehold.co URLs)
- Cross-context DbContext injection in Reviews and Wishlists modules (bounded context isolation violation)
- Strongly-typed IDs inconsistently applied at module boundaries (primitive Guid used cross-context)
- ValueObject base class deprecated but not removed (zero consumers, marked [Obsolete])

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

---
*Last updated: 2026-02-14 after v2.0 milestone started*

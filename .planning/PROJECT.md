# MicroCommerce

## What This Is

A showcase e-commerce platform demonstrating modern .NET microservices architecture with best practices. Users can browse products, add items to cart, and complete purchases through a polished Next.js storefront backed by a modular monolith with event-driven architecture, CQRS, and full admin management.

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

### Active

#### Current Milestone: v1.1 User Features

**Goal:** Add authenticated user experiences — profiles, verified purchase reviews, and wishlists — building on the existing Keycloak auth foundation.

**Target features:**
- User accounts & profiles (display name, avatar, saved addresses, linked orders/cart/history)
- Product reviews & ratings (verified purchase only, star ratings + text)
- Wishlists (single list per user, add/remove, move to cart)

### Out of Scope

- Real payment processing (Stripe, etc.) — mock payments sufficient for demo
- Event sourcing — adds complexity without proportional demo value
- Separate admin application — integrated admin routes simpler
- Mobile app — web-first, responsive design covers mobile
- Multi-tenancy — single store demonstration
- Internationalization — English only for now

## Context

**Shipped v1.0 MVP** with 94,355 LOC across .NET 10 backend and Next.js 16 frontend.

**Tech stack:**
- Backend: .NET 10, ASP.NET Core Minimal APIs, .NET Aspire 13.1.0
- Frontend: Next.js 16, React 19, TypeScript 5, shadcn/ui
- Auth: Keycloak (backend JWT + NextAuth.js frontend)
- Database: PostgreSQL (per-feature isolation, 5 DbContexts)
- Messaging: Azure Service Bus (MassTransit) with emulator
- Gateway: YARP reverse proxy with JWT, rate limiting, CORS
- Patterns: CQRS (MediatR), DDD, Event-Driven, Vertical Slice Architecture
- Testing: xUnit, FluentAssertions, Testcontainers, Playwright

**Architecture:** Modular monolith with clear bounded contexts (Catalog, Cart, Ordering, Inventory, Messaging). Ready for gradual service extraction.

**Known tech debt (from v1.0 audit):**
- Hardcoded configuration (stock thresholds, shipping rates, CORS origins)
- Placeholder images (placehold.co URLs)
- Some missing SUMMARY.md and VERIFICATION.md files in planning docs

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
| Database-per-feature | True microservices isolation, showcases data ownership | ✓ Good — 5 separate DbContexts working well |
| Guest checkout | Lower friction, simpler v1 scope | ✓ Good — cookie-based buyer identity works seamlessly |
| Mock payments | Avoid payment processor complexity while demonstrating flow | ✓ Good — toggleable success/failure |
| Cart in Postgres (not Redis) | Durability over speed, consistent with database-per-service | ✓ Good — reliable with TTL cleanup |
| Admin in main app | Simpler deployment, shared auth context | ✓ Good — route-based separation clean |
| shadcn/ui | Modern, Tailwind-based, great DX | ✓ Good — consistent design across storefront and admin |
| MassTransit + transactional outbox | Reliable event delivery with at-least-once guarantees | ✓ Good — checkout saga compensation works |
| Checkout saga (MassTransit state machine) | Orchestrate multi-step checkout with compensation | ✓ Good — handles stock reservation, payment, cart clearing |
| YARP API Gateway | Unified entry point, security centralization | ✓ Good — JWT + rate limiting at edge |
| Testcontainers for integration tests | Real database testing without Docker Compose | ✓ Good — catches real SQL/EF issues |

---
*Last updated: 2026-02-13 after v1.1 milestone start*

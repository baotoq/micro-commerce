# MicroCommerce

## What This Is

A showcase e-commerce platform demonstrating modern .NET microservices architecture with best practices. Users can browse products, add items to cart, and complete purchases through a polished Next.js storefront. The project serves as a reference implementation for cloud-native patterns using .NET Aspire, event-driven architecture, and gradual service extraction.

## Core Value

**A user can complete a purchase end-to-end** — browse products, add to cart, checkout as a guest, and see their order confirmation. Everything else supports this flow.

## Requirements

### Validated

- ✓ Keycloak authentication integrated — existing
- ✓ Next.js frontend with NextAuth — existing
- ✓ .NET Aspire orchestration — existing
- ✓ DDD building blocks (aggregates, domain events, value objects) — existing
- ✓ OpenTelemetry observability — existing

### Active

- [ ] Product catalog with categories and search
- [ ] Product detail pages
- [ ] Shopping cart (persistent, guest-friendly)
- [ ] Checkout flow with mock payment
- [ ] Order confirmation and history
- [ ] Inventory tracking with stock levels
- [ ] Admin UI for product management
- [ ] Seed data for initial products
- [ ] API Gateway for service routing
- [ ] Event-driven communication between services
- [ ] CQRS pattern with MediatR
- [ ] Database-per-service isolation
- [ ] Unit and integration tests
- [ ] shadcn/ui component library integration

### Out of Scope

- Real payment processing (Stripe, etc.) — mock payments sufficient for demo
- Event sourcing — adds complexity without proportional demo value
- Separate admin application — integrated admin routes simpler
- Mobile app — web-first, responsive design covers mobile
- Real-time features (chat, live inventory) — not core to e-commerce demo
- Multi-tenancy — single store demonstration
- Internationalization — English only for v1

## Context

**Existing Foundation:**
- .NET 10 with Aspire 13.1.0 for orchestration
- Next.js 16 with React 19 frontend
- Keycloak for identity (realm: micro-commerce)
- MediatR for domain events and CQRS
- OpenTelemetry instrumentation configured
- Kubernetes manifests with Kustomize overlays
- Dev container with full toolchain

**Architecture Approach:**
- Start as modular monolith, gradually extract to microservices
- Target services: Catalog, Cart, Ordering, Inventory
- Azure Service Bus emulator for inter-service messaging
- Each service owns its Postgres database
- API Gateway routes frontend requests to services

**Frontend Approach:**
- shadcn/ui component library for polished UI
- Admin routes integrated in main Next.js app
- Guest checkout supported (no account required)

## Constraints

- **Tech Stack**: .NET 10 + Aspire for backend, Next.js 16 for frontend — already committed
- **Identity**: Keycloak — already integrated, don't duplicate auth
- **Message Broker**: Azure Service Bus emulator — Aspire ecosystem alignment
- **Database**: Postgres — already in deployment manifests
- **Deployment**: Kubernetes with FluxCD

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Gradual service extraction | Real-world pattern, demonstrates monolith-to-microservices journey | — Pending |
| Database-per-service | True microservices isolation, showcases data ownership | — Pending |
| Guest checkout | Lower friction, simpler v1 scope | — Pending |
| Mock payments | Avoid payment processor complexity while demonstrating flow | — Pending |
| Cart in Postgres (not Redis) | Durability over speed, consistent with database-per-service | — Pending |
| Admin in main app | Simpler deployment, shared auth context | — Pending |
| shadcn/ui | Modern, Tailwind-based, great DX | — Pending |

---
*Last updated: 2025-01-29 after initialization*

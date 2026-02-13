# Roadmap

**Project:** MicroCommerce
**Depth:** Comprehensive (8-12 phases)
**Approach:** Gradual extraction (modular monolith first, then split services)
**Created:** 2026-01-29

## Overview

This roadmap implements a complete e-commerce platform through 10 phases, following the gradual extraction pattern recommended by research. Each phase builds on the previous, establishing clear bounded contexts before any service extraction.

**Total Requirements:** 24
**Phases:** 10
**Estimated Duration:** Comprehensive implementation

---

## Phase 1: Foundation & Project Structure

**Goal:** Establish modular monolith structure with clear bounded contexts, shared building blocks, and development patterns. Service Bus and transactional outbox from day one per user decision.

**Requirements:**
- None directly (infrastructure phase)

**Deliverables:**
- Modular monolith structure with Features/ folders (Catalog, Cart, Ordering, Inventory)
- Database-per-service pattern with separate DbContexts
- MediatR pipeline with validation behaviors
- FluentValidation integration
- Domain event infrastructure (Service Bus + transactional outbox)
- CQRS usage guidelines

**Success Criteria:**
1. Developer can create a new feature module by copying existing template
2. Each module has isolated DbContext with independent migrations
3. MediatR pipeline validates requests before handlers execute
4. Domain events fire via Service Bus with transactional outbox

**Plans:** 6 plans ✓ COMPLETE

Plans:
- [x] 01-01-PLAN.md — Infrastructure setup (NuGet packages, Aspire resources) ✓
- [x] 01-02-PLAN.md — Module structure & DbContexts ✓
- [x] 01-03-PLAN.md — MediatR pipeline & validation ✓
- [x] 01-04-PLAN.md — Domain event infrastructure (MassTransit + outbox) ✓
- [x] 01-05-PLAN.md — CQRS reference implementation ✓
- [x] 01-06-PLAN.md — UAT gap fixes (EF migration + exception handler) ✓

**Addresses Pitfalls:**
- Premature service extraction (bounded contexts defined first)
- CQRS overuse (guidelines established)
- Database ownership confusion (separate DbContexts)

---

## Phase 2: Catalog Domain & Admin CRUD

**Goal:** Build product catalog domain with admin management capabilities.

**Requirements:**
- **ADM-01**: Admin can create, edit, and delete products

**Deliverables:**
- Product aggregate (name, description, price, image, category)
- Category aggregate
- Admin product CRUD endpoints
- Admin product management UI
- Product domain events (ProductCreated, ProductUpdated, ProductDeleted)

**Success Criteria:**
1. Admin can create a product with image, price, and category
2. Admin can edit product details and see changes reflected
3. Admin can delete a product (soft delete)
4. Product changes publish domain events

**Plans:** 7 plans ✓ COMPLETE

Plans:
- [x] 02-01-PLAN.md — Product Domain Model & CQRS Stack ✓
- [x] 02-02-PLAN.md — Product CRUD Commands & Queries ✓
- [x] 02-03-PLAN.md — Azure Blob Storage Image Upload ✓
- [x] 02-04-PLAN.md — Category CRUD Completion ✓
- [x] 02-05-PLAN.md — Admin UI - Product List & Table ✓
- [x] 02-06-PLAN.md — Admin UI - Product Drawer (Create/Edit) ✓
- [x] 02-07-PLAN.md — Admin UI - Categories Page ✓

**Dependencies:** Phase 1

---

## Phase 3: Catalog Storefront & Seed Data

**Goal:** Build customer-facing product browsing experience with initial data.

**Requirements:**
- **CAT-01**: User can browse products in a grid view with image, name, price
- **CAT-02**: User can view product detail page with full info and add-to-cart
- **CAT-03**: User can filter products by category
- **CAT-04**: User can search products by name/description
- **INFRA-01**: System has seed data with sample products

**Deliverables:**
- Product listing page with grid layout
- Product detail page
- Category filter component
- Basic search functionality (name/description)
- Seed data script with 20+ sample products across categories
- shadcn/ui components for product cards

**Success Criteria:**
1. User sees product grid on homepage with images, names, and prices
2. User clicks product and sees detail page with full description
3. User filters by category and sees only matching products
4. User searches "laptop" and sees relevant products
5. Fresh database has sample products after seed runs

**Plans:** 6 plans

Plans:
- [ ] 03-01-PLAN.md — Backend sort support & seed data (~50 products)
- [ ] 03-02-PLAN.md — Storefront layout, header, footer & hero banner
- [ ] 03-03-PLAN.md — Product grid with cards & infinite scroll
- [ ] 03-04-PLAN.md — Product detail page & related products
- [ ] 03-05-PLAN.md — Search, category filters & sort with URL state
- [ ] 03-06-PLAN.md — End-to-end verification checkpoint

**Dependencies:** Phase 2

---

## Phase 4: Inventory Domain

**Goal:** Build inventory tracking with stock management and reservation pattern.

**Requirements:**
- **INV-01**: System tracks stock levels per product
- **INV-02**: System reserves stock during checkout (prevents overselling)
- **ADM-02**: Admin can adjust inventory stock levels

**Deliverables:**
- StockItem aggregate with quantity tracking
- Reservation pattern with TTL
- Admin inventory adjustment UI
- Stock level display on product pages
- Optimistic concurrency for stock updates
- Inventory domain events (StockReserved, StockReleased, StockAdjusted)

**Success Criteria:**
1. Admin can set stock quantity for any product
2. Product page shows "In Stock" or "Out of Stock" badge
3. Reservation reduces available quantity temporarily
4. Expired reservations auto-release (TTL)
5. Concurrent stock updates don't corrupt data (optimistic concurrency)

**Plans:** 5 plans ✓ COMPLETE

Plans:
- [x] 04-01-PLAN.md — Domain model, EF configurations & migration ✓
- [x] 04-02-PLAN.md — CQRS commands, queries, endpoints & ProductCreated consumer ✓
- [x] 04-03-PLAN.md — Reservation cleanup service & stock data seeder ✓
- [x] 04-04-PLAN.md — Admin stock management UI (adjust, history) ✓
- [x] 04-05-PLAN.md — Storefront stock display (cards & detail page) ✓

**Addresses Pitfalls:**
- Inventory overselling (reservation pattern)

**Dependencies:** Phase 3

---

## Phase 5: Event Bus Infrastructure

**Goal:** Add idempotent consumers, dead-letter queue configuration, and correlation tracking. (Core Service Bus + outbox already established in Phase 1)

**Requirements:**
- **INFRA-03**: Services communicate via Azure Service Bus events

**Deliverables:**
- Idempotent event consumers
- Dead-letter queue configuration
- Correlation ID tracking
- Consumer error handling patterns

**Success Criteria:**
1. Domain event published in transaction reaches consumer reliably
2. Duplicate messages don't cause duplicate side effects (idempotency)
3. Failed messages land in DLQ with correlation for debugging
4. Events include correlation ID for end-to-end tracing

**Plans:** 3 plans ✓ COMPLETE

Plans:
- [x] 05-01-PLAN.md — Global consumer middleware (idempotency, retry, circuit breaker, tracing) ✓
- [x] 05-02-PLAN.md — DLQ service, CQRS handlers, API endpoints & fault consumer ✓
- [x] 05-03-PLAN.md — Admin dead-letter queue UI page ✓

**Addresses Pitfalls:**
- Event-driven eventually-never consistency (outbox pattern)

**Dependencies:** Phase 4

---

## Phase 6: Cart Domain

**Goal:** Build shopping cart with guest support, persistence, and optimistic UI.

**Requirements:**
- **CART-01**: User can view cart, update quantities, and remove items
- **CART-02**: User's cart persists across page refreshes (database-backed)
- **CART-03**: User sees feedback when adding item to cart (toast/badge)
- **CART-04**: Cart updates feel instant (optimistic UI)

**Deliverables:**
- Cart aggregate with CartItem value objects
- Guest cart support (cookie-based buyer ID)
- Cart PostgreSQL persistence
- Add/update/remove endpoints
- Cart merge on login (guest + authenticated)
- Cart expiration job (30-day TTL)
- Optimistic UI with React Query mutations
- Toast notifications for cart actions
- Cart badge in header showing item count

**Success Criteria:**
1. Guest user adds item to cart, refreshes page, cart persists
2. User updates quantity, UI updates instantly before server confirms
3. Toast appears "Added to cart" when item added
4. Header badge shows correct item count
5. User logs in, anonymous cart merges with existing cart

**Plans:** 4 plans

Plans:
- [ ] 06-01-PLAN.md — Cart domain model, EF config, migration & buyer identity
- [ ] 06-02-PLAN.md — Cart CQRS handlers, API endpoints & expiration service
- [ ] 06-03-PLAN.md — React Query setup, cart API functions & hooks
- [ ] 06-04-PLAN.md — Cart page UI, header badge & add-to-cart integration

**Addresses Pitfalls:**
- Cart/checkout race conditions (idempotency, concurrency)

**Dependencies:** Phase 5

---

## Phase 7: Ordering Domain & Checkout

**Goal:** Build order creation with checkout flow and guest checkout support.

**Requirements:**
- **CHK-01**: User can complete checkout flow (shipping info, payment, confirmation)
- **CHK-02**: User can checkout as guest without creating account
- **CHK-03**: User sees mock payment that simulates success/failure
- **CHK-04**: User sees order confirmation with summary after purchase
- **INV-03**: Stock counts update in real-time when orders placed

**Deliverables:**
- Order aggregate with OrderItem value objects
- Checkout multi-step UI (shipping, payment, review)
- Guest checkout (email-based identification)
- Mock payment service (configurable success/failure)
- Order confirmation page
- Order status state machine (Submitted -> Confirmed -> Paid)
- Checkout saga with MassTransit
- Compensation handlers for saga rollback
- Real-time stock update via events

**Success Criteria:**
1. User completes checkout: enters shipping, reviews, pays, sees confirmation
2. Guest (not logged in) can complete full checkout with email
3. Mock payment shows success/failure states appropriately
4. Confirmation page shows order number, items, total, shipping address
5. Stock quantity decreases after successful order

**Plans:** 4 plans

Plans:
- [ ] 07-01-PLAN.md — Order domain model, value objects, EF configs & migration
- [ ] 07-02-PLAN.md — Order CQRS commands/queries & checkout API endpoints
- [ ] 07-03-PLAN.md — Checkout saga state machine & integration consumers
- [ ] 07-04-PLAN.md — Checkout UI (accordion, shipping, payment), hooks & order confirmation page

**Addresses Pitfalls:**
- Saga without compensation (compensation handlers)

**Dependencies:** Phase 6

---

## Phase 8: Order History & Management

**Goal:** Build order history for customers and order management for admins.

**Requirements:**
- **ORD-01**: Logged-in user can view their order history
- **ORD-02**: User sees real-time order status updates
- **ORD-03**: User can view order detail page
- **ADM-03**: Admin sees dashboard with order counts and revenue
- **ADM-04**: Admin can view and manage orders

**Deliverables:**
- Order history page (authenticated users)
- Order detail page
- Real-time status updates (polling or SignalR)
- Admin dashboard with metrics
- Admin order list with filters
- Admin order detail with status management

**Success Criteria:**
1. Logged-in user sees list of past orders with status
2. Order detail page shows all items, shipping, payment info
3. User sees status change within seconds of update
4. Admin dashboard shows today's orders, total revenue
5. Admin can update order status (e.g., mark shipped)

**Plans:** 5 plans ✓ COMPLETE

Plans:
- [x] 08-01-PLAN.md — Backend: Order domain extension (Shipped/Delivered), new queries, endpoints ✓
- [x] 08-02-PLAN.md — Frontend: Dependencies, QueryProvider, API functions, React Query hooks ✓
- [x] 08-03-PLAN.md — Storefront: Order history page (auth-gated) & order detail with status stepper ✓
- [x] 08-04-PLAN.md — Admin: Dashboard with stat cards, bar chart & recent orders table ✓
- [x] 08-05-PLAN.md — Admin: Kanban board with drag-and-drop & order detail page ✓

**Dependencies:** Phase 7

---

## Phase 9: API Gateway

**Goal:** Add YARP-based API Gateway for unified service routing.

**Requirements:**
- **INFRA-02**: API Gateway (YARP) routes frontend requests to services

**Deliverables:**
- YARP API Gateway service
- Route configuration for all modules
- JWT validation at gateway level
- Aspire service discovery integration
- Rate limiting configuration
- Request logging

**Success Criteria:**
1. Frontend requests route through gateway to appropriate service
2. Invalid JWT rejected at gateway before reaching service
3. Gateway appears in Aspire dashboard with health status
4. Rate-limited client receives 429 response

**Plans:** 3 plans ✓ COMPLETE

Plans:
- [x] 09-01-PLAN.md — Gateway project with YARP reverse proxy & Aspire registration ✓
- [x] 09-02-PLAN.md — JWT auth, rate limiting, CORS centralization & request transforms ✓
- [x] 09-03-PLAN.md — Frontend migration to route through gateway ✓

**Dependencies:** Phase 8

---

## Phase 10: Testing & Polish

**Goal:** Comprehensive testing and production readiness.

**Requirements:**
- **INFRA-04**: Unit and integration tests cover critical paths

**Deliverables:**
- Unit tests for domain logic (aggregates, value objects)
- Integration tests for API endpoints
- Integration tests for checkout saga
- E2E test for critical path (browse -> cart -> checkout)
- Performance baseline tests
- Error handling improvements
- Loading states and empty states polish

**Success Criteria:**
1. Domain logic has >80% unit test coverage
2. All API endpoints have integration tests
3. Checkout saga has tests for success and failure paths
4. E2E test passes for happy path purchase flow
5. App handles errors gracefully with user-friendly messages

**Plans:** 6 plans ✓ COMPLETE

Plans:
- [x] 10-01-PLAN.md — Test project setup & Ordering domain unit tests ✓
- [x] 10-02-PLAN.md — Catalog, Cart, Inventory unit tests & validator tests ✓
- [x] 10-03-PLAN.md — Checkout saga state machine tests ✓
- [x] 10-04-PLAN.md — Integration tests with Testcontainers PostgreSQL ✓
- [x] 10-05-PLAN.md — Playwright E2E tests for browsing & cart ✓
- [x] 10-06-PLAN.md — UX polish (error handling, loading, empty states) & final verification ✓

**Dependencies:** Phase 9

---

## Requirement Traceability Matrix

| REQ-ID | Requirement | Phase |
|--------|-------------|-------|
| CAT-01 | Browse products grid | Phase 3 |
| CAT-02 | Product detail page | Phase 3 |
| CAT-03 | Filter by category | Phase 3 |
| CAT-04 | Search products | Phase 3 |
| CART-01 | View/update/remove cart | Phase 6 |
| CART-02 | Cart persistence | Phase 6 |
| CART-03 | Add-to-cart feedback | Phase 6 |
| CART-04 | Optimistic UI | Phase 6 |
| CHK-01 | Checkout flow | Phase 7 |
| CHK-02 | Guest checkout | Phase 7 |
| CHK-03 | Mock payment | Phase 7 |
| CHK-04 | Order confirmation | Phase 7 |
| ORD-01 | Order history | Phase 8 |
| ORD-02 | Real-time status | Phase 8 |
| ORD-03 | Order detail page | Phase 8 |
| INV-01 | Stock tracking | Phase 4 |
| INV-02 | Stock reservation | Phase 4 |
| INV-03 | Real-time stock updates | Phase 7 |
| ADM-01 | Product CRUD | Phase 2 |
| ADM-02 | Inventory adjustment | Phase 4 |
| ADM-03 | Admin dashboard | Phase 8 |
| ADM-04 | Order management | Phase 8 |
| INFRA-01 | Seed data | Phase 3 |
| INFRA-02 | API Gateway | Phase 9 |
| INFRA-03 | Event bus | Phase 5 |
| INFRA-04 | Tests | Phase 10 |

**Coverage:** 24/24 requirements mapped (100%)

---

## Phase Dependencies Graph

```
Phase 1 (Foundation)
    │
    v
Phase 2 (Catalog Admin)
    │
    v
Phase 3 (Catalog Storefront + Seed)
    │
    v
Phase 4 (Inventory)
    │
    v
Phase 5 (Event Bus)
    │
    v
Phase 6 (Cart)
    │
    v
Phase 7 (Ordering + Checkout)
    │
    v
Phase 8 (Order History + Admin)
    │
    v
Phase 9 (API Gateway)
    │
    v
Phase 10 (Testing + Polish)
```

---

## Risk Mitigation

| Risk | Mitigation | Phase |
|------|------------|-------|
| Premature service extraction | Modular monolith with bounded contexts first | Phase 1 |
| CQRS overuse | Guidelines on when to use CQRS | Phase 1 |
| Inventory overselling | Reservation pattern with TTL | Phase 4 |
| Event delivery failures | Transactional outbox pattern | Phase 1 |
| Cart race conditions | Optimistic concurrency, idempotency | Phase 6 |
| Saga failures | Compensation handlers from start | Phase 7 |

---

## Future Phases (Post-v1)

After v1 completion, consider:
- **Phase 11: Service Extraction** - Extract to true microservices
- **Phase 12: Advanced Search** - Elasticsearch integration
- **Phase 13: Real Payments** - Stripe integration

---
*Roadmap created: 2026-01-29*
*Total phases: 10*
*Total requirements: 24 (100% coverage)*

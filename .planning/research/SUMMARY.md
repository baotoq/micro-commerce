# Project Research Summary

**Project:** MicroCommerce
**Domain:** E-commerce Microservices (Showcase Platform)
**Researched:** 2026-01-29
**Confidence:** HIGH

## Executive Summary

MicroCommerce is an e-commerce showcase platform built to demonstrate modern microservices architecture patterns using .NET 10 and Aspire 13.1.0. Research reveals that successful e-commerce microservices require careful attention to service boundaries, event-driven communication, and eventual consistency patterns. The recommended approach is to start with a modular monolith, establish clear bounded contexts through vertical slices, and extract services only when scaling or deployment independence is needed.

The core technical stack centers on MassTransit for messaging (with Azure Service Bus), MediatR for in-process CQRS, PostgreSQL with database-per-service, and YARP for API Gateway. These technologies are industry-standard for .NET microservices and integrate seamlessly with the existing Aspire foundation. The architecture should support gradual extraction: starting with Catalog (lowest coupling), then Inventory (event-driven), Cart (session-based), and finally Ordering (saga orchestration).

The most critical risk is premature service extraction before domain boundaries are understood. Other significant pitfalls include race conditions in cart/checkout, inventory overselling without reservations, and event-driven consistency failures. All of these can be mitigated through proper patterns (outbox, idempotency, saga compensation) implemented from the start rather than retrofitted.

## Key Findings

### Recommended Stack

The research identifies a mature, production-proven stack that integrates well with the existing .NET 10 + Aspire 13.1.0 foundation. The focus is on avoiding vendor lock-in while leveraging Azure services for infrastructure.

**Core technologies:**
- **MassTransit 9.0.0**: Message bus abstraction for service communication — provides saga orchestration, transactional outbox pattern, and transport independence (Azure Service Bus in production, RabbitMQ for local dev)
- **MediatR 14.0.0**: In-process mediator for CQRS pattern — separates command/query handling, enables cross-cutting concerns via pipeline behaviors (validation, logging, transactions)
- **YARP 2.3.0**: API Gateway for request routing — Microsoft-backed reverse proxy with native Aspire service discovery integration, replaces deprecated Ocelot
- **PostgreSQL (Npgsql.EFCore 10.0.0)**: Database-per-service with strong consistency — mature provider, supports complex queries, integrates with Aspire health checks
- **Ardalis.Result 10.1.0**: Result pattern for explicit error handling — railway-oriented programming without exception-based control flow
- **FluentValidation 12.1.1**: Request validation in MediatR pipeline — declarative, testable validation rules

**Supporting libraries:**
- Scrutor 7.0.0 for convention-based handler registration
- Mapster 7.4.0 for lightweight DTO mapping
- Aspire.Azure.Messaging.ServiceBus 13.1.0 for native Service Bus integration

**Avoided technologies:**
- Ocelot (maintenance mode, YARP is successor)
- AutoMapper (performance overhead, prefer Mapster or manual mapping)
- NServiceBus (commercial license required)
- Raw Azure SDK for messaging (loses saga/outbox abstractions)

### Expected Features

Research into e-commerce UX patterns reveals clear table stakes vs. differentiators for a showcase platform.

**Must have (table stakes):**
- Product catalog browsing with grid view and detail pages — users won't buy what they can't see
- Shopping cart with CRUD operations and persistence — core purchase flow requirement
- Category filtering and basic search — users expect to find products
- Guest checkout without forced registration — many users won't create accounts for demos
- Order confirmation page — users need immediate verification
- Basic inventory tracking — prevents overselling
- Admin product CRUD — demonstrates full-stack competency
- Responsive design with loading states — professional polish

**Should have (competitive advantage for showcase):**
- Real-time inventory updates via events — demonstrates event-driven architecture
- Order history for logged-in users — shows auth integration and data isolation
- Optimistic UI updates — modern UX pattern (cart feels instant)
- Admin inventory management — business logic beyond CRUD
- Empty states and micro-interactions — professional UX details
- Product search with filters — demonstrates query complexity handling

**Defer (v2+):**
- User reviews/ratings — requires moderation, adds little demo value
- Wishlist/favorites — distracts from core flow
- Email notifications — infrastructure complexity not worth demo value
- Coupon/discount codes — pricing logic complexity
- Product variants (size/color) — significant data model complexity
- Recommendations engine — ML complexity for showcase

**Anti-features (deliberately NOT building):**
- Real payment processing — compliance burden, mock payment sufficient
- Multi-currency — exchange rate complexity
- Saved payment methods — PCI compliance concerns
- Advanced analytics — dashboard complexity
- Multi-language i18n — translation management overhead

### Architecture Approach

The recommended architecture follows Microsoft's eShop reference patterns: modular monolith foundation with clear service boundaries, event-driven communication via Azure Service Bus, and database-per-service from the start (even while running as a monolith). This supports gradual extraction when needed.

**Major components:**

1. **Catalog Service** — Product management and search
   - Read-heavy, cache-friendly
   - Publishes: `ProductCreated`, `ProductUpdated`, `PriceChanged`
   - No upstream dependencies (foundation service)
   - Database: PostgreSQL with full-text search capability

2. **Cart Service** — Shopping cart state management
   - Session-based with guest support
   - Publishes: `CartCheckedOut`
   - Subscribes: `ProductUpdated` (to refresh cached product info)
   - Database: PostgreSQL with cart expiration TTL

3. **Ordering Service** — Order lifecycle and saga orchestration
   - CQRS with saga pattern for checkout flow
   - Publishes: `OrderSubmitted`, `OrderStockConfirmed`, `OrderPaid`
   - Subscribes: `StockConfirmed`, `StockRejected`, `PaymentSucceeded`
   - Database: PostgreSQL with saga state tables

4. **Inventory Service** — Stock management with consistency guarantees
   - Event-driven with reservation pattern
   - Publishes: `StockConfirmed`, `StockRejected`, `LowStockWarning`
   - Subscribes: `OrderSubmitted`, `OrderCancelled`, `OrderPaid`
   - Database: PostgreSQL with optimistic concurrency

5. **API Gateway** — Request routing and composition
   - YARP-based with Aspire service discovery
   - Routes to services, validates auth, composes responses
   - No business logic (thin gateway)

6. **Event Bus** — Async service communication
   - Azure Service Bus with topics/subscriptions
   - Transactional outbox pattern for guaranteed delivery
   - Idempotent consumers with deduplication

**Data flow patterns:**
- Synchronous: Frontend → Gateway → Service → Database (for queries)
- Asynchronous: Service → Outbox → Service Bus → Consumer (for events)
- Saga choreography: Each service publishes events, others react independently
- Data denormalization: Services cache snapshot data from events (e.g., Cart stores product name/price at add-time)

**Extraction order:**
1. Start: Modular monolith (single API, separate databases)
2. Extract: Catalog Service (lowest coupling, read-heavy)
3. Extract: Inventory Service (event-driven, consistency-critical)
4. Extract: Cart Service (session data, isolated concern)
5. Extract: Ordering Service (saga orchestration, most complex)

### Critical Pitfalls

Research into microservices failures reveals recurring patterns that derail projects.

1. **Premature Service Extraction** — Teams extract services before understanding domain boundaries, resulting in chatty communication and inability to deploy independently
   - **How to avoid:** Start modular monolith, track inter-module communication for 2-4 sprints, enforce communication only through interfaces
   - **Warning signs:** Services call each other synchronously for every request, "we need to deploy these 3 services together"
   - **Address in:** Foundation phase before any extraction

2. **Cart/Checkout Race Conditions** — User clicks "Place Order" twice, gets charged twice or inventory goes negative
   - **How to avoid:** Implement idempotency keys on checkout, optimistic concurrency on cart aggregate, frontend submit guards
   - **Warning signs:** QA can create duplicate orders by rapid clicking, inventory occasionally goes negative
   - **Address in:** Cart Service phase with concurrency controls from day one

3. **Inventory Overselling** — 100 users buy the last 10 items simultaneously without proper reservation
   - **How to avoid:** Implement reservation pattern with TTL, pessimistic locking at checkout, accept graceful overselling for low-value items
   - **Warning signs:** Negative inventory counts, flash sales consistently oversell, cancellations after payment
   - **Address in:** Inventory Service phase with reservation pattern as architectural concern

4. **Event-Driven Eventually-Never Consistency** — Events published but consumers fail silently, leading to data inconsistencies
   - **How to avoid:** Outbox pattern (events in same transaction as data), idempotent consumers, dead-letter queue monitoring, correlation tracking
   - **Warning signs:** DLQ has old messages, no alerts on consumer failures, inventory doesn't match orders
   - **Address in:** Messaging Infrastructure phase before any event publishing

5. **Database-Per-Service Without Data Ownership Clarity** — Services implement separate databases but still need cross-service queries
   - **How to avoid:** Define data ownership first, CQRS for reads, API composition for complex queries, event-carried state transfer
   - **Warning signs:** Services make sync calls to get data, circular dependencies, admin queries bypass services
   - **Address in:** Foundation phase with data ownership mapping before any service has database

6. **Saga Without Compensation** — Checkout saga fails mid-flow but doesn't release inventory reservation
   - **How to avoid:** Implement compensation with each step (never forward without reverse), saga orchestrator tracks state, test failure scenarios
   - **Warning signs:** Inventory reservations never released, support manually fixes stuck orders
   - **Address in:** Ordering Service phase with compensation from first implementation

7. **CQRS/MediatR Overuse** — Every operation becomes Command/Query, even simple CRUD, creating excessive ceremony
   - **How to avoid:** Reserve CQRS for complex domains (Ordering, Cart), simple CRUD stays simple, pragmatic handlers
   - **Warning signs:** More infrastructure code than business logic, developers avoid changes due to ceremony
   - **Address in:** Foundation phase with guidelines on when to use CQRS

## Implications for Roadmap

Based on research findings, the roadmap should follow a disciplined progression from foundation to gradual extraction, with each phase addressing specific architectural concerns and avoiding identified pitfalls.

### Phase 1: Foundation & Modular Monolith
**Rationale:** Establish service boundaries and patterns before any extraction. Research shows premature extraction is the #1 cause of microservices failure. A modular monolith with strict boundaries lets domain knowledge emerge naturally.

**Delivers:**
- Project structure with Features/ folder (Catalog, Cart, Ordering, Inventory as separate modules)
- Database-per-service even in monolith (separate DbContexts, migrations)
- In-process event bus for domain events (later replaced with Service Bus)
- MediatR pipeline with validation, logging, transaction behaviors
- Data ownership documentation
- Guidelines on when to use CQRS vs. simple patterns

**Addresses:**
- Pitfall: Premature service extraction
- Pitfall: CQRS overuse
- Pitfall: Database ownership confusion

**Stack elements:**
- MediatR 14.0.0, FluentValidation 12.1.1, Ardalis.Result 10.1.0
- Npgsql.EFCore 10.0.0 with multiple DbContexts
- Scrutor 7.0.0 for handler registration

**Research flag:** Standard patterns, no additional research needed

---

### Phase 2: Catalog Service
**Rationale:** Catalog is the foundation service with no upstream dependencies. Research shows read-heavy services with simple data models are easiest to build first. Provides products for all other services to reference.

**Delivers:**
- Product domain model (Product, Category aggregates)
- Admin CRUD endpoints for products
- Product listing and detail pages in frontend
- Category filtering
- Seed data with sample products
- Event publishing: `ProductCreated`, `ProductUpdated`

**Addresses:**
- Feature: Product catalog browsing (table stakes)
- Feature: Admin product CRUD (differentiator)
- Feature: Category filtering (table stakes)

**Uses:**
- MediatR for queries (simple pattern, not full CQRS)
- PostgreSQL with EF Core migrations
- Aspire health checks

**Research flag:** Standard CRUD patterns, no additional research needed

---

### Phase 3: Inventory Service
**Rationale:** Inventory must exist before Cart (to show stock levels) and before Ordering (to reserve stock). Research emphasizes consistency requirements and reservation pattern. Event-driven nature validates messaging infrastructure.

**Delivers:**
- StockItem aggregate with reservation logic
- Admin inventory adjustment UI
- Stock reservation with TTL and release
- Event handling: `OrderSubmitted` → reserve, `OrderCancelled` → release
- Event publishing: `StockConfirmed`, `StockRejected`, `LowStockWarning`
- Optimistic concurrency for stock updates

**Addresses:**
- Pitfall: Inventory overselling (reservation pattern)
- Feature: Basic inventory tracking (table stakes)
- Feature: Admin inventory management (differentiator)

**Uses:**
- MediatR for commands (full CQRS due to complexity)
- PostgreSQL with row-level locking
- Domain events for cross-service communication

**Research flag:** Reservation pattern well-documented, but test scenarios need careful design

---

### Phase 4: Messaging Infrastructure (Azure Service Bus)
**Rationale:** Before Cart and Ordering (which depend on reliable events), establish production-grade messaging. Research shows outbox pattern and idempotency are non-negotiable. This phase validates event-driven architecture works end-to-end.

**Delivers:**
- MassTransit configuration with Azure Service Bus transport
- Transactional outbox pattern implementation
- Idempotent event handlers with deduplication
- Dead-letter queue monitoring and alerting
- Correlation ID tracking across services
- Event replay capability for testing
- Replace in-process event bus with Service Bus

**Addresses:**
- Pitfall: Event-driven eventually-never consistency
- Feature: Real-time inventory updates (differentiator)

**Uses:**
- MassTransit 9.0.0 with Azure.ServiceBus transport
- Aspire.Azure.Messaging.ServiceBus 13.1.0
- Outbox table in each service database

**Research flag:** MassTransit saga patterns may need deeper research during planning

---

### Phase 5: Cart Service
**Rationale:** Cart depends on Catalog (product info) and Inventory (stock levels). Research shows session-based services have specific patterns (guest carts, cart merge on login, TTL expiration). Must handle concurrency for cart updates.

**Delivers:**
- Cart aggregate with CartItem value objects
- Guest cart support (cookie-based buyer ID)
- Cart persistence in PostgreSQL
- Add/update/remove cart items endpoints
- Cart merge on login (anonymous + authenticated)
- Cart expiration with cleanup job
- Optimistic concurrency for cart updates
- Event subscription: `ProductUpdated` to refresh cached prices
- Event publishing: `CartCheckedOut`

**Addresses:**
- Pitfall: Cart/checkout race conditions (idempotency, concurrency)
- Feature: Shopping cart with persistence (table stakes)
- UX pitfall: Cart lost on login

**Uses:**
- MediatR with transaction behavior
- PostgreSQL with optimistic concurrency (RowVersion)
- Mapster for DTO mapping

**Research flag:** Cart merge logic and TTL patterns standard, but edge cases need testing

---

### Phase 6: Ordering Service & Checkout Saga
**Rationale:** Ordering is the most complex service, orchestrating the checkout saga across Cart, Inventory, and Payment. Research shows saga compensation is critical and often forgotten. This phase validates the entire event-driven architecture.

**Delivers:**
- Order aggregate with OrderItem value objects
- Checkout endpoint (creates order from cart)
- Order status state machine (Submitted → StockConfirmed → Paid → Shipped)
- Saga orchestration: checkout flow with compensation
- Idempotency for checkout (prevents duplicate orders)
- Order confirmation page
- Order history for logged-in users
- Event publishing: `OrderSubmitted`, `OrderStockConfirmed`, `OrderPaid`
- Event subscription: `StockConfirmed`, `StockRejected`, `PaymentSucceeded`
- Mock payment service
- Compensation handlers for saga rollback

**Addresses:**
- Pitfall: Saga without compensation
- Feature: Checkout flow (table stakes)
- Feature: Order confirmation (table stakes)
- Feature: Order history (differentiator)
- Feature: Guest checkout (table stakes)

**Uses:**
- MediatR with full CQRS (complex domain)
- MassTransit saga state machine
- PostgreSQL with saga state table
- Ardalis.Result for explicit error handling

**Research flag:** MassTransit saga implementation will need phase-specific research during planning

---

### Phase 7: API Gateway
**Rationale:** With all services functional, add YARP gateway for unified API surface. Research shows gateway should be thin (routing only, no business logic). Aspire service discovery makes this straightforward.

**Delivers:**
- YARP-based API Gateway service
- Route configuration to Catalog, Cart, Ordering, Inventory
- JWT validation at gateway
- Aspire service discovery integration
- Request aggregation for complex frontend needs (optional)

**Addresses:**
- Architecture component: API Gateway
- Pattern: BFF for frontend

**Uses:**
- YARP 2.3.0
- Microsoft.Extensions.ServiceDiscovery (already in ServiceDefaults)
- Aspire service references

**Research flag:** YARP configuration is well-documented, standard patterns apply

---

### Phase 8: Service Extraction (Optional)
**Rationale:** Only extract services if scaling or deployment independence is needed. Research shows extraction should wait until domain boundaries are proven stable. For a showcase, extraction demonstrates the pattern but isn't operationally required.

**Delivers:**
- Extract Catalog.Api as standalone service
- Extract Inventory.Api as standalone service
- Extract Cart.Api as standalone service
- Extract Ordering.Api as standalone service
- IntegrationEvents as shared NuGet packages
- Update Aspire AppHost to orchestrate multiple services

**Addresses:**
- Architecture pattern: True microservices deployment

**Research flag:** Aspire multi-project orchestration and service-to-service communication patterns

---

### Phase 9: Search & Advanced Features
**Rationale:** After core flows work, add features that demonstrate technical depth. Research shows search is expected but not blocking for basic functionality.

**Delivers:**
- Product search with full-text queries
- Advanced filters (price range, category, in-stock)
- Recently viewed products (client-side)
- Low stock alerts

**Addresses:**
- Feature: Product search with filters (competitive)
- Feature: Recently viewed products (competitive)

**Research flag:** PostgreSQL full-text search patterns may need research during planning

---

### Phase Ordering Rationale

1. **Foundation first, always:** Modular monolith with clear boundaries prevents the #1 pitfall (premature extraction)

2. **Catalog before all:** No dependencies, read-heavy, provides data for other services. Research shows foundation services should be built first.

3. **Inventory before Cart:** Cart needs to display stock levels. Inventory validates event-driven patterns before complex saga.

4. **Messaging before saga:** Outbox pattern and reliable events must exist before Ordering saga. Research shows retrofitting messaging is high-cost.

5. **Cart before Ordering:** Checkout saga depends on cart data. Cart is simpler (CRUD with concurrency) vs. Ordering (saga orchestration).

6. **Gateway after services:** No value routing to non-existent services. Research shows gateway should be thin layer added late.

7. **Extraction last (optional):** Only extract when proven boundaries and operational need exist. Research shows extraction is one-way door.

This ordering maximizes learning (each phase builds on previous) while minimizing rework (patterns established before used). Each phase is independently valuable and deployable.

### Research Flags

Phases likely needing deeper research during planning:

- **Phase 4 (Messaging Infrastructure):** MassTransit saga state machine patterns — complex DSL, needs examples for our specific checkout flow
- **Phase 6 (Ordering Service):** Saga compensation patterns — failure scenarios need detailed test cases, compensating transactions are tricky
- **Phase 8 (Service Extraction):** Aspire multi-service orchestration — configuration patterns for service-to-service calls, discovery, health checks
- **Phase 9 (Search):** PostgreSQL full-text search — query patterns, indexing strategy, pagination with search

Phases with standard patterns (skip research-phase):

- **Phase 1 (Foundation):** Modular monolith patterns well-documented in eShop, Clean Architecture literature
- **Phase 2 (Catalog):** Standard CRUD with MediatR, no novel patterns
- **Phase 3 (Inventory):** Reservation pattern documented in DDD literature, straightforward implementation
- **Phase 5 (Cart):** Session-based patterns common in e-commerce, established solutions
- **Phase 7 (API Gateway):** YARP configuration well-documented, Aspire integration examples exist

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | All packages verified on NuGet with version numbers and release dates. MassTransit, MediatR, YARP are industry-standard for .NET microservices. |
| Features | HIGH | E-commerce UX patterns are well-established. Table stakes vs. differentiators clear from competitor analysis (eShop, Northwind). Anti-features identified to prevent scope creep. |
| Architecture | HIGH | Based on Microsoft official documentation (.NET Microservices Architecture Guide, Aspire docs) and eShop reference architecture. Service boundaries, event flows, and saga patterns are proven. |
| Pitfalls | MEDIUM | Based on domain expertise and common microservices failures. Specific to MicroCommerce but not yet validated through implementation. Web search verification was unavailable. |

**Overall confidence:** HIGH

The core research (stack, features, architecture) is grounded in official Microsoft documentation, verified package versions, and established e-commerce patterns. The only area with medium confidence is pitfalls, which are based on expert knowledge but not yet validated through direct MicroCommerce implementation. These will be tested as phases progress.

### Gaps to Address

**Gap 1: Token refresh implementation**
- **Issue:** Existing concern from codebase analysis — NextAuth.js token refresh not implemented, could cause session drops during checkout
- **Handle:** Address in Phase 1 (Foundation) before checkout flow. Research NextAuth.js refresh token patterns during Phase 1 planning.

**Gap 2: MassTransit saga DSL for checkout flow**
- **Issue:** Research identified saga pattern but didn't detail MassTransit-specific DSL for our checkout flow
- **Handle:** Phase 4 (Messaging Infrastructure) planning should include saga state machine examples. Reference MassTransit documentation and eShop saga implementation.

**Gap 3: Aspire multi-service orchestration**
- **Issue:** Research focused on monolith-to-microservices extraction but didn't detail Aspire AppHost patterns for multiple services
- **Handle:** Phase 8 (Service Extraction) planning should research Aspire service references, discovery, and inter-service communication patterns.

**Gap 4: Guest cart to user cart merge logic**
- **Issue:** Research identified need for cart merge on login but didn't specify algorithm
- **Handle:** Phase 5 (Cart Service) planning should research merge strategies (union, replace, user-preference). Common pattern is merge with quantity sum and user confirmation UI.

**Gap 5: Event replay and read model rebuild**
- **Issue:** Research mentioned event replay capability but didn't detail implementation
- **Handle:** Phase 4 (Messaging Infrastructure) should include replay mechanism design. MassTransit has built-in message redelivery, but full read model rebuild may need custom solution.

**Gap 6: .NET 10 preview stability for production**
- **Issue:** Project uses .NET 10 preview and NextAuth.js beta 5. Research didn't assess production readiness.
- **Handle:** Monitor .NET 10 and NextAuth.js release schedules. Consider pinning to stable versions (e.g., wait for .NET 10 RTM) before Phase 8 (extraction) if this is intended for real deployment vs. showcase.

**Gap 7: Performance testing scenarios**
- **Issue:** Research identified performance traps but didn't specify load testing approach
- **Handle:** Each phase should include performance test plan (especially Phase 5 Cart, Phase 6 Ordering). Define SLAs: e.g., checkout completes in <2s with 100 concurrent users.

## Sources

### Primary (HIGH confidence)
- [Microsoft .NET Microservices Architecture Guide](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/) — Overall architecture patterns, service boundaries, saga patterns
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/) — Aspire orchestration, service integration patterns
- [eShop Reference Application](https://github.com/dotnet/eShop) — Real-world microservices implementation, service structure, event flows
- [NuGet Gallery](https://www.nuget.org/) — All package versions and release dates verified (MassTransit 9.0.0, MediatR 14.0.0, YARP 2.3.0, etc.)
- [MassTransit Documentation](https://masstransit.io/documentation/concepts) — Saga patterns, outbox pattern, consumer configuration
- [MediatR Wiki](https://github.com/jbogard/MediatR/wiki) — Pipeline behaviors, handler registration

### Secondary (MEDIUM confidence)
- Domain expertise in distributed systems — Saga compensation patterns, eventual consistency strategies
- E-commerce UX patterns — Cart behavior, checkout flow, guest checkout (sourced from Baymard Institute research patterns)
- Microservices pitfalls — Common failure modes from distributed systems literature

### Tertiary (LOW confidence)
- MicroCommerce codebase analysis (`.planning/codebase/*.md`) — Current project state, existing concerns
- NextAuth.js beta stability — Needs validation against official NextAuth.js roadmap

---
*Research completed: 2026-01-29*
*Ready for roadmap: yes*

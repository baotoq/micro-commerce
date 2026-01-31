# Pitfalls Research

**Domain:** E-commerce Microservices
**Researched:** 2025-01-29
**Confidence:** MEDIUM (based on domain expertise; web search verification unavailable)

## Critical Pitfalls

### Pitfall 1: Premature Service Extraction

**What goes wrong:**
Teams extract services before understanding domain boundaries. Results in chatty inter-service communication, distributed transactions that should be local, and services that can't be deployed independently because they're too coupled.

**Why it happens:**
- Technical boundaries (one team, one service) chosen over business boundaries
- Excitement about microservices leads to extracting too early
- Modular monolith not given enough time to reveal natural seams

**How to avoid:**
1. Start with a modular monolith with strict module boundaries
2. Enforce: modules communicate only through well-defined interfaces (no shared database tables)
3. Track inter-module communication patterns for 2-4 sprints before extraction
4. Extract only when: deployment independence is needed OR scaling requirements differ OR team ownership is clear

**Warning signs:**
- Services call each other synchronously for every request
- "We need to deploy these 3 services together"
- Database joins across what are supposed to be separate bounded contexts
- Single user action requires >3 synchronous service calls

**Phase to address:**
Foundation phase — establish modular monolith with clear boundaries before any extraction

---

### Pitfall 2: Cart/Checkout Race Conditions (The Double-Spend Problem)

**What goes wrong:**
User clicks "Place Order" twice quickly, or opens checkout in two tabs. Without proper idempotency and optimistic concurrency, they get charged twice or inventory goes negative.

**Why it happens:**
- Cart state checked at time T, order placed at T+100ms when cart already changed
- No idempotency key on checkout requests
- Optimistic concurrency not implemented on cart aggregate
- Frontend doesn't disable submit button or show loading state

**How to avoid:**
1. **Idempotency keys**: Every checkout request includes client-generated idempotency key. Server deduplicates within 24-hour window.
2. **Optimistic concurrency**: Cart aggregate has version/ETag. Checkout fails if cart modified since last read.
3. **Frontend guards**: Disable checkout button on click, show processing state, prevent double-submit
4. **Saga compensation**: If payment succeeds but order creation fails, automatically refund

**Warning signs:**
- QA can create duplicate orders by rapid clicking
- Customer complaints about double charges
- Inventory occasionally goes negative
- Cart totals don't match order totals

**Phase to address:**
Cart Service phase — implement concurrency controls from day one, not retrofitted

---

### Pitfall 3: Inventory Overselling (Distributed Stock Problem)

**What goes wrong:**
100 users try to buy the last 10 items simultaneously. Without proper reservation, 100 orders are placed for 10 items. Either you oversell (bad) or reject orders after payment (worse).

**Why it happens:**
- Stock check happens before payment, decrement happens after — race window
- Inventory service is eventually consistent but checkout expects immediate consistency
- No reservation mechanism — just "check and decrement"

**How to avoid:**
1. **Reservation pattern**: Reserve inventory when added to cart (with TTL). Confirm on successful order, release on abandonment/timeout.
2. **Pessimistic locking at checkout**: Lock inventory row during checkout saga (short duration)
3. **Accept overselling gracefully**: For low-value items, allow overselling and handle backorders
4. **Separate high-demand items**: Flash sales need different patterns (queue-based, not optimistic)

**Warning signs:**
- Negative inventory counts in database
- Customer complaints about order cancellation after payment
- Flash sales consistently oversell
- Inventory service shows different stock than catalog service

**Phase to address:**
Inventory Service phase — reservation pattern is architectural, not just code

---

### Pitfall 4: Event-Driven Eventually-Never Consistency

**What goes wrong:**
Services publish events but consumers fail silently. Order placed event published, but inventory never decremented because consumer crashed. No one notices until inventory audit.

**Why it happens:**
- No dead-letter queue monitoring
- No idempotent consumers — retry causes duplicate processing
- No correlation ID tracking across service boundaries
- "Fire and forget" mentality without verification

**How to avoid:**
1. **Outbox pattern**: Events written to outbox table in same transaction as business data. Background process publishes reliably.
2. **Idempotent consumers**: Every handler checks if event already processed (by event ID)
3. **Dead-letter monitoring**: Alert on any message hitting DLQ. DLQ is not a trash bin.
4. **Correlation tracking**: Every event chain has correlation ID. Trace entire saga.
5. **Reconciliation jobs**: Periodic jobs verify eventual consistency actually happened

**Warning signs:**
- DLQ has messages older than 1 hour
- No alerts on consumer failures
- "We'll fix it in the next deployment" for stuck messages
- Inventory doesn't match orders after end-of-day reconciliation

**Phase to address:**
Messaging Infrastructure phase — outbox pattern and monitoring before any event publishing

---

### Pitfall 5: Database-Per-Service Without Data Ownership Clarity

**What goes wrong:**
Teams implement database-per-service but still need cross-service queries. They either: (a) make synchronous calls creating latency, (b) duplicate data without sync strategy, or (c) add shared database defeating isolation.

**Why it happens:**
- Reports need joins across Order + Customer + Product
- Admin dashboards need aggregated views
- No CQRS — same model for writes and reads
- Data ownership not defined before service split

**How to avoid:**
1. **Define data ownership first**: Before extraction, document which service owns which data entities
2. **CQRS for reads**: Separate read models that aggregate data from events
3. **API composition**: BFF or API gateway composes responses from multiple services for complex queries
4. **Event-carried state transfer**: Services publish events with enough data for consumers to build local read models

**Warning signs:**
- Services make synchronous calls to get data for responses
- "We need a reporting database" that duplicates production
- Admin queries bypass services to hit databases directly
- Circular dependencies between services

**Phase to address:**
Foundation phase — data ownership mapping before any service has a database

---

### Pitfall 6: Saga Implementation Without Compensation

**What goes wrong:**
Order saga: Reserve Inventory -> Charge Payment -> Create Order. Payment fails. No compensation logic to release inventory reservation. Inventory locked forever.

**Why it happens:**
- Happy path implemented first, compensation "later"
- Compensation logic is hard — requires reversing side effects
- No saga orchestrator — just chained events with no central tracking
- Testing only covers success scenarios

**How to avoid:**
1. **Implement compensation with each step**: Never implement forward action without its compensating action
2. **Saga orchestrator pattern**: Central coordinator tracks saga state, triggers compensations
3. **Test failure scenarios**: Every saga test includes: "what if step N fails?"
4. **Timeouts with compensation**: Long-running sagas need timeout + compensation, not just timeout

**Warning signs:**
- Inventory reservations never released
- Customer support manually fixing stuck orders
- "We need to restart the service to clear stuck sagas"
- Saga state table growing indefinitely

**Phase to address:**
Ordering Service phase — saga pattern with compensation from first implementation

---

### Pitfall 7: CQRS/MediatR Overuse

**What goes wrong:**
Every operation becomes a Command or Query, even simple CRUD. 4-file ceremony (Command, Handler, Validator, Response) for "get product by ID". Codebase becomes navigable only through IDE "find usages".

**Why it happens:**
- "We're doing CQRS" becomes religion, not pragmatism
- Pattern applied uniformly without considering complexity
- Separate read/write models for data that doesn't need it

**How to avoid:**
1. **Reserve CQRS for complex domains**: Ordering, Cart, Inventory (state machines, business rules)
2. **Simple CRUD stays simple**: Product catalog reads can be direct repository calls
3. **Vertical slices, not layers**: Group by feature, not by pattern
4. **Pragmatic handlers**: Read-only queries can skip validation ceremony

**Warning signs:**
- More infrastructure code than business logic
- Simple endpoint requires touching 5+ files
- Developers avoid making changes because "it's too much ceremony"
- MediatR pipelines have 6+ behaviors

**Phase to address:**
Foundation phase — establish when to use CQRS vs simple patterns

---

## Technical Debt Patterns

Shortcuts that seem reasonable but create long-term problems.

| Shortcut | Immediate Benefit | Long-term Cost | When Acceptable |
|----------|-------------------|----------------|-----------------|
| Shared database between services | Faster initial development | Coupling, can't deploy independently, schema migrations affect all services | Never in target architecture; OK in modular monolith phase |
| Synchronous service calls for reads | Simpler implementation | Latency multiplication, cascading failures, tight coupling | OK for BFF composition; never for service-to-service writes |
| Event handlers without idempotency | Faster development | Duplicate processing, data corruption on retries | Never |
| Cart stored in memory/session | Simpler, faster | Lost carts on restart, no cross-device support | Never for e-commerce; acceptable for quick demos |
| Skipping outbox pattern | Simpler event publishing | Lost events, inconsistent state | Only for non-critical events (analytics) |
| Single shared message topic | Less infrastructure | Consumer coupling, message filtering overhead | Never for domain events |

## Integration Gotchas

Common mistakes when connecting to external services.

| Integration | Common Mistake | Correct Approach |
|-------------|----------------|------------------|
| Azure Service Bus | No dead-letter handling | Configure DLQ, monitor actively, process or alert on DLQ messages |
| Azure Service Bus | Large messages | Keep messages small (<256KB), use claim check pattern for large payloads |
| Keycloak | Hardcoded token URLs | Use OIDC discovery (/.well-known/openid-configuration) |
| Keycloak | Not validating audience | Always validate aud claim matches expected API audience |
| PostgreSQL (per service) | Schema migrations in code | Use migration tools (EF migrations, Flyway) with versioning |
| PostgreSQL | Connection exhaustion | Configure connection pooling, use connection limits per service |
| MediatR | Handler not registered | Use assembly scanning with explicit registration verification in tests |

## Performance Traps

Patterns that work at small scale but fail as usage grows.

| Trap | Symptoms | Prevention | When It Breaks |
|------|----------|------------|----------------|
| N+1 queries in catalog | Slow product list pages | Use projection queries, include related data | >100 products per page |
| No pagination on search | Memory exhaustion, timeouts | Cursor-based pagination from day one | >1000 total products |
| Cart recalculation on every request | Slow add-to-cart | Calculate totals on cart modification, cache result | >20 items in cart |
| Synchronous inventory check | Checkout timeout | Async reservation with confirmation | >100 concurrent checkouts |
| Event replay without batching | Consumer overwhelmed | Batch processing for event replay/rebuild | >10,000 events in replay |
| Full product in search index | Index size explosion | Index only searchable fields, fetch full product on detail view | >100,000 products |

## Security Mistakes

Domain-specific security issues beyond general web security.

| Mistake | Risk | Prevention |
|---------|------|------------|
| Cart ID in URL without ownership check | Users can view/modify other carts | Associate cart with user/session, verify ownership on every operation |
| Order total calculated client-side | Price manipulation | Always recalculate totals server-side from source of truth |
| Inventory count exposed in API | Competitor intelligence | Expose only "in stock" / "low stock" / "out of stock" |
| Coupon codes enumerable | Coupon abuse | Rate limit coupon validation, use non-sequential codes |
| Guest checkout without rate limiting | Fraud, card testing | Rate limit by IP, require CAPTCHA after N failures |
| Payment status in client-accessible JWT | Order status manipulation | Payment status server-side only, never in client token |

## UX Pitfalls

Common user experience mistakes in e-commerce microservices.

| Pitfall | User Impact | Better Approach |
|---------|-------------|-----------------|
| Cart lost on login | Frustration, abandoned purchase | Merge anonymous cart with user cart on login |
| Stale inventory on product page | Disappointment at checkout | Show "X left" only for low stock, check at checkout |
| No checkout progress indicator | Anxiety, page refresh | Show clear steps (Cart -> Shipping -> Payment -> Confirm) |
| Payment timeout without recovery | Lost sale, double charge risk | Store payment intent, allow resume on return |
| Order confirmation only by email | User can't verify immediately | Show confirmation page with order number, don't rely on email |
| "Out of stock" after add to cart | Frustration | Soft reserve on add, clear message if reservation expires |

## "Looks Done But Isn't" Checklist

Things that appear complete but are missing critical pieces.

- [ ] **Cart Service:** Often missing cart merge on login — verify anonymous cart + authenticated cart merge
- [ ] **Cart Service:** Often missing cart expiration — verify TTL and cleanup job exist
- [ ] **Checkout:** Often missing idempotency — verify duplicate submit handled gracefully
- [ ] **Checkout:** Often missing address validation — verify shipping address validated before payment
- [ ] **Inventory:** Often missing reservation timeout — verify reservations released after TTL
- [ ] **Inventory:** Often missing stock reconciliation — verify periodic job compares expected vs actual
- [ ] **Orders:** Often missing order status webhooks — verify status change triggers notifications
- [ ] **Orders:** Often missing cancellation window — verify orders cancelable before shipping
- [ ] **Events:** Often missing DLQ monitoring — verify alerts on dead-lettered messages
- [ ] **Events:** Often missing event replay capability — verify system can rebuild read models from events

## Recovery Strategies

When pitfalls occur despite prevention, how to recover.

| Pitfall | Recovery Cost | Recovery Steps |
|---------|---------------|----------------|
| Premature service extraction | HIGH | Identify coupled services, consolidate back to module, re-extract with proper boundaries |
| Checkout race conditions | MEDIUM | Add idempotency retroactively, reconcile duplicate orders, refund affected customers |
| Inventory overselling | MEDIUM | Implement backorder flow, notify affected customers, add reservation pattern |
| Lost events (no outbox) | HIGH | Event replay from source systems, manual reconciliation, implement outbox pattern |
| Saga without compensation | HIGH | Manual cleanup of stuck states, implement compensation handlers, replay failed sagas |
| CQRS overuse | MEDIUM | Identify simple CRUD, remove unnecessary handlers, consolidate to simpler patterns |

## Pitfall-to-Phase Mapping

How roadmap phases should address these pitfalls.

| Pitfall | Prevention Phase | Verification |
|---------|------------------|--------------|
| Premature service extraction | Phase 1: Foundation | Module boundaries enforced, inter-module communication tracked |
| Cart/checkout race conditions | Phase 2: Cart Service | Idempotency tests pass, concurrent checkout tests pass |
| Inventory overselling | Phase 3: Inventory Service | Concurrent purchase tests verify no overselling |
| Event consistency issues | Phase 1: Foundation | Outbox pattern implemented, DLQ monitoring active |
| Database ownership confusion | Phase 1: Foundation | Data ownership document created and reviewed |
| Saga compensation missing | Phase 4: Ordering Service | Failure scenario tests verify compensation triggers |
| CQRS overuse | Phase 1: Foundation | Guidelines document specifies when to use CQRS |

## MicroCommerce-Specific Risks

Based on current project state from CONCERNS.md:

| Existing Concern | Related Pitfall | Mitigation |
|------------------|-----------------|------------|
| Domain events dispatch without transaction | Lost events | Implement outbox pattern before using domain events |
| Unused BuildingBlocks infrastructure | Over-engineering | Use when needed, remove if not needed by Phase 2 |
| Preview/beta dependencies (.NET 10, NextAuth beta) | Unexpected breaking changes | Pin versions, test upgrade path before production |
| No test coverage | All pitfalls undetected | Add integration tests for race conditions and saga failures |
| Token refresh not implemented | Session drops during checkout | Implement refresh before checkout flow |

## Sources

- Domain expertise in distributed systems and e-commerce architecture
- Analysis of existing MicroCommerce codebase (`.planning/codebase/*.md`)
- Common patterns from microservices literature (Saga pattern, Outbox pattern, CQRS)
- Note: Web search verification unavailable; findings based on established patterns

---
*Pitfalls research for: E-commerce Microservices (MicroCommerce)*
*Researched: 2025-01-29*

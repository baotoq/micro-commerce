# Milestones

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


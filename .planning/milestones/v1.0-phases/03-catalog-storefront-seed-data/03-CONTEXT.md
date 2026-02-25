# Phase 3: Catalog Storefront & Seed Data - Context

**Gathered:** 2026-02-07
**Status:** Ready for planning

<domain>
## Phase Boundary

Build the customer-facing product browsing experience: product grid listing, product detail page, category filtering, search by name/description, and seed data with ~50 sample products. Cart functionality is Phase 6 — this phase only adds the "Add to Cart" button UI (non-functional until Cart domain exists).

</domain>

<decisions>
## Implementation Decisions

### Product grid layout
- Claude's discretion on column count (responsive)
- Each card shows: product image, name, price, and category badge
- Hover reveals a quick "Add to Cart" button overlay
- Infinite scroll for loading more products
- Out-of-stock products appear greyed out with "Out of Stock" badge
- Apple Store aesthetic — clean, minimal, premium feel

### Homepage structure
- Hero banner section at top
- Featured products section
- Full product grid below with browse-all

### Product detail page
- Claude's discretion on layout (image left/info right vs full-width hero)
- Claude decides: related products section, quantity selector, content depth
- Should match the Apple Store clean aesthetic

### Search & filtering
- Claude's discretion on search bar placement
- Claude's discretion on filter UX (sidebar vs top chips)
- Sort options: price (low-to-high, high-to-low), name (A-Z), newest
- Instant/debounced search as you type
- PostgreSQL-based search (LIKE/full-text) — sufficient for seed data volume

### Seed data
- Electronics-focused categories (laptops, phones, accessories, audio, etc.)
- ~50 products across categories
- Placeholder images (not real product photos)
- Auto-seeds empty database on startup in Development mode only

### Claude's Discretion
- Grid column count (responsive breakpoints)
- Loading skeleton vs spinner approach
- Search bar placement (header vs products page)
- Filter UX pattern (sidebar vs chips)
- Product detail page layout
- Whether to include related products section
- Quantity selector on detail page (just button vs selector + button)
- Product detail content depth (description + category, or add specs table)

</decisions>

<specifics>
## Specific Ideas

- "I want it to feel like the Apple Store — clean, minimal, premium"
- Hover on product cards reveals quick add-to-cart action
- Infinite scroll (not pagination) for seamless browsing
- Debounced instant search for responsive feel

</specifics>

<deferred>
## Deferred Ideas

- Elasticsearch integration — noted in roadmap as Phase 12 (post-v1)
- User asked about Elasticsearch for search — PostgreSQL sufficient for now

</deferred>

---

*Phase: 03-catalog-storefront-seed-data*
*Context gathered: 2026-02-07*

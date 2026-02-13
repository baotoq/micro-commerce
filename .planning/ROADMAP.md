# Roadmap: MicroCommerce

## Milestones

- ✅ **v1.0 MVP** — Phases 1-10 (shipped 2026-02-13) — [archive](milestones/v1.0-ROADMAP.md)
- 🚧 **v1.1 User Features** — Phases 11-14 (in progress)

## Phases

<details>
<summary>✅ v1.0 MVP (Phases 1-10) — SHIPPED 2026-02-13</summary>

- [x] Phase 1: Foundation & Project Structure (6/6 plans) — completed 2026-01-30
- [x] Phase 2: Catalog Domain & Admin CRUD (7/7 plans) — completed 2026-01-30
- [x] Phase 3: Catalog Storefront & Seed Data (6/6 plans) — completed 2026-02-07
- [x] Phase 4: Inventory Domain (5/5 plans) — completed 2026-02-08
- [x] Phase 5: Event Bus Infrastructure (3/3 plans) — completed 2026-02-09
- [x] Phase 6: Cart Domain (4/4 plans) — completed 2026-02-09
- [x] Phase 7: Ordering Domain & Checkout (4/4 plans) — completed 2026-02-10
- [x] Phase 8: Order History & Management (5/5 plans) — completed 2026-02-12
- [x] Phase 9: API Gateway (3/3 plans) — completed 2026-02-12
- [x] Phase 10: Testing & Polish (6/6 plans) — completed 2026-02-13

</details>

### 🚧 v1.1 User Features (In Progress)

**Milestone Goal:** Add authenticated user experiences — profiles, verified purchase reviews, and wishlists — building on the existing Keycloak auth foundation.

#### Phase 11: User Profiles & Authentication Flow
**Goal**: Users can manage their profiles with display name, avatar, address book, and seamlessly transition from guest to authenticated with preserved cart and order history
**Depends on**: Phase 10 (v1.0 foundation)
**Requirements**: PROF-01, PROF-02, PROF-03, PROF-04, PROF-05, PROF-06, PROF-07, PROF-08
**Success Criteria** (what must be TRUE):
  1. User can set display name and upload avatar image on their profile
  2. User can add, edit, delete, and set default shipping address in address book
  3. User can view "My Account" page showing profile info and order history
  4. Guest cart and orders automatically merge to user account on login without data loss
  5. Avatar images persist across deployments (Azure Blob Storage, not container filesystem)
**Plans**: TBD

Plans:
- [ ] 11-01: TBD
- [ ] 11-02: TBD

#### Phase 12: Product Reviews & Ratings
**Goal**: Users can submit star ratings and written reviews for purchased products with verified purchase badges, and all users can view aggregate ratings on product pages
**Depends on**: Phase 11
**Requirements**: REVW-01, REVW-02, REVW-03, REVW-04, REVW-05, REVW-06, REVW-07, REVW-08
**Success Criteria** (what must be TRUE):
  1. User can submit a star rating (1-5) and text review for products they have purchased
  2. Reviews display "verified purchase" badge when user purchased the product
  3. User can see all reviews on product detail pages with average rating and review count
  4. User can edit or delete their own review (one review per product enforced)
  5. Review submission is blocked for products the user has not purchased
**Plans**: TBD

Plans:
- [ ] 12-01: TBD
- [ ] 12-02: TBD

#### Phase 13: Wishlists & Saved Items
**Goal**: Users can save products to a persistent wishlist and move items to cart
**Depends on**: Phase 12
**Requirements**: WISH-01, WISH-02, WISH-03, WISH-04, WISH-05
**Success Criteria** (what must be TRUE):
  1. User can add and remove products from their wishlist
  2. User can view wishlist page showing all saved products
  3. User can move wishlist items to cart with proper stock validation
  4. Product cards display heart icon indicator when product is in wishlist
  5. Wishlist persists across sessions and devices
**Plans**: TBD

Plans:
- [ ] 13-01: TBD
- [ ] 13-02: TBD

#### Phase 14: Integration & Polish
**Goal**: All user features work cohesively with seamless navigation and E2E testing coverage
**Depends on**: Phase 13
**Requirements**: None (enables all v1.1 features)
**Success Criteria** (what must be TRUE):
  1. User can navigate from profile → order history → review submission without friction
  2. User can access wishlist from product cards, product pages, and account navigation
  3. E2E tests cover guest flow, authenticated flow, and guest-to-auth migration scenarios
  4. UI is visually cohesive across profile, reviews, and wishlist features
**Plans**: TBD

Plans:
- [ ] 14-01: TBD

## Progress

**Execution Order:**
Phases execute in numeric order: 11 → 12 → 13 → 14

| Phase | Milestone | Plans | Status | Completed |
|-------|-----------|-------|--------|-----------|
| 1. Foundation & Project Structure | v1.0 | 6/6 | Complete | 2026-01-30 |
| 2. Catalog Domain & Admin CRUD | v1.0 | 7/7 | Complete | 2026-01-30 |
| 3. Catalog Storefront & Seed Data | v1.0 | 6/6 | Complete | 2026-02-07 |
| 4. Inventory Domain | v1.0 | 5/5 | Complete | 2026-02-08 |
| 5. Event Bus Infrastructure | v1.0 | 3/3 | Complete | 2026-02-09 |
| 6. Cart Domain | v1.0 | 4/4 | Complete | 2026-02-09 |
| 7. Ordering Domain & Checkout | v1.0 | 4/4 | Complete | 2026-02-10 |
| 8. Order History & Management | v1.0 | 5/5 | Complete | 2026-02-12 |
| 9. API Gateway | v1.0 | 3/3 | Complete | 2026-02-12 |
| 10. Testing & Polish | v1.0 | 6/6 | Complete | 2026-02-13 |
| 11. User Profiles & Authentication Flow | v1.1 | 0/TBD | Not started | - |
| 12. Product Reviews & Ratings | v1.1 | 0/TBD | Not started | - |
| 13. Wishlists & Saved Items | v1.1 | 0/TBD | Not started | - |
| 14. Integration & Polish | v1.1 | 0/TBD | Not started | - |

---
*Roadmap created: 2026-01-29*
*v1.0 shipped: 2026-02-13*
*v1.1 started: 2026-02-13*

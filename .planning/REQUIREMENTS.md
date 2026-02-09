# Requirements: MicroCommerce

**Defined:** 2026-02-13
**Core Value:** A user can complete a purchase end-to-end

## v1.1 Requirements

Requirements for v1.1 User Features milestone. Each maps to roadmap phases.

### User Profiles

- [ ] **PROF-01**: User can set display name on their profile
- [ ] **PROF-02**: User can upload an avatar image
- [ ] **PROF-03**: User can add a shipping address to their address book
- [ ] **PROF-04**: User can edit a saved shipping address
- [ ] **PROF-05**: User can delete a saved shipping address
- [ ] **PROF-06**: User can set a default shipping address
- [ ] **PROF-07**: User can view "My Account" page with profile and order history
- [ ] **PROF-08**: Guest cart and orders merge to user account on login

### Product Reviews

- [ ] **REVW-01**: User can submit a star rating (1-5) for a purchased product
- [ ] **REVW-02**: User can write a text review alongside the rating
- [ ] **REVW-03**: Reviews display "verified purchase" badge when user purchased the product
- [ ] **REVW-04**: User can see all reviews on a product detail page
- [ ] **REVW-05**: Product pages display average star rating and review count
- [ ] **REVW-06**: User can edit their own review
- [ ] **REVW-07**: User can delete their own review
- [ ] **REVW-08**: User can only submit one review per product

### Wishlists

- [ ] **WISH-01**: User can add a product to their wishlist
- [ ] **WISH-02**: User can remove a product from their wishlist
- [ ] **WISH-03**: User can view their wishlist page with all saved products
- [ ] **WISH-04**: User can move a wishlist item to cart
- [ ] **WISH-05**: Product cards show wishlist indicator (heart icon) when saved

## v2 Requirements

Deferred to future release. Tracked but not in current roadmap.

### Reviews

- **REVW-09**: User can vote reviews as helpful or not helpful
- **REVW-10**: User can sort reviews by date, rating, or verified status
- **REVW-11**: User can filter reviews by star count
- **REVW-12**: User can upload photos with their review

### Wishlists

- **WISH-06**: User can create multiple named wishlists
- **WISH-07**: User can share wishlist via public link
- **WISH-08**: User receives notification when wishlist item price drops

## Out of Scope

| Feature | Reason |
|---------|--------|
| Anonymous reviews | Trust collapse risk; verified purchase enforcement is core |
| Incentivized reviews | FTC compliance issues, deceptive practice |
| AI review summaries | Requires LLM integration, cost analysis — defer to v2+ |
| Review Q&A section | Large scope, separate feature entirely |
| Social login (Google, GitHub) | Keycloak email/password sufficient, can add OAuth later |
| Public user profiles | Privacy concern; profiles are private to account owner |
| Review image moderation | Requires moderation service; defer until review images added |

## Traceability

Which phases cover which requirements. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| PROF-01 | Phase 11 | Pending |
| PROF-02 | Phase 11 | Pending |
| PROF-03 | Phase 11 | Pending |
| PROF-04 | Phase 11 | Pending |
| PROF-05 | Phase 11 | Pending |
| PROF-06 | Phase 11 | Pending |
| PROF-07 | Phase 11 | Pending |
| PROF-08 | Phase 11 | Pending |
| REVW-01 | Phase 12 | Pending |
| REVW-02 | Phase 12 | Pending |
| REVW-03 | Phase 12 | Pending |
| REVW-04 | Phase 12 | Pending |
| REVW-05 | Phase 12 | Pending |
| REVW-06 | Phase 12 | Pending |
| REVW-07 | Phase 12 | Pending |
| REVW-08 | Phase 12 | Pending |
| WISH-01 | Phase 13 | Pending |
| WISH-02 | Phase 13 | Pending |
| WISH-03 | Phase 13 | Pending |
| WISH-04 | Phase 13 | Pending |
| WISH-05 | Phase 13 | Pending |

**Coverage:**
- v1.1 requirements: 21 total
- Mapped to phases: 21
- Unmapped: 0

**Coverage validation:** ✓ 100% (21/21 requirements mapped)

---
*Requirements defined: 2026-02-13*
*Last updated: 2026-02-13 after roadmap creation*

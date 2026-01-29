# Requirements

**Project:** MicroCommerce
**Version:** v1
**Last updated:** 2026-01-29

## v1 Requirements

### Catalog

- [ ] **CAT-01**: User can browse products in a grid view with image, name, price
- [ ] **CAT-02**: User can view product detail page with full info and add-to-cart
- [ ] **CAT-03**: User can filter products by category
- [ ] **CAT-04**: User can search products by name/description

### Cart

- [ ] **CART-01**: User can view cart, update quantities, and remove items
- [ ] **CART-02**: User's cart persists across page refreshes (database-backed)
- [ ] **CART-03**: User sees feedback when adding item to cart (toast/badge)
- [ ] **CART-04**: Cart updates feel instant (optimistic UI)

### Checkout

- [ ] **CHK-01**: User can complete checkout flow (shipping info, payment, confirmation)
- [ ] **CHK-02**: User can checkout as guest without creating account
- [ ] **CHK-03**: User sees mock payment that simulates success/failure
- [ ] **CHK-04**: User sees order confirmation with summary after purchase

### Orders

- [ ] **ORD-01**: Logged-in user can view their order history
- [ ] **ORD-02**: User sees real-time order status updates
- [ ] **ORD-03**: User can view order detail page

### Inventory

- [ ] **INV-01**: System tracks stock levels per product
- [ ] **INV-02**: System reserves stock during checkout (prevents overselling)
- [ ] **INV-03**: Stock counts update in real-time when orders placed

### Admin

- [ ] **ADM-01**: Admin can create, edit, and delete products
- [ ] **ADM-02**: Admin can adjust inventory stock levels
- [ ] **ADM-03**: Admin sees dashboard with order counts and revenue
- [ ] **ADM-04**: Admin can view and manage orders

### Infrastructure

- [ ] **INFRA-01**: System has seed data with sample products
- [ ] **INFRA-02**: API Gateway (YARP) routes frontend requests to services
- [ ] **INFRA-03**: Services communicate via Azure Service Bus events
- [ ] **INFRA-04**: Unit and integration tests cover critical paths

## v2 Requirements (Deferred)

Features explicitly deferred to future versions:

- [ ] Real payment processing (Stripe integration)
- [ ] User reviews and ratings
- [ ] Wishlist/favorites
- [ ] Email notifications
- [ ] Coupon/discount codes
- [ ] Multiple shipping options
- [ ] Tax calculation
- [ ] Multi-currency support
- [ ] Product variants (size/color)
- [ ] Saved payment methods
- [ ] Recommendations engine
- [ ] Advanced analytics dashboard
- [ ] Multi-language (i18n)
- [ ] Address validation
- [ ] Order cancellation/refunds

## Out of Scope

Explicitly excluded from this project:

| Exclusion | Rationale |
|-----------|-----------|
| Real payment processing | Mock payments sufficient for demo, avoids compliance burden |
| Event sourcing | Adds complexity without proportional demo value |
| Separate admin application | Integrated admin routes simpler, shared auth context |
| Mobile app | Web-first, responsive design covers mobile |
| Real-time chat/support | Not core to e-commerce demo |
| Multi-tenancy | Single store demonstration |
| Social login | Keycloak handles this if needed, not v1 priority |

## Traceability

*Populated by roadmap creation*

| REQ-ID | Phase | Status |
|--------|-------|--------|
| — | — | — |

---
*Requirements defined: 2026-01-29*
*Total v1 requirements: 24*

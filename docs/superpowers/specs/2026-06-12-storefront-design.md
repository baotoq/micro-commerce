# Buyer Storefront — Design

**Date:** 2026-06-12
**Status:** Approved (brainstorm with Bao; variant picks via visual companion)

## 1. Context & goal

The seller side of micro-commerce is complete (all `/seller` pages backed by real
Catalog.API domains, Keycloak seller auth, promotions MVP). The buyer side is a
placeholder (`src/web/src/app/page.tsx` renders "Ready for revamp"). This project
builds the buyer-facing storefront and closes the commerce loop: a buyer browses,
fills a cart, checks out, and the resulting order appears in the seller's orders,
analytics, and promotions surfaces with no seeded/manual data involved.

The seeded catalog (42 pottery products in Vessels/Tableware/Drinkware) already
matches the buyer hi-fi designs, including the "42 pieces" copy on the home design.

## 2. Scope

**In scope (core purchase funnel):**
- Home/shop page (replaces placeholder), product detail, cart, checkout,
  order confirmation.
- Keycloak buyer accounts; checkout requires login; browsing and cart are public.
- Promo code application (existing promotions domain) end to end, including
  persisting the discount on the order.
- Demo payment form (brand + last4 only; nothing charged).

**Out of scope (later phases):**
- Buyer account area, order tracking, addresses, returns, reviews, notifications
  (`hifi-buyer-extras.jsx` flows).
- Product variants (color/size) and star ratings — the "lean adapt" decision below.
- Promo redemption counting; multi-tenancy; real payment processing.

## 3. Decisions (with rationale)

| Decision | Choice | Rationale |
|---|---|---|
| MVP scope | Core funnel only | Closes the buyer→order→seller loop fastest |
| Buyer identity | Keycloak accounts; login required at checkout | One checkout code path; orders always tied to an account; browse/cart stay public |
| Buyer role model | No new realm role — buyer = any authenticated user | Avoids realm churn; `SellerPolicy` unchanged; sellers can also buy |
| Cart storage | httpOnly cookie `mc_cart` | Server Components render live prices; survives login redirect; no backend work |
| Payment | Demo card form | Matches existing `PaymentBrand`/`PaymentLastFour` order model |
| Branding | **Micro Commerce** everywhere | Per CLAUDE.md BRAND rule; "Mira Studio" in buyer hi-fi copy is superseded — Mira remains intentional only on `/seller/apply` |
| Design-data parity | **Lean adapt** | Render only real domain fields; drop swatches/size pickers/ratings from layouts; no fake data, no domain surgery |
| Home layout | `Home_Tiles` (A) | Hero strip + chip filters + product grid (`hifi-home.jsx`) |
| Product layout | `Product_Classic` (A) | Thumb rail + image + info column (`hifi-product.jsx`) |
| Cart layout | `Cart_List` (A) mobile, `Cart_Desktop` (D) desktop | (`hifi-cart.jsx`) |
| Checkout layout | `Checkout_Steps` (A) mobile, `Checkout_Desktop` (D) desktop | One form state, responsive presentation (`hifi-checkout.jsx`) |

## 4. Frontend architecture

New route group `src/web/src/app/(storefront)/` with its own layout:

| Route | Source design | Notes |
|---|---|---|
| `/` | Home A | Hero strip, category chips, piece count, sort, product grid. Home **is** the shop; filters/search/sort via `searchParams` |
| `/products/[sku]` | Product A | Gallery from `photoUrls` (thumb rail), name, price, description, tag chips, origin, low-stock badge from `inventory`, qty stepper, Add to bag |
| `/cart` | Cart A / Cart D | Responsive single page: qty steppers, remove, promo input, sticky summary (desktop), free-shipping affordances omitted (no backing data) |
| `/checkout` | Checkout A / Checkout D | Login-gated. Desktop: single-page 2-col form + order summary. Mobile: wizard Ship → Pay → Review. Same form state (react-hook-form + `"use no memo"`) |
| `/checkout/confirmation/[number]` | — | Order number, lines, totals. Buyer-authenticated, dynamic |

Shared layout: adapted `ShopperTopbar` — Micro Commerce logo, Shop nav, search
input (submits to `/?q=`), account icon wired to Auth.js session, bag icon with
live count from the cart cookie rendered as a Suspense-wrapped dynamic island
inside the otherwise cached layout. Minimal footer.

Code placement follows the seller precedent:
- Components: `src/web/src/components/storefront/`
- Loaders/actions: `src/web/src/lib/storefront/`
- Reuse `src/web/src/lib/catalog/api.ts`, `lib/money.ts`, `lib/pagination.ts`.

Category chips are derived from actual product categories, not hardcoded.

**Caching policy (existing rule: cached ↔ anonymous):** home and PDP are
anonymous cached reads (`"use cache"` + cacheTag, revalidated on product writes,
same as seller read paths). Cart, checkout, and confirmation are dynamic
(cookies/session). Read loaders never import `auth`.

## 5. Backend additions (Catalog.API, all TDD)

1. **Buyer checkout endpoint** — `POST /api/orders/checkout` →
   `PlaceStorefrontOrderCommand`. Existing seller `POST /api/orders`
   (SellerPolicy) is unchanged. The new command:
   - Auto-allocates the order number (max+1; retries on the existing
     unique-index violation — buyers never supply a number).
   - Accepts only `(sku, qty)` lines + shipping/payment/contact details;
     **prices every line from the database**. Client/cookie prices are never
     trusted.
   - Validates inventory and decrements it (`AsTracking` — see EF NoTracking
     memory); rejects quantities over stock.
   - Re-validates the promo code server-side (Status, StartsAt/EndsAt window,
     MinOrderAmount) and computes the discount from Kind/PercentValue/FixedAmount.
2. **Order discount fields** — `DiscountCode` (nullable) + `DiscountAmount` on
   `Order`, one migration (`AddOrderDiscount`), surfaced through `OrderDetailDto`
   and the seller order detail page (hi-fi already shows "Promo · WELCOME10"
   lines — this closes a seller-side gap too).
3. **Buyer order read** — confirmation page fetch under `BuyerPolicy`; the
   handler authorizes by matching the order's `CustomerEmail` to the token's
   email claim (single-tenant simplification, documented).
4. **Category filter + sort** — `GetProductsQuery` gains `category` plus a
   `sort` parameter limited to `price-asc | price-desc`; default order remains
   the existing one (by Id). No date-based "newest" sort — products carry no
   creation timestamp, and adding one is out of scope.
5. **Authorization** — new `BuyerPolicy` = any authenticated user.

## 6. Auth flow

- Keycloak realm import gains `registrationAllowed: true` (built-in sign-up
  page) and a seeded test buyer for e2e. No new roles.
- `/checkout` with no session redirects to Keycloak via the existing Auth.js
  setup; name/email prefill from the session.
- Server actions call the API with the session bearer token, same pattern as
  seller writes.

## 7. Data flow

- Cookie `mc_cart` (httpOnly): `{ lines: [{sku, qty}], promoCode? }`. Mutated
  only by server actions: `addToCart`, `setQty`, `removeLine`, `applyPromo`.
  Prices are fetched from the API at render time; a stale cookie cannot corrupt
  totals.
- Checkout submit (server action): session token → `POST /api/orders/checkout`
  → on success clear `mc_cart` → redirect to confirmation.
- Shipping ($8 standard / $22 express / free local pickup) and flat 8.5% tax are
  demo constants computed in the server action; the API recomputes item prices
  and the discount as the authority. No tax maintenance tests (CLAUDE.md).
- Money math, fixed: discount applies to the item subtotal only (never shipping);
  tax = 8.5% of the discounted subtotal; total = discounted subtotal + shipping
  + tax. A fixed-amount promo caps at the subtotal (no negative totals).

## 8. Error handling

- Unknown SKU, or a confirmation order whose email doesn't match the session →
  `notFound()` (tests assert content, not HTTP status — cacheComponents caveat).
- Add-to-cart on inactive/out-of-stock products → typed server-action error,
  inline message; quantities clamp to available inventory.
- Cart lines whose product disappeared/went inactive render a warning row with
  a remove affordance instead of breaking the page.
- Promo errors are specific and inline: unknown code, expired/not started,
  below minimum order amount.
- Checkout submit: expired session → login redirect; insufficient stock at
  placement → per-line error linking back to the cart; number-allocation
  retries happen inside the API handler (no buyer-visible 409). Form state
  survives failures.
- Empty cart: `/checkout` redirects to `/cart`; `/cart` shows an empty state
  with a Shop CTA.

## 9. Testing

TDD throughout — failing tests first, per repo rules.

- **Application.Tests**: `PlaceStorefrontOrderCommand` — auto-numbering,
  DB-side pricing, promo paths (valid/expired/min-order), inventory decrement +
  insufficient stock, discount math; `GetProductsQuery` category filter.
- **FunctionalTests**: HTTP checkout with a self-minted buyer JWT (existing
  factory pattern), discount round-trip into `OrderDetailDto`, buyer order-read
  email match/mismatch.
- **Vitest**: cart cookie codec, server actions, checkout wizard step logic,
  promo error rendering.
- **Playwright e2e** (serial, per Keycloak memory): buyer storageState setup;
  full-funnel spec — browse → filter → PDP → add → cart + promo → login →
  place order → confirmation — then assert the order appears on the seller
  orders page. Existing seller specs stay green.
- **Done gate**: `npm run lint`, `npm test`, full `dotnet test
  src/MicroCommerce.slnx`, `npm run e2e` against the AppHost, agent-browser
  visual pass, then a manual QA pass per `qa/` conventions.

## 10. Design references

- `design/project/hifi-home.jsx` — `Home_Tiles`
- `design/project/hifi-product.jsx` — `Product_Classic`
- `design/project/hifi-cart.jsx` — `Cart_List`, `Cart_Desktop`
- `design/project/hifi-checkout.jsx` — `Checkout_Steps`, `Checkout_Desktop`
- `DESIGN.md` — typography, spacing, accent rules (single blue accent, glyph
  conventions); verbatim string copy matters, except shop-brand strings which
  render "Micro Commerce" per §3.

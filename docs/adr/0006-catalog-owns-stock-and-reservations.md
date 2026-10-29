# Catalog owns stock and Reservations, behind a synchronous in-process contract

Stock lives on a Variant, so the Catalog module owns it and the Reservation table. Placing a Checkout must fail immediately when a Variant is out of stock or no longer buyable, which events between modules (ADR-0003) cannot do: a Checkout accepted first and refused later would leave the Buyer with a placed Checkout that never existed. So Ordering calls a narrow contract that Catalog exposes inside the process, and this is the one deliberate exception to "modules communicate only through events". The contract passes IDs and plain values only; Ordering never sees Catalog's entities or tables. Everything else between the two modules stays on events.

The contract has four operations:

- **Lookup**, a batch read by Variant ID: unit price, weight, owning `MerchantId`, the quantity currently available, and whether the Variant is buyable at all, which already accounts for Product status and a suspended Merchant.
- **`Reserve`** for a Checkout's lines, all or nothing, taking a `holdUntil` that Ordering computes as the placement time plus the payment window plus a grace period.
- **`Commit`**, called when the Checkout is Paid, which makes the Checkout's Reservations permanent.
- **`Release`**, called when the Checkout expires or an Order is cancelled before shipping.

A Reservation is keyed by `CheckoutId` and moves through states: _Reserved_ (it has a `holdUntil`) → _Held_ (committed, no deadline) → _Consumed_ (the Order shipped) or _Released_. Every operation is idempotent, so a repeated call is a no-op.

A Variant's `Stock` is the on-hand count the Merchant sets, and what is available is `Stock` minus the active Reservations: those that are Held, plus those that are Reserved and not past their `holdUntil`. `Reserve` checks against that and never changes `Stock`; `Stock` drops only when a Reservation is Consumed on shipping. A Merchant lowering `Stock` below the reserved quantity leaves existing Reservations standing and blocks only new ones.

A Reservation refers to its Variant by ID with no foreign key. A Merchant who edits a Product so that a Variant disappears is not blocked, even when that Variant is reserved: the Variant row is deleted, its Reservations stay valid for `Commit` and `Release`, and Consuming one later finds nothing to decrement and does nothing. Held Reservations can last until fulfilment exists, so refusing the edit would block the Merchant indefinitely.

What Buyers see follows availability, not on-hand stock. The storefront's Variant stock and the lists' `InStock` flag report the available quantity, and every `Reserve`, `Commit` and `Release` evicts the cached Product detail (`StorefrontProductCache`). A Reservation that lapses by its `holdUntil` raises no event, so a cached detail can lag by up to the cache's 10-minute lifetime, which is accepted.

## Considered Options

- **Ordering keeps an event-fed copy of Variant data and reserves stock asynchronously** (the pattern behind Catalog's `Shop` read model): rejected for Reservation. Events are at-least-once and unordered, so the copy can be stale at the moment of Checkout, and an asynchronous Reserve means accepting a Checkout before stock is confirmed. A stale copy is tolerable for display, which is why the Shop read model is fine.
- **Ordering writes to Catalog's tables directly**: rejected; it is the coupling ADR-0003 exists to prevent.
- **Stock moves to Ordering**: rejected. A Merchant edits stock together with the rest of a Variant in the Merchant Centre, so splitting it would put one edit across two modules.
- **Reservations with no deadline, released only by the Checkout workflow (ADR-0004)**: rejected. If Ordering's write fails after `Reserve` succeeds, no Checkout and no workflow exist to release it, and the stock would be locked for good. A Reserved row's `holdUntil` frees it by itself, with no sweeper, and `Commit` removes the deadline once the Checkout is Paid.

## Consequences

A Checkout placement touches two modules in one request, but not in one database transaction: Ordering calls `Reserve`, then writes the Checkout and its `CheckoutPlaced` event through its own outbox. If the second step fails, the endpoint releases the Reservation, and if even that fails the `holdUntil` frees the stock anyway. An expired Checkout releases its Reservation explicitly through `Release`, but availability recovers on its own when the deadline passes.

`Reserve` must be atomic across a Checkout's lines and safe under concurrent Checkouts for the last unit. Aspire's retrying execution strategy refuses an explicit transaction unless it wraps the whole unit in the strategy's `ExecuteAsync`, so `Reserve` does that, locking the affected Variant rows in `VariantId` order so two Checkouts can't deadlock.

A Held Reservation stays Held until something Consumes or Releases it. The first slice that creates Orders has no shipping or cancelling, so those units stay reserved until the fulfilment slice adds that.

Because the contract is in-process, extracting Catalog into its own service later turns these calls into network calls and brings back the failure modes they hide today (timeouts, partial failure). Keep the contract small and idempotent so that move stays mechanical.

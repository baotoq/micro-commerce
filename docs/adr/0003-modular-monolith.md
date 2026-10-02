# Modular monolith, extractable by design

Despite the repo's name, MicroCommerce runs as a single API process: Accounts, Catalog, Ordering, Payments and Shipping are modules inside `MicroCommerce.ApiService`, not separate services. Each module owns its own tables and no module reads another's; modules refer to each other only by ID and communicate through events written via a transactional outbox. This lets us learn event-driven integration (e.g. a Completed Order releasing Escrow) without distributed transactions, while leaving any module splittable into its own service later. Deadlines and the Order lifecycle sequence are orchestrated separately (ADR-0004).

## Considered Options

- **Microservices from day one** (service per module, database per service, message broker): rejected for now; the operational cost outweighs the learning value before the domain has settled.
- **Plain monolith with shared tables across features**: rejected; cross-module joins would make later extraction a rewrite.

## Consequences

The external payment provider and Carrier are simulated as *separate* Aspire services that call back via webhooks, so idempotent, out-of-order webhook handling is exercised even though our own code is one process.

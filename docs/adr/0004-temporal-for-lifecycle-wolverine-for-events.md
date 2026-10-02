# Temporal runs the Order lifecycle; Wolverine carries events

Two tools, with a hard split so they never overlap. **Temporal** owns sequence and time: one `CheckoutWorkflow` per Checkout (workflow ID = Checkout ID) with a child workflow per Order, holding every deadline (Payment window, Ship-by deadline, Confirmation period, Merchant response deadline) and receiving payment and Carrier webhooks as signals. **Wolverine**, with PostgreSQL message storage and EF Core transactions, owns the transactional outbox and in-process events between modules; it schedules nothing time-based. Our database stays the source of truth for Checkout and Order status: workflows only drive the sequence by calling module commands as activities, which re-check current status, so duplicate or late steps are no-ops.

The handoff: placing a Checkout writes `CheckoutPlaced` through the outbox, and a Wolverine handler starts the workflow. Reusing the Checkout ID as the workflow ID makes a redelivered event harmless. Webhooks are deduplicated by provider event ID before being signalled.

## Considered Options

- **Wolverine only (sagas with scheduled timeouts)**: fewer moving parts, but the lifecycle is scattered across handlers instead of reading top to bottom; rejected because learning durable workflows is a goal.
- **Temporal only**: rejected. Temporal has no outbox, so persisting a Checkout and starting its workflow could not be atomic.
- **MassTransit or a hand-rolled outbox with Quartz/Hangfire**: rejected in favour of one library that does outbox and messaging on the Postgres we already run.

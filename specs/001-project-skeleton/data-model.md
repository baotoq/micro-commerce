# Data Model: Project Skeleton

**Feature**: [spec.md](./spec.md) | **Research**: [research.md](./research.md)

The skeleton stores almost nothing. It has one demonstration aggregate, one inbox table and a
read-only Account view. The aggregate and the inbox together show how every later aggregate and
event handler is built. These are the conventions from Principle II and research R1–R3 and R6.

## Domain (`src/MicroCommerce.Domain`)

### Building blocks (`SharedKernel/`)

| Type | Purpose |
|------|---------|
| `AggregateRoot<TId>` | Base type. Holds `Id` and a private list of domain events, with `RaiseDomainEvent()`, `DomainEvents` and `ClearDomainEvents()`. Its state is changed only through its own methods. |
| `IDomainEvent` | Marker interface for domain events. Each event carries `OccurredAt` (`DateTimeOffset`). |
| `DomainRuleViolation` (exception) | Thrown when an invariant fails. Its message is written in domain terms, and the API maps it to a 409 or 422 ProblemDetails response. |
| `AccountId` | `[ValueObject<Guid>]`, the Keycloak `sub` value. It is not persisted in this feature. |

The domain uses no ambient clock (`DateTime.UtcNow`). Aggregate methods take `now` as a parameter,
and application code gets it from an injected `TimeProvider`, or from `Workflow.UtcNow` inside a
workflow.

### `SampleProcess` (aggregate root, `Diagnostics/`)

This is a technical demonstration (spec, Key Entities). It is not a Marketplace concept, so it isn't
added to `CONTEXT.md`. It is removed when a real Lifecycle process replaces it.

| Field | Type | Rules |
|-------|------|-------|
| `Id` | `SampleProcessId` (`[ValueObject<Guid>]`, created with `Guid.CreateVersion7()`) | Required and immutable |
| `Delay` | `TimeSpan` | Must be from 1 second to 300 seconds inclusive, enforced in `SampleProcess.Start` |
| `Status` | `SampleProcessStatus` enum: `Running`, `Completed` | Starts as `Running` |
| `StartedAt` | `DateTimeOffset` | Set when the process starts |
| `CompletedAt` | `DateTimeOffset?` | Set once, by `Complete` |

**State transitions**

```text
Start(id, delay, now) ──► Running ──Complete(now)──► Completed
                                         ▲                │
                                         └── Complete(now) again: no-op (idempotent), CompletedAt is kept
```

- The workflow's first activity, `CreateSampleProcess`, creates the aggregate, not the HTTP
  endpoint. That way the record and its timer can't diverge (research R5). The endpoint checks the
  delay with its validator before it starts the workflow, so `Start` rejecting the delay inside the
  activity is only a backstop.
- `static SampleProcess Start(SampleProcessId id, TimeSpan delay, DateTimeOffset now)` validates the
  delay. It throws `DomainRuleViolation("A sample process must wait between 1 and 300 seconds")`.
- `void Complete(DateTimeOffset now)`:
  - If the status is `Running`, it sets `Completed` and `CompletedAt = now`, then raises
    `SampleProcessCompleted(Id, now)`.
  - If the status is already `Completed`, it does nothing. Temporal can retry the activity.
  - If `now < StartedAt + Delay`, it throws a `DomainRuleViolation`. This guards against completing
    early.

**Domain event**: `SampleProcessCompleted(SampleProcessId Id, DateTimeOffset OccurredAt)`.

## Persistence (`src/MicroCommerce.ApiService/Infrastructure/Data`)

There is one `AppDbContext` on the Aspire database `micro-commerce-db`, using the `public` schema
for the skeleton. Migrations live in `Infrastructure/Data/Migrations` and are applied at startup in
Development (FR-004), as in the old codebase. Vogen converters are registered by convention
(research R2).

### Table `sample_processes`

| Column | Type | Notes |
|--------|------|-------|
| `id` | `uuid` PK | `SampleProcessId` |
| `delay` | `interval` | |
| `status` | `text` | The enum stored as a string |
| `started_at` | `timestamptz` | |
| `completed_at` | `timestamptz` null | |

### Table `processed_events` (inbox; FR-012b)

| Column | Type | Notes |
|--------|------|-------|
| `consumer` | `text` | The handler name, for example `sample-process-completed.record-receipt` |
| `message_id` | `text` | The integration event's `eventId` |
| `processed_at` | `timestamptz` | When it was first handled. The status endpoint reports it as `completionEventReceivedAt`. |
| PK | (`consumer`, `message_id`) | A duplicate delivery hits the key, and the handler treats it as already done |

The inbox row is written in the same `SaveChanges` call as the handler's effect. In the skeleton,
the inbox row is the effect, because "record that it received it" means exactly that.

## Integration event (contract: [contracts/events.md](./contracts/events.md))

`SampleProcessCompletedV1 { eventId, sampleProcessId, completedAt }` is published by the
`PublishSampleProcessCompleted` activity after the aggregate is saved. `eventId` is deterministic,
`sample-process-completed:{sampleProcessId}`, so an activity retry publishes the same ID and the
inbox drops the duplicate.

## Account (read-only, not persisted)

`CurrentAccount { AccountId Id, string DisplayName, string? Email, bool IsPlatformOperator }` is
built for each request by `ICurrentAccount` from the validated token. The display name comes from
`name`, falling back to `preferred_username`. `IsPlatformOperator` is true when the `roles` claim
contains `platform-operator`. No commands or queries take an `AccountId` from the request body
(FR-009, ADR-50).

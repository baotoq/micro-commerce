---
phase: 05-event-bus-infrastructure
plan: 01
subsystem: infra
tags: [masstransit, circuit-breaker, retry, idempotency, opentelemetry, dlq, azure-service-bus]

# Dependency graph
requires:
  - phase: 01-foundation
    provides: "MassTransit + EF Core outbox setup, OutboxDbContext"
  - phase: 04-inventory-domain
    provides: "ProductCreatedConsumer to retrofit"
provides:
  - "Global consumer middleware pipeline (circuit breaker, retry, inbox deduplication)"
  - "PermanentException marker for non-retryable errors"
  - "MassTransit OpenTelemetry tracing in Aspire dashboard"
  - "Structured correlation logging pattern for consumers"
  - "Azure Service Bus DLQ routing"
affects: [05-02, 05-03, 06-cart-domain, 07-ordering-domain]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "AddConfigureEndpointsCallback for global consumer middleware"
    - "PermanentException to skip retries for non-transient errors"
    - "BeginScope with CorrelationId/MessageId/ConversationId for consumer logging"

key-files:
  created:
    - "code/MicroCommerce.ApiService/Common/Messaging/Exceptions/PermanentException.cs"
  modified:
    - "code/MicroCommerce.ApiService/Program.cs"
    - "code/MicroCommerce.ServiceDefaults/Extensions.cs"
    - "code/MicroCommerce.ServiceDefaults/MicroCommerce.ServiceDefaults.csproj"
    - "code/MicroCommerce.ApiService/Features/Inventory/Application/Consumers/ProductCreatedConsumer.cs"

key-decisions:
  - "Single AddConfigureEndpointsCallback for all middleware and DLQ config"
  - "Middleware order: circuit breaker (outermost) -> retry -> inbox outbox (innermost)"
  - "MassTransit package added to ServiceDefaults for DiagnosticHeaders access"

patterns-established:
  - "Global middleware via AddConfigureEndpointsCallback: all consumers get circuit breaker, retry, and inbox dedup automatically"
  - "PermanentException pattern: throw to skip retries and go to DLQ"
  - "Consumer correlation logging: BeginScope with CorrelationId, MessageId, ConversationId"

# Metrics
duration: 2min
completed: 2026-02-09
---

# Phase 5 Plan 1: Global MassTransit Middleware Summary

**MassTransit global middleware pipeline with circuit breaker, exponential retry, inbox deduplication, PermanentException bypass, and OpenTelemetry tracing**

## Performance

- **Duration:** 2 min
- **Started:** 2026-02-08T17:10:31Z
- **Completed:** 2026-02-08T17:12:26Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments
- Global consumer middleware pipeline: circuit breaker (1min/15 trips) -> retry (1s, 5s, 25s) -> inbox outbox deduplication
- PermanentException marker class for non-retryable errors that skip retries and go to DLQ
- Azure Service Bus DLQ routing via ConfigureDeadLetterQueueErrorTransport
- 5-minute duplicate detection window on outbox
- MassTransit ActivitySource registered in OpenTelemetry for Aspire distributed traces
- ProductCreatedConsumer retrofitted with CorrelationId/MessageId/ConversationId logging scope

## Task Commits

Each task was committed atomically:

1. **Task 1: Global MassTransit middleware and PermanentException** - `8766ddb8` (feat)
2. **Task 2: OpenTelemetry tracing and consumer retrofit** - `cc029c82` (feat)

## Files Created/Modified
- `code/MicroCommerce.ApiService/Common/Messaging/Exceptions/PermanentException.cs` - Marker exception for non-retryable errors
- `code/MicroCommerce.ApiService/Program.cs` - Global middleware pipeline via AddConfigureEndpointsCallback
- `code/MicroCommerce.ServiceDefaults/Extensions.cs` - MassTransit ActivitySource for OpenTelemetry tracing
- `code/MicroCommerce.ServiceDefaults/MicroCommerce.ServiceDefaults.csproj` - MassTransit package reference for DiagnosticHeaders
- `code/MicroCommerce.ApiService/Features/Inventory/Application/Consumers/ProductCreatedConsumer.cs` - Structured correlation logging scope

## Decisions Made
- Single AddConfigureEndpointsCallback contains DLQ routing, circuit breaker, retry, and inbox outbox -- all in one callback for clarity
- Middleware order: circuit breaker outermost, then retry, then inbox outbox innermost
- Added MassTransit package to ServiceDefaults project for DiagnosticHeaders.DefaultListenerName access
- Kept existing AnyAsync idempotency check in ProductCreatedConsumer as defense-in-depth alongside inbox deduplication

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added MassTransit package to ServiceDefaults**
- **Found during:** Task 2 (OpenTelemetry tracing)
- **Issue:** ServiceDefaults did not reference MassTransit, so MassTransit.Logging.DiagnosticHeaders would not resolve
- **Fix:** Added MassTransit 9.0.0 PackageReference to ServiceDefaults.csproj
- **Files modified:** code/MicroCommerce.ServiceDefaults/MicroCommerce.ServiceDefaults.csproj
- **Verification:** Build succeeds with DiagnosticHeaders.DefaultListenerName reference
- **Committed in:** cc029c82 (Task 2 commit)

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Required for compilation. No scope creep.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Global middleware applied to all consumer endpoints automatically
- New consumers created in future plans will inherit circuit breaker, retry, and inbox deduplication
- PermanentException available for any consumer that encounters non-transient errors
- OpenTelemetry tracing ready for verifying message flow in Aspire dashboard

---
*Phase: 05-event-bus-infrastructure*
*Completed: 2026-02-09*

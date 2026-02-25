# Phase 5: Event Bus Infrastructure - Context

**Gathered:** 2026-02-08
**Status:** Ready for planning

<domain>
## Phase Boundary

Harden the existing MassTransit + Azure Service Bus setup (established in Phase 1) with idempotent consumers, dead-letter queue configuration, correlation tracking, and consumer error handling patterns. Core messaging and transactional outbox already work — this phase makes it production-ready. No new domain features.

</domain>

<decisions>
## Implementation Decisions

### Idempotency strategy
- Use MassTransit's built-in message deduplication mechanism
- Apply idempotency to ALL consumers as a default — safety-first approach
- Implement as a pipeline filter (middleware) — zero-config per consumer
- Retrofit the existing ProductCreatedDomainEvent consumer (Inventory module) as the reference implementation

### Dead-letter handling
- 3 retries before a message goes to the dead-letter queue
- Admin UI page for DLQ visibility — view, retry, and purge actions available
- Structured log at Warning level when a message hits the DLQ — visible in Aspire dashboard
- DLQ entries show CorrelationId to help trace back to the originating action

### Correlation & tracing
- Correlation ID origin: Claude's Discretion (evaluate MassTransit + Aspire best practices)
- CorrelationId included in ALL consumer log entries, not just errors
- Full integration with .NET Aspire distributed tracing (OpenTelemetry) — MassTransit activities visible in Aspire traces view

### Consumer error policy
- Exponential backoff between retries (e.g., 1s, 5s, 25s)
- Circuit breaker enabled — stop consuming temporarily after N consecutive failures
- Separate handling for transient vs permanent errors — transient errors retry, permanent errors go straight to DLQ
- Centralized default configuration with per-consumer overrides if needed

### Claude's Discretion
- Correlation ID origin strategy (HTTP request propagation vs MassTransit auto-generation)
- Circuit breaker thresholds (trip count, recovery interval)
- Exact exponential backoff intervals
- Admin DLQ page layout and component design
- How to classify transient vs permanent errors (exception type mapping)

</decisions>

<specifics>
## Specific Ideas

- Existing ProductCreatedDomainEvent consumer in Inventory module should be the first consumer retrofitted — serves as reference implementation
- Admin DLQ page should show: message type, error details, correlation ID, timestamp, retry/purge actions
- Warning-level structured logs on DLQ entry ensures visibility in Aspire dashboard without custom alerting

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 05-event-bus-infrastructure*
*Context gathered: 2026-02-08*

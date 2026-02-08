---
phase: 05-event-bus-infrastructure
plan: 02
subsystem: messaging
tags: [azure-service-bus, dlq, dead-letter, masstransit, cqrs, mediatr]

# Dependency graph
requires:
  - phase: 05-event-bus-infrastructure/01
    provides: MassTransit middleware with DLQ routing and retry/circuit breaker
  - phase: 01-foundation
    provides: CQRS pattern, MediatR pipeline, MassTransit registration
provides:
  - IDeadLetterQueueService for DLQ browse/retry/purge operations
  - CQRS query/commands for DLQ management
  - Admin API endpoints at /api/messaging/dead-letters
  - DomainEventFaultConsumer for structured warning logs on message faults
  - DeadLetterMessageDto for admin visibility
affects: [06-cart-domain, 07-ordering-domain, admin-ui-messaging]

# Tech tracking
tech-stack:
  added: []
  patterns: [DLQ management service, fault consumer logging, admin messaging endpoints]

key-files:
  created:
    - code/MicroCommerce.ApiService/Common/Messaging/DeadLetterQueueService.cs
    - code/MicroCommerce.ApiService/Common/Messaging/DomainEventFaultConsumer.cs
    - code/MicroCommerce.ApiService/Features/Messaging/Application/GetDeadLetterMessagesQuery.cs
    - code/MicroCommerce.ApiService/Features/Messaging/Application/RetryDeadLetterMessageCommand.cs
    - code/MicroCommerce.ApiService/Features/Messaging/Application/PurgeDeadLetterMessagesCommand.cs
    - code/MicroCommerce.ApiService/Features/Messaging/MessagingEndpoints.cs
  modified:
    - code/MicroCommerce.ApiService/Program.cs

key-decisions:
  - "Aspire AddAzureServiceBusClient for ServiceBusClient registration"
  - "Hardcoded known queue names list (grows with consumers)"
  - "Graceful ServiceBusException handling for emulator compatibility"
  - "Generic open-type DomainEventFaultConsumer<T> for all message types"
  - "RequireAuthorization on entire messaging route group"

patterns-established:
  - "DLQ service pattern: inject ServiceBusClient, create receiver with SubQueue.DeadLetter"
  - "Fault consumer pattern: IConsumer<Fault<T>> with structured warning logging"
  - "Messaging feature module: Application folder with CQRS handlers, endpoints at module root"

# Metrics
duration: 3min
completed: 2026-02-09
---

# Phase 5 Plan 2: DLQ Management Backend Summary

**DLQ management service with peek/retry/purge via Azure Service Bus SDK, CQRS handlers, admin API endpoints, and fault consumer logging**

## Performance

- **Duration:** 3 min
- **Started:** 2026-02-08T17:15:16Z
- **Completed:** 2026-02-08T17:18:02Z
- **Tasks:** 2
- **Files modified:** 7

## Accomplishments
- DeadLetterQueueService with peek, retry, and purge operations using Azure Service Bus SDK
- Three CQRS handlers following project conventions for DLQ operations
- Admin API endpoints at /api/messaging/dead-letters with authorization
- Generic DomainEventFaultConsumer for warning-level structured logging on message faults
- Graceful error handling for Azure Service Bus emulator compatibility

## Task Commits

Each task was committed atomically:

1. **Task 1: DeadLetterQueueService and ServiceBusClient registration** - `23b07849` (feat)
2. **Task 2: CQRS handlers, API endpoints, and fault consumer** - `8c03d633` (feat)

## Files Created/Modified
- `code/MicroCommerce.ApiService/Common/Messaging/DeadLetterQueueService.cs` - IDeadLetterQueueService interface and implementation with peek/retry/purge DLQ operations
- `code/MicroCommerce.ApiService/Common/Messaging/DomainEventFaultConsumer.cs` - Generic fault consumer logging warnings with message type, ID, correlation, exceptions
- `code/MicroCommerce.ApiService/Features/Messaging/Application/GetDeadLetterMessagesQuery.cs` - CQRS query to list DLQ messages from all or specific queues
- `code/MicroCommerce.ApiService/Features/Messaging/Application/RetryDeadLetterMessageCommand.cs` - CQRS command to retry a single DLQ message
- `code/MicroCommerce.ApiService/Features/Messaging/Application/PurgeDeadLetterMessagesCommand.cs` - CQRS command to purge all DLQ messages from a queue
- `code/MicroCommerce.ApiService/Features/Messaging/MessagingEndpoints.cs` - Admin API endpoints with GET/POST routes and authorization
- `code/MicroCommerce.ApiService/Program.cs` - ServiceBusClient registration, DLQ service DI, messaging endpoints mapping

## Decisions Made
- Used Aspire's `AddAzureServiceBusClient("messaging")` to register ServiceBusClient (same connection as MassTransit)
- Hardcoded known queue names list starting with `product-created-domain-event` (extensible as consumers grow)
- Graceful ServiceBusException handling returns empty results instead of failing in emulator environments
- Generic open-type `DomainEventFaultConsumer<T>` auto-discovered by MassTransit assembly scanning
- Applied `RequireAuthorization()` to entire messaging route group for admin-only access

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- DLQ management backend complete and ready for admin UI integration
- Fault consumer will automatically log warnings for any future domain event faults
- Queue names list in DeadLetterQueueService should be extended as new consumers are added in future phases
- Ready for 05-03 plan execution

---
*Phase: 05-event-bus-infrastructure*
*Completed: 2026-02-09*

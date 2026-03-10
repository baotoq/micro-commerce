# MicroCommerce — C# Conventions

- File-scoped namespaces always
- Primary constructors for DI
- Explicit types over `var` (`csharp_style_var_* = false`)
- Collection expressions `[]` over `new List<T>()`
- Private fields: `_camelCase`
- Record types for all DTOs and commands/queries
- FluentValidation for input validation
- Async/await for all I/O (no `.Result` or `.Wait()`)
- `TreatWarningsAsErrors` enabled globally via Directory.Build.props
- Nullable reference types enabled
- UUID v7 for all new entity IDs: `Guid.CreateVersion7()`
- EF Core snake_case naming: `UseSnakeCaseNamingConvention()`
- Strongly typed IDs via Vogen: `[ValueObject<Guid>] public partial record struct ProductId`
- Domain enums via Ardalis.SmartEnum
- Complex query filtering via Ardalis.Specification
- FluentResults for railway-oriented error handling in handlers
- CQRS: Commands/queries via MediatR with pipeline behaviors (ValidationBehavior, ResultValidationBehavior)

## EF Core Interceptors
- AuditInterceptor (CreatedAt/UpdatedAt)
- ConcurrencyInterceptor (Version)
- SoftDeleteInterceptor
- DomainEventInterceptor (publishes domain events via MassTransit after SaveChanges)

## EF Core Model Conventions
- AuditableConvention, ConcurrencyTokenConvention, SoftDeletableConvention
- Auto-configure global query filters (e.g., `WHERE is_deleted = false`)

## Schema-per-Feature
All DbContexts share `appdb` PostgreSQL with separate schemas:
catalog, cart, ordering, inventory, profiles, reviews, wishlists, outbox

## MassTransit
- Outbox: OutboxDbContext in `outbox` schema (inbox dedup + outbox delivery)
- Retry: exponential backoff [1s, 5s, 25s], circuit breaker at 15% failure rate
- Transport: Azure Service Bus (dev/Aspire), RabbitMQ (Kubernetes)

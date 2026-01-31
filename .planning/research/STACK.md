# Stack Research: E-Commerce Microservices

**Domain:** E-commerce microservices (Catalog, Cart, Ordering, Inventory)
**Researched:** 2026-01-29
**Confidence:** HIGH

**Context:** Adding e-commerce services to existing .NET 10 + Aspire 13.1.0 + Next.js 16 + Keycloak foundation. Focus on service communication, CQRS, database-per-service, and API Gateway patterns.

---

## Recommended Stack

### Service Communication (Messaging)

| Technology | Version | Purpose | Why Recommended | Confidence |
|------------|---------|---------|-----------------|------------|
| **MassTransit** | 9.0.0 | Message bus abstraction | Industry standard for .NET messaging. Abstracts transport (Azure Service Bus, RabbitMQ), provides saga/state machine support, transactional outbox, and integrates well with DI. Avoids vendor lock-in. | HIGH |
| MassTransit.Azure.ServiceBus.Core | 9.0.0 | Azure Service Bus transport | Production-grade transport for MassTransit. Supports topics, queues, sessions, and scheduled messages. | HIGH |
| Aspire.Azure.Messaging.ServiceBus | 13.1.0 | Aspire integration | Native Aspire component for Azure Service Bus. Handles connection configuration, health checks, and telemetry. | HIGH |
| Aspire.Hosting.Azure.ServiceBus | 13.1.0 | AppHost integration | Provisions Azure Service Bus resources in Aspire orchestration. Supports emulator for local dev. | HIGH |

**Azure Service Bus Patterns to Implement:**
- **Publish/Subscribe:** Domain events via topics (e.g., `OrderPlaced`, `InventoryReserved`)
- **Competing Consumers:** Scale processing with multiple consumers per queue
- **Saga/Choreography:** Coordinate cross-service workflows (Order -> Inventory -> Payment)
- **Transactional Outbox:** Guarantee message delivery with database consistency

### CQRS Implementation

| Technology | Version | Purpose | Why Recommended | Confidence |
|------------|---------|---------|-----------------|------------|
| **MediatR** | 14.0.0 | In-process mediator | Already in project (13.1.0). Upgrade to 14.0.0 for latest features. Decouples request handling, enables pipeline behaviors for cross-cutting concerns. | HIGH |
| FluentValidation | 12.1.1 | Request validation | Integrates with MediatR pipeline via `IPipelineBehavior`. Expressive, testable validation rules. | HIGH |
| FluentValidation.AspNetCore | 12.1.1 | ASP.NET Core integration | Auto-validation in API endpoints. | HIGH |

**MediatR CQRS Pattern:**
```
Commands (write) → IRequest<Result>     → IRequestHandler<TRequest, TResult>
Queries (read)   → IRequest<TResponse>  → IRequestHandler<TRequest, TResponse>
Events (notify)  → INotification        → INotificationHandler<TNotification>
```

**Pipeline Behaviors (order matters):**
1. `LoggingBehavior` - Log all requests
2. `ValidationBehavior` - FluentValidation integration
3. `TransactionBehavior` - Wrap commands in transactions
4. `PerformanceBehavior` - Track slow requests

### Database-per-Service (PostgreSQL)

| Technology | Version | Purpose | Why Recommended | Confidence |
|------------|---------|---------|-----------------|------------|
| **Npgsql.EntityFrameworkCore.PostgreSQL** | 10.0.0 | EF Core PostgreSQL provider | Native .NET 10 support. Production-proven, excellent performance. | HIGH |
| Microsoft.EntityFrameworkCore | 10.0.2 | ORM | Standard .NET ORM. Pairs with Aspire, supports migrations, complex queries. | HIGH |
| Aspire.Npgsql.EntityFrameworkCore.PostgreSQL | 13.1.0 | Aspire integration | Connection pooling, health checks, telemetry. Automatic configuration. | HIGH |
| Aspire.Hosting.PostgreSQL | 13.1.0 | AppHost integration | Provisions PostgreSQL containers. Supports pgAdmin, pgvector extensions. | HIGH |

**Database-per-Service Strategy:**

| Service | Database | Schema Strategy | Rationale |
|---------|----------|-----------------|-----------|
| Catalog | `catalog_db` | Single schema | Read-heavy, simple queries |
| Cart | `cart_db` | Single schema | Session-based, high churn |
| Ordering | `ordering_db` | Multi-schema (orders, sagas) | Complex aggregates, saga state |
| Inventory | `inventory_db` | Single schema | Transactional consistency critical |

**Migration Strategy:**
- EF Core migrations per service
- Idempotent migrations for CI/CD
- Separate migration project per service

### API Gateway

| Technology | Version | Purpose | Why Recommended | Confidence |
|------------|---------|---------|-----------------|------------|
| **YARP (Yet Another Reverse Proxy)** | 2.3.0 | API Gateway/BFF | Microsoft-backed, high performance, config-driven routing. First-class .NET citizen. Integrates with Aspire service discovery. | HIGH |
| Microsoft.Extensions.ServiceDiscovery | 10.0.0 | Service discovery | Already in ServiceDefaults. YARP uses this for dynamic backend resolution. | HIGH |

**YARP Configuration Pattern:**
```csharp
// In Aspire AppHost - YARP as gateway service
var gateway = builder.AddProject<Projects.Gateway>("gateway")
    .WithReference(catalogApi)
    .WithReference(orderingApi)
    .WithReference(cartApi);
```

**Routing Strategy:**
- `/api/catalog/*` → Catalog Service
- `/api/cart/*` → Cart Service
- `/api/orders/*` → Ordering Service
- `/api/inventory/*` → Inventory Service (internal only)

### Supporting Libraries

| Library | Version | Purpose | When to Use | Confidence |
|---------|---------|---------|-------------|------------|
| **Ardalis.GuardClauses** | 5.0.0 | Guard clauses | Already in project. Domain validation, fail-fast patterns. | HIGH |
| **Ardalis.Result** | 10.1.0 | Result pattern | Explicit success/failure returns. Railway-oriented programming. Avoids exceptions for control flow. | HIGH |
| Scrutor | 7.0.0 | Assembly scanning | Auto-register handlers, validators, decorators by convention. Reduces boilerplate. | HIGH |
| Mapster | 7.4.0 | Object mapping | Lightweight alternative to AutoMapper. Faster, compile-time codegen option. Use for DTO mapping. | MEDIUM |
| Polly | 8.6.5 | Resilience | Already via `Microsoft.Extensions.Http.Resilience`. Use for retry, circuit breaker, timeout policies. | HIGH |
| Microsoft.Extensions.Http.Resilience | 10.2.0 | HTTP resilience | Upgrade from 10.0.0. Standard resilience for HTTP calls between services. | HIGH |

### Event Sourcing (Optional, for Ordering)

| Library | Version | Purpose | When to Use | Confidence |
|---------|---------|---------|-------------|------------|
| Marten | 8.18.3 | Event store + document DB | If implementing event sourcing for Order aggregate. Uses PostgreSQL as event store. | MEDIUM |
| EventStoreDB Client | 23.3.9 | Dedicated event store | If dedicated event store preferred over PostgreSQL. More operational overhead. | LOW |

**Recommendation:** Start with simple CRUD + domain events. Add event sourcing later if audit trail or temporal queries needed.

### Domain-Driven Design

| Library | Version | Purpose | When to Use | Confidence |
|---------|---------|---------|-------------|------------|
| StronglyTypedId | 1.0.0-beta08 | Strongly-typed IDs | Type-safe entity IDs (OrderId, ProductId). Prevents ID mixups. Source-generated. | MEDIUM |

---

## Alternatives Considered

### Service Communication

| Recommended | Alternative | When to Use Alternative |
|-------------|-------------|-------------------------|
| **MassTransit** | Rebus 8.9.0 | Simpler needs, less abstraction. MassTransit is more feature-rich (sagas, outbox). |
| **MassTransit** | DotNetCore.CAP 10.0.1 | Lightweight outbox-only pattern. Less control over message handling. |
| **MassTransit** | WolverineFx 5.12.1 | If using Marten for event sourcing. Tight Marten integration. Less mature than MassTransit. |
| **MassTransit** | Raw Azure.Messaging.ServiceBus 7.20.1 | Maximum control needed. Lose saga, outbox, retry abstractions. |
| **MassTransit** | Dapr 1.16.1 | Multi-language microservices or Kubernetes-native. Adds sidecar complexity. |

**Verdict:** MassTransit is the standard choice for .NET microservices. Mature, well-documented, production-proven. Provides saga state machines and transactional outbox out of the box.

### CQRS / Mediator

| Recommended | Alternative | When to Use Alternative |
|-------------|-------------|-------------------------|
| **MediatR** | WolverineFx 5.12.1 | Combined mediator + message bus. More opinionated, less mature. |
| **MediatR** | Raw handlers | Very simple CRUD. MediatR adds ~1ms overhead but provides extensibility. |

**Verdict:** MediatR is the de facto standard. 14.0.0 release (Dec 2025) is current. Project already uses it.

### API Gateway

| Recommended | Alternative | When to Use Alternative |
|-------------|-------------|-------------------------|
| **YARP** | Ocelot | Legacy projects. YARP is Microsoft's recommended successor. |
| **YARP** | Azure API Management | Enterprise governance, monetization, developer portal needed. |
| **YARP** | Kong/Envoy | Multi-language polyglot, Kubernetes service mesh. |
| **YARP** | No gateway (direct) | Simple topology, < 3 services. Direct service-to-service OK. |

**Verdict:** YARP is the modern .NET choice. Native integration with Aspire service discovery. High performance, flexible configuration.

### Result Pattern

| Recommended | Alternative | When to Use Alternative |
|-------------|-------------|-------------------------|
| **Ardalis.Result** | ErrorOr 2.0.1 | Prefer discriminated unions. More functional style. |
| **Ardalis.Result** | OneOf 3.0.271 | F#-style exhaustive matching. Steeper learning curve. |
| **Ardalis.Result** | FluentResults | More verbose, comprehensive error chains. |

**Verdict:** Ardalis.Result is simple, well-maintained, works with ASP.NET Core. ErrorOr is valid alternative if team prefers functional patterns.

---

## What NOT to Use

| Avoid | Why | Use Instead |
|-------|-----|-------------|
| **Ocelot** (API Gateway) | Maintenance mode, YARP is successor | YARP 2.3.0 |
| **NServiceBus** (unless licensed) | Commercial license required for production | MassTransit (MIT) |
| **AutoMapper** | Performance overhead, runtime reflection | Mapster (codegen) or manual mapping |
| **Raw Azure SDK for messaging** | No saga, outbox, retry patterns | MassTransit with Azure transport |
| **Shared database** | Defeats microservice independence | Database-per-service |
| **Synchronous HTTP for events** | Coupling, availability dependency | Async messaging (Service Bus) |
| **Two-phase commit (2PC)** | Distributed transaction anti-pattern | Saga pattern with eventual consistency |
| **Entity Framework migrations in production code** | Deployment coupling | Separate migration runner/job |

---

## Installation

### AppHost (Aspire Orchestration)

```bash
# Add to MicroCommerce.AppHost.csproj
dotnet add package Aspire.Hosting.PostgreSQL --version 13.1.0
dotnet add package Aspire.Hosting.Azure.ServiceBus --version 13.1.0
```

### Service Projects (Catalog, Cart, Ordering, Inventory)

```bash
# Core messaging
dotnet add package MassTransit --version 9.0.0
dotnet add package MassTransit.Azure.ServiceBus.Core --version 9.0.0

# Aspire integrations
dotnet add package Aspire.Azure.Messaging.ServiceBus --version 13.1.0
dotnet add package Aspire.Npgsql.EntityFrameworkCore.PostgreSQL --version 13.1.0

# CQRS pipeline
dotnet add package MediatR --version 14.0.0
dotnet add package FluentValidation --version 12.1.1
dotnet add package FluentValidation.DependencyInjectionExtensions --version 12.1.1

# Supporting
dotnet add package Ardalis.Result --version 10.1.0
dotnet add package Scrutor --version 7.0.0
dotnet add package Mapster --version 7.4.0
```

### API Gateway Project

```bash
dotnet add package Yarp.ReverseProxy --version 2.3.0
```

### BuildingBlocks Update

```bash
# Upgrade existing MediatR
dotnet add package MediatR --version 14.0.0
```

---

## Version Compatibility Matrix

| Package | Requires | Notes |
|---------|----------|-------|
| Aspire.* 13.1.0 | .NET 8.0+ | Supports .NET 10, already in project |
| MassTransit 9.0.0 | .NET 8.0+ | Jan 2026 release, stable |
| MediatR 14.0.0 | .NET 8.0+ | Dec 2025 release, breaking changes from 12.x |
| Npgsql.EFCore 10.0.0 | EF Core 10.0.x | Must match EF Core major version |
| FluentValidation 12.1.1 | .NET 8.0+ | Dec 2025 release |
| YARP 2.3.0 | .NET 8.0+ | Feb 2025 release, stable |

**Critical:** MediatR 14.0.0 has API changes from 13.x. Review migration guide before upgrading.

---

## Stack Patterns by Service

### Catalog Service
- **Pattern:** Read-heavy CQRS (separate read models optional)
- **Database:** PostgreSQL with read replicas if needed
- **Messaging:** Publishes `ProductCreated`, `ProductUpdated`, `PriceChanged`
- **Special:** Consider caching layer (Redis) for product queries

### Cart Service
- **Pattern:** Session-based, high write throughput
- **Database:** PostgreSQL or Redis (if ephemeral carts acceptable)
- **Messaging:** Publishes `CartCheckedOut`, subscribes to `ProductPriceChanged`
- **Special:** Optimistic concurrency for cart updates

### Ordering Service
- **Pattern:** Full CQRS, saga orchestration
- **Database:** PostgreSQL with saga state tables
- **Messaging:** Orchestrates order saga, publishes `OrderPlaced`, `OrderCompleted`
- **Special:** Transactional outbox critical for order reliability

### Inventory Service
- **Pattern:** Transactional consistency critical
- **Database:** PostgreSQL with row-level locking
- **Messaging:** Subscribes to `OrderPlaced`, publishes `InventoryReserved`, `InventoryReleased`
- **Special:** Idempotent reservation handling (saga compensation)

---

## Architecture Decision Records

### ADR-001: MassTransit over raw Azure Service Bus SDK
**Decision:** Use MassTransit as messaging abstraction
**Rationale:**
- Saga state machine support out of box
- Transactional outbox pattern built-in
- Transport-agnostic (can switch to RabbitMQ for local dev)
- Mature ecosystem, extensive documentation
**Trade-off:** Additional abstraction layer, learning curve for saga DSL

### ADR-002: YARP over Ocelot for API Gateway
**Decision:** Use YARP 2.3.0 as API Gateway
**Rationale:**
- Microsoft-maintained, active development
- Native Aspire service discovery integration
- Better performance than Ocelot
- Configuration-driven, supports hot reload
**Trade-off:** Less out-of-box features than Ocelot (rate limiting needs custom middleware)

### ADR-003: Database-per-service from start
**Decision:** Separate PostgreSQL databases per service
**Rationale:**
- Enforces service boundaries
- Independent scaling and deployment
- Prevents data coupling
- Aspire makes multi-DB orchestration easy
**Trade-off:** Cross-service queries require API calls or event-driven projections

### ADR-004: MediatR for in-process, MassTransit for cross-process
**Decision:** Dual mediator pattern
**Rationale:**
- MediatR: In-process command/query handling with pipeline behaviors
- MassTransit: Cross-service async messaging with saga support
- Clear separation of concerns
- Both are industry standards
**Trade-off:** Two patterns to learn, but each optimized for its use case

---

## Sources

**Verified via NuGet Gallery (HIGH confidence):**
- MediatR 14.0.0 — Released Dec 3, 2025
- MassTransit 9.0.0 — Released Jan 6, 2026
- Azure.Messaging.ServiceBus 7.20.1 — Released Jun 12, 2025
- YARP.ReverseProxy 2.3.0 — Released Feb 27, 2025
- Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0 — Released Nov 22, 2025
- Microsoft.EntityFrameworkCore 10.0.2 — Released Jan 13, 2026
- FluentValidation 12.1.1 — Released Dec 3, 2025
- Polly 8.6.5 — Released Nov 23, 2025
- Marten 8.18.3 — Released Jan 28, 2026
- Ardalis.Result 10.1.0 — Released Oct 28, 2024
- Scrutor 7.0.0 — Released Nov 24, 2025
- Aspire.* 13.1.0 — Released Dec 17, 2025

**Verified via Official Documentation (HIGH confidence):**
- MassTransit patterns: https://masstransit.io/documentation/concepts
- MediatR wiki: https://github.com/jbogard/MediatR/wiki

**Project Analysis (HIGH confidence):**
- Existing stack: .NET 10, Aspire 13.1.0, MediatR 13.1.0, Ardalis.GuardClauses 5.0.0

---

*Stack research for: MicroCommerce e-commerce microservices*
*Researched: 2026-01-29*

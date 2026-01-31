# Phase 1: Foundation & Project Structure - Research

**Researched:** 2026-01-29
**Domain:** Modular Monolith Architecture, CQRS, Domain Events with Transactional Outbox
**Confidence:** HIGH

## Summary

This phase establishes the modular monolith architecture with clean architecture layers, separate DbContexts per module, MediatR-based CQRS, FluentValidation pipeline, and Service Bus with transactional outbox for domain events. The existing codebase has MediatR 13.1.0 in BuildingBlocks and .NET 10 with Aspire 13.1.0.

Key findings:
- **MediatR 14.0.0** is the latest version (Dec 2025), but existing MediatR 13.1.0 is compatible
- **MassTransit 9.0.0** (Jan 2026) supports .NET 10, provides Azure Service Bus transport and EF Core outbox
- **FluentValidation 12.1.1** (Dec 2025) is current, requires upgrade guide review for v12
- Aspire has native Azure Service Bus integration via `Aspire.Hosting.Azure.ServiceBus`

The existing BuildingBlocks have solid DDD primitives but need adaptation: current `MediatorDomainEventDispatcher` dispatches via MediatR, but decision is to use Service Bus with outbox pattern from Phase 1.

**Primary recommendation:** Use MassTransit 9.0.0 with EF Core outbox for transactional domain events, integrate with Aspire's Azure Service Bus hosting, and implement FluentValidation as MediatR pipeline behavior.

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| MediatR | 13.1.0 (existing) or 14.0.0 | In-process CQRS mediator | Industry standard for .NET CQRS, already integrated |
| FluentValidation | 12.1.1 | Request validation | Most popular .NET validation library, MediatR pipeline compatible |
| MassTransit | 9.0.0 | Message bus abstraction with outbox | Leading .NET service bus, has transactional outbox built-in |
| MassTransit.Azure.ServiceBus.Core | 9.0.0 | Azure Service Bus transport | Native Azure Service Bus support |
| MassTransit.EntityFrameworkCore | 9.0.0 | EF Core outbox persistence | Transactional outbox tables and delivery service |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.x | PostgreSQL EF Core provider | Aspire default database |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Aspire.Hosting.Azure.ServiceBus | 13.1.0 | Service Bus hosting | AppHost resource configuration |
| Aspire.Azure.Messaging.ServiceBus | 13.1.0 | Service Bus client | API service client integration |
| FluentValidation.DependencyInjectionExtensions | 12.1.1 | Auto-registration | Scan assemblies for validators |
| Ardalis.GuardClauses | 5.0.0 (existing) | Guard methods | Domain validation in entities |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| MassTransit outbox | Custom outbox | MassTransit is battle-tested, handles delivery, retries, dead-letter |
| MassTransit | NServiceBus | NServiceBus is commercial, MassTransit is OSS with same features |
| FluentValidation | DataAnnotations | FluentValidation is more powerful, testable, and composable |
| Multiple DbContexts | Single DbContext | Multiple contexts enforce module boundaries, enable independent migrations |

**Installation:**
```bash
# API Service
dotnet add package MediatR --version 14.0.0
dotnet add package FluentValidation --version 12.1.1
dotnet add package FluentValidation.DependencyInjectionExtensions --version 12.1.1
dotnet add package MassTransit --version 9.0.0
dotnet add package MassTransit.Azure.ServiceBus.Core --version 9.0.0
dotnet add package MassTransit.EntityFrameworkCore --version 9.0.0
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 10.0.0
dotnet add package Aspire.Azure.Messaging.ServiceBus --version 13.1.0

# AppHost
dotnet add package Aspire.Hosting.Azure.ServiceBus --version 13.1.0
dotnet add package Aspire.Hosting.PostgreSQL --version 13.1.0
```

## Architecture Patterns

### Recommended Project Structure
```
code/
├── MicroCommerce.ApiService/
│   ├── Features/
│   │   ├── Catalog/
│   │   │   ├── Domain/
│   │   │   │   ├── Entities/
│   │   │   │   │   └── Product.cs
│   │   │   │   ├── ValueObjects/
│   │   │   │   │   └── ProductName.cs
│   │   │   │   └── Events/
│   │   │   │       └── ProductCreatedDomainEvent.cs
│   │   │   ├── Application/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CreateProduct/
│   │   │   │   │   │   ├── CreateProductCommand.cs
│   │   │   │   │   │   ├── CreateProductCommandHandler.cs
│   │   │   │   │   │   └── CreateProductCommandValidator.cs
│   │   │   │   └── Queries/
│   │   │   │       └── GetProduct/
│   │   │   │           ├── GetProductQuery.cs
│   │   │   │           ├── GetProductQueryHandler.cs
│   │   │   │           └── ProductDto.cs
│   │   │   └── Infrastructure/
│   │   │       ├── CatalogDbContext.cs
│   │   │       └── Repositories/
│   │   │           └── ProductRepository.cs
│   │   ├── Cart/
│   │   │   ├── Domain/
│   │   │   ├── Application/
│   │   │   └── Infrastructure/
│   │   ├── Ordering/
│   │   │   ├── Domain/
│   │   │   ├── Application/
│   │   │   └── Infrastructure/
│   │   └── Inventory/
│   │       ├── Domain/
│   │       ├── Application/
│   │       └── Infrastructure/
│   ├── Common/
│   │   ├── Behaviors/
│   │   │   └── ValidationBehavior.cs
│   │   └── Persistence/
│   │       └── OutboxDbContext.cs
│   ├── Program.cs
│   └── MicroCommerce.ApiService.csproj
├── BuildingBlocks/
│   └── BuildingBlocks.Common/
│       ├── BaseAggregateRoot.cs
│       ├── IAggregateRoot.cs
│       ├── ValueObject.cs
│       ├── StronglyTypedId.cs
│       └── Events/
│           ├── DomainEvent.cs
│           └── IDomainEvent.cs
└── MicroCommerce.AppHost/
    └── AppHost.cs
```

### Pattern 1: MediatR Validation Pipeline Behavior
**What:** Intercept all requests, find validators, fail fast with validation errors
**When to use:** All commands and queries requiring input validation
**Example:**
```csharp
// Source: MediatR Wiki - Behaviors
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
```

### Pattern 2: Module DbContext with Independent Migrations
**What:** Each module has its own DbContext managing its own tables
**When to use:** Every feature module (Catalog, Cart, Ordering, Inventory)
**Example:**
```csharp
// Source: EF Core DbContext Configuration docs
public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations from this module only
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CatalogDbContext).Assembly,
            t => t.Namespace?.Contains("Catalog") == true);

        // Schema isolation (optional but recommended)
        modelBuilder.HasDefaultSchema("catalog");
    }
}
```

### Pattern 3: Transactional Outbox with MassTransit
**What:** Domain events saved to outbox table within same transaction as aggregate changes
**When to use:** Publishing domain events that must be guaranteed delivery
**Example:**
```csharp
// Source: MassTransit EF Outbox docs
// In DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Add MassTransit outbox tables
    modelBuilder.AddInboxStateEntity();
    modelBuilder.AddOutboxMessageEntity();
    modelBuilder.AddOutboxStateEntity();
}

// In Program.cs / DI setup
services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<CatalogDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();  // Enable delivery service
    });

    x.UsingAzureServiceBus((context, cfg) =>
    {
        cfg.Host(connectionString);
        cfg.ConfigureEndpoints(context);
    });
});
```

### Pattern 4: CQRS Command/Query Structure
**What:** Strict separation of commands (writes) and queries (reads)
**When to use:** Every operation in the application
**Example:**
```csharp
// Command - modifies state
public record CreateProductCommand(
    string Name,
    decimal Price,
    Guid CategoryId) : IRequest<ProductId>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductId>
{
    private readonly CatalogDbContext _context;

    public CreateProductCommandHandler(CatalogDbContext context) => _context = context;

    public async Task<ProductId> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = Product.Create(
            ProductName.From(request.Name),
            Money.From(request.Price),
            CategoryId.From(request.CategoryId));

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}

// Query - reads state, no side effects
public record GetProductQuery(ProductId Id) : IRequest<ProductDto?>;

public class GetProductQueryHandler : IRequestHandler<GetProductQuery, ProductDto?>
{
    private readonly CatalogDbContext _context;

    public GetProductQueryHandler(CatalogDbContext context) => _context = context;

    public async Task<ProductDto?> Handle(
        GetProductQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Products
            .Where(p => p.Id == request.Id)
            .Select(p => new ProductDto(p.Id.Value, p.Name.Value, p.Price.Amount))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
```

### Pattern 5: Domain Event with Outbox Publishing
**What:** Aggregate raises event, handler publishes to outbox (same transaction)
**When to use:** Cross-module communication, eventual consistency
**Example:**
```csharp
// Domain Event (thin - IDs only)
public record ProductCreatedDomainEvent(ProductId ProductId) : DomainEvent;

// Aggregate raises event
public class Product : BaseAggregateRoot<ProductId>
{
    public static Product Create(ProductName name, Money price, CategoryId categoryId)
    {
        var product = new Product(ProductId.New())
        {
            Name = name,
            Price = price,
            CategoryId = categoryId
        };

        product.AddDomainEvent(new ProductCreatedDomainEvent(product.Id));
        return product;
    }
}

// EF Core SaveChanges interceptor dispatches events
public class DomainEventDispatcherInterceptor : SaveChangesInterceptor
{
    private readonly IPublishEndpoint _publishEndpoint;

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null) return result;

        var aggregates = context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var events = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        aggregates.ForEach(a => a.ClearDomainEvents());

        foreach (var domainEvent in events)
        {
            // Published to outbox table, delivered by MassTransit delivery service
            await _publishEndpoint.Publish(domainEvent, cancellationToken);
        }

        return result;
    }
}
```

### Anti-Patterns to Avoid
- **Shared DbContext across modules:** Couples modules, prevents independent deployment later
- **Direct cross-module references in handlers:** Use domain events for cross-module communication
- **Validation in handlers:** Move to pipeline behavior for consistency
- **Fat domain events:** Events should carry IDs only, consumers fetch details
- **Synchronous cross-module calls:** Can cause cascading failures, use events instead
- **Multiple handlers per command:** One command = one handler, no ambiguity

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Validation pipeline | Custom middleware | MediatR IPipelineBehavior | Registration order, generic constraints handled |
| Outbox pattern | Custom outbox table + worker | MassTransit EF Outbox | Handles delivery, retries, dead-letter, concurrency |
| Value object equality | Manual Equals/GetHashCode | Existing ValueObject base class | Complex with proxies, caching |
| Strongly typed IDs | Manual wrapper classes | Existing StronglyTypedId<T> | EF Core converters, JSON serialization |
| Service Bus abstraction | Direct Azure SDK | MassTransit | Transport agnostic, testing, retry policies |
| Request/response logging | Manual logging | MediatR LoggingBehavior | Consistent, non-invasive |

**Key insight:** MassTransit's outbox handles edge cases that take months to discover: concurrent delivery, ordered delivery, exactly-once processing, dead-letter handling. Don't rebuild this.

## Common Pitfalls

### Pitfall 1: Forgetting to Register Validators
**What goes wrong:** Validation behavior finds no validators, requests pass through unvalidated
**Why it happens:** FluentValidation validators not scanned from assemblies
**How to avoid:** Use `AddValidatorsFromAssemblyContaining<T>()` in DI setup
**Warning signs:** Invalid data in database, no validation errors returned

### Pitfall 2: DbContext Thread Safety Violations
**What goes wrong:** `InvalidOperationException: A second operation started on this context`
**Why it happens:** DbContext used across multiple async operations without await
**How to avoid:** Always await DbContext operations immediately, use DbContextFactory for parallel work
**Warning signs:** Intermittent exceptions under load

### Pitfall 3: Outbox Tables Not in Migrations
**What goes wrong:** MassTransit can't store/retrieve outbox messages, events lost
**Why it happens:** Forgot to add `modelBuilder.AddOutboxMessageEntity()` etc.
**How to avoid:** Add all three outbox entities in OnModelCreating, verify with migration
**Warning signs:** Foreign key errors, missing table errors on startup

### Pitfall 4: Validation Behavior Order
**What goes wrong:** Validation runs after logging/timing behaviors, clutters logs
**Why it happens:** MediatR behaviors execute in registration order
**How to avoid:** Register ValidationBehavior first in AddMediatR configuration
**Warning signs:** Log entries for requests that immediately fail validation

### Pitfall 5: PostgreSQL Outbox Concurrency
**What goes wrong:** Optimistic concurrency failures in outbox
**Why it happens:** PostgreSQL uses `xmin` for row versioning, not byte[] RowVersion
**How to avoid:** Use `o.UsePostgres()` in outbox configuration, map RowVersion to xmin
**Warning signs:** Concurrency exceptions during high-throughput publishing

### Pitfall 6: Module DbContext Migration Conflicts
**What goes wrong:** Migrations from different modules interfere
**Why it happens:** All DbContexts in same project without schema/migration separation
**How to avoid:** Use schema prefixes (`catalog`, `cart`) and specify `--context` in migrations
**Warning signs:** Migration trying to drop tables from other module

### Pitfall 7: Events Dispatched Before SaveChanges
**What goes wrong:** Events published but aggregate changes rolled back
**Why it happens:** Publishing events before transaction commits
**How to avoid:** Use SaveChangesInterceptor that publishes AFTER successful save
**Warning signs:** Event consumers see events for data that doesn't exist

## Code Examples

Verified patterns from official sources:

### MediatR Registration with Behaviors
```csharp
// Source: MediatR Wiki
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();

    // Order matters - validation first
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
});
```

### FluentValidation DI Registration
```csharp
// Source: FluentValidation docs
services.AddValidatorsFromAssemblyContaining<Program>();
// Or for specific assembly:
services.AddValidatorsFromAssemblyContaining<CreateProductCommandValidator>();
```

### FluentValidation Command Validator
```csharp
// Source: FluentValidation Built-in Validators docs
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .PrecisionScale(18, 2, false);

        RuleFor(x => x.CategoryId)
            .NotEmpty();
    }
}
```

### MassTransit with Azure Service Bus and EF Outbox
```csharp
// Source: MassTransit docs
services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<CatalogDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
        o.QueryDelay = TimeSpan.FromSeconds(1);
    });

    x.UsingAzureServiceBus((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("messaging"));

        cfg.ConfigureEndpoints(context);
    });
});
```

### Aspire AppHost with Service Bus
```csharp
// Source: Aspire Azure Service Bus docs
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var catalogDb = postgres.AddDatabase("catalogdb");

var serviceBus = builder.AddAzureServiceBus("messaging")
    .RunAsEmulator();  // For local dev

var apiService = builder.AddProject<Projects.MicroCommerce_ApiService>("apiservice")
    .WithReference(catalogDb)
    .WithReference(serviceBus);
```

### EF Core Multiple DbContext Registration
```csharp
// Source: EF Core DbContext Configuration docs
services.AddDbContext<CatalogDbContext>((sp, options) =>
{
    options.UseNpgsql(
        connectionString,
        npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "catalog"));
});

services.AddDbContext<OrderingDbContext>((sp, options) =>
{
    options.UseNpgsql(
        connectionString,
        npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "ordering"));
});
```

### Creating Migrations for Specific Context
```bash
# Source: EF Core Migrations docs
# Catalog module migrations
dotnet ef migrations add InitialCatalog --context CatalogDbContext --output-dir Features/Catalog/Infrastructure/Migrations

# Ordering module migrations
dotnet ef migrations add InitialOrdering --context OrderingDbContext --output-dir Features/Ordering/Infrastructure/Migrations
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| MediatR in-process events | Service Bus with outbox | Decision for this project | Guaranteed delivery, durability |
| Single monolithic DbContext | DbContext per module | Modern modular monolith pattern | Better isolation, independent evolution |
| Manual validation in handlers | Pipeline behavior validation | MediatR 9+ behaviors | Consistent, DRY validation |
| Custom outbox implementation | MassTransit built-in outbox | MassTransit 8+ | Production-ready, fewer bugs |
| RowVersion byte[] | PostgreSQL xmin column | MassTransit PostgreSQL support | Correct concurrency handling |

**Deprecated/outdated:**
- `FluentValidation.AspNetCore` auto-validation: No longer recommended, use manual or MediatR pipeline
- `INotificationHandler` for domain events: Use proper event bus for durability
- MediatR 12 and below: Version 13+ has improved behavior registration

## Open Questions

Things that couldn't be fully resolved:

1. **Shared outbox tables vs per-module outbox**
   - What we know: MassTransit outbox can use any DbContext
   - What's unclear: Whether to have one shared outbox DbContext or outbox tables in each module's DbContext
   - Recommendation: Start with shared outbox DbContext (simpler), migrate if needed

2. **Domain event handler location**
   - What we know: Events are thin (IDs only), handlers fetch data
   - What's unclear: Should handlers live in publishing module or consuming module?
   - Recommendation: Handlers live in consuming module (Cart handles ProductCreatedDomainEvent)

3. **MediatR 14 vs existing 13.1.0**
   - What we know: 14.0.0 released Dec 2025, 13.1.0 is in BuildingBlocks
   - What's unclear: Breaking changes between versions
   - Recommendation: Stay with 13.1.0 unless specific 14.0 features needed

## Sources

### Primary (HIGH confidence)
- [MediatR Wiki - Behaviors](https://github.com/jbogard/MediatR/wiki/Behaviors) - Pipeline behavior implementation
- [MassTransit EF Core Outbox](https://masstransit.io/documentation/configuration/middleware/outbox) - Transactional outbox setup
- [MassTransit EF Core Persistence](https://masstransit.io/documentation/configuration/persistence/entity-framework) - DbContext configuration
- [MassTransit Azure Service Bus](https://masstransit.io/documentation/transports/azure-service-bus) - Transport setup
- [FluentValidation Docs](https://docs.fluentvalidation.net/en/latest/) - Validator creation and rules
- [EF Core DbContext Configuration](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/) - Multiple contexts, DI patterns
- [Aspire Azure Service Bus](https://aspire.dev/integrations/cloud/azure/azure-service-bus/) - Aspire integration

### Secondary (MEDIUM confidence)
- [NuGet MassTransit 9.0.0](https://www.nuget.org/packages/MassTransit) - Version and dependencies verified
- [NuGet FluentValidation 12.1.1](https://www.nuget.org/packages/FluentValidation) - Version verified
- [NuGet MediatR 14.0.0](https://www.nuget.org/packages/MediatR) - Version verified
- [EF Core Migrations with Multiple Contexts](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/projects) - Migration separation

### Tertiary (LOW confidence)
- Modular monolith folder structure: Based on common patterns, not official source

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - Verified from NuGet and official docs
- Architecture: HIGH - Patterns from official MassTransit/MediatR docs
- Pitfalls: MEDIUM - Mix of documented issues and common knowledge

**Research date:** 2026-01-29
**Valid until:** 2026-02-28 (30 days - libraries are stable)

## Existing Codebase Audit

### BuildingBlocks Assessment

**BaseAggregateRoot<TId>** - KEEP, minor enhancement needed
- Good: Has domain events collection, ClearDomainEvents
- Issue: Uses `DomainEvent` concrete type, consider using `IDomainEvent` interface
- Works with outbox pattern

**ValueObject** - KEEP as-is
- Good: Full equality, comparison, proxy handling
- Good: References established pattern from Vladimir Khorikov

**StronglyTypedId<T>** - KEEP as-is
- Good: Simple record-based implementation
- Needs: EF Core value converters for each concrete ID type

**DomainEvent** - KEEP, rename for suffix convention
- Current: `DomainEvent` base class
- Decision: Events should have `DomainEvent` suffix (e.g., `ProductCreatedDomainEvent`)
- Base class name is fine, concrete events need suffix

**MediatorDomainEventDispatcher** - REPLACE
- Current: Dispatches via MediatR.Publish (in-process)
- Decision: Use Service Bus with outbox
- Action: Remove this class, use MassTransit IPublishEndpoint instead

**IDomainEventHandler** - REPLACE
- Current: Wraps INotificationHandler
- Decision: Use MassTransit consumers instead
- Action: Remove, create MassTransit consumer base class if needed

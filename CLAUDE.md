# Claude Code Project Guide

## Project Overview

MicroCommerce is a showcase e-commerce platform demonstrating modern .NET microservices architecture with best practices. It follows a gradual extraction pattern — starting as a modular monolith with clear bounded contexts, designed to evolve into independent microservices.

## Architecture

### Modular Monolith Structure

```
code/MicroCommerce.ApiService/
├── Features/                    # Domain modules (bounded contexts)
│   ├── Catalog/                 # Product catalog domain
│   │   ├── Domain/              # Entities, value objects, events
│   │   ├── Application/         # Commands, queries, handlers
│   │   └── Infrastructure/      # DbContext, configurations
│   ├── Cart/                    # Shopping cart domain
│   ├── Ordering/                # Order management domain
│   └── Inventory/               # Stock management domain
├── Common/                      # Shared infrastructure
│   ├── Behaviors/               # MediatR pipeline behaviors
│   ├── Exceptions/              # Custom exceptions
│   └── Persistence/             # Outbox, interceptors
└── Program.cs                   # Application entry point
```

### Key Patterns

- **CQRS** — Commands and queries separated via MediatR
- **DDD** — Aggregates, value objects, domain events
- **Transactional Outbox** — MassTransit with Azure Service Bus for reliable messaging
- **Database-per-Module** — PostgreSQL with schema isolation per bounded context

## Tech Stack

| Layer | Technology |
|-------|------------|
| Backend | .NET 10, ASP.NET Core Minimal APIs |
| Orchestration | .NET Aspire 13.1.0 |
| Frontend | Next.js 16, React 19, shadcn/ui |
| Database | PostgreSQL (schema-per-module) |
| Messaging | Azure Service Bus (MassTransit) |
| Auth | Keycloak (OAuth2/OIDC) |
| Deployment | Kubernetes, FluxCD GitOps |

## Development Commands

```bash
# Run locally with Aspire
dotnet run --project code/MicroCommerce.AppHost

# Build solution
dotnet build code/

# Run tests
dotnet test code/

# Add EF migration (specify context)
dotnet ef migrations add <Name> --context CatalogDbContext --output-dir Features/Catalog/Infrastructure/Migrations

# Apply migrations
dotnet ef database update --context CatalogDbContext
```

## Code Conventions

### Naming

| Type | Pattern | Example |
|------|---------|---------|
| Command | `{Verb}{Entity}Command` | `CreateProductCommand` |
| Query | `Get{Entity}Query` | `GetProductsQuery` |
| Handler | `{Command/Query}Handler` | `CreateProductCommandHandler` |
| Validator | `{Command}Validator` | `CreateProductCommandValidator` |
| Domain Event | `{Entity}{Action}DomainEvent` | `ProductCreatedDomainEvent` |
| DbContext | `{Module}DbContext` | `CatalogDbContext` |

### CQRS Guidelines

- Every operation goes through MediatR (`ISender.Send`)
- Commands modify state, queries read state — never mix
- Validators run in pipeline before handlers (FluentValidation)
- Domain events are thin (IDs only), published via outbox
- DTOs for query responses, never expose domain entities

See `.planning/phases/01-foundation-project-structure/CQRS-GUIDELINES.md` for full details.

### Domain Model

```csharp
// Aggregate with factory method and domain events
public class Product : BaseAggregateRoot<ProductId>
{
    public static Product Create(ProductName name, Money price)
    {
        var product = new Product(ProductId.New())
        {
            Name = name,
            Price = price
        };
        product.AddDomainEvent(new ProductCreatedDomainEvent(product.Id));
        return product;
    }
}

// Value object with validation
public sealed class ProductName : ValueObject
{
    public string Value { get; }
    public static ProductName From(string value)
    {
        Guard.Against.NullOrWhiteSpace(value);
        return new ProductName(value);
    }
}

// Strongly-typed ID
public sealed record ProductId : StronglyTypedId<Guid>
{
    public static ProductId New() => new(Guid.NewGuid());
}
```

## Project Planning

Planning artifacts are in `.planning/`:

| File | Purpose |
|------|---------|
| `PROJECT.md` | Project context and requirements |
| `ROADMAP.md` | Phase breakdown and dependencies |
| `STATE.md` | Current progress and decisions |
| `REQUIREMENTS.md` | Feature requirements with traceability |
| `config.json` | Workflow preferences |
| `phases/` | Per-phase plans, research, and summaries |

## Module Boundaries

Each module (Catalog, Cart, Ordering, Inventory) has:

- **Own PostgreSQL schema** — Data isolation enforced at database level
- **Own DbContext** — Independent migrations and configurations
- **Own domain events** — Cross-module communication via Service Bus

Modules can reference each other directly within the monolith. When extracting to microservices, replace direct references with event-driven communication.

## Testing Strategy

- **Unit tests** — Domain logic (aggregates, value objects)
- **Integration tests** — API endpoints with TestContainers
- **Saga tests** — MassTransit saga state machines

## Useful Links

- [.NET Aspire docs](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [MassTransit docs](https://masstransit.io/)
- [MediatR wiki](https://github.com/jbogard/MediatR/wiki)
- [FluentValidation docs](https://docs.fluentvalidation.net/)

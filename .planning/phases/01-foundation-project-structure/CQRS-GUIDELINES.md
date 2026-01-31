# CQRS Usage Guidelines

Reference documentation for implementing CQRS patterns in MicroCommerce.

---

## Overview

This project uses CQRS (Command Query Responsibility Segregation) with MediatR. Commands mutate state, queries read state. The Category module in `Features/Catalog/` serves as the reference implementation.

---

## Folder Structure

Each feature follows this structure:

```
Features/
└── {Module}/
    ├── Application/
    │   ├── Commands/
    │   │   └── {Action}{Entity}/
    │   │       ├── {Action}{Entity}Command.cs
    │   │       ├── {Action}{Entity}CommandHandler.cs
    │   │       └── {Action}{Entity}CommandValidator.cs
    │   └── Queries/
    │       └── {Action}{Entities}/
    │           ├── {Action}{Entities}Query.cs
    │           ├── {Action}{Entities}QueryHandler.cs
    │           └── {Entity}Dto.cs
    ├── Domain/
    │   ├── Entities/
    │   │   └── {Entity}.cs
    │   ├── ValueObjects/
    │   │   └── {Entity}Id.cs
    │   └── Events/
    │       └── {Entity}{Action}DomainEvent.cs
    ├── Infrastructure/
    │   ├── {Module}DbContext.cs
    │   └── Configurations/
    │       └── {Entity}Configuration.cs
    └── {Module}Endpoints.cs
```

---

## Naming Conventions

### Commands (mutations)
- **Command:** `{Verb}{Entity}Command` (e.g., `CreateCategoryCommand`, `UpdateProductCommand`)
- **Handler:** `{Verb}{Entity}CommandHandler`
- **Validator:** `{Verb}{Entity}CommandValidator`
- **Folder:** `Commands/{Verb}{Entity}/`

### Queries (reads)
- **Query:** `{Verb}{Entities}Query` (e.g., `GetCategoriesQuery`, `GetProductByIdQuery`)
- **Handler:** `{Verb}{Entities}QueryHandler`
- **DTO:** `{Entity}Dto`
- **Folder:** `Queries/{Verb}{Entities}/`

### Domain
- **Entity:** Singular noun (e.g., `Category`, `Product`)
- **Value Object:** Descriptive name (e.g., `CategoryId`, `CategoryName`, `Money`)
- **Domain Event:** `{Entity}{Action}DomainEvent` (e.g., `CategoryCreatedDomainEvent`)

---

## Command Pattern

### Command Record

```csharp
public sealed record CreateCategoryCommand(
    string Name,
    string? Description = null) : IRequest<CategoryId>;
```

- Use `sealed record` for immutability
- Implement `IRequest<TResponse>` where `TResponse` is the return type
- Return strongly-typed IDs for created entities

### Command Handler

```csharp
public sealed class CreateCategoryCommandHandler
    : IRequestHandler<CreateCategoryCommand, CategoryId>
{
    private readonly CatalogDbContext _context;

    public CreateCategoryCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryId> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var name = CategoryName.Create(request.Name);
        var category = Category.Create(name, request.Description);

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
```

- Use `sealed class` for handlers
- Inject DbContext directly (no repository abstraction in modular monolith)
- Use domain factory methods for entity creation
- Domain events are dispatched via `SaveChangesAsync` interceptor

### Command Validator

```csharp
public sealed class CreateCategoryCommandValidator
    : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Category name is required.")
            .MinimumLength(2)
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description is not null);
    }
}
```

- Validators are auto-discovered and registered
- Run in MediatR pipeline before handler executes (fail-fast)
- Use clear, user-facing error messages

---

## Query Pattern

### Query Record

```csharp
public sealed record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>;
```

- Use `IReadOnlyList<T>` for collections
- Return DTOs, not domain entities

### Query Handler

```csharp
public sealed class GetCategoriesQueryHandler
    : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly CatalogDbContext _context;

    public GetCategoriesQueryHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CategoryDto>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Categories
            .AsNoTracking()      // Read-only optimization
            .OrderBy(c => c.Name.Value)
            .Select(c => new CategoryDto(
                c.Id.Value,
                c.Name.Value,
                c.Description,
                c.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
```

- Always use `AsNoTracking()` for queries
- Project to DTOs in the query (avoid loading full entities)
- Handle value object conversions in projection

### DTO Record

```csharp
public sealed record CategoryDto(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset CreatedAt);
```

- Use `sealed record` for immutability
- Use primitive types (not domain types) for API contracts
- Include only fields needed by consumers

---

## Domain Event Pattern

### Domain Event Record

```csharp
public sealed record CategoryCreatedDomainEvent : DomainEvent
{
    public Guid CategoryId { get; }

    public CategoryCreatedDomainEvent(CategoryId categoryId)
    {
        CategoryId = categoryId.Value;
    }
}
```

- Inherit from `DomainEvent` base class
- Use **thin events** - only include the entity ID
- Convert strongly-typed IDs to Guid for serialization
- Consumers query for additional data if needed

### Raising Events in Aggregates

```csharp
public static Category Create(CategoryName name, string? description = null)
{
    var category = new Category(CategoryId.New())
    {
        Name = name,
        Description = description?.Trim(),
        CreatedAt = DateTimeOffset.UtcNow
    };

    category.AddDomainEvent(new CategoryCreatedDomainEvent(category.Id));

    return category;
}
```

- Raise events in factory methods or state-changing methods
- Events are collected in the aggregate's `DomainEvents` list
- Events are dispatched after `SaveChangesAsync` via interceptor

---

## Endpoint Pattern

### Minimal API Endpoints

```csharp
public static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/catalog")
            .WithTags("Catalog");

        group.MapPost("/categories", CreateCategory)
            .WithName("CreateCategory")
            .Produces<CreateCategoryResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/categories", GetCategories)
            .WithName("GetCategories")
            .Produces<IReadOnlyList<CategoryDto>>();

        return endpoints;
    }

    private static async Task<IResult> CreateCategory(
        CreateCategoryRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateCategoryCommand(request.Name, request.Description);
        var categoryId = await sender.Send(command, cancellationToken);

        return Results.Created(
            $"/api/catalog/categories/{categoryId.Value}",
            new CreateCategoryResponse(categoryId.Value));
    }
}
```

- Use `ISender` (not `IMediator`) for sending commands/queries
- Map request DTOs to commands/queries
- Return appropriate HTTP status codes

---

## Anti-Patterns to Avoid

### 1. Injecting IMediator into Handlers

```csharp
// BAD - creates coupling between handlers
public class Handler : IRequestHandler<Command, Result>
{
    private readonly IMediator _mediator;

    public Task<Result> Handle(Command request, CancellationToken ct)
    {
        // Don't send commands/queries from handlers
        await _mediator.Send(new AnotherCommand());
    }
}
```

**Fix:** Use domain services or let the endpoint orchestrate multiple commands if needed.

### 2. Returning Domain Entities from Queries

```csharp
// BAD - leaks domain model
public sealed record GetCategoriesQuery : IRequest<IReadOnlyList<Category>>;
```

**Fix:** Always return DTOs from queries.

### 3. Business Logic in Validators

```csharp
// BAD - validators should only validate input format
public class Validator : AbstractValidator<Command>
{
    public Validator(IDbContext db)
    {
        RuleFor(x => x.Name)
            .MustAsync(async (name, ct) =>
                !await db.Categories.AnyAsync(c => c.Name == name, ct))
            .WithMessage("Category already exists");
    }
}
```

**Fix:** Put uniqueness checks in the handler; validators check input format only.

### 4. Fat Domain Events

```csharp
// BAD - events should be thin
public sealed record CategoryCreatedDomainEvent : DomainEvent
{
    public Guid CategoryId { get; }
    public string Name { get; }
    public string Description { get; }
    public DateTimeOffset CreatedAt { get; }
    public List<ProductDto> Products { get; }
}
```

**Fix:** Include only the ID. Consumers query for data they need.

### 5. Generic Repository Pattern

```csharp
// BAD - unnecessary abstraction for modular monolith
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task AddAsync(T entity);
}
```

**Fix:** Inject DbContext directly. Repositories add value mainly when switching ORMs.

---

## Checklist for New Features

- [ ] Create command/query records with appropriate return types
- [ ] Create handlers with DbContext injection
- [ ] Create validators for commands (input validation only)
- [ ] Create DTOs for query responses
- [ ] Create domain entities with factory methods
- [ ] Create domain events (thin, ID-only)
- [ ] Create EF configurations with value object conversions
- [ ] Create minimal API endpoints with proper HTTP semantics
- [ ] Wire endpoints in Program.cs

---

*Reference: `Features/Catalog/` module*
*Last updated: 2026-01-29*

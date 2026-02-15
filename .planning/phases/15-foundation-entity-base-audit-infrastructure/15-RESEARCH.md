# Phase 15: Foundation - Entity Base & Audit Infrastructure - Research

**Researched:** 2026-02-14
**Domain:** DDD entity patterns, EF Core interceptors, audit infrastructure, concurrency control
**Confidence:** HIGH

## Summary

This phase builds foundational DDD entity infrastructure in BuildingBlocks.Common: `Entity<TId>` base class for child entities, `IAuditable` interface with automatic timestamp management via interceptor, `AuditableAggregateRoot<TId>` convenience base, `IConcurrencyToken` with explicit Version column replacing PostgreSQL xmin, and `ISoftDeletable` with global query filters and interceptor. The codebase currently has 4 child entities (CartItem, OrderItem, StockReservation, StockAdjustment) with no base class, and 7 aggregates using xmin concurrency (Cart, Order, StockItem, Review, UserProfile, Wishlist, CheckoutState). No entities currently implement soft delete or audit timestamps.

**Entity hierarchy decision**: The research shows two valid patterns - either `BaseAggregateRoot<TId>` extends `Entity<TId>` (reduces duplication), or they remain separate (clearer separation of concerns). Given the user's decision that Entity uses reference equality only without custom Equals/GetHashCode, extending is cleaner since aggregates are entities semantically and share identity concerns.

**Version column strategy**: PostgreSQL-specific xmin (uint) works well but locks into PostgreSQL. Alternatives include int counter (portable, simple, predictable) or byte[] rowversion (SQL Server compatible). The decision specifies auto-increment via interceptor with HTTP 409 on conflict for the 7 existing xmin entities only.

**Primary recommendation:** Implement minimal infrastructure following established EF Core patterns - SaveChangesInterceptor for audit/concurrency/soft-delete, reference equality for Entity base, int Version column for portability, and global query filter for soft deletes. Avoid over-engineering - this is foundational code that must be simple and reliable.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**Entity base contract:**
- Reference equality only — do NOT override Equals/GetHashCode based on ID
- Default ToString — no custom override
- Abstract class (not record) — consistent with existing BaseAggregateRoot<TId> pattern

**Entity hierarchy:**
- Claude's discretion on whether BaseAggregateRoot<TId> extends Entity<TId> or stays separate — pick what reduces duplication while staying clean

**Concurrency version strategy:**
- Auto-increment via interceptor on SaveChanges — entities don't manage Version manually
- HTTP 409 Conflict with details (entity type + version info) on DbUpdateConcurrencyException — client knows to re-fetch and retry
- Scope: Only migrate the 7 entities already using xmin (Cart, Order, StockItem, Review, UserProfile, Wishlist, CheckoutState) — don't expand to Product/Category yet

### Claude's Discretion

**Areas of freedom:**
- Entity hierarchy design (Entity as base for AggregateRoot, or separate)
- Version column type (int counter vs Guid vs byte[])
- Soft delete cascade behavior and implementation details
- Audit interceptor design (timestamp precision, UpdatedAt on soft-delete)
- IAuditable interface shape (accommodate future CreatedBy/ModifiedBy or not)

### Deferred Ideas (OUT OF SCOPE)

None — discussion stayed within phase scope

</user_constraints>

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| EF Core 10 | Latest | ORM with interceptor support | SaveChangesInterceptor, query filters, conventions, concurrency tokens |
| .NET 10 | Current | Runtime and BCL | DateTimeOffset, primary constructors, nullable reference types |
| Npgsql.EntityFrameworkCore.PostgreSQL | Latest | PostgreSQL provider | Production database (xmin migration, schema support) |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Microsoft.EntityFrameworkCore.Diagnostics | 10.x | Interceptor interfaces | Audit, concurrency, soft delete interceptors |
| System.ComponentModel.DataAnnotations | .NET 10 | Timestamp attribute | Mark Version property for EF Core rowversion mapping |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| SaveChangesInterceptor | ChangeTracker events | Interceptor: cleaner separation, async support, testable vs Events: tighter coupling to DbContext constructor |
| int Version counter | PostgreSQL xmin (uint) | xmin: automatic, no column needed vs int: portable, predictable, explicit |
| int Version counter | byte[] rowversion | byte[]: SQL Server compatible vs int: simpler, human-readable, smaller (4 bytes vs 8) |
| Global query filter | Manual WHERE clauses | Filter: automatic, can't forget, consistent vs Manual: flexible but error-prone |
| IAuditable interface | Audit table pattern | Interface: simple, built-in timestamps vs Table: full history, queryable audit trail |

**Installation:**
```bash
# Already available in .NET 10 and EF Core 10
# No additional packages needed
```

## Architecture Patterns

### Recommended Project Structure
```
src/BuildingBlocks/BuildingBlocks.Common/
├── Entity.cs                    # NEW: Base class for child entities
├── IAuditable.cs                # NEW: Audit timestamp interface
├── IConcurrencyToken.cs         # NEW: Version column marker
├── ISoftDeletable.cs            # NEW: Soft delete marker
├── AuditableAggregateRoot.cs    # NEW: Combines BaseAggregateRoot + IAuditable
├── BaseAggregateRoot.cs         # UPDATED: Extends Entity<TId> + IAggregateRoot
├── IAggregateRoot.cs            # Existing: Domain events contract
├── StronglyTypedId.cs           # Existing: Typed ID pattern
└── ValueObject.cs               # Existing: Value object base

src/MicroCommerce.ApiService/Common/Persistence/
├── DomainEventInterceptor.cs           # Existing: Publishes domain events after save
├── AuditInterceptor.cs                 # NEW: Sets CreatedAt/UpdatedAt timestamps
├── ConcurrencyInterceptor.cs           # NEW: Auto-increments Version on update
├── SoftDeleteInterceptor.cs            # NEW: Converts delete to IsDeleted = true
└── SoftDeleteQueryFilter.cs            # NEW: Extension for ModelBuilder conventions
```

### Pattern 1: Entity Base Class (Reference Equality Only)

**What:** Abstract base providing typed ID without custom equality semantics
**When to use:** All child entities within aggregates (CartItem, OrderItem, etc.)
**Example:**
```csharp
// Source: Microsoft DDD guidance + user decision (reference equality only)
namespace MicroCommerce.BuildingBlocks.Common;

/// <summary>
/// Base class for entities within aggregates.
/// Provides typed identity without overriding equality (uses reference equality).
/// </summary>
/// <typeparam name="TId">Strongly-typed ID type</typeparam>
public abstract class Entity<TId>
{
    public TId Id { get; protected init; } = default!;

    protected Entity()
    {
    }

    protected Entity(TId id)
    {
        Id = id;
    }

    // Reference equality only - DO NOT override Equals/GetHashCode
    // Default ToString - no custom override (user decision)
}
```

**Rationale (from research):**
- [Microsoft DDD guidance](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/seedwork-domain-model-base-classes-interfaces) shows Entity base with Id property
- [Enterprise Craftsmanship Entity base](https://enterprisecraftsmanship.com/posts/entity-base-class/) implements custom Equals/GetHashCode based on Id
- User decision: Reference equality only - simpler, avoids EF Core issues with custom equality ([EF Core issue #15687](https://github.com/dotnet/efcore/issues/15687))
- Vernon's aggregate rules: Child entities don't exist independently, accessed through aggregate root

### Pattern 2: Aggregate Root Extending Entity

**What:** BaseAggregateRoot<TId> extends Entity<TId> to inherit identity + add domain events
**When to use:** All aggregate roots (Cart, Order, Product, etc.)
**Example:**
```csharp
// Source: DDD hierarchy analysis - reduces duplication
namespace MicroCommerce.BuildingBlocks.Common;

/// <summary>
/// Base class for aggregate roots.
/// Extends Entity to inherit identity, adds domain event management.
/// </summary>
public abstract class BaseAggregateRoot<TId> : Entity<TId>, IAggregateRoot
{
    private readonly List<DomainEvent> _domainEvents = [];

    protected BaseAggregateRoot() : base()
    {
    }

    protected BaseAggregateRoot(TId id) : base(id)
    {
    }

    protected void AddDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    [NotMapped]
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

**Rationale:**
- Aggregates ARE entities semantically - they have identity
- Reduces duplication: no need to duplicate Id property and constructors
- Clean hierarchy: Entity (identity) → AggregateRoot (identity + events) → AuditableAggregateRoot (identity + events + timestamps)
- Follows [ABP.IO Entity pattern](https://docs.abp.io/en/abp/2.2/Entities): AggregateRoot extends Entity

### Pattern 3: IAuditable Interface with Auto-Timestamps

**What:** Interface marking entities needing CreatedAt/UpdatedAt, set automatically by interceptor
**When to use:** Aggregates needing audit trail (optional, not required for all)
**Example:**
```csharp
// Source: EF Core interceptor docs + user discretion
namespace MicroCommerce.BuildingBlocks.Common;

/// <summary>
/// Marks an entity as requiring automatic audit timestamps.
/// Timestamps are set automatically by AuditInterceptor on SaveChanges.
/// </summary>
public interface IAuditable
{
    DateTimeOffset CreatedAt { get; set; }
    DateTimeOffset UpdatedAt { get; set; }

    // Future: string? CreatedBy { get; set; }
    // Future: string? UpdatedBy { get; set; }
}

// Convenience base for aggregates needing both domain events and timestamps
public abstract class AuditableAggregateRoot<TId> : BaseAggregateRoot<TId>, IAuditable
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    protected AuditableAggregateRoot() : base()
    {
    }

    protected AuditableAggregateRoot(TId id) : base(id)
    {
    }
}
```

**AuditInterceptor implementation:**
```csharp
// Source: EF Core docs + best practices
// https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Common.Persistence;

public sealed class AuditInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateAuditFields(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditFields(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private static void UpdateAuditFields(DbContext? context)
    {
        if (context is null)
            return;

        DateTimeOffset now = DateTimeOffset.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;
                // Deleted state: UpdatedAt on soft-delete (user discretion)
            }
        }
    }
}
```

**Rationale:**
- [EF Core events documentation](https://github.com/dotnet/entityframework.docs/blob/main/entity-framework/core/logging-events-diagnostics/events.md) shows IHasTimestamps pattern
- [Best practices 2026](https://oneuptime.com/blog/post/2026-01-25-audit-logs-ef-core-interceptors/view): SaveChangesInterceptor cleaner than ChangeTracker events
- User discretion: Interface shape accommodates future CreatedBy/ModifiedBy
- DateTimeOffset over DateTime: timezone-aware, best practice for distributed systems

### Pattern 4: IConcurrencyToken with Version Column

**What:** Interface marking entities needing optimistic concurrency, with auto-increment Version
**When to use:** Aggregates with concurrent updates (7 existing: Cart, Order, StockItem, Review, UserProfile, Wishlist, CheckoutState)
**Example:**
```csharp
// Source: User decision + EF Core concurrency patterns
namespace MicroCommerce.BuildingBlocks.Common;

/// <summary>
/// Marks an entity as requiring optimistic concurrency control.
/// Version is automatically incremented by ConcurrencyInterceptor on update.
/// </summary>
public interface IConcurrencyToken
{
    int Version { get; set; }
}
```

**ConcurrencyInterceptor implementation:**
```csharp
// Source: EF Core concurrency docs + user decision (auto-increment)
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Common.Persistence;

public sealed class ConcurrencyInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        IncrementVersions(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        IncrementVersions(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private static void IncrementVersions(DbContext? context)
    {
        if (context is null)
            return;

        foreach (var entry in context.ChangeTracker.Entries<IConcurrencyToken>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.Version++;
            }
            else if (entry.State == EntityState.Added)
            {
                entry.Entity.Version = 1;
            }
        }
    }
}
```

**EF Core configuration convention:**
```csharp
// Source: EF Core concurrency docs
// https://learn.microsoft.com/en-us/ef/core/saving/concurrency
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Common.Persistence;

public static class ConcurrencyTokenConvention
{
    public static void ApplyConcurrencyTokenConvention(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IConcurrencyToken).IsAssignableFrom(entityType.ClrType))
            {
                entityType.FindProperty(nameof(IConcurrencyToken.Version))
                    ?.IsConcurrencyToken();
            }
        }
    }
}
```

**HTTP 409 handling in command handlers:**
```csharp
// Source: User decision - HTTP 409 with details
using Microsoft.EntityFrameworkCore;
using MicroCommerce.ApiService.Common.Exceptions;

// In command handlers that update versioned entities
try
{
    await _context.SaveChangesAsync(cancellationToken);
}
catch (DbUpdateConcurrencyException ex)
{
    var entry = ex.Entries.FirstOrDefault();
    var entityName = entry?.Entity.GetType().Name ?? "Entity";

    throw new ConflictException(
        $"Concurrency conflict: {entityName} was modified by another request. " +
        "Please refresh and retry.");
}
```

**Rationale:**
- [Npgsql concurrency docs](https://www.npgsql.org/efcore/modeling/concurrency.html): xmin is PostgreSQL-specific
- User discretion: int counter for portability, predictable, simple
- Alternative: byte[] for SQL Server compatibility, but adds complexity
- Auto-increment decision: Entities don't manage Version - cleaner, less error-prone
- HTTP 409 decision: Client-friendly error with actionable guidance

### Pattern 5: ISoftDeletable with Query Filter

**What:** Interface marking entities as soft-deletable, with automatic query filtering and interceptor
**When to use:** Entities needing audit trail of deletions (user profiles, orders for compliance)
**Example:**
```csharp
// Source: EF Core query filters docs + soft delete best practices
namespace MicroCommerce.BuildingBlocks.Common;

/// <summary>
/// Marks an entity as soft-deletable.
/// Deleted entities are filtered from queries automatically and can be restored.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
}
```

**SoftDeleteInterceptor implementation:**
```csharp
// Source: EF Core soft delete pattern
// https://github.com/dotnet/entityframework.docs/blob/main/entity-framework/core/querying/filters.md
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Common.Persistence;

public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ConvertDeletesToSoftDeletes(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ConvertDeletesToSoftDeletes(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private static void ConvertDeletesToSoftDeletes(DbContext? context)
    {
        if (context is null)
            return;

        DateTimeOffset now = DateTimeOffset.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State == EntityState.Deleted)
            {
                // Convert hard delete to soft delete
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = now;

                // User discretion: UpdatedAt on soft-delete if IAuditable
                if (entry.Entity is IAuditable auditable)
                {
                    auditable.UpdatedAt = now;
                }
            }
        }
    }
}
```

**Global query filter convention:**
```csharp
// Source: EF Core query filters + convention pattern
using Microsoft.EntityFrameworkCore;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Common.Persistence;

public static class SoftDeleteQueryFilterConvention
{
    public static void ApplySoftDeleteQueryFilters(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                // Create filter: b => !b.IsDeleted
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                var filter = Expression.Lambda(Expression.Not(property), parameter);

                entityType.SetQueryFilter(filter);
            }
        }
    }
}
```

**Rationale:**
- [EF Core soft delete docs](https://learn.microsoft.com/en-us/ef/core/querying/filters): Override SaveChanges to convert deletes
- [Jon P Smith library](https://github.com/JonPSmith/EfCore.SoftDeleteServices): Handles cascade soft deletes
- User discretion: Simple soft delete without cascade complexity initially
- UpdatedAt on soft-delete: Auditable entities track deletion as modification
- Global filter: Prevents accidental queries of deleted data

### Pattern 6: Interceptor Registration

**What:** Register all interceptors in DI and apply to DbContext via Aspire integration
**Example:**
```csharp
// In Program.cs - register interceptors
builder.Services.AddScoped<DomainEventInterceptor>();  // Existing
builder.Services.AddScoped<AuditInterceptor>();        // NEW
builder.Services.AddScoped<ConcurrencyInterceptor>();  // NEW
builder.Services.AddScoped<SoftDeleteInterceptor>();   // NEW

// Apply to each DbContext via Aspire configureDbContextOptions
builder.AddNpgsqlDbContext<CartDbContext>("appdb", configureDbContextOptions: options =>
{
    options.UseNpgsql(npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "cart"));

    // Apply interceptors
    options.AddInterceptors(
        builder.Services.BuildServiceProvider().GetRequiredService<DomainEventInterceptor>(),
        builder.Services.BuildServiceProvider().GetRequiredService<AuditInterceptor>(),
        builder.Services.BuildServiceProvider().GetRequiredService<ConcurrencyInterceptor>(),
        builder.Services.BuildServiceProvider().GetRequiredService<SoftDeleteInterceptor>());
});
```

**Better approach - OnConfiguring in DbContext:**
```csharp
// Source: Avoid BuildServiceProvider anti-pattern
public class CartDbContext : DbContext
{
    private readonly AuditInterceptor _auditInterceptor;
    private readonly ConcurrencyInterceptor _concurrencyInterceptor;
    private readonly SoftDeleteInterceptor _softDeleteInterceptor;

    public CartDbContext(
        DbContextOptions<CartDbContext> options,
        AuditInterceptor auditInterceptor,
        ConcurrencyInterceptor concurrencyInterceptor,
        SoftDeleteInterceptor softDeleteInterceptor)
        : base(options)
    {
        _auditInterceptor = auditInterceptor;
        _concurrencyInterceptor = concurrencyInterceptor;
        _softDeleteInterceptor = softDeleteInterceptor;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditInterceptor, _concurrencyInterceptor, _softDeleteInterceptor);
        // DomainEventInterceptor registered globally or per-context
    }
}
```

### Anti-Patterns to Avoid

**1. Custom Equals/GetHashCode on Entity base:**
- [EF Core issue #15687](https://github.com/dotnet/efcore/issues/15687): Causes duplicate entries in navigation collections
- User decision: Reference equality only - simpler and safer

**2. Manual timestamp management in domain code:**
- Violates SRP: domain logic shouldn't manage audit fields
- Error-prone: easy to forget UpdatedAt on modifications
- Solution: AuditInterceptor handles automatically

**3. Forgetting Version increment:**
- User decision: Interceptor auto-increments, entities don't manage Version
- Avoids bugs from manual version management

**4. Hard-coding soft delete checks:**
```csharp
// BAD: Manual filtering in every query
var activeUsers = await context.Users.Where(u => !u.IsDeleted).ToListAsync();

// GOOD: Global query filter handles automatically
var activeUsers = await context.Users.ToListAsync();
```

**5. BuildServiceProvider in ConfigureServices:**
- Anti-pattern: Creates separate service provider, can cause memory leaks
- Solution: Inject interceptors into DbContext constructor

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Soft delete cascade | Custom recursive deletion logic | [EfCore.SoftDeleteServices](https://www.nuget.org/packages/EfCore.SoftDeleteServices) 9.0 | Handles multi-level cascade, soft delete levels, already tested |
| Full audit trail (who/when/what) | Custom audit table system | [Audit.EntityFramework.Core](https://www.learnentityframeworkcore.com/extensions/audit-entityframework-core) | Captures before/after values, user context, battle-tested |
| Temporal tables | Soft delete for history | EF Core temporal tables (SQL Server) or [Marten](https://martendb.io/) (PostgreSQL) | Built-in versioning, point-in-time queries |
| Concurrency conflicts | Custom retry logic | MediatR retry behavior or Polly | Proven retry patterns, exponential backoff, circuit breakers |

**Key insight:** Audit and soft delete seem simple but have complex edge cases (cascade deletes, multi-level soft delete, concurrency with soft delete, restore with dependencies). Use libraries for complex scenarios, keep it simple for basic needs.

## Common Pitfalls

### Pitfall 1: Soft Delete Cascade Complexity

**What goes wrong:** Parent soft-deleted, children not deleted - orphaned records or referential integrity violations
**Why it happens:** [EF Core doesn't cascade soft deletes](https://github.com/dotnet/efcore/issues/11240) - only hard deletes cascade
**How to avoid:**
- Start simple: No cascade, accept orphaned children (query filters hide them)
- If needed: Use [EfCore.SoftDeleteServices](https://github.com/JonPSmith/EfCore.SoftDeleteServices) for automatic cascade
- Alternative: ClientCascade delete behavior + interceptor to soft-delete dependents
**Warning signs:** Queries for child entities still return items after parent soft-deleted

### Pitfall 2: Query Filter Bypass

**What goes wrong:** Deleted entities appear in results unexpectedly
**Why it happens:** `IgnoreQueryFilters()` called globally or Include navigation bypasses filter
**How to avoid:**
- Be explicit when needing deleted entities: `context.Users.IgnoreQueryFilters().Where(u => u.IsDeleted)`
- Document when IgnoreQueryFilters is necessary (admin views, restore operations)
- Don't use IgnoreQueryFilters broadly - scope it to specific queries
**Warning signs:** Soft-deleted entities appearing in production queries

### Pitfall 3: Concurrency Exception Handling

**What goes wrong:** DbUpdateConcurrencyException crashes request instead of returning HTTP 409
**Why it happens:** Forgot to catch exception and map to ConflictException
**How to avoid:**
- User decision: Catch DbUpdateConcurrencyException in command handlers
- Return HTTP 409 with entity type and version info
- Consider: Global exception handler for DbUpdateConcurrencyException → ConflictException mapping
**Warning signs:** 500 errors on concurrent updates instead of 409

### Pitfall 4: Interceptor Ordering

**What goes wrong:** Audit timestamps set before soft delete conversion, or vice versa
**Why it happens:** Interceptor execution order matters - SavingChanges called in registration order
**How to avoid:**
- Order: SoftDelete → Concurrency → Audit → DomainEvent
- Rationale: Soft delete changes state (Deleted → Modified), concurrency increments version, audit sets timestamps, events fire after save
**Warning signs:** UpdatedAt not set on soft-deleted entities, or Version not incremented

### Pitfall 5: Transient Entity Version

**What goes wrong:** New entities have Version = 0, interceptor doesn't set Version = 1
**Why it happens:** Forgot to initialize Version on Added state
**How to avoid:** ConcurrencyInterceptor sets Version = 1 on EntityState.Added
**Warning signs:** New entities saved with Version = 0 instead of Version = 1

### Pitfall 6: Migration from xmin to Version

**What goes wrong:** Breaking change - xmin (uint) → Version (int), data loss or migration failure
**Why it happens:** Column type change requires data migration
**How to avoid:**
```csharp
// Migration: Add Version column, copy xmin, mark Version as concurrency token, drop xmin
migrationBuilder.AddColumn<int>("Version", "cart", "Carts", defaultValue: 1);
migrationBuilder.Sql("UPDATE cart.\"Carts\" SET \"Version\" = CAST(xmin AS INT)");
// Configure Version as concurrency token in next migration
```
**Warning signs:** Migration fails with data truncation or type mismatch errors

## Code Examples

Verified patterns from official sources and current codebase:

### Current BaseAggregateRoot (Before)
```csharp
// Source: BuildingBlocks.Common/BaseAggregateRoot.cs
using System.ComponentModel.DataAnnotations.Schema;
using MicroCommerce.BuildingBlocks.Common.Events;

namespace MicroCommerce.BuildingBlocks.Common;

public abstract class BaseAggregateRoot<TId>(TId id) : IAggregateRoot
{
    private readonly List<DomainEvent> _domainEvents = [];
    protected void AddDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    [NotMapped]
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public TId Id { get; init; } = id ?? throw new ArgumentNullException(nameof(id));
    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

### Refactored Entity Hierarchy (After)
```csharp
// Entity.cs - NEW base for child entities
namespace MicroCommerce.BuildingBlocks.Common;

public abstract class Entity<TId>
{
    public TId Id { get; protected init; } = default!;

    protected Entity()
    {
    }

    protected Entity(TId id)
    {
        Id = id;
    }
}

// BaseAggregateRoot.cs - UPDATED to extend Entity
public abstract class BaseAggregateRoot<TId> : Entity<TId>, IAggregateRoot
{
    private readonly List<DomainEvent> _domainEvents = [];

    protected BaseAggregateRoot() : base()
    {
    }

    protected BaseAggregateRoot(TId id) : base(id)
    {
    }

    protected void AddDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    [NotMapped]
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();
}

// AuditableAggregateRoot.cs - NEW convenience base
public abstract class AuditableAggregateRoot<TId> : BaseAggregateRoot<TId>, IAuditable
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    protected AuditableAggregateRoot() : base()
    {
    }

    protected AuditableAggregateRoot(TId id) : base(id)
    {
    }
}
```

### Current Child Entity (Before)
```csharp
// CartItem.cs - No base class
public sealed class CartItem
{
    public CartItemId Id { get; private set; } = null!;
    public CartId CartId { get; private set; } = null!;
    // ... other properties

    private CartItem() { }

    internal static CartItem Create(CartId cartId, /* ... */)
    {
        return new CartItem
        {
            Id = CartItemId.New(),
            CartId = cartId,
            // ...
        };
    }
}
```

### Refactored Child Entity (After)
```csharp
// CartItem.cs - Extends Entity<CartItemId>
public sealed class CartItem : Entity<CartItemId>
{
    public CartId CartId { get; private set; } = null!;
    // ... other properties (Id from base)

    private CartItem() : base() { }

    internal static CartItem Create(CartId cartId, /* ... */)
    {
        return new CartItem(CartItemId.New())
        {
            CartId = cartId,
            // ...
        };
    }

    private CartItem(CartItemId id) : base(id) { }
}
```

### Current Aggregate with xmin (Before)
```csharp
// Cart.cs - Uses PostgreSQL xmin
public sealed class Cart : BaseAggregateRoot<CartId>
{
    public Guid BuyerId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastModifiedAt { get; private set; }

    [Timestamp]
    public uint Version { get; private set; }  // xmin mapped

    private Cart(CartId id) : base(id) { }

    public static Cart Create(Guid buyerId)
    {
        var now = DateTimeOffset.UtcNow;
        return new Cart(CartId.New())
        {
            BuyerId = buyerId,
            CreatedAt = now,
            LastModifiedAt = now
        };
    }

    private void Touch()
    {
        LastModifiedAt = DateTimeOffset.UtcNow;  // Manual timestamp
    }
}
```

### Refactored Aggregate with Version and IAuditable (After)
```csharp
// Cart.cs - Uses IConcurrencyToken and IAuditable
public sealed class Cart : AuditableAggregateRoot<CartId>, IConcurrencyToken
{
    public Guid BuyerId { get; private set; }
    public int Version { get; set; }  // IConcurrencyToken - auto-managed
    // CreatedAt, UpdatedAt from AuditableAggregateRoot - auto-managed

    private Cart() : base() { }
    private Cart(CartId id) : base(id) { }

    public static Cart Create(Guid buyerId)
    {
        return new Cart(CartId.New())
        {
            BuyerId = buyerId
            // CreatedAt/UpdatedAt set by interceptor
        };
    }

    // Touch() removed - UpdatedAt managed by interceptor
}

// CartConfiguration.cs - EF Core mapping
public void Configure(EntityTypeBuilder<Cart> builder)
{
    // No explicit Version configuration needed - convention handles it
    // OR explicit:
    builder.Property(c => c.Version)
        .IsConcurrencyToken();
}
```

### Soft Delete Entity Example
```csharp
// UserProfile.cs - Soft deletable
public sealed class UserProfile : AuditableAggregateRoot<ProfileId>, IConcurrencyToken, ISoftDeletable
{
    public string UserId { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;

    // IConcurrencyToken
    public int Version { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    private UserProfile() : base() { }

    // Domain method - EF/interceptor handles soft delete mechanics
    public void Delete()
    {
        // Business logic validation here if needed
        // Interceptor will set IsDeleted = true, DeletedAt = now on SaveChanges
    }
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| ChangeTracker.StateChanged events | SaveChangesInterceptor | EF Core 5.0 (2020) | Cleaner separation, testable, async support |
| Manual timestamp in domain | IAuditable + Interceptor | Best practice since EF Core 5 | Automatic, consistent, cross-cutting concern separated |
| PostgreSQL xmin for concurrency | Explicit Version column | Portability priority | Database-agnostic, predictable, testable |
| Hard delete | Soft delete with query filters | EF Core 2.0+ (2017) | Audit trail, regulatory compliance, data recovery |
| Manual equality override | Reference equality | DDD + EF Core best practice | Simpler, avoids EF Core tracking issues |
| byte[] rowversion | int counter | PostgreSQL ecosystem | Simpler, human-readable, sufficient for optimistic concurrency |

**Deprecated/outdated:**
- **xmin concurrency (PostgreSQL-specific)**: Works well but locks into PostgreSQL. Explicit Version column is portable and testable.
- **ChangeTracker events for audit**: Still works but SaveChangesInterceptor is cleaner (no DbContext constructor dependency).
- **Hard deletes for user data**: GDPR/compliance often requires audit trail - soft delete is safer default.

## Open Questions

1. **Cascade soft delete strategy**
   - What we know: EF Core doesn't cascade soft deletes automatically
   - What's unclear: Does MicroCommerce need cascade soft deletes initially, or can it be deferred?
   - Recommendation: Start without cascade (simpler), add [EfCore.SoftDeleteServices](https://www.nuget.org/packages/EfCore.SoftDeleteServices) if needed in Phase 21 (full adoption)

2. **CreatedBy/ModifiedBy audit fields**
   - What we know: User discretion on IAuditable interface shape
   - What's unclear: Does v2.0 need user tracking (CreatedBy/ModifiedBy) or just timestamps?
   - Recommendation: Start with timestamps only (simpler), add string? CreatedBy/ModifiedBy properties later if needed (interface evolution)

3. **Global vs per-context interceptor registration**
   - What we know: DomainEventInterceptor registered globally (scoped service)
   - What's unclear: Should audit/concurrency/soft-delete be global or per-context?
   - Recommendation: Global (all contexts need it), registered as scoped services, injected into DbContext constructors

4. **Migration strategy for 7 xmin entities**
   - What we know: Need to migrate xmin → Version column
   - What's unclear: Data migration approach (copy xmin value or reset to 1?)
   - Recommendation: Copy xmin value for production data continuity, document that Version starts at 1 for new entities

## Sources

### Primary (HIGH confidence)
- [EF Core Interceptors Documentation](https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors) - SaveChangesInterceptor patterns
- [EF Core Concurrency Tokens](https://learn.microsoft.com/en-us/ef/core/saving/concurrency) - IsConcurrencyToken, rowversion, DbUpdateConcurrencyException
- [EF Core Global Query Filters](https://learn.microsoft.com/en-us/ef/core/querying/filters) - Soft delete pattern
- [Microsoft DDD Microservices Guide](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/seedwork-domain-model-base-classes-interfaces) - Entity base class, aggregate root
- [EF Core Events Documentation](https://github.com/dotnet/entityframework.docs/blob/main/entity-framework/core/logging-events-diagnostics/events.md) - IHasTimestamps pattern (Context7)
- [Npgsql Concurrency Documentation](https://www.npgsql.org/efcore/modeling/concurrency.html) - xmin system column for PostgreSQL

### Secondary (MEDIUM confidence)
- [How to Implement Audit Logs with EF Core Interceptors](https://oneuptime.com/blog/post/2026-01-25-audit-logs-ef-core-interceptors/view) - 2026 best practices
- [EF Core In Depth - Soft Deleting Data](https://www.thereformedprogrammer.net/ef-core-in-depth-soft-deleting-data-with-global-query-filters/) - Jon P Smith soft delete patterns
- [GitHub: EfCore.SoftDeleteServices](https://github.com/JonPSmith/EfCore.SoftDeleteServices) - Cascade soft delete library
- [Enterprise Craftsmanship - Entity Base Class](https://enterprisecraftsmanship.com/posts/entity-base-class/) - DDD entity equality patterns
- [ABP.IO Entity Documentation](https://docs.abp.io/en/abp/2.2/Entities) - AggregateRoot extends Entity pattern
- [DDD Aggregates vs Entities - Dan Does Code](https://www.dandoescode.com/blog/ddd-modelling-aggregates-vs-entities) - Aggregate design patterns
- [Martin Fowler - DDD Aggregate](https://martinfowler.com/bliki/DDD_Aggregate.html) - Aggregate root concepts

### Tertiary (LOW confidence, marked for validation)
- [Soft Delete in .NET with EF Core - Needlify](https://needlify.com/soft-delete-in-net-with-ef-core-how-to-safely-remove-data-using-query-filters-and-client-cascade/) - Cascade soft delete patterns
- [EF Core issue #11240](https://github.com/dotnet/efcore/issues/11240) - Soft delete cascade discussion
- [EF Core issue #15687](https://github.com/dotnet/efcore/issues/15687) - Custom equality issues with EF Core

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - EF Core 10, .NET 10, Npgsql are current production stack
- Architecture: HIGH - Patterns verified from Microsoft docs, Context7, established DDD practices
- Pitfalls: HIGH - Issues documented in EF Core GitHub, validated through multiple sources
- Code examples: HIGH - Based on existing codebase patterns + official EF Core documentation

**Research date:** 2026-02-14
**Valid until:** 60 days (stable patterns - entity/audit infrastructure changes slowly)

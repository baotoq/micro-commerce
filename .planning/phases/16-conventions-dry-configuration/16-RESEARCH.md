# Phase 16: Conventions - DRY Configuration - Research

**Researched:** 2026-02-14
**Domain:** EF Core model conventions, value converters, configuration automation, PostgreSQL naming
**Confidence:** HIGH

## Summary

EF Core 6.0+ provides `ConfigureConventions()` for pre-convention configuration (type-level converter/comparer setup) and `IModelFinalizingConvention` for model finalization conventions (entity-level metadata manipulation). The codebase has 13 entity configuration files with repetitive StronglyTypedId conversions, manual concurrency token mapping, and inconsistent naming (PascalCase tables in PostgreSQL). Phase 15 established IAuditable/IConcurrencyToken/ISoftDeletable interfaces ready for convention automation. The ValueObject base class is obsolete and must be removed along with all infrastructure (no converters/comparers exist currently - ValueObjects like Money/ProductName use manual HasConversion).

**Two convention approaches:** Pre-convention (`ConfigureConventions()`) for simple type-based rules (StronglyTypedId converters) vs Model Finalization (`IModelFinalizingConvention`) for complex entity inspection (interface-based configuration). Both can coexist in the same DbContext.

**snake_case strategy:** Use EFCore.NamingConventions package (v10.0.1, Jan 2026) via `UseSnakeCaseNamingConvention()` - handles tables, columns, indexes automatically. Generates breaking migration renaming all schema objects. Apply to all DbContexts through shared base class or extension method.

**Primary recommendation:** Implement type-level conventions via `ConfigureConventions()` for StronglyTypedId value converters, use EFCore.NamingConventions package for snake_case, create custom `IModelFinalizingConvention` implementations for interface-based configuration (IAuditable column types, IConcurrencyToken IsConcurrencyToken, ISoftDeletable query filters), register all conventions in shared base DbContext class, then systematically remove redundant manual configuration and generate migrations.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**Convention scope:**
- Conventions for StronglyTypedId value converters (auto HasConversion)
- Conventions for IConcurrencyToken (auto IsConcurrencyToken configuration)
- Conventions for ISoftDeletable (auto HasQueryFilter for soft-delete)
- Conventions for IAuditable (auto-configure CreatedAt/UpdatedAt column types and defaults)
- snake_case naming convention for all PostgreSQL tables and columns

**Configuration cleanup:**
- Remove redundant manual config lines that conventions now handle — single source of truth
- Delete entity configuration files that become empty or near-empty after cleanup
- Register conventions in a shared base class or helper that all DbContexts inherit/call — one place, automatic for all
- Individual DbContexts can override convention defaults for entity-specific cases when needed

**ValueObject removal:**
- Full cleanup: remove ValueObject base class AND all related infrastructure (converters, comparers, helpers)
- Migrate any remaining ValueObject inheritors to readonly record structs (v1.1 pattern)
- Generate EF Core migrations for any schema changes resulting from the cleanup
- Verify zero ValueObject references remain in codebase after cleanup (build + grep)

### Claude's Discretion

**Areas of freedom:**
- Exact convention implementation approach (IModelFinalizingConvention vs ConfigureConventions)
- Order of convention registration
- How to structure the shared base class / helper for convention registration
- Migration strategy for snake_case (whether to rename existing tables/columns or apply to new only)

### Deferred Ideas (OUT OF SCOPE)

None — discussion stayed within phase scope

</user_constraints>

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| EF Core 10 | Latest (Nov 2024) | ORM with convention support | ConfigureConventions added in EF 6.0, IModelFinalizingConvention for custom conventions |
| EFCore.NamingConventions | 10.0.1 (Jan 2026) | PostgreSQL snake_case naming | Community-standard package with 15M+ downloads, supports all naming styles |
| .NET 10 | Current | Runtime with latest C# features | Record structs for ValueObject migration, file-scoped namespaces |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Microsoft.EntityFrameworkCore.Metadata.Conventions | 10.x | Convention interfaces | IModelFinalizingConvention implementations |
| System.Text.RegularExpressions | .NET 10 | PascalCase → snake_case conversion | If implementing custom naming (not needed with package) |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| EFCore.NamingConventions package | Custom IModelFinalizingConvention | Package: battle-tested, maintained vs Custom: no dependency, full control |
| ConfigureConventions (type-level) | Manual configuration per property | Conventions: DRY, automatic vs Manual: explicit, visible in config files |
| IModelFinalizingConvention | Reflection in OnModelCreating | Convention: cleaner, standard pattern vs Reflection: ad-hoc, mixed concerns |
| Shared base DbContext | Extension method on ModelBuilder | Base class: enforced, override-friendly vs Extension: opt-in, flexible |

**Installation:**
```bash
dotnet add src/MicroCommerce.ApiService package EFCore.NamingConventions
```

## Architecture Patterns

### Recommended Project Structure
```
src/MicroCommerce.ApiService/Common/Persistence/
├── Conventions/
│   ├── StronglyTypedIdConvention.cs           # Auto-convert all StronglyTypedId<T> properties
│   ├── AuditableConvention.cs                 # Configure IAuditable timestamp columns
│   ├── ConcurrencyTokenConvention.cs          # Auto IsConcurrencyToken for IConcurrencyToken.Version
│   ├── SoftDeletableConvention.cs             # Auto query filter for ISoftDeletable entities
│   └── ConventionExtensions.cs                # Helper for applying conventions to ModelBuilder
├── BaseDbContext.cs                            # NEW: Shared base with convention registration
├── DomainEventInterceptor.cs                   # Existing
├── AuditInterceptor.cs                         # Existing (Phase 15)
├── ConcurrencyInterceptor.cs                   # Existing (Phase 15)
└── SoftDeleteInterceptor.cs                    # Existing (Phase 15)

src/MicroCommerce.ApiService/Features/{Feature}/Infrastructure/
├── {Feature}DbContext.cs                       # UPDATED: Inherits BaseDbContext (gets conventions automatically)
└── Configurations/                             # REDUCED: Only feature-specific config remains
    └── {Entity}Configuration.cs                # Relationships, indexes, lengths — NO MORE StronglyTypedId/IConcurrencyToken boilerplate
```

### Pattern 1: Pre-Convention Type-Level Configuration (ConfigureConventions)

**What:** Configure value converters/comparers for all properties of a specific CLR type before model building conventions run
**When to use:** Simple type-based rules like StronglyTypedId value converters
**Example:**
```csharp
// Source: https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions
// Apply to all StronglyTypedId<Guid> types
protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
{
    // Generic approach: requires concrete converter types per StronglyTypedId
    configurationBuilder
        .Properties<ProductId>()
        .HaveConversion<ProductIdConverter>();

    configurationBuilder
        .Properties<CartId>()
        .HaveConversion<CartIdConverter>();

    // This must be done for EVERY StronglyTypedId type - NOT fully automatic
}

// Converter implementation
public class ProductIdConverter : ValueConverter<ProductId, Guid>
{
    public ProductIdConverter()
        : base(
            id => id.Value,
            value => new ProductId(value))
    {
    }
}
```

**Limitation:** ConfigureConventions requires explicit converter type per property type. Cannot automatically detect all `StronglyTypedId<Guid>` inheritors without reflection. Better suited for a few common types or when combined with source generation.

### Pattern 2: Model Finalization Convention (IModelFinalizingConvention)

**What:** Custom convention that inspects/modifies entity metadata during model finalization phase
**When to use:** Complex rules based on entity shape, interfaces, or property inspection
**Example:**
```csharp
// Source: https://oneuptime.com/blog/post/2026-01-30-build-custom-ef-core-conventions/view
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
/// Automatically configures IsConcurrencyToken() for all properties named "Version"
/// on entities implementing IConcurrencyToken.
/// </summary>
public class ConcurrencyTokenConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            // Check if entity implements IConcurrencyToken
            if (typeof(IConcurrencyToken).IsAssignableFrom(entityType.ClrType))
            {
                var versionProperty = entityType.FindProperty(nameof(IConcurrencyToken.Version));
                if (versionProperty != null)
                {
                    versionProperty.Builder.IsConcurrencyToken(true);
                }
            }
        }
    }
}
```

**Registration:**
```csharp
protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
{
    // Register custom conventions
    configurationBuilder.Conventions.Add(_ => new ConcurrencyTokenConvention());
    configurationBuilder.Conventions.Add(_ => new AuditableConvention());
    configurationBuilder.Conventions.Add(_ => new SoftDeletableConvention());
}
```

### Pattern 3: Snake_Case Naming via Package

**What:** Use EFCore.NamingConventions to automatically convert all table/column names to snake_case
**When to use:** PostgreSQL databases (PostgreSQL convention is snake_case, not PascalCase)
**Example:**
```csharp
// Source: https://github.com/efcore/EFCore.NamingConventions
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder
        .UseNpgsql(connectionString)
        .UseSnakeCaseNamingConvention(); // Products → products, ProductId → product_id
}
```

**Impact:**
- **Before:** `CREATE TABLE "Products" ("Id" uuid, "ProductId" uuid, "CategoryId" uuid)`
- **After:** `CREATE TABLE "products" ("id" uuid, "product_id" uuid, "category_id" uuid)`

**Migration strategy:**
1. Apply convention to all 8 DbContexts
2. Generate migrations - will rename ALL tables and columns
3. Review migration carefully (breaking change for existing databases)
4. Option A: Apply migration (renames everything, requires downtime)
5. Option B: Keep existing tables as-is, apply convention only to new contexts (inconsistent but safe)

**Recommendation for micro-commerce:** Apply to all contexts (Option A) since this is a development/showcase project without production data constraints.

### Pattern 4: Shared Base DbContext

**What:** Abstract base class that all feature DbContexts inherit, providing convention registration in one place
**When to use:** Ensure all DbContexts automatically get the same conventions
**Example:**
```csharp
// Source: Convention best practice from 2026 article
namespace MicroCommerce.ApiService.Common.Persistence;

public abstract class BaseDbContext : DbContext
{
    protected BaseDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // Register all custom conventions
        configurationBuilder.Conventions.Add(_ => new ConcurrencyTokenConvention());
        configurationBuilder.Conventions.Add(_ => new AuditableConvention());
        configurationBuilder.Conventions.Add(_ => new SoftDeletableConvention());
        configurationBuilder.Conventions.Add(_ => new StronglyTypedIdConvention());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Apply snake_case naming to all contexts
        optionsBuilder.UseSnakeCaseNamingConvention();
    }
}
```

**Feature DbContext usage:**
```csharp
// BEFORE: Inherits DbContext directly
public class CatalogDbContext : DbContext { }

// AFTER: Inherits BaseDbContext, gets all conventions automatically
public class CatalogDbContext : BaseDbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    // Can still override conventions for specific needs
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Feature-specific configuration only
        modelBuilder.HasDefaultSchema("catalog");
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CatalogDbContext).Assembly,
            t => t.Namespace?.Contains("Features.Catalog") == true);
    }
}
```

### Pattern 5: StronglyTypedId Reflection Convention

**What:** Use reflection to detect all `StronglyTypedId<T>` properties and configure value converters automatically
**When to use:** Avoid manually registering converter for each StronglyTypedId type
**Example:**
```csharp
// Source: Community pattern from 2026 custom value converter research
using System.Linq.Expressions;

public class StronglyTypedIdConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                var propertyType = property.ClrType;

                // Check if property type inherits from StronglyTypedId<T>
                if (IsStronglyTypedId(propertyType, out var underlyingType))
                {
                    // Create converter: StronglyTypedId → underlying value
                    var converterType = typeof(ValueConverter<,>)
                        .MakeGenericType(propertyType, underlyingType);

                    var toProvider = CreateToProviderExpression(propertyType, underlyingType);
                    var fromProvider = CreateFromProviderExpression(propertyType, underlyingType);

                    var converter = (ValueConverter)Activator.CreateInstance(
                        converterType,
                        toProvider,
                        fromProvider)!;

                    property.Builder.HasConversion(converter);
                }
            }
        }
    }

    private static bool IsStronglyTypedId(Type type, out Type underlyingType)
    {
        underlyingType = null!;

        if (!type.IsGenericType)
            return false;

        var genericType = type.GetGenericTypeDefinition();
        if (genericType != typeof(StronglyTypedId<>))
            return false;

        underlyingType = type.GetGenericArguments()[0];
        return true;
    }

    private static LambdaExpression CreateToProviderExpression(Type idType, Type underlyingType)
    {
        // id => id.Value
        var parameter = Expression.Parameter(idType, "id");
        var valueProperty = Expression.Property(parameter, "Value");
        return Expression.Lambda(valueProperty, parameter);
    }

    private static LambdaExpression CreateFromProviderExpression(Type idType, Type underlyingType)
    {
        // value => new ProductId(value)
        var parameter = Expression.Parameter(underlyingType, "value");
        var constructor = idType.GetConstructor(new[] { underlyingType })!;
        var newExpression = Expression.New(constructor, parameter);
        return Expression.Lambda(newExpression, parameter);
    }
}
```

**Advantage:** Fully automatic - add new StronglyTypedId type and converter applies automatically.

### Pattern 6: Interface-Based Query Filter Convention

**What:** Automatically apply `HasQueryFilter(e => !e.IsDeleted)` to all entities implementing ISoftDeletable
**When to use:** Global soft-delete filtering without manual configuration per entity
**Example:**
```csharp
// Source: https://haacked.com/archive/2019/07/29/query-filter-by-interface/
// Adapted for EF Core 10 with AppendQueryFilter
public class SoftDeletableConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            // Only apply to base types (not derived types in inheritance hierarchy)
            if (entityType.BaseType != null)
                continue;

            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                // Use helper method to create strongly-typed filter expression
                var method = typeof(SoftDeletableConvention)
                    .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(null, new object[] { entityType });
            }
        }
    }

    private static void SetSoftDeleteFilter<TEntity>(IMutableEntityType entityType)
        where TEntity : class, ISoftDeletable
    {
        Expression<Func<TEntity, bool>> filter = e => !e.IsDeleted;

        entityType.SetQueryFilter(filter);
    }
}
```

**Result:** All queries automatically exclude soft-deleted records:
```csharp
// Automatic: SELECT * FROM products WHERE is_deleted = false
var products = await context.Products.ToListAsync();

// Explicit override when needed:
var allProducts = await context.Products.IgnoreQueryFilters().ToListAsync();
```

### Pattern 7: IAuditable Column Configuration Convention

**What:** Automatically configure CreatedAt/UpdatedAt column types and defaults for IAuditable entities
**When to use:** Ensure consistent timestamp column configuration across all auditable entities
**Example:**
```csharp
public class AuditableConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            if (typeof(IAuditable).IsAssignableFrom(entityType.ClrType))
            {
                // Configure CreatedAt
                var createdAtProperty = entityType.FindProperty(nameof(IAuditable.CreatedAt));
                if (createdAtProperty != null)
                {
                    createdAtProperty.Builder
                        .IsRequired(true)
                        .HasColumnType("timestamp with time zone");
                }

                // Configure UpdatedAt
                var updatedAtProperty = entityType.FindProperty(nameof(IAuditable.UpdatedAt));
                if (updatedAtProperty != null)
                {
                    updatedAtProperty.Builder
                        .IsRequired(true)
                        .HasColumnType("timestamp with time zone");
                }
            }
        }
    }
}
```

**Result:** All IAuditable entities automatically get proper timestamp columns without manual configuration.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| snake_case naming conversion | Regex-based PascalCase → snake_case in custom convention | EFCore.NamingConventions package | Package handles edge cases (consecutive capitals, numbers, acronyms), battle-tested with 15M+ downloads, supports all casing styles |
| Reflection-based convention helpers | Manual MethodInfo.Invoke with string method names | Strongly-typed helper methods with generics | Type safety, compile-time checking, better performance |
| Value converter per StronglyTypedId | 50+ individual converter classes | Single reflection-based convention | One convention handles all current + future StronglyTypedId types automatically |
| Manual query filter registration | Separate query filter configuration per entity | IModelFinalizingConvention with interface detection | Convention applies automatically to all ISoftDeletable entities |

**Key insight:** Conventions run once at model building time (zero runtime cost), so reflection overhead is acceptable. Don't prematurely optimize convention code - correctness and maintainability matter more than micro-optimizations in code that runs once per DbContext initialization.

## Common Pitfalls

### Pitfall 1: ConfigureConventions Limitation with Generic Types

**What goes wrong:** Attempting to configure all `StronglyTypedId<T>` types at once in ConfigureConventions fails
```csharp
// DOES NOT WORK - cannot configure open generic types
configurationBuilder
    .Properties<StronglyTypedId<Guid>>()  // Error: StronglyTypedId<> is abstract
    .HaveConversion<???>();                // What converter type to use?
```

**Why it happens:** `ConfigureConventions()` is pre-convention and type-based. It requires concrete types and converter types with parameterless constructors. Cannot dynamically create converters.

**How to avoid:** Use `IModelFinalizingConvention` with reflection instead (Pattern 5) for dynamic type inspection and converter creation.

**Warning signs:** Compile errors about abstract types, inability to specify converter type parameter.

### Pitfall 2: Convention Order Matters

**What goes wrong:** Conventions registered in wrong order can cause later conventions to not see earlier changes
```csharp
// WRONG ORDER - SoftDeletableConvention may run before entity types are finalized
configurationBuilder.Conventions.Add(_ => new SoftDeletableConvention());
configurationBuilder.Conventions.Add(_ => new StronglyTypedIdConvention());
```

**Why it happens:** Conventions execute sequentially. Later conventions can override earlier ones. Model finalization conventions typically need to run after property conventions.

**How to avoid:** Register property-level conventions first, then entity-level conventions, then relationship conventions. Review convention execution order documentation.

**Warning signs:** Convention not applying, properties not configured as expected, query filters missing.

**Recommended order:**
1. Type-level (StronglyTypedId converters)
2. Property-level (IAuditable column config, IConcurrencyToken)
3. Entity-level (ISoftDeletable query filters)

### Pitfall 3: Breaking Migration from snake_case Convention

**What goes wrong:** Adding `UseSnakeCaseNamingConvention()` generates migration that renames EVERY table and column in the database
```bash
# Generated migration renames everything:
migrationBuilder.RenameTable(name: "Products", newName: "products");
migrationBuilder.RenameColumn(table: "products", name: "CategoryId", newName: "category_id");
# ... hundreds of renames
```

**Why it happens:** Convention applies to entire model. EF Core sees all names as changed and generates rename operations.

**How to avoid:**
- Option 1: Accept breaking change, apply migration with downtime (acceptable for dev/showcase projects)
- Option 2: Grandfather existing tables with `[Table("Products")]` attribute to preserve names, apply snake_case only to new tables
- Option 3: Disable convention, manually apply snake_case to new features only

**Warning signs:** Migration file with hundreds of RenameTable/RenameColumn operations.

**For micro-commerce (dev project):** Apply breaking migration - no production data to worry about.

### Pitfall 4: Convention Doesn't Override Explicit Configuration

**What goes wrong:** Entity configuration explicitly sets `.HasConversion()` and convention never applies
```csharp
// Explicit configuration in ProductConfiguration.cs
builder.Property(p => p.Id)
    .HasConversion(id => id.Value, value => new ProductId(value));

// Convention never runs because property already configured
```

**Why it happens:** Conventions detect and skip properties that already have explicit configuration. Explicit always wins.

**How to avoid:** Remove explicit configuration after implementing convention. Review all entity configurations and delete redundant lines.

**Warning signs:** Convention implemented but properties still manually configured in entity configuration files.

### Pitfall 5: Forgetting to Call base.ConfigureConventions()

**What goes wrong:** Custom DbContext overrides `ConfigureConventions()` but doesn't call base, losing base class conventions
```csharp
// WRONG - base conventions lost
protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
{
    // Missing: base.ConfigureConventions(configurationBuilder);

    configurationBuilder
        .Properties<string>()
        .HaveMaxLength(256);
}
```

**Why it happens:** Override replaces base implementation by default in C#.

**How to avoid:** ALWAYS call `base.ConfigureConventions(configurationBuilder)` first in override.

**Warning signs:** Conventions from base class not applying, unexpected behavior in derived contexts.

### Pitfall 6: ValueObject Removal Schema Changes

**What goes wrong:** Removing ValueObject base class and migrating to record structs changes how EF Core handles equality/hashing, potentially breaking existing data or queries
```csharp
// BEFORE: ValueObject with custom equality
public class Money : ValueObject
{
    protected override IEnumerable<object> GetEqualityComponents()
        => new object[] { Amount, Currency };
}

// AFTER: Record struct with compiler-generated equality
public readonly record struct Money(decimal Amount, Currency Currency);
```

**Why it happens:** ValueObject implements IComparable and custom equality. Record struct uses compiler-generated value equality. JSON serialization, EF Core complex property handling, or database representation might change.

**How to avoid:**
1. Audit all ValueObject inheritors and their usage in entity configurations
2. Check for custom ValueComparers or JSON converters that depend on ValueObject
3. Generate migrations and verify schema changes (should be minimal if using `.ComplexProperty()`)
4. Build and run tests after removal to catch runtime breaks

**Warning signs:** Compilation errors in entity configurations, migration generates unexpected column changes, tests fail on value equality checks.

## Code Examples

Verified patterns from official sources and community best practices:

### Example 1: Complete Convention Registration in BaseDbContext

```csharp
// Source: Best practices from 2026 convention article + Microsoft docs
namespace MicroCommerce.ApiService.Common.Persistence;

public abstract class BaseDbContext : DbContext
{
    protected BaseDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // ORDER MATTERS: Property → Entity → Relationship

        // 1. Type-level property conventions
        configurationBuilder.Conventions.Add(_ => new StronglyTypedIdConvention());

        // 2. Interface-based property conventions
        configurationBuilder.Conventions.Add(_ => new AuditableConvention());
        configurationBuilder.Conventions.Add(_ => new ConcurrencyTokenConvention());

        // 3. Entity-level conventions (query filters must run after property config)
        configurationBuilder.Conventions.Add(_ => new SoftDeletableConvention());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Don't configure database provider here (done in DI registration)
        // But DO configure cross-cutting conventions like naming
        if (!optionsBuilder.IsConfigured)
        {
            // Only apply if not already configured (testing scenarios)
            optionsBuilder.UseSnakeCaseNamingConvention();
        }
    }
}
```

### Example 2: Minimal Entity Configuration After Conventions

```csharp
// Source: Codebase ProductConfiguration.cs - BEFORE conventions
// BEFORE: 89 lines with manual StronglyTypedId conversions
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        // REMOVE: Convention handles this
        builder.Property(p => p.Id)
            .HasConversion(
                id => id.Value,
                value => new ProductId(value));

        // REMOVE: Convention handles this
        builder.Property(p => p.CategoryId)
            .HasConversion(
                id => id.Value,
                value => new CategoryId(value))
            .IsRequired();

        // ... rest of configuration
    }
}

// AFTER: 45 lines - convention handles StronglyTypedId, only domain-specific config remains
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        // StronglyTypedId conversion automatic via convention
        // CategoryId conversion automatic via convention

        // ProductName value object (NOT StronglyTypedId, still needs manual config)
        builder.Property(p => p.Name)
            .HasConversion(
                name => name.Value,
                value => ProductName.Create(value))
            .HasMaxLength(200)
            .IsRequired();

        // Money complex property
        builder.ComplexProperty(p => p.Price, priceBuilder =>
        {
            priceBuilder.Property(m => m.Amount)
                .HasColumnName("Price")
                .HasPrecision(18, 2);

            priceBuilder.Property(m => m.Currency)
                .HasColumnName("PriceCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("USD");
        });

        builder.Property(p => p.Description)
            .HasMaxLength(4000)
            .IsRequired();

        // Relationship to Category
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.Sku).IsUnique().HasFilter("\"Sku\" IS NOT NULL");
    }
}
```

### Example 3: Testing Conventions Applied Correctly

```csharp
// Source: Convention testing best practice
public class ConventionTests
{
    [Fact]
    public void StronglyTypedId_Convention_Applies_Converter()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName: "ConventionTest")
            .Options;

        using var context = new CatalogDbContext(options);

        // Act
        var model = context.Model;
        var productIdProperty = model.FindEntityType(typeof(Product))!
            .FindProperty(nameof(Product.Id))!;

        // Assert
        Assert.NotNull(productIdProperty.GetValueConverter());
        Assert.Equal(typeof(Guid), productIdProperty.GetValueConverter()!.ProviderClrType);
    }

    [Fact]
    public void ISoftDeletable_Convention_Applies_QueryFilter()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName: "ConventionTest")
            .Options;

        using var context = new CatalogDbContext(options);

        // Act
        var model = context.Model;
        var productEntity = model.FindEntityType(typeof(Product))!;

        // Assert
        Assert.NotNull(productEntity.GetQueryFilter());
    }
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Manual HasConversion per StronglyTypedId property | ConfigureConventions or IModelFinalizingConvention | EF Core 6.0 (Nov 2021) | Reduces config boilerplate from 13 files × ~4 conversions each = 50+ manual configs to 1 convention |
| Manual IsConcurrencyToken per entity | Interface-based convention | EF Core 7.0+ pattern | Eliminates [Timestamp] attribute or .IsRowVersion() calls in 7 entity configs |
| Manual HasQueryFilter per soft-deletable entity | IModelFinalizingConvention scanning for ISoftDeletable | EF Core 5.0+ pattern | Prevents forgetting query filter on new entities |
| PascalCase PostgreSQL naming | snake_case via EFCore.NamingConventions | Community standard since 2018 | Aligns with PostgreSQL ecosystem conventions |
| ValueObject base class (2017 pattern) | readonly record struct (C# 10+, 2021) | .NET 6 / C# 10 | Simpler, compiler-generated equality, less infrastructure code |

**Deprecated/outdated:**
- **ValueObject base class**: Replaced by `readonly record struct` for simple value objects and `record class` for complex ones (Phase 14.2 decision). Infrastructure overhead not needed with modern C# features.
- **Manual value converter registration per property**: Conventions eliminate repetition since EF Core 6.0.
- **Attribute-based concurrency tokens ([Timestamp])**: Convention approach is more flexible and keeps domain models attribute-free.

## Open Questions

1. **Should conventions handle ValueObject conversions automatically?**
   - What we know: Current ValueObjects (Money, ProductName, etc.) use manual HasConversion in entity configs
   - What's unclear: Whether to create ValueObject detection convention or treat them case-by-case after migration to record structs
   - Recommendation: Handle case-by-case initially. If pattern emerges post-migration, add convention in future phase.

2. **How to handle DI-registered services in conventions?**
   - What we know: Conventions run at model build time, before DI container fully initialized
   - What's unclear: If we need ITimeProvider or other services in conventions
   - Recommendation: Not needed for this phase. Conventions configure metadata only, interceptors handle runtime logic.

3. **Should snake_case apply to schemas (catalog → catalog) or just tables/columns?**
   - What we know: EFCore.NamingConventions package converts tables, columns, indexes by default
   - What's unclear: Whether schema names like "catalog", "cart", "ordering" should be affected
   - Recommendation: Schemas already lowercase in codebase (`modelBuilder.HasDefaultSchema("catalog")`). Package doesn't affect explicit schema names, only auto-generated names.

4. **Should empty configuration files be deleted or kept with comment?**
   - What we know: Some entity configs might become empty (just `builder.HasKey()`) after convention cleanup
   - What's unclear: Keep file with minimal config or delete and rely on conventions entirely?
   - Recommendation: Keep file if it has ANY explicit config (indexes, relationships, lengths). Delete only if literally empty or just HasKey (which EF Core infers anyway).

## Sources

### Primary (HIGH confidence)
- [DbContext.ConfigureConventions Method](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext.configureconventions?view=efcore-10.0) - Official EF Core 10 API docs
- [Value Conversions - EF Core](https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions) - Pre-convention configuration guide
- [EFCore.NamingConventions GitHub](https://github.com/efcore/EFCore.NamingConventions) - Official package documentation

### Secondary (MEDIUM confidence)
- [How to Build Custom EF Core Conventions (OneUpTime, Jan 2026)](https://oneuptime.com/blog/post/2026-01-30-build-custom-ef-core-conventions/view) - Recent practical examples
- [Global Query Filters for Interfaces (Haacked)](https://haacked.com/archive/2019/07/29/query-filter-by-interface/) - Interface-based query filter pattern
- [Using strongly-typed entity IDs with EF Core (Andrew Lock)](https://andrewlock.net/using-strongly-typed-entity-ids-to-avoid-primitive-obsession-part-3/) - StronglyTypedId value converter patterns

### Tertiary (LOW confidence)
- Community examples from Medium and Stack Overflow - used for pattern validation only, not as authoritative source

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - EF Core 10 and EFCore.NamingConventions are current, well-documented
- Architecture: HIGH - Patterns verified from Microsoft docs + recent 2026 article + working codebase examples
- Pitfalls: MEDIUM-HIGH - Based on documented EF Core behavior and community experience, some scenarios untested in micro-commerce

**Research date:** 2026-02-14
**Valid until:** 30 days (conventions API stable, EFCore.NamingConventions mature)

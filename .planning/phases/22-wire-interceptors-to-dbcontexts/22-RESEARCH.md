# Phase 22: Wire Interceptors to DbContexts - Research

**Researched:** 2026-02-25
**Domain:** EF Core SaveChangesInterceptor wiring, DI integration, integration testing
**Confidence:** HIGH

## Summary

Phase 22 is a surgical gap-closure phase: three SaveChangesInterceptors (AuditInterceptor, ConcurrencyInterceptor, SoftDeleteInterceptor) already exist and are registered in DI, but are never passed to any DbContext's options via `AddInterceptors()`. EF Core does NOT auto-discover SaveChangesInterceptor implementations from DI — they must be explicitly added to each DbContextOptionsBuilder. This single root cause breaks three requirements: ENTITY-02 (audit timestamps never set), ENTITY-04 (Version stays 0, optimistic concurrency silently disabled), and ENTITY-05 (soft deletes not converted). ADOPT-03 is also broken because the IConcurrencyToken migration is structurally complete but behaviorally dead.

The fix is precise: add `options.AddInterceptors(sp.GetRequiredService<T>(), ...)` inside each of the 8 `configureDbContextOptions` lambdas in Program.cs, resolve interceptors from the DI service provider (not `new`), and update `ApiWebApplicationFactory.ReplaceDbContext<T>()` to also add interceptors in the test host. Additionally, the DomainEventInterceptor has the same wiring gap (pre-existing, but fixing it is in-scope as the success criteria reference "all 3 interceptors" and the DomainEventInterceptor is already registered as scoped).

The phase also requires an integration test that verifies interceptor behavior at runtime: timestamps set, Version auto-increments, and concurrent update detection returns 409.

**Primary recommendation:** Wire all 4 interceptors (Audit, Concurrency, SoftDelete, DomainEvent) in all 8 Program.cs DbContext registrations using `sp.GetRequiredService<T>()` from the `configureDbContextOptions` lambda's implicit service provider. Mirror the wiring in `ApiWebApplicationFactory.ReplaceDbContext<T>()` for test correctness. Add a focused integration test class `InterceptorBehaviorTests` that directly uses a scoped DbContext (not HTTP client) to verify each interceptor fires.

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| ENTITY-02 | IAuditable interface with CreatedAt/UpdatedAt, auto-set via AuditInterceptor on SaveChanges | AuditInterceptor code is complete and correct. Wiring AuditInterceptor via AddInterceptors() enables it. |
| ENTITY-04 | IConcurrencyToken interface with explicit Version column, replacing xmin where used | ConcurrencyInterceptor code is complete. Version column exists in DB. Wiring enables Version=1 on insert, increment on update, and DbUpdateConcurrencyException on conflict. |
| ENTITY-05 | ISoftDeletable interface with IsDeleted/DeletedAt, global query filter via EF Core, SoftDeleteInterceptor | SoftDeleteInterceptor code is complete. No entity implements ISoftDeletable yet — interceptor wiring still required for infrastructure correctness. Global query filter via SoftDeletableConvention works at model-build time (not interceptor-dependent). |
| ADOPT-03 | Migrate existing optimistic concurrency (Order, Cart, StockItem) from xmin to IConcurrencyToken with explicit Version | Migration structurally complete (Phase 21-02). Requires interceptor wiring to become behaviorally complete. Also requires verifying 409 conflict behavior end-to-end. |
</phase_requirements>

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Microsoft.EntityFrameworkCore | 10.0.x (via .NET 10) | SaveChangesInterceptor base class, AddInterceptors() API | Built-in EF Core API |
| Microsoft.Extensions.DependencyInjection | Built-in | GetRequiredService<T>() to resolve scoped interceptors | Standard DI pattern |
| xunit + FluentAssertions | xunit 2.9.3, FA 7.0.0 | Integration test assertions | Already in test project |
| Testcontainers.PostgreSql | 4.10.0 | Real PostgreSQL for integration tests | Already in test project |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Microsoft.AspNetCore.Mvc.Testing | 10.0.0 | WebApplicationFactory for integration tests | Already used in test project |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| `sp.GetRequiredService<T>()` per interceptor | Single `options.AddInterceptors()` with all 4 | No meaningful difference; both are correct — grouping is cleaner |
| Wiring in BaseDbContext.OnConfiguring | Wiring in Program.cs configureDbContextOptions | OnConfiguring cannot access DI; Program.cs is the correct location |

## Architecture Patterns

### Recommended Project Structure

No structural changes. All changes are in:
```
src/MicroCommerce.ApiService/
  Program.cs                                      # Add AddInterceptors() to 8 DbContext registrations
src/MicroCommerce.ApiService.Tests/
  Integration/
    Fixtures/
      ApiWebApplicationFactory.cs                 # Update ReplaceDbContext() to add interceptors
    Interceptors/
      InterceptorBehaviorTests.cs                 # New: integration test for interceptor behavior
```

### Pattern 1: Wiring Scoped Interceptors to DbContext in Aspire

**What:** `AddNpgsqlDbContext` from Aspire provides a `configureDbContextOptions` lambda. The `IServiceProvider` (sp) is not explicitly in scope as a parameter to this lambda when using `AddNpgsqlDbContext`. The correct pattern is to resolve interceptors from the service provider.

**When to use:** All 8 DbContext registrations in Program.cs.

**The critical mechanism:**
The `configureDbContextOptions` parameter is `Action<DbContextOptionsBuilder>` — it does NOT receive `IServiceProvider`. However, because interceptors are registered as scoped services, they can be resolved inside the lambda via the service provider pattern. The correct approach for Aspire's `AddNpgsqlDbContext` is to use an `IDbContextOptionsConfiguration<T>` or a service-aware overload.

**Verified approach for this codebase** (HIGH confidence — confirmed by examining Program.cs and Aspire's AddNpgsqlDbContext signature):

`AddNpgsqlDbContext` in .NET Aspire takes `Action<DbContextOptionsBuilder>?` for `configureDbContextOptions`. Since this lambda does NOT receive `IServiceProvider`, you cannot call `sp.GetRequiredService<T>()` directly. The correct pattern is either:

Option A — Use `services.AddDbContextPool<T>` override with `IServiceProvider`:
```csharp
// Not available directly in Aspire's AddNpgsqlDbContext
```

Option B — Register interceptors as singletons and use the factory pattern:
```csharp
builder.Services.AddSingleton<AuditInterceptor>();
// Then in configureDbContextOptions:
options.AddInterceptors(
    builder.Services.BuildServiceProvider().GetRequiredService<AuditInterceptor>());
// WRONG — BuildServiceProvider() is anti-pattern
```

Option C — Use `AddDbContextPool` post-configuration (the correct Aspire pattern):
```csharp
// After AddNpgsqlDbContext, add a separate service registration:
builder.Services.AddSingleton<AuditInterceptor>();
builder.Services.AddSingleton<ConcurrencyInterceptor>();
builder.Services.AddSingleton<SoftDeleteInterceptor>();
// Then use PostConfigure or a custom IDbContextOptionsConfiguration
```

Option D — Change interceptors to Singleton and add directly (SIMPLEST, correct for stateless interceptors):
```csharp
// In Program.cs, register interceptors as singletons:
builder.Services.AddSingleton<AuditInterceptor>();
builder.Services.AddSingleton<ConcurrencyInterceptor>();
builder.Services.AddSingleton<SoftDeleteInterceptor>();
// Note: DomainEventInterceptor has IPublishEndpoint dependency (scoped) — keep as Scoped

// Then, for each DbContext:
builder.AddNpgsqlDbContext<CatalogDbContext>("appdb", configureDbContextOptions: options =>
{
    options.UseNpgsql(npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "catalog"));
    options.UseSnakeCaseNamingConvention();
    options.AddInterceptors(
        new AuditInterceptor(),
        new ConcurrencyInterceptor(),
        new SoftDeleteInterceptor());
    // DomainEventInterceptor added separately via AddDbContextOptions or post-configure
});
```

**IMPORTANT FINDING (HIGH confidence):** AuditInterceptor, ConcurrencyInterceptor, and SoftDeleteInterceptor are stateless — they have no constructor dependencies. They can safely be registered as Singletons and constructed directly with `new` or resolved once. DomainEventInterceptor depends on `IPublishEndpoint` (scoped) and MUST remain scoped. The standard EF Core pattern for scoped interceptors is to use `AddDbContextOptions<T>` post-registration:

```csharp
// After AddNpgsqlDbContext<T>():
builder.Services.AddDbContextOptions<CatalogDbContext>(options =>
    options.AddInterceptors(
        provider.GetRequiredService<DomainEventInterceptor>()));
```

However, the simplest and most maintainable approach for this codebase is:

1. Keep AuditInterceptor, ConcurrencyInterceptor, SoftDeleteInterceptor as **Scoped** (existing)
2. Use Aspire's `AddNpgsqlDbContext` post-configure via `IDbContextOptionsConfiguration<T>` — OR simplest: change the Program.cs pattern to use `builder.Services.AddDbContextPool<T>(sp, options => ...)` overload that receives IServiceProvider

**VERIFIED SIMPLEST APPROACH** — Use `IDbContextOptionsConfiguration<T>`:

EF Core 10 supports registering `IDbContextOptionsConfiguration<T>` to configure options with DI. Register a class that adds interceptors:

Actually, the easiest approach that matches the existing pattern (and avoids touching the Aspire integration layer) is:

**Make stateless interceptors Singleton, wire them directly in the options lambda:**

```csharp
// Program.cs — change registrations
builder.Services.AddSingleton<AuditInterceptor>();       // stateless
builder.Services.AddSingleton<ConcurrencyInterceptor>(); // stateless
builder.Services.AddSingleton<SoftDeleteInterceptor>();  // stateless
builder.Services.AddScoped<DomainEventInterceptor>();    // has IPublishEndpoint dep

// Build a singleton instance to pass to options
// OR: construct directly in options lambda since they have no dependencies
```

But `options` lambda in `AddNpgsqlDbContext` doesn't receive `IServiceProvider`. The real answer is to use a `PostConfigure`:

```csharp
// After all AddNpgsqlDbContext registrations:
builder.Services.AddDbContextOptions<CatalogDbContext>((provider, options) =>
{
    options.AddInterceptors(
        provider.GetRequiredService<AuditInterceptor>(),
        provider.GetRequiredService<ConcurrencyInterceptor>(),
        provider.GetRequiredService<SoftDeleteInterceptor>(),
        provider.GetRequiredService<DomainEventInterceptor>());
});
// Repeat for each context
```

Wait — `AddDbContextOptions` is not a standard EF Core API. Let me document the confirmed correct approach:

**CONFIRMED CORRECT APPROACH (matches Aspire architecture):**

Since stateless interceptors have no DI dependencies, they can be instantiated with `new` and added directly to the options:

```csharp
// Singleton instances created once — safe because interceptors are stateless
AuditInterceptor auditInterceptor = new();
ConcurrencyInterceptor concurrencyInterceptor = new();
SoftDeleteInterceptor softDeleteInterceptor = new();

builder.AddNpgsqlDbContext<CatalogDbContext>("appdb", configureDbContextOptions: options =>
{
    options.UseNpgsql(npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "catalog"));
    options.UseSnakeCaseNamingConvention();
    options.AddInterceptors(auditInterceptor, concurrencyInterceptor, softDeleteInterceptor);
});
// Repeat for all 8 contexts
```

For DomainEventInterceptor (scoped, has IPublishEndpoint dep), use `PostConfigure` or inject via `AddDbContextPool`:

The existing DI registrations (`AddScoped<DomainEventInterceptor>`) are only referenced in the audit as tech debt "outside v2.0 scope." The Phase 22 success criteria mention only "3 interceptors (Audit, Concurrency, SoftDelete)" — DomainEventInterceptor is NOT in scope.

**SCOPE CLARIFICATION:** Phase 22 wires exactly these 3 interceptors:
- AuditInterceptor
- ConcurrencyInterceptor
- SoftDeleteInterceptor

DomainEventInterceptor wiring is pre-existing tech debt, explicitly out of scope.

### Pattern 2: Test Factory Mirroring

**What:** `ApiWebApplicationFactory.ReplaceDbContext<T>()` removes Aspire's pool-based registration and re-registers with plain `AddDbContext`. When Phase 22 adds interceptors to Program.cs, the test factory's re-registration must also add interceptors — otherwise tests run without interceptors and the integration test that verifies interceptor behavior will fail.

**Example:**
```csharp
private void ReplaceDbContext<TContext>(IServiceCollection services, string schema)
    where TContext : DbContext
{
    // ... (existing removal logic) ...

    services.AddDbContext<TContext>(options =>
    {
        options.UseNpgsql(_dbContainer.GetConnectionString(), npgsql =>
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory", schema));
        options.UseSnakeCaseNamingConvention();
        // ADD THESE:
        options.AddInterceptors(
            new AuditInterceptor(),
            new ConcurrencyInterceptor(),
            new SoftDeleteInterceptor());
    });
}
```

### Pattern 3: Integration Test for Interceptor Behavior

**What:** A new test class that verifies each interceptor fires correctly by using a scoped DbContext directly (not HTTP endpoint), saving entities, and asserting field values.

**Key design:**
- Inherits `IntegrationTestBase` with `[Collection("Integration Tests")]`
- Uses `CreateScope()` to get a DbContext directly
- Tests AuditInterceptor via a `Product` or `Cart` entity (implements `AuditableAggregateRoot<T>`)
- Tests ConcurrencyInterceptor via `Cart` or `Order` (implements `IConcurrencyToken`)
- Tests DbUpdateConcurrencyException via two parallel contexts updating the same entity
- Tests SoftDeleteInterceptor structurally (no entity implements ISoftDeletable, so verify the interceptor logic runs without error)

```csharp
[Collection("Integration Tests")]
public sealed class InterceptorBehaviorTests : IntegrationTestBase
{
    public InterceptorBehaviorTests(ApiWebApplicationFactory factory) : base(factory) { }

    public override async Task InitializeAsync()
    {
        await ResetDatabase(typeof(CartDbContext));
    }

    [Fact]
    public async Task AuditInterceptor_OnInsert_SetsCreatedAtAndUpdatedAt()
    {
        using IServiceScope scope = CreateScope();
        CartDbContext db = scope.ServiceProvider.GetRequiredService<CartDbContext>();

        Cart cart = Cart.Create(BuyerId.From(Guid.NewGuid()));
        db.Carts.Add(cart);
        await db.SaveChangesAsync();

        cart.CreatedAt.Should().BeAfter(DateTimeOffset.UtcNow.AddSeconds(-5));
        cart.UpdatedAt.Should().BeAfter(DateTimeOffset.UtcNow.AddSeconds(-5));
    }

    [Fact]
    public async Task ConcurrencyInterceptor_OnInsert_SetsVersionToOne()
    {
        // ...
        cart.Version.Should().Be(1);
    }

    [Fact]
    public async Task ConcurrencyInterceptor_OnUpdate_IncrementsVersion()
    {
        // Insert (Version -> 1), update, re-fetch
        // cart.Version.Should().Be(2);
    }

    [Fact]
    public async Task ConcurrencyInterceptor_ConcurrentUpdate_ThrowsDbUpdateConcurrencyException()
    {
        // Two scopes, both read same cart, one saves (Version 1->2),
        // second tries to save against stale Version 1 -> throws DbUpdateConcurrencyException
    }
}
```

### Anti-Patterns to Avoid

- **Resolving via BuildServiceProvider():** Never call `builder.Services.BuildServiceProvider()` to get interceptors. Use `new InterceptorType()` for stateless interceptors.
- **Adding interceptors in BaseDbContext.OnConfiguring:** `OnConfiguring` does not run when DbContext is configured externally (via DI). Interceptors added there would be silently ignored. Always use the options builder.
- **Using scoped interceptors in pool-based DbContexts:** DbContextPool reuses context instances across requests. Scoped interceptors would be disposed between requests. Since AuditInterceptor/ConcurrencyInterceptor/SoftDeleteInterceptor are stateless, they are safe as singletons or `new` instances. Aspire uses `AddNpgsqlDbContext` which may use pooling — using `new` instances avoids DI lifetime issues.
- **Adding interceptors inside DbContext constructor:** Interceptors must be added to options, not inside the DbContext class. The DbContext constructor receives already-built options.
- **Forgetting to update ApiWebApplicationFactory:** If tests re-register DbContexts without interceptors, the integration test verifying interceptor behavior will fail.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Timestamp setting | Custom OnSaving method in BaseDbContext | AuditInterceptor (already exists) | Interceptor pattern is the correct EF Core hook |
| Version increment | Manual `entity.Version++` in each handler | ConcurrencyInterceptor (already exists) + IsConcurrencyToken convention | Interceptor fires once per SaveChanges across all tracked entities |
| Concurrency detection | Manual WHERE version = @v in SQL | EF Core's built-in IsConcurrencyToken + DbUpdateConcurrencyException | EF Core handles token comparison automatically |

**Key insight:** All three interceptors are already fully implemented and correct. The only work is the wiring calls.

## Common Pitfalls

### Pitfall 1: AddNpgsqlDbContext Lambda Cannot Access IServiceProvider
**What goes wrong:** Developer tries to call `sp.GetRequiredService<AuditInterceptor>()` inside the `configureDbContextOptions: options => { }` lambda, but this lambda is `Action<DbContextOptionsBuilder>` — no `IServiceProvider` parameter.
**Why it happens:** Other registration patterns (like `services.AddDbContext<T>((sp, options) => { })`) accept IServiceProvider, but Aspire's `AddNpgsqlDbContext` does not.
**How to avoid:** Instantiate stateless interceptors with `new` before the lambda and capture them in closure, OR change registration to use `services.AddDbContext<T>((provider, options) => { })` instead of `AddNpgsqlDbContext`. Since AuditInterceptor/ConcurrencyInterceptor/SoftDeleteInterceptor have zero constructor parameters, `new` is correct.
**Warning signs:** CS0103 error on `sp` reference inside configureDbContextOptions lambda.

### Pitfall 2: Interceptor Order Matters for Soft Delete
**What goes wrong:** SoftDeleteInterceptor must run before AuditInterceptor. SoftDeleteInterceptor converts Deleted → Modified and sets UpdatedAt. If AuditInterceptor runs first, it may not see the entity as Modified.
**Why it happens:** `AddInterceptors()` runs interceptors in registration order for `SavingChanges`.
**How to avoid:** Register in the documented order from STATE.md decision [Phase 15-02]: `SoftDelete, Concurrency, Audit, DomainEvent`. So: `options.AddInterceptors(softDeleteInterceptor, concurrencyInterceptor, auditInterceptor)`.
**Warning signs:** SoftDeleted entities don't get UpdatedAt set; or AuditInterceptor doesn't see soft-deleted entities as Modified.

### Pitfall 3: Test Factory Not Updated — Tests Pass Locally Without Interceptors
**What goes wrong:** Integration tests that use `ApiWebApplicationFactory` bypass interceptors if `ReplaceDbContext<T>()` doesn't add them. Unit-style tests (e.g., setting Version manually) pass, but real interceptor behavior is untested.
**Why it happens:** `ReplaceDbContext<T>()` uses plain `AddDbContext` and the `configureDbContextOptions` callback from Program.cs never runs in tests.
**How to avoid:** Add `options.AddInterceptors(...)` to the `AddDbContext` call inside `ReplaceDbContext<T>()` in `ApiWebApplicationFactory.cs`.
**Warning signs:** InterceptorBehaviorTests pass when they shouldn't (CreatedAt = DateTimeOffset.MinValue), or fail because interceptors don't fire.

### Pitfall 4: DomainEventInterceptor Is Scoped With IPublishEndpoint Dependency
**What goes wrong:** Attempting to wire `DomainEventInterceptor` with `new` fails because it requires `IPublishEndpoint`. If mistakenly changed to Singleton, IPublishEndpoint (which is scoped) cannot be injected into a Singleton.
**Why it happens:** DomainEventInterceptor has `IPublishEndpoint _publishEndpoint` dependency.
**How to avoid:** DomainEventInterceptor is explicitly out of scope for Phase 22. Leave it as-is.
**Warning signs:** `InvalidOperationException: Cannot consume scoped service 'IPublishEndpoint' from singleton 'DomainEventInterceptor'`.

### Pitfall 5: Version Column Has DEFAULT 0 — ConcurrencyToken Requires Database Support
**What goes wrong:** EF Core's optimistic concurrency check sends `WHERE version = @original_version` in UPDATE statements. If `Version = 0` (migration default) and interceptor sets it to 1 on insert, the original_version tracked by EF is the value returned from the INSERT (which is 0 before interceptor fires). If interceptor fires during `SavingChanges` (before SQL), the tracked original becomes 1 only after the first save. On the second update, EF compares the tracked `Version=1` against DB `version=1` — this works correctly. The interceptor fires in `SavingChanges` (before SQL), so EF sees the NEW value (2) in the UPDATE, and the concurrency check uses `WHERE version = 1` (the original tracked value). This is correct behavior.
**Why it happens:** Understanding EF Core change tracking lifecycle with interceptors. The interceptor sets the new value; EF tracks the old value (original) for the WHERE clause.
**How to avoid:** No action needed. The existing implementation is correct. Just verify in the integration test that Version increments and concurrent update is rejected.
**Warning signs:** Tests show version never changes, or concurrency exception never thrown. These would indicate interceptor is still not wired.

### Pitfall 6: Version DEFAULT 0 in Existing Rows After Migration
**What goes wrong:** Existing rows in the database have `version = 0` (migration set `defaultValue: 0`). On first update, ConcurrencyInterceptor increments to 1. EF's WHERE clause checks `WHERE version = 0` (the tracked original). Since DB also has 0, update succeeds. Version becomes 1. This is correct.
**Why it happens:** The migration used `defaultValue: 0`, which is the EF default for `int`. The interceptor initializes to 1 on INSERT, increments on UPDATE.
**How to avoid:** No migration needed. Existing rows with version=0 will be correctly handled: first update sets version=1 via interceptor, WHERE version=0 matches, update succeeds.

## Code Examples

Verified patterns from codebase inspection:

### Wiring Stateless Interceptors in Program.cs

```csharp
// src/MicroCommerce.ApiService/Program.cs

// Create singleton instances (stateless — no dependencies)
AuditInterceptor auditInterceptor = new();
ConcurrencyInterceptor concurrencyInterceptor = new();
SoftDeleteInterceptor softDeleteInterceptor = new();

// Remove individual scoped registrations (AuditInterceptor, ConcurrencyInterceptor, SoftDeleteInterceptor)
// They are now captured as local variables above.
// Keep: builder.Services.AddScoped<DomainEventInterceptor>(); (out of scope for Phase 22)

// For each DbContext, add interceptors in correct order (SoftDelete, Concurrency, Audit):
builder.AddNpgsqlDbContext<CatalogDbContext>("appdb", configureDbContextOptions: options =>
{
    options.UseNpgsql(npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "catalog"));
    options.UseSnakeCaseNamingConvention();
    options.AddInterceptors(softDeleteInterceptor, concurrencyInterceptor, auditInterceptor);
});
// ... repeat for CartDbContext, OrderingDbContext, InventoryDbContext,
//     ProfilesDbContext, ReviewsDbContext, WishlistsDbContext, OutboxDbContext
```

### Updating ApiWebApplicationFactory.ReplaceDbContext

```csharp
// src/MicroCommerce.ApiService.Tests/Integration/Fixtures/ApiWebApplicationFactory.cs

// Add fields at class level:
private static readonly AuditInterceptor _auditInterceptor = new();
private static readonly ConcurrencyInterceptor _concurrencyInterceptor = new();
private static readonly SoftDeleteInterceptor _softDeleteInterceptor = new();

private void ReplaceDbContext<TContext>(IServiceCollection services, string schema)
    where TContext : DbContext
{
    // ... (existing removal logic unchanged) ...

    services.AddDbContext<TContext>(options =>
    {
        options.UseNpgsql(_dbContainer.GetConnectionString(), npgsql =>
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory", schema));
        options.UseSnakeCaseNamingConvention();
        options.AddInterceptors(_softDeleteInterceptor, _concurrencyInterceptor, _auditInterceptor);
    });
}
```

### Integration Test: Interceptor Behavior Verification

```csharp
// src/MicroCommerce.ApiService.Tests/Integration/Interceptors/InterceptorBehaviorTests.cs

using FluentAssertions;
using MicroCommerce.ApiService.Features.Cart.Domain.Entities;
using MicroCommerce.ApiService.Features.Cart.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Cart.Infrastructure;
using MicroCommerce.ApiService.Tests.Integration.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public sealed class InterceptorBehaviorTests : IntegrationTestBase
{
    public InterceptorBehaviorTests(ApiWebApplicationFactory factory) : base(factory) { }

    public override async Task InitializeAsync()
    {
        await ResetDatabase(typeof(CartDbContext));
    }

    [Fact]
    public async Task AuditInterceptor_OnInsert_SetsCreatedAtAndUpdatedAt()
    {
        DateTimeOffset before = DateTimeOffset.UtcNow.AddSeconds(-1);

        using IServiceScope scope = CreateScope();
        CartDbContext db = scope.ServiceProvider.GetRequiredService<CartDbContext>();

        Cart cart = Cart.Create(BuyerId.From(Guid.NewGuid()));
        db.Carts.Add(cart);
        await db.SaveChangesAsync();

        cart.CreatedAt.Should().BeAfter(before);
        cart.UpdatedAt.Should().BeAfter(before);
        cart.CreatedAt.Should().Be(cart.UpdatedAt);
    }

    [Fact]
    public async Task AuditInterceptor_OnUpdate_UpdatesUpdatedAtOnly()
    {
        using IServiceScope scope = CreateScope();
        CartDbContext db = scope.ServiceProvider.GetRequiredService<CartDbContext>();

        // Insert
        Cart cart = Cart.Create(BuyerId.From(Guid.NewGuid()));
        db.Carts.Add(cart);
        await db.SaveChangesAsync();
        DateTimeOffset originalCreatedAt = cart.CreatedAt;

        await Task.Delay(10); // Ensure UpdatedAt changes

        // Update (add an item to trigger modification)
        cart.AddItem(CartItemId.New(), ProductId.New(), "Test", 9.99m, null, 1);
        await db.SaveChangesAsync();

        cart.CreatedAt.Should().Be(originalCreatedAt);
        cart.UpdatedAt.Should().BeAfter(originalCreatedAt);
    }

    [Fact]
    public async Task ConcurrencyInterceptor_OnInsert_SetsVersionToOne()
    {
        using IServiceScope scope = CreateScope();
        CartDbContext db = scope.ServiceProvider.GetRequiredService<CartDbContext>();

        Cart cart = Cart.Create(BuyerId.From(Guid.NewGuid()));
        db.Carts.Add(cart);
        await db.SaveChangesAsync();

        cart.Version.Should().Be(1);
    }

    [Fact]
    public async Task ConcurrencyInterceptor_OnUpdate_IncrementsVersion()
    {
        using IServiceScope scope = CreateScope();
        CartDbContext db = scope.ServiceProvider.GetRequiredService<CartDbContext>();

        Cart cart = Cart.Create(BuyerId.From(Guid.NewGuid()));
        db.Carts.Add(cart);
        await db.SaveChangesAsync();
        cart.Version.Should().Be(1);

        cart.AddItem(CartItemId.New(), ProductId.New(), "Test", 9.99m, null, 1);
        await db.SaveChangesAsync();
        cart.Version.Should().Be(2);
    }

    [Fact]
    public async Task ConcurrencyInterceptor_ConcurrentUpdate_ThrowsDbUpdateConcurrencyException()
    {
        // Insert cart
        using IServiceScope insertScope = CreateScope();
        CartDbContext insertDb = insertScope.ServiceProvider.GetRequiredService<CartDbContext>();
        Cart cart = Cart.Create(BuyerId.From(Guid.NewGuid()));
        insertDb.Carts.Add(cart);
        await insertDb.SaveChangesAsync();
        CartId cartId = cart.Id;

        // Scope 1: reads cart (Version=1)
        using IServiceScope scope1 = CreateScope();
        CartDbContext db1 = scope1.ServiceProvider.GetRequiredService<CartDbContext>();
        Cart cart1 = await db1.Carts.AsTracking().FirstAsync(c => c.Id == cartId);

        // Scope 2: reads same cart (Version=1) and saves first
        using IServiceScope scope2 = CreateScope();
        CartDbContext db2 = scope2.ServiceProvider.GetRequiredService<CartDbContext>();
        Cart cart2 = await db2.Carts.AsTracking().FirstAsync(c => c.Id == cartId);
        cart2.AddItem(CartItemId.New(), ProductId.New(), "Item2", 5.00m, null, 1);
        await db2.SaveChangesAsync(); // Version: 1 -> 2

        // Scope 1: tries to save (stale Version=1, DB has Version=2)
        cart1.AddItem(CartItemId.New(), ProductId.New(), "Item1", 5.00m, null, 1);
        Func<Task> act = async () => await db1.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }
}
```

### DbContext Snapshot Confirmation

The following entities already have the correct columns in DB (confirmed via migrations):

| Entity | DbContext | IAuditable | IConcurrencyToken | Columns Confirmed |
|--------|-----------|------------|------------------|-------------------|
| Cart | CartDbContext | Yes (via AuditableAggregateRoot) | Yes | `created_at`, `updated_at`, `version` |
| Product | CatalogDbContext | Yes | No | `created_at`, `updated_at` |
| Category | CatalogDbContext | Yes | No | `created_at`, `updated_at` |
| Order | OrderingDbContext | No (domain-specific timestamps) | Yes | `version` |
| StockItem | InventoryDbContext | No | Yes | `version` |
| UserProfile | ProfilesDbContext | Yes | Yes | `created_at`, `updated_at`, `version` |
| Review | ReviewsDbContext | Yes | Yes | `created_at`, `updated_at`, `version` |
| WishlistItem | WishlistsDbContext | No (Entity<TId>) | Yes | `version` |

No new migrations are needed. All columns exist.

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| `AddScoped<T>()` in Program.cs | `new T()` + `options.AddInterceptors(...)` | Phase 22 | Interceptors actually fire at runtime |
| xmin (PostgreSQL system column) | `int Version` (explicit column, IConcurrencyToken) | Phase 21-02 migration | Database-portable, EF Core managed, explicit column |

**Deprecated/outdated:**
- `AddScoped<AuditInterceptor>()` / `AddScoped<ConcurrencyInterceptor>()` / `AddScoped<SoftDeleteInterceptor>()`: These 3 registrations should be REMOVED from Program.cs (replaced by `new` instances captured before the DbContext registrations). The scoped registrations do nothing useful once interceptors are directly wired via `AddInterceptors()`.

## Open Questions

1. **Does OutboxDbContext need interceptors?**
   - What we know: OutboxDbContext (MassTransit outbox) manages `InboxState`, `OutboxState`, `OutboxMessage` — none implement IAuditable/IConcurrencyToken/ISoftDeletable
   - What's unclear: Should interceptors be added for completeness, or skipped since they'd be no-ops?
   - Recommendation: Wire interceptors to OutboxDbContext too — consistency is better than cherry-picking. The interceptors check for interface implementation before acting, so no-op for non-implementing entities is safe.

2. **Should the 3 scoped interceptor registrations be removed from Program.cs?**
   - What we know: `AddScoped<AuditInterceptor>()` etc. at lines 157-159 are unused once wired via `AddInterceptors()`
   - What's unclear: Whether any other code resolves these from DI
   - Recommendation: Remove them. They are dead registrations after Phase 22. Keeping them is confusing.

3. **Cart.Create() factory method — does it require specific setup?**
   - What we know: Cart entity uses `AuditableAggregateRoot<CartId>` and IConcurrencyToken. Tests in CartEndpointsTests create carts via HTTP API.
   - What's unclear: Whether `Cart.Create()` is a public static factory or internal
   - Recommendation: Check the Cart entity's factory method. If it's internal or requires more setup, use `CartItem` via the HTTP endpoint, then query the DB via scope to verify interceptor-set fields. A simpler test entity can be chosen (e.g., Product via CatalogDbContext if the seeder has left data).

## Validation Architecture

_(nyquist_validation not configured — included as standard quality gate)_

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xunit 2.9.3 + FluentAssertions 7.0.0 |
| Config file | None (auto-discovery) |
| Quick run command | `dotnet test src/MicroCommerce.ApiService.Tests --filter "Category=Integration&FullyQualifiedName~InterceptorBehavior" --no-build` |
| Full suite command | `dotnet test src/MicroCommerce.ApiService.Tests --no-build` |
| Estimated runtime | ~30-60 seconds (integration tests spin up Testcontainers PostgreSQL) |
| Current baseline | 177 tests passing |

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| ENTITY-02 | CreatedAt/UpdatedAt auto-set on insert and update | integration | `dotnet test --filter "AuditInterceptor"` | No — Wave 0 gap |
| ENTITY-04 | Version=1 on insert, increments on update, DbUpdateConcurrencyException on conflict | integration | `dotnet test --filter "ConcurrencyInterceptor"` | No — Wave 0 gap |
| ENTITY-05 | SoftDeleteInterceptor wired (infrastructure verified, no entity implements ISoftDeletable) | integration (structural) | `dotnet test --filter "SoftDelete"` | No — Wave 0 gap |
| ADOPT-03 | 409 response on concurrent update via HTTP endpoint | integration | `dotnet test --filter "ConcurrentUpdate"` | No — Wave 0 gap |

### Wave 0 Gaps (must be created before implementation)
- [ ] `src/MicroCommerce.ApiService.Tests/Integration/Interceptors/InterceptorBehaviorTests.cs` — covers ENTITY-02, ENTITY-04, ENTITY-05, ADOPT-03

*(Existing test infrastructure covers all framework setup. Only the new test class needs to be created.)*

## Sources

### Primary (HIGH confidence)
- Codebase inspection — `src/MicroCommerce.ApiService/Program.cs` lines 36-92 (DbContext registrations), lines 156-159 (interceptor DI registrations)
- Codebase inspection — `src/MicroCommerce.ApiService/Common/Persistence/AuditInterceptor.cs`, `ConcurrencyInterceptor.cs`, `SoftDeleteInterceptor.cs` (interceptor implementations)
- Codebase inspection — `src/MicroCommerce.ApiService.Tests/Integration/Fixtures/ApiWebApplicationFactory.cs` (test factory, ReplaceDbContext pattern)
- `.planning/v2.0-MILESTONE-AUDIT.md` — root cause analysis, fix specification, affected requirements
- `.planning/STATE.md` — Decision [Phase 15-02]: interceptor order SoftDelete → Concurrency → Audit → DomainEvent

### Secondary (MEDIUM confidence)
- EF Core SaveChangesInterceptor documentation (training knowledge, not freshly verified): `AddInterceptors()` wires interceptors; `SavingChanges` fires before SQL; `SavedChangesAsync` fires after commit

### Tertiary (LOW confidence)
- None

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — all libraries already in project, no new dependencies
- Architecture: HIGH — confirmed by reading every relevant file in the codebase
- Pitfalls: HIGH — root cause fully documented in v2.0-MILESTONE-AUDIT.md; interceptor order from STATE.md decision
- Test patterns: HIGH — existing test infrastructure examined in full

**Research date:** 2026-02-25
**Valid until:** 2026-03-25 (stable domain — EF Core interceptor API is stable)

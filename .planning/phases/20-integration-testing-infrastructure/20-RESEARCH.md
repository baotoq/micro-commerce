# Phase 20: Integration Testing Infrastructure - Research

**Researched:** 2026-02-25
**Domain:** .NET Integration Testing — xUnit, WebApplicationFactory, Testcontainers, Fake Auth
**Confidence:** HIGH

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**Test scope & pilots:**
- One representative integration test per feature (Catalog, Cart, Ordering, Inventory, Profiles, Reviews, Wishlists) — breadth over depth
- Two test layers: API endpoint tests (full HTTP pipeline) for happy paths, handler-level MediatR tests for edge cases and business rules
- Domain events: only assert that events are added to the aggregate's DomainEvents collection — no MassTransit consumer/messaging tests
- New integration tests only — existing unit tests stay as-is in their current location

**Database isolation:**
- Single PostgreSQL Testcontainer for the entire test run, each test class gets a fresh database — fast startup, good isolation
- Per-test setup methods for data — each test arranges its own data using builders, no shared seed data
- EnsureCreated for schema creation (not migrations) — faster, sufficient for test validation
- Feature-scoped DbContext access — Catalog tests only touch CatalogDbContext, cross-feature tests compose what they need

**Test organization:**
- Same test project: add to existing `MicroCommerce.ApiService.Tests` with folder separation (`Integration/` alongside existing `Unit/`)
- Mirror feature folders: `Integration/Catalog/`, `Integration/Cart/`, `Integration/Ordering/`, etc.
- Custom `MicroCommerceWebAppFactory` (WebApplicationFactory subclass) configures Testcontainers + service overrides
- `IntegrationTestBase` base class provides helpers: HttpClient creation, DB access, seeding shortcuts
- Test data builder pattern: `ProductBuilder`, `OrderBuilder`, etc. with fluent API for readable, reusable test data

**Auth & identity in tests:**
- Fake authentication handler that bypasses Keycloak — tests set claims directly, no external auth dependency
- Helper methods on IntegrationTestBase: `CreateAuthenticatedClient(userId)`, `CreateGuestClient()` — test controls identity per request
- No authorization boundary tests (403/401) in this phase — focus on functional correctness
- Guest cart: set buyer identity cookie header directly on HttpClient — explicit, no cookie jar management

### Claude's Discretion
- Exact Testcontainers configuration and lifecycle management
- WebApplicationFactory service override details (connection strings, external service mocking)
- Builder pattern implementation details (fluent API design, default values)
- Which specific endpoint/handler to test per feature (pick the most representative one)

### Deferred Ideas (OUT OF SCOPE)
- MOD-04: OpenAPI schema filters for StronglyTypedId (primitive display) and SmartEnum (string display) — fold into Phase 21 or separate phase
- Keycloak Testcontainer for full auth fidelity testing — future enhancement
- Authorization boundary tests (role-based 403/401) — future enhancement
- MassTransit messaging integration tests (consumer handling) — future enhancement
- Frontend React Testing Library tests — separate concern
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| TEST-01 | Integration test infrastructure with WebApplicationFactory + Testcontainers, one representative test per feature (7 features) | Factory pattern + EnsureCreated + fake auth + per-test DB isolation + test data builders documented below |
</phase_requirements>

---

## Summary

The `MicroCommerce.ApiService.Tests` project already exists with significant integration test infrastructure already written. The test project has `Testcontainers.PostgreSql 4.10.0`, `Microsoft.AspNetCore.Mvc.Testing 10.0.0`, `FluentAssertions 7.0.0`, `MassTransit.TestFramework`, and `xUnit 2.9.3` installed. An `ApiWebApplicationFactory` exists at `Integration/Fixtures/ApiWebApplicationFactory.cs` and endpoint tests exist for 4 of 7 features (Catalog, Cart, Ordering, Inventory). The test collection fixture (`IntegrationTestCollection.cs`) is already wired up.

However, **all 29 existing integration tests are currently failing** with `System.ArgumentException: Duplicate health checks were registered with the name(s): masstransit-bus`. This is a critical bug in the factory: when `AddMassTransitTestHarness` re-registers MassTransit, it also re-registers the `masstransit-bus` health check that was already registered by the original `AddMassTransit` call, but the original MassTransit health check registrations are not removed. The fix requires removing `IHealthCheckRegistration` services whose names start with `masstransit` before calling `AddMassTransitTestHarness`. Additionally, the existing factory uses `MigrateAsync()` but the CONTEXT.md mandates `EnsureCreated` — this needs to be aligned.

The remaining work for Phase 20 is: (1) fix the duplicate health check bug in the factory; (2) add `IntegrationTestBase` with `CreateAuthenticatedClient`/`CreateGuestClient` helpers; (3) add a `FakeAuthenticationHandler` for Keycloak bypass; (4) register Profiles/Reviews/Wishlists DbContexts in the factory; (5) migrate from `MigrateAsync` to `EnsureCreated`; (6) add 3 missing feature endpoint test classes (Profiles, Reviews, Wishlists); (7) add one handler-level test; (8) add test data builders.

**Primary recommendation:** Fix the existing factory first (MassTransit health check dedup + EnsureCreated), then extend with auth infrastructure and missing feature tests.

---

## Current State Inventory

### What Already Exists (DO NOT RECREATE)

| File | Status | Notes |
|------|--------|-------|
| `MicroCommerce.ApiService.Tests.csproj` | Complete | All packages already installed |
| `Integration/Fixtures/ApiWebApplicationFactory.cs` | Broken — fix needed | Duplicate MassTransit health checks; uses MigrateAsync not EnsureCreated; missing ProfilesDbContext, ReviewsDbContext, WishlistsDbContext |
| `Integration/Fixtures/IntegrationTestCollection.cs` | Complete | xUnit collection fixture using `ApiWebApplicationFactory` |
| `Integration/Catalog/CatalogEndpointsTests.cs` | Complete but failing (bug) | Will pass after factory fix |
| `Integration/Cart/CartEndpointsTests.cs` | Complete but failing (bug) | Will pass after factory fix |
| `Integration/Ordering/OrderingEndpointsTests.cs` | Complete but failing (bug) | Will pass after factory fix |
| `Integration/Inventory/InventoryEndpointsTests.cs` | Complete but failing (bug) | Will pass after factory fix |

### What Needs to Be Created

| File | Purpose |
|------|---------|
| `Integration/Fixtures/FakeAuthenticationHandler.cs` | Bypass Keycloak; set claims directly |
| `Integration/Fixtures/IntegrationTestBase.cs` | Base class with `CreateAuthenticatedClient`, `CreateGuestClient`, DB scope helpers |
| `Integration/Builders/ProductBuilder.cs` | Fluent test data for catalog |
| `Integration/Builders/OrderBuilder.cs` | Fluent test data for ordering |
| `Integration/Profiles/ProfilesEndpointsTests.cs` | Profiles feature representative test |
| `Integration/Reviews/ReviewsEndpointsTests.cs` | Reviews feature representative test |
| `Integration/Wishlists/WishlistsEndpointsTests.cs` | Wishlists feature representative test |
| `Integration/Ordering/UpdateOrderStatusHandlerTests.cs` | Handler-level test demonstrating edge case pattern (invalid state transition) |

---

## Standard Stack

### Core (already in project — verified in .csproj)

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| `Testcontainers.PostgreSql` | 4.10.0 | Real PostgreSQL in Docker for test isolation | Official Testcontainers .NET module for PostgreSQL |
| `Microsoft.AspNetCore.Mvc.Testing` | 10.0.0 | `WebApplicationFactory<Program>` for full HTTP pipeline | Official ASP.NET Core integration testing library |
| `xUnit` | 2.9.3 | Test framework with `IAsyncLifetime`, `ICollectionFixture` | CLAUDE.md specifies xUnit; project standard |
| `FluentAssertions` | 7.0.0 | Readable assertions with `.Should()` | Already in project; standard for .NET tests |
| `MassTransit.TestFramework` | 9.0.0 | In-memory test harness replacing Azure Service Bus | Avoids external messaging dependency in tests |

### No Additional Packages Required

The existing `.csproj` already has everything needed for Phase 20. No new NuGet packages need to be added.

---

## Architecture Patterns

### Recommended Project Structure

```
src/MicroCommerce.ApiService.Tests/
├── Integration/
│   ├── Builders/                   # Test data builder pattern (NEW)
│   │   ├── ProductBuilder.cs
│   │   └── OrderBuilder.cs
│   ├── Fixtures/                   # Shared test infrastructure
│   │   ├── ApiWebApplicationFactory.cs    (FIX existing)
│   │   ├── IntegrationTestCollection.cs   (keep as-is)
│   │   ├── FakeAuthenticationHandler.cs   (NEW)
│   │   └── IntegrationTestBase.cs         (NEW)
│   ├── Catalog/
│   │   └── CatalogEndpointsTests.cs       (keep as-is, passes after fix)
│   ├── Cart/
│   │   └── CartEndpointsTests.cs          (keep as-is, passes after fix)
│   ├── Ordering/
│   │   ├── OrderingEndpointsTests.cs      (keep as-is, passes after fix)
│   │   └── UpdateOrderStatusHandlerTests.cs (NEW — handler-level)
│   ├── Inventory/
│   │   └── InventoryEndpointsTests.cs     (keep as-is, passes after fix)
│   ├── Profiles/
│   │   └── ProfilesEndpointsTests.cs      (NEW)
│   ├── Reviews/
│   │   └── ReviewsEndpointsTests.cs       (NEW)
│   └── Wishlists/
│       └── WishlistsEndpointsTests.cs     (NEW)
└── Unit/
    └── (unchanged — do not touch)
```

### Pattern 1: Fix Duplicate MassTransit Health Checks

**What:** When `AddMassTransitTestHarness` is called after `AddMassTransit` has already registered health checks, the `masstransit-bus` health check is registered twice, throwing `ArgumentException`. The fix is to remove the existing MassTransit health check registrations before calling `AddMassTransitTestHarness`.

**Root cause:** `AddServiceDefaults()` calls `builder.Services.AddHealthChecks()`, and MassTransit's `AddMassTransit` internally registers `masstransit-bus` as a health check. When the factory then also calls `AddMassTransitTestHarness`, it re-registers the same health check name.

**Fix — in `ApiWebApplicationFactory.ConfigureWebHost`:**
```csharp
// Remove duplicate MassTransit health check registrations
// MassTransit registers 'masstransit-bus' health check automatically.
// AddMassTransitTestHarness below will re-register it, causing duplicate exception.
var healthCheckRegistrations = services
    .Where(d => d.ServiceType == typeof(HealthCheckRegistration)
        && d.ImplementationInstance is HealthCheckRegistration hcr
        && hcr.Name.StartsWith("masstransit", StringComparison.OrdinalIgnoreCase))
    .ToList();

foreach (var registration in healthCheckRegistrations)
{
    services.Remove(registration);
}
```

Using namespace: `Microsoft.Extensions.Diagnostics.HealthChecks`

### Pattern 2: EnsureCreated Instead of MigrateAsync

**What:** The CONTEXT.md mandates `EnsureCreated` (not `MigrateAsync`) for schema creation. `EnsureCreated` is faster — it creates all tables from the EF model directly without consulting migration history.

**Warning:** `EnsureCreated` and `EnsureDeleted` must be called per test class (not once globally) to achieve the per-test-class isolation the CONTEXT.md specifies. The mechanism is: each test class that needs fresh database state calls `EnsureDeleted` then `EnsureCreated` in its constructor or `InitializeAsync`.

**Factory InitializeAsync replacement:**
```csharp
public async Task InitializeAsync()
{
    await _dbContainer.StartAsync();
    // Database schema will be created per-test-class using EnsureCreated
    // No global schema creation here — each test class handles it
}
```

**Per-test-class isolation via IntegrationTestBase (or direct):**
```csharp
public async Task InitializeAsync()
{
    using IServiceScope scope = _factory.Services.CreateScope();

    // Drop and recreate all relevant schemas for this test class
    CatalogDbContext catalog = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await catalog.Database.EnsureDeletedAsync();
    await catalog.Database.EnsureCreatedAsync();
    // ... repeat for each DbContext used by this test class
}
```

**Confidence:** HIGH — this is how the CONTEXT.md says to do it.

### Pattern 3: FakeAuthenticationHandler (Bypass Keycloak)

**What:** Profiles, Reviews, and Wishlists endpoints use `RequireAuthorization()` and extract `userId` from `ClaimTypes.NameIdentifier` or `"sub"` claims. Without a real Keycloak token, these endpoints return 401. The fake handler lets tests inject any claims.

**Implementation:**
```csharp
// Source: ASP.NET Core testing docs pattern
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace MicroCommerce.ApiService.Tests.Integration.Fixtures;

public sealed class FakeAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check for test userId header set by test
        if (!Request.Headers.TryGetValue("X-Test-UserId", out var userIdValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        string userId = userIdValues.ToString();

        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("sub", userId),
        ];

        ClaimsIdentity identity = new(claims, SchemeName);
        ClaimsPrincipal principal = new(identity);
        AuthenticationTicket ticket = new(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
```

**Registration in factory:**
```csharp
// In ConfigureWebHost — replace Keycloak JWT with fake handler
services.AddAuthentication(FakeAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, FakeAuthenticationHandler>(
        FakeAuthenticationHandler.SchemeName, options => { });
```

### Pattern 4: IntegrationTestBase with Helper Methods

**What:** A base class that test classes inherit from (or a helper they receive via constructor injection from the fixture). Since xUnit doesn't allow base class + collection fixture easily, the recommended pattern here is a class that wraps `ApiWebApplicationFactory`.

**Pattern — collection fixture injection + base class:**
```csharp
namespace MicroCommerce.ApiService.Tests.Integration.Fixtures;

[Collection("Integration Tests")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly ApiWebApplicationFactory Factory;

    protected IntegrationTestBase(ApiWebApplicationFactory factory)
    {
        Factory = factory;
    }

    /// <summary>
    /// Creates an authenticated HttpClient with the given userId injected as a claim.
    /// Use for endpoints that call RequireAuthorization() and read User.Identity.
    /// </summary>
    protected HttpClient CreateAuthenticatedClient(Guid userId)
    {
        HttpClient client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        return client;
    }

    /// <summary>
    /// Creates an unauthenticated guest HttpClient.
    /// For cart operations, also sets buyer_id cookie.
    /// </summary>
    protected HttpClient CreateGuestClient(Guid? buyerId = null)
    {
        HttpClient client = Factory.CreateClient();
        if (buyerId.HasValue)
        {
            client.DefaultRequestHeaders.Add("Cookie", $"buyer_id={buyerId.Value}");
        }
        return client;
    }

    /// <summary>
    /// Creates a scoped service provider for direct DbContext access.
    /// Caller is responsible for disposing the scope.
    /// </summary>
    protected IServiceScope CreateScope() => Factory.Services.CreateScope();

    // IAsyncLifetime — subclasses override for per-test-class DB setup
    public virtual Task InitializeAsync() => Task.CompletedTask;
    public virtual Task DisposeAsync() => Task.CompletedTask;
}
```

**Note on xUnit + IAsyncLifetime + CollectionFixture:** xUnit supports test classes inheriting `IAsyncLifetime` AND using `[Collection]` fixtures simultaneously. The `InitializeAsync`/`DisposeAsync` on the test class fires per-test-class, while the collection fixture fires once for the entire collection. This is the mechanism for per-test-class DB isolation while sharing one container.

### Pattern 5: Test Data Builders (Fluent API)

**What:** Test data builders with sensible defaults and fluent `.With*()` overrides. This avoids boilerplate `new Category(...)` calls scattered across tests.

**Example builder for Catalog:**
```csharp
namespace MicroCommerce.ApiService.Tests.Integration.Builders;

/// <summary>
/// Fluent builder for creating product test data directly in the DB.
/// Use when tests need pre-existing products without going through the API.
/// </summary>
public sealed class ProductBuilder
{
    private string _name = "Test Product";
    private string _description = "Test Description";
    private decimal _price = 99.99m;
    private Guid _categoryId = Guid.NewGuid();

    public ProductBuilder WithName(string name) { _name = name; return this; }
    public ProductBuilder WithPrice(decimal price) { _price = price; return this; }
    public ProductBuilder WithCategoryId(Guid categoryId) { _categoryId = categoryId; return this; }

    public Product Build()
    {
        return Product.Create(_name, _description, _price, _categoryId, null, null);
    }
}
```

### Pattern 6: Handler-Level Test Pattern

**What:** Tests that instantiate a `DbContext` directly + a handler, bypassing the full HTTP pipeline. Used for edge cases and business rule testing where the HTTP layer adds no value.

**Representative choice — `UpdateOrderStatusCommandHandler`:** Tests the business rule that you cannot ship an order that hasn't been confirmed (Result.Fail path). This is the most valuable handler-level test to demonstrate since it exercises the FluentResults integration.

```csharp
// Handler-level test with real DB (Testcontainer)
public sealed class UpdateOrderStatusHandlerTests(ApiWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    public override async Task InitializeAsync()
    {
        using IServiceScope scope = CreateScope();
        OrderingDbContext db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    [Fact]
    public async Task Handle_ShipOrderInSubmittedStatus_ReturnsFailResult()
    {
        // Arrange — seed an order in Submitted status directly
        Order order;
        using (IServiceScope scope = CreateScope())
        {
            OrderingDbContext db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
            order = /* create order */;
            db.Orders.Add(order);
            await db.SaveChangesAsync();
        }

        // Act — call handler directly
        using IServiceScope handlerScope = CreateScope();
        OrderingDbContext handlerDb = handlerScope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        UpdateOrderStatusCommandHandler handler = new(handlerDb);
        Result result = await handler.Handle(
            new UpdateOrderStatusCommand(order.Id.Value, "Shipped"),
            CancellationToken.None);

        // Assert — business rule: cannot ship a Submitted order
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e.Message.Contains("Cannot ship order when status is 'Submitted'"));
    }
}
```

**Note on domain event assertion:** For domain events, assert `order.DomainEvents.Should().ContainSingle(e => e is OrderSubmittedDomainEvent)` after calling the domain method. This is pure unit-test style and doesn't require infrastructure — the existing `Unit/Ordering/Aggregates/OrderTests.cs` already demonstrates this correctly.

### Pattern 7: Profiles, Reviews, Wishlists Tests Need Auth

**Critical:** `ProfilesEndpoints`, `ReviewsEndpoints` (authenticated routes), and `WishlistsEndpoints` all use `.RequireAuthorization()` at the group or route level and extract `userId` from claims using:

```csharp
var sub = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
          ?? context.User.FindFirstValue("sub");
```

Tests for these features MUST use `CreateAuthenticatedClient(userId)` which sets the `X-Test-UserId` header consumed by `FakeAuthenticationHandler`.

**Reviews public route exception:** `GET /api/reviews/products/{productId}` does NOT require auth. This is the best representative endpoint for the Reviews feature test (no auth needed, just insert a review and fetch it).

**Wishlists DbContext registration:** The factory must be updated to also register/override `ProfilesDbContext`, `ReviewsDbContext`, and `WishlistsDbContext` in `ConfigureWebHost`.

### Anti-Patterns to Avoid

- **Using MigrateAsync in the factory:** CONTEXT.md says EnsureCreated. MigrateAsync is slower and requires the Profiles feature to have migrations (it currently has no migrations directory, only a DbContext).
- **Shared seed data across tests:** Each test must arrange its own data. Shared seed data in `InitializeAsync` causes test ordering dependencies.
- **One database for all tests without reset:** Without `EnsureDeleted`/`EnsureCreated` per class, tests pollute each other's data. The Catalog test `GetProducts_EmptyDatabase_ReturnsEmptyList` would fail after other tests add products.
- **Re-registering MassTransit without removing health checks:** The existing bug — must remove `HealthCheckRegistration` descriptors before calling `AddMassTransitTestHarness`.
- **Calling `new PostgreSqlBuilder("postgres:15-alpine")` with a positional argument:** The `PostgreSqlBuilder` constructor takes no string argument in the current Testcontainers API. The image should be set with `.WithImage("postgres:15-alpine")`.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Container lifecycle | Manual Docker API calls | `PostgreSqlBuilder` + `IAsyncLifetime` | Testcontainers handles port mapping, cleanup, readiness |
| HTTP testing | Manual HttpClient + Kestrel setup | `WebApplicationFactory<Program>.CreateClient()` | Handles service override, in-process server |
| Claims injection | Mock `IHttpContextAccessor` | `FakeAuthenticationHandler` scheme | Proper ASP.NET Core auth pipeline integration |
| Database reset | Truncate all tables manually | `EnsureDeleted` + `EnsureCreated` | Simpler, no Respawn dependency needed for this scale |

**Key insight:** The existing infrastructure is 80% correct. The critical work is fixing the MassTransit health check duplicate bug and adding the auth infrastructure for the three authenticated features.

---

## Common Pitfalls

### Pitfall 1: Duplicate MassTransit Health Check (CRITICAL — Already Occurring)

**What goes wrong:** All integration tests fail with `System.ArgumentException: Duplicate health checks were registered with the name(s): masstransit-bus`.

**Why it happens:** `Program.cs` calls `builder.AddServiceDefaults()` which calls `AddDefaultHealthChecks()`, AND `AddMassTransit` registers its own `masstransit-bus` health check. The factory's `ConfigureWebHost` then calls `AddMassTransitTestHarness` which registers `masstransit-bus` again. The original `IBus`/`IBusControl`/`IPublishEndpoint`/`ISendEndpointProvider` removal does NOT remove health check registrations.

**How to avoid:** Before `AddMassTransitTestHarness`, remove `IHealthCheckRegistration` descriptors whose `HealthCheckRegistration.Name` starts with `"masstransit"`.

**Warning signs:** `System.ArgumentException: Duplicate health checks` in test output. All integration tests fail immediately in factory startup.

### Pitfall 2: EnsureCreated vs MigrateAsync

**What goes wrong:** `MigrateAsync` requires an `__EFMigrationsHistory` table and will fail if schema migrations are inconsistent with the model — especially for `ProfilesDbContext` which has no migrations directory at all.

**Why it happens:** The factory currently calls `MigrateAsync` on all DbContexts. ProfilesDbContext has no migrations (only a DbContext file), so `MigrateAsync` would fail with "No migrations found" or similar.

**How to avoid:** Use `EnsureCreated` per test class as the CONTEXT.md mandates. This creates the schema from the EF model without requiring migrations to exist.

### Pitfall 3: Test Class Data Pollution with Shared Container

**What goes wrong:** Tests that expect empty tables (like `GetCart_EmptyCart_ReturnsNoContent`) fail because previous test classes added data.

**Why it happens:** All test classes share one PostgreSQL container via the xUnit collection fixture. Without per-class database reset, data accumulates across test classes.

**How to avoid:** Every test class that reads data must call `EnsureDeleted` + `EnsureCreated` in its `InitializeAsync`. This drops and recreates the schema before each test class runs.

### Pitfall 4: Auth Endpoints Return 401 Without Fake Handler

**What goes wrong:** Tests for Profiles, Reviews (auth routes), and Wishlists get HTTP 401 because the production Keycloak JWT bearer validates against a real Keycloak instance.

**Why it happens:** In test environment, Keycloak is not running. The Aspire service discovery URL for Keycloak resolves to nothing.

**How to avoid:** In the factory's `ConfigureWebHost`, replace the authentication scheme with `FakeAuthenticationHandler`. The existing auth middleware chain still fires, but the fake handler injects claims from the `X-Test-UserId` header.

### Pitfall 5: PostgreSqlBuilder API Mismatch

**What goes wrong:** `new PostgreSqlBuilder("postgres:15-alpine")` throws a compile error — the constructor does not accept a string.

**Correct API (verified with Context7):**
```csharp
_dbContainer = new PostgreSqlBuilder()
    .WithImage("postgres:15-alpine")
    .WithDatabase("microcommerce_test")
    .WithCleanUp(true)
    .Build();
```

The existing factory already has this correct (despite the constructor call syntax appearing to use a positional arg — this is the `imageName` parameter on the generic `ContainerBuilder` base which is internal; the fluent `.WithImage()` is the public API).

### Pitfall 6: xUnit Collection Fixture + IAsyncLifetime Interaction

**What goes wrong:** Tests assume `IAsyncLifetime.InitializeAsync` on the collection fixture fires before every test. It only fires once per test run (when the collection is first used).

**How this matters:** The per-test-class `EnsureDeleted`/`EnsureCreated` must be in the **test class's** `IAsyncLifetime.InitializeAsync`, NOT in the collection fixture's. The collection fixture starts the container once; test classes reset the DB before their tests.

---

## Code Examples

### Factory Fix — Complete Updated ConfigureWebHost

```csharp
// Source: Based on existing factory + fix for duplicate health checks
protected override void ConfigureWebHost(IWebHostBuilder builder)
{
    builder.ConfigureServices(services =>
    {
        // Remove all hosted services (data seeders, background cleanup)
        var hostedServices = services
            .Where(d => d.ServiceType == typeof(IHostedService))
            .ToList();
        foreach (ServiceDescriptor service in hostedServices) services.Remove(service);

        // Remove duplicate MassTransit health check registrations
        // AddMassTransitTestHarness below re-registers 'masstransit-bus'
        var masstransitHealthChecks = services
            .Where(d => d.ServiceType == typeof(HealthCheckRegistration)
                && d.ImplementationInstance is HealthCheckRegistration hcr
                && hcr.Name.StartsWith("masstransit", StringComparison.OrdinalIgnoreCase))
            .ToList();
        foreach (ServiceDescriptor registration in masstransitHealthChecks) services.Remove(registration);

        // Replace DbContexts with Testcontainer connection string
        // (OutboxDbContext, CatalogDbContext, CartDbContext, OrderingDbContext,
        //  InventoryDbContext, ProfilesDbContext, ReviewsDbContext, WishlistsDbContext)
        // ... RemoveAll + AddDbContext pattern for each ...

        // Replace MassTransit with in-memory test harness
        services.RemoveAll(typeof(IBus));
        services.RemoveAll(typeof(IBusControl));
        services.RemoveAll(typeof(IPublishEndpoint));
        services.RemoveAll(typeof(ISendEndpointProvider));
        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddConsumers(typeof(Program).Assembly);
        });

        // Replace Azure Blob Storage with no-op stub
        services.RemoveAll<IImageUploadService>();
        services.AddScoped<IImageUploadService, NoOpImageUploadService>();

        // Replace Azure Avatar service with no-op stub
        services.RemoveAll<IAvatarImageService>();
        services.AddScoped<IAvatarImageService, NoOpAvatarImageService>();

        // Replace Keycloak JWT auth with fake handler for tests
        services.AddAuthentication(FakeAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, FakeAuthenticationHandler>(
                FakeAuthenticationHandler.SchemeName, options => { });
    });

    builder.UseEnvironment("Testing");
}
```

### Factory InitializeAsync — No Global Schema Creation

```csharp
public async Task InitializeAsync()
{
    // Start the PostgreSQL container only — schema creation is per-test-class
    await _dbContainer.StartAsync();
    // Do NOT call EnsureCreated or MigrateAsync here
}
```

### Profiles Test — Representative Authenticated Endpoint

```csharp
[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public sealed class ProfilesEndpointsTests(ApiWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    public override async Task InitializeAsync()
    {
        using IServiceScope scope = CreateScope();
        ProfilesDbContext db = scope.ServiceProvider.GetRequiredService<ProfilesDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    [Fact]
    public async Task GetMyProfile_NewUser_AutoCreatesAndReturnsProfile()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/profiles/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ProfileDto? profile = await response.Content.ReadFromJsonAsync<ProfileDto>();
        profile.Should().NotBeNull();
        profile!.UserId.Should().Be(userId);
    }
}
```

### Reviews Test — Public Endpoint (No Auth Required)

```csharp
[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public sealed class ReviewsEndpointsTests(ApiWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    public override async Task InitializeAsync()
    {
        using IServiceScope scope = CreateScope();
        ReviewsDbContext db = scope.ServiceProvider.GetRequiredService<ReviewsDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    [Fact]
    public async Task GetProductReviews_NoReviews_ReturnsEmptyList()
    {
        // Arrange
        Guid productId = Guid.NewGuid();
        HttpClient client = CreateGuestClient();  // Public endpoint — no auth needed

        // Act
        ReviewListDto? result = await client.GetFromJsonAsync<ReviewListDto>(
            $"/api/reviews/products/{productId}");

        // Assert
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
```

### Wishlists Test — Authenticated Endpoint

```csharp
[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public sealed class WishlistsEndpointsTests(ApiWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    public override async Task InitializeAsync()
    {
        using IServiceScope scope = CreateScope();
        WishlistsDbContext db = scope.ServiceProvider.GetRequiredService<WishlistsDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    [Fact]
    public async Task GetWishlist_EmptyWishlist_ReturnsEmptyList()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        // Act
        List<WishlistItemDto>? items = await client.GetFromJsonAsync<List<WishlistItemDto>>(
            "/api/wishlist");

        // Assert
        items.Should().NotBeNull();
        items.Should().BeEmpty();
    }
}
```

---

## Feature Analysis — What Each Feature Needs

### Missing DbContext Registrations in Factory

The existing factory registers 5 DbContexts. Three are missing:

| DbContext | Schema | Has Migrations? | Notes |
|-----------|--------|-----------------|-------|
| `ProfilesDbContext` | `profiles` | No | Only DbContext + Configuration file |
| `ReviewsDbContext` | `reviews` | Yes | Has 3 migration files |
| `WishlistsDbContext` | `wishlists` | Yes | Has 3 migration files |

All three need to be added to `ConfigureWebHost` with the same `RemoveAll + AddDbContext` pattern, and in `InitializeAsync` (or per-test-class) with `EnsureCreated`.

### Profiles Feature Auth Requirements

`ProfilesEndpoints.GetUserId(HttpContext)` throws `UnauthorizedAccessException` if `ClaimTypes.NameIdentifier` or `"sub"` claim is missing. Tests must use `CreateAuthenticatedClient`. The `GetMyProfile` endpoint auto-creates a profile for a new userId (via `CreateProfileCommand` in `GetProfileQuery`), making it ideal as the representative test — no prior data needed.

### Reviews Feature Endpoint Selection

`GET /api/reviews/products/{productId}` is public (no `RequireAuthorization`). This is the simplest representative test — no auth setup, returns empty list for unknown product. POST (CreateReview) requires auth and the `IAvatarImageService` is not involved.

### Wishlists Feature Endpoint Selection

All Wishlists endpoints require auth (group-level `RequireAuthorization()`). `GET /api/wishlist` returning empty list for a new user is the simplest representative test.

### Handler-Level Test Selection

`UpdateOrderStatusCommandHandler` is the best candidate because:
1. It uses `Result` return type (FluentResults integration — tests the new pattern from Phase 17)
2. The happy path (Order.Ship() when not in Confirmed status) tests business rule enforcement
3. It requires a seeded Order in the database — exercises the DB setup pattern
4. The `NotFoundException` path is also testable without complex setup

---

## State of the Art

| Old Approach | Current Approach | Impact |
|--------------|------------------|--------|
| `MigrateAsync` in factory | `EnsureCreated` per test class | Faster, works without migrations |
| Real Keycloak in tests | `FakeAuthenticationHandler` scheme | No external dependency |
| Mock `IBus` | `AddMassTransitTestHarness` | Proper MassTransit test integration |
| No test data builders | Fluent builders with defaults | Reduces test boilerplate, readable |
| `new List<T>` syntax | `[]` collection expressions | CLAUDE.md convention |

---

## Open Questions

1. **IAvatarImageService vs IImageUploadService**
   - What we know: The factory already stubs `IImageUploadService` (catalog). Profiles feature also has `IAvatarImageService` (in `ProfilesDbContext` and registered in `Program.cs` as `AvatarImageService`).
   - What's unclear: Whether `IAvatarImageService` also tries to connect to Azure Blob Storage during tests and needs stubbing.
   - Recommendation: Add a `NoOpAvatarImageService` stub in the factory alongside `NoOpImageUploadService`. Check `AvatarImageService` constructor for Azure dependency injection.

2. **IDeadLetterQueueService**
   - What we know: `IDeadLetterQueueService` is registered in `Program.cs` using `DeadLetterQueueService` which presumably uses `ServiceBusClient`. No Messaging test is required.
   - Recommendation: Check if `ServiceBusClient` throws on startup in the test environment. If so, stub it as well.

3. **PostgreSqlBuilder image version**
   - What we know: The existing factory uses `"postgres:15-alpine"`. The project uses PostgreSQL via Aspire (production version may differ).
   - Recommendation: Keep `postgres:15-alpine` for compatibility unless there are schema-level features requiring a newer version.

---

## Sources

### Primary (HIGH confidence)

- `/testcontainers/testcontainers-dotnet` (Context7) — PostgreSqlBuilder API, collection fixtures, IAsyncLifetime
- `/Users/baotoq/Work/micro-commerce/src/MicroCommerce.ApiService.Tests/` — Existing test infrastructure verified by direct file inspection
- `/Users/baotoq/Work/micro-commerce/src/MicroCommerce.ApiService/Program.cs` — Production DI registrations, auth setup
- Test run output — Confirmed 29 failures with `Duplicate health checks: masstransit-bus`

### Secondary (MEDIUM confidence)

- Project skills: `testcontainers-integration-tests/SKILL.md` — PostgreSQL container patterns, IAsyncLifetime lifecycle
- Project skills: `test-driven-development/SKILL.md` — Test quality guidelines

---

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — verified from existing .csproj and running test project
- Architecture: HIGH — existing code inspected directly, failure root cause confirmed by running tests
- Pitfalls: HIGH — duplicate health check bug confirmed by running tests; auth requirement confirmed from endpoint code

**Research date:** 2026-02-25
**Valid until:** 2026-03-25 (stable .NET testing ecosystem)

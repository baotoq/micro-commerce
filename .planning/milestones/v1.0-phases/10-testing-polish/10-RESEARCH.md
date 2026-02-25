# Phase 10: Testing & Polish - Research

**Researched:** 2026-02-12
**Domain:** .NET integration testing, unit testing, E2E testing, code coverage, UX polish
**Confidence:** HIGH

## Summary

Phase 10 requires comprehensive testing coverage for the MicroCommerce platform: unit tests for domain logic (especially Ordering), integration tests for API endpoints with real PostgreSQL via Testcontainers, saga testing with MassTransit test harnesses, E2E testing with Playwright, and a UX polish pass to ensure production readiness.

The .NET testing ecosystem for this stack is mature and well-documented. xUnit with WebApplicationFactory is the gold standard for ASP.NET Core integration testing. Testcontainers provides high-fidelity database testing without the brittleness of in-memory databases. MassTransit's InMemoryTestHarness and SagaTestHarness enable fast, isolated message bus testing. For E2E testing, Playwright has emerged as the superior choice over Cypress for 2026 due to faster execution, native parallelization, and broader browser support. Code coverage with Coverlet is built into the xUnit template by default.

The testing pyramid should prioritize: (1) unit tests for aggregates and value objects (fast, abundant), (2) integration tests for commands/queries and saga flows (moderate speed, comprehensive API coverage), (3) E2E tests for critical user journeys (slower, focused on happy paths and key failure modes).

**Primary recommendation:** Use xUnit + WebApplicationFactory + Testcontainers.PostgreSql for integration tests, MassTransit.TestFramework for saga testing, Coverlet for code coverage, and Playwright for E2E testing. Organize tests in `src/MicroCommerce.ApiService.Tests/` mirroring the feature module structure.

## Standard Stack

The established libraries/tools for comprehensive .NET testing:

### Core Testing Framework
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| xUnit.net | 2.9+ | Test framework and runner | Best choice for integration tests with straightforward lifetime control; .NET team uses it |
| WebApplicationFactory | Built-in ASP.NET Core | In-memory test server | Gold standard for API integration testing; spins up entire app without deployment |
| Testcontainers.PostgreSql | 4.10.0 | Docker-based PostgreSQL for tests | High fidelity database testing; eliminates in-memory DB behavior gaps; 18.4M+ downloads |
| MassTransit.TestFramework | 9.0.0 (matches runtime) | InMemory test harness for consumers/sagas | Official MassTransit testing support; fast isolated message bus verification |

### Supporting Testing Tools
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| coverlet.collector | Built-in xUnit template | Code coverage data collection | Already integrated; use for 80%+ domain coverage verification |
| FluentAssertions | 7.0+ | Readable test assertions | Optional but recommended for better test readability |
| ReportGenerator | Latest | HTML coverage reports from Cobertura XML | Local dev and CI for human-readable coverage summaries |
| Respawn | Latest | Smart database cleanup between tests | Optional; alternative to transaction rollback or manual cleanup |

### E2E Testing
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Playwright | @playwright/test latest | Browser automation for E2E | Superior to Cypress in 2026: 6x faster (14min vs 90min enterprise), native parallelization, multi-browser (Chrome/Firefox/Safari) |
| Next.js + Playwright | Built-in integration | Next.js official E2E support | Configured via `create-next-app --example with-playwright` |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| xUnit | NUnit/MSTest | xUnit has better integration test lifetime control; preferred by .NET team |
| Testcontainers | In-memory DB (EF InMemory) | In-memory DBs don't catch DB-specific issues (concurrency, SQL quirks); Testcontainers = production fidelity |
| Playwright | Cypress | Cypress better debugging UI but slower (paid parallelization), JavaScript-only, no Safari; Playwright = enterprise scale |
| Respawn | Transaction rollback | Rollback can't test transaction boundaries; Respawn = smarter but adds dependency |

**Installation (Backend):**
```bash
# Test project
dotnet new xunit -n MicroCommerce.ApiService.Tests -f net10.0
cd src/MicroCommerce.ApiService.Tests
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Testcontainers.PostgreSql
dotnet add package FluentAssertions
dotnet add package MassTransit.TestFramework --version 9.0.0
dotnet add reference ../MicroCommerce.ApiService/MicroCommerce.ApiService.csproj
```

**Installation (Frontend):**
```bash
cd src/MicroCommerce.Web
npm init playwright
# Follow prompts: TypeScript, tests in e2e/, GitHub Actions workflow
npm install --save-dev @playwright/test
npx playwright install-deps
```

## Architecture Patterns

### Recommended Test Project Structure
```
src/
├── MicroCommerce.ApiService.Tests/
│   ├── Integration/                    # WebApplicationFactory + Testcontainers
│   │   ├── Fixtures/
│   │   │   ├── ApiWebApplicationFactory.cs      # Custom WAF with Testcontainers
│   │   │   └── IntegrationTestCollection.cs     # xUnit collection fixture
│   │   ├── Catalog/
│   │   │   ├── Commands/                        # CreateProduct, UpdateProduct, etc.
│   │   │   └── Queries/                         # GetProducts, GetProductById, etc.
│   │   ├── Cart/                                # Cart CQRS endpoints
│   │   ├── Inventory/                           # Inventory CQRS endpoints
│   │   └── Ordering/
│   │       ├── Commands/                        # SubmitOrder command tests
│   │       ├── Queries/                         # GetOrder query tests
│   │       └── Sagas/
│   │           └── CheckoutSagaTests.cs         # Saga state machine tests
│   ├── Unit/                           # Fast isolated tests
│   │   ├── Ordering/
│   │   │   ├── Aggregates/
│   │   │   │   └── OrderTests.cs                # Order.Create, MarkAsPaid, etc.
│   │   │   └── ValueObjects/
│   │   │       ├── OrderNumberTests.cs
│   │   │       ├── ShippingAddressTests.cs
│   │   │       └── OrderStatusTests.cs
│   │   ├── Catalog/
│   │   │   ├── Aggregates/
│   │   │   │   └── ProductTests.cs              # Product.Create, ChangeStatus, etc.
│   │   │   └── ValueObjects/
│   │   │       ├── MoneyTests.cs
│   │   │       └── ProductNameTests.cs
│   │   ├── Cart/
│   │   │   └── Aggregates/
│   │   │       └── CartTests.cs                 # Cart.AddItem, UpdateQuantity, etc.
│   │   ├── Inventory/
│   │   │   └── Aggregates/
│   │   │       └── StockItemTests.cs            # StockItem.Reserve, Deduct, etc.
│   │   └── Validators/                 # FluentValidation tests
│   │       ├── CreateProductCommandValidatorTests.cs
│   │       ├── SubmitOrderCommandValidatorTests.cs
│   │       └── AdjustStockCommandValidatorTests.cs
│   ├── coverlet.runsettings            # Exclude migrations, generated files
│   └── MicroCommerce.ApiService.Tests.csproj
│
src/MicroCommerce.Web/
└── e2e/                                # Playwright E2E tests
    ├── fixtures/                       # Playwright fixtures if needed
    ├── critical-path.spec.ts           # Browse → Cart → Checkout → Confirmation
    ├── product-browsing.spec.ts        # Filters, search, pagination
    └── admin-orders.spec.ts            # Admin order management
```

### Pattern 1: Integration Test with WebApplicationFactory + Testcontainers
**What:** Spin up the full ASP.NET Core app in-memory with a real PostgreSQL database in Docker
**When to use:** Testing API endpoints, command handlers with DB persistence, query handlers with real SQL
**Example:**
```csharp
// Source: https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests + https://dotnet.testcontainers.org/modules/postgres/
public class ApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("microcommerce_test")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Replace PostgreSQL connection string with Testcontainer
            services.RemoveAll<DbContextOptions<CatalogDbContext>>();
            services.AddDbContext<CatalogDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));

            // Similar for other DbContexts: OrderingDbContext, InventoryDbContext, CartDbContext

            // Replace MassTransit with InMemoryTestHarness
            services.AddMassTransitTestHarness();
        });
    }

    public async Task InitializeAsync() => await _dbContainer.StartAsync();
    public new async Task DisposeAsync() => await _dbContainer.DisposeAsync();
}

[Collection("Integration Tests")]
public class CreateProductCommandTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CreateProductCommandTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateProduct_ValidCommand_ReturnsCreatedProduct()
    {
        // Arrange
        var command = new { Name = "Test Product", Price = 99.99, CategoryId = Guid.NewGuid() };

        // Act
        var response = await _client.PostAsJsonAsync("/api/catalog/products", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        product.Name.Should().Be("Test Product");
    }
}
```

### Pattern 2: Unit Testing DDD Aggregates
**What:** Test aggregate root public methods (Create factory, state transitions, invariant enforcement)
**When to use:** Testing domain logic in isolation without infrastructure
**Example:**
```csharp
// Source: https://www.jamesmichaelhickey.com/ddd-unit-tests/ + DDD best practices
public class OrderTests
{
    [Fact]
    public void Create_ValidData_RaisesOrderSubmittedEvent()
    {
        // Arrange
        var items = new[] { (Guid.NewGuid(), "Product", 10m, "url", 2) };
        var address = ShippingAddress.Create("John", "123 Main St", "NY", "10001", "US");

        // Act
        var order = Order.Create(Guid.NewGuid(), "john@example.com", address, items);

        // Assert
        order.Should().NotBeNull();
        order.Status.Should().Be(OrderStatus.Submitted);
        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderSubmittedDomainEvent>();
    }

    [Fact]
    public void MarkAsPaid_WhenSubmitted_TransitionsToPaid()
    {
        // Arrange
        var order = CreateValidOrder(); // Helper method

        // Act
        order.MarkAsPaid();

        // Assert
        order.Status.Should().Be(OrderStatus.Paid);
        order.PaidAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsPaid_WhenShipped_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = CreateValidOrder();
        order.MarkAsPaid();
        order.Confirm();
        order.Ship(); // Now Shipped

        // Act
        var act = () => order.MarkAsPaid();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*status is 'Shipped'*");
    }
}
```

### Pattern 3: FluentValidation Validator Testing
**What:** Use TestValidate helper to verify validation rules with clear assertions
**When to use:** Dedicated tests for each validator to ensure rules are enforced
**Example:**
```csharp
// Source: https://docs.fluentvalidation.net/en/latest/testing.html
public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public void Validate_EmptyName_ShouldHaveError()
    {
        // Arrange
        var command = new CreateProductCommand("", 100m, Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Product name is required.");
    }

    [Fact]
    public void Validate_NegativePrice_ShouldHaveError()
    {
        // Arrange
        var command = new CreateProductCommand("Valid Name", -10m, Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreateProductCommand("Valid Product", 99.99m, Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
```

### Pattern 4: MassTransit Saga State Machine Testing
**What:** Use SagaTestHarness to verify saga state transitions and event publishing
**When to use:** Testing CheckoutStateMachine happy paths and compensation flows
**Example:**
```csharp
// Source: https://masstransit.io/documentation/configuration/test-harness
public class CheckoutSagaTests
{
    [Fact]
    public async Task CheckoutStarted_ReservesStock_TransitionsToSubmitted()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<CheckoutStateMachine, CheckoutState>()
                    .InMemoryRepository();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();

        // Act
        await harness.Bus.Publish<CheckoutStarted>(new
        {
            OrderId = orderId,
            BuyerId = Guid.NewGuid(),
            BuyerEmail = "test@example.com",
            Items = new[] { new { ProductId = Guid.NewGuid(), Quantity = 1 } }
        });

        // Assert
        (await harness.Published.Any<ReserveStockForOrder>()).Should().BeTrue();

        var saga = harness.GetSagaStateMachineHarness<CheckoutStateMachine, CheckoutState>();
        (await saga.Exists(orderId, x => x.Submitted)).Should().NotBeNull();
    }
}
```

### Pattern 5: Playwright E2E Test
**What:** Full browser automation testing user journeys from frontend to backend
**When to use:** Critical path validation (browse → cart → checkout → confirmation)
**Example:**
```typescript
// Source: https://nextjs.org/docs/app/guides/testing/playwright
import { test, expect } from '@playwright/test';

test('critical path: browse to purchase', async ({ page }) => {
  // Browse products
  await page.goto('/');
  await expect(page.getByRole('heading', { name: 'Featured Products' })).toBeVisible();

  // View product detail
  await page.getByRole('link', { name: /Product/ }).first().click();
  await expect(page.getByRole('heading')).toContainText('Product');

  // Add to cart
  await page.getByRole('button', { name: 'Add to Cart' }).click();
  await expect(page.getByText('Added to cart')).toBeVisible();

  // View cart
  await page.getByRole('link', { name: 'Cart' }).click();
  await expect(page.getByRole('heading', { name: 'Shopping Cart' })).toBeVisible();

  // Checkout
  await page.getByRole('button', { name: 'Checkout' }).click();
  await page.fill('input[name="email"]', 'test@example.com');
  await page.fill('input[name="fullName"]', 'Test User');
  await page.fill('input[name="street"]', '123 Main St');
  await page.fill('input[name="city"]', 'New York');
  await page.fill('input[name="postalCode"]', '10001');
  await page.fill('input[name="country"]', 'US');

  // Submit order
  await page.getByRole('button', { name: 'Place Order' }).click();

  // Verify confirmation
  await expect(page.getByRole('heading', { name: 'Order Confirmed' })).toBeVisible();
  await expect(page.getByText(/MC-\d{6}/)).toBeVisible(); // Order number
});
```

### Anti-Patterns to Avoid

- **Testing implementation details:** Don't test private methods or internal aggregate state. Test public API only.
- **Mocking FluentValidation validators:** FluentValidation strongly recommends against mocking. Use TestValidate or test through integration tests.
- **Shared mutable state across tests:** Each test should be isolated. Use IClassFixture for read-only shared setup, not mutable state.
- **Asserting on database side effects in unit tests:** Unit tests should not touch the database. Use integration tests for persistence verification.
- **Using in-memory DB for integration tests:** EF InMemory doesn't catch SQL-specific issues (constraints, concurrency). Use Testcontainers for real PostgreSQL.
- **Rolling back transactions in tests:** Transactions can't test transaction boundary logic. Use Respawn or recreate containers for isolation.

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Database cleanup between tests | Manual DELETE scripts per table | Respawn | Intelligently handles FK relationships, avoids deadlocks |
| In-memory test server | Custom HTTP server wrapper | WebApplicationFactory | Built-in ASP.NET Core, handles lifetime, service replacement |
| Docker container lifecycle | Shell scripts with `docker run` | Testcontainers | Handles startup/shutdown, port discovery, cleanup on failure |
| Test assertions | `Assert.Equal(expected, actual)` | FluentAssertions | Better error messages, chainable, reads like English |
| Code coverage collection | Custom instrumentation | Coverlet | Built into xUnit template, industry standard |
| Saga testing | Publishing messages manually | MassTransit.TestFramework | Verifies state transitions, correlation, event publishing |

**Key insight:** Testing infrastructure is mature in .NET. Re-use official tools (WebApplicationFactory, Testcontainers, MassTransit test harnesses) rather than building custom test infrastructure. Tests should focus on business logic verification, not infrastructure plumbing.

## Common Pitfalls

### Pitfall 1: Shared Database State Between Tests
**What goes wrong:** Tests pass individually but fail when run in parallel due to shared data
**Why it happens:** Multiple tests writing to same database without isolation
**How to avoid:** Use xUnit collection fixtures with IClassFixture to share WebApplicationFactory across test classes, but use Testcontainers to spin up isolated DB per test run or use Respawn to reset DB state before each test
**Warning signs:** Tests fail in CI but pass locally, flaky tests, "record already exists" errors

### Pitfall 2: Testing Through MediatR Instead of Handlers Directly
**What goes wrong:** Tests become integration tests requiring full DI setup
**Why it happens:** Calling `mediator.Send(command)` instead of `handler.Handle(command)`
**How to avoid:** For unit tests, instantiate handler directly and pass dependencies (DbContext, services). MediatR pipeline is for integration tests.
**Warning signs:** Unit tests require WebApplicationFactory, slow test execution, complex test setup

### Pitfall 3: Forgetting to Start Test Harness
**What goes wrong:** MassTransit saga tests timeout or hang indefinitely
**Why it happens:** `harness.Start()` not called before publishing messages
**How to avoid:** Always call `await harness.Start()` after building provider and before publishing
**Warning signs:** Tests timeout after 30+ seconds, no messages consumed

### Pitfall 4: Not Configuring baseURL in Playwright
**What goes wrong:** E2E tests hardcode `http://localhost:3000` which breaks in CI
**Why it happens:** Missing `baseURL` in playwright.config.ts
**How to avoid:** Set `baseURL: process.env.BASE_URL || 'http://localhost:3000'` in config, use `page.goto('/')` in tests
**Warning signs:** Tests pass locally but fail in CI, "ECONNREFUSED" errors

### Pitfall 5: Integration Tests Without Migrations
**What goes wrong:** Tests fail with "relation does not exist" errors
**Why it happens:** Testcontainers starts empty PostgreSQL, no schema applied
**How to avoid:** In `ApiWebApplicationFactory.InitializeAsync()`, run migrations: `await dbContext.Database.MigrateAsync()` for each DbContext
**Warning signs:** First test fails with SQL errors about missing tables/columns

### Pitfall 6: Over-Testing Value Objects
**What goes wrong:** Test suite bloated with trivial tests for simple value objects
**Why it happens:** Misunderstanding "test everything" to include constructor validation
**How to avoid:** Focus value object tests on business rules and validation logic. Don't test framework behavior (e.g., record equality).
**Warning signs:** Tests like `Money_Constructor_SetsAmount` with no business logic validation

### Pitfall 7: Ignoring Code Coverage of Generated Files
**What goes wrong:** Coverage reports show low percentages due to migrations, generated files
**Why it happens:** Coverlet includes everything by default
**How to avoid:** Create `coverlet.runsettings` excluding `**/Migrations/**`, `**/*.Designer.cs`, `**/*.g.cs`
**Warning signs:** 40% coverage despite thorough tests, migrations dominating coverage report

## Code Examples

Verified patterns from official sources:

### xUnit Collection Fixture for WebApplicationFactory
```csharp
// Source: https://xunit.net/docs/shared-context + WebApplicationFactory docs
[CollectionDefinition("Integration Tests")]
public class IntegrationTestCollection : ICollectionFixture<ApiWebApplicationFactory>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

// All test classes in this collection share the same ApiWebApplicationFactory instance
[Collection("Integration Tests")]
public class CatalogEndpointsTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CatalogEndpointsTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ReturnsProducts()
    {
        var response = await _client.GetAsync("/api/catalog/products");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

### Coverlet Configuration (coverlet.runsettings)
```xml
<!-- Source: https://github.com/coverlet-coverage/coverlet + best practices -->
<?xml version="1.0" encoding="utf-8" ?>
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat Code Coverage">
        <Configuration>
          <Format>cobertura</Format>
          <Exclude>[*]*.Migrations.*,[*]*.Designer,[*]*.g</Exclude>
          <ExcludeByFile>**/Migrations/**/*.cs</ExcludeByFile>
          <IncludeTestAssembly>false</IncludeTestAssembly>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
```

Run tests with coverage:
```bash
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
# Generate HTML report
reportgenerator -reports:TestResults/**/coverage.cobertura.xml -targetdir:coveragereport -reporttypes:Html
```

### Playwright Configuration for Next.js
```typescript
// Source: https://nextjs.org/docs/app/guides/testing/playwright
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',
  use: {
    baseURL: process.env.BASE_URL || 'http://localhost:3000',
    trace: 'on-first-retry',
  },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
    { name: 'firefox', use: { ...devices['Desktop Firefox'] } },
    { name: 'webkit', use: { ...devices['Desktop Safari'] } },
  ],
  webServer: {
    command: 'npm run build && npm run start',
    url: 'http://localhost:3000',
    reuseExistingServer: !process.env.CI,
  },
});
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| EF Core InMemory for tests | Testcontainers with real DB | ~2021 | Catches DB-specific bugs (constraints, SQL syntax, concurrency) |
| NUnit for integration tests | xUnit for integration tests | ~2018 | Better test lifetime control with IClassFixture/ICollectionFixture |
| Cypress for E2E | Playwright for E2E | 2024-2025 | 6x faster execution, native parallelization, multi-browser support |
| Manual test harness setup | MassTransit.TestFramework | MassTransit 8.0+ | Simplified saga/consumer testing with InMemoryTestHarness |
| Custom assertion helpers | FluentAssertions | Industry standard | Better error messages, chainable syntax |
| Transaction rollback for test cleanup | Respawn for test cleanup | ~2019 | Tests transaction boundaries, avoids rollback limitations |

**Deprecated/outdated:**
- **MSTest**: Still works but xUnit has better integration test support via IClassFixture
- **EF Core InMemory Provider for integration tests**: Use Testcontainers instead for production fidelity
- **Coverlet.MSBuild**: Use coverlet.collector (integrated in xUnit template) instead
- **Selenium for E2E**: Playwright has superseded Selenium for modern web testing

## Open Questions

Things that couldn't be fully resolved:

1. **Should we use Respawn or transaction-per-test for database cleanup?**
   - What we know: Respawn is smarter (handles FK relationships) but adds dependency. Transactions are faster but can't test transaction boundaries.
   - What's unclear: Performance impact at scale (hundreds of tests).
   - Recommendation: Start without Respawn (Testcontainers recreates DB per run). Add Respawn if test suite becomes slow (>2min).

2. **How much unit test coverage for Cart/Inventory/Catalog vs Ordering?**
   - What we know: Context doc says Ordering gets most thorough coverage (highest complexity).
   - What's unclear: Specific coverage % targets for other domains.
   - Recommendation: Ordering 80%+ coverage. Cart/Inventory/Catalog 60%+ coverage focused on aggregate invariants and business rules. Skip trivial property setters.

3. **Which E2E scenarios beyond critical path?**
   - What we know: Must cover browse → cart → checkout → confirmation.
   - What's unclear: Priority for admin flows, error scenarios, edge cases.
   - Recommendation: Add admin order management E2E (view orders, drag-drop status). Add 1-2 failure scenarios (out of stock, payment decline). Total 5-7 E2E tests max.

4. **Performance baseline: which endpoints to benchmark?**
   - What we know: Need performance baseline tests per requirements.
   - What's unclear: Which endpoints are critical for performance.
   - Recommendation: Benchmark GET /api/catalog/products (most frequent), POST /api/ordering/orders/submit (most complex), GET /api/cart (high traffic). Use BenchmarkDotNet for microbenchmarks or simple load test with Playwright.

## Sources

### Primary (HIGH confidence)
- [Microsoft Learn: Integration tests in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0) - Official WebApplicationFactory docs
- [Testcontainers for .NET PostgreSQL Module](https://dotnet.testcontainers.org/modules/postgres/) - Official Testcontainers docs
- [MassTransit Test Harness](https://masstransit.io/documentation/configuration/test-harness) - Official saga testing docs
- [FluentValidation Test Extensions](https://docs.fluentvalidation.net/en/latest/testing.html) - Official testing guide
- [Next.js Playwright Testing](https://nextjs.org/docs/app/guides/testing/playwright) - Official Next.js E2E guide
- [xUnit Shared Context](https://xunit.net/docs/shared-context) - Official IClassFixture/ICollectionFixture docs
- [Coverlet GitHub](https://github.com/coverlet-coverage/coverlet) - Official code coverage tool
- [BenchmarkDotNet](https://benchmarkdotnet.org/) - Official .NET benchmarking library

### Secondary (MEDIUM confidence)
- [ASP.NET Core Integration Testing Best Practises](https://antondevtips.com/blog/asp-net-core-integration-testing-best-practises) - Practical WebApplicationFactory patterns
- [Testcontainers Best Practices for .NET](https://www.milanjovanovic.tech/blog/testcontainers-best-practices-dotnet-integration-testing) - Milan Jovanovic best practices
- [Domain-Driven Design & Unit Tests](https://www.jamesmichaelhickey.com/ddd-unit-tests/) - DDD aggregate testing strategies
- [Cypress vs Playwright: I Ran 500 E2E Tests in Both](https://medium.com/lets-code-future/cypress-vs-playwright-i-ran-500-e2e-tests-in-both-heres-what-broke-2afc448470ee) - Real-world performance comparison
- [Respawn GitHub](https://github.com/jbogard/Respawn) - Database cleanup tool by Jimmy Bogard
- [Best Practices for Loading States in Next.js](https://www.getfishtank.com/insights/best-practices-for-loading-states-in-nextjs) - UX loading patterns
- [Ecommerce UX Audit Checklist For 2026](https://www.convertcart.com/blog/ecommerce-ux-audit-checklist) - Production readiness checklist

### Tertiary (LOW confidence)
- Various Medium articles and community discussions - Used for ecosystem discovery, cross-verified with official docs

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All tools are official, widely adopted, well-documented
- Architecture: HIGH - WebApplicationFactory + Testcontainers is proven pattern; MassTransit test harness is official
- Pitfalls: MEDIUM - Based on community experience and blog posts, cross-verified where possible
- UX Polish: MEDIUM - Based on 2026 e-commerce trends, not project-specific audit

**Research date:** 2026-02-12
**Valid until:** 2026-05-12 (90 days for stable testing stack; .NET 10 stable, tools mature)

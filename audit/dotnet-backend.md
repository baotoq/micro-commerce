# Catalog.API .NET Backend Audit

Reviewed against 6 skills (dotnet-backend-patterns, dotnet-best-practices, dotnet-design-pattern-review, csharp-async, csharp-xunit, ef-core).
Strongest area: async correctness, CancellationToken propagation, and xUnit test structure (AAA + Theory + scoped DbContext per test).
Weakest area: EF Core read-path hygiene (no `AsNoTracking()`, no projections, no resilience/retry config) and exception-based control flow at API boundaries.

---

### [High] Read-path queries materialize tracked entities instead of projecting
- **Where**: `src/Services/Catalog.API/src/Application/Products/Queries/GetProducts.cs:30-36`, `GetProductBySku.cs:17`, `Api/SeedData/ProductSeeder.cs:24`
- **Issue**: `GetProductsHandler` does `query.OrderByDescending(...).Skip().Take().ToListAsync()` with no `AsNoTracking()` and no projection — every page load loads full tracked `Product` entities into the change tracker, then maps to DTOs in memory. `GetProductBySkuHandler` and the seeder do the same. `ProductSeeder.SeedAsync` even calls `db.Products.ToListAsync()` purely to extract SKUs.
- **Skill rule**: `ef-core` ("Use AsNoTracking() for read-only queries"; "Consider projection (Select) to retrieve only required fields"); `dotnet-backend-patterns/references/ef-core-best-practices.md` rules 1 and 2.
- **Fix**: Add `.AsNoTracking()` to all read handlers and project into `ProductDto` inside the query (`.Select(p => new ProductDto(p.Sku.Value, p.Name, ...))`). In the seeder, replace the `ToListAsync` + `HashSet` with `db.Products.Select(p => p.Sku).ToListAsync()` (or use `AnyAsync` per SKU when seed sets are small).

### [High] Business rule violation surfaced via `throw` + catch-on-the-endpoint
- **Where**: `src/Services/Catalog.API/src/Application/Products/Commands/CreateProduct.cs:18-19`, caught at `src/Services/Catalog.API/src/Api/Endpoints/ProductWriteEndpoints.cs:14-22`
- **Issue**: Duplicate-SKU is a normal validation outcome but is signalled with `throw new InvalidOperationException(...)`, then the Minimal API endpoint wraps every POST in `try/catch (InvalidOperationException)` to produce a 409. This is exception-driven control flow — the exact anti-pattern the skill calls out — and it also brittle-couples the endpoint to the handler's choice of exception type.
- **Skill rule**: `dotnet-backend-patterns` Best Practices DO #4 ("Return Result types instead of throwing exceptions for business logic"); `dotnet-design-pattern-review` ("consistent error handling").
- **Fix**: Return a `Result<ProductDto>` (or `OneOf<ProductDto, Conflict>`) from the handler and translate to `Results.Conflict(...)` in the endpoint. The endpoint then has no `try/catch`.

### [High] `CreateProduct` has a duplicate-SKU race (TOCTOU)
- **Where**: `src/Services/Catalog.API/src/Application/Products/Commands/CreateProduct.cs:18-23`
- **Issue**: `AnyAsync(p => p.Sku == sku)` followed by `db.Products.Add(...)` + `SaveChangesAsync` has no transaction; two concurrent requests with the same SKU both pass the pre-check, then one of them blows up at `SaveChangesAsync` with a Postgres unique-constraint violation that is not caught — the client gets a 500 instead of the intended 409. The unique index defined in `ProductConfiguration.cs:20` is the real guarantee; the pre-check is decorative.
- **Skill rule**: `dotnet-backend-patterns` Concurrency/Transactions; `ef-core` Best Practices "Change Tracking & Saving" (concurrency control).
- **Fix**: Drop the pre-check (or keep it as an optimization), wrap `SaveChangesAsync` in a `try/catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })`, and return the Conflict result from that branch.

### [Medium] Counts query risks empty-set bug and over-grouping
- **Where**: `src/Services/Catalog.API/src/Application/Products/Queries/GetProductCounts.cs:15-29`
- **Issue**: Using `GroupBy(_ => 1).Select(...).FirstOrDefaultAsync()` returns `null` when the table is empty (handled), but this pattern materializes one row per group via a `GROUP BY 1`. The simpler and faster shape is a single round-trip with conditional aggregates: `SELECT COUNT(*), COUNT(*) FILTER (WHERE Status='Active'), ...` which EF expresses cleanly as anonymous-projection on the whole `DbSet` without `GroupBy`.
- **Skill rule**: `ef-core` Performance ("Consider projection (Select) to retrieve only required fields"); `dotnet-backend-patterns` ("Select only needed columns").
- **Fix**: Replace with `await db.Products.AsNoTracking().Select(_ => 1).GroupBy(_ => 1)...` removed entirely — use `await db.Products.GroupBy(p => 1).Select(g => new ProductCountsDto(g.Count(), g.Count(p => p.Status == Active), ...)).FirstOrDefaultAsync(ct) ?? new(0,0,0,0,0)` or, cleaner, fire one query with `CountAsync` filtered per status via projection on a constant.

### [Medium] No EF retry / pooling / NoTracking-by-default configured
- **Where**: `src/Services/Catalog.API/src/Infrastructure/InfrastructureExtensions.cs:10`
- **Issue**: `AddNpgsqlDbContext<AppDbContext>("catalogdb")` is the only call — there's no `UseQueryTrackingBehavior(NoTracking)`, no `EnableRetryOnFailure`, and no `AddDbContextPool` consideration. Aspire's Npgsql integration adds resilience for the connection but does not set EF tracking behavior or retry strategy.
- **Skill rule**: `dotnet-backend-patterns/references/ef-core-best-practices.md` Rules 8 & 9 ("Configure Connection Pooling", "Use DbContext Pooling").
- **Fix**: Pass a configure-action: `builder.AddNpgsqlDbContext<AppDbContext>("catalogdb", configureDbContextOptions: o => o.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking))`. Consider `AddNpgsqlDbContextPool` if the workload warrants it.

### [Medium] Application handlers depend on `Infrastructure` (Clean Architecture inversion)
- **Where**: `src/Services/Catalog.API/src/Application/Products/Commands/CreateProduct.cs:5` (and every other handler), `src/Application/MicroCommerce.Catalog.Application.csproj` (ProjectReference to Infrastructure implied)
- **Issue**: The CLAUDE.md states "Application → MediatR ... depends on Domain" and "Infrastructure ... depends on Domain", but every handler imports `MicroCommerce.Catalog.Infrastructure.Persistence` to consume `AppDbContext`. The dependency arrow is Application → Infrastructure, which inverts the Clean Architecture direction the skill prescribes. (Note: the project convention is "no repository pattern, handlers inject DbContext directly". That part is intentional and not flagged. The inversion problem is independent: `AppDbContext` lives in the wrong layer.)
- **Skill rule**: `dotnet-backend-patterns` Project Structure (Application depends on Domain only; Infrastructure depends on Domain); `dotnet-design-pattern-review` SOLID — Dependency Inversion.
- **Fix**: Move `AppDbContext` (or an `IAppDbContext` abstraction exposing `DbSet<Product>` and `SaveChangesAsync`) into a shared place Application can see — typically promote the context to Application or expose an interface from Application that Infrastructure implements. The DbSet-using handler bodies stay unchanged.

### [Medium] Endpoints lack input validation and `Results.ValidationProblem` responses
- **Where**: `src/Services/Catalog.API/src/Api/Endpoints/ProductWriteEndpoints.cs:12-31`
- **Issue**: `CreateProductCommand` / `UpdateProductCommand` go straight to MediatR with no validation pipeline. Negative price, empty name, or unknown status surface as raw 500 `ArgumentException`/`ArgumentOutOfRangeException` (thrown in the `Product` ctor / `ParseStatus`) rather than 400s. The Domain throws — but the Application/API never translates.
- **Skill rule**: `dotnet-backend-patterns` Best Practices DO #4 + DONT #10 ("Don't skip validation at API boundaries"); `dotnet-best-practices` ("Implement proper input validation and sanitization").
- **Fix**: Add FluentValidation (or a MediatR validation pipeline behavior) for the commands, returning `Results.ValidationProblem`. Catch `ArgumentException`/`VogenValidationException` in a single ProblemDetails mapper instead of leaking 500s.

### [Medium] Output cache wired but never used on any endpoint
- **Where**: `src/Services/Catalog.API/src/Api/Program.cs:10-11,39` and `Endpoints/ProductReadEndpoints.cs` (no `.CacheOutput()` calls)
- **Issue**: `AddRedisClientBuilder("cache").WithOutputCache()` and `app.UseOutputCache()` are configured, but no read endpoint calls `.CacheOutput(...)` — the Redis dependency does no work, and reads always hit Postgres. The Functional tests even ship a `NoOpOutputCacheStore` to defeat caching, which masks that no caching is happening in prod either.
- **Skill rule**: `dotnet-backend-patterns` Caching Patterns ("Cache aggressively with proper invalidation strategies").
- **Fix**: Add `.CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(1)).Tag("products"))` to `GET /api/products`, `/api/products/counts`, `/api/products/{sku}`, and call `IOutputCacheStore.EvictByTagAsync("products", ct)` from the write handlers (or a notification handler) after `SaveChangesAsync`.

### [Medium] Domain events are published *before* the transaction commits / publishes from the handler
- **Where**: `src/Services/Catalog.API/src/Application/Products/Commands/CreateProduct.cs:23-26`, `UpdateProduct.cs:21-24`, `DeleteProduct.cs:20-21`, and the Dapr handlers in `src/Api/EventHandlers/*.cs`
- **Issue**: The pattern is `SaveChangesAsync` → `publisher.Publish(new ...Event)` → MediatR fan-out → Dapr `PublishEventAsync`. If Dapr publish fails after the DB commit, the catalog and downstream consumers diverge silently (no outbox, no retry, exception bubbles to the endpoint as 500 even though the DB write succeeded). For Delete, the event is published after the row is gone — no way to compensate.
- **Skill rule**: `dotnet-backend-patterns` Common Pitfalls ("Cache Stampede" and resilience patterns); `ef-core` ("Implement concurrency control"). General reliability principle.
- **Fix**: Either implement a transactional outbox (write the event row in the same `SaveChangesAsync`, then a background dispatcher publishes), or at minimum wrap publish in `try/catch` + log and rely on Dapr's at-least-once retry. Mark the event handlers `IPublishNotificationHandler` with explicit fire-and-forget semantics.

### [Low] xUnit Theories use boolean-arg ternary to derive expected data
- **Where**: `src/Services/Catalog.API/tests/Application.Tests/Products/Queries/GetProductsHandlerTests.cs:42-57`
- **Issue**: The `Handle_StatusFilter_ReturnsOnlyMatchingProducts` theory builds the "non-matching" row with `ProductStatus.Draft == matchingStatus ? ProductStatus.Active : ProductStatus.Draft`. The skill's rule "Keep tests focused on a single behavior" and "Use meaningful parameter names in data-driven tests" is bent here — the test data depends on an in-test branch which obscures what's being verified for the `Draft` case.
- **Skill rule**: `csharp-xunit` Data-Driven Tests / Standard Tests.
- **Fix**: Replace with `[MemberData]` returning an explicit `(string filter, ProductStatus matching, ProductStatus nonMatching)` tuple per case.

---

## Not flagged (intentional project convention)

- **No repository pattern** — handlers inject `AppDbContext` directly. Explicit project convention per CLAUDE.md.
- **MediatR v12 records implementing `IRequest<T>`** — used throughout commands/queries; matches both the convention and CQRS guidance.
- **Vogen value objects (`Sku.From`, `ProductId.From`)** — not the GoF/skill "Factory Pattern" but the project's standard. Used correctly (string conversion via `HasConversion` in `ProductConfiguration.cs:14,17`).
- **`async void`, `.Result`, `.Wait()`** — none present. `csharp-async` core rules pass.
- **`CancellationToken` propagation** — every async method takes and forwards `ct`. `csharp-async` rule satisfied.
- **Primary constructor DI** — `dotnet-best-practices` recommends this and the code uses it consistently (`CreateProductHandler(AppDbContext db, IPublisher publisher)`, `AppDbContext(DbContextOptions<AppDbContext>)`, etc.).
- **Test pyramid (Domain.Tests / Application.Tests / FunctionalTests / IntegrationTests)** — matches the skill's recommended split (`csharp-xunit` + `dotnet-backend-patterns` testing section). AAA pattern, `IClassFixture`, `ICollectionFixture`, Testcontainers, and `WebApplicationFactory` are all used correctly.
- **No XML doc comments on internal/handler types** — `dotnet-best-practices` asks for XML comments on public APIs; in this code the "public API" is the HTTP surface, which OpenAPI handles. Not worth flagging.
- **MSTest/FluentAssertions** — `dotnet-best-practices` mentions MSTest, but the project uses xUnit per CLAUDE.md. The other skill (`csharp-xunit`) endorses this — no conflict to act on.
- **No `ConfigureAwait(false)`** — ASP.NET Core has no synchronization context, so this is correct in a Web API host (the skill's "in libraries" caveat doesn't apply to handlers/endpoints).

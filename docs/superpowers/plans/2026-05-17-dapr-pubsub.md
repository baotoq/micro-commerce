# Dapr Pub/Sub — Domain Events Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Publish `ProductCreated`, `ProductUpdated`, and `ProductDeleted` domain events via Dapr pub/sub whenever catalog write commands succeed.

**Architecture:** Command handlers (Application layer) raise MediatR `INotification` events via `IPublisher` — no Dapr imports. Notification handlers in the Api layer consume those events and call `DaprClient.PublishEventAsync`. MediatR is already scanning the Application assembly; `Program.cs` gets a second `AddMediatR` call to also scan the Api assembly so the new handlers are picked up.

**Tech Stack:** MediatR v12.5.0 (`IPublisher`, `INotification`, `INotificationHandler`), `Dapr.AspNetCore` v1.17.9 (`DaprClient`), `CommunityToolkit.Aspire.Hosting.Dapr` v13.0.0 (`AddDaprPubSub`), xUnit v3.

---

## File Map

**New files:**
- `src/Services/Catalog.API/src/Application/Products/Events/ProductCreatedEvent.cs`
- `src/Services/Catalog.API/src/Application/Products/Events/ProductUpdatedEvent.cs`
- `src/Services/Catalog.API/src/Application/Products/Events/ProductDeletedEvent.cs`
- `src/Services/Catalog.API/src/Api/EventHandlers/ProductCreatedEventHandler.cs`
- `src/Services/Catalog.API/src/Api/EventHandlers/ProductUpdatedEventHandler.cs`
- `src/Services/Catalog.API/src/Api/EventHandlers/ProductDeletedEventHandler.cs`
- `src/Services/Catalog.API/tests/Application.Tests/FakePublisher.cs`

**Modified files:**
- `src/AppHost/AppHost.cs`
- `src/Services/Catalog.API/src/Application/Products/Commands/CreateProduct.cs`
- `src/Services/Catalog.API/src/Application/Products/Commands/UpdateProduct.cs`
- `src/Services/Catalog.API/src/Application/Products/Commands/DeleteProduct.cs`
- `src/Services/Catalog.API/src/Api/Program.cs`
- `src/Services/Catalog.API/tests/Application.Tests/Products/Commands/CreateProductHandlerTests.cs`
- `src/Services/Catalog.API/tests/Application.Tests/Products/Commands/UpdateProductHandlerTests.cs`
- `src/Services/Catalog.API/tests/Application.Tests/Products/Commands/DeleteProductHandlerTests.cs`

---

### Task 1: Wire pub/sub component in AppHost

**Files:**
- Modify: `src/AppHost/AppHost.cs`

- [ ] **Step 1: Update AppHost.cs**

Replace the full file:

```csharp
#pragma warning disable ASPIREJAVASCRIPT001

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache").WithDataVolume().WithLifetime(ContainerLifetime.Persistent);

var postgres = builder.AddPostgres("postgres").WithDataVolume().WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("catalogdb");

var pubSub = builder.AddDaprPubSub("pubsub");

var catalog = builder.AddProject<Projects.MicroCommerce_Catalog>("catalog-api")
    .WithReference(cache)
    .WithReference(postgres)
    .WaitFor(cache)
    .WaitFor(postgres)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .WithDaprSidecar(sidecar => sidecar.WithReference(pubSub));

if (builder.Environment.EnvironmentName != "Testing")
{
    builder.AddNextJsApp("web", "../web")
        .WithEnvironment("API_URL", catalog.GetEndpoint("http"))
        .WithExternalHttpEndpoints();
}

builder.Build().Run();
```

- [ ] **Step 2: Build AppHost to verify**

```bash
dotnet build src/AppHost
```

Expected: `Build succeeded, 0 Error(s)`.

- [ ] **Step 3: Commit**

```bash
git add src/AppHost/AppHost.cs
git commit -m "feat(apphost): wire Dapr pub/sub component"
```

---

### Task 2: Define domain event records

**Files:**
- Create: `src/Services/Catalog.API/src/Application/Products/Events/ProductCreatedEvent.cs`
- Create: `src/Services/Catalog.API/src/Application/Products/Events/ProductUpdatedEvent.cs`
- Create: `src/Services/Catalog.API/src/Application/Products/Events/ProductDeletedEvent.cs`

These are pure data records; no tests.

- [ ] **Step 1: Create ProductCreatedEvent.cs**

```csharp
using MediatR;

namespace MicroCommerce.Catalog.Application.Products.Events;

public record ProductCreatedEvent(
    string Sku,
    string Name,
    string Category,
    decimal Price,
    int Inventory,
    string Status) : INotification;
```

- [ ] **Step 2: Create ProductUpdatedEvent.cs**

```csharp
using MediatR;

namespace MicroCommerce.Catalog.Application.Products.Events;

public record ProductUpdatedEvent(
    string Sku,
    string Name,
    string Category,
    decimal Price,
    int Inventory,
    string Status) : INotification;
```

- [ ] **Step 3: Create ProductDeletedEvent.cs**

```csharp
using MediatR;

namespace MicroCommerce.Catalog.Application.Products.Events;

public record ProductDeletedEvent(string Sku) : INotification;
```

- [ ] **Step 4: Build Application layer**

```bash
dotnet build src/Services/Catalog.API/src/Application
```

Expected: `Build succeeded, 0 Error(s)`.

- [ ] **Step 5: Commit**

```bash
git add src/Services/Catalog.API/src/Application/Products/Events/
git commit -m "feat(catalog): add product domain event records"
```

---

### Task 3: TDD — CreateProductHandler publishes ProductCreatedEvent

**Files:**
- Create: `src/Services/Catalog.API/tests/Application.Tests/FakePublisher.cs`
- Modify: `src/Services/Catalog.API/tests/Application.Tests/Products/Commands/CreateProductHandlerTests.cs`
- Modify: `src/Services/Catalog.API/src/Application/Products/Commands/CreateProduct.cs`

- [ ] **Step 1: Create FakePublisher.cs**

`FakePublisher` is a simple test double used across all three handler test classes. It implements MediatR's `IPublisher` and records every notification published.

```csharp
using MediatR;

namespace MicroCommerce.Catalog.Application.Tests;

sealed class FakePublisher : IPublisher
{
    public List<INotification> Published { get; } = [];

    public Task Publish(object notification, CancellationToken cancellationToken = default)
    {
        if (notification is INotification n) Published.Add(n);
        return Task.CompletedTask;
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : notnull
    {
        if (notification is INotification n) Published.Add(n);
        return Task.CompletedTask;
    }
}
```

- [ ] **Step 2: Update CreateProductHandlerTests.cs**

All existing tests must pass `new FakePublisher()` to the handler constructor (which is about to gain a second parameter). Two new tests are added: one asserting the event is published on success, one asserting it is not published on duplicate SKU.

Replace the full file:

```csharp
using MicroCommerce.Catalog.Application.Products.Commands;
using MicroCommerce.Catalog.Application.Products.Dtos;
using MicroCommerce.Catalog.Application.Products.Events;

namespace MicroCommerce.Catalog.Application.Tests.Products.Commands;

public class CreateProductHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesProductAndReturnsDto()
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreateProductHandler(db, new FakePublisher());

        var result = await handler.Handle(new CreateProductCommand("MC-001", "Widget", "Electronics", 9.99m, 10, "active"), TestContext.Current.CancellationToken);

        Assert.Equivalent(new ProductDto("MC-001", "Widget", "Electronics", 9.99m, 10, "active", 0), result);
        Assert.Single(db.Products);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesProductCreatedEvent()
    {
        await using var db = DbContextFactory.Create();
        var publisher = new FakePublisher();
        var handler = new CreateProductHandler(db, publisher);

        await handler.Handle(new CreateProductCommand("MC-001", "Widget", "Electronics", 9.99m, 10, "active"), TestContext.Current.CancellationToken);

        var evt = Assert.IsType<ProductCreatedEvent>(Assert.Single(publisher.Published));
        Assert.Equal("MC-001", evt.Sku);
        Assert.Equal("Widget", evt.Name);
        Assert.Equal("Electronics", evt.Category);
        Assert.Equal(9.99m, evt.Price);
        Assert.Equal(10, evt.Inventory);
        Assert.Equal("active", evt.Status);
    }

    [Fact]
    public async Task Handle_DuplicateSku_DoesNotPublishEvent()
    {
        await using var db = DbContextFactory.Create();
        var publisher = new FakePublisher();
        var handler = new CreateProductHandler(db, publisher);
        await handler.Handle(new CreateProductCommand("MC-001", "Widget", "Electronics", 9.99m, 10, "active"), TestContext.Current.CancellationToken);
        publisher.Published.Clear();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new CreateProductCommand("MC-001", "Other", "Electronics", 5m, 1, "active"), TestContext.Current.CancellationToken));

        Assert.Empty(publisher.Published);
    }

    [Theory]
    [InlineData("active")]
    [InlineData("low")]
    [InlineData("out")]
    [InlineData("draft")]
    [InlineData("ACTIVE")]
    public async Task Handle_ValidStatus_CreatesProduct(string status)
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreateProductHandler(db, new FakePublisher());

        var result = await handler.Handle(new CreateProductCommand("MC-001", "Widget", "Cat", 1m, 1, status), TestContext.Current.CancellationToken);

        Assert.Equal(status.ToLowerInvariant(), result.Status);
    }

    [Fact]
    public async Task Handle_InvalidStatus_ThrowsArgumentException()
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreateProductHandler(db, new FakePublisher());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.Handle(new CreateProductCommand("MC-001", "Widget", "Cat", 1m, 1, "unknown"), TestContext.Current.CancellationToken));
    }
}
```

- [ ] **Step 3: Build tests — expect compile failure**

```bash
dotnet build src/Services/Catalog.API/tests/Application.Tests
```

Expected: Build error — `CreateProductHandler` does not have a constructor that takes 2 arguments.

- [ ] **Step 4: Update CreateProduct.cs**

Replace the full file:

```csharp
using MediatR;
using MicroCommerce.Catalog.Application.Products.Dtos;
using MicroCommerce.Catalog.Application.Products.Events;
using MicroCommerce.Catalog.Domain.Products;
using MicroCommerce.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Products.Commands;

public record CreateProductCommand(string Sku, string Name, string Category, decimal Price, int Inventory, string Status) : IRequest<ProductDto>;

public class CreateProductHandler(AppDbContext db, IPublisher publisher) : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var sku = Sku.From(request.Sku);

        if (await db.Products.AnyAsync(p => p.Sku == sku, ct))
            throw new InvalidOperationException($"Product with SKU '{request.Sku}' already exists.");

        var product = new Product(sku, request.Name, request.Category, request.Price, request.Inventory, ParseStatus(request.Status));
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        var dto = ToDto(product);
        await publisher.Publish(new ProductCreatedEvent(dto.Sku, dto.Name, dto.Category, dto.Price, dto.Inventory, dto.Status), ct);
        return dto;
    }

    internal static ProductDto ToDto(Product p) =>
        new(p.Sku.Value, p.Name, p.Category, p.Price, p.Inventory, p.Status.ToString().ToLowerInvariant(), p.Views7d);

    internal static ProductStatus ParseStatus(string s) => s.ToLowerInvariant() switch
    {
        "active" => ProductStatus.Active,
        "low" => ProductStatus.Low,
        "out" => ProductStatus.Out,
        "draft" => ProductStatus.Draft,
        _ => throw new ArgumentException($"Invalid status: {s}"),
    };
}
```

- [ ] **Step 5: Run CreateProductHandler tests**

```bash
dotnet test src/Services/Catalog.API/tests/Application.Tests --filter "FullyQualifiedName~CreateProductHandler"
```

Expected: All tests pass.

- [ ] **Step 6: Commit**

```bash
git add src/Services/Catalog.API/tests/Application.Tests/FakePublisher.cs \
        src/Services/Catalog.API/tests/Application.Tests/Products/Commands/CreateProductHandlerTests.cs \
        src/Services/Catalog.API/src/Application/Products/Commands/CreateProduct.cs
git commit -m "feat(catalog): CreateProductHandler publishes ProductCreatedEvent"
```

---

### Task 4: TDD — UpdateProductHandler publishes ProductUpdatedEvent

**Files:**
- Modify: `src/Services/Catalog.API/tests/Application.Tests/Products/Commands/UpdateProductHandlerTests.cs`
- Modify: `src/Services/Catalog.API/src/Application/Products/Commands/UpdateProduct.cs`

- [ ] **Step 1: Update UpdateProductHandlerTests.cs**

Replace the full file:

```csharp
using MicroCommerce.Catalog.Application.Products.Commands;
using MicroCommerce.Catalog.Application.Products.Dtos;
using MicroCommerce.Catalog.Application.Products.Events;
using MicroCommerce.Catalog.Domain.Products;

namespace MicroCommerce.Catalog.Application.Tests.Products.Commands;

public class UpdateProductHandlerTests
{
    [Fact]
    public async Task Handle_ExistingProduct_UpdatesAndReturnsDto()
    {
        await using var db = DbContextFactory.Create();
        db.Products.Add(new Product(Sku.From("MC-001"), "Widget", "Electronics", 9.99m, 10, ProductStatus.Active));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateProductHandler(db, new FakePublisher());
        var result = await handler.Handle(new UpdateProductCommand("MC-001", "Updated", "Gadgets", 19.99m, 5, "low"), TestContext.Current.CancellationToken);

        Assert.Equivalent(new ProductDto("MC-001", "Updated", "Gadgets", 19.99m, 5, "low", 0), result);
    }

    [Fact]
    public async Task Handle_ExistingProduct_PublishesProductUpdatedEvent()
    {
        await using var db = DbContextFactory.Create();
        db.Products.Add(new Product(Sku.From("MC-001"), "Widget", "Electronics", 9.99m, 10, ProductStatus.Active));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var publisher = new FakePublisher();
        var handler = new UpdateProductHandler(db, publisher);
        await handler.Handle(new UpdateProductCommand("MC-001", "Updated", "Gadgets", 19.99m, 5, "low"), TestContext.Current.CancellationToken);

        var evt = Assert.IsType<ProductUpdatedEvent>(Assert.Single(publisher.Published));
        Assert.Equal("MC-001", evt.Sku);
        Assert.Equal("Updated", evt.Name);
        Assert.Equal("Gadgets", evt.Category);
        Assert.Equal(19.99m, evt.Price);
        Assert.Equal(5, evt.Inventory);
        Assert.Equal("low", evt.Status);
    }

    [Fact]
    public async Task Handle_NonExistentSku_ReturnsNull()
    {
        await using var db = DbContextFactory.Create();
        var handler = new UpdateProductHandler(db, new FakePublisher());

        var result = await handler.Handle(new UpdateProductCommand("MC-999", "X", "Y", 1m, 1, "active"), TestContext.Current.CancellationToken);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_NonExistentSku_DoesNotPublishEvent()
    {
        await using var db = DbContextFactory.Create();
        var publisher = new FakePublisher();
        var handler = new UpdateProductHandler(db, publisher);

        await handler.Handle(new UpdateProductCommand("MC-999", "X", "Y", 1m, 1, "active"), TestContext.Current.CancellationToken);

        Assert.Empty(publisher.Published);
    }
}
```

- [ ] **Step 2: Build tests — expect compile failure**

```bash
dotnet build src/Services/Catalog.API/tests/Application.Tests
```

Expected: Build error — `UpdateProductHandler` does not have a constructor that takes 2 arguments.

- [ ] **Step 3: Update UpdateProduct.cs**

Replace the full file:

```csharp
using MediatR;
using MicroCommerce.Catalog.Application.Products.Dtos;
using MicroCommerce.Catalog.Application.Products.Events;
using MicroCommerce.Catalog.Domain.Products;
using MicroCommerce.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Products.Commands;

public record UpdateProductCommand(string Sku, string Name, string Category, decimal Price, int Inventory, string Status) : IRequest<ProductDto?>;

public class UpdateProductHandler(AppDbContext db, IPublisher publisher) : IRequestHandler<UpdateProductCommand, ProductDto?>
{
    public async Task<ProductDto?> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var sku = Sku.From(request.Sku);
        var product = await db.Products.FirstOrDefaultAsync(p => p.Sku == sku, ct);
        if (product is null) return null;

        product.Update(request.Name, request.Category, request.Price, request.Inventory, CreateProductHandler.ParseStatus(request.Status));
        await db.SaveChangesAsync(ct);

        var dto = CreateProductHandler.ToDto(product);
        await publisher.Publish(new ProductUpdatedEvent(dto.Sku, dto.Name, dto.Category, dto.Price, dto.Inventory, dto.Status), ct);
        return dto;
    }
}
```

- [ ] **Step 4: Run UpdateProductHandler tests**

```bash
dotnet test src/Services/Catalog.API/tests/Application.Tests --filter "FullyQualifiedName~UpdateProductHandler"
```

Expected: All tests pass.

- [ ] **Step 5: Commit**

```bash
git add src/Services/Catalog.API/tests/Application.Tests/Products/Commands/UpdateProductHandlerTests.cs \
        src/Services/Catalog.API/src/Application/Products/Commands/UpdateProduct.cs
git commit -m "feat(catalog): UpdateProductHandler publishes ProductUpdatedEvent"
```

---

### Task 5: TDD — DeleteProductHandler publishes ProductDeletedEvent

**Files:**
- Modify: `src/Services/Catalog.API/tests/Application.Tests/Products/Commands/DeleteProductHandlerTests.cs`
- Modify: `src/Services/Catalog.API/src/Application/Products/Commands/DeleteProduct.cs`

- [ ] **Step 1: Update DeleteProductHandlerTests.cs**

Replace the full file:

```csharp
using MicroCommerce.Catalog.Application.Products.Commands;
using MicroCommerce.Catalog.Application.Products.Events;
using MicroCommerce.Catalog.Domain.Products;

namespace MicroCommerce.Catalog.Application.Tests.Products.Commands;

public class DeleteProductHandlerTests
{
    [Fact]
    public async Task Handle_ExistingProduct_RemovesAndReturnsTrue()
    {
        await using var db = DbContextFactory.Create();
        db.Products.Add(new Product(Sku.From("MC-001"), "Widget", "Electronics", 9.99m, 10, ProductStatus.Active));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new DeleteProductHandler(db, new FakePublisher());
        var result = await handler.Handle(new DeleteProductCommand("MC-001"), TestContext.Current.CancellationToken);

        Assert.True(result);
        Assert.Empty(db.Products);
    }

    [Fact]
    public async Task Handle_ExistingProduct_PublishesProductDeletedEvent()
    {
        await using var db = DbContextFactory.Create();
        db.Products.Add(new Product(Sku.From("MC-001"), "Widget", "Electronics", 9.99m, 10, ProductStatus.Active));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var publisher = new FakePublisher();
        var handler = new DeleteProductHandler(db, publisher);
        await handler.Handle(new DeleteProductCommand("MC-001"), TestContext.Current.CancellationToken);

        var evt = Assert.IsType<ProductDeletedEvent>(Assert.Single(publisher.Published));
        Assert.Equal("MC-001", evt.Sku);
    }

    [Fact]
    public async Task Handle_NonExistentSku_ReturnsFalse()
    {
        await using var db = DbContextFactory.Create();
        var handler = new DeleteProductHandler(db, new FakePublisher());

        var result = await handler.Handle(new DeleteProductCommand("MC-999"), TestContext.Current.CancellationToken);

        Assert.False(result);
    }

    [Fact]
    public async Task Handle_NonExistentSku_DoesNotPublishEvent()
    {
        await using var db = DbContextFactory.Create();
        var publisher = new FakePublisher();
        var handler = new DeleteProductHandler(db, publisher);

        await handler.Handle(new DeleteProductCommand("MC-999"), TestContext.Current.CancellationToken);

        Assert.Empty(publisher.Published);
    }
}
```

- [ ] **Step 2: Build tests — expect compile failure**

```bash
dotnet build src/Services/Catalog.API/tests/Application.Tests
```

Expected: Build error — `DeleteProductHandler` does not have a constructor that takes 2 arguments.

- [ ] **Step 3: Update DeleteProduct.cs**

Replace the full file:

```csharp
using MediatR;
using MicroCommerce.Catalog.Application.Products.Events;
using MicroCommerce.Catalog.Domain.Products;
using MicroCommerce.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Products.Commands;

public record DeleteProductCommand(string Sku) : IRequest<bool>;

public class DeleteProductHandler(AppDbContext db, IPublisher publisher) : IRequestHandler<DeleteProductCommand, bool>
{
    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var sku = Sku.From(request.Sku);
        var product = await db.Products.FirstOrDefaultAsync(p => p.Sku == sku, ct);
        if (product is null) return false;

        db.Products.Remove(product);
        await db.SaveChangesAsync(ct);
        await publisher.Publish(new ProductDeletedEvent(request.Sku), ct);
        return true;
    }
}
```

- [ ] **Step 4: Run DeleteProductHandler tests**

```bash
dotnet test src/Services/Catalog.API/tests/Application.Tests --filter "FullyQualifiedName~DeleteProductHandler"
```

Expected: All tests pass.

- [ ] **Step 5: Commit**

```bash
git add src/Services/Catalog.API/tests/Application.Tests/Products/Commands/DeleteProductHandlerTests.cs \
        src/Services/Catalog.API/src/Application/Products/Commands/DeleteProduct.cs
git commit -m "feat(catalog): DeleteProductHandler publishes ProductDeletedEvent"
```

---

### Task 6: Notification handlers and Program.cs

**Files:**
- Create: `src/Services/Catalog.API/src/Api/EventHandlers/ProductCreatedEventHandler.cs`
- Create: `src/Services/Catalog.API/src/Api/EventHandlers/ProductUpdatedEventHandler.cs`
- Create: `src/Services/Catalog.API/src/Api/EventHandlers/ProductDeletedEventHandler.cs`
- Modify: `src/Services/Catalog.API/src/Api/Program.cs`

These handlers are one-liners exercised by the existing functional/integration test suites.

- [ ] **Step 1: Create ProductCreatedEventHandler.cs**

```csharp
using Dapr.Client;
using MediatR;
using MicroCommerce.Catalog.Application.Products.Events;

namespace MicroCommerce.Catalog.Api.EventHandlers;

class ProductCreatedEventHandler(DaprClient dapr) : INotificationHandler<ProductCreatedEvent>
{
    public Task Handle(ProductCreatedEvent e, CancellationToken ct)
        => dapr.PublishEventAsync("pubsub", "catalog.product.created", e, ct);
}
```

- [ ] **Step 2: Create ProductUpdatedEventHandler.cs**

```csharp
using Dapr.Client;
using MediatR;
using MicroCommerce.Catalog.Application.Products.Events;

namespace MicroCommerce.Catalog.Api.EventHandlers;

class ProductUpdatedEventHandler(DaprClient dapr) : INotificationHandler<ProductUpdatedEvent>
{
    public Task Handle(ProductUpdatedEvent e, CancellationToken ct)
        => dapr.PublishEventAsync("pubsub", "catalog.product.updated", e, ct);
}
```

- [ ] **Step 3: Create ProductDeletedEventHandler.cs**

```csharp
using Dapr.Client;
using MediatR;
using MicroCommerce.Catalog.Application.Products.Events;

namespace MicroCommerce.Catalog.Api.EventHandlers;

class ProductDeletedEventHandler(DaprClient dapr) : INotificationHandler<ProductDeletedEvent>
{
    public Task Handle(ProductDeletedEvent e, CancellationToken ct)
        => dapr.PublishEventAsync("pubsub", "catalog.product.deleted", e, ct);
}
```

- [ ] **Step 4: Update Program.cs**

Add one line after `builder.Services.AddApplication()` to also scan the Api assembly for `INotificationHandler` registrations. Replace the full file:

```csharp
using MicroCommerce.Catalog.Application;
using MicroCommerce.Catalog.Infrastructure;
using MicroCommerce.Catalog.Infrastructure.Persistence;
using MicroCommerce.Catalog.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddRedisClientBuilder("cache")
    .WithOutputCache();
builder.AddInfrastructure();

builder.Services.AddApplication();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddDaprClient();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreatedAsync();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseOutputCache();

app.MapDefaultEndpoints();
app.MapProductReadEndpoints();
app.MapProductWriteEndpoints();

app.UseFileServer();

app.Run();
```

- [ ] **Step 5: Build the full solution**

```bash
dotnet build src/MicroCommerce.slnx
```

Expected: `Build succeeded, 0 Error(s)`.

- [ ] **Step 6: Commit**

```bash
git add src/Services/Catalog.API/src/Api/EventHandlers/ \
        src/Services/Catalog.API/src/Api/Program.cs
git commit -m "feat(catalog): add Dapr notification handlers for product events"
```

---

### Task 7: Verify full test suite

- [ ] **Step 1: Run all non-Docker tests**

```bash
dotnet test src/Services/Catalog.API/tests/Domain.Tests && \
dotnet test src/Services/Catalog.API/tests/Application.Tests
```

Expected: All tests pass, 0 failures.

- [ ] **Step 2: Run FunctionalTests (requires Docker)**

```bash
dotnet test src/Services/Catalog.API/tests/FunctionalTests
```

Expected: All tests pass. Skip if Docker is unavailable.

- [ ] **Step 3: Done**

The bus is ready. Topics `catalog.product.created`, `catalog.product.updated`, `catalog.product.deleted` are live on the `pubsub` component. Future subscriber services consume them by calling `MapSubscribeHandler()` + `[Topic("pubsub", "catalog.product.created")]` on their own endpoints.

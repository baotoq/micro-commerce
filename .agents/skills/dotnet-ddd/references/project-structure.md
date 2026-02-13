# Project Structure

Recommended .NET solution layout for a DDD application with Clean Architecture layers.

## Solution Structure

```
src/
├── MyApp.Domain/                  # Domain Layer (innermost, no dependencies)
│   ├── Common/
│   │   ├── Entity.cs
│   │   ├── IDomainEvent.cs
│   │   ├── DomainEvent.cs
│   │   ├── DomainException.cs
│   │   ├── IUnitOfWork.cs
│   │   └── Guard.cs
│   ├── Orders/                    # Aggregate folder
│   │   ├── Order.cs               # Aggregate Root
│   │   ├── OrderLine.cs           # Child Entity
│   │   ├── OrderId.cs             # Strongly-typed ID
│   │   ├── OrderLineId.cs
│   │   ├── OrderStatus.cs         # Value Object / Enumeration
│   │   ├── IOrderRepository.cs    # Repository interface
│   │   ├── OrderErrors.cs         # Error constants
│   │   └── Events/
│   │       ├── OrderCreatedEvent.cs
│   │       └── OrderSubmittedEvent.cs
│   ├── Customers/
│   │   ├── Customer.cs
│   │   ├── CustomerId.cs
│   │   ├── Email.cs               # Value Object
│   │   └── ICustomerRepository.cs
│   └── SharedKernel/              # Value Objects shared across aggregates
│       ├── Money.cs
│       ├── Address.cs
│       └── DateRange.cs
│
├── MyApp.Application/             # Application Layer (orchestration)
│   ├── Common/
│   │   ├── IDomainEventDispatcher.cs
│   │   └── IIntegrationEventPublisher.cs
│   ├── Orders/
│   │   ├── Commands/
│   │   │   ├── CreateOrderCommand.cs
│   │   │   └── CreateOrderHandler.cs
│   │   ├── Queries/
│   │   │   ├── GetOrderQuery.cs
│   │   │   └── GetOrderHandler.cs
│   │   └── EventHandlers/
│   │       └── OrderCreatedEventHandler.cs
│   └── Customers/
│       └── ...
│
├── MyApp.Infrastructure/          # Infrastructure Layer (implementations)
│   ├── Persistence/
│   │   ├── AppDbContext.cs
│   │   ├── Configurations/
│   │   │   ├── OrderConfiguration.cs
│   │   │   └── CustomerConfiguration.cs
│   │   ├── Repositories/
│   │   │   ├── OrderRepository.cs
│   │   │   └── CustomerRepository.cs
│   │   └── Migrations/
│   ├── Messaging/
│   │   ├── OutboxProcessor.cs
│   │   └── RabbitMqPublisher.cs
│   └── DependencyInjection.cs     # Extension method for registering infra services
│
├── MyApp.Api/                     # Presentation Layer (entry point)
│   ├── Controllers/ or Endpoints/
│   ├── Middleware/
│   ├── Program.cs
│   └── appsettings.json
│
tests/
├── MyApp.Domain.Tests/
├── MyApp.Application.Tests/
├── MyApp.Infrastructure.Tests/
└── MyApp.Api.Tests/
```

## Layer Dependencies

```
Api → Application → Domain
 ↓
Infrastructure → Domain
                 Application (for interface implementations)
```

**Rules:**
- **Domain** depends on nothing (no project references, no NuGet packages except pure libraries like `System.Text.Json`)
- **Application** depends on **Domain** only
- **Infrastructure** depends on **Domain** and **Application** (implements their interfaces)
- **Api** depends on **Application** and **Infrastructure** (for DI registration)

## Project Files

### Domain (zero dependencies)

```xml
<!-- MyApp.Domain.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <!-- No PackageReference — domain is pure C# -->
</Project>
```

### Application

```xml
<!-- MyApp.Application.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\MyApp.Domain\MyApp.Domain.csproj" />
  </ItemGroup>
  <!-- Optional: MediatR, FluentValidation -->
</Project>
```

### Infrastructure

```xml
<!-- MyApp.Infrastructure.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\MyApp.Domain\MyApp.Domain.csproj" />
    <ProjectReference Include="..\MyApp.Application\MyApp.Application.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.*" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.*" />
    <!-- Or any other DB provider -->
  </ItemGroup>
</Project>
```

## Aggregate Folder Organization

Group by **Aggregate**, not by technical type:

```
# GOOD: Group by Aggregate (feature folder)
Domain/
├── Orders/
│   ├── Order.cs
│   ├── OrderLine.cs
│   ├── OrderId.cs
│   ├── IOrderRepository.cs
│   └── Events/

# BAD: Group by type
Domain/
├── Entities/
│   ├── Order.cs
│   ├── OrderLine.cs
│   ├── Customer.cs
├── ValueObjects/
│   ├── Money.cs
│   ├── Email.cs
├── Repositories/
│   ├── IOrderRepository.cs
```

The feature-folder approach keeps related code together and makes it easy to see what belongs to each Aggregate.

## EF Core Configuration (Infrastructure)

```csharp
// Infrastructure/Persistence/Configurations/OrderConfiguration.cs
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .HasConversion(id => id.Value, value => OrderId.From(value));

        builder.Property(o => o.CustomerId)
            .HasConversion(id => id.Value, value => CustomerId.From(value));

        // Owned entity (Value Object)
        builder.OwnsMany(o => o.Lines, lineBuilder =>
        {
            lineBuilder.ToTable("order_lines");
            lineBuilder.HasKey(l => l.Id);
            lineBuilder.Property(l => l.Id)
                .HasConversion(id => id.Value, value => OrderLineId.From(value));
            lineBuilder.Property(l => l.ProductId)
                .HasConversion(id => id.Value, value => ProductId.From(value));

            // Value Object as owned type
            lineBuilder.OwnsOne(l => l.UnitPrice, money =>
            {
                money.Property(m => m.Amount).HasColumnName("unit_price_amount");
                money.Property(m => m.Currency).HasColumnName("unit_price_currency");
            });
        });

        builder.Property(o => o.Status)
            .HasConversion<string>();

        // Ignore domain events (not persisted)
        builder.Ignore(o => o.DomainEvents);
    }
}
```

## DI Registration Pattern

```csharp
// Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IDomainEventDispatcher, InProcessDomainEventDispatcher>();

        return services;
    }
}

// In Program.cs
builder.Services.AddInfrastructure(builder.Configuration);
```

## Testing Strategy

| Layer | Test Type | What to Test |
|---|---|---|
| **Domain** | Unit tests | Aggregate behavior, invariants, value object equality, domain events |
| **Application** | Unit tests (with mocks) | Command/query handlers, orchestration logic |
| **Infrastructure** | Integration tests | Repository queries, DB migrations, message publishing |
| **Api** | Integration tests | Full request/response, middleware, auth |

### Domain Unit Test Example

```csharp
public class OrderTests
{
    [Fact]
    public void Submit_WithLines_ShouldChangeStatusAndRaiseEvent()
    {
        // Arrange
        var order = Order.Create(CustomerId.New());
        order.AddLine(ProductId.New(), 2, new Money(10.00m, "USD"));

        // Act
        order.Submit();

        // Assert
        Assert.Equal(OrderStatus.Submitted, order.Status);
        Assert.Contains(order.DomainEvents, e => e is OrderSubmittedEvent);
    }

    [Fact]
    public void Submit_EmptyOrder_ShouldThrow()
    {
        var order = Order.Create(CustomerId.New());

        Assert.Throws<DomainException>(() => order.Submit());
    }

    [Fact]
    public void AddLine_ToSubmittedOrder_ShouldThrow()
    {
        var order = Order.Create(CustomerId.New());
        order.AddLine(ProductId.New(), 1, new Money(5.00m, "USD"));
        order.Submit();

        Assert.Throws<DomainException>(() =>
            order.AddLine(ProductId.New(), 1, new Money(10.00m, "USD")));
    }
}
```

## Decision Guide

**When to use this full structure:**
- Multiple Aggregates with complex business rules
- Team of 2+ developers
- Long-lived project with evolving domain

**When to simplify:**
- Single Aggregate or very simple domain — collapse Domain + Application into one project
- Prototype / MVP — start with fewer layers, extract later
- CRUD-dominant — you probably don't need DDD

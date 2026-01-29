# Testing Guide

## Current State

This codebase is in **early development** with minimal test coverage. The testing infrastructure exists but test projects have not yet been implemented.

---

## Test Infrastructure

### CI/CD Pipeline

Tests run via GitHub Actions (`.github/workflows/dotnet-test.yml`):

```yaml
jobs:
  unit-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 9.0.x
      - run: dotnet workload install aspire
      - run: dotnet restore
      - run: dotnet build --no-restore
      - run: dotnet test --no-build --verbosity normal
```

**Triggers**:
- Push to `master` branch (paths: `code/**`, `.github/**`)
- Manual dispatch
- Workflow call (reusable)

---

## Recommended Test Structure

### .NET Test Projects

Follow the standard .NET test project naming convention:

```
code/
├── MicroCommerce.ApiService/
├── MicroCommerce.ApiService.Tests/           # Unit tests
├── MicroCommerce.ApiService.IntegrationTests/ # Integration tests
├── BuildingBlocks/
│   ├── BuildingBlocks.Common/
│   └── BuildingBlocks.Common.Tests/
└── MicroCommerce.Tests.Shared/               # Shared test utilities
```

### Recommended Frameworks

| Purpose | Package | Notes |
|---------|---------|-------|
| Test runner | `xUnit` | Standard for .NET |
| Assertions | `FluentAssertions` | Readable assertions |
| Mocking | `NSubstitute` or `Moq` | Interface mocking |
| Integration | `Microsoft.AspNetCore.Mvc.Testing` | WebApplicationFactory |
| Test containers | `Testcontainers` | Docker-based integration tests |
| Aspire testing | `Aspire.Hosting.Testing` | Aspire-specific testing |

### Sample Test Project Configuration

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
    <PackageReference Include="FluentAssertions" Version="6.*" />
    <PackageReference Include="NSubstitute" Version="5.*" />
    <PackageReference Include="coverlet.collector" Version="6.*" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\MicroCommerce.ApiService\MicroCommerce.ApiService.csproj" />
  </ItemGroup>
</Project>
```

---

## Unit Testing Patterns

### Testing Domain Entities

```csharp
public class AggregateRootTests
{
    [Fact]
    public void AddDomainEvent_ShouldAddEventToCollection()
    {
        // Arrange
        var aggregate = new TestAggregate(new TestId(Guid.NewGuid()));
        var domainEvent = new TestDomainEvent();

        // Act
        aggregate.RaiseDomainEvent(domainEvent);

        // Assert
        aggregate.DomainEvents.Should().ContainSingle()
            .Which.Should().Be(domainEvent);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var aggregate = new TestAggregate(new TestId(Guid.NewGuid()));
        aggregate.RaiseDomainEvent(new TestDomainEvent());

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();
    }
}
```

### Testing Value Objects

```csharp
public class ValueObjectTests
{
    [Fact]
    public void Equals_WithSameComponents_ShouldReturnTrue()
    {
        // Arrange
        var value1 = new Money(100, "USD");
        var value2 = new Money(100, "USD");

        // Assert
        value1.Should().Be(value2);
        (value1 == value2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameComponents_ShouldBeSame()
    {
        // Arrange
        var value1 = new Money(100, "USD");
        var value2 = new Money(100, "USD");

        // Assert
        value1.GetHashCode().Should().Be(value2.GetHashCode());
    }
}
```

### Testing Strongly Typed IDs

```csharp
public class EventIdTests
{
    [Fact]
    public void New_ShouldCreateUniqueId()
    {
        // Act
        var id1 = EventId.New();
        var id2 = EventId.New();

        // Assert
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void From_ShouldCreateIdWithGivenValue()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var id = EventId.From(guid);

        // Assert
        id.Value.Should().Be(guid);
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id = EventId.From(guid);

        // Assert
        id.ToString().Should().Be(guid.ToString());
    }
}
```

---

## Integration Testing Patterns

### API Integration Tests

```csharp
public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace services for testing
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetRoot_ShouldReturnSuccessMessage()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("API service is running");
    }

    [Fact]
    public async Task GetMe_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
```

### Aspire Integration Tests

```csharp
public class AspireIntegrationTests
{
    [Fact]
    public async Task AppHost_ShouldStartSuccessfully()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.MicroCommerce_AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        var response = await httpClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

---

## Mocking Patterns

### Mocking Domain Event Dispatcher

```csharp
public class OrderServiceTests
{
    private readonly IDomainEventDispatcher _dispatcher;
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _dispatcher = Substitute.For<IDomainEventDispatcher>();
        _sut = new OrderService(_dispatcher);
    }

    [Fact]
    public async Task CreateOrder_ShouldDispatchOrderCreatedEvent()
    {
        // Act
        await _sut.CreateOrderAsync(new CreateOrderCommand());

        // Assert
        await _dispatcher.Received(1)
            .DispatchAsync<OrderCreatedEvent>(Arg.Any<OrderCreatedEvent>());
    }
}
```

### Mocking MediatR

```csharp
public class HandlerTests
{
    private readonly IMediator _mediator;

    public HandlerTests()
    {
        _mediator = Substitute.For<IMediator>();
    }

    [Fact]
    public async Task Handler_ShouldPublishNotification()
    {
        // Arrange
        var dispatcher = new MediatorDomainEventDispatcher(_mediator);
        var domainEvent = new TestDomainEvent();

        // Act
        await dispatcher.DispatchAsync<TestDomainEvent>(domainEvent);

        // Assert
        await _mediator.Received(1).Publish(domainEvent);
    }
}
```

---

## Frontend Testing

### Recommended Setup

No test configuration currently exists. Recommended packages:

```json
{
  "devDependencies": {
    "@testing-library/react": "^14.0.0",
    "@testing-library/jest-dom": "^6.0.0",
    "vitest": "^1.0.0",
    "@vitejs/plugin-react": "^4.0.0",
    "jsdom": "^23.0.0"
  }
}
```

### Component Test Example

```tsx
// components/auth/auth-button.test.tsx
import { render, screen } from "@testing-library/react";
import { SessionProvider } from "next-auth/react";
import { AuthButton } from "./auth-button";

describe("AuthButton", () => {
  it("shows sign in button when not authenticated", () => {
    render(
      <SessionProvider session={null}>
        <AuthButton />
      </SessionProvider>
    );

    expect(screen.getByText("Sign in with Keycloak")).toBeInTheDocument();
  });

  it("shows user email when authenticated", () => {
    const session = {
      user: { email: "test@example.com" },
      expires: "2024-01-01",
    };

    render(
      <SessionProvider session={session}>
        <AuthButton />
      </SessionProvider>
    );

    expect(screen.getByText("test@example.com")).toBeInTheDocument();
  });
});
```

---

## Test Organization

### Naming Conventions

| Type | Pattern | Example |
|------|---------|---------|
| Test class | `{ClassUnderTest}Tests` | `ValueObjectTests` |
| Test method | `{Method}_{Scenario}_{Expected}` | `Equals_WithSameComponents_ShouldReturnTrue` |
| Test file | `{ClassUnderTest}.Tests.cs` | `ValueObject.Tests.cs` |

### Test Categories

Use traits for categorization:

```csharp
[Trait("Category", "Unit")]
public class UnitTests { }

[Trait("Category", "Integration")]
public class IntegrationTests { }

[Trait("Category", "E2E")]
public class EndToEndTests { }
```

Run specific categories:
```bash
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
```

---

## Coverage

### Collecting Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Coverage Report Generation

```bash
# Install report generator
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report
reportgenerator \
  -reports:"**/coverage.cobertura.xml" \
  -targetdir:"coveragereport" \
  -reporttypes:Html
```

### CI Coverage Integration

Add to workflow:
```yaml
- name: Run Tests with Coverage
  run: dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

- name: Upload Coverage
  uses: codecov/codecov-action@v3
  with:
    directory: ./coverage
```

---

## Health Check Testing

The application exposes health endpoints:

| Endpoint | Purpose | Tags |
|----------|---------|------|
| `/health` | Readiness check | All checks |
| `/alive` | Liveness check | `live` tag only |

Test health checks:
```csharp
[Fact]
public async Task HealthEndpoint_ShouldReturnHealthy()
{
    var response = await _client.GetAsync("/health");
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

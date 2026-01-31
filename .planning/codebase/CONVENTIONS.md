# Code Conventions

## Overview

This is a microservices e-commerce application built with:
- **Backend**: .NET 10 with ASP.NET Core, Aspire orchestration
- **Frontend**: Next.js 16, React 19, TypeScript
- **Architecture**: Domain-Driven Design (DDD) patterns

---

## Project Structure

```
code/
├── MicroCommerce.AppHost/        # Aspire orchestrator
├── MicroCommerce.ApiService/     # API service (ASP.NET Core)
├── MicroCommerce.ServiceDefaults/ # Shared Aspire extensions
├── MicroCommerce.Web/            # Next.js frontend
└── BuildingBlocks/
    └── BuildingBlocks.Common/    # Shared DDD building blocks
```

---

## C# Conventions

### File & Namespace Organization

- **File-scoped namespaces**: Always use file-scoped namespace declarations
- **One type per file**: Each class/interface/record in its own file
- **Namespace matches folder**: `dotnet_style_namespace_match_folder = true`

```csharp
// ✅ Correct
namespace MicroCommerce.BuildingBlocks.Common;

public class MyClass { }

// ❌ Avoid
namespace MicroCommerce.BuildingBlocks.Common
{
    public class MyClass { }
}
```

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Classes | PascalCase | `BaseAggregateRoot` |
| Interfaces | I + PascalCase | `IAggregateRoot`, `IDomainEvent` |
| Methods | PascalCase | `ClearDomainEvents()` |
| Properties | PascalCase | `DomainEvents` |
| Private fields | _camelCase | `_domainEvents` |
| Parameters | camelCase | `domainEvent` |
| Constants | PascalCase | `HealthEndpointPath` |
| Generic types | T + Descriptive | `TId`, `TBuilder` |

### Type Preferences

- **Explicit types over var**: `csharp_style_var_* = false`
- **Language keywords over BCL types**: Use `int` not `Int32`
- **Collection expressions**: Use `[]` syntax for collections

```csharp
// ✅ Correct
private readonly List<DomainEvent> _domainEvents = [];
string name = "example";

// ❌ Avoid
private readonly List<DomainEvent> _domainEvents = new List<DomainEvent>();
var name = "example";
```

### Code Style

- **Braces required**: Always use braces for control flow
- **Expression-bodied members**: Use for accessors, properties, lambdas
- **Primary constructors**: Preferred for dependency injection

```csharp
// ✅ Primary constructor pattern
public class MediatorDomainEventDispatcher(IMediator mediator) : IDomainEventDispatcher
{
    public Task DispatchAsync<T>(IDomainEvent domainEvent) where T : IDomainEvent
    {
        return mediator.Publish(domainEvent);
    }
}

// ✅ Expression-bodied property
public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
```

### Modifier Order

```csharp
public, private, protected, internal, file, static, extern, new, virtual,
abstract, sealed, override, readonly, unsafe, required, volatile, async
```

---

## DDD Building Blocks

### Aggregate Root Pattern

```csharp
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

### Strongly Typed IDs

```csharp
[DebuggerStepThrough]
public abstract record StronglyTypedId<T>(T Value)
{
    public override string ToString() => Value?.ToString() ?? string.Empty;
}

// Usage
public record EventId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static EventId New() => new(Guid.NewGuid());
    public static EventId From(Guid value) => new(value);
}
```

### Value Objects

```csharp
[Serializable]
public abstract class ValueObject : IComparable, IComparable<ValueObject>
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    // Equality implemented via GetEqualityComponents()
}

// For simple cases, prefer: readonly record struct
```

### Domain Events

```csharp
// Base event
public abstract record DomainEvent : IDomainEvent
{
    public EventId Id { get; }
    public DateTimeOffset DateOccurred { get; protected set; } = DateTimeOffset.UtcNow;
}

// Handler interface
public interface IDomainEventHandler<in TDomainEvent> : INotificationHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    new Task Handle(TDomainEvent @event, CancellationToken cancellationToken);
}
```

---

## ASP.NET Core Patterns

### Minimal API Structure

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire integrations
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

// Middleware pipeline
app.UseExceptionHandler();
app.UseCors();

// Endpoints
app.MapGet("/", () => "API service is running.");
app.MapGet("/me", (ClaimsPrincipal user) => { /* ... */ })
    .WithName("GetCurrentUser")
    .RequireAuthorization();

app.MapDefaultEndpoints();
app.Run();
```

### Service Defaults Extension Pattern

```csharp
public static class Extensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();
        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });
        return builder;
    }
}
```

### Dependency Injection Extensions

```csharp
public static class DependencyInjection
{
    public static void AddMediatorDomainEventDispatcher(this IServiceCollection services)
    {
        services.AddTransient<IDomainEventDispatcher, MediatorDomainEventDispatcher>();
    }
}
```

---

## TypeScript/React Conventions

### File Organization

```
src/
├── app/                  # Next.js App Router pages
│   ├── api/             # API routes
│   ├── layout.tsx       # Root layout
│   └── page.tsx         # Home page
├── components/
│   ├── auth/            # Feature-grouped components
│   │   └── auth-button.tsx
│   └── providers/       # Context providers
│       └── session-provider.tsx
├── lib/                 # Utilities & configuration
│   └── config.ts
└── types/               # TypeScript declarations
    └── next-auth.d.ts
```

### Component Patterns

**Client Components** - Use `"use client"` directive:
```tsx
"use client";

import { useState } from "react";

interface Props {
  children: ReactNode;
}

export function SessionProvider({ children }: Props) {
  return <NextAuthSessionProvider>{children}</NextAuthSessionProvider>;
}
```

**Server Components** - Default in App Router:
```tsx
// No "use client" = Server Component
export default function Home() {
  return <main>...</main>;
}
```

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Components | PascalCase | `AuthButton`, `ApiTest` |
| Files | kebab-case | `auth-button.tsx`, `api-test.tsx` |
| Interfaces | PascalCase | `UserInfo`, `WeatherForecast` |
| Hooks | camelCase with use prefix | `useSession` |
| Utilities | camelCase | `getApiBaseUrl` |

### TypeScript Configuration

- **Strict mode**: Enabled
- **Path aliases**: `@/*` maps to `./src/*`
- **Module resolution**: Bundler

### Biome Linting

Configuration (biome.json):
- **Indent**: 2 spaces
- **Rules**: Recommended + Next.js + React domains
- **Import organization**: Automatic

---

## Error Handling

### Backend

- **ProblemDetails**: Standard RFC 7807 error responses
- **Guard clauses**: Using Ardalis.GuardClauses for input validation
- **Nullable reference types**: Enabled globally

```csharp
public TId Id { get; init; } = id ?? throw new ArgumentNullException(nameof(id));
```

### Frontend

- **Error boundaries**: Use React error boundaries
- **State-based errors**: Track error state in components

```tsx
const [error, setError] = useState<string | null>(null);

try {
  const data = await fetchWithAuth("/me");
} catch (err) {
  setError(err instanceof Error ? err.message : "Failed to fetch");
}
```

---

## Configuration

### Build Properties (Directory.Build.props)

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

### EditorConfig Highlights

- **Indentation**: 4 spaces for C#, 2 spaces for XML/csproj
- **Line endings**: LF
- **Trailing whitespace**: Trimmed
- **Final newline**: Not inserted

---

## Package Management

### Core Dependencies

**Backend (NuGet)**:
- `MediatR` - CQRS/Mediator pattern
- `Ardalis.GuardClauses` - Input validation
- `Aspire.*` - Service orchestration & telemetry
- `OpenTelemetry.*` - Observability

**Frontend (npm)**:
- `next` / `react` - UI framework
- `next-auth` - Authentication
- `tailwindcss` - Styling
- `@biomejs/biome` - Linting & formatting

---

## Aspire Orchestration

### AppHost Pattern

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var keycloak = builder
    .AddKeycloak("keycloak", 8101)
    .WithDataVolume()
    .WithRealmImport("./Realms")
    .WithLifetime(ContainerLifetime.Persistent);

var apiService = builder.AddProject<Projects.MicroCommerce_ApiService>("apiservice")
    .WithReference(keycloak)
    .WithHttpHealthCheck("/health");

builder.AddJavaScriptApp("frontend", "../MicroCommerce.Web")
    .WithReference(apiService)
    .WithReference(keycloak)
    .WithHttpEndpoint(port: 3000, env: "PORT");

builder.Build().Run();
```

### Health Endpoints

- `/health` - Full health check
- `/alive` - Liveness probe (tagged with "live")

---

## Authentication

### Backend (Keycloak JWT)

```csharp
builder.Services.AddAuthentication()
    .AddKeycloakJwtBearer(
        serviceName: "keycloak",
        realm: "micro-commerce",
        options => { /* configuration */ });
```

### Frontend (NextAuth.js)

```typescript
export const { handlers, signIn, signOut, auth } = NextAuth({
  providers: [Keycloak({ /* ... */ })],
  callbacks: {
    async jwt({ token, account }) {
      if (account) {
        token.accessToken = account.access_token;
      }
      return token;
    },
    async session({ session, token }) {
      session.accessToken = token.accessToken as string;
      return session;
    },
  },
});
```

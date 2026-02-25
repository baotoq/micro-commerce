# Phase 9: API Gateway - Research

**Researched:** 2026-02-12
**Domain:** YARP (Yet Another Reverse Proxy) + .NET Aspire Integration
**Confidence:** HIGH

## Summary

YARP is Microsoft's official reverse proxy toolkit for .NET, built on ASP.NET Core and actively maintained. It provides a highly customizable, production-ready solution for building API gateways with built-in support for routing, authentication/authorization, rate limiting, and request transforms. YARP integrates seamlessly with .NET Aspire through the `Aspire.Hosting.Yarp` package, enabling service discovery, containerized deployment, and standard observability patterns.

For this phase, YARP offers all required capabilities: JWT validation via ASP.NET Core authentication middleware, rate limiting through ASP.NET Core 7+ rate limiting middleware, automatic X-Forwarded-* headers, CORS policy configuration, and full integration with Aspire's ServiceDefaults for tracing, metrics, and health checks.

**Primary recommendation:** Create a separate `MicroCommerce.Gateway` project using YARP with code-based configuration (Aspire 9.4+ standard), leverage ASP.NET Core middleware for auth/rate-limiting/CORS, and use Aspire ServiceDefaults for observability with no custom telemetry code needed.

## Standard Stack

### Core

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Yarp.ReverseProxy | 2.2+ | Core reverse proxy | Official Microsoft toolkit, production-proven (Azure AD, Dynamics 365 use it) |
| Aspire.Hosting.Yarp | Latest (13.1+) | Aspire integration | Standard Aspire pattern for containerized YARP deployment |
| Microsoft.AspNetCore.Authentication.JwtBearer | .NET 10 | JWT validation | Built-in ASP.NET Core authentication, integrates with YARP authorization policies |
| Microsoft.AspNetCore.RateLimiting | .NET 7+ | Rate limiting | Built-in middleware, integrated with YARP via RouteConfig.RateLimiterPolicy |

### Supporting

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| MicroCommerce.ServiceDefaults | Project reference | Cross-cutting concerns | Required - provides telemetry, health checks, service discovery |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| YARP | Ocelot | Ocelot is community-maintained, less active development; YARP is Microsoft-backed and production-proven at scale |
| YARP | Envoy | Envoy requires sidecar pattern, more complex setup; YARP is native .NET with simpler deployment for .NET stacks |
| Code-based config | JSON config files | JSON was deprecated in Aspire 9.4; code provides IntelliSense, type safety, and better deployment portability |

**Installation:**
```bash
# In AppHost project
dotnet add package Aspire.Hosting.Yarp

# In Gateway project (if standalone, not containerized)
dotnet add package Yarp.ReverseProxy
```

## Architecture Patterns

### Recommended Project Structure
```
src/
├── MicroCommerce.Gateway/          # YARP gateway project (if standalone)
│   ├── Program.cs                  # Route config, middleware pipeline
│   └── appsettings.json            # Rate limit policies, JWT config
└── MicroCommerce.AppHost/
    └── AppHost.cs                  # Aspire orchestration with YARP resource
```

### Pattern 1: Aspire-Hosted YARP Gateway (Recommended)

**What:** YARP runs as a containerized Aspire resource configured via code-based fluent API
**When to use:** Default approach for Aspire projects - provides service discovery, deployment consistency, dashboard visibility
**Example:**
```csharp
// Source: https://aspire.dev/integrations/reverse-proxies/yarp/
var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.MicroCommerce_ApiService>("apiservice")
    .WithReference(keycloak)
    .WithReference(appDb);

var gateway = builder.AddYarp("gateway")
    .WithConfiguration(yarp => {
        // Catch-all route to ApiService
        yarp.AddRoute("/api/{**catch-all}", apiService);
    })
    .WithHostPort(8080); // Frontend calls gateway:8080

builder.AddJavaScriptApp("frontend", "../MicroCommerce.Web")
    .WithReference(gateway)  // Changed from apiservice
    .WithReference(keycloak);
```

### Pattern 2: JWT Validation at Gateway

**What:** Use ASP.NET Core authentication middleware with YARP authorization policies
**When to use:** All routes requiring authentication - gateway validates before proxying
**Example:**
```csharp
// Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/authn-authz
// In Gateway Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.Authority = "https://keycloak:8101/realms/micro-commerce";
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateAudience = false, // Accept nextjs-app client tokens
            ValidateIssuer = true,
            ValidateLifetime = true
        };
        if (builder.Environment.IsDevelopment()) {
            options.RequireHttpsMetadata = false;
        }
    });

builder.Services.AddAuthorization(options => {
    options.AddPolicy("authenticated", policy => policy.RequireAuthenticatedUser());
});

// Route configuration (code-based in Aspire 9.4+)
var routes = new[] {
    new RouteConfig {
        RouteId = "api-authenticated",
        ClusterId = "apiservice",
        AuthorizationPolicy = "authenticated", // JWT required
        Match = new RouteMatch {
            Path = "/api/ordering/{**catch-all}"
        }
    },
    new RouteConfig {
        RouteId = "api-public",
        ClusterId = "apiservice",
        AuthorizationPolicy = "anonymous", // No auth needed
        Match = new RouteMatch {
            Path = "/api/catalog/{**catch-all}"
        }
    }
};

app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();
```

### Pattern 3: Sliding Window Rate Limiting

**What:** ASP.NET Core rate limiting middleware with partitioned limiters (different limits for authenticated vs anonymous)
**When to use:** Protect backend from abuse, enforce fair usage policies
**Example:**
```csharp
// Source: https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit
builder.Services.AddRateLimiter(options => {
    options.OnRejected = async (context, token) => {
        context.HttpContext.Response.StatusCode = 429;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)) {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString();
        }
        await context.HttpContext.Response.WriteAsync("Too many requests", token);
    };

    // Partitioned by user identity - authenticated users get higher limits
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext => {
        var userName = httpContext.User.Identity?.Name;

        if (!string.IsNullOrEmpty(userName)) {
            // Authenticated: 100 req/min with sliding window
            return RateLimitPartition.GetSlidingWindowLimiter(userName, _ =>
                new SlidingWindowRateLimiterOptions {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 6, // 10-second segments
                    QueueLimit = 0
                });
        } else {
            // Anonymous: 30 req/min
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetSlidingWindowLimiter(ipAddress, _ =>
                new SlidingWindowRateLimiterOptions {
                    PermitLimit = 30,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 6,
                    QueueLimit = 0
                });
        }
    });
});

app.UseRateLimiter();
```

### Pattern 4: Request Transforms for Proxy Headers

**What:** YARP adds X-Forwarded-* headers by default; configure X-Request-ID via transforms
**When to use:** Always - provides backend services with client IP, protocol, and request tracing
**Example:**
```csharp
// Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/transforms-request
// X-Forwarded-* headers added by default (For, Proto, Host)
// Add custom X-Request-ID for distributed tracing

services.AddReverseProxy()
    .LoadFromConfig(configuration.GetSection("ReverseProxy"))
    .AddTransforms(context => {
        // Add unique request ID if not present
        context.AddRequestTransform(transformContext => {
            if (!transformContext.HttpContext.Request.Headers.ContainsKey("X-Request-ID")) {
                transformContext.ProxyRequest.Headers.Add(
                    "X-Request-ID",
                    Activity.Current?.Id ?? Guid.NewGuid().ToString()
                );
            }
            return ValueTask.CompletedTask;
        });
    });
```

### Pattern 5: CORS at Gateway Level

**What:** Centralized CORS policy configured in gateway, removed from ApiService
**When to use:** Required for browser-based frontends, simplifies per-service configuration
**Example:**
```csharp
// Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/cors
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Apply CORS before rate limiting and auth
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();
```

### Anti-Patterns to Avoid

- **Hand-rolling authentication:** YARP integrates with ASP.NET Core auth middleware - use `AuthorizationPolicy` on routes instead of custom validation logic
- **JSON-based configuration in Aspire:** Deprecated in Aspire 9.4 - use code-based `.WithConfiguration()` fluent API instead
- **Forgetting middleware order:** CORS must come before UseRateLimiter, which must come before UseAuthentication/UseAuthorization
- **Missing X-Forwarded headers:** YARP adds them by default, but ensure backend services trust the gateway's forwarded headers
- **Global policies without partitioning:** Use `PartitionedRateLimiter` to prevent single user/IP from consuming entire rate limit

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| JWT validation | Custom token parsing/validation | ASP.NET Core JwtBearer middleware | Handles expiration, issuer validation, signature verification, clock skew - complex edge cases |
| Rate limiting | Custom request counting logic | ASP.NET Core RateLimiting middleware | Supports sliding window, token bucket, concurrent limiters with queuing and partition keys |
| X-Forwarded headers | Manual header injection | YARP default transforms | Automatically adds For, Proto, Host, Prefix with correct handling of existing headers |
| Service discovery | Hardcoded endpoints | Aspire service references | Dynamic endpoint resolution, health-aware routing, deployment portability |
| Request logging | Custom logging interceptors | Aspire ServiceDefaults + YARP telemetry | Structured logging with correlation IDs, integrated with OpenTelemetry, appears in Aspire dashboard |

**Key insight:** YARP's strength is integration with existing ASP.NET Core middleware ecosystem. Don't treat it as a standalone proxy - compose it with standard .NET patterns (auth, CORS, rate limiting, logging) rather than building custom solutions.

## Common Pitfalls

### Pitfall 1: Middleware Ordering Mistakes
**What goes wrong:** CORS errors, auth bypassed, rate limiting applied before authentication partitioning works
**Why it happens:** ASP.NET Core middleware runs in registration order; wrong order breaks functionality
**How to avoid:** Follow strict order: CORS → RateLimiter → Authentication → Authorization → ReverseProxy
**Warning signs:**
- 401 responses even with valid tokens (auth after proxy)
- CORS preflight failures (CORS after other middleware)
- All users hit same rate limit (rate limiter before auth)

### Pitfall 2: Aspire Configuration-File Pattern (Deprecated)
**What goes wrong:** Using `.WithConfigFile()` fails with IntelliSense warnings or runtime errors
**Why it happens:** Aspire 9.4+ removed JSON config support for better type safety
**How to avoid:** Use code-based `.WithConfiguration(yarp => yarp.AddRoute(...))` fluent API only
**Warning signs:**
- `WithConfigFile` method not found
- Documentation showing old pattern
- Deployment issues due to missing config files

### Pitfall 3: Frontend Still Calling ApiService Directly
**What goes wrong:** Gateway unused, no auth/rate-limiting enforcement, CORS remains in ApiService
**Why it happens:** Forgetting to update frontend environment variable from apiservice to gateway
**How to avoid:**
- Change `WithReference(apiservice)` to `WithReference(gateway)` in AppHost
- Remove CORS config from ApiService after verifying gateway handles it
- Test that ApiService returns CORS error when called directly
**Warning signs:**
- Gateway shows zero traffic in Aspire dashboard
- Auth validation bypassed
- Rate limits not enforced

### Pitfall 4: Trusting Forwarded Headers from Untrusted Sources
**What goes wrong:** IP spoofing, security vulnerabilities if backend blindly trusts X-Forwarded-For
**Why it happens:** Backend services accept forwarded headers without validating they came from gateway
**How to avoid:**
- Configure backend to only trust forwarded headers from gateway IP
- Use `ForwardedHeadersOptions.KnownProxies` in backend services
- In development, accept from localhost/container network only
**Warning signs:**
- Rate limiting bypassable by spoofing X-Forwarded-For
- Audit logs show incorrect client IPs

### Pitfall 5: No Health Check Endpoint in Gateway
**What goes wrong:** Aspire shows gateway as unhealthy, deployment orchestration fails
**Why it happens:** Forgetting to add `MapDefaultEndpoints()` for Aspire ServiceDefaults
**How to avoid:** Add `builder.AddServiceDefaults()` and `app.MapDefaultEndpoints()` in gateway Program.cs
**Warning signs:**
- Gateway shows gray/unknown health in Aspire dashboard
- Aspire dependency graph missing gateway health status
- Deployment fails waiting for healthy gateway

### Pitfall 6: Rate Limiting Without Retry-After Header
**What goes wrong:** Clients retry immediately, amplifying load instead of backing off
**Why it happens:** Default ASP.NET Core rate limiter returns 429 but no Retry-After hint
**How to avoid:** Configure `OnRejected` callback to add `Retry-After` header using `MetadataName.RetryAfter`
**Warning signs:**
- Thundering herd of retries after rate limit hit
- Frontend shows generic error instead of "try again in X seconds"
- Rate limit becomes ineffective under load

## Code Examples

Verified patterns from official sources:

### Complete Gateway Program.cs

```csharp
// Source: Combined from Microsoft Learn YARP docs
using System.Diagnostics;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire ServiceDefaults (health checks, telemetry, service discovery)
builder.AddServiceDefaults();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateAudience = false,
            ValidateIssuer = true,
            ValidateLifetime = true
        };
        if (builder.Environment.IsDevelopment()) {
            options.RequireHttpsMetadata = false;
        }
    });

// Authorization policies
builder.Services.AddAuthorization(options => {
    options.AddPolicy("authenticated", policy => policy.RequireAuthenticatedUser());
});

// CORS policy
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ??
                ["http://localhost:3000"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Rate limiting with partitioning
builder.Services.AddRateLimiter(options => {
    options.OnRejected = async (context, token) => {
        context.HttpContext.Response.StatusCode = 429;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)) {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString();
        }
        await context.HttpContext.Response.WriteAsync("Too many requests", token);
    };

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext => {
        var userName = httpContext.User.Identity?.Name;

        return string.IsNullOrEmpty(userName)
            ? RateLimitPartition.GetSlidingWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new SlidingWindowRateLimiterOptions {
                    PermitLimit = 30,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 6,
                    QueueLimit = 0
                })
            : RateLimitPartition.GetSlidingWindowLimiter(userName, _ =>
                new SlidingWindowRateLimiterOptions {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 6,
                    QueueLimit = 0
                });
    });
});

// YARP reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(context => {
        // Add X-Request-ID for distributed tracing
        context.AddRequestTransform(transformContext => {
            if (!transformContext.HttpContext.Request.Headers.ContainsKey("X-Request-ID")) {
                transformContext.ProxyRequest.Headers.Add(
                    "X-Request-ID",
                    Activity.Current?.Id ?? Guid.NewGuid().ToString()
                );
            }
            return ValueTask.CompletedTask;
        });
    });

var app = builder.Build();

// Middleware pipeline - ORDER MATTERS
app.UseCors();                    // 1. CORS first for preflight
app.UseRateLimiter();             // 2. Rate limiting before auth
app.UseAuthentication();          // 3. Validate JWT tokens
app.UseAuthorization();           // 4. Enforce policies
app.MapReverseProxy();            // 5. Proxy to backend
app.MapDefaultEndpoints();        // 6. Aspire health checks

app.Run();
```

### Aspire AppHost Configuration

```csharp
// Source: https://aspire.dev/integrations/reverse-proxies/yarp/
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var appDb = postgres.AddDatabase("appdb");

var messaging = builder.AddAzureServiceBus("messaging").RunAsEmulator();
var storage = builder.AddAzureStorage("storage").RunAsEmulator();
var blobs = storage.AddBlobs("blobs");

var keycloak = builder.AddKeycloak("keycloak", 8101)
    .WithDataVolume()
    .WithRealmImport("./Realms")
    .WithLifetime(ContainerLifetime.Persistent);

var apiService = builder.AddProject<Projects.MicroCommerce_ApiService>("apiservice")
    .WithReference(keycloak)
    .WithReference(appDb)
    .WithReference(messaging)
    .WithReference(blobs)
    .WithHttpHealthCheck("/health");

// YARP Gateway - becomes entry point for frontend
var gateway = builder.AddYarp("gateway")
    .WithConfiguration(yarp => {
        // Proxy all /api/* requests to ApiService
        yarp.AddRoute("/api/{**catch-all}", apiService);
    })
    .WithHostPort(8080)
    .WithHttpHealthCheck("/health");

// Frontend now references gateway instead of apiservice
builder.AddJavaScriptApp("frontend", "../MicroCommerce.Web")
    .WithReference(gateway)    // Changed from apiService
    .WithReference(keycloak)
    .WithHttpEndpoint(port: 3000, env: "PORT");

builder.Build().Run();
```

### Determining Public vs Protected Routes

```csharp
// Based on existing ApiService endpoint configuration patterns
// Public routes (no auth required):
// - GET /api/catalog/* (browse products, categories)
// - GET /api/cart/* (guest cart access via cookie)
// - POST /api/cart/items (guest can add to cart)

// Protected routes (authentication required):
// - POST /api/ordering/checkout (requires user identity)
// - GET /api/ordering/orders/my (user's order history)
// - GET /api/ordering/orders (admin - all orders)
// - POST /api/ordering/orders/{id}/pay (payment simulation)
// - GET /api/messaging/dlq/* (admin dead letter queue)
// - All Catalog write operations (POST/PUT/DELETE categories, products)

// YARP route configuration example:
var routes = new[] {
    new RouteConfig {
        RouteId = "catalog-read",
        ClusterId = "apiservice",
        AuthorizationPolicy = "anonymous",
        Match = new RouteMatch {
            Path = "/api/catalog/{**catch-all}",
            Methods = new[] { "GET" }
        }
    },
    new RouteConfig {
        RouteId = "catalog-write",
        ClusterId = "apiservice",
        AuthorizationPolicy = "authenticated",
        Match = new RouteMatch {
            Path = "/api/catalog/{**catch-all}",
            Methods = new[] { "POST", "PUT", "DELETE" }
        }
    },
    new RouteConfig {
        RouteId = "cart-guest",
        ClusterId = "apiservice",
        AuthorizationPolicy = "anonymous",
        Match = new RouteMatch { Path = "/api/cart/{**catch-all}" }
    },
    new RouteConfig {
        RouteId = "ordering",
        ClusterId = "apiservice",
        AuthorizationPolicy = "authenticated",
        Match = new RouteMatch { Path = "/api/ordering/{**catch-all}" }
    },
    new RouteConfig {
        RouteId = "messaging",
        ClusterId = "apiservice",
        AuthorizationPolicy = "authenticated",
        Match = new RouteMatch { Path = "/api/messaging/{**catch-all}" }
    }
};
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| JSON config files for YARP in Aspire | Code-based fluent API `.WithConfiguration()` | Aspire 9.4 (Nov 2024) | Better IntelliSense, type safety, no config file deployment issues |
| Manual route configuration in appsettings.json | Service discovery via Aspire references | Aspire GA (2024) | Dynamic endpoint resolution, no hardcoded URLs |
| Custom rate limiting logic | ASP.NET Core 7+ built-in middleware | .NET 7 (Nov 2022) | Standard partitioning, sliding window, token bucket algorithms |
| Separate authentication per service | Gateway-level validation | YARP + ASP.NET Core pattern | Single auth point, reduced backend load |
| IIS or standalone Kestrel proxy | YARP on Kestrel | YARP GA 2021, production use 2022 | Native .NET solution, used in Azure AD, Dynamics 365 |

**Deprecated/outdated:**
- `.WithConfigFile()` in Aspire YARP integration: Removed in Aspire 9.4, use code-based configuration
- Global rate limiters without partitioning: Vulnerable to single-user DoS, always partition by user/IP
- Ocelot for .NET reverse proxy: Less active development, YARP is Microsoft-backed standard

## Open Questions

1. **Does Aspire's containerized YARP support custom transforms?**
   - What we know: Aspire uses `mcr.microsoft.com/dotnet/nightly/yarp` container image
   - What's unclear: Whether custom C# transforms (like X-Request-ID injection) work in containerized mode
   - Recommendation: Test with simple transform; if blocked, create standalone Gateway project instead of containerized resource

2. **How to handle backend service health in gateway routing?**
   - What we know: Aspire tracks health per service, YARP supports health checks on clusters
   - What's unclear: Whether Aspire YARP integration automatically fails over to healthy instances
   - Recommendation: Initially proxy to single ApiService; investigate YARP health checks if scaling to multiple instances later

3. **Should JWT validation happen at both gateway and backend?**
   - What we know: Gateway can validate and reject invalid tokens; backend still has auth middleware
   - What's unclear: Best practice for defense-in-depth vs performance
   - Recommendation: Validate at gateway for rejection, keep backend middleware for claim extraction (BuyerIdentity pattern still needs user info)

## Sources

### Primary (HIGH confidence)
- [Microsoft Learn: Overview of YARP](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/yarp-overview?view=aspnetcore-10.0) - Core concepts, features
- [Microsoft Learn: YARP Authentication and Authorization](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/authn-authz?view=aspnetcore-10.0) - JWT integration, policy config
- [Microsoft Learn: YARP Rate Limiting](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/rate-limiting?view=aspnetcore-10.0) - Route-level policy configuration
- [Microsoft Learn: YARP CORS](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/cors?view=aspnetcore-10.0) - CORS policy patterns
- [Microsoft Learn: ASP.NET Core Rate Limiting Middleware](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-10.0) - Sliding window algorithm, partitioning strategies
- [Aspire.dev: YARP Integration](https://aspire.dev/integrations/reverse-proxies/yarp/) - Aspire-specific setup, code-based config
- [Microsoft Learn: Aspire Service Defaults](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/service-defaults) - ServiceDefaults telemetry, health checks

### Secondary (MEDIUM confidence)
- [Tim Deschryver: Using YARP as BFF within .NET Aspire](https://timdeschryver.dev/blog/integrating-yarp-within-dotnet-aspire) - Real-world Aspire YARP implementation
- [Milan Jovanovic: Implementing API Gateway Authentication with YARP](https://www.milanjovanovic.tech/blog/implementing-api-gateway-authentication-with-yarp) - JWT validation patterns
- [Milan Jovanovic: Advanced Rate Limiting Use Cases in .NET](https://www.milanjovanovic.tech/blog/advanced-rate-limiting-use-cases-in-dotnet) - Partitioning strategies
- [Marek Sirkovský: YARP — Reverse Proxying the .NET Way (Jan 2026)](https://mareks-082.medium.com/yarp-reverse-proxying-the-net-way-2972be2d4701) - Recent best practices

### Tertiary (LOW confidence)
- [GitHub YARP Discussions](https://github.com/dotnet/yarp/discussions) - Community troubleshooting, patterns
- [CodeNx: Get started with YARP .NET Reverse Proxy Solution](https://medium.com/codenx/yarp-net-core-reverse-proxy-9b20ceb76995) - Setup guide

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All libraries official Microsoft, production-proven, well-documented
- Architecture: HIGH - Patterns verified in official Microsoft Learn docs, Aspire samples
- Pitfalls: MEDIUM - Derived from community discussions, GitHub issues, and Microsoft Learn warnings

**Research date:** 2026-02-12
**Valid until:** 90 days (June 2026) - YARP and Aspire are stable, configuration patterns unlikely to change rapidly

# Service-to-Service Communication in .NET Aspire

This guide covers service discovery, inter-service HTTP communication, and resilience patterns in .NET Aspire applications.

## Service Discovery

### How It Works

When services are added to AppHost, Aspire automatically:
1. Assigns internal names (e.g., "api", "web")
2. Registers services in a service discovery mechanism
3. Injects connection information as environment variables
4. Configures service discovery clients

Services communicate using short service names instead of hardcoded URLs.

### Configuration in AppHost

**Define services with unique names:**
```csharp
var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.MyApi>("api");
var web = builder.AddProject<Projects.MyWeb>("web")
    .WithReference(api);  // web depends on api

builder.Build().Run();
```

The name parameter ("api", "web") becomes the service discovery identifier.

## HTTP Client Communication

### Basic Service-to-Service Calls

**Create HttpClient with service name:**
```csharp
// Program.cs in the service that calls another service
builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri("http://api");
});
```

**Use in a controller or service:**
```csharp
public class OrderService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public OrderService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ProductDto> GetProductAsync(int productId)
    {
        var httpClient = _httpClientFactory.CreateClient("api");
        var response = await httpClient.GetAsync($"/products/{productId}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ProductDto>(json);
    }
}
```

### Typed HttpClients

For better type safety and testability, use typed clients:

**Define client interface:**
```csharp
public interface IProductApiClient
{
    Task<ProductDto> GetProductAsync(int productId);
    Task<IEnumerable<ProductDto>> GetProductsAsync();
}

public class ProductApiClient : IProductApiClient
{
    private readonly HttpClient _httpClient;

    public ProductApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductDto> GetProductAsync(int productId)
    {
        var response = await _httpClient.GetAsync($"/products/{productId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsAsync<ProductDto>();
    }

    public async Task<IEnumerable<ProductDto>> GetProductsAsync()
    {
        var response = await _httpClient.GetAsync("/products");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsAsync<IEnumerable<ProductDto>>();
    }
}
```

**Register typed client:**
```csharp
builder.Services.AddHttpClient<IProductApiClient, ProductApiClient>(client =>
{
    client.BaseAddress = new Uri("http://api");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
```

**Inject and use:**
```csharp
public class OrderController
{
    private readonly IProductApiClient _productApiClient;

    public OrderController(IProductApiClient productApiClient)
    {
        _productApiClient = productApiClient;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var product = await _productApiClient.GetProductAsync(id);
        return Ok(product);
    }
}
```

## Service Discovery with Resilience

### Add Resilience Policies

ServiceDefaults automatically includes resilience policies via `Microsoft.Extensions.Http.Resilience`.

**Services with ServiceDefaults already include:**
- Retry policy (exponential backoff)
- Circuit breaker
- Timeout policy
- Hedging for safe operations

**Custom resilience configuration:**
```csharp
builder.Services.AddHttpClient<IProductApiClient, ProductApiClient>(client =>
{
    client.BaseAddress = new Uri("http://api");
})
.AddStandardResilienceHandler(options =>
{
    // Customize resilience policies
    options.Retry.MaxRetryAttempts = 3;
    options.CircuitBreaker.FailureRatio = 0.5;
    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
});
```

### Health Checks for Service Communication

Verify dependent services are healthy before making calls:

**Check service health:**
```csharp
public class ServiceHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ServiceHealthCheck(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient("api");
        
        try
        {
            var response = await httpClient.GetAsync("/health", cancellationToken);
            return response.IsSuccessStatusCode 
                ? HealthCheckResult.Healthy() 
                : HealthCheckResult.Unhealthy("Service returned unhealthy status");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Service unavailable: {ex.Message}");
        }
    }
}

// Register in Program.cs
builder.Services.AddHealthChecks()
    .AddCheck<ServiceHealthCheck>("api-health");
```

## GRPC Service Communication

For higher-performance service-to-service communication, use gRPC.

### Define gRPC Service

**Create proto file (Services/Orders.proto):**
```protobuf
syntax = "proto3";

package Orders;

service OrderService {
  rpc GetOrder(GetOrderRequest) returns (OrderResponse);
}

message GetOrderRequest {
  int32 id = 1;
}

message OrderResponse {
  int32 id = 1;
  string customer = 2;
  double total = 3;
}
```

**Generate code and implement service:**
```csharp
public class OrderGrpcService : OrderService.OrderServiceBase
{
    private readonly IOrderRepository _orderRepository;

    public OrderGrpcService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public override async Task<OrderResponse> GetOrder(GetOrderRequest request, ServerCallContext context)
    {
        var order = await _orderRepository.GetOrderAsync(request.Id);
        
        return new OrderResponse
        {
            Id = order.Id,
            Customer = order.Customer,
            Total = order.Total
        };
    }
}
```

**Register in Program.cs:**
```csharp
builder.Services.AddGrpc();

var app = builder.Build();
app.MapGrpcService<OrderGrpcService>();
```

### Call gRPC Service

**Create gRPC client:**
```csharp
public interface IOrderGrpcClient
{
    Task<OrderResponse> GetOrderAsync(int orderId);
}

public class OrderGrpcClient : IOrderGrpcClient
{
    private readonly OrderService.OrderServiceClient _client;

    public OrderGrpcClient(OrderService.OrderServiceClient client)
    {
        _client = client;
    }

    public async Task<OrderResponse> GetOrderAsync(int orderId)
    {
        var request = new GetOrderRequest { Id = orderId };
        return await _client.GetOrderAsync(request);
    }
}
```

**Register client:**
```csharp
builder.Services.AddGrpcClient<OrderService.OrderServiceClient>(client =>
{
    client.Address = new Uri("http://orders");  // Service name from AppHost
});

builder.Services.AddScoped<IOrderGrpcClient, OrderGrpcClient>();
```

### Add gRPC to AppHost

Add gRPC endpoint for service that exposes gRPC:
```csharp
var orderService = builder.AddProject<Projects.OrderService>("orders")
    .WithHttpEndpoint(scheme: "http", port: 5000, targetPort: 5000)
    .WithHttpsEndpoint(scheme: "https", port: 5001, targetPort: 5001);
```

## Service-to-Service Authentication

For secure service communication, use authentication between services.

### Service Identity with Certificates

**AppHost configuration:**
```csharp
var api = builder.AddProject<Projects.Api>("api")
    .WithHttpsEndpoint(scheme: "https");

var web = builder.AddProject<Projects.Web>("web")
    .WithReference(api);
```

Aspire automatically manages certificates for local development.

### API Key Authentication

**Web service calls API with key:**

Define API key in AppHost:
```csharp
var apiKey = builder.CreateResourceBuilder(new Parameter("api-key", secret: true))
    .WithDefault("default-dev-key");

var api = builder.AddProject<Projects.Api>("api")
    .WithEnvironment("API_KEY", apiKey);

var web = builder.AddProject<Projects.Web>("web")
    .WithReference(api);
```

**Retrieve and use in service:**
```csharp
builder.Services.AddHttpClient<IProductApiClient, ProductApiClient>(client =>
{
    client.BaseAddress = new Uri("http://api");
})
.ConfigureHttpClient((serviceProvider, client) =>
{
    var apiKey = serviceProvider.GetRequiredService<IConfiguration>()["API_KEY"];
    client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
});
```

### Service-to-Service Bearer Tokens

For services that need to authenticate with each other:

**AppHost passes token:**
```csharp
var token = builder.CreateResourceBuilder(new Parameter("service-token", secret: true))
    .WithDefault("eyJhbGc...");  // JWT token

var api = builder.AddProject<Projects.Api>("api")
    .WithEnvironment("SERVICE_TOKEN", token);
```

**HttpClient includes bearer token:**
```csharp
builder.Services.AddHttpClient<IProductApiClient, ProductApiClient>(client =>
{
    client.BaseAddress = new Uri("http://api");
})
.ConfigureHttpClient((serviceProvider, client) =>
{
    var token = serviceProvider.GetRequiredService<IConfiguration>()["SERVICE_TOKEN"];
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
});
```

## Error Handling in Service Communication

### Graceful Degradation

Handle service unavailability gracefully:

```csharp
public class ResilientProductService
{
    private readonly IProductApiClient _productApiClient;
    private readonly ILogger<ResilientProductService> _logger;

    public ResilientProductService(IProductApiClient productApiClient, ILogger<ResilientProductService> logger)
    {
        _productApiClient = productApiClient;
        _logger = logger;
    }

    public async Task<ProductDto> GetProductSafeAsync(int productId)
    {
        try
        {
            return await _productApiClient.GetProductAsync(productId);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning($"Product API unavailable: {ex.Message}");
            // Return cached data or default
            return new ProductDto { Id = productId, Name = "Unavailable", Price = 0 };
        }
    }
}
```

### Circuit Breaker Pattern

Already included in ServiceDefaults via resilience policies:

```csharp
builder.Services.AddHttpClient<IProductApiClient, ProductApiClient>(client =>
{
    client.BaseAddress = new Uri("http://api");
})
.AddStandardResilienceHandler(options =>
{
    options.CircuitBreaker.SamplingDurationInSeconds = 30;
    options.CircuitBreaker.FailureRatio = 0.5;
    options.CircuitBreaker.MinimumThroughput = 5;
});
```

### Retry with Exponential Backoff

Configured by default, customize as needed:

```csharp
.AddStandardResilienceHandler(options =>
{
    options.Retry.MaxRetryAttempts = 5;
    options.Retry.BackoffType = BackoffType.ExponentialBackoff;
    options.Retry.Delay = TimeSpan.FromSeconds(1);
    options.Retry.UseJitter = true;
});
```

## Observability in Service Communication

### Distributed Tracing

Aspire automatically traces HTTP calls between services via OpenTelemetry.

**View traces in dashboard:**
1. Run AppHost: `dotnet run --project AppHost`
2. Open dashboard: https://localhost:15001
3. Navigate to "Traces" tab
4. Select a request to see full trace across services

### Structured Logging

Log service calls with structured data:

```csharp
public class LoggingProductApiClient : IProductApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LoggingProductApiClient> _logger;

    public LoggingProductApiClient(HttpClient httpClient, ILogger<LoggingProductApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ProductDto> GetProductAsync(int productId)
    {
        using var activity = new System.Diagnostics.Activity("GetProduct").Start();
        activity?.SetTag("product.id", productId);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            var response = await _httpClient.GetAsync($"/products/{productId}");
            response.EnsureSuccessStatusCode();

            stopwatch.Stop();
            _logger.LogInformation("Retrieved product {ProductId} in {ElapsedMs}ms", productId, stopwatch.ElapsedMilliseconds);

            return await response.Content.ReadAsAsync<ProductDto>();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to retrieve product {ProductId} after {ElapsedMs}ms", productId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
```

### Metrics

Export metrics about service communication:

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddHttpClientInstrumentation();
        metrics.AddAspNetCoreInstrumentation();
    });
```

View in Prometheus dashboard (if configured in AppHost).

## Troubleshooting Service Communication

### Service Discovery Not Working

**Problem:** HttpClient gets connection refused

**Solution:**
1. Verify service name matches AppHost definition
2. Check service is running in dashboard
3. Confirm ServiceDefaults is referenced and AddServiceDefaults() called
4. Ensure MapDefaultEndpoints() called in ASP.NET services

### Timeout Issues

**Problem:** Service calls timeout frequently

**Solution:**
1. Increase timeout: `.AddStandardResilienceHandler(options => options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60))`
2. Check service performance in dashboard
3. Review logs for service errors
4. Consider reducing retry attempts if connection is too slow

### Authentication Failures

**Problem:** 401/403 errors on service-to-service calls

**Solution:**
1. Verify bearer token is set correctly in HttpClient
2. Check token expiration
3. Confirm API key environment variable is set in AppHost
4. Review service logs for authentication errors

### High Latency

**Problem:** Service communication is slow

**Solution:**
1. Use gRPC for performance-critical calls
2. Implement caching to reduce calls
3. Use connection pooling (already configured)
4. Monitor network latency in dashboard traces
5. Consider using service mesh (in Kubernetes) for advanced routing

## Common Patterns

### Fan-Out Pattern

One service calls multiple services in parallel:

```csharp
public class OrderService
{
    private readonly IProductApiClient _productClient;
    private readonly IInventoryApiClient _inventoryClient;
    private readonly IShippingApiClient _shippingClient;

    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request)
    {
        // Call multiple services in parallel
        var productTask = _productClient.GetProductAsync(request.ProductId);
        var inventoryTask = _inventoryClient.CheckInventoryAsync(request.ProductId, request.Quantity);
        var shippingTask = _shippingClient.CalculateShippingAsync(request.Address);

        await Task.WhenAll(productTask, inventoryTask, shippingTask);

        // Combine results
        var product = productTask.Result;
        var inventory = inventoryTask.Result;
        var shipping = shippingTask.Result;

        return new OrderDto { /* ... */ };
    }
}
```

### Cache-Aside Pattern

Cache service responses to reduce calls:

```csharp
public class CachedProductApiClient : IProductApiClient
{
    private readonly IProductApiClient _innerClient;
    private readonly IDistributedCache _cache;

    public async Task<ProductDto> GetProductAsync(int productId)
    {
        var cacheKey = $"product:{productId}";
        
        var cached = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<ProductDto>(cached);
        }

        var product = await _innerClient.GetProductAsync(productId);
        
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(product), 
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });

        return product;
    }
}
```

### Circuit Breaker with Fallback

Already handled by resilience policies, but explicit example:

```csharp
public class RobustProductApiClient
{
    private readonly IProductApiClient _client;
    private readonly IProductCache _cache;
    private readonly ILogger _logger;

    public async Task<ProductDto> GetProductAsync(int productId)
    {
        try
        {
            return await _client.GetProductAsync(productId);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
        {
            _logger.LogWarning("Product service unavailable, using cache");
            var cached = await _cache.GetAsync(productId);
            return cached ?? new ProductDto { Id = productId, Name = "Unavailable" };
        }
    }
}
```

## References

- [Service Discovery in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/service-discovery)
- [Resilience Patterns](https://learn.microsoft.com/en-us/dotnet/core/extensions/resilience)
- [gRPC Performance](https://learn.microsoft.com/en-us/aspnet/core/grpc/performance)
- [OpenTelemetry Instrumentation](https://learn.microsoft.com/en-us/dotnet/core/runtime-config/build-error-telemetry)

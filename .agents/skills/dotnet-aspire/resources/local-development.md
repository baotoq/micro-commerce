# Local Development with .NET Aspire

This guide covers using the Aspire dashboard, debugging, testing, and health checks for local development.

## Aspire Dashboard Overview

### Starting the Dashboard

**Launch AppHost to start dashboard:**
```bash
cd MyApp.AppHost
dotnet run
```

**Dashboard opens automatically at:** `https://localhost:15001`

If not automatic, check the AppHost console output for the URL.

### Dashboard Features

#### Resources View

**Shows all running services and resources:**
- Status indicator (green = healthy, red = failed, yellow = degraded)
- Resource type (Project, Container, Azure resource, etc.)
- Port information
- Endpoints for direct access

**Interact with resources:**
- Click service name to view logs
- Click endpoint URL to open service
- View resource configuration

#### Projects Section

Lists all services (AppHost projects):
- Current health status
- Replica count if running multiple instances
- Resource consumption (memory, CPU)
- Restart options

**Actions:**
- Click project to view detailed logs
- View environment variables
- Monitor real-time metrics

#### Containers Section

Shows infrastructure resources:
- PostgreSQL databases
- Redis caches
- RabbitMQ message brokers
- etc.

**Container info:**
- Container ID and image
- Port mappings
- Volume mounts
- Environment variables (visible for debugging)

#### Traces Tab

Distributed tracing across services:
- View request flow through microservices
- See latency at each step
- Identify bottlenecks
- Export traces for analysis

**Trace details:**
- HTTP request/response
- Database queries
- Service-to-service calls
- Execution time breakdown

#### Logs Tab

Real-time and historical logs from all services:
- Filter by service, level, or keyword
- Search for specific entries
- Export logs
- Tail live logs

**Log levels:**
- Trace (most verbose)
- Debug
- Information
- Warning
- Error
- Critical

#### Metrics Tab

Operational metrics:
- Request rate
- Error rate
- Latency percentiles (p50, p95, p99)
- Resource utilization
- Custom application metrics

## Local Development Workflow

### 1. Start AppHost

```bash
dotnet run --project MyApp.AppHost
```

**What starts:**
- All configured services (APIs, web apps, workers)
- All infrastructure resources (databases, caches, messaging)
- Developer dashboard
- OpenTelemetry collection

### 2. Monitor Dashboard

Open dashboard and verify:
- [ ] All services showing green "running" status
- [ ] All resources started successfully
- [ ] No error logs in initial startup

### 3. Test Service Communication

**From dashboard:**
1. Click on web service endpoint
2. Verify page loads
3. Click API service endpoint
4. Test API endpoint

**Or use curl:**
```bash
curl https://localhost:7001/api/products
```

### 4. Debug Issues via Dashboard

**Check logs:**
1. Click service in Projects section
2. Review logs for errors
3. Search for specific keywords
4. Check timestamps for correlation

**Check traces:**
1. Make a request through the UI
2. Navigate to Traces tab
3. Find your request
4. Expand to see service calls and timing

**Monitor metrics:**
1. Go to Metrics tab
2. Watch request rate and error rate
3. Check p95/p99 latencies
4. Look for resource pressure

## Debugging Services

### Enable Debug Logging

**In development configuration:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Debug",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

**Or via environment variable in AppHost:**
```csharp
var api = builder.AddProject<Projects.MyApi>("api")
    .WithEnvironment("Logging__LogLevel__Default", "Debug");
```

### Attach Debugger in Visual Studio

**Set breakpoint in service code**

**Start AppHost with debugger:**
```bash
dotnet run --project MyApp.AppHost
```

**Attach to service process:**
1. In Visual Studio: Debug → Attach to Process
2. Find the service process (MyApp.Api)
3. Click Attach
4. Trigger the code path in dashboard
5. Breakpoint activates

### Remote Debugging

**For services running in containers:**

**AppHost configuration for debugging:**
```csharp
var postgres = builder.AddPostgres("postgres")
    .WithEnvironment("POSTGRES_INITDB_ARGS", "--log-statement=all");
```

**View container logs via dashboard → Containers → PostgreSQL → Logs**

### Console Output

View service output directly:
```bash
# AppHost shows all service logs in console
dotnet run --project MyApp.AppHost
```

Look for:
- Service startup messages
- Configuration validation output
- Database connection info
- Port assignments

## Health Checks

### Enable Health Checks

ServiceDefaults automatically configures health checks.

**Health check endpoint:** `GET /health`

**In AppHost:**
```csharp
var api = builder.AddProject<Projects.MyApi>("api");

// Check service health
var health = api.GetProperty("health.status");  // In dashboard
```

### Custom Health Checks

**Define health check:**
```csharp
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDbConnection _connection;

    public DatabaseHealthCheck(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _connection.OpenAsync(cancellationToken);
            return HealthCheckResult.Healthy("Database connection successful");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Database connection failed: {ex.Message}");
        }
    }
}
```

**Register in Program.cs:**
```csharp
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");
```

**View in dashboard:**
- Projects → Select service → Health section shows all checks
- Red indicator if any check fails

### Liveness and Readiness Probes

For Kubernetes deployments, distinguish between:

**Liveness** (restart if unhealthy):
```csharp
builder.Services.AddHealthChecks()
    .AddCheck("live", () => HealthCheckResult.Healthy())
    .PublishHealthCheck("live");
```

**Readiness** (stop receiving traffic):
```csharp
builder.Services.AddHealthChecks()
    .AddCheck("ready", async () =>
    {
        var db = /* ... */;
        var canConnect = await db.CanConnectAsync();
        return canConnect 
            ? HealthCheckResult.Healthy() 
            : HealthCheckResult.Unhealthy("Database unavailable");
    });
```

## Performance Testing

### Load Testing Tools

#### K6 (JavaScript-based)

**Install:**
```bash
# macOS
brew install k6

# Linux
sudo apt-get install k6

# Windows
choco install k6
```

**Create test script (load-test.js):**
```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  vus: 10,
  duration: '30s',
};

export default function () {
  const res = http.get('https://localhost:7001/api/products');
  check(res, {
    'status is 200': (r) => r.status === 200,
    'response time < 200ms': (r) => r.timings.duration < 200,
  });
  sleep(1);
}
```

**Run test:**
```bash
k6 run load-test.js
```

**View results in dashboard metrics.**

#### Apache Benchmark (ab)

**Simple HTTP load test:**
```bash
# 1000 requests, 10 concurrent
ab -n 1000 -c 10 https://localhost:7001/api/products
```

### Database Query Performance

**Enable query logging:**

For PostgreSQL in AppHost:
```csharp
var postgres = builder.AddPostgres("postgres")
    .WithEnvironment("POSTGRES_INITDB_ARGS", "--log-statement=all");
```

**View slow queries:**
1. Dashboard → Containers → PostgreSQL → Logs
2. Search for slow queries
3. Check execution time

**In service, use Entity Framework logging:**
```csharp
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseNpgsql(sp.GetRequiredService<IConfiguration>().GetConnectionString("appdb"))
        .LogTo(Console.WriteLine, LogLevel.Information);
});
```

## Integration Testing

### Test Against Local Services

**Use AppHost in tests:**

```csharp
[TestClass]
public class ProductApiTests
{
    private static DistributedApplication _app;
    private static string _apiUrl;

    [ClassInitialize]
    public static async Task Initialize(TestContext context)
    {
        var builder = DistributedApplication.CreateBuilder();
        
        var postgres = builder.AddPostgres("postgres")
            .AddDatabase("testdb");
        
        var api = builder.AddProject<Projects.MyApi>("api")
            .WithReference(postgres)
            .WithHttpEndpoint(name: "http", scheme: "http");

        _app = builder.Build();
        await _app.StartAsync();

        // Get the API endpoint
        var httpResource = _app.Resources
            .OfType<ProjectResource>()
            .First(p => p.Name == "api");
        
        var endpoint = httpResource.GetEndpoint("http");
        _apiUrl = $"http://{endpoint.Address}:{endpoint.Port}";
    }

    [ClassCleanup]
    public static async Task Cleanup()
    {
        await _app.StopAsync();
    }

    [TestMethod]
    public async Task GetProducts_Returns200()
    {
        var client = new HttpClient();
        var response = await client.GetAsync($"{_apiUrl}/api/products");
        
        Assert.AreEqual(StatusCode.OK, response.StatusCode);
    }
}
```

### Container Resource Testing

**Start only required resources:**

```csharp
var postgres = builder.AddPostgres("postgres", password: "testpass");
var appdb = postgres.AddDatabase("testdb");

var api = builder.AddProject<Projects.MyApi>("api")
    .WithReference(appdb);
```

**Clean up after tests:**
```bash
# Stop AppHost and clean containers
# Or use test fixtures to handle cleanup
```

### Mock External Services

For services outside AppHost:

```csharp
var mockExternalApi = builder.AddContainer("external-api", "mockserver")
    .WithHttpEndpoint(port: 8080, targetPort: 8080)
    .WithEnvironment("MOCK_DEFINITION_PATH", "/config");

var api = builder.AddProject<Projects.MyApi>("api")
    .WithEnvironment("ExternalApi:Url", "http://external-api:8080")
    .WithReference(mockExternalApi);
```

## Logging Configuration

### Structured Logging

**Use Serilog for rich logging:**

```bash
dotnet add package Serilog
dotnet add package Serilog.AspNetCore
```

**In Program.cs:**
```csharp
builder.Host.UseSerilog((ctx, lc) =>
{
    lc.WriteTo.Console()
      .MinimumLevel.Debug()
      .Enrich.FromLogContext()
      .Enrich.WithProperty("Service", "MyApi");
});
```

**Log with context:**
```csharp
using (LogContext.PushProperty("UserId", userId))
{
    _logger.Information("User accessed resource");
    // All logs in this scope include UserId
}
```

### Log to File (Development)

**Configuration:**
```csharp
lc.WriteTo.File(
    "logs/app-.txt",
    rollingInterval: RollingInterval.Day,
    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
);
```

**View logs:**
```bash
tail -f logs/app-20240101.txt
```

### Custom Log Filtering

**Filter logs by service:**

```csharp
var logsForApi = dashboardLogs
    .Where(l => l.Service == "api")
    .Where(l => l.Level >= LogLevel.Warning);
```

## Performance Profiling

### CPU Profiling

**Use dotTrace or built-in tools:**

```bash
# With dotTrace
dotrace start --app-profiling-interval=1ms
# Use application
dotrace stop
```

**Or enable Diagnostic Events:**
```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddService("MyApi"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();
    });
```

### Memory Profiling

**Track memory usage:**
1. Dashboard → Projects → Select service
2. View memory graph
3. Watch for memory leaks during operation

**Or use dotMemory for detailed analysis**

## Persistence for Development

### Data Persistence

**Keep database data between AppHost runs:**

```csharp
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()  // Persists data
    .AddDatabase("appdb");
```

**Without persistence:**
```csharp
var postgres = builder.AddPostgres("postgres")
    // Database lost when AppHost stops
    .AddDatabase("appdb");
```

### Named Volumes

**Explicit volume management:**
```csharp
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("postgres-data")  // Named volume
    .AddDatabase("appdb");
```

**Clean up old volumes:**
```bash
docker volume ls
docker volume rm postgres-data
```

## Network Debugging

### Port Conflicts

**Check if ports are in use:**
```bash
# Windows
netstat -ano | findstr :5432

# macOS/Linux
lsof -i :5432
```

**AppHost shows port assignments in console:**
```
service started: listening on http://localhost:5000
```

### DNS Resolution

**Test service discovery:**
```bash
# From within container
docker exec -it container-name nslookup servicename

# Or
ping servicename  # Should resolve in Docker network
```

### Network Policy Issues

**Verify services can communicate:**
1. Start AppHost
2. Open service logs in dashboard
3. Look for connection errors
4. Check Docker network: `docker network ls`

## Environment-Specific Development

### Different Development Scenarios

**Minimal setup (API + database only):**
```csharp
var postgres = builder.AddPostgres("postgres").AddDatabase("appdb");
var api = builder.AddProject<Projects.MyApi>("api")
    .WithReference(postgres);

// Skip web app, workers, etc.
```

**Full system (all services):**
```csharp
// Everything from AppHost Program.cs
```

### Feature Development

**Test only changed feature:**
```csharp
// Disable unchanged services
// Keep dependencies needed for changes

var postgres = builder.AddPostgres("postgres").AddDatabase("appdb");

// Only the service being developed
var api = builder.AddProject<Projects.MyApi>("api")
    .WithReference(postgres)
    .WithEnvironment("FEATURE_NEW_API", "true");  // Enable feature
```

### Team Development

**Shared AppHost configuration:**
```csharp
// appsettings.json in AppHost for team defaults
// Team members override locally as needed
```

**Local overrides:**
```csharp
// appsettings.Development.json (in .gitignore)
// Team members customize for their machine
```

## Troubleshooting Local Development

### Dashboard Won't Connect

**Check AppHost is running:**
```bash
ps aux | grep "dotnet run"
```

**Verify port 15001 is not blocked:**
```bash
netstat -an | grep 15001
```

**Restart AppHost:**
```bash
# Stop existing process
# Start fresh
dotnet run --project MyApp.AppHost
```

### Services Can't Connect to Each Other

**Check service names match:**
- AppHost: `AddProject<Projects.MyApi>("api")`
- Service HttpClient: `new Uri("http://api")`

**Verify health checks pass:**
- Dashboard shows green indicators
- Click service for health details

**Check Docker network:**
```bash
docker network inspect bridge  # Or the network name
```

### Database Connection Fails

**Verify database is running:**
1. Dashboard → Containers → PostgreSQL (or other DB)
2. Check status and logs

**Test connection string:**
```bash
psql -h localhost -U postgres -d appdb
```

**Reset database:**
```bash
# AppHost will recreate with next run
# Or clean Docker volume:
docker volume rm postgres-data
```

### High Memory Usage

**Check for leaks:**
1. Dashboard → Metrics tab
2. Watch memory graph over time
3. Should stabilize after initial spike

**Reduce memory if testing:**
```csharp
var postgres = builder.AddPostgres("postgres")
    .WithMemoryLimit(512 * 1024 * 1024);  // 512 MB
```

## Best Practices

1. **Use dashboard daily** - Monitor all services and identify issues early
2. **Check logs first** - Most issues visible in logs tab
3. **Enable structured logging** - Makes debugging easier
4. **Test locally before pushing** - Catch integration issues early
5. **Use health checks** - Know when services are healthy
6. **Monitor metrics** - Watch for performance degradation
7. **Clean up volumes** - Remove old data between tests
8. **Document local setup** - Help new team members

## References

- [Aspire Dashboard](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard)
- [Local Development](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/local-development)
- [Health Checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [Logging](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging)
- [OpenTelemetry](https://learn.microsoft.com/en-us/dotnet/core/runtime-config/build-error-telemetry)

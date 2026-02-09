# Configuration Management in .NET Aspire

This guide covers environment configuration, secrets management, and feature flags in .NET Aspire applications.

## Configuration Fundamentals

### How Configuration Works

Aspire uses standard .NET configuration hierarchy:
1. `appsettings.json` (default)
2. `appsettings.{Environment}.json` (environment-specific)
3. Environment variables
4. User secrets (development only)
5. Command-line arguments

AppHost automatically injects configuration through environment variables.

### Development vs. Production

**Development:**
- Uses local `appsettings.Development.json`
- User secrets for sensitive data
- Aspire-managed resources (containers)
- Local dashboard for debugging

**Production:**
- Uses Azure Key Vault or managed secrets
- Connection strings injected by deployment system
- Managed Azure resources
- Remote telemetry (Application Insights, etc.)

## Configuration in AppHost

### Environment Variables

Pass environment variables from AppHost to services:

**In AppHost Program.cs:**
```csharp
var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.MyApi>("api")
    .WithEnvironment("LOG_LEVEL", "Information")
    .WithEnvironment("CACHE_DURATION", "300")
    .WithEnvironment("FEATURE_FLAG_NEW_PRICING", "true");

builder.Build().Run();
```

**Access in service Program.cs:**
```csharp
var builder = WebApplication.CreateBuilder(args);

var logLevel = builder.Configuration["LOG_LEVEL"];  // "Information"
var cacheDuration = int.Parse(builder.Configuration["CACHE_DURATION"]); // 300
var featureEnabled = bool.Parse(builder.Configuration["FEATURE_FLAG_NEW_PRICING"]); // true
```

### Resource Connection Strings

Aspire automatically injects connection strings for resources:

**AppHost configuration:**
```csharp
var postgres = builder.AddPostgres("postgres")
    .AddDatabase("appdb");

var redis = builder.AddRedis("cache");

var api = builder.AddProject<Projects.MyApi>("api")
    .WithReference(postgres)
    .WithReference(redis);
```

**Injected environment variables:**
- `ConnectionStrings__postgres=Host=localhost;Database=postgres;Username=postgres;Password=...`
- `ConnectionStrings__appdb=Host=localhost;Database=appdb;Username=postgres;Password=...`
- `ConnectionStrings__cache=localhost:6379`

**Access in service:**
```csharp
// Using connection string directly
var dbConnection = builder.Configuration.GetConnectionString("postgres");

// Or using Aspire component method (recommended)
builder.AddNpgsqlDbContext<MyDbContext>("appdb");

builder.AddRedisClient("cache");
```

### Parameters for Secrets

Define secrets in AppHost that are injected at runtime:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Define secret parameter
var apiKey = builder.CreateResourceBuilder(new Parameter("api-key", secret: true))
    .WithDefault("dev-key-12345");  // Default for local development

// Pass to services
var api = builder.AddProject<Projects.MyApi>("api")
    .WithEnvironment("API_KEY", apiKey);

var web = builder.AddProject<Projects.MyWeb>("web")
    .WithEnvironment("API_KEY", apiKey)
    .WithReference(api);

builder.Build().Run();
```

**Access in service:**
```csharp
var apiKey = builder.Configuration["API_KEY"];
```

## Configuration in Services

### Reading Configuration

**In Program.cs:**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Read specific values
var databaseHost = builder.Configuration["Database:Host"];
var cacheTimeout = builder.Configuration.GetValue<int>("Cache:Timeout", defaultValue: 300);

// Read sections
var databaseSection = builder.Configuration.GetSection("Database");
string host = databaseSection["Host"];
int port = databaseSection.GetValue<int>("Port");
```

**In controllers/services:**
```csharp
[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public ProductsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var pageSize = _configuration.GetValue<int>("Pagination:PageSize", 20);
        return Ok(new { pageSize });
    }
}
```

### Strongly Typed Configuration

**Define configuration class:**
```csharp
public class CacheSettings
{
    public int DurationSeconds { get; set; }
    public string Mode { get; set; }
    public bool Enabled { get; set; }
}

public class DatabaseSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Database { get; set; }
    public int PoolSize { get; set; }
}

public class ApplicationSettings
{
    public CacheSettings Cache { get; set; }
    public DatabaseSettings Database { get; set; }
    public string LogLevel { get; set; }
}
```

**Bind configuration in Program.cs:**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Option 1: Bind entire configuration
var applicationSettings = new ApplicationSettings();
builder.Configuration.Bind(applicationSettings);
builder.Services.AddSingleton(applicationSettings);

// Option 2: Use configuration option pattern (recommended)
builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("Cache"));
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("Database"));
```

**Use in service:**
```csharp
public class ProductService
{
    private readonly CacheSettings _cacheSettings;

    public ProductService(IOptions<CacheSettings> cacheSettings)
    {
        _cacheSettings = cacheSettings.Value;
    }

    public void DoSomething()
    {
        if (_cacheSettings.Enabled)
        {
            var duration = TimeSpan.FromSeconds(_cacheSettings.DurationSeconds);
            // Use cache with duration
        }
    }
}
```

**appsettings.json:**
```json
{
  "Cache": {
    "DurationSeconds": 300,
    "Mode": "Distributed",
    "Enabled": true
  },
  "Database": {
    "Host": "localhost",
    "Port": 5432,
    "Database": "appdb",
    "PoolSize": 10
  },
  "LogLevel": "Information"
}
```

## Secrets Management

### User Secrets (Development)

Store sensitive development values outside source control:

**Initialize user secrets:**
```bash
cd MyService
dotnet user-secrets init
```

**Set secrets:**
```bash
dotnet user-secrets set "Database:Password" "secure-dev-password"
dotnet user-secrets set "Api:Key" "dev-api-key-12345"
```

**List secrets:**
```bash
dotnet user-secrets list
```

**Clear secrets:**
```bash
dotnet user-secrets clear
```

Secrets are stored in `%APPDATA%\Microsoft\UserSecrets\<user-secrets-id>\secrets.json` (Windows) or `~/.microsoft/usersecrets/<user-secrets-id>/secrets.json` (macOS/Linux).

**Access in code:**
```csharp
var dbPassword = builder.Configuration["Database:Password"];  // From user secrets
```

### Environment-Specific Secrets

**appsettings.Development.json (NOT in source control):**
```json
{
  "Database": {
    "Password": "dev-password"
  },
  "Api": {
    "Key": "dev-api-key"
  }
}
```

Add to .gitignore:
```
appsettings.Development.json
appsettings.*.json  # Exclude all environment-specific files
```

### Azure Key Vault (Production)

Store production secrets in Azure Key Vault:

**AppHost deployment configuration:**
```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Reference Key Vault resource
var keyVault = builder.AddAzureKeyVault("keyvault");

// Services access secrets from Key Vault
var api = builder.AddProject<Projects.MyApi>("api")
    // When deployed, connects to Key Vault automatically
    .WithEnvironment("KEYVAULT_ENDPOINT", keyVault.GetProperty("endpoint"));

builder.Build().Run();
```

**Service configuration for Key Vault:**
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri("https://mykeyvault.vault.azure.net/"),
    new DefaultAzureCredential());
```

**Retrieve secrets:**
```csharp
var dbPassword = builder.Configuration["Database--Password"];  // Azure KV uses "--" separator
```

### Managed Identities

In Azure, use managed identities instead of connection strings:

**Enable managed identity for Container App:**
```bash
az containerapp identity assign \
  --name myapp \
  --resource-group myapp-rg \
  --system-assigned
```

**Grant Key Vault access:**
```bash
az keyvault set-policy \
  --name mykeyvault \
  --object-id <managed-identity-id> \
  --secret-permissions get list
```

**No credentials needed; Azure handles authentication automatically.**

## Feature Flags

### Simple Feature Flags

**In AppHost:**
```csharp
var enableNewPricing = builder.CreateResourceBuilder(new Parameter("feature-new-pricing"))
    .WithDefault("false");

var enableNewUI = builder.CreateResourceBuilder(new Parameter("feature-new-ui"))
    .WithDefault("true");

var api = builder.AddProject<Projects.MyApi>("api")
    .WithEnvironment("FEATURES__NEW_PRICING", enableNewPricing)
    .WithEnvironment("FEATURES__NEW_UI", enableNewUI);
```

**In configuration:**
```csharp
builder.Services.Configure<FeatureFlags>(
    builder.Configuration.GetSection("Features"));
```

**appsettings.json:**
```json
{
  "Features": {
    "NewPricing": false,
    "NewUI": true,
    "BetaApi": false
  }
}
```

**Feature flag class:**
```csharp
public class FeatureFlags
{
    public bool NewPricing { get; set; }
    public bool NewUI { get; set; }
    public bool BetaApi { get; set; }
}
```

### Using Feature Flags in Code

**In controller:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IOptions<FeatureFlags> _features;

    public ProductsController(IOptions<FeatureFlags> features)
    {
        _features = features.Value;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(CreateProductRequest request)
    {
        if (_features.NewPricing)
        {
            // Use new pricing engine
            var pricing = new NewPricingEngine().Calculate(request);
        }
        else
        {
            // Use legacy pricing
            var pricing = new LegacyPricingEngine().Calculate(request);
        }

        return Ok(pricing);
    }
}
```

**Middleware for UI feature:**
```csharp
app.Use(async (context, next) =>
{
    var features = context.RequestServices.GetRequiredService<IOptions<FeatureFlags>>();
    
    if (features.Value.NewUI)
    {
        context.Items["UI_Version"] = "v2";
    }
    else
    {
        context.Items["UI_Version"] = "v1";
    }

    await next();
});
```

### Advanced Feature Flags with Launch Darkly

For more sophisticated feature management, integrate Launch Darkly:

**Install NuGet package:**
```bash
dotnet add package LaunchDarkly.ServerSdk
```

**Configure in AppHost:**
```csharp
var launchDarklyKey = builder.CreateResourceBuilder(new Parameter("launchdarkly-key", secret: true))
    .WithDefault("sdk-key-dev");

var api = builder.AddProject<Projects.MyApi>("api")
    .WithEnvironment("LAUNCHDARKLY_KEY", launchDarklyKey);
```

**Initialize in service:**
```csharp
builder.Services.AddSingleton<ILdClient>(sp =>
{
    var key = sp.GetRequiredService<IConfiguration>()["LAUNCHDARKLY_KEY"];
    return new LdClient(key);
});
```

**Use feature flag:**
```csharp
public class PricingService
{
    private readonly ILdClient _ldClient;

    public decimal CalculatePrice(int productId)
    {
        var user = LaunchDarkly.Sdk.User.WithKey("user123");
        
        if (_ldClient.BoolVariation("new-pricing-enabled", user, false))
        {
            return CalculateNewPrice(productId);
        }
        else
        {
            return CalculateLegacyPrice(productId);
        }
    }
}
```

## Configuration Best Practices

### Development Configuration

**appsettings.Development.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information"
    }
  },
  "AllowedHosts": "*",
  "Database": {
    "Host": "localhost",
    "Port": 5432
  },
  "Cache": {
    "DurationSeconds": 60
  }
}
```

### Production Configuration

**Never commit production secrets or keys:**
- Use Azure Key Vault
- Use environment variables
- Use managed identities

**Minimal appsettings.Production.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning"
    }
  },
  "AllowedHosts": ".example.com",
  "Cache": {
    "DurationSeconds": 3600
  }
}
```

### Configuration Validation

Validate configuration at startup:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Validate configuration
builder.Services.AddOptions<DatabaseSettings>()
    .BindConfiguration("Database")
    .ValidateDataAnnotations()
    .ValidateOnStart();

public class DatabaseSettings
{
    [Required]
    public string Host { get; set; }

    [Range(1, 65535)]
    public int Port { get; set; } = 5432;

    [Required]
    public string Database { get; set; }
}
```

### Configuration Reload

Reload configuration without restarting (for non-critical settings):

```csharp
builder.Services.AddSingleton<IConfigurationRoot>(sp => 
    builder.Configuration as IConfigurationRoot);

public class FeatureFlagService
{
    private readonly IConfigurationRoot _configuration;

    public FeatureFlagService(IConfigurationRoot configuration)
    {
        _configuration = configuration;
    }

    public bool IsFeatureEnabled(string feature)
    {
        // Reloads configuration each call (for development)
        _configuration.Reload();
        return _configuration.GetValue<bool>($"Features:{feature}", false);
    }
}
```

## Common Patterns

### Secrets as Environment Variables

**AppHost:**
```csharp
var databasePassword = builder.CreateResourceBuilder(new Parameter("db-password", secret: true))
    .WithDefault("dev-password");

var postgres = builder.AddPostgres("postgres", password: databasePassword)
    .AddDatabase("appdb");
```

### Multi-Environment Support

**AppHost detects environment from ASPNETCORE_ENVIRONMENT:**

```csharp
var environment = builder.Environment.IsProduction() ? "Production" : "Development";

builder.AddProject<Projects.MyApi>("api")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", environment);
```

**Service loads appropriate settings file automatically.**

### Configuration Override Hierarchy

**Priority (highest to lowest):**
1. Command-line arguments
2. Environment variables
3. User secrets (development)
4. `appsettings.{Environment}.json`
5. `appsettings.json`

**Example:**
```bash
# Command-line override wins
dotnet run --Database:Host=prod-db.example.com

# Or environment variable
ASPNETCORE_DATABASE__HOST=prod-db.example.com dotnet run

# Or appsettings.Production.json
```

### Keyed Configuration Sections

Group related settings:

```json
{
  "ConnectionStrings": {
    "Primary": "Host=localhost;Database=primary",
    "Secondary": "Host=localhost;Database=secondary"
  },
  "Logging": {
    "Serilog": {
      "MinimumLevel": "Information"
    }
  }
}
```

**Access:**
```csharp
var primaryConnection = builder.Configuration.GetConnectionString("Primary");
var logLevel = builder.Configuration.GetSection("Logging:Serilog:MinimumLevel").Value;
```

## Troubleshooting Configuration

### Configuration Not Being Read

1. Verify file is named correctly (case-sensitive on Linux)
2. Check file is in project root
3. Ensure project includes it: `<Content Include="appsettings.json" CopyToOutputDirectory="PreserveNewest" />`
4. Verify environment name matches filename

### Secrets Not Injected

1. Check environment variable name in AppHost matches service code
2. Verify `WithEnvironment()` is called
3. Confirm secret parameter has `.WithDefault()` value
4. Review AppHost console output for environment variable names

### Production Secrets Exposed

1. Check `.gitignore` includes `appsettings.*.json` and sensitive files
2. Review commit history: `git log --all -S "password"`
3. Never hardcode secrets in source
4. Use Azure Key Vault for production
5. Rotate compromised secrets immediately

## References

- [Configuration in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration)
- [Options Pattern](https://learn.microsoft.com/en-us/dotnet/core/extensions/options)
- [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Azure Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/)
- [Feature Management](https://learn.microsoft.com/en-us/azure/azure-app-configuration/concept-feature-management)

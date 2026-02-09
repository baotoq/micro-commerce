# .NET Aspire Component Packages

This reference guide covers the available Aspire component packages for integrating databases, caching, messaging, storage, and other cloud services into your distributed applications.

## Component Categories

### Database Components

#### PostgreSQL
**Hosting Package:** `Aspire.Hosting.PostgreSQL`
**Client Package:** `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL`

**AppHost Configuration:**
```csharp
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()  // Optional: Add PgAdmin container
    .AddDatabase("mydb");
```

**Service Configuration:**
```csharp
builder.AddNpgsqlDbContext<MyDbContext>("mydb");
```

**Connection String Pattern:** `Host=localhost;Database=mydb;Username=postgres;Password=...`

---

#### SQL Server
**Hosting Package:** `Aspire.Hosting.SqlServer`
**Client Package:** `Aspire.Microsoft.EntityFrameworkCore.SqlServer`

**AppHost Configuration:**
```csharp
var sql = builder.AddSqlServer("sqlserver")
    .AddDatabase("catalogdb");
```

**Service Configuration:**
```csharp
builder.AddSqlServerDbContext<CatalogDbContext>("catalogdb");
```

**Connection String Pattern:** `Server=localhost,1433;Database=catalogdb;User Id=sa;Password=...`

---

#### MySQL
**Hosting Package:** `Aspire.Hosting.MySql`
**Client Package:** `Aspire.Pomelo.EntityFrameworkCore.MySql`

**AppHost Configuration:**
```csharp
var mysql = builder.AddMySql("mysql")
    .AddDatabase("appdb");
```

**Service Configuration:**
```csharp
builder.AddMySqlDbContext<AppDbContext>("appdb");
```

---

#### MongoDB
**Hosting Package:** `Aspire.Hosting.MongoDB`
**Client Package:** `Aspire.MongoDB.Driver`

**AppHost Configuration:**
```csharp
var mongo = builder.AddMongoDB("mongodb")
    .AddDatabase("ordersdb");
```

**Service Configuration:**
```csharp
builder.AddMongoDBClient("mongodb");
```

**Access in Service:**
```csharp
var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
var database = mongoClient.GetDatabase("ordersdb");
```

---

#### Oracle Database
**Hosting Package:** `Aspire.Hosting.Oracle`
**Client Package:** `Aspire.Oracle.EntityFrameworkCore`

**AppHost Configuration:**
```csharp
var oracle = builder.AddOracle("oracle")
    .AddDatabase("freepdb1");
```

**Service Configuration:**
```csharp
builder.AddOracleDbContext<MyOracleContext>("freepdb1");
```

---

### Caching Components

#### Redis
**Hosting Package:** `Aspire.Hosting.Redis`
**Client Packages:**
- `Aspire.StackExchange.Redis` (general caching)
- `Aspire.StackExchange.Redis.OutputCaching` (ASP.NET output caching)
- `Aspire.StackExchange.Redis.DistributedCaching` (distributed cache)

**AppHost Configuration:**
```csharp
var redis = builder.AddRedis("cache")
    .WithRedisCommander();  // Optional: Add Redis Commander UI
```

**Service Configuration (General):**
```csharp
builder.AddRedisClient("cache");
```

**Service Configuration (Distributed Cache):**
```csharp
builder.AddRedisDistributedCache("cache");
```

**Service Configuration (Output Cache):**
```csharp
builder.AddRedisOutputCache("cache");
```

**Usage:**
```csharp
// IDistributedCache
var cache = serviceProvider.GetRequiredService<IDistributedCache>();
await cache.SetStringAsync("key", "value");

// IConnectionMultiplexer
var redis = serviceProvider.GetRequiredService<IConnectionMultiplexer>();
var db = redis.GetDatabase();
await db.StringSetAsync("key", "value");
```

---

#### Valkey (Redis alternative)
**Hosting Package:** `Aspire.Hosting.Valkey`
**Client Package:** `Aspire.StackExchange.Redis`

**AppHost Configuration:**
```csharp
var valkey = builder.AddValkey("cache");
```

**Service Configuration:**
```csharp
builder.AddRedisClient("cache");
```

---

### Messaging Components

#### RabbitMQ
**Hosting Package:** `Aspire.Hosting.RabbitMQ`
**Client Package:** `Aspire.RabbitMQ.Client`

**AppHost Configuration:**
```csharp
var messaging = builder.AddRabbitMQ("messaging")
    .WithManagementPlugin();  // Optional: Enable management UI
```

**Service Configuration:**
```csharp
builder.AddRabbitMQClient("messaging");
```

**Usage:**
```csharp
var connectionFactory = serviceProvider.GetRequiredService<IConnectionFactory>();
using var connection = connectionFactory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare("orders", durable: true, exclusive: false);
channel.BasicPublish("", "orders", null, body);
```

---

#### Azure Service Bus
**Client Package:** `Aspire.Azure.Messaging.ServiceBus`

**AppHost Configuration:**
```csharp
var serviceBus = builder.AddAzureServiceBus("messaging");
```

**Service Configuration:**
```csharp
builder.AddAzureServiceBusClient("messaging");
```

---

#### Kafka
**Hosting Package:** `Aspire.Hosting.Kafka`
**Client Package:** `Aspire.Confluent.Kafka`

**AppHost Configuration:**
```csharp
var kafka = builder.AddKafka("messaging")
    .WithKafkaUI();  // Optional: Add Kafka UI
```

**Service Configuration:**
```csharp
builder.AddKafkaProducer<string, string>("messaging");
builder.AddKafkaConsumer<string, string>("messaging", config => {
    config.GroupId = "my-consumer-group";
});
```

---

### Storage Components

#### Azure Blob Storage
**Client Package:** `Aspire.Azure.Storage.Blobs`

**AppHost Configuration:**
```csharp
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator()  // Use Azurite for local dev
    .AddBlobs("blobs");
```

**Service Configuration:**
```csharp
builder.AddAzureBlobClient("blobs");
```

**Usage:**
```csharp
var blobServiceClient = serviceProvider.GetRequiredService<BlobServiceClient>();
var containerClient = blobServiceClient.GetBlobContainerClient("mycontainer");
```

---

#### Azure Queue Storage
**Client Package:** `Aspire.Azure.Storage.Queues`

**AppHost Configuration:**
```csharp
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator()
    .AddQueues("queues");
```

**Service Configuration:**
```csharp
builder.AddAzureQueueClient("queues");
```

---

#### Azure Table Storage
**Client Package:** `Aspire.Azure.Storage.Tables`

**AppHost Configuration:**
```csharp
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator()
    .AddTables("tables");
```

**Service Configuration:**
```csharp
builder.AddAzureTableClient("tables");
```

---

### Search Components

#### Elasticsearch
**Hosting Package:** `Aspire.Hosting.Elasticsearch`
**Client Package:** `Aspire.Elastic.Clients.Elasticsearch`

**AppHost Configuration:**
```csharp
var elasticsearch = builder.AddElasticsearch("search")
    .WithDataVolume();
```

**Service Configuration:**
```csharp
builder.AddElasticsearchClient("search");
```

---

#### Azure AI Search
**Client Package:** `Aspire.Azure.Search.Documents`

**AppHost Configuration:**
```csharp
var search = builder.AddAzureSearch("search");
```

**Service Configuration:**
```csharp
builder.AddAzureSearchClient("search");
```

---

### Configuration and Secrets

#### Azure Key Vault
**Client Package:** `Aspire.Azure.Security.KeyVault`

**AppHost Configuration:**
```csharp
var keyVault = builder.AddAzureKeyVault("keyvault");
```

**Service Configuration:**
```csharp
builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
```

---

### Observability Components

#### Application Insights
**Client Package:** `Aspire.Azure.ApplicationInsights`

**Service Configuration:**
```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

---

### Email Components

#### MailKit (SMTP)
**Hosting Package:** `Aspire.Hosting.MailDev`
**Client Package:** `Aspire.MailKit`

**AppHost Configuration:**
```csharp
var mail = builder.AddMailDev("mail");  // Local SMTP server for dev
```

**Service Configuration:**
```csharp
builder.AddMailKitClient("mail");
```

**Usage:**
```csharp
var smtpClient = serviceProvider.GetRequiredService<ISmtpClient>();
await smtpClient.SendAsync(message);
```

---

### Identity and Authentication

#### Azure Active Directory
**Client Package:** `Aspire.Azure.Identity`

**Service Configuration:**
```csharp
builder.Services.AddDefaultAzureCredential();
```

---

### Developer Tools

#### Seq (Logging)
**Hosting Package:** `Aspire.Hosting.Seq`

**AppHost Configuration:**
```csharp
var seq = builder.AddSeq("seq");
```

Services automatically send logs to Seq when configured.

---

#### Grafana + Prometheus
**Hosting Package:** `Aspire.Hosting.Prometheus` / `Aspire.Hosting.Grafana`

**AppHost Configuration:**
```csharp
var prometheus = builder.AddPrometheus("prometheus");
var grafana = builder.AddGrafana("grafana")
    .WithReference(prometheus);
```

---

## Component Configuration Patterns

### Health Checks
Most components automatically register health checks:
```csharp
builder.AddRedisClient("cache");  // Registers Redis health check
```

Access at `/health` endpoint (when using ServiceDefaults).

### Connection String Override
Override auto-generated connection strings:
```csharp
var postgres = builder.AddPostgres("postgres")
    .WithEnvironment("POSTGRES_PASSWORD", "custom-password");
```

### Persistence
Add volumes for data persistence:
```csharp
var mongo = builder.AddMongoDB("mongodb")
    .WithDataVolume();  // Persists data between restarts
```

### Resource Limits
Set container resource limits:
```csharp
var postgres = builder.AddPostgres("postgres")
    .WithMemoryLimit(1024 * 1024 * 1024);  // 1GB limit
```

### Custom Images
Use custom container images:
```csharp
var redis = builder.AddRedis("cache")
    .WithImage("redis/redis-stack", "latest");
```

## Component Selection Guide

### When to Use Each Database

| Database | Use Case |
|----------|----------|
| PostgreSQL | General-purpose RDBMS, JSON support, full-text search |
| SQL Server | Microsoft stack, advanced analytics, reporting |
| MySQL | Web applications, read-heavy workloads |
| MongoDB | Document storage, flexible schemas, hierarchical data |
| Oracle | Enterprise applications, complex transactions |

### When to Use Each Cache

| Cache | Use Case |
|-------|----------|
| Redis | Session storage, real-time analytics, pub/sub |
| Valkey | Redis alternative (open-source fork) |
| Output Cache | ASP.NET page/fragment caching |

### When to Use Each Message Broker

| Broker | Use Case |
|--------|----------|
| RabbitMQ | Traditional messaging, task queues, routing |
| Kafka | Event streaming, log aggregation, high throughput |
| Azure Service Bus | Enterprise messaging, Azure integration |

## Quick Reference: Install Commands

```bash
# Database
dotnet add package Aspire.Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Aspire.Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Aspire.Pomelo.EntityFrameworkCore.MySql
dotnet add package Aspire.MongoDB.Driver

# Caching
dotnet add package Aspire.StackExchange.Redis
dotnet add package Aspire.StackExchange.Redis.DistributedCaching
dotnet add package Aspire.StackExchange.Redis.OutputCaching

# Messaging
dotnet add package Aspire.RabbitMQ.Client
dotnet add package Aspire.Azure.Messaging.ServiceBus
dotnet add package Aspire.Confluent.Kafka

# Storage
dotnet add package Aspire.Azure.Storage.Blobs
dotnet add package Aspire.Azure.Storage.Queues
dotnet add package Aspire.Azure.Storage.Tables

# Search
dotnet add package Aspire.Elastic.Clients.Elasticsearch
dotnet add package Aspire.Azure.Search.Documents

# Other
dotnet add package Aspire.MailKit
dotnet add package Aspire.Azure.Security.KeyVault
```

## Additional Component Resources

For the latest component packages and versions:
- NuGet: Search for "Aspire" packages
- GitHub: https://github.com/dotnet/aspire
- Documentation: https://learn.microsoft.com/dotnet/aspire/fundamentals/components-overview

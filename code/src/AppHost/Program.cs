using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin()
    .WithLifetime(ContainerLifetime.Persistent);

var db = postgres.AddDatabase("db");

var storage = builder.AddAzureStorage("storage");

if (builder.Environment.IsDevelopment())
{
    storage.RunAsEmulator(s => s
        .WithBlobPort(27000)
        .WithQueuePort(27001)
        .WithTablePort(27002)
        .WithLifetime(ContainerLifetime.Persistent)
        .WithDataVolume()
    );
}

var blobs = storage.AddBlobs("blobs");

var elasticsearch = builder.AddElasticsearch("elasticsearch")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

// var kibana = builder.AddContainer("kibana", "kibana", "8.13.0")
//     .WaitFor(elasticsearch)
//     .WithEnvironment("ELASTICSEARCH_HOSTS", elasticsearch.GetEndpoint("http"))
//     .WithEnvironment("ELASTICSEARCH_USERNAME", "kibana_system")
//     .WithEnvironment("ELASTICSEARCH_PASSWORD", elasticsearch.Resource.PasswordParameter.Value)
//     .WithHttpEndpoint(targetPort: 5601)
//     .WithLifetime(ContainerLifetime.Persistent);

var cache = builder.AddRedis("redis")
    .WithRedisInsight()
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var rabbitmq = builder.AddRabbitMQ("messaging")
    .WithDataVolume()
    .WithManagementPlugin()
    .WithLifetime(ContainerLifetime.Persistent);

var cartService = builder.AddProject<Projects.CartService_Api>("cart-service")
    .WithReference(elasticsearch)
    .WithReference(cache)
    .WithReference(rabbitmq).WaitFor(rabbitmq)
    .WithReference(db).WaitFor(db)
    .WithReference(blobs)
    .WithHttpHealthCheck("/health");

var productService = builder.AddProject<Projects.ProductService_Api>("product-service")
    .WithReference(elasticsearch)
    .WithReference(cache)
    .WithReference(rabbitmq).WaitFor(rabbitmq)
    .WithReference(db).WaitFor(db)
    .WithReference(blobs)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.Yarp>("yarp")
    .WithReference(productService)
    .WithReference(cartService);

builder.Build().Run();

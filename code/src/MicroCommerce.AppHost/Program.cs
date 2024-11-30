var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin()
    .WithLifetime(ContainerLifetime.Persistent);

var db = postgres.AddDatabase("db");

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

var migrationService = builder.AddProject<Projects.MicroCommerce_MigrationService>("migrationservice")
    .WithReference(db).WaitFor(db)
    .WithReference(rabbitmq).WaitFor(rabbitmq)
    .WithHttpHealthCheck("/health");

var apiService = builder.AddProject<Projects.MicroCommerce_ApiService>("apiservice")
    .WithReference(elasticsearch)
    .WithReference(cache)
    .WithReference(rabbitmq).WaitFor(rabbitmq)
    .WithReference(db).WaitFor(db)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.MicroCommerce_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();

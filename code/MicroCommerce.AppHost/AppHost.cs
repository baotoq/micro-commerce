var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL with persistent data volume
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin();

// Create database for the application (shared connection, schema isolation per module)
var appDb = postgres.AddDatabase("appdb");

// Azure Service Bus emulator for domain events
var messaging = builder.AddAzureServiceBus("messaging")
    .RunAsEmulator();

var keycloak = builder
    .AddKeycloak("keycloak", 8101)
    .WithDataVolume()
    .WithRealmImport("./Realms")
    .WithLifetime(ContainerLifetime.Persistent);

var apiService = builder.AddProject<Projects.MicroCommerce_ApiService>("apiservice")
    .WithReference(keycloak)
    .WithReference(appDb)
    .WithReference(messaging)
    .WithHttpHealthCheck("/health");

builder.AddJavaScriptApp("frontend", "../MicroCommerce.Web")
    .WithReference(apiService)
    .WithReference(keycloak)
    .WithHttpEndpoint(port: 3000, env: "PORT");

builder.Build().Run();
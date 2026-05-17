#pragma warning disable ASPIREJAVASCRIPT001

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache").WithDataVolume().WithLifetime(ContainerLifetime.Persistent);

var postgres = builder.AddPostgres("postgres").WithDataVolume().WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("catalogdb");

var pubSub = builder.AddDaprPubSub("pubsub");

var catalog = builder.AddProject<Projects.MicroCommerce_Catalog>("catalog-api")
    .WithReference(cache)
    .WithReference(postgres)
    .WaitFor(cache)
    .WaitFor(postgres)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .WithDaprSidecar(sidecar => sidecar.WithReference(pubSub));

if (builder.Environment.EnvironmentName != "Testing")
{
    builder.AddNextJsApp("web", "../web")
        .WithEnvironment("API_URL", catalog.GetEndpoint("http"))
        .WithExternalHttpEndpoints();
}

builder.Build().Run();

#pragma warning disable ASPIREJAVASCRIPT001

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var postgres = builder.AddPostgres("postgres")
    .AddDatabase("catalogdb");

var server = builder.AddProject<Projects.MicroCommerce_Catalog>("catalog-api")
    .WithReference(cache)
    .WithReference(postgres)
    .WaitFor(cache)
    .WaitFor(postgres)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .WithDaprSidecar();

if (builder.Environment.EnvironmentName != "Testing")
{
    builder.AddNextJsApp("web", "../web")
        .WithEnvironment("API_URL", server.GetEndpoint("http"))
        .WaitFor(server)
        .WithExternalHttpEndpoints();
}

builder.Build().Run();

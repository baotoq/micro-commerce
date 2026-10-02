using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

// Integration tests pass --EphemeralResources=true to get throwaway containers
// instead of reusing the persistent dev containers and their data volumes.
var ephemeral = builder.Configuration.GetValue<bool>("EphemeralResources");

var postgres = builder.AddPostgres("postgres");
var redis = builder.AddRedis("redis");

if (!ephemeral)
{
    postgres.WithDataVolume().WithPgAdmin().WithLifetime(ContainerLifetime.Persistent);
    redis.WithDataVolume().WithRedisInsight().WithLifetime(ContainerLifetime.Persistent);
}

var db = postgres.AddDatabase("micro-commerce-db");

var api = builder.AddProject<Projects.MicroCommerce_ApiService>("apiservice")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(db)
    .WaitFor(db)
    .WithReference(redis)
    .WaitFor(redis);

#pragma warning disable ASPIREJAVASCRIPT001 // AddNextJsApp is experimental
builder.AddNextJsApp("frontend", "../micro-commerce", runScriptName: "dev")
    .WithPnpm()
    .WithEnvironment("API_URL", api.GetEndpoint("http"))
    .WithExternalHttpEndpoints()
    .WaitFor(api);
#pragma warning restore ASPIREJAVASCRIPT001

builder.Build().Run();

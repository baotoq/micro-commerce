#pragma warning disable ASPIREJAVASCRIPT001

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var server = builder.AddProject<Projects.MicroCommerce_Server>("server")
    .WithReference(cache)
    .WaitFor(cache)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

builder.AddNextJsApp("web", "../web")
    .WithEnvironment("API_URL", server.GetEndpoint("http"))
    .WaitFor(server)
    .WithExternalHttpEndpoints();

builder.Build().Run();

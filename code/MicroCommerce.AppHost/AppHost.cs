var builder = DistributedApplication.CreateBuilder(args);

var keycloak = builder
    .AddKeycloak("keycloak", 8101)
    .WithDataVolume()
     .WithLifetime(ContainerLifetime.Persistent);

var apiService = builder.AddProject<Projects.MicroCommerce_ApiService>("apiservice")
    .WithReference(keycloak)
    .WithHttpHealthCheck("/health");

builder.AddJavaScriptApp("frontend", "../MicroCommerce.Web")
    .WithReference(apiService)
    .WithReference(keycloak)
    .WithHttpEndpoint(port: 3000, env: "PORT");

builder.Build().Run();
var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.MicroCommerce_ApiService>("apiservice");

builder.AddProject<Projects.MicroCommerce_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();

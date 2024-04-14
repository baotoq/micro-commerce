var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var apiService = builder.AddProject<Projects.MicroCommerce_ApiService>("apiservice")
    .WithReference(cache);

builder.AddProject<Projects.MicroCommerce_Web>("webfrontend")
    .WithReference(cache)
    .WithReference(apiService);

builder.Build().Run();
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin();

var catalogDb = postgres.AddDatabase("catalogdb");

var elasticsearch = builder.AddElasticsearch("elasticsearch")
    .WithDataVolume();

var apiService = builder.AddProject<Projects.MicroCommerce_ApiService>("apiservice")
    .WithReference(elasticsearch)
    .WithReference(catalogDb);

builder.AddProject<Projects.MicroCommerce_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();

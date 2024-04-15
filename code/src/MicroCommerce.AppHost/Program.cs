using MicroCommerce.AppHost;
using MicroCommerce.ServiceDefaults;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// elasticsearch take too much time to start, aspire is suck on this

// var elasticsearch = builder.AddContainer("elasticsearch", "elasticsearch", "8.13.0")
//     .WithEnvironment("xpack.security.enabled", "false")
//     .WithEnvironment("discovery.type", "single-node")
//     .WithVolume(target: "/usr/share/elasticsearch/data")
//     .WithHttpEndpoint(9200, targetPort: 9200);
//
// var elasticsearchEndpoint = elasticsearch.GetEndpoint("http");
//
// var kibana = builder.AddContainer("kibana", "kibana", "8.13.0")
//     .WithReference(elasticsearchEndpoint)
//     .WithEnvironment("ELASTICSEARCH_HOSTS", "http://elasticsearch:9200")
//     .WithHttpEndpoint(5601);

var postgres = builder
    .AddPostgres("postgres", password: builder.CreateStablePassword("postgrespassword"))
    .WithDataVolume()
    .AddDatabase(AspireConstants.Database);

var cache = builder.AddRedis(AspireConstants.Redis);
var messaging = builder.AddRabbitMQ(AspireConstants.Messaging, password: builder.CreateStablePassword(AspireConstants.Messaging+"password"))
    .WithDataVolume()
    .WithManagementPlugin();

var apiService = builder.AddProject<Projects.MicroCommerce_ApiService>(AspireConstants.ApiService)
    .WithReference(postgres)
    .WithReference(cache)
    .WithReference(messaging);

var frontend = builder.AddNpmApp(AspireConstants.NextjsWeb, "../MicroCommerce.NextjsWeb", "dev")
    .WithReference(apiService)
    .WithReference(cache)
    .WithHttpEndpoint(port: 3000, env: "PORT")
    .PublishAsDockerFile();

if (builder.Environment.IsDevelopment() && builder.Configuration["DOTNET_LAUNCH_PROFILE"] == "https")
{
    // Disable TLS certificate validation in development, see https://github.com/dotnet/aspire/issues/3324 for more details.
    frontend.WithEnvironment("NODE_TLS_REJECT_UNAUTHORIZED", "0");
}

builder.Build().Run();
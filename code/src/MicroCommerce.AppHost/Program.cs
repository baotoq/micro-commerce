using MicroCommerce.AppHost;
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
    .AddDatabase("microcommerce");

var cache = builder.AddRedis("redis");
var messaging = builder.AddRabbitMQ("messeaging");

var apiService = builder.AddProject<Projects.MicroCommerce_ApiService>("apiservice")
    .WithReference(postgres)
    .WithReference(cache)
    .WithReference(messaging);

var frontend = builder.AddNpmApp("nextjsweb", "../MicroCommerce.NextjsWeb", "dev")
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
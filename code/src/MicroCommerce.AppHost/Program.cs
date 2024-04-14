using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddPostgres("db").AddDatabase("micro-commerce");
var cache = builder.AddRedis("redis");
var messaging = builder.AddRabbitMQ("rabbitmq");

var apiService = builder.AddProject<Projects.MicroCommerce_ApiService>("apiservice")
    .WithReference(db)
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
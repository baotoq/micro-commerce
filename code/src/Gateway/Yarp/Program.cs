using System.Threading.RateLimiting;
using MicroCommerce.BuildingBlocks.ServiceDefaults;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

builder.Services.AddRateLimiter(options =>
{
    var window = TimeSpan.FromSeconds(10);
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        var httpContext = context.HttpContext;
        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        httpContext.Response.Headers.RetryAfter = window.Seconds.ToString();

        await httpContext.Response.WriteAsync("Rate limit exceeded. Please try again later.", cancellationToken);

        var logger = httpContext.RequestServices.GetService<ILogger<RateLimiter>>();
        logger?.LogWarning("Rate limit exceeded for IP: {IpAddress} User: {User}", httpContext.Connection.RemoteIpAddress, httpContext.User.Identity?.Name);
    };
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10,
                QueueLimit = 0,
                Window = window
            }));
});

var app = builder.Build();

app.UseDefault();
app.UseRateLimiter();
app.MapDefaultEndpoints();
app.MapReverseProxy();

app.Run();

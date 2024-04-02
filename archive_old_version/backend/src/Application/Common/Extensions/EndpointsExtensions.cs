using Application.UseCases.Ping;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Extensions;

public static class EndpointsExtensions
{
    public static void MapEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/ping", async (IMediator mediator) => Results.Ok(await mediator.Send(new PingCommand())));
        endpoints.MapGet("/migrate", async (ApplicationDbContext context) =>
        {
            await context.Database.MigrateAsync();
            return Results.Ok("Migrated successfully");
        });
        
        endpoints.MapHealthChecks("/healthz");
    }
}
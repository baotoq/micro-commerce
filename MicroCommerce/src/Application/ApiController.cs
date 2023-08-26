using System.Diagnostics.CodeAnalysis;
using Application.Ping;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Application;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiController : ControllerBase
{
    protected readonly IMediator Mediator;

    protected ApiController(IMediator mediator)
    {
        Mediator = mediator;
    }
}

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
    }
}

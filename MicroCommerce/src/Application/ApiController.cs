using System.Diagnostics.CodeAnalysis;
using Application.Ping;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Application;

[ApiController]
[Route("[controller]")]
public abstract class ApiController : Controller
{
    protected readonly IMediator Mediator;

    protected ApiController(IMediator mediator)
    {
        Mediator = mediator;
    }
}

public static class EndpointsExtensions
{
    public static void MapPing(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/ping", async (IMediator mediator) => Results.Ok(await mediator.Send(new PingCommand())));
    }
}

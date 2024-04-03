using Api.UseCases.Database;
using MediatR;

namespace Api.Endpoints;

public static class DatabaseEndpoint
{
    public static void MapSeed(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/database")
            .WithTags("database");
        
        group.MapGet("/seed", SeedDataCommand.EndpointHandler);
        group.MapGet("/migrate", MigrateDatabaseCommand.EndpointHandler);
    }
}
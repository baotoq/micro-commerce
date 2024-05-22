using MediatR;
using MicroCommerce.ApiService.UseCases.Categories;

namespace MicroCommerce.ApiService.Endpoints;

public static class CartEndCategoryEndpoint
{
    public static void MapCategories(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/categories")
            .WithTags("categories");
        
        group.MapGet("/", (IMediator mediator) => mediator.Send(new GetCategoriesQuery()));
        group.MapGet("/{id}", (IMediator mediator, string id) => mediator.Send(new GetCategoryQuery(id)));
        group.MapPost("/", (IMediator mediator, CreateCategoryCommand request) => mediator.Send(request));
        group.MapPatch("/{id}", (IMediator mediator, UpdateCategoryCommand request, string id) =>
        {
            request.Id = id;
            return mediator.Send(request);
        });
        group.MapDelete("/{id}", (IMediator mediator, string id) => mediator.Send(new DeleteCategoryCommand(id)));
    }
}
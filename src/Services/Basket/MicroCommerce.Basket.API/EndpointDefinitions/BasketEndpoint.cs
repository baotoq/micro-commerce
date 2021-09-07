using MicroCommerce.Basket.API.Models;
using MicroCommerce.Basket.API.Persistence.Repositories;
using MicroCommerce.Basket.API.Persistence.Repositories.Abstractions;

namespace MicroCommerce.Basket.API.EndpointDefinitions;

public class BasketEndpoint : IEndpointDefinition
{
    public void DefineEnpoints(WebApplication app)
    {
        app.MapGet("api/basket/{id}", GetBasketByIdAsync);
        app.MapPut("api/basket/", UpdateBasketAsync);
        app.MapDelete("api/basket/{id}", DeleteBasketByIdAsync);
    }

    public void DefineServices(IServiceCollection services)
    {
        services.AddTransient<IBasketRepository, DaprBasketRepository>();
    }

    public async Task<IResult> GetBasketByIdAsync(string id, IBasketRepository repository)
    {
        var basket = await repository.GetBasketAsync(id);

        return Results.Ok(basket ?? new CustomerBasket(id));
    }

    public async Task<IResult> UpdateBasketAsync(CustomerBasket value, IBasketRepository repository)
    {
        return Results.Ok(await repository.UpdateBasketAsync(value));
    }

    public async Task<IResult> DeleteBasketByIdAsync(string id, IBasketRepository repository)
    {
        await repository.DeleteBasketAsync(id);
        return Results.Ok();
    }
}

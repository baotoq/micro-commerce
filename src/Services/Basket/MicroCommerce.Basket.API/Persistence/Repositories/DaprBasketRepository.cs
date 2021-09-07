using Dapr.Client;
using MicroCommerce.Basket.API.Models;
using MicroCommerce.Basket.API.Persistence.Repositories.Abstractions;

namespace MicroCommerce.Basket.API.Persistence.Repositories;

public class DaprBasketRepository : IBasketRepository
{
    private const string StoreName = "statestore";

    private readonly ILogger<DaprBasketRepository> _logger;
    private readonly DaprClient _daprClient;

    public DaprBasketRepository(ILogger<DaprBasketRepository> logger, DaprClient daprClient)
    {
        _logger = logger;
        _daprClient = daprClient;
    }

    public Task<CustomerBasket> GetBasketAsync(string buyerId)
    {
        return _daprClient.GetStateAsync<CustomerBasket>(StoreName, buyerId);
    }

    public async Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket)
    {
        var state = await _daprClient.GetStateEntryAsync<CustomerBasket>(StoreName, basket.BuyerId);
        state.Value = basket;

        await state.SaveAsync();

        _logger.LogInformation("Basket item persisted successfully");

        return await GetBasketAsync(basket.BuyerId);
    }

    public async Task DeleteBasketAsync(string id)
    {
        await _daprClient.DeleteStateAsync(StoreName, id);
    }
}

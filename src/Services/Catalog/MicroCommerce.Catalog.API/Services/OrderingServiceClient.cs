using System.Threading.Tasks;
using Dapr.Client;
using MicroCommerce.Catalog.API.IntegrationEvents;
using MicroCommerce.Shared.EventBus.Abstractions;

namespace MicroCommerce.Catalog.API.Services
{
    public class OrderingServiceClient : IOrderingServiceClient
    {
        private readonly DaprClient _daprClient;
        private readonly IEventBus _eventBus;

        public OrderingServiceClient(DaprClient daprClient, IEventBus eventBus)
        {
            _daprClient = daprClient;
            _eventBus = eventBus;
        }

        public async Task<object> SayHello()
        {
            var a = await _daprClient.InvokeMethodAsync<object>("ordering-api", "localApi");
            await _daprClient.InvokeMethodAsync("catalog-api", "health/readiness");
            await _daprClient.InvokeMethodAsync("basket-api", "health/readiness");


            for (int i = 0; i < 100; i++)
            {
                await _eventBus.PublishAsync(new ProductDeleted("test"));
                await _eventBus.PublishAsync(new ProductUpdated("test"));
            }

            return a;
        }
    }
}

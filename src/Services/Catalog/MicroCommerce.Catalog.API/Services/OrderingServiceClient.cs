using System.Threading.Tasks;
using Dapr.Client;
using MicroCommerce.Catalog.API.IntegrationEvents;
using MicroCommerce.Ordering.API;

namespace MicroCommerce.Catalog.API.Services
{
    public class OrderingServiceClient : IOrderingServiceClient
    {
        private readonly DaprClient _daprClient;

        public OrderingServiceClient(DaprClient daprClient)
        {
            _daprClient = daprClient;
        }

        public async Task<HelloReply> SayHello(HelloRequest request)
        {
            var a = await _daprClient.InvokeMethodAsync<HelloReply>("ordering-api", "localApi");
            await _daprClient.InvokeMethodAsync("catalog-api", "health/readiness");
            await _daprClient.InvokeMethodAsync("basket-api", "health/readiness");


            for (int i = 0; i < 100; i++)
            {
                await _daprClient.PublishEventAsync("pubsub", "product-deleted", new ProductDeleted("test"));
                await _daprClient.PublishEventAsync("pubsub", "product-updated", new ProductUpdated("test"));
            }

            return a;
        }
    }
}

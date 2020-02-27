using System.Threading.Tasks;
using Basket.API;

namespace Catalog.API.AppServices
{
    public class BasketClientService
    {
        private readonly Basket.API.Basket.BasketClient _basketClient;

        public BasketClientService(Basket.API.Basket.BasketClient basketClient)
        {
            _basketClient = basketClient;
        }

        public async Task SayHello()
        {
            await _basketClient.SayHelloAsync(new HelloRequest());
        }
    }
}

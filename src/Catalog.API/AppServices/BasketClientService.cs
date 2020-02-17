using Basket.API;

namespace Catalog.API.AppServices
{
    public class BasketClientService
    {
        private readonly Basket.API.Basket.BasketClient _basketClient;

        public BasketClientService(Basket.API.Basket.BasketClient basketClient)
        {
            _basketClient = basketClient;

            _basketClient.SayHello(new HelloRequest());
        }
    }
}

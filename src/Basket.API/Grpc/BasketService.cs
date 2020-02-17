using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Basket.API.Grpc
{
    public class BasketService : Basket.BasketBase
    {
        private readonly ILogger<BasketService> _logger;

        public BasketService()
        {
            _logger = NullLogger<BasketService>.Instance;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            _logger.LogInformation("{@request}", request);
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }
    }
}

using System;
using System.Threading.Tasks;
using Dapr.AppCallback.Autogen.Grpc.v1;
using Dapr.Client;
using Dapr.Client.Autogen.Grpc.v1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.Ordering.API.Services
{
    public class GreeterService : AppCallback.AppCallbackBase
    {
        private readonly ILogger<GreeterService> _logger;
        private readonly DaprClient _daprClient;

        public GreeterService(ILogger<GreeterService> logger, DaprClient daprClient)
        {
            _logger = logger;
            _daprClient = daprClient;
        }

        public override async Task<InvokeResponse> OnInvoke(InvokeRequest request, ServerCallContext context)
        {
            var response = new InvokeResponse();
            switch (request.Method)
            {
                case nameof(Greeter.GreeterBase.SayHello):
                    var input = request.Data.Unpack<HelloRequest>();
                    var output = await SayHello(input, context);
                    response.Data = Any.Pack(output);
                    break;
                default:
                    throw new InvalidOperationException();
            }
            return response;
        }

        public Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }
    }
}

﻿using System.Net.Http;
using System.Threading.Tasks;
using Dapr.Client;
using MicroCommerce.Ordering.API;

namespace MicroCommerce.Catalog.API.Services
{
    public interface IOrderingServiceClient
    {
        Task<HelloReply> SayHello(HelloRequest request);
    }
    
    public class OrderingServiceClient : IOrderingServiceClient
    {
        private readonly DaprClient _daprClient;

        public OrderingServiceClient(DaprClient daprClient)
        {
            _daprClient = daprClient;
        }

        public async Task<HelloReply> SayHello(HelloRequest request)
        {
            var a = await _daprClient.InvokeMethodAsync<HelloReply>(HttpMethod.Get, "ordering-api", "localApi");
            a = await _daprClient.InvokeMethodAsync<HelloReply>(HttpMethod.Get, "ordering-api", "localApi/2");

            return a;
        }
    }
}

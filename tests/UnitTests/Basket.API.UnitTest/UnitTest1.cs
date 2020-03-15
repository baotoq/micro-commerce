using System;
using System.Threading.Tasks;
using Basket.API.Grpc;
using FluentAssertions;
using Xunit;

namespace Basket.API.UnitTest
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
        {
            var sut = new BasketService();

            var act = await sut.SayHello(new HelloRequest
            {
                Name = "Bao"
            }, null);

            act.Message.Should().Be("Hello Bao");
        }
    }
}

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bshop.V1.Identity;
using Catalog.API.Application.Orders.Commands;
using Catalog.API.Application.Orders.Models;
using Catalog.API.Data;
using Catalog.API.Data.Models;
using Catalog.API.FunctionalTests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shared.Testings;
using UnitOfWork.Common;
using Xunit;

namespace Catalog.API.FunctionalTests
{
    public class OrdersApiTests : IClassFixture<TestWebApplicationFactory<Startup>>
    {
        private const string Uri = "api/orders";
        private readonly TestWebApplicationFactory<Startup> _factory;

        public OrdersApiTests(TestWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Find_Success()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(async services =>
            {
                using var scope = services.BuildServiceProvider().CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.Orders.AddAsync(new Order
                {
                    OrderStatus = OrderStatus.New,
                    CustomerId = MasterData.CurrentUserId,
                    OrderItems = new List<OrderItem> { new OrderItem() }
                });
                await context.SaveChangesAsync();

                var identityClientMock = new Mock<IdentityService.IdentityServiceClient>();
                identityClientMock.Setup(s => s.GetUsersByIdsAsync(It.IsAny<GetUsersByIdsRequest>(),
                        null, null, It.IsAny<CancellationToken>()))
                    .Returns(GrpcTestCalls.AsyncUnaryCall(new GetUsersByIdsResponse
                    {
                        Users = { new GetUsersByIdsResponse.Types.User { Id = MasterData.CurrentUserId, Email = "test@gmail.com", UserName = "test@gmail.com" } }
                    }));

                services.AddSingleton(identityClientMock.Object);
            })).CreateClient();

            // Act
            var response = await client.GetAsync(Uri);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<OffsetPaged<OrderDto>>();
            result.Data.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Update_Authenticated_Success()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(async services =>
            {
                using var scope = services.BuildServiceProvider().CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.Orders.AddAsync(new Order
                {
                    OrderStatus = OrderStatus.New
                });
                await context.SaveChangesAsync();
            })).CreateAuthenticatedClient();
            var command = new ChangeOrderStatusCommand { OrderStatus = OrderStatus.Completed };

            // Act
            var response = await client.PutAsJsonAsync($"{Uri}/1/change-status", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}

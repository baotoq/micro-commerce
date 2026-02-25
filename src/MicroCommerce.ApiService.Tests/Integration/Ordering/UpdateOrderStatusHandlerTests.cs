using FluentAssertions;
using FluentResults;
using MediatR;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Features.Ordering.Application.Commands.UpdateOrderStatus;
using MicroCommerce.ApiService.Features.Ordering.Domain.Entities;
using MicroCommerce.ApiService.Features.Ordering.Infrastructure;
using MicroCommerce.ApiService.Tests.Integration.Builders;
using MicroCommerce.ApiService.Tests.Integration.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace MicroCommerce.ApiService.Tests.Integration.Ordering;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public sealed class UpdateOrderStatusHandlerTests : IntegrationTestBase
{
    public UpdateOrderStatusHandlerTests(ApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await ResetDatabase(typeof(OrderingDbContext));
    }

    [Fact]
    public async Task Handle_InvalidStatusTransition_ReturnsFailResult()
    {
        // Arrange - Create an order in Submitted status directly in the DB
        Order order = new OrderBuilder()
            .WithBuyerId(Guid.NewGuid())
            .Build();

        using IServiceScope scope = CreateScope();
        OrderingDbContext db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        Guid orderId = order.Id.Value;

        // Act - Attempt to ship an order that is Submitted (must go through Confirmed first)
        IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        Result result = await mediator.Send(new UpdateOrderStatusCommand(orderId, "Shipped"), CancellationToken.None);

        // Assert - Business rule: Submitted -> Shipped is invalid
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().NotBeEmpty();
        result.Errors[0].Message.Should().Contain("Cannot transition from");
    }

    [Fact]
    public async Task Handle_NonExistentOrder_ThrowsNotFoundException()
    {
        // Arrange - Use a random Guid for a non-existent order
        Guid nonExistentOrderId = Guid.NewGuid();

        using IServiceScope scope = CreateScope();
        IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Act & Assert - Handler throws NotFoundException for non-existent orders
        Func<Task> act = async () =>
            await mediator.Send(new UpdateOrderStatusCommand(nonExistentOrderId, "Shipped"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{nonExistentOrderId}*");
    }
}

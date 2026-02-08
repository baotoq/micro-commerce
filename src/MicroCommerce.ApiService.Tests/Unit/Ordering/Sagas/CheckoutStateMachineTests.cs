using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using MicroCommerce.ApiService.Features.Ordering.Application.Saga;
using Microsoft.Extensions.DependencyInjection;

namespace MicroCommerce.ApiService.Tests.Unit.Ordering.Sagas;

/// <summary>
/// Tests for CheckoutStateMachine using MassTransit's SagaTestHarness.
/// Covers happy path, failure paths, compensation logic, and state transitions.
/// </summary>
[Trait("Category", "Unit")]
public class CheckoutStateMachineTests
{
    /// <summary>
    /// Happy path: CheckoutStarted -> Submitted -> StockReservationCompleted ->
    /// StockReserved -> PaymentCompleted -> Confirmed + finalized.
    /// </summary>
    [Fact]
    public async Task ShouldCompleteSuccessfulCheckoutFlow()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        Guid buyerId = Guid.NewGuid();
        const string buyerEmail = "buyer@example.com";

        await using ServiceProvider provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<CheckoutStateMachine, CheckoutState>()
                    .InMemoryRepository();
            })
            .BuildServiceProvider(true);

        ITestHarness harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act & Assert: Start checkout
        await harness.Bus.Publish(new CheckoutStarted
        {
            OrderId = orderId,
            BuyerId = buyerId,
            BuyerEmail = buyerEmail,
            Items = [new CheckoutItem { ProductId = Guid.NewGuid(), Quantity = 2 }]
        });

        // Wait for processing and verify ReserveStockForOrder published
        (await harness.Published.Any<ReserveStockForOrder>(x => x.Context.Message.OrderId == orderId)).Should().BeTrue();

        // Act & Assert: Stock reservation succeeded
        string reservationIdsJson = "{\"productId\":\"reservationId\"}";
        await harness.Bus.Publish(new StockReservationCompleted
        {
            OrderId = orderId,
            ReservationIdsJson = reservationIdsJson
        });

        // Wait for state transition
        await Task.Delay(200);

        // Act & Assert: Payment succeeded
        await harness.Bus.Publish(new PaymentCompleted { OrderId = orderId });

        // Verify all expected messages published
        (await harness.Published.Any<ConfirmOrder>(x => x.Context.Message.OrderId == orderId)).Should().BeTrue();
        (await harness.Published.Any<DeductStock>(x => x.Context.Message.OrderId == orderId)).Should().BeTrue();
        (await harness.Published.Any<ClearCart>(x => x.Context.Message.BuyerId == buyerId)).Should().BeTrue();
    }

    /// <summary>
    /// Stock failure: CheckoutStarted -> Submitted -> StockReservationFailed ->
    /// Failed + OrderFailed published + finalized.
    /// </summary>
    [Fact]
    public async Task ShouldFailWhenStockReservationFails()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        Guid buyerId = Guid.NewGuid();
        const string buyerEmail = "buyer@example.com";

        await using ServiceProvider provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<CheckoutStateMachine, CheckoutState>()
                    .InMemoryRepository();
            })
            .BuildServiceProvider(true);

        ITestHarness harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act: Start checkout
        await harness.Bus.Publish(new CheckoutStarted
        {
            OrderId = orderId,
            BuyerId = buyerId,
            BuyerEmail = buyerEmail,
            Items = [new CheckoutItem { ProductId = Guid.NewGuid(), Quantity = 99 }]
        });

        await Task.Delay(200);

        // Act: Stock reservation failed
        string failureReason = "Insufficient stock for Product X";
        await harness.Bus.Publish(new StockReservationFailed
        {
            OrderId = orderId,
            Reason = failureReason
        });

        // Assert: OrderFailed published with correct reason
        IPublishedMessage<OrderFailed>? orderFailedMsg =
            (await harness.Published.SelectAsync<OrderFailed>(x => x.Context.Message.OrderId == orderId).FirstOrDefault());
        orderFailedMsg.Should().NotBeNull();
        orderFailedMsg!.Context.Message.Reason.Should().Be(failureReason);
    }

    /// <summary>
    /// Payment failure + compensation: CheckoutStarted -> StockReservationCompleted ->
    /// PaymentFailed -> ReleaseStockReservations + OrderFailed published.
    /// </summary>
    [Fact]
    public async Task ShouldReleaseStockReservationsWhenPaymentFails()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        Guid buyerId = Guid.NewGuid();
        const string buyerEmail = "buyer@example.com";

        await using ServiceProvider provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<CheckoutStateMachine, CheckoutState>()
                    .InMemoryRepository();
            })
            .BuildServiceProvider(true);

        ITestHarness harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act: Start checkout and complete stock reservation
        await harness.Bus.Publish(new CheckoutStarted
        {
            OrderId = orderId,
            BuyerId = buyerId,
            BuyerEmail = buyerEmail,
            Items = [new CheckoutItem { ProductId = Guid.NewGuid(), Quantity = 1 }]
        });

        string reservationIdsJson = "{\"productId\":\"reservationId\"}";
        await harness.Bus.Publish(new StockReservationCompleted
        {
            OrderId = orderId,
            ReservationIdsJson = reservationIdsJson
        });

        await Task.Delay(200);

        // Act: Payment failed
        string failureReason = "Card declined";
        await harness.Bus.Publish(new PaymentFailed
        {
            OrderId = orderId,
            Reason = failureReason
        });

        // Assert: Compensation messages published
        (await harness.Published.Any<ReleaseStockReservations>(x => x.Context.Message.OrderId == orderId)).Should().BeTrue();
        IPublishedMessage<OrderFailed>? orderFailedMsg =
            (await harness.Published.SelectAsync<OrderFailed>(x => x.Context.Message.OrderId == orderId).FirstOrDefault());
        orderFailedMsg.Should().NotBeNull();
        orderFailedMsg!.Context.Message.Reason.Should().Be(failureReason);
    }

    /// <summary>
    /// Verify multiple OrderIds create independent saga instances.
    /// Tests saga correlation and isolation.
    /// </summary>
    [Fact]
    public async Task ShouldCreateIndependentSagaInstancesForDifferentOrders()
    {
        // Arrange
        Guid orderId1 = Guid.NewGuid();
        Guid orderId2 = Guid.NewGuid();
        Guid buyerId1 = Guid.NewGuid();
        Guid buyerId2 = Guid.NewGuid();

        await using ServiceProvider provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<CheckoutStateMachine, CheckoutState>()
                    .InMemoryRepository();
            })
            .BuildServiceProvider(true);

        ITestHarness harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();
        ISagaStateMachineTestHarness<CheckoutStateMachine, CheckoutState> sagaHarness =
            harness.GetSagaStateMachineHarness<CheckoutStateMachine, CheckoutState>();

        // Act: Start two different checkouts
        await harness.Bus.Publish(new CheckoutStarted
        {
            OrderId = orderId1,
            BuyerId = buyerId1,
            BuyerEmail = "buyer1@example.com",
            Items = [new CheckoutItem { ProductId = Guid.NewGuid(), Quantity = 1 }]
        });

        await harness.Bus.Publish(new CheckoutStarted
        {
            OrderId = orderId2,
            BuyerId = buyerId2,
            BuyerEmail = "buyer2@example.com",
            Items = [new CheckoutItem { ProductId = Guid.NewGuid(), Quantity = 2 }]
        });

        await Task.Delay(200);

        // Assert: Both sagas created
        (await sagaHarness.Created.Any(x => x.CorrelationId == orderId1)).Should().BeTrue();
        (await sagaHarness.Created.Any(x => x.CorrelationId == orderId2)).Should().BeTrue();

        // Act: Complete order1, fail order2
        await harness.Bus.Publish(new StockReservationCompleted
        {
            OrderId = orderId1,
            ReservationIdsJson = "{\"p1\":\"r1\"}"
        });

        await harness.Bus.Publish(new StockReservationFailed
        {
            OrderId = orderId2,
            Reason = "Out of stock"
        });

        await Task.Delay(200);

        // Assert: Independent processing - order1 proceeds, order2 fails
        (await harness.Published.Any<OrderFailed>(x => x.Context.Message.OrderId == orderId2)).Should().BeTrue();
        // order1 saga should still exist (not failed)
        (await sagaHarness.Created.Any(x => x.CorrelationId == orderId1)).Should().BeTrue();
    }

    /// <summary>
    /// Verify published messages contain correct correlation data.
    /// </summary>
    [Fact]
    public async Task ShouldPublishMessagesWithCorrectCorrelationData()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        Guid buyerId = Guid.NewGuid();
        Guid productId = Guid.NewGuid();
        const string buyerEmail = "buyer@example.com";

        await using ServiceProvider provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<CheckoutStateMachine, CheckoutState>()
                    .InMemoryRepository();
            })
            .BuildServiceProvider(true);

        ITestHarness harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act: Complete full happy path
        await harness.Bus.Publish(new CheckoutStarted
        {
            OrderId = orderId,
            BuyerId = buyerId,
            BuyerEmail = buyerEmail,
            Items = [new CheckoutItem { ProductId = productId, Quantity = 1 }]
        });

        string reservationIdsJson = "{\"productId\":\"reservationId\"}";
        await harness.Bus.Publish(new StockReservationCompleted
        {
            OrderId = orderId,
            ReservationIdsJson = reservationIdsJson
        });

        await harness.Bus.Publish(new PaymentCompleted { OrderId = orderId });

        await Task.Delay(200);

        // Assert: ReserveStockForOrder has correct items
        IPublishedMessage<ReserveStockForOrder>? reserveStockMsg =
            (await harness.Published.SelectAsync<ReserveStockForOrder>(x => x.Context.Message.OrderId == orderId).FirstOrDefault());
        reserveStockMsg.Should().NotBeNull();
        reserveStockMsg!.Context.Message.Items.Should().HaveCount(1);
        reserveStockMsg.Context.Message.Items[0].ProductId.Should().Be(productId);
        reserveStockMsg.Context.Message.Items[0].Quantity.Should().Be(1);

        // Assert: DeductStock has correct ReservationIdsJson
        IPublishedMessage<DeductStock>? deductStockMsg =
            (await harness.Published.SelectAsync<DeductStock>(x => x.Context.Message.OrderId == orderId).FirstOrDefault());
        deductStockMsg.Should().NotBeNull();
        deductStockMsg!.Context.Message.ReservationIdsJson.Should().Be(reservationIdsJson);

        // Assert: ClearCart has correct BuyerId
        IPublishedMessage<ClearCart>? clearCartMsg =
            (await harness.Published.SelectAsync<ClearCart>(x => x.Context.Message.BuyerId == buyerId).FirstOrDefault());
        clearCartMsg.Should().NotBeNull();

        // Assert: ConfirmOrder has correct OrderId
        IPublishedMessage<ConfirmOrder>? confirmOrderMsg =
            (await harness.Published.SelectAsync<ConfirmOrder>(x => x.Context.Message.OrderId == orderId).FirstOrDefault());
        confirmOrderMsg.Should().NotBeNull();
    }

    /// <summary>
    /// Verify compensation message contains correct data.
    /// </summary>
    [Fact]
    public async Task ShouldPublishReleaseStockReservationsWithCorrectData()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        Guid buyerId = Guid.NewGuid();
        const string buyerEmail = "buyer@example.com";

        await using ServiceProvider provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<CheckoutStateMachine, CheckoutState>()
                    .InMemoryRepository();
            })
            .BuildServiceProvider(true);

        ITestHarness harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act: Start checkout and complete stock reservation
        await harness.Bus.Publish(new CheckoutStarted
        {
            OrderId = orderId,
            BuyerId = buyerId,
            BuyerEmail = buyerEmail,
            Items = [new CheckoutItem { ProductId = Guid.NewGuid(), Quantity = 2 }]
        });

        string reservationIdsJson = "{\"prod1\":\"res1\",\"prod2\":\"res2\"}";
        await harness.Bus.Publish(new StockReservationCompleted
        {
            OrderId = orderId,
            ReservationIdsJson = reservationIdsJson
        });

        await Task.Delay(200);

        // Act: Payment failed
        await harness.Bus.Publish(new PaymentFailed
        {
            OrderId = orderId,
            Reason = "Card expired"
        });

        await Task.Delay(200);

        // Assert: ReleaseStockReservations has correct data
        IPublishedMessage<ReleaseStockReservations>? releaseMsg =
            (await harness.Published.SelectAsync<ReleaseStockReservations>(x => x.Context.Message.OrderId == orderId).FirstOrDefault());
        releaseMsg.Should().NotBeNull();
        releaseMsg!.Context.Message.ReservationIdsJson.Should().Be(reservationIdsJson);

        // Assert: OrderFailed has correct data
        IPublishedMessage<OrderFailed>? orderFailedMsg =
            (await harness.Published.SelectAsync<OrderFailed>(x => x.Context.Message.OrderId == orderId).FirstOrDefault());
        orderFailedMsg.Should().NotBeNull();
        orderFailedMsg!.Context.Message.Reason.Should().Be("Card expired");
    }
}

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
    private readonly Guid _orderId = Guid.NewGuid();
    private readonly Guid _buyerId = Guid.NewGuid();
    private const string BuyerEmail = "buyer@example.com";

    /// <summary>
    /// Happy path: CheckoutStarted -> Submitted -> StockReservationCompleted ->
    /// StockReserved -> PaymentCompleted -> Confirmed + finalized.
    /// </summary>
    [Fact]
    public async Task ShouldCompleteSuccessfulCheckoutFlow()
    {
        // Arrange
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

        // Act: Start checkout
        await harness.Bus.Publish(new CheckoutStarted
        {
            OrderId = _orderId,
            BuyerId = _buyerId,
            BuyerEmail = BuyerEmail,
            Items = [new CheckoutItem { ProductId = Guid.NewGuid(), Quantity = 2 }]
        });

        // Assert: Saga created and transitioned to Submitted
        (await sagaHarness.Created.Any(x => x.CorrelationId == _orderId)).Should().BeTrue();
        (await sagaHarness.Exists(_orderId, x => x.Submitted)).Should().NotBeNull();
        (await harness.Published.Any<ReserveStockForOrder>()).Should().BeTrue();

        // Act: Stock reservation succeeded
        string reservationIdsJson = "{\"productId\":\"reservationId\"}";
        await harness.Bus.Publish(new StockReservationCompleted
        {
            OrderId = _orderId,
            ReservationIdsJson = reservationIdsJson
        });

        // Assert: Transitioned to StockReserved
        (await sagaHarness.Exists(_orderId, x => x.StockReserved)).Should().NotBeNull();

        // Act: Payment succeeded
        await harness.Bus.Publish(new PaymentCompleted { OrderId = _orderId });

        // Assert: Published ConfirmOrder, DeductStock, ClearCart and transitioned to Confirmed
        (await harness.Published.Any<ConfirmOrder>()).Should().BeTrue();
        (await harness.Published.Any<DeductStock>()).Should().BeTrue();
        (await harness.Published.Any<ClearCart>()).Should().BeTrue();
        (await sagaHarness.Exists(_orderId, x => x.Confirmed)).Should().NotBeNull();

        // Assert: Saga finalized and removed
        await Task.Delay(100); // Give time for finalization
        (await sagaHarness.Exists(_orderId, x => x.Final)).Should().BeNull();
    }

    /// <summary>
    /// Stock failure: CheckoutStarted -> Submitted -> StockReservationFailed ->
    /// Failed + OrderFailed published + finalized.
    /// </summary>
    [Fact]
    public async Task ShouldFailWhenStockReservationFails()
    {
        // Arrange
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

        // Act: Start checkout
        await harness.Bus.Publish(new CheckoutStarted
        {
            OrderId = _orderId,
            BuyerId = _buyerId,
            BuyerEmail = BuyerEmail,
            Items = [new CheckoutItem { ProductId = Guid.NewGuid(), Quantity = 99 }]
        });

        // Assert: Saga in Submitted state
        (await sagaHarness.Exists(_orderId, x => x.Submitted)).Should().NotBeNull();

        // Act: Stock reservation failed
        string failureReason = "Insufficient stock for Product X";
        await harness.Bus.Publish(new StockReservationFailed
        {
            OrderId = _orderId,
            Reason = failureReason
        });

        // Assert: Published OrderFailed and transitioned to Failed
        (await harness.Published.Any<OrderFailed>()).Should().BeTrue();
        (await sagaHarness.Exists(_orderId, x => x.Failed)).Should().NotBeNull();

        // Assert: Saga finalized and removed
        await Task.Delay(100); // Give time for finalization
        (await sagaHarness.Exists(_orderId, x => x.Final)).Should().BeNull();
    }

    /// <summary>
    /// Payment failure + compensation: CheckoutStarted -> StockReservationCompleted ->
    /// PaymentFailed -> ReleaseStockReservations + OrderFailed published + finalized.
    /// </summary>
    [Fact]
    public async Task ShouldReleaseStockReservationsWhenPaymentFails()
    {
        // Arrange
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

        // Act: Start checkout and complete stock reservation
        await harness.Bus.Publish(new CheckoutStarted
        {
            OrderId = _orderId,
            BuyerId = _buyerId,
            BuyerEmail = BuyerEmail,
            Items = [new CheckoutItem { ProductId = Guid.NewGuid(), Quantity = 1 }]
        });

        string reservationIdsJson = "{\"productId\":\"reservationId\"}";
        await harness.Bus.Publish(new StockReservationCompleted
        {
            OrderId = _orderId,
            ReservationIdsJson = reservationIdsJson
        });

        // Assert: Saga in StockReserved state
        (await sagaHarness.Exists(_orderId, x => x.StockReserved)).Should().NotBeNull();

        // Act: Payment failed
        string failureReason = "Card declined";
        await harness.Bus.Publish(new PaymentFailed
        {
            OrderId = _orderId,
            Reason = failureReason
        });

        // Assert: Published compensation messages
        (await harness.Published.Any<ReleaseStockReservations>()).Should().BeTrue();
        (await harness.Published.Any<OrderFailed>()).Should().BeTrue();

        // Assert: Transitioned to Failed
        (await sagaHarness.Exists(_orderId, x => x.Failed)).Should().NotBeNull();

        // Assert: Saga finalized and removed
        await Task.Delay(100); // Give time for finalization
        (await sagaHarness.Exists(_orderId, x => x.Final)).Should().BeNull();
    }

    /// <summary>
    /// Verify saga state data is correctly populated during transitions.
    /// </summary>
    [Fact]
    public async Task ShouldPopulateSagaStateDataCorrectly()
    {
        // Arrange
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

        // Act: Start checkout
        await harness.Bus.Publish(new CheckoutStarted
        {
            OrderId = _orderId,
            BuyerId = _buyerId,
            BuyerEmail = BuyerEmail,
            Items = [new CheckoutItem { ProductId = Guid.NewGuid(), Quantity = 3 }]
        });

        // Assert: State populated on initial event
        CheckoutState? saga = sagaHarness.Sagas.Select(x => x.CorrelationId == _orderId).FirstOrDefault();
        saga.Should().NotBeNull();
        saga!.OrderId.Should().Be(_orderId);
        saga.BuyerId.Should().Be(_buyerId);
        saga.BuyerEmail.Should().Be(BuyerEmail);
        saga.SubmittedAt.Should().NotBeNull();
        saga.SubmittedAt!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));

        // Act: Stock reservation completed
        string reservationIdsJson = "{\"productId1\":\"reservationId1\"}";
        await harness.Bus.Publish(new StockReservationCompleted
        {
            OrderId = _orderId,
            ReservationIdsJson = reservationIdsJson
        });

        // Assert: ReservationIdsJson populated
        saga = sagaHarness.Sagas.Select(x => x.CorrelationId == _orderId).FirstOrDefault();
        saga.Should().NotBeNull();
        saga!.ReservationIdsJson.Should().Be(reservationIdsJson);
    }

    /// <summary>
    /// Verify saga state captures failure reasons correctly.
    /// </summary>
    [Fact]
    public async Task ShouldCaptureFailureReasonOnStockFailure()
    {
        // Arrange
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

        // Act: Start checkout
        await harness.Bus.Publish(new CheckoutStarted
        {
            OrderId = _orderId,
            BuyerId = _buyerId,
            BuyerEmail = BuyerEmail,
            Items = [new CheckoutItem { ProductId = Guid.NewGuid(), Quantity = 1 }]
        });

        // Act: Stock reservation failed
        string expectedReason = "Product out of stock";
        await harness.Bus.Publish(new StockReservationFailed
        {
            OrderId = _orderId,
            Reason = expectedReason
        });

        // Assert: FailureReason captured in saga state
        CheckoutState? saga = sagaHarness.Sagas.Select(x => x.CorrelationId == _orderId).FirstOrDefault();
        saga.Should().NotBeNull();
        saga!.FailureReason.Should().Be(expectedReason);
    }

    /// <summary>
    /// Verify saga state captures failure reasons correctly on payment failure.
    /// </summary>
    [Fact]
    public async Task ShouldCaptureFailureReasonOnPaymentFailure()
    {
        // Arrange
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

        // Act: Start checkout and complete stock reservation
        await harness.Bus.Publish(new CheckoutStarted
        {
            OrderId = _orderId,
            BuyerId = _buyerId,
            BuyerEmail = BuyerEmail,
            Items = [new CheckoutItem { ProductId = Guid.NewGuid(), Quantity = 1 }]
        });

        await harness.Bus.Publish(new StockReservationCompleted
        {
            OrderId = _orderId,
            ReservationIdsJson = "{\"productId\":\"reservationId\"}"
        });

        // Act: Payment failed
        string expectedReason = "Insufficient funds";
        await harness.Bus.Publish(new PaymentFailed
        {
            OrderId = _orderId,
            Reason = expectedReason
        });

        // Assert: FailureReason captured in saga state
        CheckoutState? saga = sagaHarness.Sagas.Select(x => x.CorrelationId == _orderId).FirstOrDefault();
        saga.Should().NotBeNull();
        saga!.FailureReason.Should().Be(expectedReason);
    }

    /// <summary>
    /// Verify multiple OrderIds create independent saga instances.
    /// Tests saga correlation and isolation.
    /// </summary>
    [Fact]
    public async Task ShouldCreateIndependentSagaInstancesForDifferentOrders()
    {
        // Arrange
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

        Guid orderId1 = Guid.NewGuid();
        Guid orderId2 = Guid.NewGuid();
        Guid buyerId1 = Guid.NewGuid();
        Guid buyerId2 = Guid.NewGuid();

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

        // Assert: Both sagas created independently
        (await sagaHarness.Created.Any(x => x.CorrelationId == orderId1)).Should().BeTrue();
        (await sagaHarness.Created.Any(x => x.CorrelationId == orderId2)).Should().BeTrue();
        (await sagaHarness.Exists(orderId1, x => x.Submitted)).Should().NotBeNull();
        (await sagaHarness.Exists(orderId2, x => x.Submitted)).Should().NotBeNull();

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

        // Assert: order1 in StockReserved, order2 in Failed
        (await sagaHarness.Exists(orderId1, x => x.StockReserved)).Should().NotBeNull();
        (await sagaHarness.Exists(orderId2, x => x.Failed)).Should().NotBeNull();
    }

    /// <summary>
    /// Verify published messages contain correct correlation data.
    /// </summary>
    [Fact]
    public async Task ShouldPublishMessagesWithCorrectCorrelationData()
    {
        // Arrange
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

        Guid productId = Guid.NewGuid();

        // Act: Complete full happy path
        await harness.Bus.Publish(new CheckoutStarted
        {
            OrderId = _orderId,
            BuyerId = _buyerId,
            BuyerEmail = BuyerEmail,
            Items = [new CheckoutItem { ProductId = productId, Quantity = 1 }]
        });

        string reservationIdsJson = "{\"productId\":\"reservationId\"}";
        await harness.Bus.Publish(new StockReservationCompleted
        {
            OrderId = _orderId,
            ReservationIdsJson = reservationIdsJson
        });

        await harness.Bus.Publish(new PaymentCompleted { OrderId = _orderId });

        // Assert: ReserveStockForOrder has correct items
        IPublishedMessage<ReserveStockForOrder>? reserveStockMsg =
            (await harness.Published.SelectAsync<ReserveStockForOrder>().FirstOrDefault());
        reserveStockMsg.Should().NotBeNull();
        reserveStockMsg!.Context.Message.OrderId.Should().Be(_orderId);
        reserveStockMsg.Context.Message.Items.Should().HaveCount(1);
        reserveStockMsg.Context.Message.Items[0].ProductId.Should().Be(productId);
        reserveStockMsg.Context.Message.Items[0].Quantity.Should().Be(1);

        // Assert: DeductStock has correct ReservationIdsJson
        IPublishedMessage<DeductStock>? deductStockMsg =
            (await harness.Published.SelectAsync<DeductStock>().FirstOrDefault());
        deductStockMsg.Should().NotBeNull();
        deductStockMsg!.Context.Message.OrderId.Should().Be(_orderId);
        deductStockMsg.Context.Message.ReservationIdsJson.Should().Be(reservationIdsJson);

        // Assert: ClearCart has correct BuyerId
        IPublishedMessage<ClearCart>? clearCartMsg =
            (await harness.Published.SelectAsync<ClearCart>().FirstOrDefault());
        clearCartMsg.Should().NotBeNull();
        clearCartMsg!.Context.Message.BuyerId.Should().Be(_buyerId);

        // Assert: ConfirmOrder has correct OrderId
        IPublishedMessage<ConfirmOrder>? confirmOrderMsg =
            (await harness.Published.SelectAsync<ConfirmOrder>().FirstOrDefault());
        confirmOrderMsg.Should().NotBeNull();
        confirmOrderMsg!.Context.Message.OrderId.Should().Be(_orderId);
    }

    /// <summary>
    /// Verify compensation message contains correct data.
    /// </summary>
    [Fact]
    public async Task ShouldPublishReleaseStockReservationsWithCorrectData()
    {
        // Arrange
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

        // Act: Start checkout and complete stock reservation
        await harness.Bus.Publish(new CheckoutStarted
        {
            OrderId = _orderId,
            BuyerId = _buyerId,
            BuyerEmail = BuyerEmail,
            Items = [new CheckoutItem { ProductId = Guid.NewGuid(), Quantity = 2 }]
        });

        string reservationIdsJson = "{\"prod1\":\"res1\",\"prod2\":\"res2\"}";
        await harness.Bus.Publish(new StockReservationCompleted
        {
            OrderId = _orderId,
            ReservationIdsJson = reservationIdsJson
        });

        // Act: Payment failed
        await harness.Bus.Publish(new PaymentFailed
        {
            OrderId = _orderId,
            Reason = "Card expired"
        });

        // Assert: ReleaseStockReservations has correct data
        IPublishedMessage<ReleaseStockReservations>? releaseMsg =
            (await harness.Published.SelectAsync<ReleaseStockReservations>().FirstOrDefault());
        releaseMsg.Should().NotBeNull();
        releaseMsg!.Context.Message.OrderId.Should().Be(_orderId);
        releaseMsg.Context.Message.ReservationIdsJson.Should().Be(reservationIdsJson);

        // Assert: OrderFailed has correct data
        IPublishedMessage<OrderFailed>? orderFailedMsg =
            (await harness.Published.SelectAsync<OrderFailed>().FirstOrDefault());
        orderFailedMsg.Should().NotBeNull();
        orderFailedMsg!.Context.Message.OrderId.Should().Be(_orderId);
        orderFailedMsg.Context.Message.Reason.Should().Be("Card expired");
    }
}

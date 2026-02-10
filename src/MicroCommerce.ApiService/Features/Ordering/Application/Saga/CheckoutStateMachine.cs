using MassTransit;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Saga;

/// <summary>
/// MassTransit state machine orchestrating the checkout flow.
/// Flow: CheckoutStarted -> reserve stock -> await payment -> confirm/fail.
/// Compensation: payment failure releases all stock reservations.
/// </summary>
public sealed class CheckoutStateMachine : MassTransitStateMachine<CheckoutState>
{
    // States
    public State Submitted { get; private set; } = null!;
    public State StockReserved { get; private set; } = null!;
    public State Failed { get; private set; } = null!;
    public State Confirmed { get; private set; } = null!;

    // Events
    public Event<CheckoutStarted> CheckoutStartedEvent { get; private set; } = null!;
    public Event<StockReservationCompleted> StockReservationCompletedEvent { get; private set; } = null!;
    public Event<StockReservationFailed> StockReservationFailedEvent { get; private set; } = null!;
    public Event<PaymentCompleted> PaymentCompletedEvent { get; private set; } = null!;
    public Event<PaymentFailed> PaymentFailedEvent { get; private set; } = null!;

    public CheckoutStateMachine()
    {
        InstanceState(x => x.CurrentState);

        // Correlate all events by OrderId
        Event(() => CheckoutStartedEvent, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => StockReservationCompletedEvent, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => StockReservationFailedEvent, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => PaymentCompletedEvent, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => PaymentFailedEvent, x => x.CorrelateById(ctx => ctx.Message.OrderId));

        // Initial state: checkout started -> reserve stock
        Initially(
            When(CheckoutStartedEvent)
                .Then(ctx =>
                {
                    ctx.Saga.OrderId = ctx.Message.OrderId;
                    ctx.Saga.BuyerId = ctx.Message.BuyerId;
                    ctx.Saga.BuyerEmail = ctx.Message.BuyerEmail;
                    ctx.Saga.SubmittedAt = DateTimeOffset.UtcNow;
                })
                .PublishAsync(ctx => ctx.Init<ReserveStockForOrder>(new
                {
                    ctx.Message.OrderId,
                    ctx.Message.Items
                }))
                .TransitionTo(Submitted));

        // Submitted: waiting for stock reservation result
        During(Submitted,
            When(StockReservationCompletedEvent)
                .Then(ctx => ctx.Saga.ReservationIdsJson = ctx.Message.ReservationIdsJson)
                .TransitionTo(StockReserved),
            When(StockReservationFailedEvent)
                .Then(ctx => ctx.Saga.FailureReason = ctx.Message.Reason)
                .PublishAsync(ctx => ctx.Init<OrderFailed>(new
                {
                    ctx.Saga.OrderId,
                    Reason = ctx.Message.Reason
                }))
                .TransitionTo(Failed)
                .Finalize());

        // StockReserved: waiting for payment result
        During(StockReserved,
            When(PaymentCompletedEvent)
                .PublishAsync(ctx => ctx.Init<ConfirmOrder>(new { ctx.Saga.OrderId }))
                .PublishAsync(ctx => ctx.Init<DeductStock>(new
                {
                    ctx.Saga.OrderId,
                    ctx.Saga.ReservationIdsJson
                }))
                .PublishAsync(ctx => ctx.Init<ClearCart>(new { ctx.Saga.BuyerId }))
                .TransitionTo(Confirmed)
                .Finalize(),
            When(PaymentFailedEvent)
                .Then(ctx => ctx.Saga.FailureReason = ctx.Message.Reason)
                // Compensation: release all stock reservations
                .PublishAsync(ctx => ctx.Init<ReleaseStockReservations>(new
                {
                    ctx.Saga.OrderId,
                    ctx.Saga.ReservationIdsJson
                }))
                .PublishAsync(ctx => ctx.Init<OrderFailed>(new
                {
                    ctx.Saga.OrderId,
                    Reason = ctx.Message.Reason
                }))
                .TransitionTo(Failed)
                .Finalize());

        SetCompletedWhenFinalized();
    }
}

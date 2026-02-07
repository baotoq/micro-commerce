using MassTransit;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Saga;

/// <summary>
/// Saga state instance for the checkout state machine.
/// Persisted in the ordering schema via EF Core with optimistic concurrency.
/// CorrelationId equals OrderId for saga correlation.
/// </summary>
public class CheckoutState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;
    public Guid OrderId { get; set; }
    public Guid BuyerId { get; set; }
    public string? BuyerEmail { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public string? FailureReason { get; set; }

    /// <summary>
    /// Serialized Dictionary&lt;Guid, Guid&gt; (ProductId -> ReservationId) for compensation.
    /// </summary>
    public string? ReservationIdsJson { get; set; }

    /// <summary>
    /// PostgreSQL xmin optimistic concurrency token.
    /// </summary>
    public uint RowVersion { get; set; }
}

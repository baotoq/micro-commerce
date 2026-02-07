namespace MicroCommerce.ApiService.Features.Ordering;

/// <summary>
/// MassTransit message contracts for the ordering saga.
/// Published by command handlers, consumed by saga state machine.
/// </summary>
public sealed record PaymentCompleted
{
    public Guid OrderId { get; init; }
}

public sealed record PaymentFailed
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

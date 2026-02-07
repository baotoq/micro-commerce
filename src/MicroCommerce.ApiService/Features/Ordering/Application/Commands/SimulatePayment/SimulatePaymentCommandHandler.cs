using MassTransit;
using MediatR;
using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Ordering.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Commands.SimulatePayment;

public sealed class SimulatePaymentCommandHandler
    : IRequestHandler<SimulatePaymentCommand, SimulatePaymentResult>
{
    private readonly OrderingDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public SimulatePaymentCommandHandler(OrderingDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<SimulatePaymentResult> Handle(
        SimulatePaymentCommand request,
        CancellationToken cancellationToken)
    {
        OrderId orderId = OrderId.From(request.OrderId);

        Domain.Entities.Order order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order '{request.OrderId}' not found.");

        if (request.ShouldSucceed)
        {
            order.MarkAsPaid();
            await _context.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new PaymentCompleted { OrderId = request.OrderId }, cancellationToken);

            return new SimulatePaymentResult(true, null);
        }

        string reason = "Payment declined (simulated)";
        order.MarkAsFailed(reason);
        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new PaymentFailed { OrderId = request.OrderId, Reason = reason }, cancellationToken);

        return new SimulatePaymentResult(false, reason);
    }
}

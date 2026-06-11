using MediatR;
using MicroCommerce.Catalog.Application.Orders.Dtos;
using MicroCommerce.Catalog.Application.Orders.Events;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Common;
using MicroCommerce.Catalog.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MicroCommerce.Catalog.Application.Orders.Commands;

public record CreateOrderLineInput(
    string Sku,
    string ProductName,
    string Tone,
    int Qty,
    decimal UnitPrice,
    string Fulfillment,
    string? Tracking,
    string? RestockNote);

public record CreateOrderCommand(
    int Number,
    DateTimeOffset PlacedAt,
    string CustomerName,
    string CustomerEmail,
    string CityState,
    string ShipLine1,
    string ShipLine2,
    bool BillSameAsShip,
    string ItemsSummary,
    string ShippingMethod,
    decimal ShippingPaid,
    decimal Tax,
    decimal FeePct,
    string PaymentBrand,
    string PaymentLastFour,
    string? LabelCarrier,
    decimal LabelCost,
    string? LabelWeightLabel,
    string InternalNote,
    IReadOnlyList<CreateOrderLineInput> Lines) : IRequest<Result<OrderDetailDto>>;

public class CreateOrderHandler(AppDbContext db, IPublisher publisher) : IRequestHandler<CreateOrderCommand, Result<OrderDetailDto>>
{
    private const string PostgresUniqueViolation = "23505";

    public async Task<Result<OrderDetailDto>> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        // Optimistic pre-check; the unique index on Number is the authoritative guarantee
        // via the DbUpdateException catch below.
        if (await db.Orders.AsNoTracking().AnyAsync(o => o.Number == request.Number, ct))
            return Result<OrderDetailDto>.Conflict("ORDER_NUMBER_DUPLICATE", $"Order #{request.Number} already exists.");

        var lines = request.Lines
            .Select(l => new OrderLine(
                l.Sku,
                l.ProductName,
                l.Tone,
                l.Qty,
                l.UnitPrice,
                OrderMapping.ParseFulfillment(l.Fulfillment),
                l.Tracking,
                l.RestockNote))
            .ToList();

        var order = new Order(
            request.Number,
            request.PlacedAt,
            request.CustomerName,
            request.CustomerEmail,
            request.CityState,
            request.ShipLine1,
            request.ShipLine2,
            request.BillSameAsShip,
            request.ItemsSummary,
            request.ShippingMethod,
            request.ShippingPaid,
            request.Tax,
            request.FeePct,
            request.PaymentBrand,
            request.PaymentLastFour,
            request.LabelCarrier,
            request.LabelCost,
            request.LabelWeightLabel,
            request.InternalNote,
            lines);

        db.Orders.Add(order);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresUniqueViolation })
        {
            return Result<OrderDetailDto>.Conflict("ORDER_NUMBER_DUPLICATE", $"Order #{request.Number} already exists.");
        }

        var customer = new OrderCustomerDto(
            order.CustomerName,
            order.CustomerEmail,
            order.ShipLine1,
            order.ShipLine2,
            order.BillSameAsShip,
            LifetimeOrderCount: 1,
            LifetimeSpend: order.Paid,
            Tags: []);

        var dto = OrderMapping.ToDetailDto(order, customer);
        await publisher.Publish(new OrderCreatedEvent(order.Number, dto.Status), ct);
        return Result<OrderDetailDto>.Success(dto);
    }
}

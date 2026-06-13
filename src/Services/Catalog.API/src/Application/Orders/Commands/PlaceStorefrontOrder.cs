using MediatR;
using MicroCommerce.Catalog.Application.Orders.Dtos;
using MicroCommerce.Catalog.Application.Orders.Events;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Common;
using MicroCommerce.Catalog.Domain.Orders;
using MicroCommerce.Catalog.Domain.Products;
using MicroCommerce.Catalog.Domain.Promotions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MicroCommerce.Catalog.Application.Orders.Commands;

public record StorefrontLineInput(string Sku, int Qty);

/// <summary>
/// Buyer checkout (spec docs/superpowers/specs/2026-06-12-storefront-design.md §5).
/// Server-authoritative: prices come from Products, the discount from Promotions, the
/// order number from max+1 allocation. The web server action supplies shipping/tax
/// demo constants and the payment summary; CustomerEmail comes from the bearer token.
/// </summary>
public record PlaceStorefrontOrderCommand(
    string CustomerEmail,
    string CustomerName,
    string ShipLine1,
    string ShipLine2,
    string CityState,
    string ShippingMethod,
    decimal ShippingPaid,
    decimal Tax,
    string PaymentBrand,
    string PaymentLastFour,
    string? PromoCode,
    IReadOnlyList<StorefrontLineInput> Lines) : IRequest<Result<OrderDetailDto>>;

public class PlaceStorefrontOrderHandler(AppDbContext db, IPublisher publisher)
    : IRequestHandler<PlaceStorefrontOrderCommand, Result<OrderDetailDto>>
{
    private const string PostgresUniqueViolation = "23505";
    private const int FirstOrderNumber = 1001;
    private const decimal DefaultFeePct = 4m;
    private const int MaxNumberRetries = 3;

    public async Task<Result<OrderDetailDto>> Handle(PlaceStorefrontOrderCommand request, CancellationToken ct)
    {
        if (request.Lines.Count == 0 || request.Lines.Any(l => l.Qty < 1))
            return Result<OrderDetailDto>.Conflict("EMPTY_CART", "The cart has no valid lines.");

        // Sku.From throws on whitespace; treat that as an unknown product, not a 500.
        if (request.Lines.Any(l => string.IsNullOrWhiteSpace(l.Sku)))
            return Result<OrderDetailDto>.Conflict("PRODUCT_NOT_FOUND", "A cart line has no SKU.");

        var lines = request.Lines.Select(l => (Sku: Sku.From(l.Sku), l.Qty)).ToList();

        // Load + mutate inventory => tracking required (global default is NoTracking).
        // Load each distinct product by a closure-captured scalar `==` on the value-converted Sku
        // column. This is the only form that BOTH translates and round-trips the Vogen converter on
        // Npgsql: a value-object `list.Contains(p.Sku)` and `.Sku.Value` member access inside
        // Contains either match zero rows or fail to translate; a captured-variable equality applies
        // the converter to the SQL parameter (mirrors GetProductBySku). The cart is small.
        var products = new Dictionary<string, Product>();
        foreach (var sku in lines.Select(l => l.Sku).Distinct())
        {
            var captured = sku;
            var product = await db.Products.AsTracking()
                .FirstOrDefaultAsync(p => p.Sku == captured, ct);
            if (product is not null)
                products[product.Sku.Value] = product;
        }

        var orderLines = new List<OrderLine>();
        foreach (var (sku, qty) in lines)
        {
            if (!products.TryGetValue(sku.Value, out var product) || product.Status == ProductStatus.Draft)
                return Result<OrderDetailDto>.Conflict("PRODUCT_NOT_FOUND", $"Product '{sku.Value}' is not available.");
            if (qty > product.Inventory)
                return Result<OrderDetailDto>.Conflict(
                    "INSUFFICIENT_STOCK",
                    $"Only {product.Inventory} of '{product.Name}' in stock.");

            orderLines.Add(new OrderLine(
                product.Sku.Value, product.Name, tone: "", qty, product.Price,
                FulfillmentStatus.Awaiting, tracking: null, restockNote: null));
        }

        var subtotal = orderLines.Sum(l => l.UnitPrice * l.Qty);

        string? discountCode = null;
        var discountAmount = 0m;
        if (!string.IsNullOrWhiteSpace(request.PromoCode))
        {
            var validation = await ValidatePromoAsync(request.PromoCode, subtotal, ct);
            if (validation.IsFailure)
                return Result<OrderDetailDto>.Conflict(validation.Error!.Value.Code, validation.Error.Value.Message);
            (discountCode, discountAmount) = validation.Value;
        }

        foreach (var (sku, qty) in lines)
            products[sku.Value].DecrementInventory(qty);

        var itemsSummary = string.Join(", ", orderLines
            .Select(l => l.Qty > 1 ? $"{l.ProductName} ×{l.Qty}" : l.ProductName));

        Order? order = null;
        for (var attempt = 1; attempt <= MaxNumberRetries; attempt++)
        {
            var next = (await db.Orders.AsNoTracking().MaxAsync(o => (int?)o.Number, ct)
                ?? (FirstOrderNumber - 1)) + 1;

            order = new Order(
                next, DateTimeOffset.UtcNow,
                request.CustomerName, request.CustomerEmail, request.CityState,
                request.ShipLine1, request.ShipLine2, billSameAsShip: true,
                itemsSummary, request.ShippingMethod, request.ShippingPaid, request.Tax,
                DefaultFeePct, request.PaymentBrand, request.PaymentLastFour,
                labelCarrier: null, labelCost: 0m, labelWeightLabel: null,
                internalNote: "", orderLines);
            if (discountAmount > 0 && discountCode is not null)
                order.ApplyDiscount(discountCode, discountAmount);

            db.Orders.Add(order);
            try
            {
                await db.SaveChangesAsync(ct);
                break;
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException { SqlState: PostgresUniqueViolation }
                      && attempt < MaxNumberRetries)
            {
                db.Entry(order).State = EntityState.Detached;
            }
        }

        var dto = await OrderMapping.ToDetailDtoWithCustomerAsync(db, order!, ct);
        await publisher.Publish(new OrderCreatedEvent(order!.Number, dto.Status), ct);
        return Result<OrderDetailDto>.Success(dto);
    }

    private async Task<Result<(string Code, decimal Amount)>> ValidatePromoAsync(
        string code, decimal subtotal, CancellationToken ct)
    {
        // PromotionCode.From normalizes (trim + uppercase); hoist it so the comparison
        // translates the same way GetPromotionByCode does.
        var promoCode = PromotionCode.From(code);
        var promo = await db.Promotions.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Code == promoCode, ct);

        var now = DateTimeOffset.UtcNow;
        if (promo is null
            || promo.Status != PromotionStatus.Active
            || (promo.StartsAt is { } start && now < start)
            || (promo.EndsAt is { } end && now > end))
        {
            return Result<(string, decimal)>.Conflict("PROMO_INVALID", "This promo code is not valid.");
        }

        if (promo.MinOrderAmount is { } min && subtotal < min)
            return Result<(string, decimal)>.Conflict(
                "PROMO_MIN_ORDER", $"This code needs a minimum order of {min:C}.");

        var amount = promo.Kind switch
        {
            DiscountKind.Percentage =>
                Math.Round(subtotal * promo.PercentValue!.Value / 100m, 2, MidpointRounding.AwayFromZero),
            DiscountKind.FixedAmount => Math.Min(promo.FixedAmount!.Value, subtotal),
            _ => 0m,
        };

        return Result<(string, decimal)>.Success((promoCode.Value, amount));
    }
}

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

    // Server-authoritative demo constants (spec §7). Shipping is derived from the chosen
    // method; tax is 8.5% of the discounted item subtotal. Client-supplied ShippingPaid/Tax
    // are advisory only — never trusted for money (review finding 4).
    private const decimal TaxRate = 0.085m;
    private const decimal StandardShipping = 8m;
    private const decimal ExpressShipping = 22m;
    private const decimal PickupShipping = 0m;

    private static decimal ShippingFor(string method) => method?.Trim().ToLowerInvariant() switch
    {
        "express" => ExpressShipping,
        "pickup" => PickupShipping,
        _ => StandardShipping,
    };

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
            // Buyability is governed by status, not inventory (review finding 1): only Active
            // or Low products may be purchased. Treat Out/Draft the same as a missing product,
            // mirroring the Buyable filter in GetProducts/GetProductCategories. Otherwise a
            // shop-hidden Out product with stale inventory > 0 would stay purchasable here.
            if (!products.TryGetValue(sku.Value, out var product)
                || product.Status is not (ProductStatus.Active or ProductStatus.Low))
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

        var stockError = DecrementStock(lines, products);
        if (stockError is not null)
            return stockError.Value;

        // Money is server-authoritative (review finding 4): recompute shipping from the chosen
        // method and tax at 8.5% of the discounted subtotal. The client's ShippingPaid/Tax are
        // ignored so a buyer cannot POST Tax:0/ShippingPaid:0 and underpay.
        var shippingPaid = ShippingFor(request.ShippingMethod);
        var tax = Math.Round((subtotal - discountAmount) * TaxRate, 2, MidpointRounding.AwayFromZero);

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
                itemsSummary, request.ShippingMethod, shippingPaid, tax,
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
            // Inventory oversell guard (review finding 2): Product carries an xmin concurrency
            // token, so a concurrent checkout that decremented the same product mid-flight makes
            // SaveChanges throw. Detach the order, reload the products with fresh inventory + xmin,
            // re-run the stock check (it may now be insufficient), re-decrement, and retry — same
            // shape as the order-number retry. This closes the lost-update race that could drive
            // Inventory negative across requests.
            catch (DbUpdateConcurrencyException) when (attempt < MaxNumberRetries)
            {
                db.Entry(order).State = EntityState.Detached;
                var reloadError = await ReloadAndReapplyStockAsync(lines, products, ct);
                if (reloadError is not null)
                    return reloadError.Value;
            }
        }

        var dto = await OrderMapping.ToDetailDtoWithCustomerAsync(db, order!, ct);
        await publisher.Publish(new OrderCreatedEvent(order!.Number, dto.Status), ct);
        return Result<OrderDetailDto>.Success(dto);
    }

    /// <summary>Applies the per-line decrement after a stock re-check; null = success.</summary>
    private static Result<OrderDetailDto>? DecrementStock(
        IReadOnlyList<(Sku Sku, int Qty)> lines, IReadOnlyDictionary<string, Product> products)
    {
        foreach (var (sku, qty) in lines)
        {
            var product = products[sku.Value];
            if (qty > product.Inventory)
                return Result<OrderDetailDto>.Conflict(
                    "INSUFFICIENT_STOCK", $"Only {product.Inventory} of '{product.Name}' in stock.");
            product.DecrementInventory(qty);
        }
        return null;
    }

    /// <summary>
    /// On a concurrency conflict, reload each product from the store so we pick up the winning
    /// transaction's inventory + a fresh xmin, then re-run the decrement (re-validating stock).
    /// </summary>
    private async Task<Result<OrderDetailDto>?> ReloadAndReapplyStockAsync(
        IReadOnlyList<(Sku Sku, int Qty)> lines, Dictionary<string, Product> products, CancellationToken ct)
    {
        foreach (var sku in lines.Select(l => l.Sku).Distinct())
        {
            var captured = sku;
            var reloaded = await db.Products.AsTracking()
                .FirstOrDefaultAsync(p => p.Sku == captured, ct);
            if (reloaded is null
                || reloaded.Status is not (ProductStatus.Active or ProductStatus.Low))
                return Result<OrderDetailDto>.Conflict(
                    "PRODUCT_NOT_FOUND", $"Product '{sku.Value}' is not available.");
            products[reloaded.Sku.Value] = reloaded;
        }
        return DecrementStock(lines, products);
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

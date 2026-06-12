using MicroCommerce.Catalog.Application.Orders.Commands;
using MicroCommerce.Catalog.Application.Orders.Events;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Products;
using MicroCommerce.Catalog.Domain.Promotions;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Tests.Orders.Commands;

public class PlaceStorefrontOrderHandlerTests
{
    private static Product Vase(int inventory = 10) =>
        TestProducts.Create("MC-VS-001", "Persimmon vase", "Vessels", 86m, inventory, ProductStatus.Active);

    private static Product Bowl(int inventory = 10) =>
        TestProducts.Create("MC-BW-014", "Forest bowl", "Tableware", 68m, inventory, ProductStatus.Active);

    private static Promotion Welcome10()
    {
        var p = new Promotion(
            PromotionCode.From("WELCOME10"), "first order",
            DiscountKind.FixedAmount, null, 10m, 40m, null, null);
        p.Activate();
        return p;
    }

    private static PlaceStorefrontOrderCommand Cmd(
        string? promoCode = null,
        params (string sku, int qty)[] lines) =>
        new(
            CustomerEmail: "buyer@microcommerce.dev",
            CustomerName: "Bao Buyer",
            ShipLine1: "241 Telegraph Ave",
            ShipLine2: "Oakland, CA 94612",
            CityState: "Oakland, CA",
            ShippingMethod: "Standard",
            ShippingPaid: 8m,
            Tax: 6.46m,
            PaymentBrand: "Visa",
            PaymentLastFour: "4242",
            PromoCode: promoCode,
            Lines: lines.Length == 0
                ? [new StorefrontLineInput("MC-VS-001", 1)]
                : [.. lines.Select(l => new StorefrontLineInput(l.sku, l.qty))]);

    private static async Task<(AppDbContext db, PlaceStorefrontOrderHandler handler, FakePublisher publisher)>
        ArrangeAsync(string dbName, params object[] entities)
    {
        var db = DbContextFactory.Create(dbName);
        foreach (var e in entities) db.Add(e);
        await db.SaveChangesAsync();
        var publisher = new FakePublisher();
        return (db, new PlaceStorefrontOrderHandler(db, publisher), publisher);
    }

    [Fact]
    public async Task PricesLinesFromDatabase_AndAllocatesNumber()
    {
        var dbName = Guid.NewGuid().ToString();
        var (db, handler, publisher) = await ArrangeAsync(dbName, Vase(), Bowl());

        var result = await handler.Handle(
            Cmd(null, ("MC-VS-001", 1), ("MC-BW-014", 2)), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        var dto = result.Value!;
        Assert.True(dto.Number >= 1001); // allocated, not supplied
        Assert.Equal(86m + 2 * 68m, dto.Summary.Subtotal); // DB prices, not client prices
        Assert.Equal("new", dto.Status);
        var evt = Assert.IsType<OrderCreatedEvent>(Assert.Single(publisher.Published));
        Assert.Equal(dto.Number, evt.Number);
    }

    [Fact]
    public async Task SequentialOrders_GetIncreasingNumbers()
    {
        var dbName = Guid.NewGuid().ToString();
        var (db, handler, _) = await ArrangeAsync(dbName, Vase());

        var first = await handler.Handle(Cmd(), TestContext.Current.CancellationToken);
        var second = await handler.Handle(Cmd(), TestContext.Current.CancellationToken);

        Assert.Equal(first.Value!.Number + 1, second.Value!.Number);
    }

    [Fact]
    public async Task DecrementsInventory_AndPersists()
    {
        var dbName = Guid.NewGuid().ToString();
        var (db, handler, _) = await ArrangeAsync(dbName, Vase(inventory: 5));

        var result = await handler.Handle(Cmd(null, ("MC-VS-001", 2)), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        // Fresh context over the SAME store: catches a missing AsTracking()
        await using var verify = DbContextFactory.Create(dbName);
        var product = await verify.Products.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(3, product.Inventory);
    }

    [Fact]
    public async Task InsufficientStock_ReturnsError_AndPersistsNothing()
    {
        var dbName = Guid.NewGuid().ToString();
        var (db, handler, publisher) = await ArrangeAsync(dbName, Vase(inventory: 1));

        var result = await handler.Handle(Cmd(null, ("MC-VS-001", 2)), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("INSUFFICIENT_STOCK", result.Error!.Value.Code);
        Assert.Empty(publisher.Published);
        Assert.Empty(db.Orders);
    }

    [Fact]
    public async Task UnknownSku_ReturnsProductNotFound()
    {
        var (db, handler, _) = await ArrangeAsync(Guid.NewGuid().ToString(), Vase());
        var result = await handler.Handle(Cmd(null, ("MC-NOPE-99", 1)), TestContext.Current.CancellationToken);
        Assert.True(result.IsFailure);
        Assert.Equal("PRODUCT_NOT_FOUND", result.Error!.Value.Code);
    }

    [Fact]
    public async Task DraftProduct_IsNotBuyable()
    {
        var draft = TestProducts.Create("MC-DR-001", "Draft pot", "Vessels", 50m, 10, ProductStatus.Draft);
        var (db, handler, _) = await ArrangeAsync(Guid.NewGuid().ToString(), draft);
        var result = await handler.Handle(Cmd(null, ("MC-DR-001", 1)), TestContext.Current.CancellationToken);
        Assert.True(result.IsFailure);
        Assert.Equal("PRODUCT_NOT_FOUND", result.Error!.Value.Code);
    }

    [Fact]
    public async Task EmptyLines_ReturnsEmptyCart()
    {
        var (db, handler, _) = await ArrangeAsync(Guid.NewGuid().ToString(), Vase());
        var cmd = Cmd() with { Lines = [] };
        var result = await handler.Handle(cmd, TestContext.Current.CancellationToken);
        Assert.True(result.IsFailure);
        Assert.Equal("EMPTY_CART", result.Error!.Value.Code);
    }

    [Fact]
    public async Task FixedPromo_AppliedAndPersisted()
    {
        var (db, handler, _) = await ArrangeAsync(Guid.NewGuid().ToString(), Vase(), Welcome10());

        var result = await handler.Handle(Cmd("WELCOME10", ("MC-VS-001", 1)), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("WELCOME10", result.Value!.Summary.DiscountCode);
        Assert.Equal(10m, result.Value.Summary.DiscountAmount);
        Assert.Equal(86m - 10m + 8m + 6.46m, result.Value.Summary.Paid);
    }

    [Fact]
    public async Task PercentPromo_RoundsAwayFromZero()
    {
        var promo = new Promotion(
            PromotionCode.From("SPRING20"), "sitewide",
            DiscountKind.Percentage, 20, null, null, null, null);
        promo.Activate();
        var (db, handler, _) = await ArrangeAsync(Guid.NewGuid().ToString(), Vase(), promo);

        var result = await handler.Handle(Cmd("SPRING20", ("MC-VS-001", 1)), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(17.20m, result.Value!.Summary.DiscountAmount); // 86 * 20%
    }

    [Fact]
    public async Task PromoBelowMinOrder_ReturnsError()
    {
        var cheap = TestProducts.Create("MC-CH-001", "Cheap mug", "Drinkware", 20m, 10, ProductStatus.Active);
        var (db, handler, _) = await ArrangeAsync(Guid.NewGuid().ToString(), cheap, Welcome10());

        var result = await handler.Handle(Cmd("WELCOME10", ("MC-CH-001", 1)), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("PROMO_MIN_ORDER", result.Error!.Value.Code);
    }

    [Fact]
    public async Task UnknownOrInactivePromo_ReturnsPromoInvalid()
    {
        var draftPromo = new Promotion(
            PromotionCode.From("FRIENDS"), "sitewide", DiscountKind.Percentage, 15, null, null, null, null);
        // not activated -> Draft
        var (db, handler, _) = await ArrangeAsync(Guid.NewGuid().ToString(), Vase(), draftPromo);

        var unknown = await handler.Handle(Cmd("NOPE", ("MC-VS-001", 1)), TestContext.Current.CancellationToken);
        var inactive = await handler.Handle(Cmd("FRIENDS", ("MC-VS-001", 1)), TestContext.Current.CancellationToken);

        Assert.Equal("PROMO_INVALID", unknown.Error!.Value.Code);
        Assert.Equal("PROMO_INVALID", inactive.Error!.Value.Code);
    }

    [Fact]
    public async Task ExpiredPromo_ReturnsPromoInvalid()
    {
        var expired = new Promotion(
            PromotionCode.From("BLOOM"), "sitewide", DiscountKind.Percentage, 10, null, null,
            new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 4, 12, 0, 0, 0, TimeSpan.Zero));
        expired.Activate();
        var (db, handler, _) = await ArrangeAsync(Guid.NewGuid().ToString(), Vase(), expired);

        var result = await handler.Handle(Cmd("BLOOM", ("MC-VS-001", 1)), TestContext.Current.CancellationToken);

        Assert.Equal("PROMO_INVALID", result.Error!.Value.Code);
    }

    [Fact]
    public async Task FixedPromo_CapsAtSubtotal()
    {
        var cheap = TestProducts.Create("MC-CH-002", "Tiny dish", "Tableware", 5m, 10, ProductStatus.Active);
        var big = new Promotion(
            PromotionCode.From("BIGOFF"), "test", DiscountKind.FixedAmount, null, 50m, null, null, null);
        big.Activate();
        var (db, handler, _) = await ArrangeAsync(Guid.NewGuid().ToString(), cheap, big);

        var result = await handler.Handle(Cmd("BIGOFF", ("MC-CH-002", 1)), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(5m, result.Value!.Summary.DiscountAmount); // capped, total never negative
    }
}

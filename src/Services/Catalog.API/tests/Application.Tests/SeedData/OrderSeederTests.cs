using MicroCommerce.Catalog.Api.SeedData;
using MicroCommerce.Catalog.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Tests.SeedData;

public class OrderSeederTests
{
    [Fact]
    public async Task SeedsExactly46_Idempotent()
    {
        var dbName = Guid.NewGuid().ToString();
        var ct = TestContext.Current.CancellationToken;

        await using (var db = DbContextFactory.Create(dbName))
        {
            await OrderSeeder.SeedAsync(db, ct);
            await OrderSeeder.SeedAsync(db, ct); // run twice — idempotent by Number
        }

        await using (var verify = DbContextFactory.Create(dbName))
        {
            Assert.Equal(46, await verify.Orders.CountAsync(ct));

            // Status distribution per drift policy D2 #1 (All = 46).
            Assert.Equal(3, await verify.Orders.CountAsync(o => o.Status == OrderStatus.New, ct));
            Assert.Equal(2, await verify.Orders.CountAsync(o => o.Status == OrderStatus.Packed, ct));
            Assert.Equal(18, await verify.Orders.CountAsync(o => o.Status == OrderStatus.Shipped, ct));
            Assert.Equal(21, await verify.Orders.CountAsync(o => o.Status == OrderStatus.Delivered, ct));
            Assert.Equal(1, await verify.Orders.CountAsync(o => o.Status == OrderStatus.RefundRequested, ct));
            Assert.Equal(1, await verify.Orders.CountAsync(o => o.Status == OrderStatus.Cancelled, ct));
        }
    }

    [Fact]
    public async Task Order1042_HasTwoLinesRefundAndTimeline()
    {
        var dbName = Guid.NewGuid().ToString();
        var ct = TestContext.Current.CancellationToken;

        await using (var db = DbContextFactory.Create(dbName))
        {
            await OrderSeeder.SeedAsync(db, ct);
        }

        await using (var verify = DbContextFactory.Create(dbName))
        {
            // Owned collections (Lines/Timeline/Refund) are auto-included by EF — no explicit Include.
            var order = await verify.Orders
                .FirstAsync(o => o.Number == 1042, ct);

            Assert.Equal(OrderStatus.New, order.Status);
            Assert.True(order.Starred);

            // Two lines: Persimmon vase (shipped) + Ash budstem (awaiting) — split fulfillment.
            Assert.Equal(2, order.Lines.Count);
            Assert.Contains(order.Lines, l => l.Sku == "MC-VS-001" && l.FulfillmentStatus == FulfillmentStatus.Shipped);
            Assert.Contains(order.Lines, l => l.Sku == "MC-AB-002" && l.FulfillmentStatus == FulfillmentStatus.Awaiting);

            // Money: Subtotal 152, Fee 6.08 (4%), Net 136.08 (152 - 6.08 - 9.84 label).
            Assert.Equal(152.00m, order.Subtotal);
            Assert.Equal(152.00m, order.Paid);
            Assert.Equal(6.08m, order.Fee);
            Assert.Equal(136.08m, order.Net);

            // Refund draft with two items.
            Assert.NotNull(order.Refund);
            Assert.Equal("Item arrived chipped", order.Refund!.Reason);
            Assert.Equal("Damage", order.Refund.RestockChoice);
            Assert.Equal(2, order.Refund.Items.Count);

            // Five-entry timeline.
            Assert.Equal(5, order.Timeline.Count);
            Assert.Contains(order.Timeline, t => t.Icon == "check" && t.Highlight);
            Assert.Contains(order.Timeline, t => t.Icon == "info" && t.Tone == "warn");
        }
    }
}

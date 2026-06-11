using MicroCommerce.Catalog.Application.Orders.Queries;
using MicroCommerce.Catalog.Domain.Customers;
using MicroCommerce.Catalog.Domain.Orders;

namespace MicroCommerce.Catalog.Application.Tests.Orders.Queries;

public class GetOrderByNumberHandlerTests
{
    [Fact]
    public async Task UnknownNumber_ReturnsNull()
    {
        await using var db = DbContextFactory.Create();
        var handler = new GetOrderByNumberHandler(db);

        var dto = await handler.Handle(new GetOrderByNumberQuery(9999), TestContext.Current.CancellationToken);

        Assert.Null(dto);
    }

    [Fact]
    public async Task KnownNumber_ReturnsFullDetail()
    {
        await using var db = DbContextFactory.Create();
        var order = TestOrders.CreateNew(
            number: 1042,
            labelCost: 9.84m,
            lines:
            [
                TestOrders.Line("MC-VS-001", "Persimmon vase", "clay", 1, 86.00m, FulfillmentStatus.Shipped, "USPS · 9405", null),
                TestOrders.Line("MC-AB-002", "Ash budstem", "rust", 1, 66.00m, FulfillmentStatus.Awaiting, null, "back in stock Tue"),
            ]);
        db.Orders.Add(order);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetOrderByNumberHandler(db);
        var dto = await handler.Handle(new GetOrderByNumberQuery(1042), TestContext.Current.CancellationToken);

        Assert.NotNull(dto);
        Assert.Equal(1042, dto!.Number);
        Assert.Equal("new", dto.Status);
        Assert.Equal(2, dto.Lines.Count);
        Assert.Equal(1, dto.Lines[0].Idx);
        Assert.Equal(2, dto.Lines[0].Of);
        // Paid = 152, Fee = 6.08, Net = 152 - 6.08 - 9.84 = 136.08
        Assert.Equal(152.00m, dto.Summary.Paid);
        Assert.Equal(136.08m, dto.Summary.Net);
        // First Awaiting line drives OutstandingProductName.
        Assert.Equal("Ash budstem", dto.OutstandingProductName);
    }

    [Fact]
    public async Task JoinsCustomerTagsAndLifetime_WhenCustomerExists()
    {
        await using var db = DbContextFactory.Create();

        // Sasha's current order (#1042, paid 152) + 2 prior non-cancelled orders totaling 132 → $284 lifetime, 3rd order.
        db.Orders.Add(TestOrders.CreateNew(
            number: 1042,
            lines:
            [
                TestOrders.Line(unitPrice: 86.00m),
                TestOrders.Line(unitPrice: 66.00m),
            ]));
        db.Orders.Add(TestOrders.CreateDelivered(number: 990, customerEmail: "sasha.l@gmail.com", lines: [TestOrders.Line(unitPrice: 66.00m)]));
        db.Orders.Add(TestOrders.CreateDelivered(number: 991, customerEmail: "sasha.l@gmail.com", lines: [TestOrders.Line(unitPrice: 66.00m)]));

        var customer = new Customer("Sasha Leblanc", Email.From("sasha.l@gmail.com"), "San Francisco, CA",
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        customer.AddTag("VIP");
        customer.AddTag("Repeat buyer");
        db.Customers.Add(customer);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetOrderByNumberHandler(db);
        var dto = await handler.Handle(new GetOrderByNumberQuery(1042), TestContext.Current.CancellationToken);

        Assert.NotNull(dto);
        Assert.Equal(3, dto!.Customer.LifetimeOrderCount);
        Assert.Equal(284.00m, dto.Customer.LifetimeSpend); // 152 + 66 + 66
        Assert.Equal(["VIP", "Repeat buyer"], dto.Customer.Tags);
    }

    [Fact]
    public async Task ReturnsEmptyTags_WhenNoCustomerRow()
    {
        await using var db = DbContextFactory.Create();
        db.Orders.Add(TestOrders.CreateNew(number: 1042, customerEmail: "nobody@example.com"));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetOrderByNumberHandler(db);
        var dto = await handler.Handle(new GetOrderByNumberQuery(1042), TestContext.Current.CancellationToken);

        Assert.NotNull(dto);
        Assert.Empty(dto!.Customer.Tags);
        Assert.Equal(1, dto.Customer.LifetimeOrderCount);
    }
}

using MicroCommerce.Catalog.Application.Customers.Queries;
using MicroCommerce.Catalog.Domain.Customers;
using MicroCommerce.Catalog.Domain.Orders;

namespace MicroCommerce.Catalog.Application.Tests.Customers.Queries;

public class GetCustomersHandlerTests
{
    private static Customer NewCustomer(string name, string email, string city = "San Francisco, CA") =>
        new(name, Email.From(email), city, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

    // Build an Order row directly (cross-domain test data) — no dependency on the Orders
    // test-builder file. One line, no shipping/tax/label so Net == Subtotal == unitPrice * qty.
    private static Order NewOrder(
        int number,
        string customerEmail,
        decimal unitPrice,
        DateTimeOffset placedAt,
        string customerName = "Sasha Leblanc") =>
        new(
            number,
            placedAt,
            customerName,
            customerEmail,
            "San Francisco, CA",
            "820 Sutter St",
            "San Francisco, CA 94109",
            true,
            "Persimmon vase",
            "USPS Priority",
            0m,
            0m,
            0m,
            "Visa",
            "4421",
            null,
            0m,
            null,
            string.Empty,
            new List<OrderLine>
            {
                new("MC-VS-001", "Persimmon vase", "clay", 1, unitPrice, FulfillmentStatus.Awaiting, null, null),
            });

    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsEmptyPage()
    {
        await using var db = DbContextFactory.Create();
        var handler = new GetCustomersHandler(db);

        var result = await handler.Handle(new GetCustomersQuery(1, 10, null), TestContext.Current.CancellationToken);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.Total);
    }

    [Fact]
    public async Task Handle_Search_FiltersByNameOrEmail()
    {
        await using var db = DbContextFactory.Create();
        db.Customers.AddRange(
            NewCustomer("Sasha Leblanc", "sasha.l@gmail.com"),
            NewCustomer("Dev Patel", "dev.p@gmail.com"),
            NewCustomer("Luz Moreno", "luz.m@gmail.com"));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetCustomersHandler(db);

        // match by name (case-insensitive)
        var byName = await handler.Handle(new GetCustomersQuery(1, 10, "sasha"), TestContext.Current.CancellationToken);
        Assert.Equal(1, byName.Total);
        Assert.Equal("sasha.l@gmail.com", byName.Items.Single().Email);

        // match by email fragment
        var byEmail = await handler.Handle(new GetCustomersQuery(1, 10, "dev.p"), TestContext.Current.CancellationToken);
        Assert.Equal(1, byEmail.Total);
        Assert.Equal("Dev Patel", byEmail.Items.Single().Name);

        // no match
        var none = await handler.Handle(new GetCustomersQuery(1, 10, "zzz"), TestContext.Current.CancellationToken);
        Assert.Equal(0, none.Total);
    }

    [Fact]
    public async Task Handle_LifetimeStats_DerivedFromOrders()
    {
        await using var db = DbContextFactory.Create();
        db.Customers.Add(NewCustomer("Sasha Leblanc", "sasha.l@gmail.com"));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // 3 orders for Sasha: 152 + 66 + 66 = 284 lifetime, 3 orders.
        db.Orders.AddRange(
            NewOrder(1042, "sasha.l@gmail.com", 152m, new DateTimeOffset(2026, 4, 8, 14, 14, 0, TimeSpan.FromHours(-7))),
            NewOrder(1020, "sasha.l@gmail.com", 66m, new DateTimeOffset(2026, 3, 1, 9, 0, 0, TimeSpan.FromHours(-7))),
            NewOrder(1005, "sasha.l@gmail.com", 66m, new DateTimeOffset(2026, 2, 1, 9, 0, 0, TimeSpan.FromHours(-7))));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetCustomersHandler(db);
        var result = await handler.Handle(new GetCustomersQuery(1, 10, null), TestContext.Current.CancellationToken);

        var sasha = result.Items.Single();
        Assert.Equal(3, sasha.LifetimeOrderCount);
        Assert.Equal(284m, sasha.LifetimeSpend);
        Assert.Equal(new DateTimeOffset(2026, 2, 1, 9, 0, 0, TimeSpan.FromHours(-7)), sasha.FirstOrderAt);
        Assert.Equal(new DateTimeOffset(2026, 4, 8, 14, 14, 0, TimeSpan.FromHours(-7)), sasha.LastOrderAt);
    }

    [Fact]
    public async Task Handle_NoOrders_LifetimeStatsAreZeroAndNull()
    {
        await using var db = DbContextFactory.Create();
        db.Customers.Add(NewCustomer("Dev Patel", "dev.p@gmail.com"));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetCustomersHandler(db);
        var result = await handler.Handle(new GetCustomersQuery(1, 10, null), TestContext.Current.CancellationToken);

        var dev = result.Items.Single();
        Assert.Equal(0, dev.LifetimeOrderCount);
        Assert.Equal(0m, dev.LifetimeSpend);
        Assert.Null(dev.FirstOrderAt);
        Assert.Null(dev.LastOrderAt);
    }

    [Fact]
    public async Task Handle_OrdersByCustomer_DoNotLeakAcrossEmails()
    {
        await using var db = DbContextFactory.Create();
        db.Customers.AddRange(
            NewCustomer("Sasha Leblanc", "sasha.l@gmail.com"),
            NewCustomer("Dev Patel", "dev.p@gmail.com"));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        db.Orders.Add(NewOrder(1042, "sasha.l@gmail.com", 152m,
            new DateTimeOffset(2026, 4, 8, 14, 14, 0, TimeSpan.FromHours(-7))));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetCustomersHandler(db);
        var result = await handler.Handle(new GetCustomersQuery(1, 10, null), TestContext.Current.CancellationToken);

        var sasha = result.Items.Single(c => c.Email == "sasha.l@gmail.com");
        var dev = result.Items.Single(c => c.Email == "dev.p@gmail.com");
        Assert.Equal(152m, sasha.LifetimeSpend);
        Assert.Equal(0m, dev.LifetimeSpend);
    }

    [Fact]
    public async Task Handle_OrdersByLifetimeSpendDescending()
    {
        await using var db = DbContextFactory.Create();
        db.Customers.AddRange(
            NewCustomer("Sasha Leblanc", "sasha.l@gmail.com"),
            NewCustomer("Dev Patel", "dev.p@gmail.com"));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        db.Orders.AddRange(
            NewOrder(1042, "sasha.l@gmail.com", 152m, new DateTimeOffset(2026, 4, 8, 14, 14, 0, TimeSpan.FromHours(-7))),
            NewOrder(1041, "dev.p@gmail.com", 64m, new DateTimeOffset(2026, 4, 8, 11, 8, 0, TimeSpan.FromHours(-7))));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetCustomersHandler(db);
        var result = await handler.Handle(new GetCustomersQuery(1, 10, null), TestContext.Current.CancellationToken);

        Assert.Equal("sasha.l@gmail.com", result.Items[0].Email);
        Assert.Equal("dev.p@gmail.com", result.Items[1].Email);
    }

    [Fact]
    public async Task Handle_ReturnsTags()
    {
        await using var db = DbContextFactory.Create();
        var c = NewCustomer("Sasha Leblanc", "sasha.l@gmail.com");
        c.AddTag("VIP");
        c.AddTag("Repeat buyer");
        db.Customers.Add(c);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetCustomersHandler(db);
        var result = await handler.Handle(new GetCustomersQuery(1, 10, null), TestContext.Current.CancellationToken);

        var sasha = result.Items.Single();
        Assert.Contains("VIP", sasha.Tags);
        Assert.Contains("Repeat buyer", sasha.Tags);
    }
}

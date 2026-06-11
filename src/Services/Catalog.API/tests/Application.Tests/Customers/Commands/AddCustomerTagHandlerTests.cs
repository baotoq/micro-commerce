using MicroCommerce.Catalog.Application.Customers.Commands;
using MicroCommerce.Catalog.Application.Customers.Events;
using MicroCommerce.Catalog.Domain.Customers;

namespace MicroCommerce.Catalog.Application.Tests.Customers.Commands;

public class AddCustomerTagHandlerTests
{
    private static Customer NewCustomer(string email = "sasha.l@gmail.com") =>
        new("Sasha Leblanc", Email.From(email), "San Francisco, CA",
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Handle_UnknownEmail_ReturnsConflict()
    {
        await using var db = DbContextFactory.Create();
        var publisher = new FakePublisher();
        var handler = new AddCustomerTagHandler(db, publisher);

        var result = await handler.Handle(
            new AddCustomerTagCommand("missing@gmail.com", "VIP"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("CUSTOMER_NOT_FOUND", result.Error!.Value.Code);
        Assert.Empty(publisher.Published);
    }

    [Fact]
    public async Task Handle_Valid_PersistsTag_FreshContextAssert()
    {
        // Shared in-memory DB name: seed in one context, run the handler in a second
        // over the SAME database, then assert in a third. This is the only reliable way
        // to catch a missing .AsTracking() — a within-context read would hit the
        // change-tracker and pass even if SaveChanges was a silent no-op.
        var dbName = Guid.NewGuid().ToString();

        await using (var seed = DbContextFactory.Create(dbName))
        {
            seed.Customers.Add(NewCustomer());
            await seed.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var publisher = new FakePublisher();
        await using (var act = DbContextFactory.Create(dbName))
        {
            var handler = new AddCustomerTagHandler(act, publisher);
            var result = await handler.Handle(
                new AddCustomerTagCommand("sasha.l@gmail.com", "VIP"),
                TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Contains("VIP", result.Value!.Tags);
        }

        await using (var assert = DbContextFactory.Create(dbName))
        {
            var stored = assert.Customers.Single();
            Assert.Contains("VIP", stored.Tags);
        }

        var evt = Assert.IsType<CustomerUpdatedEvent>(Assert.Single(publisher.Published));
        Assert.Equal("sasha.l@gmail.com", evt.Email);
    }

    [Fact]
    public async Task Handle_DuplicateTag_IsIdempotent_AndPersistsOnce()
    {
        var dbName = Guid.NewGuid().ToString();

        await using (var seed = DbContextFactory.Create(dbName))
        {
            var c = NewCustomer();
            c.AddTag("VIP");
            seed.Customers.Add(c);
            await seed.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var act = DbContextFactory.Create(dbName))
        {
            var handler = new AddCustomerTagHandler(act, new FakePublisher());
            var result = await handler.Handle(
                new AddCustomerTagCommand("sasha.l@gmail.com", "VIP"),
                TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Single(result.Value!.Tags);
        }

        await using (var assert = DbContextFactory.Create(dbName))
        {
            Assert.Single(assert.Customers.Single().Tags);
        }
    }
}

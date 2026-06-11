using MicroCommerce.Catalog.Application.Customers.Commands;
using MicroCommerce.Catalog.Application.Customers.Events;
using MicroCommerce.Catalog.Domain.Common;
using MicroCommerce.Catalog.Domain.Customers;

namespace MicroCommerce.Catalog.Application.Tests.Customers.Commands;

public class CreateCustomerHandlerTests
{
    private sealed class FixedClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset Now { get; } = now;
    }

    private static readonly IClock Clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 16, 45, 0, TimeSpan.FromHours(-7)));

    private static CreateCustomerCommand Cmd(string email = "sasha.l@gmail.com") =>
        new("Sasha Leblanc", email, "San Francisco, CA");

    [Fact]
    public async Task Handle_Valid_PersistsAndPublishesEvent()
    {
        await using var db = DbContextFactory.Create();
        var publisher = new FakePublisher();
        var handler = new CreateCustomerHandler(db, publisher, Clock);

        var result = await handler.Handle(Cmd(), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("Sasha Leblanc", result.Value!.Name);
        Assert.Equal("sasha.l@gmail.com", result.Value.Email);
        Assert.Equal("San Francisco, CA", result.Value.City);
        Assert.Single(db.Customers);

        var evt = Assert.IsType<CustomerCreatedEvent>(Assert.Single(publisher.Published));
        Assert.Equal("sasha.l@gmail.com", evt.Email);
    }

    [Fact]
    public async Task Handle_NormalizesEmailToLowercase()
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreateCustomerHandler(db, new FakePublisher(), Clock);

        var result = await handler.Handle(
            new CreateCustomerCommand("Sasha Leblanc", "  Sasha.L@GMAIL.com ", "San Francisco, CA"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("sasha.l@gmail.com", result.Value!.Email);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsConflict()
    {
        await using var db = DbContextFactory.Create();
        var publisher = new FakePublisher();
        var handler = new CreateCustomerHandler(db, publisher, Clock);
        await handler.Handle(Cmd(), TestContext.Current.CancellationToken);
        publisher.Published.Clear();

        var result = await handler.Handle(Cmd(), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("CUSTOMER_EMAIL_DUPLICATE", result.Error!.Value.Code);
        Assert.Empty(publisher.Published);
    }
}

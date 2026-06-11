using MicroCommerce.Catalog.Domain.Customers;
using Vogen;

namespace MicroCommerce.Catalog.Domain.Tests.Customers;

public class CustomerTests
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 4, 8, 16, 45, 0, TimeSpan.FromHours(-7));

    private static Customer Create(
        string name = "Sasha Leblanc",
        string email = "sasha.l@gmail.com",
        string city = "San Francisco, CA") =>
        new(name, Email.From(email), city, CreatedAt);

    [Fact]
    public void Customer_Valid_Creates()
    {
        var customer = Create();

        Assert.NotEqual(default, customer.Id.Value);
        Assert.Equal("Sasha Leblanc", customer.Name);
        Assert.Equal("sasha.l@gmail.com", customer.Email.Value);
        Assert.Equal("San Francisco, CA", customer.City);
        Assert.Empty(customer.Tags);
        Assert.Equal(CreatedAt, customer.CreatedAt);
    }

    [Fact]
    public void Customer_EmptyName_Throws()
    {
        Assert.Throws<ArgumentException>(() => Create(name: ""));
    }

    [Fact]
    public void Customer_WhitespaceName_Throws()
    {
        Assert.Throws<ArgumentException>(() => Create(name: "   "));
    }

    [Fact]
    public void Customer_NameTooLong_Throws()
    {
        var longName = new string('a', 121);
        Assert.Throws<ArgumentException>(() => Create(name: longName));
    }

    [Fact]
    public void Customer_EmptyCity_Throws()
    {
        Assert.Throws<ArgumentException>(() => Create(city: ""));
    }

    [Fact]
    public void AddTag_DedupsAndStores()
    {
        var customer = Create();

        customer.AddTag("VIP");
        customer.AddTag("VIP");
        customer.AddTag("Repeat buyer");

        Assert.Equal(2, customer.Tags.Count);
        Assert.Contains("VIP", customer.Tags);
        Assert.Contains("Repeat buyer", customer.Tags);
    }

    [Fact]
    public void AddTag_Over10_Throws()
    {
        var customer = Create();
        for (var i = 0; i < 10; i++)
            customer.AddTag($"tag-{i}");

        Assert.Throws<InvalidOperationException>(() => customer.AddTag("tag-overflow"));
    }

    [Fact]
    public void AddTag_TooLong_Throws()
    {
        var customer = Create();
        var longTag = new string('a', 51);

        Assert.Throws<InvalidOperationException>(() => customer.AddTag(longTag));
    }

    [Fact]
    public void AddTag_Empty_Throws()
    {
        var customer = Create();

        Assert.Throws<InvalidOperationException>(() => customer.AddTag("   "));
    }

    [Fact]
    public void RemoveTag_WhenPresent_Removes()
    {
        var customer = Create();
        customer.AddTag("VIP");
        customer.AddTag("Repeat buyer");

        customer.RemoveTag("VIP");

        Assert.Single(customer.Tags);
        Assert.DoesNotContain("VIP", customer.Tags);
        Assert.Contains("Repeat buyer", customer.Tags);
    }

    [Fact]
    public void RemoveTag_WhenAbsent_NoOp()
    {
        var customer = Create();
        customer.AddTag("VIP");

        customer.RemoveTag("Nonexistent");

        Assert.Single(customer.Tags);
        Assert.Contains("VIP", customer.Tags);
    }

    [Fact]
    public void UpdateName_Empty_Throws()
    {
        var customer = Create();

        Assert.Throws<ArgumentException>(() => customer.UpdateName(""));
    }

    [Fact]
    public void UpdateName_TooLong_Throws()
    {
        var customer = Create();
        var longName = new string('a', 121);

        Assert.Throws<ArgumentException>(() => customer.UpdateName(longName));
    }

    [Fact]
    public void UpdateName_Valid_AppliesChange()
    {
        var customer = Create();

        customer.UpdateName("Sasha L.");

        Assert.Equal("Sasha L.", customer.Name);
    }

    [Fact]
    public void Email_NormalizesTrimAndLowercase()
    {
        var email = Email.From("  Sasha.L@Gmail.com  ");
        Assert.Equal("sasha.l@gmail.com", email.Value);
    }

    [Fact]
    public void Email_Empty_Throws()
    {
        Assert.Throws<ValueObjectValidationException>(() => Email.From(""));
    }

    [Fact]
    public void Email_WithoutAtSign_Throws()
    {
        Assert.Throws<ValueObjectValidationException>(() => Email.From("not-an-email"));
    }

    [Fact]
    public void Email_SameValueAreEqual()
    {
        var a = Email.From("sasha.l@gmail.com");
        var b = Email.From("SASHA.L@GMAIL.COM");
        Assert.Equal(a, b);
    }
}

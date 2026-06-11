using MicroCommerce.Catalog.Api.SeedData;
using MicroCommerce.Catalog.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Tests.SeedData;

public class CustomerSeederTests
{
    [Fact]
    public async Task SeedAsync_OnEmptyDb_AddsSeedCustomers()
    {
        await using var db = DbContextFactory.Create();
        var ct = TestContext.Current.CancellationToken;

        await CustomerSeeder.SeedAsync(db, ct);

        var customers = await db.Customers.ToListAsync(ct);
        Assert.NotEmpty(customers);
        Assert.Contains(customers, c => c.Email.Value == "sasha.l@gmail.com");
        Assert.Contains(customers, c => c.Email.Value == "dev.p@gmail.com");
        Assert.Contains(customers, c => c.Email.Value == "luz.m@gmail.com");
        Assert.Contains(customers, c => c.Email.Value == "kai.z@gmail.com");
        Assert.Contains(customers, c => c.Email.Value == "jordan.e@gmail.com");
    }

    [Fact]
    public async Task SeedAsync_RunTwice_DoesNotDuplicateCustomers()
    {
        await using var db = DbContextFactory.Create();
        var ct = TestContext.Current.CancellationToken;

        await CustomerSeeder.SeedAsync(db, ct);
        var afterFirst = await db.Customers.CountAsync(ct);

        await CustomerSeeder.SeedAsync(db, ct);
        var afterSecond = await db.Customers.CountAsync(ct);

        Assert.Equal(afterFirst, afterSecond);
    }

    [Fact]
    public async Task SeedAsync_WithSashaAlreadyPresent_LeavesExistingRowUntouched()
    {
        await using var db = DbContextFactory.Create();
        var ct = TestContext.Current.CancellationToken;

        var existing = new Customer(
            "Existing Sasha",
            Email.From("sasha.l@gmail.com"),
            "Nowhere, NA",
            new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));
        db.Customers.Add(existing);
        await db.SaveChangesAsync(ct);

        await CustomerSeeder.SeedAsync(db, ct);

        var sasha = await db.Customers.SingleAsync(c => c.Email.Value == "sasha.l@gmail.com", ct);
        Assert.Equal("Existing Sasha", sasha.Name);
        Assert.Empty(sasha.Tags);
    }

    [Fact]
    public async Task SeedAsync_SashaLeblanc_HasVipTag()
    {
        await using var db = DbContextFactory.Create();
        var ct = TestContext.Current.CancellationToken;

        await CustomerSeeder.SeedAsync(db, ct);

        var sasha = await db.Customers.SingleAsync(c => c.Email.Value == "sasha.l@gmail.com", ct);
        Assert.Equal("Sasha Leblanc", sasha.Name);
        Assert.Equal("San Francisco, CA", sasha.City);
        Assert.Contains("VIP", sasha.Tags);
        Assert.Contains("Repeat buyer", sasha.Tags);
    }
}

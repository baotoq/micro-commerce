using FluentAssertions;
using MicroCommerce.ApiService.Features.Cart.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Cart.Infrastructure;
using MicroCommerce.ApiService.Tests.Integration.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CartEntity = MicroCommerce.ApiService.Features.Cart.Domain.Entities.Cart;

namespace MicroCommerce.ApiService.Tests.Integration.Interceptors;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class InterceptorBehaviorTests : IntegrationTestBase
{
    public InterceptorBehaviorTests(ApiWebApplicationFactory factory) : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await ResetDatabase(typeof(CartDbContext));
    }

    [Fact]
    public async Task AuditInterceptor_OnInsert_SetsCreatedAtAndUpdatedAt()
    {
        DateTimeOffset before = DateTimeOffset.UtcNow.AddSeconds(-1);

        using IServiceScope scope = CreateScope();
        CartDbContext db = scope.ServiceProvider.GetRequiredService<CartDbContext>();

        CartEntity cart = CartEntity.Create(Guid.NewGuid());
        db.Carts.Add(cart);
        await db.SaveChangesAsync();

        cart.CreatedAt.Should().BeAfter(before);
        cart.UpdatedAt.Should().BeAfter(before);
        cart.CreatedAt.Should().Be(cart.UpdatedAt);
    }

    [Fact]
    public async Task AuditInterceptor_OnUpdate_UpdatesUpdatedAtOnly()
    {
        using IServiceScope scope = CreateScope();
        CartDbContext db = scope.ServiceProvider.GetRequiredService<CartDbContext>();

        CartEntity cart = CartEntity.Create(Guid.NewGuid());
        db.Carts.Add(cart);
        await db.SaveChangesAsync();

        DateTimeOffset originalCreatedAt = cart.CreatedAt;

        await Task.Delay(50);

        cart.AddItem(Guid.NewGuid(), "Test Product", 9.99m, null, 1);
        await db.SaveChangesAsync();

        cart.CreatedAt.Should().Be(originalCreatedAt);
        cart.UpdatedAt.Should().BeAfter(originalCreatedAt);
    }

    [Fact]
    public async Task ConcurrencyInterceptor_OnInsert_SetsVersionToOne()
    {
        using IServiceScope scope = CreateScope();
        CartDbContext db = scope.ServiceProvider.GetRequiredService<CartDbContext>();

        CartEntity cart = CartEntity.Create(Guid.NewGuid());
        db.Carts.Add(cart);
        await db.SaveChangesAsync();

        cart.Version.Should().Be(1);
    }

    [Fact]
    public async Task ConcurrencyInterceptor_OnUpdate_IncrementsVersion()
    {
        using IServiceScope scope = CreateScope();
        CartDbContext db = scope.ServiceProvider.GetRequiredService<CartDbContext>();

        CartEntity cart = CartEntity.Create(Guid.NewGuid());
        db.Carts.Add(cart);
        await db.SaveChangesAsync();

        cart.AddItem(Guid.NewGuid(), "Test Product", 9.99m, null, 1);
        await db.SaveChangesAsync();

        cart.Version.Should().Be(2);
    }

    [Fact]
    public async Task ConcurrencyInterceptor_ConcurrentUpdate_ThrowsDbUpdateConcurrencyException()
    {
        // Insert the cart in scope 0
        CartEntity insertedCart;
        using (IServiceScope scope0 = CreateScope())
        {
            CartDbContext db0 = scope0.ServiceProvider.GetRequiredService<CartDbContext>();
            insertedCart = CartEntity.Create(Guid.NewGuid());
            db0.Carts.Add(insertedCart);
            await db0.SaveChangesAsync();
        }

        CartId cartId = insertedCart.Id;

        // Open scope1 — reads cart with Version=1 (tracked)
        IServiceScope scope1 = CreateScope();
        CartDbContext db1 = scope1.ServiceProvider.GetRequiredService<CartDbContext>();
        CartEntity cart1 = await db1.Carts.AsTracking().FirstAsync(c => c.Id == cartId);

        // Open scope2 — reads same cart with Version=1 (tracked)
        IServiceScope scope2 = CreateScope();
        CartDbContext db2 = scope2.ServiceProvider.GetRequiredService<CartDbContext>();
        CartEntity cart2 = await db2.Carts.AsTracking().FirstAsync(c => c.Id == cartId);

        try
        {
            // scope2 saves first — Version goes 1->2 in DB
            cart2.AddItem(Guid.NewGuid(), "Test Product", 9.99m, null, 1);
            await db2.SaveChangesAsync();

            // scope1 attempts to save with stale Version=1 — should fail
            cart1.AddItem(Guid.NewGuid(), "Another Product", 19.99m, null, 1);
            Func<Task> act = async () => await db1.SaveChangesAsync();
            await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
        }
        finally
        {
            scope1.Dispose();
            scope2.Dispose();
        }
    }
}

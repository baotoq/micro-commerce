using System.Net;
using System.Net.Http.Json;
using MicroCommerce.ApiService.Features.Catalog.Categories;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MicroCommerce.FunctionalTests.Features.Catalog;

/// <summary>
/// The leaf rule holds when a Product create races a child being added to the same Category (FR-035,
/// research R4). The child is added through CategoryRules directly, so this test doesn't depend on
/// the Platform Operator endpoints of User Story 6.
/// </summary>
public class CategoryConcurrencyTests(ApiFixture App) : TestBase<ApiFixture>
{
    [Fact]
    public async Task A_product_never_ends_up_in_a_category_with_children()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();

        for (var i = 0; i < 20; i++)
        {
            var leaf = await AddTopLevelCategoryAsync();

            var create = merchant.PostAsJsonAsync("/api/merchant/products",
                ProductHelpers.ProductRequest(CatalogTestData.Unique("Racer"), leaf.Value), TestContext.Current.CancellationToken);
            var addChild = AddChildAsync(leaf);
            await Task.WhenAll(create, addChild);

            await using var scope = App.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            var hasChildren = await db.Categories.AnyAsync(c => c.ParentId == leaf, TestContext.Current.CancellationToken);
            var hasProducts = await db.Products.AnyAsync(p => p.CategoryId == leaf, TestContext.Current.CancellationToken);
            (hasChildren && hasProducts).ShouldBeFalse();
            (await create).StatusCode.ShouldBeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
        }
    }

    private async Task<CategoryId> AddTopLevelCategoryAsync()
    {
        await using var scope = App.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var category = Category.CreateTopLevel(CategoryId.New(), CatalogTestData.Unique("Race"));
        db.Categories.Add(category);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return category.Id;
    }

    /// <returns>Whether the child was added (false when the Category already held a Product).</returns>
    private async Task<bool> AddChildAsync(CategoryId parent)
    {
        await using var scope = App.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var added = false;
        await CategoryRules.SaveWithCategoryLockedAsync(db, parent, async (locked, ct) =>
        {
            added = false;
            if (locked is null || await CategoryRules.HoldsProductsAsync(db, parent, ct)) return;
            db.Categories.Add(Category.CreateChild(CategoryId.New(), CatalogTestData.Unique("Child"), parent));
            added = true;
        }, TestContext.Current.CancellationToken);
        return added;
    }
}

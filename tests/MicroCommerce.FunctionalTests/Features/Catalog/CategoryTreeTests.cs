using System.Net;

namespace MicroCommerce.FunctionalTests.Features.Catalog;

/// <summary>The Category tree is public, and Development seeds the starter tree (FR-033).</summary>
public class CategoryTreeTests(ApiFixture app) : TestBase<ApiFixture>
{
    [Fact]
    public async Task Anyone_reads_the_seeded_tree_nested_and_sorted_by_name()
    {
        var response = await app.Client.GetAsync("/api/marketplace/categories", TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var tree = await CatalogTestData.CategoryTreeAsync(app.Client);

        var fashion = tree.Single(c => c.Name == "Fashion");
        fashion.Children.Select(c => c.Name).ShouldBe(["Men", "Women"]);
        fashion.Children[0].Children.Select(c => c.Name).ShouldBe(["Shirts", "T-Shirts"]);

        var kitchen = tree.Single(c => c.Name == "Home").Children.Single(c => c.Name == "Kitchen");
        kitchen.Children.ShouldBeEmpty();

        // Top-level siblings are sorted by name, ignoring case.
        var names = tree.Select(c => c.Name).ToList();
        names.ShouldBe(names.Order(StringComparer.OrdinalIgnoreCase).ToList());
    }
}

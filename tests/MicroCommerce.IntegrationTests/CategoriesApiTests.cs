using System.Net.Http.Json;

namespace MicroCommerce.IntegrationTests;

public class CategoriesApiTests(AppHostFixture fixture)
{
    private sealed record CategoryNode(Guid Id, string Name, List<CategoryNode> Children);

    [Fact]
    public async Task Development_seeds_the_starter_category_tree()
    {
        var tree = await fixture.ApiClient.GetFromJsonAsync<List<CategoryNode>>(
            "/storefront/categories", TestContext.Current.CancellationToken);

        var fashion = tree.ShouldNotBeNull().Single(n => n.Name == "Fashion");
        fashion.Children.Single(n => n.Name == "Men").Children.Select(n => n.Name).ShouldContain("T-Shirts");
        tree.Single(n => n.Name == "Home").Children.Select(n => n.Name).ShouldContain("Kitchen");
    }
}

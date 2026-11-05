using System.Net.Http.Json;

namespace MicroCommerce.FunctionalTests;

/// <summary>
/// Shared test data helpers. The app and its database are shared by every test, and Development
/// seeds a starter Category tree, so tests make their own data unique and never assume an empty tree.
/// </summary>
public static class CatalogTestData
{
    public static string Unique(string prefix) => $"{prefix} {Guid.NewGuid():N}";

    /// <summary>A short unique Shop name: Shop names are 3 to 50 characters.</summary>
    public static string UniqueShopName() => $"Shop {Guid.NewGuid():N}"[..37];

    public sealed record CategoryNodeDto(Guid Id, string Name, List<CategoryNodeDto> Children);

    public static async Task<List<CategoryNodeDto>> CategoryTreeAsync(HttpClient client) =>
        (await client.GetFromJsonAsync<List<CategoryNodeDto>>("/api/marketplace/categories", TestContext.Current.CancellationToken))!;

    /// <summary>The seeded leaf Home › Kitchen. Tests never give it children or delete it.</summary>
    public static async Task<Guid> SeededLeafCategoryAsync(HttpClient client)
    {
        var tree = await CategoryTreeAsync(client);
        return tree.Single(c => c.Name == "Home").Children.Single(c => c.Name == "Kitchen").Id;
    }

    /// <summary>The seeded top-level Home, which has children.</summary>
    public static async Task<Guid> SeededHomeCategoryAsync(HttpClient client) =>
        (await CategoryTreeAsync(client)).Single(c => c.Name == "Home").Id;
}

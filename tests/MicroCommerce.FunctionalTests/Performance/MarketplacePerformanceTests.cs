using System.Diagnostics;
using System.Net;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.Domain.Catalog;
using MicroCommerce.Domain.SharedKernel;
using Microsoft.Extensions.DependencyInjection;

namespace MicroCommerce.FunctionalTests.Performance;

/// <summary>Its own app and database, so 10,000 extra Products never slow or disturb the other tests.</summary>
public sealed class PerformanceApiFixture : ApiFixture;

/// <summary>
/// SC-003: with at least 10,000 Published Products, 95% of browse, search and Category-filter requests
/// answer in under 1 second. Explicit (opt-in): run with <c>--explicit only</c> (quickstart Scenario 6).
/// </summary>
public class MarketplacePerformanceTests(PerformanceApiFixture App) : TestBase<PerformanceApiFixture>
{
    private const int ProductCount = 10_000;
    private const int MerchantCount = 50;
    private static readonly string[] Words = ["mug", "cup", "bowl", "plate", "lamp", "shirt", "dress", "phone", "laptop", "towel"];

    [Fact(Explicit = true)]
    public async Task Ninety_five_percent_of_marketplace_requests_answer_within_a_second()
    {
        var ct = TestContext.Current.CancellationToken;
        var merchantIds = new List<MerchantId>();
        for (var i = 0; i < MerchantCount; i++) merchantIds.Add(MerchantId.From((await App.NewMerchantAsync()).Shop.MerchantId));

        var tree = await CatalogTestData.CategoryTreeAsync(App.Client);
        var leaves = Leaves(tree).Select(CategoryId.From).ToList();
        var now = DateTimeOffset.UtcNow;

        for (var batch = 0; batch < ProductCount / 1000; batch++)
        {
            await using var scope = App.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            for (var i = 0; i < 1000; i++)
            {
                var n = batch * 1000 + i;
                var product = Product.Create(
                    ProductId.New(), merchantIds[n % MerchantCount], $"{Words[n % Words.Length]} number {n}",
                    $"A fine {Words[(n + 3) % Words.Length]} for everyday use", leaves[n % leaves.Count], [],
                    [new VariantSpec([], Money.From(10_000 + n), n % 7, 300)], now.AddSeconds(n));
                product.Publish();
                db.Products.Add(product);
            }

            await db.SaveChangesAsync(ct);
        }

        var home = tree.Single(c => c.Name == "Home").Id;
        string[] requests =
        [
            "/api/marketplace/products",
            "/api/marketplace/products?query=mugs",
            "/api/marketplace/products?query=lamp everyday",
            $"/api/marketplace/products?categoryId={home}",
            $"/api/marketplace/products?categoryId={home}&query=plates&page=3",
        ];

        // The data really is there: every Product is Published and visible.
        (await App.Client.BrowseAsync("pageSize=1")).TotalCount.ShouldBeGreaterThanOrEqualTo(ProductCount);
        foreach (var url in requests) (await App.Client.GetAsync(url, ct)).StatusCode.ShouldBe(HttpStatusCode.OK); // warm up

        var timings = new List<double>();
        for (var i = 0; i < 50; i++)
        {
            var stopwatch = Stopwatch.StartNew();
            var response = await App.Client.GetAsync(requests[i % requests.Length], ct);
            stopwatch.Stop();
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            timings.Add(stopwatch.Elapsed.TotalMilliseconds);
        }

        var p95 = timings.Order().ElementAt((int)Math.Ceiling(timings.Count * 0.95) - 1);
        TestContext.Current.SendDiagnosticMessage($"Marketplace p95 over {timings.Count} requests: {p95:F0} ms");
        p95.ShouldBeLessThan(1000, $"p95 was {p95:F0} ms over {timings.Count} requests");
        Console.WriteLine($"SC-003: Marketplace p95 {p95:F0} ms over {timings.Count} requests with {ProductCount} Products");
    }

    private static IEnumerable<Guid> Leaves(IEnumerable<CatalogTestData.CategoryNodeDto> nodes) =>
        nodes.SelectMany(n => n.Children.Count == 0 ? [n.Id] : Leaves(n.Children));
}

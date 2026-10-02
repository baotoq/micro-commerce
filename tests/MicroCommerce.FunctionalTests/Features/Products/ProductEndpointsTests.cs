using System.Net;
using MicroCommerce.ApiService.Features.Products;
using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.FunctionalTests.Features.Products;

public class ProductEndpointsTests(ApiFixture App) : TestBase<ApiFixture>
{
    [Fact]
    public async Task Create_then_get_returns_the_product_and_caches_it()
    {
        var (createRsp, created) = await App.Client.POSTAsync<CreateProductEndpoint, CreateProductRequest, ProductResponse>(
            new("Coffee Mug", 12.5m));

        createRsp.StatusCode.ShouldBe(HttpStatusCode.Created);
        createRsp.Headers.Location!.ToString().ShouldBe($"/products/{created.Id}");

        var (firstRsp, first) = await App.Client.GETAsync<GetProductEndpoint, GetProductRequest, ProductResponse>(
            new(created.Id));
        var (secondRsp, second) = await App.Client.GETAsync<GetProductEndpoint, GetProductRequest, ProductResponse>(
            new(created.Id));

        first.ShouldBe(created);
        second.ShouldBe(created);
        firstRsp.Headers.GetValues("X-Cache").ShouldBe(["MISS"]);
        secondRsp.Headers.GetValues("X-Cache").ShouldBe(["HIT"]);
    }

    [Fact]
    public async Task Create_with_invalid_input_returns_problem_details()
    {
        var (rsp, problem) = await App.Client.POSTAsync<CreateProductEndpoint, CreateProductRequest, ProblemDetails>(
            new("", -1m));

        rsp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.Errors.Select(e => e.Name).ShouldBe(["name", "price"], ignoreOrder: true);
    }

    [Fact]
    public async Task Get_unknown_product_returns_not_found()
    {
        var (rsp, _) = await App.Client.GETAsync<GetProductEndpoint, GetProductRequest, ProductResponse>(
            new(ProductId.New()));

        rsp.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task List_includes_created_product()
    {
        var (_, created) = await App.Client.POSTAsync<CreateProductEndpoint, CreateProductRequest, ProductResponse>(
            new("Teapot", 30m));

        var (rsp, products) = await App.Client.GETAsync<ListProductsEndpoint, List<ProductResponse>>();

        rsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        products.ShouldContain(created);
    }
}

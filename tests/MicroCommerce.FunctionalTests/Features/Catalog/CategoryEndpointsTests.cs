using System.Net;
using System.Net.Http.Json;

namespace MicroCommerce.FunctionalTests.Features.Catalog;

public sealed record CategoryDto(Guid Id, string Name, Guid? ParentId);

/// <summary>Platform Operators maintain the Category tree (User Story 6, FR-033 to FR-036).</summary>
public class CategoryEndpointsTests(ApiFixture App) : TestBase<ApiFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private HttpClient Operator => App.ClientFor(TestActor.PlatformOperator());

    private async Task<CategoryDto> CreateAsync(HttpClient client, string name, Guid? parentId = null)
    {
        var response = await client.PostAsJsonAsync("/api/ops/categories", new { name, parentId }, Ct);
        response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync(Ct));
        var category = (await response.Content.ReadFromJsonAsync<CategoryDto>(Ct))!;
        response.Headers.Location!.ToString().ShouldBe($"/api/ops/categories/{category.Id}");
        return category;
    }

    [Fact]
    public async Task Operators_create_top_level_and_child_categories_that_appear_in_the_tree()
    {
        var op = Operator;
        var top = await CreateAsync(op, CatalogTestData.Unique("Garden"));
        var child = await CreateAsync(op, "Tools", top.Id);

        child.ParentId.ShouldBe(top.Id);
        var tree = await CatalogTestData.CategoryTreeAsync(App.Client);
        tree.Single(c => c.Id == top.Id).Children.ShouldHaveSingleItem().Name.ShouldBe("Tools");
        (await op.GetFromJsonAsync<CategoryDto>($"/api/ops/categories/{child.Id}", Ct)).ShouldBe(child);
    }

    [Fact]
    public async Task Invalid_names_and_unknown_parents_are_refused_on_their_field()
    {
        var op = Operator;
        var blank = await op.PostAsJsonAsync("/api/ops/categories", new { name = " " }, Ct);
        var tooLong = await op.PostAsJsonAsync("/api/ops/categories", new { name = new string('n', 101) }, Ct);
        var unknownParent = await op.PostAsJsonAsync("/api/ops/categories", new { name = "Orphan", parentId = Guid.NewGuid() }, Ct);

        (await blank.ProblemAsync()).Errors!.Keys.ShouldBe(["name"]);
        (await tooLong.ProblemAsync()).Errors!.Keys.ShouldBe(["name"]);
        unknownParent.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await unknownParent.ProblemAsync()).Errors!.Keys.ShouldBe(["parentId"]);
    }

    [Fact]
    public async Task Sibling_names_are_unique_ignoring_case_including_at_the_top()
    {
        var op = Operator;
        var name = CatalogTestData.Unique("Pets");
        var top = await CreateAsync(op, name);
        await CreateAsync(op, "Food", top.Id);

        var topClash = await op.PostAsJsonAsync("/api/ops/categories", new { name = name.ToUpperInvariant() }, Ct);
        var childClash = await op.PostAsJsonAsync("/api/ops/categories", new { name = "FOOD", parentId = top.Id }, Ct);
        var seededClash = await op.PostAsJsonAsync("/api/ops/categories", new { name = "home" }, Ct);

        foreach (var response in new[] { topClash, childClash, seededClash })
        {
            response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
            (await response.ProblemAsync()).Type.ShouldBe("category-name-taken");
        }
    }

    [Fact]
    public async Task A_category_holding_products_gains_no_children_and_is_not_deleted()
    {
        var op = Operator;
        var leaf = await CreateAsync(op, CatalogTestData.Unique("Toys"));
        var (merchant, _, _) = await App.NewMerchantAsync();
        await merchant.CreateProductAsync(ProductHelpers.ProductRequest("Kite", leaf.Id));

        var addChild = await op.PostAsJsonAsync("/api/ops/categories", new { name = "Outdoor", parentId = leaf.Id }, Ct);
        var delete = await op.DeleteAsync($"/api/ops/categories/{leaf.Id}", Ct);

        foreach (var response in new[] { addChild, delete })
        {
            response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
            (await response.ProblemAsync()).Type.ShouldBe("category-has-products");
        }
    }

    [Fact]
    public async Task Renaming_keeps_the_products_and_the_marketplace_shows_the_new_name()
    {
        var op = Operator;
        var leaf = await CreateAsync(op, CatalogTestData.Unique("Books"));
        var (merchant, _, _) = await App.NewMerchantAsync();
        var product = await merchant.PublishAsync(await merchant.CreateProductAsync(ProductHelpers.ProductRequest("Novel", leaf.Id)));
        var newName = CatalogTestData.Unique("Reading");

        var response = await op.PutAsJsonAsync($"/api/ops/categories/{leaf.Id}", new { name = newName }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<CategoryDto>(Ct))!.Name.ShouldBe(newName);
        var detail = (await App.Client.GetFromJsonAsync<ProductDetailDto>($"/api/marketplace/products/{product.Id}", Ct))!;
        detail.Category.ShouldBe(new CategoryRefDto(leaf.Id, newName));
    }

    [Fact]
    public async Task Renaming_to_a_siblings_name_or_an_unknown_category_is_refused()
    {
        var op = Operator;
        var top = await CreateAsync(op, CatalogTestData.Unique("Music"));
        await CreateAsync(op, "Vinyl", top.Id);
        var cds = await CreateAsync(op, "CDs", top.Id);

        var clash = await op.PutAsJsonAsync($"/api/ops/categories/{cds.Id}", new { name = "vinyl" }, Ct);
        var unknown = await op.PutAsJsonAsync($"/api/ops/categories/{Guid.NewGuid()}", new { name = "Anything" }, Ct);
        var blank = await op.PutAsJsonAsync($"/api/ops/categories/{cds.Id}", new { name = "" }, Ct);

        clash.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await clash.ProblemAsync()).Type.ShouldBe("category-name-taken");
        unknown.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        blank.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Only_empty_leaves_are_deleted()
    {
        var op = Operator;
        var top = await CreateAsync(op, CatalogTestData.Unique("Crafts"));
        var leaf = await CreateAsync(op, "Yarn", top.Id);

        var withChildren = await op.DeleteAsync($"/api/ops/categories/{top.Id}", Ct);
        withChildren.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await withChildren.ProblemAsync()).Type.ShouldBe("category-has-children");

        (await op.DeleteAsync($"/api/ops/categories/{leaf.Id}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await op.GetAsync($"/api/ops/categories/{leaf.Id}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await op.DeleteAsync($"/api/ops/categories/{leaf.Id}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Only_platform_operators_change_the_tree()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var id = Guid.NewGuid();

        foreach (var (client, expected) in new[] { (App.ClientFor(TestActor.Buyer()), HttpStatusCode.Forbidden), (merchant, HttpStatusCode.Forbidden), (App.Client, HttpStatusCode.Unauthorized) })
        {
            (await client.PostAsJsonAsync("/api/ops/categories", new { name = "Nope" }, Ct)).StatusCode.ShouldBe(expected);
            (await client.GetAsync($"/api/ops/categories/{id}", Ct)).StatusCode.ShouldBe(expected);
            (await client.PutAsJsonAsync($"/api/ops/categories/{id}", new { name = "Nope" }, Ct)).StatusCode.ShouldBe(expected);
            (await client.DeleteAsync($"/api/ops/categories/{id}", Ct)).StatusCode.ShouldBe(expected);
        }
    }
}

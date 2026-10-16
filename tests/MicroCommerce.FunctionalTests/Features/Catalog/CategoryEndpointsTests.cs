using System.Net;
using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.FunctionalTests.Features.Catalog;

public class CategoryEndpointsTests(ApiFixture App) : TestBase<ApiFixture>
{
    private readonly HttpClient _operator = App.ClientFor(TestActor.PlatformOperator());

    // The tree is shared across the test run, so every test makes its own uniquely named Categories.
    private static string UniqueName(string prefix = "Category") => $"{prefix} {Guid.NewGuid():N}";

    private async Task<CategoryResponse> CreateAsync(string name, CategoryId? parentId = null)
    {
        var (rsp, category) = await _operator
            .POSTAsync<CreateCategoryEndpoint, CreateCategoryRequest, CategoryResponse>(new(name, parentId));
        rsp.StatusCode.ShouldBe(HttpStatusCode.Created);
        return category;
    }

    private async Task<IReadOnlyList<CategoryNodeResponse>> TreeAsync()
    {
        var (rsp, tree) = await App.Client.GETAsync<GetCategoryTreeEndpoint, IReadOnlyList<CategoryNodeResponse>>();
        rsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        return tree;
    }

    /// <summary>The node for <paramref name="id"/> anywhere in the tree, or null.</summary>
    private static CategoryNodeResponse? Find(IEnumerable<CategoryNodeResponse> nodes, CategoryId id) =>
        nodes.Select(n => n.Id == id ? n : Find(n.Children, id)).FirstOrDefault(n => n is not null);

    [Fact]
    public async Task Operator_creates_a_top_level_category()
    {
        var name = UniqueName("Fashion");

        var (rsp, category) = await _operator
            .POSTAsync<CreateCategoryEndpoint, CreateCategoryRequest, CategoryResponse>(new($"  {name}  ", null));

        rsp.StatusCode.ShouldBe(HttpStatusCode.Created);
        rsp.Headers.Location!.ToString().ShouldBe($"/ops/categories/{category.Id}");
        category.Name.ShouldBe(name);
        category.ParentId.ShouldBeNull();

        var (_, read) = await _operator.GETAsync<GetCategoryEndpoint, GetCategoryRequest, CategoryResponse>(new(category.Id));
        read.ShouldBe(category);
        (await TreeAsync()).ShouldContain(n => n.Id == category.Id && n.Name == name && n.Children.Count == 0);
    }

    [Fact]
    public async Task Anyone_reads_the_tree_with_children_nested_and_ordered_by_name()
    {
        var fashion = await CreateAsync(UniqueName("Fashion"));
        var women = await CreateAsync("Women", fashion.Id);
        var men = await CreateAsync("Men", fashion.Id);
        var shirts = await CreateAsync("Shirts", men.Id);
        var tShirts = await CreateAsync("T-Shirts", men.Id);

        var node = (await TreeAsync()).Single(n => n.Id == fashion.Id);

        node.ShouldBe(new CategoryNodeResponse(fashion.Id, fashion.Name, [
            new(men.Id, "Men", [new(shirts.Id, "Shirts", []), new(tShirts.Id, "T-Shirts", [])]),
            new(women.Id, "Women", []),
        ]), new CategoryNodeComparer());
    }

    [Fact]
    public async Task Child_category_has_its_parent()
    {
        var parent = await CreateAsync(UniqueName());

        var child = await CreateAsync("Kitchen", parent.Id);

        child.ParentId.ShouldBe(parent.Id);
    }

    [Fact]
    public async Task Creating_under_an_unknown_parent_is_a_bad_request()
    {
        var (rsp, problem) = await _operator.POSTAsync<CreateCategoryEndpoint, CreateCategoryRequest, ProblemDetails>(
            new(UniqueName(), CategoryId.New()));

        rsp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.Errors.Select(e => e.Name).ShouldBe(["parentId"]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Creating_without_a_name_is_a_bad_request(string name)
    {
        var (rsp, problem) = await _operator.POSTAsync<CreateCategoryEndpoint, CreateCategoryRequest, ProblemDetails>(
            new(name, null));

        rsp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.Errors.Select(e => e.Name).ShouldBe(["name"]);
    }

    [Fact]
    public async Task Creating_with_a_too_long_name_is_a_bad_request()
    {
        var (rsp, problem) = await _operator.POSTAsync<CreateCategoryEndpoint, CreateCategoryRequest, ProblemDetails>(
            new(new string('a', 101), null));

        rsp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.Errors.Select(e => e.Name).ShouldBe(["name"]);
    }

    [Fact]
    public async Task Operator_renames_a_category_and_it_keeps_its_children()
    {
        var parent = await CreateAsync(UniqueName("Hom"));
        var child = await CreateAsync("Kitchen", parent.Id);
        var newName = UniqueName("Home");

        var (rsp, renamed) = await _operator.PUTAsync<RenameCategoryEndpoint, RenameCategoryRequest, CategoryResponse>(
            new(parent.Id, newName));

        rsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        renamed.ShouldBe(parent with { Name = newName });
        var node = Find(await TreeAsync(), parent.Id).ShouldNotBeNull();
        node.Name.ShouldBe(newName);
        node.Children.ShouldHaveSingleItem().Id.ShouldBe(child.Id);
    }

    [Fact]
    public async Task Renaming_a_child_keeps_it_under_its_parent()
    {
        var parent = await CreateAsync(UniqueName());
        var child = await CreateAsync("Kitchn", parent.Id);

        var (_, renamed) = await _operator.PUTAsync<RenameCategoryEndpoint, RenameCategoryRequest, CategoryResponse>(
            new(child.Id, "Kitchen"));

        renamed.ShouldBe(child with { Name = "Kitchen" });
        Find(await TreeAsync(), parent.Id).ShouldNotBeNull().Children.ShouldHaveSingleItem().Name.ShouldBe("Kitchen");
    }

    [Fact]
    public async Task Renaming_to_an_empty_name_is_a_bad_request()
    {
        var category = await CreateAsync(UniqueName());

        var (rsp, problem) = await _operator.PUTAsync<RenameCategoryEndpoint, RenameCategoryRequest, ProblemDetails>(
            new(category.Id, " "));

        rsp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.Errors.Select(e => e.Name).ShouldBe(["name"]);
    }

    [Fact]
    public async Task Renaming_an_unknown_category_is_not_found()
    {
        var (rsp, _) = await _operator.PUTAsync<RenameCategoryEndpoint, RenameCategoryRequest, ProblemDetails>(
            new(CategoryId.New(), UniqueName()));

        rsp.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Operator_deletes_a_leaf_category()
    {
        var parent = await CreateAsync(UniqueName());
        var leaf = await CreateAsync("Leaf", parent.Id);

        var rsp = await _operator.DELETEAsync<DeleteCategoryEndpoint, DeleteCategoryRequest>(new(leaf.Id));

        rsp.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        Find(await TreeAsync(), parent.Id).ShouldNotBeNull().Children.ShouldBeEmpty();
        var (getRsp, _) = await _operator.GETAsync<GetCategoryEndpoint, GetCategoryRequest, CategoryResponse>(new(leaf.Id));
        getRsp.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deleting_a_category_with_children_is_a_conflict()
    {
        var parent = await CreateAsync(UniqueName());
        var child = await CreateAsync("Child", parent.Id);

        var (rsp, problem) = await _operator.DELETEAsync<DeleteCategoryEndpoint, DeleteCategoryRequest, ProblemDetails>(
            new(parent.Id));

        rsp.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        problem.Errors.ShouldHaveSingleItem().Name.ShouldBe("generalErrors");
        Find(await TreeAsync(), parent.Id).ShouldNotBeNull().Children.ShouldHaveSingleItem().Id.ShouldBe(child.Id);
    }

    [Fact]
    public async Task Deleting_an_unknown_category_is_not_found()
    {
        var rsp = await _operator.DELETEAsync<DeleteCategoryEndpoint, DeleteCategoryRequest>(new(CategoryId.New()));

        rsp.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Getting_an_unknown_category_is_not_found()
    {
        var (rsp, _) = await _operator.GETAsync<GetCategoryEndpoint, GetCategoryRequest, CategoryResponse>(new(CategoryId.New()));

        rsp.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Ops_endpoints_refuse_accounts_without_the_operator_role()
    {
        var category = await CreateAsync(UniqueName());

        var statuses = await OpsStatusesAsync(App.ClientFor(TestActor.Buyer()), category.Id);

        statuses.ShouldAllBe(s => s == HttpStatusCode.Forbidden);
        (await TreeAsync()).ShouldContain(n => n.Id == category.Id && n.Name == category.Name && n.Children.Count == 0);
    }

    [Fact]
    public async Task Ops_endpoints_refuse_anonymous_requests()
    {
        var category = await CreateAsync(UniqueName());

        var statuses = await OpsStatusesAsync(App.Client, category.Id);

        statuses.ShouldAllBe(s => s == HttpStatusCode.Unauthorized);
        (await TreeAsync()).ShouldContain(n => n.Id == category.Id && n.Name == category.Name && n.Children.Count == 0);
    }

    [Fact]
    public async Task Starter_tree_is_not_seeded_outside_development()
    {
        (await TreeAsync()).ShouldNotContain(n => n.Name == "Fashion" || n.Name == "Home" || n.Name == "Electronics");
    }

    /// <summary>Tries every <c>/ops</c> Category endpoint against <paramref name="id"/>.</summary>
    private static async Task<HttpStatusCode[]> OpsStatusesAsync(HttpClient client, CategoryId id) =>
    [
        (await client.POSTAsync<CreateCategoryEndpoint, CreateCategoryRequest, ProblemDetails>(new(UniqueName(), null))).Response.StatusCode,
        (await client.POSTAsync<CreateCategoryEndpoint, CreateCategoryRequest, ProblemDetails>(new(UniqueName(), id))).Response.StatusCode,
        (await client.GETAsync<GetCategoryEndpoint, GetCategoryRequest, ProblemDetails>(new(id))).Response.StatusCode,
        (await client.PUTAsync<RenameCategoryEndpoint, RenameCategoryRequest, ProblemDetails>(new(id, UniqueName()))).Response.StatusCode,
        (await client.DELETEAsync<DeleteCategoryEndpoint, DeleteCategoryRequest>(new(id))).StatusCode,
    ];

    /// <summary>Compares trees by value: records compare their <c>Children</c> lists by reference.</summary>
    private sealed class CategoryNodeComparer : IEqualityComparer<CategoryNodeResponse>
    {
        public bool Equals(CategoryNodeResponse? x, CategoryNodeResponse? y) =>
            x is not null && y is not null && x.Id == y.Id && x.Name == y.Name && x.Children.SequenceEqual(y.Children, this);

        public int GetHashCode(CategoryNodeResponse obj) => obj.Id.GetHashCode();
    }
}

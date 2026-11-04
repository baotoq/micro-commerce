using System.Net;
using System.Text.Json.Nodes;

namespace MicroCommerce.FunctionalTests;

/// <summary>Health checks and the OpenAPI document are served without a token (FR-003, FR-017).</summary>
public class InfrastructureEndpointsTests(ApiFixture App) : TestBase<ApiFixture>
{
    [Theory]
    [InlineData("/health")]
    [InlineData("/alive")]
    public async Task Health_endpoints_are_anonymous(string path)
    {
        var response = await App.Client.GetAsync(path, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OpenApi_document_is_served_without_dapr_routes()
    {
        var document = await DocumentAsync();

        document["openapi"]!.GetValue<string>().ShouldStartWith("3.");
        var paths = document["paths"]!.AsObject().Select(p => p.Key).ToList();
        paths.ShouldNotBeEmpty();
        paths.ShouldAllBe(p => p.StartsWith("/api/"));
        paths.ShouldNotContain(p => p.Contains("/dapr/"));
    }

    /// <summary>
    /// IDs reach clients as UUID strings. MeResponse.AccountId is already a Guid, so this passes without
    /// the Vogen schema transformer too; VogenIdSchemaTransformerTests checks the transformer itself.
    /// </summary>
    [Fact]
    public async Task Account_id_is_a_uuid_string()
    {
        var document = await DocumentAsync();

        var schemas = document["components"]!["schemas"]!.AsObject();
        var me = schemas.Single(s => s.Key.EndsWith("MeResponse")).Value!;
        var accountId = me["properties"]!["accountId"]!;

        accountId["type"]!.GetValue<string>().ShouldBe("string");
        accountId["format"]!.GetValue<string>().ShouldBe("uuid");
    }

    private async Task<JsonNode> DocumentAsync()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await App.Client.GetAsync("/openapi/v1.json", ct);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return JsonNode.Parse(await response.Content.ReadAsStringAsync(ct))!;
    }
}

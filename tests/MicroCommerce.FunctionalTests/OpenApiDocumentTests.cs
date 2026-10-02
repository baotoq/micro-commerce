using System.Text.Json.Nodes;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Products;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace MicroCommerce.FunctionalTests;

/// <summary>
/// Value objects must reach the OpenAPI document as the primitives they wrap, so clients see plain strings.
/// The document is generated in-process: <c>/openapi/v1.json</c> is only mapped in Development.
/// </summary>
public class OpenApiDocumentTests(ApiFixture App) : TestBase<ApiFixture>
{
    [Theory]
    [InlineData(typeof(ProductResponse))]
    [InlineData(typeof(MerchantResponse))]
    [InlineData(typeof(MeResponse))]
    public async Task Typed_ids_are_uuid_strings(Type response)
    {
        var document = await DocumentAsync();

        var resolved = Resolve(document, Schema(document, response)["properties"]!["id"]!);

        resolved["type"]!.GetValue<string>().ShouldBe("string");
        resolved["format"]!.GetValue<string>().ShouldBe("uuid");
    }

    [Fact]
    public async Task Shop_name_is_a_string()
    {
        var document = await DocumentAsync();

        var resolved = Resolve(document, Schema(document, typeof(MerchantResponse))["properties"]!["shopName"]!);

        resolved["type"]!.GetValue<string>().ShouldBe("string");
    }

    [Fact]
    public async Task Product_id_route_parameter_is_a_uuid_string()
    {
        var document = await DocumentAsync();

        var parameter = document["paths"]!["/products/{id}"]!["get"]!["parameters"]!.AsArray()
            .Single(p => p!["name"]!.GetValue<string>() == "id")!;
        var resolved = Resolve(document, parameter["schema"]!);

        resolved["type"]!.GetValue<string>().ShouldBe("string");
        resolved["format"]!.GetValue<string>().ShouldBe("uuid");
    }

    private async Task<JsonNode> DocumentAsync()
    {
        var provider = App.Services.GetRequiredKeyedService<IOpenApiDocumentProvider>("v1");
        var document = await provider.GetOpenApiDocumentAsync(TestContext.Current.CancellationToken);
        var json = await document.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_1, TestContext.Current.CancellationToken);
        return JsonNode.Parse(json)!;
    }

    /// <summary>FastEndpoints names component schemas after the type's full name, without the dots.</summary>
    private static JsonNode Schema(JsonNode document, Type type) => Component(document, type.FullName!.Replace(".", ""));

    private static JsonNode Component(JsonNode document, string name) => document["components"]!["schemas"]![name]!;

    /// <summary>Follows a <c>$ref</c> to its component schema, if the schema is one.</summary>
    private static JsonNode Resolve(JsonNode document, JsonNode schema) =>
        schema["$ref"]?.GetValue<string>() is { } reference
            ? Component(document, reference["#/components/schemas/".Length..])
            : schema;
}

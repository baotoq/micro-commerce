using System.Text.Json;
using MicroCommerce.ApiService.Infrastructure.OpenApi;
using MicroCommerce.Domain.SharedKernel;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MicroCommerce.UnitTests.Infrastructure;

/// <summary>
/// Typed IDs appear in the OpenAPI document as UUID strings. No response in the skeleton exposes a
/// typed ID directly (they map to Guid), so the transformer is checked on its own.
/// </summary>
public class VogenIdSchemaTransformerTests
{
    [Theory]
    [InlineData(typeof(AccountId))]
    [InlineData(typeof(AccountId?))]
    public async Task Typed_id_schema_becomes_a_uuid_string(Type type)
    {
        var schema = new OpenApiSchema { Type = JsonSchemaType.Object };

        await new VogenIdSchemaTransformer().TransformAsync(schema, ContextFor(type), TestContext.Current.CancellationToken);

        schema.Type.ShouldBe(JsonSchemaType.String);
        schema.Format.ShouldBe("uuid");
    }

    [Fact]
    public async Task Other_schemas_are_left_alone()
    {
        var schema = new OpenApiSchema { Type = JsonSchemaType.Object };

        await new VogenIdSchemaTransformer().TransformAsync(schema, ContextFor(typeof(string)), TestContext.Current.CancellationToken);

        schema.Type.ShouldBe(JsonSchemaType.Object);
        schema.Format.ShouldBeNull();
    }

    private static OpenApiSchemaTransformerContext ContextFor(Type type) => new()
    {
        DocumentName = "v1",
        JsonTypeInfo = JsonSerializerOptions.Default.GetTypeInfo(type),
        JsonPropertyInfo = null,
        ParameterDescription = null,
        ApplicationServices = null!,
    };
}

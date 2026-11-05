using MicroCommerce.Domain.Catalog;
using MicroCommerce.Domain.Diagnostics;
using MicroCommerce.Domain.SharedKernel;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MicroCommerce.ApiService.Infrastructure.OpenApi;

/// <summary>
/// Shows typed IDs in the OpenAPI document as the UUID strings they are on the wire. Hand-written
/// instead of Vogen's generated mapping, which would pull OpenAPI types into the Domain project
/// (research R2). Add every new ID type here, next to its <c>[EfCoreConverter&lt;T&gt;]</c> line.
/// </summary>
public sealed class VogenIdSchemaTransformer : IOpenApiSchemaTransformer
{
    public static readonly IReadOnlySet<Type> IdTypes = new HashSet<Type>
    {
        typeof(AccountId),
        typeof(SampleProcessId),
        typeof(MerchantId),
        typeof(CategoryId),
        typeof(ProductId),
        typeof(VariantId),
        typeof(ProductImageId),
    };

    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = Nullable.GetUnderlyingType(context.JsonTypeInfo.Type) ?? context.JsonTypeInfo.Type;
        if (IdTypes.Contains(type))
        {
            schema.Type = JsonSchemaType.String;
            schema.Format = "uuid";
            schema.Properties?.Clear();
        }

        return Task.CompletedTask;
    }
}

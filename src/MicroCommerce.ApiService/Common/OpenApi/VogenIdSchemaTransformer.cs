using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MicroCommerce.ApiService.Common.OpenApi;

internal sealed class VogenIdSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        Type type = context.JsonTypeInfo.Type;

        // Vogen generates types with ValueObjectAttribute
        bool isVogenId = type.GetCustomAttributes(inherit: false)
            .Any(a => a.GetType().Name.Contains("ValueObject"));

        if (!isVogenId)
        {
            // Fallback: check if the type is a value type with a single Value property of type Guid
            isVogenId = type.IsValueType && type.GetProperty("Value")?.PropertyType == typeof(Guid);
        }

        if (isVogenId)
        {
            schema.Type = JsonSchemaType.String;
            schema.Format = "uuid";
            schema.Properties?.Clear();
            schema.AdditionalProperties = null;
        }

        return Task.CompletedTask;
    }
}

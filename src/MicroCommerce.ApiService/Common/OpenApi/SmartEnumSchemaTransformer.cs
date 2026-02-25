using System.Text.Json.Nodes;
using Ardalis.SmartEnum;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MicroCommerce.ApiService.Common.OpenApi;

internal sealed class SmartEnumSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        Type type = context.JsonTypeInfo.Type;

        if (IsSmartEnumType(type))
        {
            schema.Type = JsonSchemaType.String;
            schema.Format = null;
            schema.Properties?.Clear();
            schema.AdditionalProperties = null;

            // Populate enum values from SmartEnum.List via reflection
            Type? smartEnumBase = FindSmartEnumBase(type);
            if (smartEnumBase is not null)
            {
                System.Reflection.PropertyInfo? listProp = smartEnumBase.GetProperty("List");
                if (listProp?.GetValue(null) is System.Collections.IEnumerable values)
                {
                    schema.Enum = [];
                    foreach (object val in values)
                    {
                        System.Reflection.PropertyInfo? nameProp = val.GetType().GetProperty("Name");
                        string? name = nameProp?.GetValue(val)?.ToString();
                        if (name is not null)
                        {
                            schema.Enum.Add(JsonValue.Create(name));
                        }
                    }
                }
            }
        }

        return Task.CompletedTask;
    }

    private static bool IsSmartEnumType(Type type)
    {
        return FindSmartEnumBase(type) is not null;
    }

    private static Type? FindSmartEnumBase(Type type)
    {
        Type? current = type.BaseType;
        while (current is not null)
        {
            if (current.IsGenericType &&
                current.GetGenericTypeDefinition() == typeof(SmartEnum<>))
            {
                return current;
            }
            current = current.BaseType;
        }
        return null;
    }
}

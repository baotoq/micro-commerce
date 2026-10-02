using Vogen;

// Every value object maps to its primitive in EF Core and JSON, and appears as that primitive in the OpenAPI
// document (via the generated MapVogenTypesInMicroCommerce_ApiService schema transformer).
[assembly: VogenDefaults(
    conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson,
    openApiSchemaCustomizations: OpenApiSchemaCustomizations.GenerateOpenApiMappingExtensionMethod)]

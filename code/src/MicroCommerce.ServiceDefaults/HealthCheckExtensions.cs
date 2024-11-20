using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MicroCommerce.ServiceDefaults;

public static class HealthCheckExtensions
{
    private static readonly Lazy<JsonSerializerOptions> s_options = new(CreateJsonOptions);

    public static Task WriteResponse(HttpContext context, HealthReport report)
    {
        string json = JsonSerializer.Serialize(report, s_options.Value);

        context.Response.ContentType = MediaTypeNames.Application.Json;
        return context.Response.WriteAsync(json);
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        options.Converters.Add(new JsonStringEnumConverter());

        return options;
    }
}

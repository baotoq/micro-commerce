using System.Runtime.CompilerServices;
using DiffEngine;
using VerifyTests;

namespace MicroCommerce.CartService.Api.IntegrationTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyHttp.Initialize();
        VerifierSettings.AddExtraSettings(settings =>
        {
            settings.Converters.Add(new HttpResponseMessageConverter());
        });
        DiffRunner.Disabled = true;
    }
}

public class HttpResponseMessageConverter : WriteOnlyJsonConverter<HttpResponseMessage>
{
    public override void Write(VerifyJsonWriter writer, HttpResponseMessage response)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("StatusCode");
        writer.WriteValue((int)response.StatusCode);
        writer.WritePropertyName("Headers");
        writer.Serialize(response.Headers.ToDictionary(h => h.Key, h => h.Value));

        if (response.Content != null)
        {
            writer.WritePropertyName("Content");
            var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            writer.WriteValue(content);
        }

        writer.WriteEndObject();
    }
}

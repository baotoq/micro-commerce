using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MicroCommerce.FunctionalTests;

/// <summary>Tiny files whose first bytes are each format's signature; the API judges content, not names.</summary>
public static class TestImages
{
    public static byte[] Png(int size = 64) => Pad([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A], size);

    public static byte[] Jpeg(int size = 64) => Pad([0xFF, 0xD8, 0xFF, 0xE0], size);

    public static byte[] WebP(int size = 64) => Pad([.. "RIFF"u8, 0x24, 0, 0, 0, .. "WEBP"u8], size);

    public static byte[] Text() => "just some text, not an image"u8.ToArray();

    private static byte[] Pad(byte[] head, int size)
    {
        var bytes = new byte[size];
        head.CopyTo(bytes, 0);
        for (var i = head.Length; i < size; i++) bytes[i] = (byte)(i % 251);
        return bytes;
    }

    public static async Task<HttpResponseMessage> UploadAsync(this HttpClient client, Guid productId, byte[] bytes, string fileName = "photo.png", string contentType = "image/png")
    {
        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(file, "file", fileName);
        return await client.PostAsync($"/api/merchant/products/{productId}/images", form, TestContext.Current.CancellationToken);
    }

    public static async Task<MerchantProductDto> UploadOkAsync(this HttpClient client, Guid productId, byte[]? bytes = null)
    {
        var response = await client.UploadAsync(productId, bytes ?? Png());
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created, await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        return (await response.Content.ReadFromJsonAsync<MerchantProductDto>(TestContext.Current.CancellationToken))!;
    }
}

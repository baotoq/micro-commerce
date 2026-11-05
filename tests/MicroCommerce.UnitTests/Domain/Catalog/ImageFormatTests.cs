using MicroCommerce.Domain.Catalog;

namespace MicroCommerce.UnitTests.Domain.Catalog;

public class ImageFormatTests
{
    internal static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0, 0x10, (byte)'J', (byte)'F', (byte)'I', (byte)'F', 0, 1];
    internal static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0x0D];
    internal static readonly byte[] WebP = [.. "RIFF"u8, 0x24, 0, 0, 0, .. "WEBP"u8];

    [Fact]
    public void Detects_jpeg_png_and_webp_by_their_signatures()
    {
        ImageFormats.Detect(Jpeg).ShouldBe(ImageFormat.Jpeg);
        ImageFormats.Detect(Png).ShouldBe(ImageFormat.Png);
        ImageFormats.Detect(WebP).ShouldBe(ImageFormat.WebP);
    }

    [Fact]
    public void Anything_else_is_not_an_image()
    {
        ImageFormats.Detect("GIF89a......"u8).ShouldBeNull();
        ImageFormats.Detect("hello, world"u8).ShouldBeNull();
        ImageFormats.Detect([]).ShouldBeNull();
        ImageFormats.Detect("RIFF....AVI "u8).ShouldBeNull();
        ImageFormats.Detect(Png[..7]).ShouldBeNull();
    }

    [Fact]
    public void Each_format_has_its_content_type()
    {
        ImageFormat.Jpeg.ContentType().ShouldBe("image/jpeg");
        ImageFormat.Png.ContentType().ShouldBe("image/png");
        ImageFormat.WebP.ContentType().ShouldBe("image/webp");
    }
}

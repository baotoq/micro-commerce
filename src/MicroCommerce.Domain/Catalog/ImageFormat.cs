namespace MicroCommerce.Domain.Catalog;

/// <summary>The image formats a Product image may have (FR-020).</summary>
public enum ImageFormat
{
    Jpeg,
    Png,
    WebP,
}

public static class ImageFormats
{
    /// <summary>How many leading bytes <see cref="Detect"/> needs.</summary>
    public const int HeaderLength = 12;

    /// <summary>
    /// Judges the format from the file's first bytes (its signature), never its name or declared type.
    /// Null when it is not a JPEG, PNG or WebP image.
    /// </summary>
    public static ImageFormat? Detect(ReadOnlySpan<byte> head)
    {
        if (head.Length >= 3 && head[0] == 0xFF && head[1] == 0xD8 && head[2] == 0xFF) return ImageFormat.Jpeg;
        if (head.Length >= 8 && head[..8].SequenceEqual((ReadOnlySpan<byte>)[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A])) return ImageFormat.Png;
        if (head.Length >= 12 && head[..4].SequenceEqual("RIFF"u8) && head[8..12].SequenceEqual("WEBP"u8)) return ImageFormat.WebP;
        return null;
    }

    public static string ContentType(this ImageFormat format) => format switch
    {
        ImageFormat.Jpeg => "image/jpeg",
        ImageFormat.Png => "image/png",
        ImageFormat.WebP => "image/webp",
        _ => throw new ArgumentOutOfRangeException(nameof(format)),
    };
}

namespace MicroCommerce.Domain.Catalog;

/// <summary>A picture of a Product at a position in its ordered images. Position 0 is the cover (FR-021).</summary>
public sealed class ProductImage
{
    public const long MaxSizeBytes = 5 * 1024 * 1024;

    private ProductImage()
    {
    }

    public ProductImageId Id { get; private init; }

    /// <summary>Dense, 0..n−1. Position 0 is the cover.</summary>
    public int Position { get; internal set; }

    /// <summary>Judged from the file's content, never its name (FR-020).</summary>
    public ImageFormat Format { get; private init; }

    public long SizeBytes { get; private init; }

    public DateTimeOffset UploadedAt { get; private init; }

    internal static ProductImage Create(ProductImageId id, int position, ImageFormat format, long sizeBytes, DateTimeOffset now) =>
        new() { Id = id, Position = position, Format = format, SizeBytes = sizeBytes, UploadedAt = now };
}

using MicroCommerce.Domain.SharedKernel;

namespace MicroCommerce.Domain.Catalog;

/// <summary>
/// A Merchant's listing: name, description, one leaf Category, up to two Options, the Variants that
/// are exactly their combinations, and up to 9 ordered images. Every change bumps <see cref="Version"/>,
/// the optimistic concurrency token for the whole aggregate (FR-019, research R5). The leaf rule needs
/// other Categories, so the Catalog's handlers check it under a row lock.
/// </summary>
public sealed class Product : AggregateRoot<ProductId>
{
    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 5000;

    private readonly List<Variant> _variants = [];
    private readonly List<ProductImage> _images = [];

    private Product()
    {
    }

    public MerchantId MerchantId { get; private init; }

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public CategoryId CategoryId { get; private set; }

    public ProductStatus Status { get; private set; }

    public IReadOnlyList<ProductOption> Options { get; private set; } = [];

    /// <summary>In combination order.</summary>
    public IReadOnlyList<Variant> Variants => _variants.OrderBy(v => v.Position).ToList();

    /// <summary>In position order; the first is the cover.</summary>
    public IReadOnlyList<ProductImage> Images => _images.OrderBy(i => i.Position).ToList();

    public DateTimeOffset CreatedAt { get; private init; }

    public int Version { get; private set; }

    public static Product Create(
        ProductId id,
        MerchantId merchantId,
        string name,
        string? description,
        CategoryId categoryId,
        IReadOnlyList<ProductOption> options,
        IReadOnlyList<VariantSpec> variants,
        DateTimeOffset now)
    {
        var product = new Product
        {
            Id = id,
            MerchantId = merchantId,
            Status = ProductStatus.Draft,
            CreatedAt = now,
            Version = 1,
        };
        product.SetDetails(name, description, categoryId);
        product.SetVariants(options, variants);
        return product;
    }

    /// <summary>Changes everything the Merchant edits. The status stays; surviving combinations keep their Variant IDs.</summary>
    public void Edit(string name, string? description, CategoryId categoryId, IReadOnlyList<ProductOption> options, IReadOnlyList<VariantSpec> variants)
    {
        EnsureNotRemoved();
        SetDetails(name, description, categoryId);
        SetVariants(options, variants);
        Version++;
    }

    public const int MaxImages = 9;

    /// <summary>Appends an image; the first image is the cover (FR-020, FR-021).</summary>
    public void AddImage(ProductImageId id, ImageFormat format, long sizeBytes, DateTimeOffset now)
    {
        EnsureNotRemoved();
        if (_images.Count >= MaxImages) throw new ImageLimitReachedException();
        if (sizeBytes is <= 0 or > ProductImage.MaxSizeBytes)
            throw new DomainRuleViolation($"An image must be at most {ProductImage.MaxSizeBytes} bytes");

        _images.Add(ProductImage.Create(id, _images.Count, format, sizeBytes, now));
        Version++;
    }

    /// <summary>Puts the images in the given order, which must name each current image exactly once.</summary>
    public void ReorderImages(IReadOnlyList<ProductImageId> order)
    {
        EnsureNotRemoved();
        if (order.Count != _images.Count || order.Distinct().Count() != order.Count || order.Any(id => _images.All(i => i.Id != id)))
            throw new DomainRuleViolation("The new order must list each of the Product's images exactly once");

        foreach (var (position, id) in order.Index()) _images.Single(i => i.Id == id).Position = position;
        Version++;
    }

    public void RemoveImage(ProductImageId id)
    {
        EnsureNotRemoved();
        var image = _images.SingleOrDefault(i => i.Id == id) ?? throw new DomainRuleViolation("The Product has no such image");
        _images.Remove(image);
        foreach (var (position, remaining) in _images.OrderBy(i => i.Position).Index()) remaining.Position = position;
        Version++;
    }

    public bool HasImage(ProductImageId id) => _images.Any(i => i.Id == id);

    /// <summary>A Platform Operator removes the Product. Final, and repeating it changes nothing (FR-038).</summary>
    public void Remove()
    {
        if (Status == ProductStatus.Removed) return;
        Status = ProductStatus.Removed;
        Version++;
    }

    public void Publish() => MoveTo(ProductStatus.Published);

    public void Unpublish() => MoveTo(ProductStatus.Draft);

    private void MoveTo(ProductStatus status)
    {
        EnsureNotRemoved();
        if (Status == status) return;
        Status = status;
        Version++;
    }

    private void EnsureNotRemoved()
    {
        if (Status == ProductStatus.Removed)
            throw new ProductRemovedException();
    }

    private void SetDetails(string name, string? description, CategoryId categoryId)
    {
        var trimmedName = name?.Trim() ?? string.Empty;
        if (trimmedName.Length is 0 or > MaxNameLength)
            throw new DomainRuleViolation($"A Product name must be 1 to {MaxNameLength} characters");

        var trimmedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (trimmedDescription?.Length > MaxDescriptionLength)
            throw new DomainRuleViolation($"A Product description must be at most {MaxDescriptionLength} characters");

        Name = trimmedName;
        Description = trimmedDescription;
        CategoryId = categoryId;
    }

    private void SetVariants(IReadOnlyList<ProductOption> options, IReadOnlyList<VariantSpec> specs)
    {
        ProductOption.CheckSet(options);
        var matched = VariantCombinations.Match(options, specs);

        var existing = _variants.ToDictionary(v => VariantCombinations.Key(Options, v.OptionValues), StringComparer.Ordinal);
        var next = new List<Variant>(matched.Count);
        foreach (var (position, (values, spec)) in matched.Index())
        {
            var key = VariantCombinations.Key(options, values);
            var variant = existing.Remove(key, out var kept) ? kept : Variant.Create(VariantId.New(), values, spec);
            variant.Update(values, spec);
            variant.Position = position;
            next.Add(variant);
        }

        Options = options.ToList();
        _variants.Clear();
        _variants.AddRange(next);
    }
}

/// <summary>A Platform Operator removed the Product, so its Merchant can't change it any more (FR-038).</summary>
public sealed class ProductRemovedException() : DomainRuleViolation("A Platform Operator removed this Product, so it can't be changed");

/// <summary>A Product already has the most images it may have (FR-020).</summary>
public sealed class ImageLimitReachedException() : DomainRuleViolation($"A Product can have at most {Product.MaxImages} images");

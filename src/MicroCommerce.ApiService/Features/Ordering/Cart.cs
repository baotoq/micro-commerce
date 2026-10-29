using FluentValidation;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.ApiService.Features.Ordering;

/// <summary>
/// An Account's working list of Variants it may buy, across any number of Merchants; one per Account. It holds no
/// stock and no price: <see cref="CartView"/> reads each line live from the Catalog.
/// </summary>
public class Cart
{
    public const int MaxQuantity = 99;
    public const int MaxLines = 50;

    /// <summary>The owning Account: one Cart per Account, so it is also the key.</summary>
    public required AccountId AccountId { get; init; }

    /// <summary>
    /// Bumped by every edit and checked when saving, so two concurrent edits can't both apply: the later one is a
    /// <see cref="DbUpdateConcurrencyException"/>.
    /// </summary>
    public int Version { get; private set; }

    /// <summary>In the order they were first added.</summary>
    public List<CartLine> Lines { get; } = [];

    /// <summary>Adds the Variant, or adds to its quantity if the Cart already has it. Run <see cref="CartValidator"/> after.</summary>
    public void Add(VariantId variantId, int quantity)
    {
        if (Find(variantId) is { } line)
            line.Quantity += quantity;
        else
            Lines.Add(new CartLine { AccountId = AccountId, VariantId = variantId, Quantity = quantity });
        Version++;
    }

    /// <returns>Whether the Cart has the Variant.</returns>
    public bool SetQuantity(VariantId variantId, int quantity)
    {
        if (Find(variantId) is not { } line) return false;
        line.Quantity = quantity;
        Version++;
        return true;
    }

    /// <returns>Whether the Cart had the Variant.</returns>
    public bool Remove(VariantId variantId)
    {
        if (Find(variantId) is not { } line) return false;
        Lines.Remove(line);
        Version++;
        return true;
    }

    private CartLine? Find(VariantId variantId) => Lines.Find(l => l.VariantId == variantId);
}

/// <summary>One Variant in a Cart with the quantity wanted.</summary>
public class CartLine
{
    public required AccountId AccountId { get; init; }
    public required VariantId VariantId { get; init; }
    public required int Quantity { get; set; }
    public DateTimeOffset AddedAt { get; init; } = DatabaseClock.UtcNow;
}

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.HasKey(c => c.AccountId);
        builder.Property(c => c.AccountId).ValueGeneratedNever();
        builder.Property(c => c.Version).IsConcurrencyToken();

        // Lines are part of the Cart: always loaded, saved and deleted with it. A Variant is referenced by ID only;
        // the Catalog owns it (ADR-0006).
        builder.OwnsMany(c => c.Lines, l =>
        {
            l.ToTable("CartLines");
            l.WithOwner().HasForeignKey(x => x.AccountId);
            l.HasKey(x => new { x.AccountId, x.VariantId });
            l.Property(x => x.VariantId).ValueGeneratedNever();
        });
    }
}

/// <summary>What a Cart may hold, checked after each edit: the request validators can't see the whole Cart.</summary>
public class CartValidator : AbstractValidator<Cart>
{
    public CartValidator()
    {
        RuleFor(c => c.Lines)
            .Must(l => l.Count <= Cart.MaxLines)
            .WithMessage($"A Cart can hold at most {Cart.MaxLines} different Variants.");
        RuleForEach(c => c.Lines)
            .Must(l => l.Quantity <= Cart.MaxQuantity)
            .WithMessage($"A Cart can hold at most {Cart.MaxQuantity} of one Variant.");
    }
}

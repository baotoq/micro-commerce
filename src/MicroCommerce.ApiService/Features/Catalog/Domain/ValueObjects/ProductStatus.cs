using Ardalis.SmartEnum;

namespace MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;

/// <summary>
/// Represents the lifecycle status of a product with state transition rules.
/// </summary>
public abstract class ProductStatus : SmartEnum<ProductStatus>
{
    public static readonly ProductStatus Draft     = new DraftStatus();
    public static readonly ProductStatus Published = new PublishedStatus();
    public static readonly ProductStatus Archived  = new ArchivedStatus();

    private ProductStatus(string name, int value) : base(name, value) { }

    public abstract bool CanTransitionTo(ProductStatus next);

    public void TransitionTo(ProductStatus next)
    {
        if (!CanTransitionTo(next))
        {
            IEnumerable<string> validNames = List.Where(s => CanTransitionTo(s)).Select(s => s.Name);
            throw new InvalidOperationException(
                $"Cannot transition from '{Name}' to '{next.Name}'. " +
                $"Valid transitions from '{Name}': {string.Join(", ", validNames)}.");
        }
    }

    private sealed class DraftStatus : ProductStatus
    {
        public DraftStatus() : base("Draft", 0) { }
        public override bool CanTransitionTo(ProductStatus next) =>
            next == Published || next == Archived;
    }

    private sealed class PublishedStatus : ProductStatus
    {
        public PublishedStatus() : base("Published", 1) { }
        public override bool CanTransitionTo(ProductStatus next) =>
            next == Draft || next == Archived;
    }

    private sealed class ArchivedStatus : ProductStatus
    {
        public ArchivedStatus() : base("Archived", 2) { }
        public override bool CanTransitionTo(ProductStatus next) => false; // terminal
    }
}

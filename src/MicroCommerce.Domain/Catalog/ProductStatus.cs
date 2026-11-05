namespace MicroCommerce.Domain.Catalog;

/// <summary>Draft ⇄ Published by the Merchant; any status → Removed by a Platform Operator (final).</summary>
public enum ProductStatus
{
    Draft,
    Published,
    Removed,
}

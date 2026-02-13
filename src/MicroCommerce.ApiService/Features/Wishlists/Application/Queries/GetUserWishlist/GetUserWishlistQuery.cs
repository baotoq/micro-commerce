using MediatR;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Catalog.Infrastructure;
using MicroCommerce.ApiService.Features.Inventory.Infrastructure;
using MicroCommerce.ApiService.Features.Wishlists.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Wishlists.Application.Queries.GetUserWishlist;

public sealed record GetUserWishlistQuery(Guid UserId) : IRequest<List<WishlistItemDto>>;

public sealed record WishlistItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal Price,
    string Currency,
    string? ImageUrl,
    decimal? AverageRating,
    int ReviewCount,
    int AvailableQuantity,
    DateTimeOffset AddedAt);

public sealed class GetUserWishlistQueryHandler
    : IRequestHandler<GetUserWishlistQuery, List<WishlistItemDto>>
{
    private readonly WishlistsDbContext _wishlistsContext;
    private readonly CatalogDbContext _catalogContext;
    private readonly InventoryDbContext _inventoryContext;

    public GetUserWishlistQueryHandler(
        WishlistsDbContext wishlistsContext,
        CatalogDbContext catalogContext,
        InventoryDbContext inventoryContext)
    {
        _wishlistsContext = wishlistsContext;
        _catalogContext = catalogContext;
        _inventoryContext = inventoryContext;
    }

    public async Task<List<WishlistItemDto>> Handle(
        GetUserWishlistQuery request,
        CancellationToken cancellationToken)
    {
        // Step 1: Query wishlist items for user, ordered by most recent first
        var wishlistItems = await _wishlistsContext.WishlistItems
            .Where(w => w.UserId == request.UserId)
            .OrderByDescending(w => w.AddedAt)
            .ToListAsync(cancellationToken);

        // Step 2: If empty, return empty list
        if (wishlistItems.Count == 0)
        {
            return [];
        }

        // Step 3: Collect all ProductIds
        var productIds = wishlistItems.Select(w => w.ProductId).ToList();

        // Step 4: Batch lookup products from CatalogDbContext
        var products = await _catalogContext.Products
            .Where(p => productIds.Contains(p.Id.Value))
            .ToDictionaryAsync(p => p.Id.Value, cancellationToken);

        // Step 5: Batch lookup stocks from InventoryDbContext
        var stocks = await _inventoryContext.StockItems
            .Where(s => productIds.Contains(s.ProductId))
            .ToDictionaryAsync(s => s.ProductId, cancellationToken);

        // Step 6: Map to WishlistItemDto, filtering out items where product is null (deleted products)
        var results = wishlistItems
            .Select(item =>
            {
                if (!products.TryGetValue(item.ProductId, out var product))
                {
                    return null;
                }

                var stock = stocks.GetValueOrDefault(item.ProductId);
                var availableQuantity = stock?.AvailableQuantity ?? 0;

                return new WishlistItemDto(
                    item.Id.Value,
                    item.ProductId,
                    product.Name.Value,
                    product.Price.Amount,
                    product.Price.Currency,
                    product.ImageUrl,
                    product.AverageRating,
                    product.ReviewCount,
                    availableQuantity,
                    item.AddedAt);
            })
            .Where(dto => dto is not null)
            .Cast<WishlistItemDto>()
            .ToList();

        return results;
    }
}

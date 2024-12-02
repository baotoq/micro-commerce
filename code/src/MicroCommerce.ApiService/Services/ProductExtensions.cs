using MicroCommerce.ApiService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Services;

public static class ProductExtensions
{
    public static async Task<bool> RefundProductRemainingStockAsync(this DbSet<Product> products, Guid productId, long quantity, CancellationToken cancellationToken)
    {
        var rowAffected = await products
            .Where(s => s.Id == productId)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(p => p.RemainingStock, p => p.RemainingStock + quantity), cancellationToken);

        return rowAffected > 0;
    }

    public static async Task<bool> UseProductRemainingStockAsync(this DbSet<Product> products, Guid productId, long quantity, CancellationToken cancellationToken)
    {
        var rowAffected = await products
            .Where(s => s.Id == productId)
            .Where(s => s.RemainingStock - quantity >= 0)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(p => p.RemainingStock, p => p.RemainingStock - quantity), cancellationToken);

        return rowAffected > 0;
    }

    public static async Task<int> ChangeProductStockAsync(this DbSet<Product> products, Guid productId, long changeQuantity, CancellationToken cancellationToken)
    {
        var rowAffected = await products
            .Where(s => s.Id == productId)
            .Where(s => s.RemainingStock + changeQuantity >= 0)
            .ExecuteUpdateAsync(setters =>
                setters
                    .SetProperty(p => p.RemainingStock, p => p.RemainingStock + changeQuantity)
                    .SetProperty(p => p.TotalStock, p => p.TotalStock + changeQuantity), cancellationToken);

        return rowAffected;
    }
}

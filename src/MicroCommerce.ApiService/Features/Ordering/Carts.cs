using System.Security.Claims;
using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.Features.Ordering.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MicroCommerce.ApiService.Features.Ordering;

/// <summary>
/// The <c>/cart/*</c> route group: the signed-in Account's own Cart, read from <see cref="BuyerCart"/>, never one
/// named in the route or body. Any Account can use it.
/// </summary>
public sealed class Carts : Group
{
    public Carts()
    {
        Configure("cart", _ => { });
    }
}

/// <summary>The signed-in Account's Cart, for <see cref="Carts"/> endpoints.</summary>
public class BuyerCart(
    OrderingDbContext db, CurrentAccount currentAccount, CurrentMerchant currentMerchant, ICatalogLookup catalog)
{
    public const string ChangedConcurrently = "The Cart was changed by another request at the same time; try again.";

    /// <summary>The Account's Cart, tracked for editing; a new, empty one (saved with the first edit) if it has none.</summary>
    public async Task<Cart> GetForEditAsync(ClaimsPrincipal user, CancellationToken ct)
    {
        var account = await currentAccount.GetOrCreateAsync(user, ct);
        var cart = await db.Carts.SingleOrDefaultAsync(c => c.AccountId == account.Id, ct);
        if (cart is not null) return cart;

        cart = new Cart { AccountId = account.Id };
        db.Carts.Add(cart);
        return cart;
    }

    /// <summary>The Account's Cart, untracked; a new, empty one if it has none.</summary>
    public async Task<Cart> GetAsync(ClaimsPrincipal user, CancellationToken ct)
    {
        var account = await currentAccount.GetOrCreateAsync(user, ct);
        return await db.Carts.AsNoTracking().SingleOrDefaultAsync(c => c.AccountId == account.Id, ct)
            ?? new Cart { AccountId = account.Id };
    }

    /// <returns>Whether the edit was saved: false if another request changed the Cart first.</returns>
    public async Task<bool> TrySaveAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // A concurrent request created the Cart, or added the same Variant, first.
            return false;
        }
    }

    /// <summary>The Cart with its lines read live from the Catalog.</summary>
    public async Task<CartResponse> ViewAsync(Cart cart, ClaimsPrincipal user, CancellationToken ct)
    {
        var lines = cart.Lines.OrderBy(l => l.AddedAt).ToList();
        var variants = await catalog.LookupAsync([.. lines.Select(l => l.VariantId)], ct);
        var ownMerchantId = await currentMerchant.ResolveAsync(user, ct) ? currentMerchant.Id : (MerchantId?)null;
        return CartView.Build(lines, variants, ownMerchantId);
    }
}

/// <summary>An endpoint that edits the signed-in Account's Cart and replies with it.</summary>
public abstract class CartEditEndpoint<TRequest>(BuyerCart buyerCart) : Endpoint<TRequest, CartResponse>
    where TRequest : notnull
{
    private static readonly CartValidator Validator = new();

    protected BuyerCart BuyerCart => buyerCart;

    /// <summary>
    /// Checks the edited Cart against <see cref="CartValidator"/> (400), saves it (409 if another request changed it
    /// first) and replies with its view.
    /// </summary>
    protected async Task SaveAsync(Cart cart, CancellationToken ct)
    {
        var validation = await Validator.ValidateAsync(cart, ct);
        if (!validation.IsValid)
        {
            ValidationFailures.AddRange(validation.Errors);
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        if (!await buyerCart.TrySaveAsync(ct))
        {
            AddError(BuyerCart.ChangedConcurrently);
            await Send.ErrorsAsync(StatusCodes.Status409Conflict, ct);
            return;
        }

        await Send.OkAsync(await buyerCart.ViewAsync(cart, User, ct), ct);
    }
}

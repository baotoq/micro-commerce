using FastEndpoints;
using FluentValidation;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.Features.Ordering.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using Wolverine.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Ordering;

public sealed class CheckoutPlacement : Group
{
    public CheckoutPlacement() => Configure("checkout", _ => { });
}

public record PlaceCheckoutRequest(DeliveryAddress DeliveryAddress, long ItemsTotal);

public class PlaceCheckoutValidator : Validator<PlaceCheckoutRequest>
{
    public PlaceCheckoutValidator()
    {
        RuleFor(x => x.DeliveryAddress).NotNull().SetValidator(new DeliveryAddressValidator());
        RuleFor(x => x.ItemsTotal).InclusiveBetween(0, Product.MaxPrice);
    }
}

public class PlaceCheckoutEndpoint(
    OrderingDbContext db,
    CurrentAccount currentAccount,
    BuyerCart buyerCart,
    ICatalogLookup catalog,
    ICatalogReservations reservations,
    IShippingFeeQuote shippingFeeQuote,
    IOptions<OrderingOptions> options,
    IDbContextOutbox<OrderingDbContext> outbox) : Endpoint<PlaceCheckoutRequest, CheckoutResponse>
{
    public override void Configure()
    {
        Post("/");
        Group<CheckoutPlacement>();
        Summary(s => s.Summary = "Place a Checkout from the signed-in Buyer's Cart.");
    }

    public override async Task HandleAsync(PlaceCheckoutRequest request, CancellationToken ct)
    {
        var account = await currentAccount.GetOrCreateAsync(User, ct);
        var existingId = await db.Checkouts.AsNoTracking()
            .Where(c => c.AccountId == account.Id && c.Status == CheckoutStatus.AwaitingPayment)
            .Select(c => (CheckoutId?)c.Id)
            .FirstOrDefaultAsync(ct);
        if (existingId is { } existingCheckoutId)
        {
            await ConflictAsync("This Account already has a Checkout awaiting payment.", [], existingCheckoutId, ct);
            return;
        }

        var cart = await buyerCart.GetForEditAsync(User, ct);
        if (cart.Lines.Count == 0)
        {
            await ConflictAsync("The Cart is empty.", [], null, ct);
            return;
        }

        var cartView = await buyerCart.ViewAsync(cart, User, ct);
        var lineProblems = cartView.Lines.SelectMany(line => line.Flags.Select(flag => new CheckoutLineProblem(
            line.VariantId,
            flag switch
            {
                CartLineFlag.Unavailable => "This Variant is no longer buyable.",
                CartLineFlag.InsufficientStock => "The available stock is less than the requested quantity.",
                CartLineFlag.OwnMerchant => "A Buyer cannot buy from their own Shop.",
                CartLineFlag.TotalTooLarge => "This line would exceed the maximum payable total.",
                _ => "This line cannot be checked out.",
            },
            line.Available)))
            .ToList();
        if (lineProblems.Count > 0)
        {
            await ConflictAsync("One or more Cart lines cannot be bought.", lineProblems, null, ct);
            return;
        }

        if (request.ItemsTotal != cartView.ItemsTotal.Value)
        {
            await ConflictAsync("The Cart's items total changed. Review it and try again.", [], null, ct);
            return;
        }

        var variantIds = cart.Lines.Select(line => line.VariantId).ToArray();
        var variants = await catalog.LookupAsync(variantIds, ct);
        var checkoutId = CheckoutId.New();
        var placedAt = DateTimeOffset.UtcNow;
        var expiresAt = placedAt + options.Value.PaymentWindow;
        var holdUntil = expiresAt + options.Value.ReservationGrace;
        var groups = cart.Lines
            .GroupBy(line => variants[line.VariantId].MerchantId)
            .Select(group =>
            {
                var subtotal = group.Sum(line => checked(variants[line.VariantId].UnitPrice.Value * line.Quantity));
                return new CheckoutMerchant
                {
                    CheckoutId = checkoutId,
                    MerchantId = group.Key,
                    Subtotal = Money.From(subtotal),
                    ShippingFee = shippingFeeQuote.Quote(group.Key),
                };
            })
            .ToList();
        var shippingTotal = groups.Sum(group => group.ShippingFee.Value);
        var total = checked(cartView.ItemsTotal.Value + shippingTotal);
        if (total > Product.MaxPrice)
        {
            await ConflictAsync("Items and shipping exceed the maximum payable total.",
                cart.Lines.Select(line => new CheckoutLineProblem(line.VariantId, "The Checkout total is too large.")).ToList(),
                null, ct);
            return;
        }

        var checkout = new Checkout
        {
            Id = checkoutId,
            AccountId = account.Id,
            PlacedAt = placedAt,
            ExpiresAt = expiresAt,
            DeliveryAddress = request.DeliveryAddress,
            ItemsTotal = cartView.ItemsTotal,
            ShippingTotal = Money.From(shippingTotal),
            Total = Money.From(total),
        };
        checkout.Merchants.AddRange(groups);
        checkout.Lines.AddRange(cart.Lines.Select(line =>
        {
            var variant = variants[line.VariantId];
            return new OrderLine
            {
                CheckoutId = checkoutId,
                VariantId = variant.VariantId,
                MerchantId = variant.MerchantId,
                ProductId = variant.ProductId,
                ProductName = variant.ProductName,
                OptionValues = [.. variant.OptionValues],
                UnitPrice = variant.UnitPrice,
                Quantity = line.Quantity,
            };
        }));

        var reserveResult = await reservations.ReserveAsync(
            checkoutId,
            [.. cart.Lines.Select(line => new ReservationLine(line.VariantId, line.Quantity))],
            holdUntil,
            ct);
        if (!reserveResult.Succeeded)
        {
            await ConflictAsync(
                "Stock or availability changed while placing the Checkout.",
                reserveResult.Failures.Select(f => new CheckoutLineProblem(
                    f.VariantId,
                    f.Reason == ReservationFailureReason.NotBuyable
                        ? "This Variant is no longer buyable."
                        : "The available stock is less than the requested quantity.",
                    f.Available)).ToList(),
                null,
                ct);
            return;
        }

        try
        {
            db.Checkouts.Add(checkout);
            cart.Clear();
            await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                await outbox.PublishAsync(new CheckoutPlaced(checkoutId, expiresAt));
                await outbox.SaveChangesAndFlushMessagesAsync(ct);
            });
        }
        catch (DbUpdateConcurrencyException)
        {
            await ReleaseBestEffortAsync(checkoutId);
            await ConflictAsync("The Cart or Checkout changed while placing it. Try again.", [], null, ct);
            return;
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            await ReleaseBestEffortAsync(checkoutId);
            await ConflictAsync("This Account already has a Checkout awaiting payment.", [], null, ct);
            return;
        }
        catch
        {
            await ReleaseBestEffortAsync(checkoutId);
            throw;
        }

        await Send.CreatedAtAsync<GetCheckoutEndpoint>(
            new { id = checkoutId },
            checkout.ToCheckoutResponse(),
            cancellation: ct);
    }

    private Task ConflictAsync(string detail, IReadOnlyList<CheckoutLineProblem> lines, CheckoutId? existingCheckoutId, CancellationToken ct)
    {
        HttpContext.Response.StatusCode = StatusCodes.Status409Conflict;
        return HttpContext.Response.WriteAsJsonAsync(
            new CheckoutConflictResponse(detail, lines, existingCheckoutId),
            cancellationToken: ct);
    }

    private async Task ReleaseBestEffortAsync(CheckoutId checkoutId)
    {
        try
        {
            await reservations.ReleaseAsync(checkoutId, CancellationToken.None);
        }
        catch
        {
            // HoldUntil is the fallback if the best-effort release is unavailable.
        }
    }
}

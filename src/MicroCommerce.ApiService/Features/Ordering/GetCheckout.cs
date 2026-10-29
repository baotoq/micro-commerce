using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Ordering.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Ordering;

public record GetCheckoutRequest(CheckoutId Id);

public class GetCheckoutEndpoint(OrderingDbContext db, CurrentAccount currentAccount) : Endpoint<GetCheckoutRequest, CheckoutResponse>
{
    public override void Configure()
    {
        Get("/checkout/{id:guid}");
    }

    public override async Task HandleAsync(GetCheckoutRequest request, CancellationToken ct)
    {
        var account = await currentAccount.GetOrCreateAsync(User, ct);
        var checkout = await db.Checkouts.AsNoTracking()
            .Include(c => c.Merchants)
            .Include(c => c.Lines)
            .SingleOrDefaultAsync(c => c.Id == request.Id && c.AccountId == account.Id, ct);
        if (checkout is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(checkout.ToCheckoutResponse(), ct);
    }
}

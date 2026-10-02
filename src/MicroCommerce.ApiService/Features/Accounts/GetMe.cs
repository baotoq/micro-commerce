using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Accounts;

public class GetMeEndpoint(AccountsDbContext db, CurrentAccount currentAccount, IAuthorizationService authorization)
    : EndpointWithoutRequest<MeResponse>
{
    public override void Configure()
    {
        Get("/me");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var account = await currentAccount.GetOrCreateAsync(User, ct);
        var isOperator = (await authorization.AuthorizeAsync(User, AuthPolicies.PlatformOperator)).Succeeded;
        var merchant = await db.Merchants.AsNoTracking().SingleOrDefaultAsync(m => m.AccountId == account.Id, ct);

        await Send.OkAsync(MeResponse.From(account, isOperator, merchant), ct);
    }
}

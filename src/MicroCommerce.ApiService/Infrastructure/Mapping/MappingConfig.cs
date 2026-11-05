using Mapster;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Diagnostics;
using MicroCommerce.Domain.Diagnostics;
using MicroCommerce.Domain.SharedKernel;

namespace MicroCommerce.ApiService.Infrastructure.Mapping;

/// <summary>
/// The one Mapster configuration. Mapping goes outward only, from domain types to responses; nothing
/// maps into an aggregate (research R3). <c>MappingConfigTests</c> compiles it, so a missing or
/// ambiguous mapping fails the build's tests instead of a request.
/// </summary>
public static class MappingConfig
{
    public static TypeAdapterConfig Create()
    {
        var config = new TypeAdapterConfig
        {
            RequireExplicitMapping = true,
            RequireDestinationMemberSource = true,
        };

        // Typed IDs leave the domain as the primitive they wrap.
        config.NewConfig<AccountId, Guid>().MapWith(x => x.Value);
        config.NewConfig<SampleProcessId, Guid>().MapWith(x => x.Value);

        // Features register their mappings here.
        config.NewConfig<CurrentAccount, MeResponse>()
            .Map(d => d.AccountId, s => s.Id.Value)
            // Not part of the token: GetMe reads it from the Merchant directory.
            .Ignore(d => d.Merchant!);
        config.NewConfig<SampleProcess, SampleProcessResponse>()
            .Map(d => d.Id, s => s.Id.Value)
            .Map(d => d.Status, s => s.Status.ToString())
            .Map(d => d.DelaySeconds, s => (int)s.Delay.TotalSeconds)
            // Not part of the aggregate: GetSampleProcess reads it from the inbox.
            .Ignore(d => d.CompletionEventReceivedAt);

        return config;
    }
}

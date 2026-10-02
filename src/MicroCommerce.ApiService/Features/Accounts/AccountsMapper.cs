using Riok.Mapperly.Abstractions;

namespace MicroCommerce.ApiService.Features.Accounts;

/// <summary>Maps Accounts module entities to their responses; every response member must have a source.</summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class AccountsMapper
{
    public static partial MerchantResponse ToResponse(this Merchant merchant);

    public static partial MeResponse ToMeResponse(this Account account, bool isPlatformOperator, Merchant? merchant);
}

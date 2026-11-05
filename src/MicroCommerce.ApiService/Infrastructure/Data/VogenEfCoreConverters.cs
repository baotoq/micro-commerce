using Vogen;

namespace MicroCommerce.ApiService.Infrastructure.Data;

/// <summary>
/// Declares the EF Core value converters for the domain's typed IDs, so the Domain project needs no
/// EF Core reference (research R2). Add one <c>[EfCoreConverter&lt;T&gt;]</c> per persisted ID type, and
/// add the same type to <see cref="OpenApi.VogenIdSchemaTransformer"/>.
/// </summary>
// Vogen only generates RegisterAllInVogenEfCoreConverters() when at least one converter is declared.
[EfCoreConverter<Domain.SharedKernel.AccountId>]
[EfCoreConverter<Domain.Diagnostics.SampleProcessId>]
[EfCoreConverter<Domain.SharedKernel.MerchantId>]
[EfCoreConverter<Domain.Accounts.ShopName>]
[EfCoreConverter<Domain.Catalog.CategoryId>]
[EfCoreConverter<Domain.Catalog.ProductId>]
[EfCoreConverter<Domain.Catalog.VariantId>]
[EfCoreConverter<Domain.Catalog.ProductImageId>]
[EfCoreConverter<Domain.Catalog.Money>]
internal sealed partial class VogenEfCoreConverters;

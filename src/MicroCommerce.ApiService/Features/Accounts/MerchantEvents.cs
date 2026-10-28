using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.ApiService.Features.Accounts;

// The events the Accounts module publishes about Merchants, through Wolverine's transactional outbox: each is
// written in the same transaction as the change it describes (ADR-0003, ADR-0004). They carry the Shop name as a
// plain string, since other modules don't use ShopName (ADR-0005). Delivery is at least once and in no guaranteed
// order, so each carries the Merchant's ProfileVersion: a handler keeps the highest version it has seen.

/// <summary>A Merchant opened, with its Shop profile at <see cref="ProfileVersion"/> 1.</summary>
public record MerchantOpened(MerchantId MerchantId, string ShopName, string? Description, int ProfileVersion);

/// <summary>A Merchant's Shop profile changed to the given <see cref="ProfileVersion"/>.</summary>
public record MerchantProfileChanged(MerchantId MerchantId, string ShopName, string? Description, int ProfileVersion);

/// <summary>A Merchant was suspended, with its Shop profile at the given <see cref="ProfileVersion"/>.</summary>
public record MerchantSuspended(MerchantId MerchantId, string ShopName, string? Description, int ProfileVersion);

/// <summary>A Merchant was reinstated, with its Shop profile at the given <see cref="ProfileVersion"/>.</summary>
public record MerchantReinstated(MerchantId MerchantId, string ShopName, string? Description, int ProfileVersion);

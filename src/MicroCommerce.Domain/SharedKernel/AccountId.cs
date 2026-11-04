using Vogen;

namespace MicroCommerce.Domain.SharedKernel;

/// <summary>Identifies an Account. It is the Keycloak <c>sub</c> claim.</summary>
[ValueObject<Guid>]
public readonly partial struct AccountId;

using Vogen;

namespace MicroCommerce.ApiService.SharedKernel;

/// <summary>Identifies an Account across modules (ADR-0005).</summary>
[ValueObject<Guid>]
public readonly partial struct AccountId
{
    public static AccountId New() => From(Guid.CreateVersion7());
}

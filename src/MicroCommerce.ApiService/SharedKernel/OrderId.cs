using Vogen;

namespace MicroCommerce.ApiService.SharedKernel;

[ValueObject<Guid>]
public readonly partial struct OrderId
{
    public static OrderId New() => From(Guid.CreateVersion7());
}
